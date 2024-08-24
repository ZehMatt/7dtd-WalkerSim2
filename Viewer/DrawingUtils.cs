using System.Drawing;
using System.Drawing.Drawing2D;

namespace WalkerSim.Viewer
{
    internal static class DrawingUtils
    {
        private static Pen arrowPen = new Pen(Color.Green, 3);

        public static void DrawArrow(Graphics gr, Vector3 normalizedDir, PointF center, float length, float arrowHeadSize)
        {
            // Calculate half the length of the arrow shaft
            float halfLength = length / 2;

            // Calculate the start and end points of the arrow shaft
            var start = new PointF(
                center.X - normalizedDir.X * halfLength,
                center.Y - normalizedDir.Y * halfLength);

            var end = new PointF(
                center.X + normalizedDir.X * halfLength,
                center.Y + normalizedDir.Y * halfLength);

            // Draw the arrow shaft with the custom thick pen
            gr.DrawLine(arrowPen, start, end);

            // Calculate the angle of the arrow in radians
            var arrowAngle = System.Math.Atan2(normalizedDir.Y, normalizedDir.X);

            // Calculate the points for the arrowhead
            var arrowLeft = new PointF(
                end.X - arrowHeadSize * (float)System.Math.Cos(arrowAngle - System.Math.PI / 6),
                end.Y - arrowHeadSize * (float)System.Math.Sin(arrowAngle - System.Math.PI / 6));

            var arrowRight = new PointF(
                end.X - arrowHeadSize * (float)System.Math.Cos(arrowAngle + System.Math.PI / 6),
                end.Y - arrowHeadSize * (float)System.Math.Sin(arrowAngle + System.Math.PI / 6));

            // Create a polygon for the arrowhead
            using (GraphicsPath arrowHeadPath = new GraphicsPath())
            {
                arrowHeadPath.AddPolygon(new PointF[] { end, arrowLeft, arrowRight });
                gr.FillPath(Brushes.Green, arrowHeadPath);
                gr.DrawPath(arrowPen, arrowHeadPath); // Use the same thick pen for the arrowhead outline
            }
        }

    }
}
