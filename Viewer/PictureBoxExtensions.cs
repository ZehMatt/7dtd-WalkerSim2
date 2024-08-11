using System;
using System.Drawing;
using System.Windows.Forms;

namespace WalkerSim.Viewer
{
    internal static class PictureBoxExtensions
    {
        private static Point TranslateZoomMousePosition(System.Windows.Forms.PictureBox picture, Point coordinates)
        {
            //	test to make sure our image is not null
            if (picture.Image == null)
                return coordinates;

            //	Make sure our control width and height are not 0 and our image width and height are not 0
            if (picture.Width == 0 || picture.Height == 0 || picture.Image.Width == 0 || picture.Image.Height == 0)
                return coordinates;

            var image = picture.Image;

            //	This is the one that gets a little tricky.  Essentially, need to check the aspect ratio of the image to the aspect ratio of the control
            // to determine how it is being rendered
            float imageAspect = (float)image.Width / image.Height;
            float controlAspect = (float)picture.Width / picture.Height;
            float newX = coordinates.X;
            float newY = coordinates.Y;
            if (imageAspect > controlAspect)
            {
                //	This means that we are limited by width, meaning the image fills up the entire control from left to right
                float ratioWidth = (float)image.Width / picture.Width;
                newX *= ratioWidth;
                float scale = (float)picture.Width / image.Width;
                float displayHeight = scale * image.Height;
                float diffHeight = picture.Height - displayHeight;
                diffHeight /= 2;
                newY -= diffHeight;
                newY /= scale;
            }
            else
            {
                //	This means that we are limited by height, meaning the image fills up the entire control from top to bottom
                float ratioHeight = (float)image.Height / picture.Height;
                newY *= ratioHeight;
                float scale = (float)picture.Height / image.Height;
                float displayWidth = scale * image.Width;
                float diffWidth = picture.Width - displayWidth;
                diffWidth /= 2;
                newX -= diffWidth;
                newX /= scale;
            }
            return new Point((int)newX, (int)newY);
        }

        public static Point TranslateToImagePosition(this System.Windows.Forms.PictureBox picture, Point controlCoordinates)
        {
            switch (picture.SizeMode)
            {
                case PictureBoxSizeMode.Zoom:
                    return TranslateZoomMousePosition(picture, controlCoordinates);
            }
            throw new NotImplementedException("Implement me");
        }
    }
}
