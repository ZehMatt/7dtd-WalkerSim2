using System;
using System.Threading;

namespace Editor
{
    public sealed class ChipSynth : IDisposable
    {
        private const int SampleRate = 44100;
        private const int NumBuffers = 2;
        private const int BufferSamples = 4096;
        private const int BufferBytes = BufferSamples * 2;

        private const double BPM = 70.0;
        private const double SecPerBeat = 60.0 / BPM;
        private const double SmpPerBeat = SecPerBeat * SampleRate;
        private const double IntroBeats = 4.0;
        private const double GrooveBeats = 256.0;
        private static readonly long TotalFirstSmp = (long)((IntroBeats + GrooveBeats) * SmpPerBeat);
        private static readonly long GrooveSmp = (long)(GrooveBeats * SmpPerBeat);

        private Audio _audio;
        private byte[] _pcmBuffer;
        private Thread _thread;
        private volatile bool _running;
        private bool _disposed;

        private double _riffP1, _riffP2;
        private double _riffFilt;
        private double _subP;
        private double _growlP1, _growlP2;
        private double _growlFilt, _growlAmp;
        private int _growlLast = -1;
        private double _impP1, _impP2;
        private double _impFilt, _impAmp;
        private int _impLast = -1;
        private double _kickP;
        private uint _ns = 12345;
        private double _lfoP, _driftP;
        private long _pos;

        private static readonly int[] CL = { 1117, 1313, 1523, 1733 };
        private readonly double[][] _cb = new double[4][];
        private readonly int[] _ci = new int[4];
        private readonly double[] _cf = new double[4];
        private static readonly int[] AL = { 173, 79 };
        private readonly double[][] _ab = new double[2][];
        private readonly int[] _ai = new int[2];
        private double _rlp; private bool _ri;
        private double[] _dly; private int _di;
        private static readonly int DL = (int)(0.341 * SampleRate);

        public double VisBass, VisLead, VisPerc;
        public int VisChord;
        public double VisLeadNote, VisChoir, VisFlute, VisBassNote, VisEnergy, VisPizz;

        // Bass riff
        private static readonly int[] _riffNotes =
        {
            46,-1, 46, 58, -1, 46,-1, 46, 58,-1, 46, 58, -1, 46,-1, 46,
            46,-1, 46, 58, -1, 46,-1, 58, 46,-1, 46, 58, -1, 46, 44, 42,

            44,-1, 44, 56, -1, 44,-1, 44, 56,-1, 44, 56, -1, 44,-1, 44,
            44,-1, 44, 56, -1, 44,-1, 56, 46,-1, 44, 56, -1, 46, 44, 42,

            46,-1, 46, 58, -1, 46,-1, 58, 46,-1, 46, 58, -1, 46, 44, 42,
            42,-1, 42, 54, -1, 42,-1, 54, 42,-1, 42, 54, -1, 44, 46,-1,

            46,-1, 46, 58, -1, 46, 44, 46, 58,-1, 46, 58, -1, 46, 44, 42,
            42,-1, 44,-1, 46,-1, 44,-1, 42,-1, 39,-1, 37,-1, 34, 46,
        };

