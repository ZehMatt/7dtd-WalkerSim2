using Avalonia.OpenGL;
using System;
using System.Runtime.InteropServices;

namespace Editor.Gl
{
    // Shader pipeline with async compile support via GL_KHR_parallel_shader_compile.
    // Initialization is split into three phases so the render thread never
    // blocks on the driver's compile/link:
    //   1. Init()  — called once on the GL thread. Creates shaders, sets
    //      source, kicks off compilation and linking. With KHR_parallel_shader_compile
    //      these calls return immediately while the driver compiles on worker
    //      threads; without it, the work still runs on the render thread.
    //   2. Poll()  — called every frame until it returns true. Uses the
    //      KHR extension's GL_COMPLETION_STATUS_KHR to test readiness
    //      without forcing a sync.
    //   3. Finalize() — called once, when Poll returns true the first time.
    //      Queries uniform locations and creates the VBO; only safe once the
    //      program has actually linked.
    public sealed unsafe class GlShaderPipeline : IDisposable
    {
        private const int GL_COLOR_BUFFER_BIT = 0x4000;
        private const int GL_TRIANGLES = 0x0004;
        private const int GL_VERTEX_SHADER = 0x8B31;
        private const int GL_FRAGMENT_SHADER = 0x8B30;
        private const int GL_DEPTH_TEST = 0x0B71;
        private const int GL_CULL_FACE = 0x0B44;
        private const int GL_BLEND = 0x0BE2;
        private const int GL_FRAMEBUFFER = 0x8D40;
        private const int GL_ARRAY_BUFFER = 0x8892;
        private const int GL_STATIC_DRAW = 0x88E4;
        private const int GL_FLOAT = 0x1406;
        private const int GL_COMPILE_STATUS = 0x8B81;
        private const int GL_LINK_STATUS = 0x8B82;
        private const int GL_COMPLETION_STATUS_KHR = 0x91B1;
        private const int GL_INFO_LOG_LENGTH = 0x8B84;
        private const int GL_PROGRAM_BINARY_LENGTH = 0x8741;
        private const int GL_PROGRAM_BINARY_RETRIEVABLE_HINT = 0x8257;
        // 4-byte magic + driver version string written at the start of every
        // cache file so a driver/extension change forces a rebuild.
        private const uint CACHE_MAGIC = 0x57534831; // "WSH1"

        private delegate* unmanaged<int, int, int, int, void> _glViewport;
        private delegate* unmanaged<float, float, float, float, void> _glClearColor;
        private delegate* unmanaged<uint, void> _glClear;
        private delegate* unmanaged<uint, void> _glDisable;
        private delegate* unmanaged<uint> _glCreateProgram;
        private delegate* unmanaged<uint, void> _glDeleteProgram;
        private delegate* unmanaged<uint, void> _glUseProgram;
        private delegate* unmanaged<uint, uint, void> _glAttachShader;
        private delegate* unmanaged<uint, uint> _glCreateShader;
        private delegate* unmanaged<uint, void> _glDeleteShader;
        private delegate* unmanaged<int, float, void> _glUniform1f;
        private delegate* unmanaged<int, float, float, void> _glUniform2f;
        private delegate* unmanaged<int, int, void> _glUniform1i;
        private delegate* unmanaged<uint, int, int, void> _glDrawArrays;
        private delegate* unmanaged<uint, uint, void> _glBindFramebuffer;
        private delegate* unmanaged<uint, uint, void> _glBindBuffer;
        private delegate* unmanaged<uint, IntPtr, void*, uint, void> _glBufferData;
        private delegate* unmanaged<uint, void> _glEnableVertexAttribArray;
        private delegate* unmanaged<uint, int, uint, byte, int, void*, void> _glVertexAttribPointer;
        private delegate* unmanaged<uint, int, byte**, int*, void> _glShaderSource;
        private delegate* unmanaged<uint, void> _glCompileShader;
        private delegate* unmanaged<uint, void> _glLinkProgram;
        private delegate* unmanaged<uint, uint, int*, void> _glGetShaderiv;
        private delegate* unmanaged<uint, uint, int*, void> _glGetProgramiv;
        private delegate* unmanaged<uint, int, int*, byte*, void> _glGetShaderInfoLog;
        private delegate* unmanaged<uint, int, int*, byte*, void> _glGetProgramInfoLog;
        private delegate* unmanaged<uint, void> _glMaxShaderCompilerThreadsKHR;
        private delegate* unmanaged<uint, int, int*, uint*, void*, void> _glGetProgramBinary;
        private delegate* unmanaged<uint, uint, void*, int, void> _glProgramBinary;
        private delegate* unmanaged<uint, uint, int, void> _glProgramParameteri;
        private delegate* unmanaged<uint, byte*> _glGetString;

