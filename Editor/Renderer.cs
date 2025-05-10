using System.Drawing;

namespace WalkerSim.Editor
{
    internal static class Renderer
    {
        private static Bitmap _cachedRoads;
        private static string _cachedRoadsPath;
        private static Brush _activeAgentColor = new SolidBrush(Color.Green);

        private static Vector3 SimPosToBitmapPos(System.Drawing.Graphics gr, Simulation simulation, Vector3 pos)
        {
            var width = gr.VisibleClipBounds.Width;
            var height = gr.VisibleClipBounds.Height;

            return simulation.RemapPosition2D(pos, Vector3.Zero, new Vector3(width, height));
        }

        public static void RenderRoads(System.Drawing.Graphics gr, Simulation simulation)
        {
            var mapData = simulation.MapData;
            if (mapData == null)
                return;

            var roads = mapData.Roads;
            if (roads == null)
                return;

            Bitmap roadBitmap;
            if (_cachedRoads != null && _cachedRoadsPath == roads.Name)
            {
                roadBitmap = _cachedRoads;
            }
            else
            {
                if (_cachedRoads != null)
                {
                    _cachedRoads.Dispose();
                    _cachedRoads = null;
                }

                roadBitmap = new Bitmap(roads.Width, roads.Height);

                using (var gr2 = System.Drawing.Graphics.FromImage(roadBitmap))
                {
                    // Should probably be transparent.
                    gr2.Clear(System.Drawing.Color.Black);

                    var brushMain = new SolidBrush(Color.FromArgb(100, 255, 255, 255));
                    var brushOffroad = new SolidBrush(Color.FromArgb(50, 255, 255, 255));

                    for (int y = 0; y < roads.Height; y++)
                    {
                        for (int x = 0; x < roads.Width; x++)
                        {
                            var roadType = roads.GetRoadType(x, y);
                            if (roadType == RoadType.None)
                                continue;

                            if (roadType == RoadType.Asphalt)
                            {
                                gr2.FillRectangle(brushMain, x, y, 1, 1);
                            }
                            else
                            {
                                gr2.FillRectangle(brushOffroad, x, y, 1, 1);
                            }
                        }
                    }
                }

                _cachedRoads = roadBitmap;
                _cachedRoadsPath = roads.Name;
            }

            // Copy into dst, we take the width and height from dst
            var width = gr.VisibleClipBounds.Width;
            var height = gr.VisibleClipBounds.Height;

            gr.DrawImage(roadBitmap, 0, 0, width, height);

            return;
        }

        public static void RenderAgents(System.Drawing.Graphics gr, Simulation simulation, Brush[] groupColors)
        {
            var agents = simulation.Agents;
            foreach (var agent in agents)
            {
                if (agent.CurrentState != Agent.State.Wandering)
                    continue;

                var imagePos = SimPosToBitmapPos(gr, simulation, agent.Position);

                var color = groupColors[agent.Group % groupColors.Length];
                gr.FillRectangle(color, imagePos.X, imagePos.Y, 1f, 1f);
            }
        }
        public static void RenderActiveAgents(System.Drawing.Graphics gr, Simulation simulation, Brush[] groupColors)
        {
            var agents = simulation.Active;
            foreach (var kv in agents)
            {
                var agent = kv.Value;
                if (agent.CurrentState != Agent.State.Active)
                    continue;

                var imagePos = SimPosToBitmapPos(gr, simulation, agent.Position);

                gr.FillRectangle(_activeAgentColor, imagePos.X, imagePos.Y, 1f, 1f);
            }
        }

        public static void RenderPlayers(System.Drawing.Graphics gr, Simulation simulation, Brush[] playerColors)
        {
            var width = gr.VisibleClipBounds.Width;
            var height = gr.VisibleClipBounds.Height;

            var plyIdx = 0;
            var worldSize = simulation.WorldSize;
            foreach (var kv in simulation.Players)
            {
                var player = kv.Value;
                var imagePos = SimPosToBitmapPos(gr, simulation, player.Position);

                var color = playerColors[plyIdx++ % playerColors.Length];
                //gr.FillRectangle(color, imagePos.X, imagePos.Y, 1f, 1f);
                gr.FillEllipse(color, imagePos.X - 2, imagePos.Y - 2, 4f, 4f);

                var viewRadius = Math.Remap(player.ViewRadius, 0, worldSize.X, 0, width);
                gr.DrawEllipse(Pens.Blue, imagePos.X - viewRadius, imagePos.Y - viewRadius, viewRadius * 2, viewRadius * 2);
            }
        }
        public static void RenderEvents(System.Drawing.Graphics gr, Simulation simulation)
        {
            var width = gr.VisibleClipBounds.Width;
            var height = gr.VisibleClipBounds.Height;

            var worldSize = simulation.WorldSize;

            var events = simulation.Events;
            foreach (var ev in simulation.Events)
            {
                var imagePos = SimPosToBitmapPos(gr, simulation, ev.Position);
                var radius = Math.Remap(ev.Radius, 0, worldSize.X, 0, width);

                gr.DrawEllipse(Pens.Red, imagePos.X - radius, imagePos.Y - radius, radius * 2, radius * 2);
            }

        }

        public static void RenderPrefabs(System.Drawing.Graphics gr, Simulation simulation)
        {
            var width = gr.VisibleClipBounds.Width;
            var height = gr.VisibleClipBounds.Height;
            var worldSize = simulation.WorldSize;

            var mapData = simulation.MapData;
            if (mapData == null)
                return;

            var pois = mapData.Prefabs.Decorations;

            foreach (var poi in pois)
            {
                var imagePos = SimPosToBitmapPos(gr, simulation, poi.Position);

                var sizeW = Math.Remap(poi.Bounds.X, 0, worldSize.X, 0, width);
                var sizeH = Math.Remap(poi.Bounds.Y, 0, worldSize.Y, 0, height);

                // Rectangle
                var rect = new RectangleF(imagePos.X - sizeW / 2, imagePos.Y - sizeH / 2, sizeW, sizeH);

                // Draw the rectangle
                gr.DrawRectangle(Pens.Green, rect.X, rect.Y, rect.Width, rect.Height);
            }
        }
    }
}