        // Melody
        private static readonly int[] _growlMel =
        {
            -2,-2,-2,-2,-2,-2,-2,-2, -2,-2,-2,-2,-2,-2,-2,-2,
            -2,-2,-2,-2,-2,-2,-2,-2, -2,-2,-2,-2, 70, 60, 61, 65,
            70, 60, 61, 60, 61, 60, 61, 65, 70, 60, 61, 60, 61, 60, 61, 65,
            70, 60, 61, 60, 61, 60, 61, 65, 70, 60, 61, 60, 61, 60, 61, 65,

            70, 60, 61, 60, 61, 60, 61, 65, 70, 60, 61, 60, 61, 60, 61, 65,
            70, 60, 61, 60, 61, 60, 61, 65, 70, 60, 61, 60, 61, 65, 70, 65,
            68, 58, 59, 58, 59, 58, 59, 63, 68, 58, 59, 58, 59, 58, 59, 63,
            68, 58, 59, 58, 59, 58, 59, 63, 68, 63, 65, 63, 68, 70, 68, 65,

            70, 65, 61, 65, 70, 65, 61, 65, 70, 65, 61, 58, 61, 65, 70,-1,
            70, 60, 61, 60, 61, 60, 61, 65, 70, 61, 65, 70, 68, 70, 65, 61,
            68, 63, 59, 63, 68, 63, 59, 63, 68, 63, 59, 56, 59, 63, 68,-1,
            68, 58, 59, 58, 59, 58, 59, 63, 68, 59, 63, 68, 66, 68, 63, 59,

            70,-1,-1,-1, 61,-1,-1,-1, 65,-1,-1,-1, 70,-1, 68,-1,
            66,-1,-1,-1, 61,-1,-1,-1, 63,-1,-1,-1, 68,-1, 66,-1,
            70, 60, 61, 65, 70, 60, 61, 65, 68, 58, 59, 63, 68, 58, 59, 63,
            66, 56, 58, 63, 66, 56, 58, 63, 68, 58, 59, 63, 70, 60, 61, 65,

            70, 60, 61, 60, 61, 60, 61, 65, 70, 60, 61, 60, 61, 60, 61, 65,
            70, 60, 61, 60, 61, 60, 61, 65, 70, 65, 61, 65, 70, 68, 70, 65,
            68, 58, 59, 58, 59, 58, 59, 63, 68, 58, 59, 58, 59, 58, 59, 63,
            66, 56, 58, 56, 58, 56, 58, 63, 66, 63, 68, 63, 70, 68, 65, 63,

            70, 60, 61, 65, 68, 58, 59, 63, 70, 60, 61, 65, 68, 58, 59, 63,
            66, 56, 58, 63, 68, 58, 59, 63, 70, 60, 61, 65, 68, 70, 65, 61,
            70, 61, 65, 70, 68, 70, 65, 61, 68, 59, 63, 68, 66, 68, 63, 59,
            70, 68, 66, 68, 70, 68, 66, 65, 63, 61, 70, 68, 66, 63, 61, 58,

            70, 60, 61, 60, 61, 60, 61, 65, 70, 60, 61, 60, 61, 60, 61, 65,
            70, 60, 61, 60, 61, 60, 61, 65, 70, 60, 61, 60, 61, 65, 70, 75,
            68, 58, 59, 58, 59, 58, 59, 63, 68, 58, 59, 58, 59, 58, 59, 63,
            66, 56, 58, 56, 58, 56, 58, 63, 68, 63, 66, 68, 70,-1, 68,-1,

            70, 60, 61, 60, 61, 60, 61, 65, 70,-1,-1,-1, 65,-1,-1,-1,
            68, 58, 59, 58, 59, 58, 59, 63, 68,-1,-1,-1, 63,-1,-1,-1,
            70,-1,-1,-1, 65,-1,-1,-1, 61,-1,-1,-1, 58,-1,-1,-1,
            -2,-2,-2,-2,-2,-2,-2,-2, -2,-2,-2,-2,-2,-2,-2,-2,
        };

