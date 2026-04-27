using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Editor.Audio;
using Editor.Gl;
using System;

namespace Editor.Views
{
    // Raymarched first-person walk down an infinite road at night. A walker
    // silhouette stays ahead of the camera and animates a walking cycle.
    // Lamp posts repeat via mod() on z so the road is unbounded. Music drives
    // sky tint, lamp brightness, road glow, and step tempo.
    public sealed class AboutVFXGl : OpenGlControlBase
    {
        public WavPlayer Synth { get; set; }

        // Fires once (from the GL render thread) after the first successful
        // frame has been rendered — i.e., after shader compilation has
        // completed. Host windows can use this to delay work until the
        // heavy VFX initialization is done.
        public event Action Ready;

        private GlShaderPipeline _pipeline;
        private readonly DateTime _startTime = DateTime.UtcNow;
        private DateTime _lastFrame = DateTime.UtcNow;
        private bool _readyFired;

        // Forward-travel distance and integrated step phase. Walker moves at
        // a constant slow pace independent of music.
        private float _camZ;
        private float _stepPhase;

        // Shader sources live as files under Assets/Shaders, included by the
        // csproj's <AvaloniaResource Include="Assets\**"/>. Loaded on first
        // use and cached statically so multiple instances share the work.
        private static string _vertSrc;
        private static string _fragSrc;

        private static string LoadShader(string avaresUri)
        {
            using var s = Avalonia.Platform.AssetLoader.Open(new Uri(avaresUri));
            using var r = new System.IO.StreamReader(s);
            return r.ReadToEnd();
        }

        // Compiled-shader binary cache lives next to the editor's log files,
        // under %APPDATA%\WalkerSim2\ShaderCache\. Each file is keyed by a
        // SHA-256 of (driver-identity + shader source) so any change to the
        // shader text or a driver upgrade invalidates the cache.
        private static string GetShaderCacheDir()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return System.IO.Path.Combine(appData, "WalkerSim2", "ShaderCache");
        }

        protected override void OnOpenGlInit(GlInterface gl)
        {
            try
            {
                _vertSrc ??= LoadShader("avares://Editor/Assets/Shaders/AboutVFX.vert");
                _fragSrc ??= LoadShader("avares://Editor/Assets/Shaders/AboutVFX.frag");
                _pipeline = new GlShaderPipeline();
                // Kicks off the compile/link on the driver's worker threads
                // if GL_KHR_parallel_shader_compile is supported. Loads from
                // the on-disk binary cache if a matching entry exists.
                _pipeline.Init(gl, _vertSrc, _fragSrc, GetShaderCacheDir());
            }
            catch
            {
                _pipeline?.Dispose();
                _pipeline = null;
            }
        }

        protected override void OnOpenGlRender(GlInterface gl, int fb)
        {
            if (_pipeline == null)
            {
                GlClearMagenta(gl, fb);
                RequestNextFrameRendering();
                return;
            }

            int wPx = Math.Max(1, (int)Bounds.Width);
            int hPx = Math.Max(1, (int)Bounds.Height);

            // Poll the async compile. While not ready, clear to the dark
            // window background color so credits/info show continuity.
            if (!_pipeline.IsReady)
            {
                try
                {
                    if (!_pipeline.Poll())
                    {
                        _pipeline.ClearTo(fb, wPx, hPx, 0.07f, 0.07f, 0.10f);
                        RequestNextFrameRendering();
                        return;
                    }
                }
                catch
                {
                    _pipeline.Dispose();
                    _pipeline = null;
                    GlClearMagenta(gl, fb);
                    RequestNextFrameRendering();
                    return;
                }
            }

            int w = wPx;
            int h = hPx;
            var now = DateTime.UtcNow;
            float time = (float)(now - _startTime).TotalSeconds;
            float dt = (float)(now - _lastFrame).TotalSeconds;
            _lastFrame = now;
            if (dt < 0f) dt = 0f;
            else if (dt > 0.1f) dt = 0.1f;

            float bass = 0, energy = 0, perc = 0, lead = 0;
            int chord = 0;
            var synth = Synth;
            if (synth != null)
            {
                bass = (float)synth.VisBass;
                energy = (float)synth.VisEnergy;
                perc = (float)synth.VisPerc;
                lead = (float)synth.VisLead;
                chord = synth.VisChord;
            }

            // Constant slow walking pace, independent of music.
            const float WalkSpeed = 1.1f;
            _camZ += WalkSpeed * dt;

            // Step phase tied to distance travelled — one leg cycle per 1.5m.
            _stepPhase += WalkSpeed * dt * (6.2832f / 1.5f);
            if (_stepPhase > 1e6f) _stepPhase -= 1e6f;

            _pipeline.Render(fb, w, h, time, bass, energy, perc, lead, chord,
                _camZ, _stepPhase);
            if (!_readyFired)
            {
                _readyFired = true;
                Ready?.Invoke();
            }
            RequestNextFrameRendering();
        }

        private static unsafe void GlClearMagenta(GlInterface gl, int fb)
        {
            const int GL_COLOR_BUFFER_BIT = 0x4000;
            const int GL_FRAMEBUFFER = 0x8D40;
            var bindFb = (delegate* unmanaged<uint, uint, void>)gl.GetProcAddress("glBindFramebuffer");
            var clearColor = (delegate* unmanaged<float, float, float, float, void>)gl.GetProcAddress("glClearColor");
            var clear = (delegate* unmanaged<uint, void>)gl.GetProcAddress("glClear");
            if (bindFb == null || clearColor == null || clear == null)
                return;
            bindFb(GL_FRAMEBUFFER, (uint)fb);
            clearColor(1f, 0f, 1f, 1f);
            clear(GL_COLOR_BUFFER_BIT);
        }

        protected override void OnOpenGlDeinit(GlInterface gl)
        {
            _pipeline?.Dispose();
            _pipeline = null;
        }
    }
}
