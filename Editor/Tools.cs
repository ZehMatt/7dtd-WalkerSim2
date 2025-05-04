using System.Drawing;
using System.Windows.Forms;

namespace WalkerSim.Editor
{
    internal enum NextToolState
    {
        Keep,
        Stop,
    }

    internal interface ITool
    {
        NextToolState OnClick(Vector3 position);

        void DrawPreview(PictureBox canvas, Graphics graphics, Vector3 position);
    }

    internal static class Tool
    {
        public static ITool Active;
    }

    internal class SoundEventTool : ITool
    {
        public float Radius = 700.0f;

        public NextToolState OnClick(Vector3 position)
        {
            var simulation = Simulation.Instance;

            simulation.AddSoundEvent(position, Radius, 20.0f);

            return NextToolState.Keep;
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
        public float Radius = 650.0f;
        public float Decay = 1.0f;

        public NextToolState OnClick(Vector3 position)
        {
            var simulation = Simulation.Instance;

            var hitAgents = new FixedBufferList<Agent>(30000);
            simulation.QueryCells(position, -1, Radius, hitAgents);

            foreach (var agent in hitAgents)
            {
                simulation.MarkAgentDead(agent);
            }

            return NextToolState.Keep;
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

    internal class AddPlayerTool : ITool
    {
        public NextToolState OnClick(Vector3 position)
        {
            var simulation = Simulation.Instance;
            simulation.AddPlayer(0, position, 96, 0);

            return NextToolState.Stop;
        }

        public void DrawPreview(PictureBox canvas, Graphics graphics, Vector3 position)
        {
            var simulation = Simulation.Instance;
            var worldSize = simulation.WorldSize;

            var image = canvas.Image;
            var imagePos = simulation.RemapPosition2D(position, Vector3.Zero, new Vector3(image.Width, image.Height));
            var radius = Math.Remap(96, 0, worldSize.X, 0, image.Width);

            graphics.DrawEllipse(Pens.Red, imagePos.X - radius, imagePos.Y - radius, radius * 2, radius * 2);
        }
    }

    internal class SetPlayerPositionTool : ITool
    {
        public NextToolState OnClick(Vector3 position)
        {
            var simulation = Simulation.Instance;
            simulation.UpdatePlayer(0, position, true);

            return NextToolState.Stop;
        }
        public void DrawPreview(PictureBox canvas, Graphics graphics, Vector3 position)
        {
            var simulation = Simulation.Instance;
            var worldSize = simulation.WorldSize;

            var image = canvas.Image;
            var imagePos = simulation.RemapPosition2D(position, Vector3.Zero, new Vector3(image.Width, image.Height));
            var radius = Math.Remap(96, 0, worldSize.X, 0, image.Width);

            graphics.DrawEllipse(Pens.Red, imagePos.X - radius, imagePos.Y - radius, radius * 2, radius * 2);
        }
    }
}