        // Chord stabs
        private static readonly int[] _impactMel =
        {
            58,-1,-1,-1,-1,-1,-1,-1, -1,-1,-1,-1,-1,-1,-1,-1,
            56,-1,-1,-1,-1,-1,-1,-1, -1,-1,-1,-1,-1,-1,-1,-1,
            58,-1,-1,-1,-1,-1,-1,-1, -1,-1,-1,-1,-1,-1,-1,-1,
            54,-1,-1,-1,-1,-1,-1,-1, 56,-1,-1,-1,-1,-1,-1,-1,

            -1, 58, 61, 65,-1,-1,-1,-1, -1, 58, 61, 65,-1,-1,-1,-1,
            -1, 56, 63, 68,-1,-1,-1,-1, -1, 56, 63, 68,-1,-1,-1,-1,
            -1, 58, 61, 65,-1,-1,-1,-1, -1, 58, 61, 65,-1,-1,-1,-1,
            -1, 54, 58, 66,-1,-1,-1,-1, -1, 56, 63, 68,-1,-1,-1,-1,

            -1, 58, 61, 65,-1,-1, 61,-1, -1, 58, 61, 65,-1, 65, 61,-1,
            -1, 56, 63, 68,-1,-1, 63,-1, -1, 56, 63, 68,-1, 68, 63,-1,
            -1, 58, 61, 65,-1,-1, 61,-1, -1, 58, 61, 65,-1, 65, 61,-1,
            -1, 54, 58, 66,-1,-1,-1,-1, -1, 56, 63, 68,-1,-1,-1,-1,

            58,-1,-1,-1,-1,-1,-1,-1, -1,-1,-1,-1,-1,-1,-1,-1,
            -1,-1,-1,-1,-1,-1,-1,-1, -1,-1,-1,-1,-1,-1,-1,-1,
            58,-1,-1,-1,-1,-1,-1,-1, -1,-1,-1,-1,-1,-1,-1,-1,
            -1,-1,-1,-1,-1,-1,-1,-1, 56,-1, 54,-1, 56,-1, 58,-1,

            -1, 58, 61, 65,-1,-1,-1,-1, -1, 56, 63, 68,-1,-1,-1,-1,
            -1, 58, 61, 65,-1,-1,-1,-1, -1, 54, 58, 66,-1,-1,-1,-1,
            -1, 58, 61, 65,-1,-1,-1,-1, -1, 56, 63, 68,-1,-1,-1,-1,
            -1, 54, 58, 66,-1, 56, 63,-1, -1, 58, 61, 65,-1,-1,-1,-1,

            58, 61, 65,-1, 58, 61, 65,-1, 56, 63, 68,-1, 56, 63, 68,-1,
            58, 61, 65,-1, 58, 61, 65,-1, 54, 58, 66,-1, 54, 58, 66,-1,
            58, 61, 65,-1, 58, 61, 65,-1, 56, 63, 68,-1, 56, 63, 68,-1,
            54, 58, 66,-1, 56, 63, 68,-1, 58, 61, 65,-1, 58,-1, 61,-1,

            -1, 58, 61, 65,-1,-1,-1,-1, -1, 58, 61, 65,-1,-1,-1,-1,
            -1, 56, 63, 68,-1,-1,-1,-1, -1, 56, 63, 68,-1,-1,-1,-1,
            -1, 58, 61, 65,-1,-1,-1,-1, -1, 54, 58, 66,-1,-1,-1,-1,
            -1, 56, 63, 68,-1,-1,-1,-1, -1, 58, 61, 65,-1,-1,-1,-1,

            58,-1,-1,-1,-1,-1,-1,-1, 56,-1,-1,-1,-1,-1,-1,-1,
            54,-1,-1,-1,-1,-1,-1,-1, 58,-1,-1,-1,-1,-1,-1,-1,
            58,-1,-1,-1,-1,-1,-1,-1, -1,-1,-1,-1,-1,-1,-1,-1,
            -1,-1,-1,-1,-1,-1,-1,-1, -1,-1,-1,-1,-1,-1,-1,-1,
        };

        public static bool IsSupported => Audio.IsSupported;
        public void Play()
        {
            if (!IsSupported || _audio != null)
                return;
            try
            {
                _audio = new Audio(SampleRate, 1, 16, BufferSamples, NumBuffers);
                if (!_audio.Open())
                {
                    _audio = null;
                    return;
                }
                _audio.SetVolume(0.4f);
                _pcmBuffer = new byte[BufferBytes];
                for (int i = 0; i < NumBuffers; i++)
                {
                    Fill();
                    _audio.SubmitBuffer(_pcmBuffer, BufferBytes);
                }
                _running = true;
                _thread = new Thread(Loop) { IsBackground = true, Name = "ChipSynth" };
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
                try
                {
                    _audio.Close();
                }
                catch { }
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
                    Fill();
                    _audio.SubmitBuffer(_pcmBuffer, BufferBytes);
                }
                Thread.Sleep(5);
            }
        }

