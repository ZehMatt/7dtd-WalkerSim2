using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace WalkerSim.Editor.Drawing
{
    internal class SystemDrawingImpl : WalkerSim.Drawing.IBitmap
    {
        public int Width => Inner?.Width ?? 0;

        public int Height => Inner?.Height ?? 0;

        public readonly System.Drawing.Bitmap Inner;

        public SystemDrawingImpl(Bitmap bitmap)
        {
            Inner = bitmap;
        }

        public void Dispose()
        {
            Inner?.Dispose();
        }

        public WalkerSim.Drawing.Color GetPixel(int x, int y)
        {
            var src = Inner.GetPixel(x, y);
            return new WalkerSim.Drawing.Color(
                src.R, src.G, src.B, src.A);
        }

        public void RemoveTransparency()
        {
            var img = Inner;
            var rect = new Rectangle(0, 0, img.Width, img.Height);
            var bmpData = img.LockBits(rect, ImageLockMode.ReadWrite, img.PixelFormat);

            int bytesPerPixel = Image.GetPixelFormatSize(img.PixelFormat) / 8;
            int byteCount = img.Width * img.Height * bytesPerPixel;
            byte[] pixels = new byte[byteCount];

            IntPtr ptrFirstPixel = bmpData.Scan0;
            Marshal.Copy(ptrFirstPixel, pixels, 0, pixels.Length);

            var height = img.Height;
            var width = img.Width;

            Parallel.For(0, height, y =>
            {
                int yOffset = y * bmpData.Stride;
                for (int x = 0; x < width; x++)
                {
                    int xOffset = x * bytesPerPixel;

                    // Set alpha channel to 255
                    if (bytesPerPixel == 3)
                    {
                        // For 24bpp RGB images
                        byte r = pixels[yOffset + xOffset];
                        byte g = pixels[yOffset + xOffset + 1];
                        byte b = pixels[yOffset + xOffset + 2];

                        pixels[yOffset + xOffset] = r;
                        pixels[yOffset + xOffset + 1] = g;
                        pixels[yOffset + xOffset + 2] = b;
                        // Alpha is ignored because the image is 24bpp
                    }
                    else if (bytesPerPixel == 4)
                    {
                        // For 32bpp RGBA images
                        pixels[yOffset + xOffset + 3] = 255; // Set alpha to 255
                    }
                }
            });

            Marshal.Copy(pixels, 0, ptrFirstPixel, pixels.Length);
            img.UnlockBits(bmpData);
        }
    }

    internal class ImageLoader : WalkerSim.Drawing.IImageLoader
    {
        public WalkerSim.Drawing.IBitmap CreateBitmap(WalkerSim.Drawing.IBitmap src, int width, int height)
        {
            var bitmapSrc = src as SystemDrawingImpl;
            return new SystemDrawingImpl(new System.Drawing.Bitmap(bitmapSrc.Inner, width, height));
        }

        public WalkerSim.Drawing.IBitmap LoadFromFile(string filePath)
        {
            var bitmapSrc = System.Drawing.Image.FromFile(filePath);

            return new SystemDrawingImpl((Bitmap)bitmapSrc);
        }
    }
}
