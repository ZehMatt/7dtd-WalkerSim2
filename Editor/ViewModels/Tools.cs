using Avalonia;
using Avalonia.Media;
using Editor.Views;
using WalkerSim;

namespace Editor.ViewModels
{
    internal enum NextToolState
    {
        Keep,
        Stop,
    }

    internal interface ITool
    {
        NextToolState OnClick(Vector3 position);
        void DrawPreview(Views.SimulationCanvas canvas, DrawingContext context, Vector3 position);
    }

    internal static class Tool
    {
        public static ITool? Active { get; set; }
    }

    internal class SoundEventTool : ITool
    {
        public float Radius = 90.0f;

        public NextToolState OnClick(Vector3 position)
        {
            var simulation = Simulation.Instance;
            simulation.AddSoundEvent(position, Radius, 20.0f);
            return NextToolState.Keep;
        }

        public void DrawPreview(SimulationCanvas canvas, DrawingContext context, Vector3 position)
        {
            var simulation = Simulation.Instance;
            var worldSize = simulation.WorldSize;
            var bounds = canvas.Bounds;
            var width = bounds.Width;
            var height = bounds.Height;

            if (width <= 0 || height <= 0)
                return;

            // Convert simulation position to canvas pixel coordinates
            var mapped = simulation.RemapPosition2D(position, Vector3.Zero, new Vector3((float)width, (float)height));
            var cx = mapped.X;
            var cy = mapped.Y;

            // Remap radius from world units to canvas pixels
            var radiusPx = MathEx.Remap(Radius, 0, worldSize.X, 0, (float)width);

            // Draw red circle
            var pen = new Pen(Brushes.Red, 1.0 / canvas.Zoom);
            context.DrawEllipse(null, pen, new Point(cx, cy), radiusPx, radiusPx);
        }
    }

    internal class KillTool : ITool
    {
        public float Radius = 650.0f;

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

        public void DrawPreview(SimulationCanvas canvas, DrawingContext context, Vector3 position)
        {
            var simulation = Simulation.Instance;
            var worldSize = simulation.WorldSize;
            var bounds = canvas.Bounds;
            var width = bounds.Width;
            var height = bounds.Height;

            if (width <= 0 || height <= 0)
                return;

            var mapped = simulation.RemapPosition2D(position, Vector3.Zero, new Vector3((float)width, (float)height));
            var cx = mapped.X;
            var cy = mapped.Y;
            var radiusPx = MathEx.Remap(Radius, 0, worldSize.X, 0, (float)width);

            var pen = new Pen(Brushes.Red, 1.0 / canvas.Zoom);
            context.DrawEllipse(null, pen, new Point(cx, cy), radiusPx, radiusPx);
        }
    }

    internal class AddPlayerTool : ITool
    {
        public NextToolState OnClick(Vector3 position)
        {
            var simulation = Simulation.Instance;
            simulation.AddPlayer(0, position, 0);
            return NextToolState.Stop;
        }

        public void DrawPreview(SimulationCanvas canvas, DrawingContext context, Vector3 position)
        {
            var simulation = Simulation.Instance;
            var worldSize = simulation.WorldSize;
            var bounds = canvas.Bounds;
            var width = bounds.Width;
            var height = bounds.Height;

            if (width <= 0 || height <= 0)
                return;

            var mapped = simulation.RemapPosition2D(position, Vector3.Zero, new Vector3((float)width, (float)height));
            var cx = mapped.X;
            var cy = mapped.Y;
            var radiusPx = MathEx.Remap(96, 0, worldSize.X, 0, (float)width);

            var pen = new Pen(Brushes.Red, 1.0 / canvas.Zoom);
            context.DrawEllipse(null, pen, new Point(cx, cy), radiusPx, radiusPx);
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

        public void DrawPreview(SimulationCanvas canvas, DrawingContext context, Vector3 position)
        {
            var simulation = Simulation.Instance;
            var worldSize = simulation.WorldSize;
            var bounds = canvas.Bounds;
            var width = bounds.Width;
            var height = bounds.Height;

            if (width <= 0 || height <= 0)
                return;

            var mapped = simulation.RemapPosition2D(position, Vector3.Zero, new Vector3((float)width, (float)height));
            var cx = mapped.X;
            var cy = mapped.Y;
            var radiusPx = MathEx.Remap(96, 0, worldSize.X, 0, (float)width);

            var pen = new Pen(Brushes.Red, 1.0 / canvas.Zoom);
            context.DrawEllipse(null, pen, new Point(cx, cy), radiusPx, radiusPx);
        }
    }
}
