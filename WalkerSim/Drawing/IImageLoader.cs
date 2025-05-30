namespace WalkerSim
{
    public static partial class Drawing
    {
        public static IImageLoader Loader;

        public interface IImageLoader
        {
            IBitmap LoadFromFile(string filePath);

            IBitmap CreateBitmap(IBitmap src, int width, int height);
        }
    }

}
