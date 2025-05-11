using System.Drawing;
using System.Drawing.Drawing2D;

namespace WalkerSim.Editor
{
    internal static class DrawingUtils
    {
        private static Pen arrowPen = new Pen(Color.Green, 3);
        public static void DrawArrow(Graphics gr, Vector3 normalizedDir, PointF center, float length, float arrowHeadSize)
        {
            // Ensure normalizedDir is unit length
            float mag = (float)System.Math.Sqrt(normalizedDir.X * normalizedDir.X + normalizedDir.Y * normalizedDir.Y);
            Vector3 dir = mag > 0 ? new Vector3(normalizedDir.X / mag, normalizedDir.Y / mag, 0) : normalizedDir;

            // Calculate half the length of the arrow shaft
            float halfLength = length / 2;

            // Calculate the start and end points of the arrow shaft
            var start = new PointF(
                center.X - dir.X * halfLength,
                center.Y - dir.Y * halfLength);

            var end = new PointF(
                center.X + dir.X * halfLength,
                center.Y + dir.Y * halfLength);

            // Draw the arrow shaft
            gr.DrawLine(arrowPen, start, end);

            // Calculate the angle for the arrowhead
            float arrowAngle = (float)System.Math.Atan2(dir.Y, dir.X);

            // Calculate arrowhead points (30-degree angle for each side)
            float angleOffset = (float)(System.Math.PI / 6); // 30 degrees
            var arrowLeft = new PointF(
                end.X - arrowHeadSize * (float)System.Math.Cos(arrowAngle - angleOffset),
                end.Y - arrowHeadSize * (float)System.Math.Sin(arrowAngle - angleOffset));

            var arrowRight = new PointF(
                end.X - arrowHeadSize * (float)System.Math.Cos(arrowAngle + angleOffset),
                end.Y - arrowHeadSize * (float)System.Math.Sin(arrowAngle + angleOffset));

            // Draw the arrowhead as a filled polygon
            using (GraphicsPath arrowHeadPath = new GraphicsPath())
            {
                arrowHeadPath.AddPolygon(new PointF[] { end, arrowLeft, arrowRight });
                gr.FillPath(Brushes.Green, arrowHeadPath);
                gr.DrawPath(arrowPen, arrowHeadPath); // Outline with the same pen
            }
        }

    }
}
