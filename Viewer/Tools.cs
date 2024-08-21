using System.Drawing;
using System.Windows.Forms;

namespace WalkerSim.Viewer
{
    internal interface ITool
    {
        void OnClick(Vector3 position);

        void DrawPreview(PictureBox canvas, Graphics graphics, Vector3 position);
    }

    internal static class Tool
    {
        public static ITool Active;
    }

    internal class SoundEventTool : ITool
    {
        public float Radius = 700.0f;

        public void OnClick(Vector3 position)
        {
            var simulation = Simulation.Instance;

            simulation.AddSoundEvent(position, Radius);
        }

        public void DrawPreview(PictureBox canvas, Graphics graphics, Vector3 position)
        {
            var simulation = Simulation.Instance;
            var worldSize = simulation.WorldSize;

            var image = canvas.Image;
            var imagePos = simulation.RemapPosition2D(position, Vector3.Zero, new Vector3(image.Width, image.Height));
            var radius = Math.Remap(Radius, 0, worldSize.X, 0, image.Width);

            graphics.DrawEllipse(Pens.Red, imagePos.X - radius, imagePos.Y - radius, radius * 2, radius * 2);
        }
    }

    internal class KillTool : ITool
    {
        public float Radius = 350.0f;
        public float Decay = 1.0f;

        public void OnClick(Vector3 position)
        {
            var simulation = Simulation.Instance;

            var hitAgents = simulation.QueryCells(position, -1, Radius);
            foreach (var agent in hitAgents)
            {
                simulation.MarkAgentDead(agent);
            }
        }

        public void DrawPreview(PictureBox canvas, Graphics graphics, Vector3 position)
        {
            var simulation = Simulation.Instance;
            var worldSize = simulation.WorldSize;

            var image = canvas.Image;
            var imagePos = simulation.RemapPosition2D(position, Vector3.Zero, new Vector3(image.Width, image.Height));
            var radius = Math.Remap(Radius, 0, worldSize.X, 0, image.Width);

            graphics.DrawEllipse(Pens.Red, imagePos.X - radius, imagePos.Y - radius, radius * 2, radius * 2);
        }
    }
}
