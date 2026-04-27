using SkiaSharp;
using System;

namespace Editor.Drawing
{
    internal class SkiaBitmapImpl : WalkerSim.Drawing.IBitmap
    {
        public SKBitmap Inner { get; private set; }

        public int Width => Inner?.Width ?? 0;
        public int Height => Inner?.Height ?? 0;

        public SkiaBitmapImpl(SKBitmap bitmap)
        {
            Inner = bitmap;
        }

        public void LockPixels()
        {
            // SkiaSharp pixels are always directly accessible; no explicit lock needed.
        }

        public void UnlockPixels()
        {
            // No-op for SkiaSharp.
        }

        public WalkerSim.Drawing.Color GetPixel(int x, int y)
        {
            var c = Inner.GetPixel(x, y);
            return new WalkerSim.Drawing.Color(c.Red, c.Green, c.Blue, c.Alpha);
        }

        public void Dispose()
        {
            Inner?.Dispose();
            Inner = null;
        }
    }

    internal class ImageLoader : WalkerSim.Drawing.IImageLoader
    {
        public WalkerSim.Drawing.IBitmap LoadFromFile(string filePath)
        {
            using var fileData = SKData.Create(filePath);
            if (fileData == null)
                throw new Exception($"Failed to read file: {filePath}");
            using var codec = SKCodec.Create(fileData);
            if (codec == null)
                throw new Exception($"Failed to create codec for image: {filePath}");

            var info = new SKImageInfo(codec.Info.Width, codec.Info.Height,
                SKColorType.Rgba8888, SKAlphaType.Unpremul);
            var bitmap = new SKBitmap(info);
            var result = codec.GetPixels(info, bitmap.GetPixels());
            if (result != SKCodecResult.Success && result != SKCodecResult.IncompleteInput)
                throw new Exception($"Failed to decode image: {filePath}, result: {result}");

            return new SkiaBitmapImpl(bitmap);
        }

        public WalkerSim.Drawing.IBitmap CreateBitmap(WalkerSim.Drawing.IBitmap src, int width, int height)
        {
            var srcImpl = (SkiaBitmapImpl)src;
            var info = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Unpremul);
            var sampling = new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear);
            var resized = srcImpl.Inner.Resize(info, sampling);
            return new SkiaBitmapImpl(resized);
        }
    }
}