        private GlInterface _gl;
        private int _program;
        private int _vs;
        private int _fs;
        private int _vbo;
        private int _uResolution;
        private int _uTime;
        private int _uBass;
        private int _uEnergy;
        private int _uPerc;
        private int _uLead;
        private int _uChord;
        private int _uCamZ;
        private int _uPhase;

        private bool _parallelSupported;
        private bool _shadersCompiled;
        private bool _linkStarted;
        private bool _ready;
        private bool _loadedFromCache;

        // Cache configuration provided by the caller.
        private string _cacheDir;
        private string _cacheKey;
        private string _vertSrc;
        private string _fragSrc;

        public bool IsReady => _ready;
        public bool LoadedFromCache => _loadedFromCache;

        // cacheDir: directory to read/write the compiled program binary
        // (one file per content hash). Pass null to disable caching.
        public void Init(GlInterface gl, string vertSrc, string fragSrc, string cacheDir = null)
        {
            _gl = gl;
            _vertSrc = vertSrc;
            _fragSrc = fragSrc;
            _cacheDir = cacheDir;

            _glViewport = (delegate* unmanaged<int, int, int, int, void>)gl.GetProcAddress("glViewport");
            _glClearColor = (delegate* unmanaged<float, float, float, float, void>)gl.GetProcAddress("glClearColor");
            _glClear = (delegate* unmanaged<uint, void>)gl.GetProcAddress("glClear");
            _glDisable = (delegate* unmanaged<uint, void>)gl.GetProcAddress("glDisable");
            _glCreateProgram = (delegate* unmanaged<uint>)gl.GetProcAddress("glCreateProgram");
            _glDeleteProgram = (delegate* unmanaged<uint, void>)gl.GetProcAddress("glDeleteProgram");
            _glUseProgram = (delegate* unmanaged<uint, void>)gl.GetProcAddress("glUseProgram");
            _glAttachShader = (delegate* unmanaged<uint, uint, void>)gl.GetProcAddress("glAttachShader");
            _glCreateShader = (delegate* unmanaged<uint, uint>)gl.GetProcAddress("glCreateShader");
            _glDeleteShader = (delegate* unmanaged<uint, void>)gl.GetProcAddress("glDeleteShader");
            _glUniform1f = (delegate* unmanaged<int, float, void>)gl.GetProcAddress("glUniform1f");
            _glUniform2f = (delegate* unmanaged<int, float, float, void>)gl.GetProcAddress("glUniform2f");
            _glUniform1i = (delegate* unmanaged<int, int, void>)gl.GetProcAddress("glUniform1i");
            _glDrawArrays = (delegate* unmanaged<uint, int, int, void>)gl.GetProcAddress("glDrawArrays");
            _glBindFramebuffer = (delegate* unmanaged<uint, uint, void>)gl.GetProcAddress("glBindFramebuffer");
            _glBindBuffer = (delegate* unmanaged<uint, uint, void>)gl.GetProcAddress("glBindBuffer");
            _glBufferData = (delegate* unmanaged<uint, IntPtr, void*, uint, void>)gl.GetProcAddress("glBufferData");
            _glEnableVertexAttribArray = (delegate* unmanaged<uint, void>)gl.GetProcAddress("glEnableVertexAttribArray");
            _glVertexAttribPointer = (delegate* unmanaged<uint, int, uint, byte, int, void*, void>)gl.GetProcAddress("glVertexAttribPointer");
            _glShaderSource = (delegate* unmanaged<uint, int, byte**, int*, void>)gl.GetProcAddress("glShaderSource");
            _glCompileShader = (delegate* unmanaged<uint, void>)gl.GetProcAddress("glCompileShader");
            _glLinkProgram = (delegate* unmanaged<uint, void>)gl.GetProcAddress("glLinkProgram");
            _glGetShaderiv = (delegate* unmanaged<uint, uint, int*, void>)gl.GetProcAddress("glGetShaderiv");
            _glGetProgramiv = (delegate* unmanaged<uint, uint, int*, void>)gl.GetProcAddress("glGetProgramiv");
            _glGetShaderInfoLog = (delegate* unmanaged<uint, int, int*, byte*, void>)gl.GetProcAddress("glGetShaderInfoLog");
            _glGetProgramInfoLog = (delegate* unmanaged<uint, int, int*, byte*, void>)gl.GetProcAddress("glGetProgramInfoLog");
            _glMaxShaderCompilerThreadsKHR = (delegate* unmanaged<uint, void>)gl.GetProcAddress("glMaxShaderCompilerThreadsKHR");
            _glGetProgramBinary = (delegate* unmanaged<uint, int, int*, uint*, void*, void>)gl.GetProcAddress("glGetProgramBinary");
            _glProgramBinary = (delegate* unmanaged<uint, uint, void*, int, void>)gl.GetProcAddress("glProgramBinary");
            _glProgramParameteri = (delegate* unmanaged<uint, uint, int, void>)gl.GetProcAddress("glProgramParameteri");
            _glGetString = (delegate* unmanaged<uint, byte*>)gl.GetProcAddress("glGetString");

            _parallelSupported = _glMaxShaderCompilerThreadsKHR != null;
            if (_parallelSupported)
                _glMaxShaderCompilerThreadsKHR(0xFFFFFFFF);

            _program = (int)_glCreateProgram();

            // Cache key includes the driver-identity strings so a driver
            // upgrade automatically invalidates stale binaries (their format
            // is driver-specific). Source is also folded in so any shader
            // edit invalidates the cache too.
            _cacheKey = ComputeCacheKey(vertSrc, fragSrc);

            // Try to load a previously-saved program binary from disk. If it
            // works we skip the compile/link entirely.
            if (TryLoadProgramBinary())
            {
                _loadedFromCache = true;
                _shadersCompiled = true;
                _linkStarted = true;
                return;
            }

            // Tell the driver we want the linked binary back later.
            if (_glProgramParameteri != null)
                _glProgramParameteri((uint)_program, (uint)GL_PROGRAM_BINARY_RETRIEVABLE_HINT, 1);

            _vs = (int)_glCreateShader((uint)GL_VERTEX_SHADER);
            _fs = (int)_glCreateShader((uint)GL_FRAGMENT_SHADER);
            SetShaderSource(_vs, vertSrc);
            SetShaderSource(_fs, fragSrc);
            _glCompileShader((uint)_vs);
            _glCompileShader((uint)_fs);

            _glAttachShader((uint)_program, (uint)_vs);
            _glAttachShader((uint)_program, (uint)_fs);
            gl.BindAttribLocationString(_program, 0, "a_pos");
        }

