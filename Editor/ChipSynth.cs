using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Editor
{
    public sealed class ChipSynth : IDisposable
    {
        private const int WAVE_FORMAT_PCM = 1;
        private const int CALLBACK_NULL = 0;
        private const int WHDR_DONE = 0x01;
        private const int WHDR_PREPARED = 0x02;

        [StructLayout(LayoutKind.Sequential)]
        private struct WAVEFORMATEX
        {
            public ushort wFormatTag;
            public ushort nChannels;
            public uint nSamplesPerSec;
            public uint nAvgBytesPerSec;
            public ushort nBlockAlign;
            public ushort wBitsPerSample;
            public ushort cbSize;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct WAVEHDR
        {
            public IntPtr lpData;
            public uint dwBufferLength;
            public uint dwBytesRecorded;
            public IntPtr dwUser;
            public uint dwFlags;
            public uint dwLoops;
            public IntPtr lpNext;
            public IntPtr reserved;
        }

        [DllImport("winmm.dll")]
        private static extern int waveOutOpen(out IntPtr hWaveOut, int uDeviceID,
            ref WAVEFORMATEX lpFormat, IntPtr dwCallback, IntPtr dwInstance, int fdwOpen);

        [DllImport("winmm.dll")]
        private static extern int waveOutPrepareHeader(IntPtr hWaveOut, IntPtr lpWaveHdr, int uSize);

        [DllImport("winmm.dll")]
        private static extern int waveOutUnprepareHeader(IntPtr hWaveOut, IntPtr lpWaveHdr, int uSize);

        [DllImport("winmm.dll")]
        private static extern int waveOutWrite(IntPtr hWaveOut, IntPtr lpWaveHdr, int uSize);

        [DllImport("winmm.dll")]
        private static extern int waveOutClose(IntPtr hWaveOut);

        [DllImport("winmm.dll")]
        private static extern int waveOutReset(IntPtr hWaveOut);

        [DllImport("winmm.dll")]
        private static extern int waveOutSetVolume(IntPtr hWaveOut, uint dwVolume);

        private const int SampleRate = 22050;
        private const int NumBuffers = 2;
        private const int BufferSamples = 4096;
        private const int BufferBytes = BufferSamples * 2;
        private static readonly int HdrSize = Marshal.SizeOf<WAVEHDR>();
        private static readonly int FlagsOffset = IntPtr.Size + 4 + 4 + IntPtr.Size;

        private const double BPM = 100.0;
        private const int StepsPerBar = 4;
        private const double StepDur = 60.0 / BPM;
        private const int SamplesPerStep = (int)(StepDur * SampleRate);
        private const int TotalBars = 20;
        private const int PatternLen = TotalBars * StepsPerBar;

        private IntPtr _hWaveOut;
        private readonly IntPtr[] _hdrPtrs = new IntPtr[NumBuffers];
        private readonly GCHandle[] _pinned = new GCHandle[NumBuffers];
        private readonly byte[][] _buffers = new byte[NumBuffers][];
        private Thread _thread;
        private volatile bool _running;
        private bool _disposed;
        private bool _opened;

        private double _bellCarrPhase;
        private double _bellModPhase;
        private double _choirPhase0;
        private double _choirPhase1;
        private double _choirFilter0;
        private double _choirFilter1;
        private double _dronePhase;
        private double _dronePhase2;
        private double _pizzPhase;
        private double _lfoPhase;
        private uint _noiseSeed = 12345;
        private long _samplePos;

        public double VisBass;
        public double VisLead;
        public double VisPerc;
        public int VisChord;
        public double VisLeadNote;

        // Celesta melody in D minor (quarter notes, 80 steps = 20 bars x 4)
        private static readonly int[] Melody =
        {
            // M1-4: sparse intro (single notes hinting at the theme)
            -1, -1, -1, -1,  -1, -1, 74, -1,  -1, 72, -1, 69,  -1, 70, 72, -1,
            // M5-8: motif enters
            74, 77, 76, 74,  77, 74, 70, 69,  70, 74, 72, 70,  69, 72, 76, 69,
            // M9-12: second phrase
            69, 74, 77, 76,  74, 72, 70, 69,  67, 70, 74, 72,  76, 72, 69, 67,
            // M13-16: variation (reaches higher)
            74, 77, 81, 77,  76, 74, 70, 69,  70, 74, 79, 77,  76, 72, 69, 67,
            // M17-20: return and resolve
            74, 77, 76, 74,  77, 74, 70, 69,  70, 74, 72, 70,  69, 76, 74, 69,
        };

        // Bass drone: one note per bar (whole notes, Dm-Bb-Gm-A progression)
        private static readonly int[] Bass =
        {
            38, 34, 31, 33,
            38, 34, 31, 33,
            38, 34, 31, 33,
            38, 34, 31, 33,
            38, 34, 31, 33,
        };

        // Choir dyads per bar (sustained open voicings)
        private static readonly int[][] Choir =
        {
            new[] { 62, 69 }, new[] { 58, 65 }, new[] { 55, 62 }, new[] { 57, 64 },
            new[] { 62, 69 }, new[] { 58, 65 }, new[] { 55, 62 }, new[] { 57, 64 },
            new[] { 62, 69 }, new[] { 58, 65 }, new[] { 55, 62 }, new[] { 57, 64 },
            new[] { 62, 69 }, new[] { 58, 65 }, new[] { 55, 62 }, new[] { 57, 64 },
            new[] { 62, 69 }, new[] { 58, 65 }, new[] { 55, 62 }, new[] { 57, 64 },
        };

        // Pizzicato: two notes per bar for beats 1 and 3 (null = silent)
        private static readonly int[][] Pizz =
        {
            null, null,
            new[] { 43, 50 }, new[] { 45, 52 },
            new[] { 50, 57 }, new[] { 46, 53 },
            new[] { 43, 50 }, new[] { 45, 52 },
            new[] { 50, 57 }, new[] { 46, 53 },
            new[] { 43, 50 }, new[] { 45, 52 },
            new[] { 50, 57 }, new[] { 46, 53 },
            new[] { 43, 50 }, new[] { 45, 52 },
            new[] { 50, 57 }, new[] { 46, 53 },
            new[] { 43, 50 }, new[] { 45, 52 },
        };

        public static bool IsSupported => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        public void Play()
        {
            if (!IsSupported || _opened)
            {
                return;
            }

            try
            {
                var fmt = new WAVEFORMATEX
                {
                    wFormatTag = WAVE_FORMAT_PCM,
                    nChannels = 1,
                    nSamplesPerSec = SampleRate,
                    wBitsPerSample = 16,
                    nBlockAlign = 2,
                    nAvgBytesPerSec = SampleRate * 2,
                    cbSize = 0
                };

                int result = waveOutOpen(out _hWaveOut, -1, ref fmt,
                    IntPtr.Zero, IntPtr.Zero, CALLBACK_NULL);
                if (result != 0)
                {
                    return;
                }
                _opened = true;

                waveOutSetVolume(_hWaveOut, 0xCC00_CC00);

                for (int i = 0; i < NumBuffers; i++)
                {
                    _buffers[i] = new byte[BufferBytes];
                    _pinned[i] = GCHandle.Alloc(_buffers[i], GCHandleType.Pinned);

                    _hdrPtrs[i] = Marshal.AllocHGlobal(HdrSize);
                    var hdr = new WAVEHDR
                    {
                        lpData = _pinned[i].AddrOfPinnedObject(),
                        dwBufferLength = BufferBytes,
                    };
                    Marshal.StructureToPtr(hdr, _hdrPtrs[i], false);

                    waveOutPrepareHeader(_hWaveOut, _hdrPtrs[i], HdrSize);

                    FillBuffer(i);
                    waveOutWrite(_hWaveOut, _hdrPtrs[i], HdrSize);
                }

                _running = true;
                _thread = new Thread(StreamLoop)
                {
                    IsBackground = true,
                    Name = "ChipSynth"
                };
                _thread.Start();
            }
            catch
            {
                _opened = false;
            }
        }

        public void Stop()
        {
            _running = false;
            _thread?.Join(500);

            if (!_opened)
            {
                return;
            }

            try
            {
                waveOutReset(_hWaveOut);

                for (int i = 0; i < NumBuffers; i++)
                {
                    if (_hdrPtrs[i] != IntPtr.Zero)
                    {
                        uint flags = ReadFlags(i);
                        if ((flags & WHDR_PREPARED) != 0)
                        {
                            waveOutUnprepareHeader(_hWaveOut, _hdrPtrs[i], HdrSize);
                        }
                        Marshal.FreeHGlobal(_hdrPtrs[i]);
                        _hdrPtrs[i] = IntPtr.Zero;
                    }
                    if (_pinned[i].IsAllocated)
                    {
                        _pinned[i].Free();
                    }
                }

                waveOutClose(_hWaveOut);
            }
            catch { }

            _opened = false;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;
            Stop();
        }

        private uint ReadFlags(int bufIdx)
        {
            return (uint)Marshal.ReadInt32(_hdrPtrs[bufIdx], FlagsOffset);
        }

        private void StreamLoop()
        {
            while (_running)
            {
                for (int i = 0; i < NumBuffers; i++)
                {
                    uint flags = ReadFlags(i);
                    if ((flags & WHDR_DONE) != 0)
                    {
                        FillBuffer(i);
                        waveOutWrite(_hWaveOut, _hdrPtrs[i], HdrSize);
                    }
                }
                Thread.Sleep(5);
            }
        }

        private void FillBuffer(int bufIdx)
        {
            var buf = _buffers[bufIdx];

            for (int i = 0; i < BufferSamples; i++)
            {
                double sample = GenerateSample();
                _samplePos++;

                sample = Math.Tanh(sample * 1.5);

                short pcm = (short)(Math.Clamp(sample, -1.0, 1.0) * 30000);
                buf[i * 2] = (byte)(pcm & 0xFF);
                buf[i * 2 + 1] = (byte)((pcm >> 8) & 0xFF);
            }

            UpdateVisState();
        }

        private void UpdateVisState()
        {
            int step = (int)(_samplePos / SamplesPerStep);
            int sampleInStep = (int)(_samplePos % SamplesPerStep);
            double t = (double)sampleInStep / SamplesPerStep;

            int bar = (step / StepsPerBar) % TotalBars;
            int beat = step % StepsPerBar;
            int melodyIdx = step % PatternLen;

            VisChord = bar % 4;

            int note = Melody[melodyIdx];
            VisLead = (note >= 0) ? Decay(t, 0.001, 0.3) : 0;
            VisLeadNote = (note >= 0) ? note / 127.0 : 0;

            VisBass = 0.7;

            VisPerc = (bar >= 8 && (beat == 0 || beat == 2)) ? Decay(t, 0.0005, 0.012) : 0;
        }

        private double GenerateSample()
        {
            int step = (int)(_samplePos / SamplesPerStep);
            int sampleInStep = (int)(_samplePos % SamplesPerStep);
            double t = (double)sampleInStep / SamplesPerStep;

            int bar = (step / StepsPerBar) % TotalBars;
            int beat = step % StepsPerBar;
            int melodyIdx = step % PatternLen;

            _lfoPhase += 5.0 / SampleRate;
            if (_lfoPhase >= 1.0)
            {
                _lfoPhase -= 1.0;
            }
            double lfo = Math.Sin(_lfoPhase * 2.0 * Math.PI);

            double sample = 0;

            // Dark bell / kalimba (FM synthesis)
            int note = Melody[melodyIdx];
            if (note >= 0)
            {
                double env = Decay(t, 0.001, 0.3);
                double freq = MidiToFreq(note);
                _bellModPhase += (freq * 3.5) / SampleRate;
                if (_bellModPhase >= 1.0)
                {
                    _bellModPhase -= 1.0;
                }
                double modDepth = freq * 0.2 * env;
                double modSig = Math.Sin(_bellModPhase * 2.0 * Math.PI) * modDepth;
                _bellCarrPhase += (freq + modSig) / SampleRate;
                if (_bellCarrPhase >= 1.0)
                {
                    _bellCarrPhase -= 1.0;
                }
                double bell = Math.Sin(_bellCarrPhase * 2.0 * Math.PI);
                sample += bell * 0.07 * env;
            }

            // Choir (sustained filtered saw dyads with vibrato)
            var choir = Choir[bar];
            if (choir != null)
            {
                int samplesPerBar = SamplesPerStep * StepsPerBar;
                int sampleInBar = beat * SamplesPerStep + sampleInStep;
                double tBar = (double)sampleInBar / samplesPerBar;
                double choirEnv = Math.Min(1.0, tBar / 0.3) * (0.85 + 0.15 * (1.0 - tBar));
                double vibrato = 1.0 + lfo * 0.003;

                double freq0 = MidiToFreq(choir[0]) * vibrato;
                double freq1 = MidiToFreq(choir[1]) * vibrato;

                _choirPhase0 += freq0 / SampleRate;
                if (_choirPhase0 >= 1.0)
                {
                    _choirPhase0 -= 1.0;
                }
                _choirPhase1 += freq1 / SampleRate;
                if (_choirPhase1 >= 1.0)
                {
                    _choirPhase1 -= 1.0;
                }

                double raw0 = 2.0 * _choirPhase0 - 1.0;
                double raw1 = 2.0 * _choirPhase1 - 1.0;

                double cutoff = 400.0 + 100.0 * lfo;
                double rc = 1.0 / (cutoff * 2.0 * Math.PI);
                double alpha = (1.0 / SampleRate) / (rc + 1.0 / SampleRate);
                _choirFilter0 += alpha * (raw0 - _choirFilter0);
                _choirFilter1 += alpha * (raw1 - _choirFilter1);

                sample += (_choirFilter0 + _choirFilter1) * 0.10 * choirEnv;
            }

            // Low Strings / Drone (detuned sine pair)
            int bassNote = Bass[bar];
            if (bassNote >= 0)
            {
                double freq = MidiToFreq(bassNote);
                _dronePhase += freq / SampleRate;
                if (_dronePhase >= 1.0)
                {
                    _dronePhase -= 1.0;
                }
                _dronePhase2 += (freq * 1.002) / SampleRate;
                if (_dronePhase2 >= 1.0)
                {
                    _dronePhase2 -= 1.0;
                }
                double drone = Math.Sin(_dronePhase * 2.0 * Math.PI) * 0.5
                             + Math.Sin(_dronePhase2 * 2.0 * Math.PI) * 0.5;
                sample += drone * 0.16;
            }

            // Pizzicato (short pluck on beats 1 and 3)
            var pizz = Pizz[bar];
            if (pizz != null && (beat == 0 || beat == 2))
            {
                int pizzNote = (beat == 0) ? pizz[0] : pizz[1];
                double pizzEnv = Decay(t, 0.001, 0.08);
                double freq = MidiToFreq(pizzNote);
                _pizzPhase += freq / SampleRate;
                if (_pizzPhase >= 1.0)
                {
                    _pizzPhase -= 1.0;
                }
                sample += Math.Sin(_pizzPhase * 2.0 * Math.PI) * 0.10 * pizzEnv;
            }

            // Clock ticks (bars 9 onward, beats 1 and 3)
            if (bar >= 8 && (beat == 0 || beat == 2))
            {
                double tickEnv = Decay(t, 0.0005, 0.012);
                _noiseSeed = _noiseSeed * 1103515245 + 12345;
                double noise = ((_noiseSeed >> 16) & 0xFFFF) / 65535.0 * 2.0 - 1.0;
                sample += noise * 0.05 * tickEnv;
            }

            return sample;
        }

        private static double MidiToFreq(int midi)
        {
            return 440.0 * Math.Pow(2.0, (midi - 69) / 12.0);
        }

        private static double Decay(double t, double attack, double decay)
        {
            if (t < attack)
            {
                return t / attack;
            }
            return Math.Exp(-(t - attack) / decay);
        }
    }
}
