using System.IO;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

namespace WalkerSim.Unity.Drawing
{
    internal class UnityDrawingImpl : WalkerSim.Drawing.IBitmap
    {
        public int Width => Inner?.width ?? 0;
        public int Height => Inner?.height ?? 0;

        public readonly Texture2D Inner;

        private NativeArray<byte> _cachedRaw;
        private bool _hasCached;

        public UnityDrawingImpl(Texture2D texture)
        {
            Inner = texture;
        }

        public void Dispose()
        {
            if (Inner != null)
            {
                Object.Destroy(Inner);
            }
        }

        public void LockPixels()
        {
            if (Inner != null && !_hasCached)
            {
                _cachedRaw = Inner.GetRawTextureData<byte>();
                _hasCached = true;
            }
        }

        public void UnlockPixels()
        {
            _hasCached = false;
        }

        public WalkerSim.Drawing.Color GetPixel(int x, int y)
        {
            if (Inner == null)
            {
                return WalkerSim.Drawing.Color.Transparent;
            }

            var raw = _hasCached ? _cachedRaw : Inner.GetRawTextureData<byte>();
            int yflip = Inner.height - 1 - y;
            switch (Inner.format)
            {
                case TextureFormat.RGB24:
                {
                    int idx = (yflip * Inner.width + x) * 3;
                    return new WalkerSim.Drawing.Color(raw[idx], raw[idx + 1], raw[idx + 2]);
                }
                case TextureFormat.BGRA32:
                {
                    int idx = (yflip * Inner.width + x) * 4;
                    return new WalkerSim.Drawing.Color(raw[idx + 2], raw[idx + 1], raw[idx], raw[idx + 3]);
                }
                default:
                {
                    int idx = (yflip * Inner.width + x) * 4;
                    return new WalkerSim.Drawing.Color(raw[idx + 1], raw[idx + 2], raw[idx + 3], raw[idx]);
                }
            }
        }

    }

    internal class UnityImageLoader : WalkerSim.Drawing.IImageLoader
    {
        public WalkerSim.Drawing.IBitmap LoadFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Debug.LogError($"File not found: {filePath}");
                return null;
            }

            // Read the image file as bytes
            byte[] fileData = File.ReadAllBytes(filePath);

            // Create a new Texture2D
            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false, true);
            if (texture.LoadImage(fileData))
            {
                return new UnityDrawingImpl(texture);
            }
            else
            {
                Debug.LogError($"Failed to load image data from {filePath}");
                Object.Destroy(texture);
                return null;
            }
        }

        public WalkerSim.Drawing.IBitmap CreateBitmap(WalkerSim.Drawing.IBitmap src, int width, int height)
        {
            if (src is UnityDrawingImpl unitySrc && unitySrc.Inner != null)
            {
                Texture2D resized = ResizeTexture(unitySrc.Inner, width, height);
                return new UnityDrawingImpl(resized);
            }
            return null;
        }

        private static Texture2D ResizeTexture(Texture2D source, int newWidth, int newHeight)
        {
            var result = new Texture2D(newWidth, newHeight, source.format, false, true);
            var srcData = source.GetRawTextureData<byte>();
            var dstData = result.GetRawTextureData<byte>();

            int srcW = source.width;
            int srcH = source.height;
            int bpp = source.format == TextureFormat.RGB24 ? 3 : 4;
            float xRatio = (float)srcW / newWidth;
            float yRatio = (float)srcH / newHeight;

            Parallel.For(0, newHeight, y =>
            {
                int sy = (int)(y * yRatio);
                if (sy >= srcH) sy = srcH - 1;
                int srcRow = sy * srcW * bpp;
                int dstRow = y * newWidth * bpp;
                for (int x = 0; x < newWidth; x++)
                {
                    int sx = (int)(x * xRatio);
                    if (sx >= srcW) sx = srcW - 1;
                    int s = srcRow + sx * bpp;
                    int d = dstRow + x * bpp;
                    for (int b = 0; b < bpp; b++)
                        dstData[d + b] = srcData[s + b];
                }
            });

            result.Apply(false);
            return result;
        }
    }
}