        private void Fill()
        {
            if (!_ri)
            {
                for (int i = 0; i < 4; i++)
                    _cb[i] = new double[CL[i]];
                for (int i = 0; i < 2; i++)
                    _ab[i] = new double[AL[i]];
                _dly = new double[DL];
                _ri = true;
            }
            for (int i = 0; i < BufferSamples; i++)
            {
                double d = Gen();
                double dl = Dly(d, 0.28, 0.16);
                double w = Rvb(dl * 0.30);
                double s = Math.Tanh((d * 0.70 + w * 0.30) * 2.5);
                _pos++;
                short p = (short)(Math.Clamp(s, -1, 1) * 30000);
                _pcmBuffer[i * 2] = (byte)(p & 0xFF);
                _pcmBuffer[i * 2 + 1] = (byte)((p >> 8) & 0xFF);
            }
            UpdVis();
        }

        private double Dly(double i, double fb, double mx)
        {
            double d = _dly[_di];
            _dly[_di] = i + d * fb;
            _di = (_di + 1) % DL;
            return i * (1 - mx) + d * mx;
        }
        private double Rvb(double i)
        {
            double co = 0;
            for (int c = 0; c < 4; c++)
            {
                double d = _cb[c][_ci[c]];
                _cf[c] = d * 0.55 + _cf[c] * 0.45;
                _cb[c][_ci[c]] = i + _cf[c] * 0.75;
                _ci[c] = (_ci[c] + 1) % CL[c];
                co += d;
            }
            co *= 0.25;
            for (int a = 0; a < 2; a++)
            {
                double b = _ab[a][_ai[a]];
                double nv = co + 0.5 * b;
                co = b - 0.5 * nv;
                _ab[a][_ai[a]] = nv;
                _ai[a] = (_ai[a] + 1) % AL[a];
            }
            double la = (1.0 / SampleRate) / (1.0 / (2500 * 6.283) + 1.0 / SampleRate);
            _rlp += la * (co - _rlp);
            return _rlp;
        }
        private double Bt()
        {
            if (_pos < TotalFirstSmp)
                return _pos / SmpPerBeat;
            return IntroBeats + ((_pos - TotalFirstSmp) % GrooveSmp) / SmpPerBeat;
        }
        private bool IsBD(double beat)
        {
            double g = (beat - IntroBeats) % GrooveBeats;
            return g >= 96 && g < 112;
        }

        private double Gen()
        {
            double beat = Bt();
            _lfoP += 1.2 / SampleRate;
            if (_lfoP >= 1)
                _lfoP -= 1;
            _driftP += 0.4 / SampleRate;
            if (_driftP >= 1)
                _driftP -= 1;
            double lfo = Math.Sin(_lfoP * 6.283);
            bool bd = IsBD(beat);

            double s = 0;
            s += SubDrone(beat, lfo);
            s += Kick(beat, bd);
            if (beat >= 1)
                s += Impact(beat, bd);
            if (beat >= 2)
                s += BassRiff(beat, bd);
            if (beat >= 2)
                s += Drums(beat, bd);
            if (beat >= 3)
                s += Growl(beat, lfo);
            return s;
        }

        // Sub bass — Bb0 triangle drone
        private double SubDrone(double beat, double lfo)
        {
            double f = Hz(34); // Bb1
            _subP += f / SampleRate;
            if (_subP >= 1)
                _subP -= 1;
            double bf = beat - Math.Floor(beat);
            double pump = 0.6 + 0.4 * Math.Exp(-bf * SecPerBeat / 0.2);
            double tri = Math.Abs(4.0 * _subP - 2.0) - 1.0;
            return tri * 0.06 * pump;
        }

