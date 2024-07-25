using System;
using System.Drawing;
using System.Windows.Forms;

namespace WalkerSim.Viewer
{
    public partial class FormMain : Form
    {
        static readonly int ImageWidth = 768;
        static readonly int ImageHeight = 768;

        static readonly float WorldSizeX = 6000;
        static readonly float WorldSizeY = 6000;

        static readonly Vector3 WorldMins = new Vector3(-(WorldSizeX * 0.5f), -(WorldSizeY * 0.5f), 0);
        static readonly Vector3 WorldMaxs = new Vector3(WorldSizeX * 0.5f, WorldSizeY * 0.5f, 256);

        static WalkerSim.Config Config = new WalkerSim.Config()
        {
            MaxAgents = 6000,
        };

        Simulation simulation = Simulation.Instance;
        Random prng = new Random(1);
        Bitmap bitmap;
        Bitmap roadBitmap;
        System.Drawing.Graphics gr;

        Brush[] ColorTable;
        Brush[] PlayerColors;

        void GenerateColorTable()
        {
            var groupCount = Config.MaxAgents / Config.GroupSize;
            ColorTable = new Brush[groupCount];

            for (int i = 0; i < groupCount; i++)
            {
                var r = (byte)(i * 10 % 255);
                var g = (byte)(i * 20 % 255);
                var b = (byte)(i * 30 % 255);
                ColorTable[i] = new SolidBrush(System.Drawing.Color.FromArgb(r, g, b));
            }

            PlayerColors = new Brush[64];
            for (int i = 0; i < 64; i++)
            {
                PlayerColors[i] = new SolidBrush(System.Drawing.Color.Magenta);
            }
        }

        public FormMain()
        {
            InitializeComponent();

            bitmap = new Bitmap(ImageWidth, ImageHeight);
            gr = System.Drawing.Graphics.FromImage(bitmap);
            simCanvas.Image = bitmap;

            simulation.LoadMapData(@"G:\Steam\steamapps\common\7 Days To Die\Data\Worlds\Navezgane");
            simulation.Reset(WorldMins, WorldMaxs, Config);

            var addFakePlayers = false;
            if (addFakePlayers)
            {
                // Add a few fake players.
                var prng = new System.Random(1);
                for (var i = 0; i < 10; i++)
                {
                    // Select random position between world min and max.
                    var x = (float)(WorldMins.X + (WorldMaxs.X - WorldMins.X) * prng.NextDouble());
                    var y = (float)(WorldMins.Y + (WorldMaxs.Y - WorldMins.Y) * prng.NextDouble());
                    var z = 0f;

                    var maxViewDistance = 12; // GamePrefs.GetInt(EnumGamePrefs.ServerMaxAllowedViewDistance);
                    var viewRadius = (maxViewDistance * 16) / 2;

                    simulation.AddPlayer(i, new Vector3(x, y, z), viewRadius);
                }

            }

            GenerateColorTable();

            simTimer.Start();

            var warmup = false;
            if (warmup)
            {
                for (int i = 0; i < 3000; i++)
                {
                    simulation.Tick();
                }
            }

            simulation.Start();

            this.FormClosed += (sender, e) =>
            {
                simulation.Stop();
            };
        }

        private void OnTick(object sender, EventArgs e)
        {
            simulation.Update(simTimer.Interval / 1000.0f);

            // TODO: Add more UI controls for this.
            var fakeGunshots = true;
            if (fakeGunshots)
            {
                if (simulation.Events.Count < 3 && prng.NextDouble() < 0.01)
                {
                    var pos = new Vector3((float)(WorldMins.X + (WorldMaxs.X - WorldMins.X) * prng.NextDouble()),
                                          (float)(WorldMins.Y + (WorldMaxs.Y - WorldMins.Y) * prng.NextDouble()),
                                          0f);
                    var radius = 500.0f;

                    simulation.AddNoiseEvent(pos, radius, 2.0f);
                }
            }

            float updateRate = 1000f / simulation.GetAverageTickTime();
            this.Text = $"Ticks: {updateRate}/s";

            RenderSimulation();
        }

        Vector3 RemapPosition(Vector3 pos)
        {
            return simulation.RemapPosition2D(pos, Vector3.Zero, new Vector3(ImageWidth, ImageHeight));
        }

        private Bitmap GetCachedRoadsBitmap()
        {
            if (roadBitmap != null)
            {
                return roadBitmap;
            }

            var mapData = simulation.MapData;
            if (mapData == null)
                return null;

            var roads = mapData.Roads;
            if (roads == null)
                return null;

            roadBitmap = new Bitmap(roads.Width, roads.Height);

            using (var gr = System.Drawing.Graphics.FromImage(roadBitmap))
            {
                gr.Clear(System.Drawing.Color.Black);

                var brushRed = new SolidBrush(Color.FromArgb(32, 255, 0, 0));
                var brushGreen = new SolidBrush(Color.FromArgb(32, 0, 255, 0));

                for (int y = 0; y < roads.Height; y++)
                {
                    for (int x = 0; x < roads.Width; x++)
                    {
                        var roadType = roads.GetRoadType(x, y);
                        if (roadType == RoadType.None)
                            continue;

                        if (roadType == RoadType.Asphalt)
                        {
                            gr.FillRectangle(brushRed, x, y, 1, 1);
                        }
                        else
                        {
                            gr.FillRectangle(brushGreen, x, y, 1, 1);
                        }
                    }
                }
            }

            // Scale the bitmap to the simulation size.
            roadBitmap = new Bitmap(roadBitmap, ImageWidth, ImageHeight);

            return roadBitmap;
        }

        private void RenderToBitmap()
        {
            gr.Clear(System.Drawing.Color.Black);

            var roadsBitmap = GetCachedRoadsBitmap();
            if (roadsBitmap != null)
            {
                gr.DrawImage(roadsBitmap, 0, 0);
            }

            var agents = simulation.Agents;
            foreach (var agent in agents)
            {
                var imagePos = RemapPosition(agent.Position);

                var color = ColorTable[agent.Group % ColorTable.Length];
                gr.FillRectangle(color, imagePos.X, imagePos.Y, 1f, 1f);
            }

            var plyIdx = 0;
            var worldSize = simulation.WorldSize;
            foreach (var kv in simulation.Players)
            {
                var player = kv.Value;
                var imagePos = RemapPosition(player.Position);

                var color = PlayerColors[plyIdx++ % PlayerColors.Length];
                //gr.FillRectangle(color, imagePos.X, imagePos.Y, 1f, 1f);
                gr.FillEllipse(color, imagePos.X - 2, imagePos.Y - 2, 4f, 4f);

                var viewRadius = Math.Remap(player.ViewRadius, 0, worldSize.X, 0, ImageWidth);
                gr.DrawEllipse(Pens.Blue, imagePos.X - viewRadius, imagePos.Y - viewRadius, viewRadius * 2, viewRadius * 2);
            }

            foreach (var ev in simulation.Events)
            {
                var imagePos = RemapPosition(ev.Position);
                var radius = Math.Remap(ev.Radius, 0, worldSize.X, 0, ImageWidth);

                gr.DrawEllipse(Pens.Red, imagePos.X - radius, imagePos.Y - radius, radius * 2, radius * 2);
            }
        }

        private void RenderSimulation()
        {
            RenderToBitmap();

            simCanvas.Invalidate();
        }

        private void restartToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
    }
}
