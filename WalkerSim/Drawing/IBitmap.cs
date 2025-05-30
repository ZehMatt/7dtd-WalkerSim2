using System;

namespace WalkerSim
{
    public static partial class Drawing
    {
        public interface IBitmap : IDisposable
        {
            int Width { get; }

            int Height { get; }

            Color GetPixel(int x, int y);

            void RemoveTransparency();
        }
    }
}