        // Bass — sustained sawtooth with slow LFO filter modulation
        private double BassRiff(double beat, bool bd)
        {
            double sx = beat * 4;
            int sp = (int)Math.Floor(sx);

            int note = _riffNotes[sp % _riffNotes.Length];
            if (note < 0)
                return 0;
            if (bd)
                return 0;

            double f = Hz(note);
            _riffP1 += f / SampleRate;
            if (_riffP1 >= 1)
                _riffP1 -= 1;

            // Sawtooth — fat sustained bass
            double raw = 2.0 * _riffP1 - 1.0;

            // LP filter — slow breathing, dark
            double bf = beat * 0.3;
            double mod = 0.5 + 0.5 * Math.Sin(bf * 6.283);
            double cut = Math.Min(250 + 500 * mod, SampleRate * 0.45);
            double fc = 2.0 * Math.Sin(Math.PI * cut / SampleRate);
            double q = 0.35;
            _riffFilt += fc * _riffP2;
            double hp = raw - _riffFilt - q * _riffP2;
            _riffP2 += fc * hp;

            return _riffFilt * 0.18;
        }

        // Arpeggio — clean square wave, continuous stream, minimal decay
        private double Growl(double beat, double lfo)
        {
            double gb = (beat - IntroBeats);
            if (gb < 0)
                gb = beat;
            int mi = (int)(gb * 2) % _growlMel.Length;
            int note = _growlMel[mi];

            if (note == -2)
            {
                _growlAmp *= 0.995;
                _growlLast = -1;
                return 0;
            }
            if (note >= 0 && note != _growlLast)
            {
                _growlAmp = 1.0;
                _growlLast = note;
            }
            if (note == -1 && _growlLast < 0)
                return 0;
            if (note == -1)
                note = _growlLast;

            double f = Hz(note);
            _growlP1 += f / SampleRate;
            if (_growlP1 >= 1)
                _growlP1 -= 1;

            // Triangle wave — muffled, dark
            double raw = Math.Abs(4.0 * _growlP1 - 2.0) - 1.0;

            // LP filter — low ceiling, slow LFO breathing
            double cut = Math.Min(600 + 800 * (0.5 + 0.5 * lfo), SampleRate * 0.45);
            double fc = 2.0 * Math.Sin(Math.PI * cut / SampleRate);
            double q = 0.30;
            _growlFilt += fc * _growlP2;
            double hp = raw - _growlFilt - q * _growlP2;
            _growlP2 += fc * hp;

            // Very slow decay — notes sustain into each other
            _growlAmp *= (1.0 - 0.3 / SampleRate);
            if (_growlAmp < 0.01)
                _growlAmp = 0;

            return _growlFilt * 0.14 * _growlAmp;
        }

        // Synth pad — warm sawtooth, slow attack/release
        private double Impact(double beat, bool bd)
        {
            double gb = (beat - IntroBeats);
            if (gb < 0)
                gb = beat;
            int mi = (int)(gb * 2) % _impactMel.Length;
            int note = _impactMel[mi];

            if (note >= 0)
            {
                _impAmp = 1.0;
                _impLast = note;
            }
            if (note < 0)
            {
                note = _impLast;
                if (note < 0)
                    return 0;
            }

            double env = _impAmp;
            if (bd)
                env *= 0.3;

            double f = Hz(note);
            _impP1 += f / SampleRate;
            if (_impP1 >= 1)
                _impP1 -= 1;

            // Sawtooth — dark pad
            double raw = 2.0 * _impP1 - 1.0;

            // LP filter — muffled, warm
            double cut = Math.Min(400 + 800 * _impAmp, SampleRate * 0.45);
            double fc = 2.0 * Math.Sin(Math.PI * cut / SampleRate);
            double q = 0.25;
            _impFilt += fc * _impP2;
            double hp = raw - _impFilt - q * _impP2;
            _impP2 += fc * hp;

            // Slow decay — chords linger
            _impAmp *= (1.0 - 0.5 / SampleRate);
            if (_impAmp < 0.01)
                _impAmp = 0;
            return _impFilt * 0.12 * env;
        }