        private string ComputeCacheKey(string vert, string frag)
        {
            string vendor = ReadGlString(0x1F00); // GL_VENDOR
            string renderer = ReadGlString(0x1F01); // GL_RENDERER
            string version = ReadGlString(0x1F02); // GL_VERSION
            string identity = vendor + "|" + renderer + "|" + version + "|" + vert + "\n----\n" + frag;
            using var sha = System.Security.Cryptography.SHA256.Create();
            byte[] hash = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(identity));
            return Convert.ToHexString(hash).Substring(0, 32).ToLowerInvariant();
        }

        private string ReadGlString(uint name)
        {
            if (_glGetString == null) return "";
            byte* p = _glGetString(name);
            if (p == null) return "";
            int len = 0;
            while (p[len] != 0) len++;
            return System.Text.Encoding.UTF8.GetString(p, len);
        }

        private string CachePath => _cacheDir == null ? null
            : System.IO.Path.Combine(_cacheDir, _cacheKey + ".glprog");

        private bool TryLoadProgramBinary()
        {
            if (_glProgramBinary == null || _cacheDir == null) return false;
            string path = CachePath;
            if (!System.IO.File.Exists(path)) return false;
            try
            {
                byte[] data = System.IO.File.ReadAllBytes(path);
                if (data.Length < 8) return false;
                uint magic = BitConverter.ToUInt32(data, 0);
                if (magic != CACHE_MAGIC) return false;
                uint format = BitConverter.ToUInt32(data, 4);
                int payloadLen = data.Length - 8;
                fixed (byte* pData = data)
                {
                    _glProgramBinary((uint)_program, format, pData + 8, payloadLen);
                }
                int status = 0;
                _glGetProgramiv((uint)_program, GL_LINK_STATUS, &status);
                if (status == 0)
                {
                    // Driver rejected the binary — probably a driver/version
                    // change. Drop the file and fall back to compiling.
                    try { System.IO.File.Delete(path); } catch { }
                    return false;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void SaveProgramBinary()
        {
            if (_glGetProgramBinary == null || _cacheDir == null) return;
            try
            {
                int len = 0;
                _glGetProgramiv((uint)_program, (uint)GL_PROGRAM_BINARY_LENGTH, &len);
                if (len <= 0) return;
                byte[] data = new byte[len + 8];
                BitConverter.GetBytes(CACHE_MAGIC).CopyTo(data, 0);
                uint format = 0;
                int written = 0;
                fixed (byte* p = data)
                {
                    _glGetProgramBinary((uint)_program, len, &written, &format, p + 8);
                }
                BitConverter.GetBytes(format).CopyTo(data, 4);
                if (written <= 0) return;
                Array.Resize(ref data, written + 8);
                System.IO.Directory.CreateDirectory(_cacheDir);
                System.IO.File.WriteAllBytes(CachePath, data);
            }
            catch
            {
            }
        }

        // Returns true once the program is linked and the pipeline has
        // queried its uniforms + set up its VBO. Callers should invoke
        // this once per frame during loading.
        public bool Poll()
        {
            if (_ready) return true;

            // Cache hit path — program is already linked, just finalize.
            if (_loadedFromCache)
            {
                FinalizePipeline();
                _ready = true;
                return true;
            }

            if (!_shadersCompiled)
            {
                if (_parallelSupported)
                {
                    int vsDone = 0, fsDone = 0;
                    _glGetShaderiv((uint)_vs, GL_COMPLETION_STATUS_KHR, &vsDone);
                    _glGetShaderiv((uint)_fs, GL_COMPLETION_STATUS_KHR, &fsDone);
                    if (vsDone == 0 || fsDone == 0) return false;
                }
                CheckShaderStatus(_vs, "vertex");
                CheckShaderStatus(_fs, "fragment");
                _shadersCompiled = true;

                _glLinkProgram((uint)_program);
                _linkStarted = true;
                return false;
            }

            if (_linkStarted)
            {
                if (_parallelSupported)
                {
                    int linkDone = 0;
                    _glGetProgramiv((uint)_program, GL_COMPLETION_STATUS_KHR, &linkDone);
                    if (linkDone == 0) return false;
                }
                CheckProgramStatus(_program);
                // Persist the linked binary to disk so future runs skip the
                // compile altogether (driver-specific format, keyed by source
                // hash + driver identity).
                SaveProgramBinary();
                FinalizePipeline();
                _ready = true;
                return true;
            }

            return false;
        }

        private void FinalizePipeline()
        {
            _glDeleteShader((uint)_vs); _vs = 0;
            _glDeleteShader((uint)_fs); _fs = 0;

            _uResolution = _gl.GetUniformLocationString(_program, "u_resolution");
            _uTime = _gl.GetUniformLocationString(_program, "u_time");
            _uBass = _gl.GetUniformLocationString(_program, "u_bass");
            _uEnergy = _gl.GetUniformLocationString(_program, "u_energy");
            _uPerc = _gl.GetUniformLocationString(_program, "u_perc");
            _uLead = _gl.GetUniformLocationString(_program, "u_lead");
            _uChord = _gl.GetUniformLocationString(_program, "u_chord");
            _uCamZ = _gl.GetUniformLocationString(_program, "u_camZ");
            _uPhase = _gl.GetUniformLocationString(_program, "u_phase");

            _vbo = _gl.GenBuffer();
            _glBindBuffer(GL_ARRAY_BUFFER, (uint)_vbo);
            float[] verts = { -1f, -1f, 3f, -1f, -1f, 3f };
            fixed (float* pv = verts)
            {
                _glBufferData(GL_ARRAY_BUFFER, (IntPtr)(verts.Length * sizeof(float)), pv, GL_STATIC_DRAW);
            }
            _glBindBuffer(GL_ARRAY_BUFFER, 0);
        }

        private void SetShaderSource(int shader, string src)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(src);
            fixed (byte* pSrc = bytes)
            {
                byte* ptr = pSrc;
                int len = bytes.Length;
                _glShaderSource((uint)shader, 1, &ptr, &len);
            }
        }

        private void CheckShaderStatus(int shader, string kind)
        {
            int status = 0;
            _glGetShaderiv((uint)shader, GL_COMPILE_STATUS, &status);
            if (status == 0)
            {
                int logLen = 0;
                _glGetShaderiv((uint)shader, GL_INFO_LOG_LENGTH, &logLen);
                string msg = ReadInfoLog(shader, logLen, isShader: true);
                throw new InvalidOperationException($"GL shader compile failed ({kind}): {msg}");
            }
        }

        private void CheckProgramStatus(int program)
        {
            int status = 0;
            _glGetProgramiv((uint)program, GL_LINK_STATUS, &status);
            if (status == 0)
            {
                int logLen = 0;
                _glGetProgramiv((uint)program, GL_INFO_LOG_LENGTH, &logLen);
                string msg = ReadInfoLog(program, logLen, isShader: false);
                throw new InvalidOperationException("GL program link failed: " + msg);
            }
        }

        private string ReadInfoLog(int handle, int logLen, bool isShader)
        {
            if (logLen <= 1) return "(no info log)";
            byte[] buf = new byte[logLen];
            fixed (byte* pb = buf)
            {
                int written = 0;
                if (isShader) _glGetShaderInfoLog((uint)handle, logLen, &written, pb);
                else _glGetProgramInfoLog((uint)handle, logLen, &written, pb);
                return System.Text.Encoding.UTF8.GetString(buf, 0, Math.Max(0, written));
            }
        }

        public void Render(int fb, int width, int height,
            float time, float bass, float energy, float perc, float lead, int chord,
            float camZ, float phase)
        {
            _glBindFramebuffer(GL_FRAMEBUFFER, (uint)fb);

            _glDisable(GL_DEPTH_TEST);
            _glDisable(GL_CULL_FACE);
            _glDisable(GL_BLEND);

            _glViewport(0, 0, width, height);
            _glClearColor(0f, 0f, 0f, 1f);
            _glClear(GL_COLOR_BUFFER_BIT);

            _glUseProgram((uint)_program);
            if (_uResolution >= 0) _glUniform2f(_uResolution, width, height);
            if (_uTime >= 0) _glUniform1f(_uTime, time);
            if (_uBass >= 0) _glUniform1f(_uBass, bass);
            if (_uEnergy >= 0) _glUniform1f(_uEnergy, energy);
            if (_uPerc >= 0) _glUniform1f(_uPerc, perc);
            if (_uLead >= 0) _glUniform1f(_uLead, lead);
            if (_uChord >= 0) _glUniform1i(_uChord, chord);
            if (_uCamZ >= 0) _glUniform1f(_uCamZ, camZ);
            if (_uPhase >= 0) _glUniform1f(_uPhase, phase);

            _glBindBuffer(GL_ARRAY_BUFFER, (uint)_vbo);
            _glVertexAttribPointer(0, 2, GL_FLOAT, 0, 0, null);
            _glEnableVertexAttribArray(0);
            _glDrawArrays(GL_TRIANGLES, 0, 3);
            _glBindBuffer(GL_ARRAY_BUFFER, 0);
        }

        // Clears the target framebuffer to a solid color — used while the
        // pipeline is still compiling so the control shows something.
        public void ClearTo(int fb, int width, int height, float r, float g, float b)
        {
            _glBindFramebuffer(GL_FRAMEBUFFER, (uint)fb);
            _glViewport(0, 0, width, height);
            _glClearColor(r, g, b, 1f);
            _glClear(GL_COLOR_BUFFER_BIT);
        }

        public void Dispose()
        {
            if (_gl == null)
                return;

            // If the pipeline isn't ready yet, we're somewhere in the
            // compile/link state machine: glCompileShader is queued, or
            // glLinkProgram is queued, or the driver is still doing either
            // on its worker threads. glDeleteProgram/glDeleteShader in any
            // of those states blocks the render thread until the driver
            // finishes the in-flight work — freezing the compositor and
            // the whole app. Leak the GL handles instead; the OS reclaims
            // them when the GL context is torn down.
            bool inFlight = !_ready;

            if (_vbo != 0)
            {
                try { _gl.DeleteBuffer(_vbo); } catch { }
                _vbo = 0;
            }

            if (inFlight)
            {
                // Leak GL handles deliberately: deleting them while a compile
                // is in flight makes the driver block the render thread.
                _vs = 0; _fs = 0; _program = 0;
                _gl = null;
                return;
            }

            if (_vs != 0) { _glDeleteShader((uint)_vs); _vs = 0; }
            if (_fs != 0) { _glDeleteShader((uint)_fs); _fs = 0; }
            if (_program != 0)
            {
                _glDeleteProgram((uint)_program);
                _program = 0;
            }
            _gl = null;
        }
    }
}
