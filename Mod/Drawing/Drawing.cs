using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace WalkerSim.Unity.Drawing
{
    internal class UnityDrawingImpl : WalkerSim.Drawing.IBitmap
    {
        public int Width => Inner?.width ?? 0;
        public int Height => Inner?.height ?? 0;

        public readonly Texture2D Inner;

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
        }

        public void UnlockPixels()
        {
        }

        public WalkerSim.Drawing.Color GetPixel(int x, int y)
        {
            if (Inner == null)
            {
                return WalkerSim.Drawing.Color.Transparent;
            }

            // Unity's Texture2D uses a bottom-origin convention (y=0 is the
            // bottom row), while the rest of WalkerSim (editor/Skia, road and
            // biome map logic) uses top-origin (y=0 is the top row). Flip here
            // so the IBitmap abstraction is top-origin on both backends.
            var pixel = Inner.GetPixel(x, Inner.height - 1 - y);
            return new WalkerSim.Drawing.Color(
                (byte)(pixel.r * 255),
                (byte)(pixel.g * 255),
                (byte)(pixel.b * 255),
                (byte)(pixel.a * 255)
            );
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
            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
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
            var result = new Texture2D(newWidth, newHeight, TextureFormat.RGBA32, false);
            var srcData = source.GetRawTextureData<byte>();
            var dstData = result.GetRawTextureData<byte>();

            int srcW = source.width;
            int srcH = source.height;
            float xRatio = (float)srcW / newWidth;
            float yRatio = (float)srcH / newHeight;

            Parallel.For(0, newHeight, y =>
            {
                int sy = (int)(y * yRatio);
                if (sy >= srcH) sy = srcH - 1;
                int srcRow = sy * srcW * 4;
                int dstRow = y * newWidth * 4;
                for (int x = 0; x < newWidth; x++)
                {
                    int sx = (int)(x * xRatio);
                    if (sx >= srcW) sx = srcW - 1;
                    int s = srcRow + sx * 4;
                    int d = dstRow + x * 4;
                    dstData[d] = srcData[s];
                    dstData[d + 1] = srcData[s + 1];
                    dstData[d + 2] = srcData[s + 2];
                    dstData[d + 3] = srcData[s + 3];
                }
            });

            result.Apply(false);
            return result;
        }
    }
}
