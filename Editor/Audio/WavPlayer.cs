using System;
using System.Threading;

namespace Editor.Audio
{
    // Plays a mono IMA ADPCM (fmt code 0x0011) WAV embedded as a resource
    // and derives music-reactive visual drivers (bass/energy/perc/lead/chord)
    // from the decoded PCM in real time.
    public sealed class WavPlayer : IDisposable
    {
        private const int OutBufferSamples = 2048;
        private const int OutBufferBytes = OutBufferSamples * 2;
        private const int NumBuffers = 2;

        private int _sampleRate;
        private short[] _pcm;
        private int _posSamples;

        private Audio _audio;
        private byte[] _outBuffer;
        private Thread _thread;
        private volatile bool _running;
        private bool _disposed;

        // Visual drivers — same names/semantics as ChipSynth.
        public double VisBass, VisLead, VisPerc, VisEnergy;
        public int VisChord;

        // Driver state
        private double _bassLpf;
        private double _midHp;
        private double _envBass, _envEnergy, _envLead;
        private double _prevBassAbs;
        private double _percEnv;
        private double _chordPhase;

        public static bool IsSupported => Audio.IsSupported;

        public void Play()
        {
            if (!IsSupported || _audio != null)
                return;
            try
            {
                if (!LoadAndDecode())
                    return;
                _audio = new Audio(_sampleRate, 1, 16, OutBufferSamples, NumBuffers);
                if (!_audio.Open())
                {
                    _audio = null;
                    return;
                }
                _audio.SetVolume(0.7f);
                _outBuffer = new byte[OutBufferBytes];
                for (int i = 0; i < NumBuffers; i++)
                {
                    FillNext();
                    _audio.SubmitBuffer(_outBuffer, OutBufferBytes);
                }
                _running = true;
                _thread = new Thread(Loop) { IsBackground = true, Name = "WavPlayer" };
                _thread.Start();
            }
            catch { _audio?.Dispose(); _audio = null; }
        }

