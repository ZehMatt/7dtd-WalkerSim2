using System.IO;
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

            var pixel = Inner.GetPixel(x, y);
            return new WalkerSim.Drawing.Color(
                (byte)(pixel.r * 255),
                (byte)(pixel.g * 255),
                (byte)(pixel.b * 255),
                (byte)(pixel.a * 255)
            );
        }

        public void RemoveTransparency()
        {
            if (Inner == null)
            {
                return;
            }

            // Get all pixels
            Color[] pixels = Inner.GetPixels();

            // Set alpha to 255 for all pixels
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i].a = 1.0f;
            }

            // Apply changes
            Inner.SetPixels(pixels);
            Inner.Apply();
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
            Texture2D result = new Texture2D(newWidth, newHeight, TextureFormat.RGBA32, false);
            Color[] sourcePixels = source.GetPixels();
            Color[] resultPixels = new Color[newWidth * newHeight];

            float xRatio = (float)source.width / newWidth;
            float yRatio = (float)source.height / newHeight;

            for (int y = 0; y < newHeight; y++)
            {
                for (int x = 0; x < newWidth; x++)
                {
                    float srcX = x * xRatio;
                    float srcY = y * yRatio;
                    int xFloor = Mathf.FloorToInt(srcX);
                    int yFloor = Mathf.FloorToInt(srcY);
                    int xCeil = Mathf.Min(xFloor + 1, source.width - 1);
                    int yCeil = Mathf.Min(yFloor + 1, source.height - 1);

                    Color c00 = sourcePixels[yFloor * source.width + xFloor];
                    Color c10 = sourcePixels[yFloor * source.width + xCeil];
                    Color c01 = sourcePixels[yCeil * source.width + xFloor];
                    Color c11 = sourcePixels[yCeil * source.width + xCeil];

                    float xLerp = srcX - xFloor;
                    float yLerp = srcY - yFloor;

                    Color c0 = Color.Lerp(c00, c10, xLerp);
                    Color c1 = Color.Lerp(c01, c11, xLerp);
                    Color finalColor = Color.Lerp(c0, c1, yLerp);

                    resultPixels[y * newWidth + x] = finalColor;
                }
            }

            result.SetPixels(resultPixels);
            result.Apply();
            return result;
        }
    }
}
