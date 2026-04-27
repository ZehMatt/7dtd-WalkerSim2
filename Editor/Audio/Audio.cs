using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Editor.Audio
{
    public sealed class Audio : IDisposable
    {
        private enum Backend { None, WinMM, CoreAudio, Alsa }

        #region Windows P/Invoke

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

        #endregion

        #region macOS P/Invoke

        private const uint kAudioFormatLinearPCM = 0x6C70636D; // 'lpcm'
        private const uint kAudioFormatFlagIsSignedInteger = 0x04;
        private const uint kAudioFormatFlagIsPacked = 0x08;
        private const uint kAudioQueueParam_Volume = 1;

        // AudioQueueBuffer field offsets on 64-bit macOS (arm64).
        // struct layout: uint32 mAudioDataBytesCapacity, [4 pad], void* mAudioData, uint32 mAudioDataByteSize
        private const int AQB_mAudioData = 8;
        private const int AQB_mAudioDataByteSize = 16;

        [StructLayout(LayoutKind.Sequential)]
        private struct AudioStreamBasicDescription
        {
            public double mSampleRate;
            public uint mFormatID;
            public uint mFormatFlags;
            public uint mBytesPerPacket;
            public uint mFramesPerPacket;
            public uint mBytesPerFrame;
            public uint mChannelsPerFrame;
            public uint mBitsPerChannel;
            public uint mReserved;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void AudioQueueOutputCallback(IntPtr userData, IntPtr queue, IntPtr buffer);

        [DllImport("/System/Library/Frameworks/AudioToolbox.framework/AudioToolbox")]
        private static extern int AudioQueueNewOutput(
            ref AudioStreamBasicDescription format,
            IntPtr callbackProc,
            IntPtr userData,
            IntPtr runLoop,
            IntPtr runLoopMode,
            uint flags,
            out IntPtr audioQueue);

        [DllImport("/System/Library/Frameworks/AudioToolbox.framework/AudioToolbox")]
        private static extern int AudioQueueAllocateBuffer(
            IntPtr audioQueue, uint bufferByteSize, out IntPtr bufferRef);

        [DllImport("/System/Library/Frameworks/AudioToolbox.framework/AudioToolbox")]
        private static extern int AudioQueueEnqueueBuffer(
            IntPtr audioQueue, IntPtr bufferRef, uint numPacketDescs, IntPtr packetDescs);

        [DllImport("/System/Library/Frameworks/AudioToolbox.framework/AudioToolbox")]
        private static extern int AudioQueueStart(IntPtr audioQueue, IntPtr startTime);

        [DllImport("/System/Library/Frameworks/AudioToolbox.framework/AudioToolbox")]
        private static extern int AudioQueueStop(IntPtr audioQueue, byte immediate);

        [DllImport("/System/Library/Frameworks/AudioToolbox.framework/AudioToolbox")]
        private static extern int AudioQueueDispose(IntPtr audioQueue, byte immediate);

        [DllImport("/System/Library/Frameworks/AudioToolbox.framework/AudioToolbox")]
        private static extern int AudioQueueSetParameter(IntPtr audioQueue, uint paramId, float value);

        #endregion

        #region Linux P/Invoke

        private const int SND_PCM_STREAM_PLAYBACK = 0;
        private const int SND_PCM_FORMAT_S16_LE = 2;
        private const int SND_PCM_ACCESS_RW_INTERLEAVED = 3;

        [DllImport("libasound.so.2")]
        private static extern int snd_pcm_open(out IntPtr pcm,
            [MarshalAs(UnmanagedType.LPUTF8Str)] string name, int stream, int mode);

        [DllImport("libasound.so.2")]
        private static extern int snd_pcm_set_params(IntPtr pcm, int format, int access,
            uint channels, uint rate, int softResample, uint latency);

        [DllImport("libasound.so.2")]
        private static extern nint snd_pcm_writei(IntPtr pcm, byte[] buffer, nuint size);

        [DllImport("libasound.so.2")]
        private static extern int snd_pcm_recover(IntPtr pcm, int err, int silent);

        [DllImport("libasound.so.2")]
        private static extern int snd_pcm_drop(IntPtr pcm);

        [DllImport("libasound.so.2")]
        private static extern int snd_pcm_close(IntPtr pcm);

        [DllImport("libasound.so.2")]
        private static extern int snd_device_name_hint(int card,
            [MarshalAs(UnmanagedType.LPUTF8Str)] string iface, out IntPtr hints);

        [DllImport("libasound.so.2")]
        private static extern IntPtr snd_device_name_get_hint(IntPtr hint,
            [MarshalAs(UnmanagedType.LPUTF8Str)] string id);

        [DllImport("libasound.so.2")]
        private static extern int snd_device_name_free_hint(IntPtr hints);

        [DllImport("libc.so.6", EntryPoint = "free")]
        private static extern void posix_free(IntPtr ptr);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void snd_lib_error_handler_t(
            IntPtr file, int line, IntPtr function, int err, IntPtr fmt);

        [DllImport("libasound.so.2")]
        private static extern int snd_lib_error_set_handler(IntPtr handler);

        private static snd_lib_error_handler_t _alsaNoOpErrorHandler;

        private static void AlsaSuppressErrors()
        {
            _alsaNoOpErrorHandler = (file, line, function, err, fmt) => { };
            snd_lib_error_set_handler(
                Marshal.GetFunctionPointerForDelegate(_alsaNoOpErrorHandler));
        }

        private static void AlsaRestoreErrors()
        {
            snd_lib_error_set_handler(IntPtr.Zero);
            _alsaNoOpErrorHandler = null;
        }

        private static System.Collections.Generic.List<string> EnumerateAlsaDevices()
        {
            var devices = new System.Collections.Generic.List<string>();
            if (snd_device_name_hint(-1, "pcm", out IntPtr hints) < 0)
                return devices;

            try
            {
                for (int i = 0; ; i++)
                {
                    IntPtr entry = Marshal.ReadIntPtr(hints, i * IntPtr.Size);
                    if (entry == IntPtr.Zero)
                        break;

                    IntPtr namePtr = snd_device_name_get_hint(entry, "NAME");
                    if (namePtr != IntPtr.Zero)
                    {
                        string name = Marshal.PtrToStringUTF8(namePtr);
                        posix_free(namePtr);
                        if (!string.IsNullOrEmpty(name) && name != "null")
                            devices.Add(name);
                    }
                }
            }
            finally
            {
                snd_device_name_free_hint(hints);
            }

            return devices;
        }

        #endregion

        // Format configuration.
        private readonly int _sampleRate;
        private readonly int _channels;
        private readonly int _bitsPerSample;
        private readonly int _bufferBytes;
        private readonly int _bufferFrames;
        private readonly int _numBuffers;

        // State.
        private Backend _backend;
        private bool _opened;
        private bool _disposed;

        // Windows state.
        private IntPtr _winHandle;
        private IntPtr[] _winHdrPtrs;
        private GCHandle[] _winPinned;
        private byte[][] _winBuffers;
        private int _winNextBuffer;
        private static readonly int WinHdrSize = Marshal.SizeOf<WAVEHDR>();
        private static readonly int WinFlagsOffset = IntPtr.Size + 4 + 4 + IntPtr.Size;

        // macOS state.
        private IntPtr _macQueue;
        private IntPtr[] _macBufferPtrs;
        private int[] _macAvailable;
        private int _macNextBuffer;
        private AudioQueueOutputCallback _macCallbackDelegate;

        // Linux state.
        private IntPtr _alsaPcm;

        public static bool IsSupported { get; } = ProbeSupport();

        private static bool ProbeSupport()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return true;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return true;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                try { return NativeLibrary.TryLoad("libasound.so.2", out _); }
                catch { return false; }
            }
            return false;
        }

        public Audio(int sampleRate, int channels, int bitsPerSample, int bufferSamples, int numBuffers)
        {
            _sampleRate = sampleRate;
            _channels = channels;
            _bitsPerSample = bitsPerSample;
            _bufferFrames = bufferSamples;
            _bufferBytes = bufferSamples * channels * (bitsPerSample / 8);
            _numBuffers = numBuffers;
        }

        public bool Open()
        {
            if (_opened || _disposed)
                return false;

            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    return OpenWinMM();
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    return OpenCoreAudio();
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    return OpenAlsa();
            }
            catch { }

            return false;
        }

        public void SetVolume(float volume)
        {
            if (!_opened)
                return;

            try
            {
                volume = Math.Clamp(volume, 0f, 1f);
                switch (_backend)
                {
                    case Backend.WinMM:
                        uint v = (uint)(volume * 0xFFFF);
                        waveOutSetVolume(_winHandle, v | (v << 16));
                        break;
                    case Backend.CoreAudio:
                        AudioQueueSetParameter(_macQueue, kAudioQueueParam_Volume, volume);
                        break;
                    case Backend.Alsa:
                        break;
                }
            }
            catch { }
        }

        public bool IsBufferAvailable()
        {
            if (!_opened)
                return false;

            try
            {
                switch (_backend)
                {
                    case Backend.WinMM:
                        return IsAvailableWinMM();
                    case Backend.CoreAudio:
                        return IsAvailableCoreAudio();
                    case Backend.Alsa:
                        return true;
                }
            }
            catch { }

            return false;
        }

        public bool SubmitBuffer(byte[] data, int length)
        {
            if (!_opened)
                return false;

            try
            {
                switch (_backend)
                {
                    case Backend.WinMM:
                        return SubmitWinMM(data, length);
                    case Backend.CoreAudio:
                        return SubmitCoreAudio(data, length);
                    case Backend.Alsa:
                        return SubmitAlsa(data, length);
                }
            }
            catch { }

            return false;
        }

        public void Reset()
        {
            if (!_opened)
                return;

            try
            {
                switch (_backend)
                {
                    case Backend.WinMM:
                        waveOutReset(_winHandle);
                        break;
                    case Backend.CoreAudio:
                        AudioQueueStop(_macQueue, 1);
                        break;
                    case Backend.Alsa:
                        snd_pcm_drop(_alsaPcm);
                        break;
                }
            }
            catch { }
        }

        public void Close()
        {
            if (!_opened)
                return;

            try
            {
                switch (_backend)
                {
                    case Backend.WinMM:
                        CloseWinMM();
                        break;
                    case Backend.CoreAudio:
                        CloseCoreAudio();
                        break;
                    case Backend.Alsa:
                        CloseAlsa();
                        break;
                }
            }
            catch { }

            _opened = false;
            _backend = Backend.None;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            Close();
        }

        #region Windows Implementation

        private bool OpenWinMM()
        {
            var fmt = new WAVEFORMATEX
            {
                wFormatTag = WAVE_FORMAT_PCM,
                nChannels = (ushort)_channels,
                nSamplesPerSec = (uint)_sampleRate,
                wBitsPerSample = (ushort)_bitsPerSample,
                nBlockAlign = (ushort)(_channels * (_bitsPerSample / 8)),
                nAvgBytesPerSec = (uint)(_sampleRate * _channels * (_bitsPerSample / 8)),
                cbSize = 0
            };

            if (waveOutOpen(out _winHandle, -1, ref fmt, IntPtr.Zero, IntPtr.Zero, CALLBACK_NULL) != 0)
                return false;

            _winBuffers = new byte[_numBuffers][];
            _winPinned = new GCHandle[_numBuffers];
            _winHdrPtrs = new IntPtr[_numBuffers];

            for (int i = 0; i < _numBuffers; i++)
            {
                _winBuffers[i] = new byte[_bufferBytes];
                _winPinned[i] = GCHandle.Alloc(_winBuffers[i], GCHandleType.Pinned);

                _winHdrPtrs[i] = Marshal.AllocHGlobal(WinHdrSize);
                var hdr = new WAVEHDR
                {
                    lpData = _winPinned[i].AddrOfPinnedObject(),
                    dwBufferLength = (uint)_bufferBytes,
                };
                Marshal.StructureToPtr(hdr, _winHdrPtrs[i], false);
                waveOutPrepareHeader(_winHandle, _winHdrPtrs[i], WinHdrSize);

                // Mark as done so buffers appear available for initial fill.
                int flags = Marshal.ReadInt32(_winHdrPtrs[i], WinFlagsOffset);
                Marshal.WriteInt32(_winHdrPtrs[i], WinFlagsOffset, flags | WHDR_DONE);
            }

            _winNextBuffer = 0;
            _backend = Backend.WinMM;
            _opened = true;
            return true;
        }

        private bool IsAvailableWinMM()
        {
            for (int i = 0; i < _numBuffers; i++)
            {
                uint flags = (uint)Marshal.ReadInt32(_winHdrPtrs[i], WinFlagsOffset);
                if ((flags & WHDR_DONE) != 0)
                    return true;
            }
            return false;
        }

        private bool SubmitWinMM(byte[] data, int length)
        {
            for (int i = 0; i < _numBuffers; i++)
            {
                int idx = (_winNextBuffer + i) % _numBuffers;
                uint flags = (uint)Marshal.ReadInt32(_winHdrPtrs[idx], WinFlagsOffset);
                if ((flags & WHDR_DONE) != 0)
                {
                    Buffer.BlockCopy(data, 0, _winBuffers[idx], 0, Math.Min(length, _bufferBytes));
                    waveOutWrite(_winHandle, _winHdrPtrs[idx], WinHdrSize);
                    _winNextBuffer = (idx + 1) % _numBuffers;
                    return true;
                }
            }
            return false;
        }

        private void CloseWinMM()
        {
            waveOutReset(_winHandle);

            for (int i = 0; i < _numBuffers; i++)
            {
                if (_winHdrPtrs[i] != IntPtr.Zero)
                {
                    uint flags = (uint)Marshal.ReadInt32(_winHdrPtrs[i], WinFlagsOffset);
                    if ((flags & WHDR_PREPARED) != 0)
                        waveOutUnprepareHeader(_winHandle, _winHdrPtrs[i], WinHdrSize);

                    Marshal.FreeHGlobal(_winHdrPtrs[i]);
                    _winHdrPtrs[i] = IntPtr.Zero;
                }
                if (_winPinned[i].IsAllocated)
                    _winPinned[i].Free();
            }

            waveOutClose(_winHandle);
            _winHandle = IntPtr.Zero;
        }

        #endregion

        #region macOS Implementation

        private bool OpenCoreAudio()
        {
            uint bytesPerFrame = (uint)(_channels * (_bitsPerSample / 8));
            var desc = new AudioStreamBasicDescription
            {
                mSampleRate = _sampleRate,
                mFormatID = kAudioFormatLinearPCM,
                mFormatFlags = kAudioFormatFlagIsSignedInteger | kAudioFormatFlagIsPacked,
                mBytesPerPacket = bytesPerFrame,
                mFramesPerPacket = 1,
                mBytesPerFrame = bytesPerFrame,
                mChannelsPerFrame = (uint)_channels,
                mBitsPerChannel = (uint)_bitsPerSample,
                mReserved = 0
            };

            _macCallbackDelegate = MacCallback;
            IntPtr callbackPtr = Marshal.GetFunctionPointerForDelegate(_macCallbackDelegate);

            if (AudioQueueNewOutput(ref desc, callbackPtr, IntPtr.Zero,
                    IntPtr.Zero, IntPtr.Zero, 0, out _macQueue) != 0)
                return false;

            _macBufferPtrs = new IntPtr[_numBuffers];
            _macAvailable = new int[_numBuffers];

            for (int i = 0; i < _numBuffers; i++)
            {
                if (AudioQueueAllocateBuffer(_macQueue, (uint)_bufferBytes, out _macBufferPtrs[i]) != 0)
                {
                    AudioQueueDispose(_macQueue, 1);
                    return false;
                }
                Volatile.Write(ref _macAvailable[i], 1);
            }

            _macNextBuffer = 0;

            if (AudioQueueStart(_macQueue, IntPtr.Zero) != 0)
            {
                AudioQueueDispose(_macQueue, 1);
                return false;
            }

            _backend = Backend.CoreAudio;
            _opened = true;
            return true;
        }

        private void MacCallback(IntPtr userData, IntPtr queue, IntPtr bufferRef)
        {
            if (_macBufferPtrs == null)
                return;

            for (int i = 0; i < _macBufferPtrs.Length; i++)
            {
                if (_macBufferPtrs[i] == bufferRef)
                {
                    Volatile.Write(ref _macAvailable[i], 1);
                    break;
                }
            }
        }

        private bool IsAvailableCoreAudio()
        {
            for (int i = 0; i < _numBuffers; i++)
            {
                if (Volatile.Read(ref _macAvailable[i]) != 0)
                    return true;
            }
            return false;
        }

        private bool SubmitCoreAudio(byte[] data, int length)
        {
            for (int i = 0; i < _numBuffers; i++)
            {
                int idx = (_macNextBuffer + i) % _numBuffers;
                if (Volatile.Read(ref _macAvailable[idx]) != 0)
                {
                    IntPtr bufPtr = _macBufferPtrs[idx];
                    IntPtr audioData = Marshal.ReadIntPtr(bufPtr, AQB_mAudioData);
                    int copyLen = Math.Min(length, _bufferBytes);
                    Marshal.Copy(data, 0, audioData, copyLen);
                    Marshal.WriteInt32(bufPtr, AQB_mAudioDataByteSize, copyLen);

                    Volatile.Write(ref _macAvailable[idx], 0);
                    AudioQueueEnqueueBuffer(_macQueue, bufPtr, 0, IntPtr.Zero);
                    _macNextBuffer = (idx + 1) % _numBuffers;
                    return true;
                }
            }
            return false;
        }

        private void CloseCoreAudio()
        {
            AudioQueueStop(_macQueue, 1);
            AudioQueueDispose(_macQueue, 1);
            _macQueue = IntPtr.Zero;
            _macBufferPtrs = null;
            _macAvailable = null;
            _macCallbackDelegate = null;
        }

        #endregion

        #region Linux Implementation

        private bool OpenAlsa()
        {
            uint latencyUs = (uint)((long)_bufferFrames * 1000000 / _sampleRate);
            var devices = EnumerateAlsaDevices();

            AlsaSuppressErrors();
            try
            {
                foreach (var device in devices)
                {
                    if (snd_pcm_open(out _alsaPcm, device, SND_PCM_STREAM_PLAYBACK, 0) < 0)
                        continue;

                    if (snd_pcm_set_params(_alsaPcm, SND_PCM_FORMAT_S16_LE, SND_PCM_ACCESS_RW_INTERLEAVED,
                            (uint)_channels, (uint)_sampleRate, 1, latencyUs) < 0)
                    {
                        snd_pcm_close(_alsaPcm);
                        _alsaPcm = IntPtr.Zero;
                        continue;
                    }

                    _backend = Backend.Alsa;
                    _opened = true;
                    return true;
                }

                return false;
            }
            finally
            {
                AlsaRestoreErrors();
            }
        }

        private bool SubmitAlsa(byte[] data, int length)
        {
            int bytesPerFrame = _channels * (_bitsPerSample / 8);
            nuint frames = (nuint)(length / bytesPerFrame);
            nint written = snd_pcm_writei(_alsaPcm, data, frames);

            if (written < 0)
            {
                snd_pcm_recover(_alsaPcm, (int)written, 1);
                written = snd_pcm_writei(_alsaPcm, data, frames);
            }

            return written >= 0;
        }

        private void CloseAlsa()
        {
            snd_pcm_drop(_alsaPcm);
            snd_pcm_close(_alsaPcm);
            _alsaPcm = IntPtr.Zero;
        }

        #endregion
    }
}
