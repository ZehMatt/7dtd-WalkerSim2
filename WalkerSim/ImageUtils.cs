using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace WalkerSim
{
    internal static class ImageUtils
    {
        public static void RemoveTransparency(Bitmap img)
        {
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
}
