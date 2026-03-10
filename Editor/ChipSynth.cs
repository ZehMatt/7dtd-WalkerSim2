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

        private const double BPM = 80.0;
        private const double TotalBeats = 60.0;
        private const double SecPerBeat = 60.0 / BPM;
        private const double SmpPerBeat = SecPerBeat * SampleRate;
        private static readonly long TotalSmp = (long)(TotalBeats * SmpPerBeat);

        private IntPtr _hWaveOut;
        private readonly IntPtr[] _hdrPtrs = new IntPtr[NumBuffers];
        private readonly GCHandle[] _pinned = new GCHandle[NumBuffers];
        private readonly byte[][] _buffers = new byte[NumBuffers][];
        private Thread _thread;
        private volatile bool _running;
        private bool _disposed;
        private bool _opened;

        private readonly double[] _csPhase0 = new double[6];
        private double _csFilt0;
        private double _bassPhase, _bass2Phase, _bassFilt;
        private double _kickPhase;
        private double _snrPhase;
        private uint _noiseSeed = 12345;
        private double _v0Phase, _v0Phase2;
        private double _v1Phase, _v1Phase2;
        private double _synthPhase, _synth2Phase;
        private double _orgPhase, _orgFilt;
        private double _strP0, _strP1, _strP2, _strP3, _strFilt;
        private double _lfoPhase;
        private long _samplePos;

        private static readonly int[] CombLen = { 661, 773, 887, 1013 };
        private readonly double[][] _combBuf = new double[4][];
        private readonly int[] _combIdx = new int[4];
        private readonly double[] _combFlt = new double[4];
        private static readonly int[] ApLen = { 113, 53 };
        private readonly double[][] _apBuf = new double[2][];
        private readonly int[] _apIdx = new int[2];
        private bool _rvbInit;

        public double VisBass;
        public double VisLead;
        public double VisPerc;
        public int VisChord;
        public double VisLeadNote;
        public double VisChoir;
        public double VisFlute;
        public double VisBassNote;
        public double VisEnergy;
        public double VisPizz;

        private static readonly int[] _melody =
        {
            -1, -1, -1, -1,  -1, -1, 74, -1,  -1, 72, -1, 69,  -1, 70, 72, -1,
            74, 77, 76, 74,  77, 74, 70, 69,  70, 74, 72, 70,  69, 72, 76, 69,
            69, 74, 77, 76,  74, 72, 70, 69,  67, 70, 74, 72,  76, 72, 69, 67,
            74, 77, 81, 77,  76, 74, 70, 69,  70, 74, 79, 77,
        };

        private static readonly int[] _bassR =
        {
            38, 34, 31, 33,  38, 34, 31, 33,  38, 34, 31, 33,  38, 34, 31,
        };

        private static readonly int[] _choirLo =
        {
            62, 58, 55, 57,  62, 58, 55, 57,  62, 58, 55, 57,  62, 58, 55,
        };

        private static readonly int[] _choirHi =
        {
            69, 65, 62, 64,  69, 65, 62, 64,  69, 65, 62, 64,  69, 65, 62,
        };

        private static readonly int[] _pizzA =
        {
            -1, -1, 43, 45,  50, 46, 43, 45,  50, 46, 43, 45,  50, 46, 43,
        };

        private static readonly int[] _pizzB =
        {
            -1, -1, 50, 52,  57, 53, 50, 52,  57, 53, 50, 52,  57, 53, 50,
        };

        private static readonly double[] CsDetune =
        {
            0.990, 0.995, 0.998, 1.002, 1.005, 1.010
        };

        public static bool IsSupported => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        public void Play()
        {
            if (!IsSupported || _opened)
                return;

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
                    return;

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
                return;

            try
            {
                waveOutReset(_hWaveOut);

                for (int i = 0; i < NumBuffers; i++)
                {
                    if (_hdrPtrs[i] != IntPtr.Zero)
                    {
                        uint flags = ReadFlags(i);
                        if ((flags & WHDR_PREPARED) != 0)
                            waveOutUnprepareHeader(_hWaveOut, _hdrPtrs[i], HdrSize);

                        Marshal.FreeHGlobal(_hdrPtrs[i]);
                        _hdrPtrs[i] = IntPtr.Zero;
                    }
                    if (_pinned[i].IsAllocated)
                        _pinned[i].Free();
                }

                waveOutClose(_hWaveOut);
            }
            catch { }

            _opened = false;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

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
            if (!_rvbInit)
            {
                for (int i = 0; i < 4; i++)
                    _combBuf[i] = new double[CombLen[i]];
                for (int i = 0; i < 2; i++)
                    _apBuf[i] = new double[ApLen[i]];
                _rvbInit = true;
            }

            var buf = _buffers[bufIdx];

            for (int i = 0; i < BufferSamples; i++)
            {
                double dry = GenerateSample();
                double wet = Reverb(dry * 0.35);
                double sample = Math.Tanh((dry + wet) * 2.5);
                _samplePos++;

                short pcm = (short)(Math.Clamp(sample, -1.0, 1.0) * 30000);
                buf[i * 2] = (byte)(pcm & 0xFF);
                buf[i * 2 + 1] = (byte)((pcm >> 8) & 0xFF);
            }

            UpdateVisState();
        }

        private double Reverb(double input)
        {
            double combOut = 0;
            for (int c = 0; c < 4; c++)
            {
                double d = _combBuf[c][_combIdx[c]];
                _combFlt[c] = d * 0.7 + _combFlt[c] * 0.3;
                _combBuf[c][_combIdx[c]] = input + _combFlt[c] * 0.8;
                _combIdx[c] = (_combIdx[c] + 1) % CombLen[c];
                combOut += d;
            }
            combOut *= 0.25;

            double ap = combOut;
            for (int a = 0; a < 2; a++)
            {
                double bufOut = _apBuf[a][_apIdx[a]];
                double nv = ap + 0.5 * bufOut;
                ap = bufOut - 0.5 * nv;
                _apBuf[a][_apIdx[a]] = nv;
                _apIdx[a] = (_apIdx[a] + 1) % ApLen[a];
            }
            return ap;
        }

        private void UpdateVisState()
        {
            double beat = (_samplePos % TotalSmp) / SmpPerBeat;
            int bar = Math.Min((int)(beat / 4.0), _bassR.Length - 1);
            int beatIdx = (int)beat;

            VisChord = bar % 4;

            if (beatIdx >= 0 && beatIdx < _melody.Length)
            {
                int note = _melody[beatIdx];
                if (note >= 0)
                {
                    double frac = beat - beatIdx;
                    VisLead = Math.Exp(-frac * 4);
                    VisLeadNote = (note - 62) / 20.0;
                }
                else
                {
                    VisLead *= 0.95;
                }
            }

            double halfFrac = (beat * 2) - Math.Floor(beat * 2);
            VisBass = Math.Exp(-halfFrac * 6);
            VisBassNote = (_bassR[bar] - 31) / 7.0;

            if (beat >= 4)
            {
                double barBeat = beat - bar * 4.0;
                double barFrac = barBeat / 4.0;
                VisChoir = Math.Min(1.0, barFrac / 0.08) * (0.85 + 0.15 * (1.0 - barFrac));
            }
            else
            {
                VisChoir = 0;
            }

            if (beat >= 16 && beatIdx < _melody.Length && _melody[beatIdx] >= 0)
            {
                double frac = beat - beatIdx;
                VisFlute = Math.Exp(-frac * 3);
            }
            else
            {
                VisFlute *= 0.92;
            }

            if (beat >= 8)
            {
                int barBeat = beatIdx % 4;
                double bf = beat - Math.Floor(beat);
                if (barBeat == 0 || barBeat == 2)
                {
                    int pNote = (barBeat == 0) ? _pizzA[bar] : _pizzB[bar];
                    if (pNote >= 0)
                        VisPizz = Math.Exp(-bf * 12);
                    else
                        VisPizz *= 0.9;
                }
                else
                {
                    VisPizz *= 0.9;
                }
            }

            double bfrac = beat - Math.Floor(beat);
            int bi = (int)Math.Floor(beat);
            if (beat >= 32 && (bi % 4 == 0 || bi % 4 == 2))
                VisPerc = Math.Exp(-bfrac * 8);
            else if (bi % 2 == 1)
                VisPerc = Math.Exp(-bfrac * 6);
            else
                VisPerc = Math.Exp(-bfrac * 10) * 0.4;

            VisEnergy = VisBass * 0.2 + VisLead * 0.25 + VisChoir * 0.2
                      + VisFlute * 0.15 + VisPizz * 0.1 + VisPerc * 0.1;
        }

        private double Noise()
        {
            _noiseSeed = _noiseSeed * 1103515245 + 12345;
            return ((_noiseSeed >> 16) & 0xFFFF) / 65535.0 * 2.0 - 1.0;
        }

        private double GenerateSample()
        {
            double beat = (_samplePos % TotalSmp) / SmpPerBeat;
            double sample = 0;

            _lfoPhase += 4.5 / SampleRate;
            if (_lfoPhase >= 1)
                _lfoPhase -= 1;
            double lfo = Math.Sin(_lfoPhase * 2 * Math.PI);

            sample += GenChainsaw(beat);
            sample += GenBass(beat);
            sample += GenDrums(beat);
            if (beat >= 4)
                sample += GenChoir(beat, lfo);
            if (beat >= 16)
                sample += GenOrgan(beat);
            if (beat >= 8)
                sample += GenPizz(beat);
            sample += GenStrings(beat, lfo);

            return sample;
        }

        private double GenChainsaw(double beat)
        {
            int beatIdx = (int)beat;
            if (beatIdx < 0 || beatIdx >= _melody.Length)
                return 0;

            int note = _melody[beatIdx];
            if (note < 0)
                return 0;

            double frac = beat - beatIdx;
            double sec = frac * SecPerBeat;
            double env = Env(sec, 0.001, 0.25);

            return Chainsaw(_csPhase0, ref _csFilt0, note, env) * 0.13;
        }

        private double Chainsaw(double[] phases, ref double filt, int note, double env)
        {
            double freq = MidiHz(note);
            double sum = 0;
            for (int i = 0; i < 6; i++)
            {
                phases[i] += freq * CsDetune[i] / SampleRate;
                if (phases[i] >= 1)
                    phases[i] -= 1;
                sum += 2 * phases[i] - 1;
            }
            sum /= 6.0;
            sum = Math.Tanh(sum * 4.0) * env;

            double cutoff = 300 + 1200 * env;
            double rc = 1.0 / (cutoff * 2 * Math.PI);
            double alpha = (1.0 / SampleRate) / (rc + 1.0 / SampleRate);
            filt += alpha * (sum - filt);
            return filt;
        }

        private double GenBass(double beat)
        {
            double halfFrac = (beat * 2) - Math.Floor(beat * 2);
            double sec = halfFrac * SecPerBeat / 2;
            double env = Env(sec, 0.001, 0.12);

            int bar = Math.Min((int)(beat / 4.0), _bassR.Length - 1);
            double freq = MidiHz(_bassR[bar]);

            _bassPhase += freq / SampleRate;
            if (_bassPhase >= 1)
                _bassPhase -= 1;
            double saw = 2 * _bassPhase - 1;

            _bass2Phase += (freq * 0.5) / SampleRate;
            if (_bass2Phase >= 1)
                _bass2Phase -= 1;
            double sub = Math.Sin(_bass2Phase * 2 * Math.PI);

            double raw = Math.Tanh((saw * 0.6 + sub * 0.4) * 2.5);

            double cutoff = 250 + 350 * env;
            double rc = 1.0 / (cutoff * 2 * Math.PI);
            double alpha = (1.0 / SampleRate) / (rc + 1.0 / SampleRate);
            _bassFilt += alpha * (raw - _bassFilt);

            return _bassFilt * 0.22 * env;
        }

        private double GenDrums(double beat)
        {
            double s = 0;
            double bf = beat - Math.Floor(beat);
            double bSec = bf * SecPerBeat;
            int beatInt = (int)Math.Floor(beat);

            if (bSec < 0.2)
            {
                double kEnv = Math.Exp(-bSec / 0.045);
                double kFreq = 220 * Math.Exp(-bSec / 0.012) + 38;
                _kickPhase += kFreq / SampleRate;
                if (_kickPhase >= 1)
                    _kickPhase -= 1;
                s += Math.Sin(_kickPhase * 2 * Math.PI) * 0.24 * kEnv;
                if (bSec < 0.004)
                    s += Noise() * 0.12 * (1 - bSec / 0.004);
            }

            if (beatInt % 2 == 1)
            {
                double snEnv = Math.Exp(-bSec / 0.06);
                _snrPhase += 185.0 / SampleRate;
                if (_snrPhase >= 1)
                    _snrPhase -= 1;
                s += Math.Sin(_snrPhase * 2 * Math.PI) * 0.10 * snEnv;
                s += Noise() * 0.14 * Math.Exp(-bSec / 0.1);
            }

            double hf = (beat * 2) - Math.Floor(beat * 2);
            double hhSec = hf * SecPerBeat / 2;
            s += Noise() * 0.03 * Math.Exp(-hhSec / 0.01);

            if (beat >= 32)
            {
                int barBeat = beatInt % 4;
                if (barBeat == 0 || barBeat == 2)
                    s += Noise() * 0.05 * Math.Exp(-bSec / 0.012);
            }

            return s;
        }

        private double GenChoir(double beat, double lfo)
        {
            int bar = Math.Min((int)(beat / 4.0), _choirLo.Length - 1);
            double barBeat = beat - bar * 4.0;
            double barFrac = barBeat / 4.0;
            double env = Math.Min(1.0, barFrac / 0.08) * (0.85 + 0.15 * (1.0 - barFrac));
            env *= 0.75 + 0.25 * Math.Sin(barBeat * SecPerBeat * 8 * Math.PI);

            double vibrato = 1.0 + lfo * 0.003;

            double freq0 = MidiHz(_choirLo[bar]);
            _v0Phase += freq0 * vibrato / SampleRate;
            if (_v0Phase >= 1)
                _v0Phase -= 1;
            _v0Phase2 += freq0 * vibrato * 1.005 / SampleRate;
            if (_v0Phase2 >= 1)
                _v0Phase2 -= 1;
            double v = (2 * _v0Phase - 1) + (2 * _v0Phase2 - 1);
            double s = Math.Tanh(v * 1.5) * 0.06 * env;

            double freq1 = MidiHz(_choirHi[bar]);
            _v1Phase += freq1 * vibrato / SampleRate;
            if (_v1Phase >= 1)
                _v1Phase -= 1;
            _v1Phase2 += freq1 * vibrato * 1.005 / SampleRate;
            if (_v1Phase2 >= 1)
                _v1Phase2 -= 1;
            double v2 = (2 * _v1Phase - 1) + (2 * _v1Phase2 - 1);
            s += Math.Tanh(v2 * 1.5) * 0.05 * env;

            return s;
        }

        private double GenOrgan(double beat)
        {
            int beatIdx = (int)beat;
            if (beatIdx < 0 || beatIdx >= _melody.Length)
                return 0;

            int note = _melody[beatIdx];
            if (note < 0)
                return 0;

            note -= 12;

            double frac = beat - beatIdx;
            double sec = frac * SecPerBeat;
            double env = Math.Min(1, sec / 0.15);

            double freq = MidiHz(note);
            _orgPhase += freq / SampleRate;
            if (_orgPhase >= 1)
                _orgPhase -= 1;

            double p = _orgPhase * 2 * Math.PI;
            double raw = Math.Sin(p) * 0.7
                       + Math.Sin(p * 2) * 1.0
                       + Math.Sin(p * 3) * 0.5
                       + Math.Sin(p * 4) * 0.6
                       + Math.Sin(p * 6) * 0.3
                       + Math.Sin(p * 8) * 0.2;
            raw /= 3.3;
            raw = Math.Tanh(raw * 3.5);

            double cutoff = 400 + 200 * env;
            double rc = 1.0 / (cutoff * 2 * Math.PI);
            double alpha = (1.0 / SampleRate) / (rc + 1.0 / SampleRate);
            _orgFilt += alpha * (raw - _orgFilt);

            return _orgFilt * 0.10 * env;
        }

        private double GenPizz(double beat)
        {
            int bar = Math.Min((int)(beat / 4.0), _pizzA.Length - 1);
            int barBeat = (int)(beat - bar * 4.0);
            double bf = beat - Math.Floor(beat);

            int pizzNote;
            if (barBeat == 0)
                pizzNote = _pizzA[bar];
            else if (barBeat == 2)
                pizzNote = _pizzB[bar];
            else
                return 0;

            if (pizzNote < 0)
                return 0;

            double sec = bf * SecPerBeat;
            double env = Env(sec, 0.0005, 0.03);
            double freq = MidiHz(pizzNote);

            _synthPhase += freq / SampleRate;
            if (_synthPhase >= 1)
                _synthPhase -= 1;
            _synth2Phase += (freq * 3.01) / SampleRate;
            if (_synth2Phase >= 1)
                _synth2Phase -= 1;

            double pulse = _synthPhase < 0.35 ? 1 : -1;
            double ring = Math.Sin(_synth2Phase * 2 * Math.PI);
            double raw = pulse * (0.6 + 0.4 * ring);
            return Math.Tanh(raw * 2) * 0.05 * env;
        }

        private double GenStrings(double beat, double lfo)
        {
            double env = Math.Min(1.0, beat / 4.0);
            if (beat > 56)
                env *= Math.Max(0, (TotalBeats - beat) / 4.0);

            if (env <= 0)
                return 0;

            int bar = Math.Min((int)(beat / 4.0), _bassR.Length - 1);
            double freq = MidiHz(_bassR[bar] + 12);
            double vib = 1 + lfo * 0.003;

            _strP0 += freq * vib * 0.998 / SampleRate;
            if (_strP0 >= 1)
                _strP0 -= 1;
            _strP1 += freq * vib * 1.002 / SampleRate;
            if (_strP1 >= 1)
                _strP1 -= 1;
            _strP2 += freq * vib * 0.995 / SampleRate;
            if (_strP2 >= 1)
                _strP2 -= 1;
            _strP3 += freq * vib * 1.005 / SampleRate;
            if (_strP3 >= 1)
                _strP3 -= 1;

            double raw = (2 * _strP0 - 1) + (2 * _strP1 - 1)
                       + (2 * _strP2 - 1) + (2 * _strP3 - 1);
            raw *= 0.25;

            double cutoff = 200 + 150 * lfo + 100 * env;
            double rc = 1.0 / (cutoff * 2 * Math.PI);
            double alpha = (1.0 / SampleRate) / (rc + 1.0 / SampleRate);
            _strFilt += alpha * (raw - _strFilt);

            return _strFilt * 0.15 * env;
        }

        private static double MidiHz(int midi)
        {
            return 440.0 * Math.Pow(2.0, (midi - 69) / 12.0);
        }

        private static double Env(double sec, double attack, double decay)
        {
            if (sec < attack)
                return sec / attack;
            return Math.Exp(-(sec - attack) / decay);
        }
    }
}