        // Kick — deep, slow thump
        private double Kick(double beat, bool bd)
        {
            double bf = beat - Math.Floor(beat);
            double sec = bf * SecPerBeat;
            if (sec > 0.40)
                return 0;
            double vol = bd ? 0.05 : 0.14;
            double env = Math.Exp(-sec / 0.10);
            double f = 200 * Math.Exp(-sec / 0.015) + 30;
            _kickP += f / SampleRate;
            if (_kickP >= 1)
                _kickP -= 1;
            double tri = Math.Abs(4.0 * _kickP - 2.0) - 1.0;
            double s = tri * vol * env;
            if (sec < 0.004)
                s += Nz() * 0.06 * (1 - sec / 0.004);
            return s;
        }

        // Drums — subdued hats + muffled snare
        private double Drums(double beat, bool bd)
        {
            double sx = beat * 4;
            int sp = (int)Math.Floor(sx);
            double sf = sx - sp;
            double sec = sf * SecPerBeat / 4;
            double s = 0;
            double hv = bd ? 0.004 : 0.010;
            double acc = (sp % 4 == 0) ? 1.0 : ((sp % 2 == 0) ? 0.4 : 0.15);
            s += Nz() * hv * Math.Exp(-sec / 0.012) * acc;
            if (!bd && (sp % 16 == 4 || sp % 16 == 12))
            {
                double snEnv = Math.Exp(-sec / 0.06);
                s += Nz() * 0.07 * snEnv;
                s += Math.Sin(sec * 100 * 6.283) * 0.03 * Math.Exp(-sec / 0.025);
            }
            return s;
        }

        private void UpdVis()
        {
            double beat = Bt();
            bool bd = IsBD(beat);
            VisChord = bd ? 1 : 0;
            double gb = (beat - IntroBeats);
            if (gb < 0)
                gb = beat;

            int mi = (int)(gb * 2) % _growlMel.Length;
            int n = _growlMel[mi];
            if (n >= 0)
            {
                VisLead = Math.Max(VisLead * 0.9, Math.Exp(-(beat - (int)beat) * 2));
                VisLeadNote = (n - 58) / 12.0;
            }
            else
                VisLead *= 0.96;

            int sp = (int)Math.Floor(beat * 4);
            double sf = beat * 4 - sp;
            int rn = _riffNotes[sp % _riffNotes.Length];
            VisBass = (rn >= 0 && !bd) ? Math.Exp(-sf * 10) : VisBass * 0.9;
            VisBassNote = (rn >= 0) ? (rn - 46) / 20.0 : VisBassNote * 0.99;
            VisFlute = VisBass * 0.6;
            VisChoir = 0.8;

            double bf = beat - Math.Floor(beat);
            VisPerc = bd ? Math.Exp(-bf * 10) * 0.3 : ((int)beat % 2 == 1 ? Math.Exp(-bf * 5) : Math.Exp(-bf * 8) * 0.6);
            VisPizz = Math.Exp(-sf * 12);
            VisEnergy = VisBass * 0.3 + VisLead * 0.2 + VisChoir * 0.05 + VisFlute * 0.1 + VisPizz * 0.05 + VisPerc * 0.3;
        }

        private double Nz()
        {
            _ns = _ns * 1103515245 + 12345;
            return ((_ns >> 16) & 0xFFFF) / 65535.0 * 2 - 1;
        }
        private static double Hz(int m) => 440.0 * Math.Pow(2.0, (m - 69) / 12.0);
    }
}
