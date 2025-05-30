namespace WalkerSim
{
    public static partial class Drawing
    {
        public static IBitmap LoadFromFile(string filePath)
        {
            return Loader.LoadFromFile(filePath);
        }

        public static IBitmap Create(IBitmap bitmap, int width, int height)
        {
            return Loader.CreateBitmap(bitmap, width, height);
        }
    }

}