        public void Stop()
        {
            _running = false;
            _thread?.Join(500);
            if (_audio != null)
            {
                try { _audio.Close(); } catch { }
                _audio = null;
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;
            Stop();
        }

        private void Loop()
        {
            while (_running)
            {
                if (_audio.IsBufferAvailable())
                {
                    FillNext();
                    _audio.SubmitBuffer(_outBuffer, OutBufferBytes);
                }
                Thread.Sleep(5);
            }
        }

        private void FillNext()
        {
            if (_pcm == null || _pcm.Length == 0)
                return;
            int total = _pcm.Length;
            for (int i = 0; i < OutBufferSamples; i++)
            {
                short s = _pcm[_posSamples];
                _outBuffer[i * 2] = (byte)(s & 0xFF);
                _outBuffer[i * 2 + 1] = (byte)((s >> 8) & 0xFF);

                UpdateDrivers(s * (1.0 / 32768.0));

                _posSamples++;
                if (_posSamples >= total)
                    _posSamples = 0;
            }
        }

        private void UpdateDrivers(double x)
        {
            // Narrow sub-bass LPF (~90 Hz) to isolate kick fundamentals.
            // 150 Hz was too wide — it caught bass lines + kicks indistinguishably.
            const double bassAlpha = 0.035;
            _bassLpf += bassAlpha * (x - _bassLpf);
            double absBass = Math.Abs(_bassLpf);
            double bass = absBass * 5.0;

            // HPF at ~500 Hz isolates mids/highs for the "lead" driver.
            const double midAlpha = 0.19;
            _midHp += midAlpha * (x - _midHp);
            double mid = Math.Abs(x - _midHp) * 1.8;

            // Broadband rectified energy.
            double energy = Math.Abs(x) * 1.6;

            // VisBass now shaped like a kick envelope: fast attack, FAST release
            // (~30ms half-life) so it spikes on each kick and falls near zero
            // between kicks rather than staying sustained during bass lines.
            _envBass = _envBass < bass ? bass : _envBass * 0.9940;
            _envEnergy = _envEnergy < energy ? energy : _envEnergy * 0.9992;
            _envLead = _envLead < mid ? mid : _envLead * 0.9988;

            // Perc: rising edge of SUB-BASS (not broadband) with a threshold to
            // suppress small fluctuations, and fast release so kicks fire
            // discretely instead of staying saturated.
            double bassDelta = absBass - _prevBassAbs;
            if (bassDelta < 0.003) bassDelta = 0;
            bassDelta *= 60.0;
            _percEnv = _percEnv < bassDelta ? bassDelta : _percEnv * 0.9945;
            _prevBassAbs = absBass;

            // No harmonic analysis for a pre-recorded track — slowly drift the
            // hue index so the palette still evolves over song length.
            _chordPhase += 1.0 / _sampleRate;

            VisBass = _envBass > 1.0 ? 1.0 : _envBass;
            VisEnergy = _envEnergy > 1.0 ? 1.0 : _envEnergy;
            VisPerc = _percEnv > 1.0 ? 1.0 : _percEnv;
            VisLead = _envLead > 1.0 ? 1.0 : _envLead;
            VisChord = (int)(_chordPhase * 0.12) & 7;
        }

        private bool LoadAndDecode()
        {
            // Loaded as an Avalonia asset (Assets/Audio/WalkerSim.adpcm.wav)
            // — same convention as the shaders, picked up automatically by
            // the csproj's <AvaloniaResource Include="Assets\**"/> glob.
            using var stream = Avalonia.Platform.AssetLoader.Open(
                new Uri("avares://Editor/Assets/Audio/WalkerSim.adpcm.wav"));
            if (stream == null)
                return false;
            using var ms = new System.IO.MemoryStream();
            stream.CopyTo(ms);
            return ParseAndDecode(ms.ToArray());
        }

        private bool ParseAndDecode(byte[] b)
        {
            if (b.Length < 44) return false;
            if (b[0] != 'R' || b[1] != 'I' || b[2] != 'F' || b[3] != 'F') return false;
            if (b[8] != 'W' || b[9] != 'A' || b[10] != 'V' || b[11] != 'E') return false;

            int fmtCode = 0, channels = 0, sampleRate = 0, blockAlign = 0, samplesPerBlock = 0;
            int dataOff = -1, dataLen = 0;

            int p = 12;
            while (p + 8 <= b.Length)
            {
                int id = b[p] | (b[p + 1] << 8) | (b[p + 2] << 16) | (b[p + 3] << 24);
                int size = b[p + 4] | (b[p + 5] << 8) | (b[p + 6] << 16) | (b[p + 7] << 24);
                int body = p + 8;
                const int ID_FMT = 0x20746D66;  // "fmt "
                const int ID_DATA = 0x61746164; // "data"
                if (id == ID_FMT && body + 16 <= b.Length)
                {
                    fmtCode = b[body] | (b[body + 1] << 8);
                    channels = b[body + 2] | (b[body + 3] << 8);
                    sampleRate = b[body + 4] | (b[body + 5] << 8) | (b[body + 6] << 16) | (b[body + 7] << 24);
                    blockAlign = b[body + 12] | (b[body + 13] << 8);
                    if (size >= 20 && body + 20 <= b.Length)
                        samplesPerBlock = b[body + 18] | (b[body + 19] << 8);
                }
                else if (id == ID_DATA)
                {
                    dataOff = body;
                    dataLen = size;
                }
                int step = size + (size & 1);
                p = body + step;
                if (dataOff >= 0 && fmtCode != 0) break;
            }

            if (dataOff < 0 || fmtCode != 0x0011 || channels != 1 || blockAlign < 5)
                return false;
            if (samplesPerBlock <= 0)
                samplesPerBlock = ((blockAlign - 4) * 2) + 1;

            _sampleRate = sampleRate;

            if (dataOff + dataLen > b.Length)
                dataLen = b.Length - dataOff;
            int numBlocks = dataLen / blockAlign;
            int totalSamples = numBlocks * samplesPerBlock;
            _pcm = new short[totalSamples];

            int outIdx = 0;
            for (int bl = 0; bl < numBlocks; bl++)
            {
                int bOff = dataOff + bl * blockAlign;
                DecodeImaBlock(b, bOff, blockAlign, _pcm, outIdx);
                outIdx += samplesPerBlock;
            }
            return true;
        }

        private static readonly int[] ImaIndexTable =
        {
            -1, -1, -1, -1, 2, 4, 6, 8,
            -1, -1, -1, -1, 2, 4, 6, 8,
        };

        private static readonly int[] ImaStepTable =
        {
            7, 8, 9, 10, 11, 12, 13, 14, 16, 17, 19, 21, 23, 25, 28, 31,
            34, 37, 41, 45, 50, 55, 60, 66, 73, 80, 88, 97, 107, 118, 130, 143,
            157, 173, 190, 209, 230, 253, 279, 307, 337, 371, 408, 449, 494, 544,
            598, 658, 724, 796, 876, 964, 1060, 1166, 1282, 1411, 1552, 1707, 1878,
            2066, 2272, 2499, 2749, 3024, 3327, 3660, 4026, 4428, 4871, 5358, 5894,
            6484, 7132, 7845, 8630, 9493, 10442, 11487, 12635, 13899, 15289, 16818,
            18500, 20350, 22385, 24623, 27086, 29794, 32767
        };

        private static void DecodeImaBlock(byte[] src, int off, int blockSize, short[] dst, int dstIdx)
        {
            int predictor = (short)(src[off] | (src[off + 1] << 8));
            int stepIndex = src[off + 2];
            if (stepIndex < 0) stepIndex = 0;
            else if (stepIndex > 88) stepIndex = 88;
            dst[dstIdx++] = (short)predictor;

            for (int i = 4; i < blockSize; i++)
            {
                byte data = src[off + i];

                // Low nibble = next sample, high nibble = sample after that.
                int nibble = data & 0x0F;
                int step = ImaStepTable[stepIndex];
                int diff = step >> 3;
                if ((nibble & 1) != 0) diff += step >> 2;
                if ((nibble & 2) != 0) diff += step >> 1;
                if ((nibble & 4) != 0) diff += step;
                if ((nibble & 8) != 0) predictor -= diff;
                else predictor += diff;
                if (predictor > 32767) predictor = 32767;
                else if (predictor < -32768) predictor = -32768;
                stepIndex += ImaIndexTable[nibble];
                if (stepIndex < 0) stepIndex = 0;
                else if (stepIndex > 88) stepIndex = 88;
                dst[dstIdx++] = (short)predictor;

                nibble = (data >> 4) & 0x0F;
                step = ImaStepTable[stepIndex];
                diff = step >> 3;
                if ((nibble & 1) != 0) diff += step >> 2;
                if ((nibble & 2) != 0) diff += step >> 1;
                if ((nibble & 4) != 0) diff += step;
                if ((nibble & 8) != 0) predictor -= diff;
                else predictor += diff;
                if (predictor > 32767) predictor = 32767;
                else if (predictor < -32768) predictor = -32768;
                stepIndex += ImaIndexTable[nibble];
                if (stepIndex < 0) stepIndex = 0;
                else if (stepIndex > 88) stepIndex = 88;
                dst[dstIdx++] = (short)predictor;
            }
        }
    }
}
