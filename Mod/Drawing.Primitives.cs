using UnityEngine;

namespace WalkerSim.Drawing
{
    internal class Primitives
    {
        private static Material lineMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));

        public static void DrawBox(Rect rect, Color color)
        {
            lineMaterial.SetPass(0);
            GL.Begin(GL.QUADS);
            GL.Color(color);
            GL.Vertex3(rect.xMin, rect.yMin, 0);
            GL.Vertex3(rect.xMax, rect.yMin, 0);
            GL.Vertex3(rect.xMax, rect.yMax, 0);
            GL.Vertex3(rect.xMin, rect.yMax, 0);
            GL.End();
        }

        public static void DrawFilledCircle(Vector2 center, float radius, Color color)
        {
            int segments = 100;
            float angle = 0f;
            float increment = 2.0f * Mathf.PI / segments;

            lineMaterial.SetPass(0);
            GL.Begin(GL.TRIANGLES);
            GL.Color(color);

            for (int i = 0; i < segments; i++)
            {
                float x1 = center.x + Mathf.Cos(angle) * radius;
                float y1 = center.y + Mathf.Sin(angle) * radius;
                angle += increment;
                float x2 = center.x + Mathf.Cos(angle) * radius;
                float y2 = center.y + Mathf.Sin(angle) * radius;

                GL.Vertex3(center.x, center.y, 0);
                GL.Vertex3(x1, y1, 0);
                GL.Vertex3(x2, y2, 0);
            }

            GL.End();
        }

        public static void DrawCircle(Vector2 center, float radius, Color color)
        {
            int segments = 100;
            float angle = 0f;
            float increment = 2.0f * Mathf.PI / segments;

            lineMaterial.SetPass(0);
            GL.Begin(GL.LINES);
            GL.Color(color);

            for (int i = 0; i < segments; i++)
            {
                float x1 = center.x + Mathf.Cos(angle) * radius;
                float y1 = center.y + Mathf.Sin(angle) * radius;
                angle += increment;
                float x2 = center.x + Mathf.Cos(angle) * radius;
                float y2 = center.y + Mathf.Sin(angle) * radius;

                GL.Vertex3(x1, y1, 0);
                GL.Vertex3(x2, y2, 0);
            }

            GL.End();
        }

        public static void DrawLine(Vector2 start, Vector2 end, Color color)
        {
            lineMaterial.SetPass(0);
            GL.Begin(GL.LINES);
            GL.Color(color);
            GL.Vertex3(start.x, start.y, 0);
            GL.Vertex3(end.x, end.y, 0);
            GL.End();
        }

        public static void DrawPixel(Vector2 pos, Color color)
        {
            lineMaterial.SetPass(0);
            GL.Begin(GL.QUADS);
            GL.Color(color);
            GL.Vertex3(pos.x, pos.y, 0);
            GL.Vertex3(pos.x + 1, pos.y, 0);
            GL.Vertex3(pos.x + 1, pos.y + 1, 0);
            GL.Vertex3(pos.x, pos.y + 1, 0);
            GL.End();
        }
    }
}
