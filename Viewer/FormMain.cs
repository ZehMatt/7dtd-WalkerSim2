using System;
using System.Drawing;
using System.Linq;
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

        static WalkerSim.Config CurrentConfig;

        Simulation simulation = Simulation.Instance;
        Random prng = new Random(1);
        Bitmap bitmap;
        Bitmap roadBitmap;
        System.Drawing.Graphics gr;

        Brush[] GroupColors;
        Brush[] PlayerColors;

        void GenerateColorTable()
        {
            var groupCount = CurrentConfig.MaxAgents / CurrentConfig.GroupSize;
            GroupColors = new Brush[groupCount];

            for (int i = 0; i < groupCount; i++)
            {
                GroupColors[i] = new SolidBrush(ColorTable.GetColorForIndex(i));
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

            this.FormClosed += (sender, e) =>
            {
                simulation.Stop();
            };

            SetupLimits();
            SetupChoices();
            LoadDefaultConfiguration();
            UpdateConfigFields();
            //StartSimulation();
        }

        private void SetupLimits()
        {
            inputMaxAgents.Minimum = Simulation.Limits.MinAgents;
            inputMaxAgents.Maximum = Simulation.Limits.MaxAgents;
            inputGroupSize.Minimum = 0;
            inputGroupSize.Maximum = Simulation.Limits.MaxAgents;
            inputRandomSeed.Minimum = 1;
            inputRandomSeed.Maximum = UInt32.MaxValue;
        }

        private void SetupChoices()
        {
            var startChoices = Enum.GetValues(typeof(Config.WorldLocation)).Cast<Config.WorldLocation>();
            foreach (var choice in startChoices)
            {
                if (choice == Config.WorldLocation.None)
                    continue;

                var name = Utils.GetWorldLocationString(choice);
                inputStartPosition.Items.Add(name);
            }

            foreach (var choice in startChoices)
            {
                var name = Utils.GetWorldLocationString(choice);
                inputRespawnPosition.Items.Add(name);
            }
        }

        private void LoadDefaultConfiguration()
        {
            CurrentConfig = Config.LoadFromFile("WalkerSim.xml");
        }

        private void UpdateConfigFields()
        {
            if (CurrentConfig == null)
                return;

            inputRandomSeed.Value = CurrentConfig.RandomSeed;
            inputMaxAgents.Value = CurrentConfig.MaxAgents;
            inputGroupSize.Value = CurrentConfig.GroupSize;
            inputStartGrouped.Checked = CurrentConfig.StartAgentsGrouped;
            inputPausePlayerless.Checked = CurrentConfig.PauseWithoutPlayers;
            inputPauseDuringBloodmoon.Checked = CurrentConfig.PauseDuringBloodmoon;

            var spawnChoice = Utils.GetWorldLocationString(CurrentConfig.StartPosition);
            inputStartPosition.SelectedItem = spawnChoice;

            var respawnChoice = Utils.GetWorldLocationString(CurrentConfig.RespawnPosition);
            inputRespawnPosition.SelectedItem = respawnChoice;

            listProcessorGroups.Items.Clear();

            var groups = CurrentConfig.Processors;
            for (int i = 0; i < groups.Length; i++)
            {
                listProcessorGroups.Items.Add("Group #" + i);
            }
        }

        private void StartSimulation()
        {
            CurrentConfig = Config.LoadFromFile("WalkerSim.xml");

            bitmap = new Bitmap(ImageWidth, ImageHeight);
            gr = System.Drawing.Graphics.FromImage(bitmap);
            simCanvas.Image = bitmap;

            // "G:\Steam\steamapps\common\7 Days To Die\Data\Worlds\Navezgane"
            // "C:\Users\Matt\AppData\Roaming\7DaysToDie\GeneratedWorlds\Ducufa Valley"
            simulation.LoadMapData(@"C:\Users\Matt\AppData\Roaming\7DaysToDie\GeneratedWorlds\Ducufa Valley");
            simulation.Reset(WorldMins, WorldMaxs, CurrentConfig);

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

            if (CurrentConfig.TicksToAdvanceOnStartup > 0)
            {
                simulation.FastAdvance(CurrentConfig.TicksToAdvanceOnStartup);
            }

            simulation.Start();
            updateTimer.Start();
        }

        private void OnTick(object sender, EventArgs e)
        {
            simulation.Update(updateTimer.Interval / 1000.0f);

            // TODO: Add more UI controls for this.
            var fakeGunshots = false;
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

        Vector3 SimPosToBitmapPos(Vector3 pos)
        {
            return simulation.RemapPosition2D(pos, Vector3.Zero, new Vector3(ImageWidth, ImageHeight));
        }

        Vector3 BitmapPosToSimPos(int x, int y)
        {
            var sim = Simulation.Instance;
            var worldMins = sim.WorldMins;
            var worldMaxs = sim.WorldMaxs;

            var newX = Math.Remap(x, 0, ImageWidth, worldMins.X, worldMaxs.X);
            var newY = Math.Remap(y, 0, ImageHeight, worldMins.Y, worldMaxs.Y);

            return new Vector3(newX, newY);
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

                var brushRed = new SolidBrush(Color.FromArgb(100, 255, 0, 0));
                var brushGreen = new SolidBrush(Color.FromArgb(100, 0, 255, 0));

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

            if (viewRoads.Checked)
            {
                var roadsBitmap = GetCachedRoadsBitmap();
                if (roadsBitmap != null)
                {
                    gr.DrawImage(roadsBitmap, 0, 0);
                }
            }

            if (viewAgents.Checked)
            {
                var agents = simulation.Agents;
                foreach (var agent in agents)
                {
                    var imagePos = SimPosToBitmapPos(agent.Position);

                    var color = GroupColors[agent.Group % GroupColors.Length];
                    gr.FillRectangle(color, imagePos.X, imagePos.Y, 1f, 1f);
                }
            }

            var plyIdx = 0;
            var worldSize = simulation.WorldSize;
            foreach (var kv in simulation.Players)
            {
                var player = kv.Value;
                var imagePos = SimPosToBitmapPos(player.Position);

                var color = PlayerColors[plyIdx++ % PlayerColors.Length];
                //gr.FillRectangle(color, imagePos.X, imagePos.Y, 1f, 1f);
                gr.FillEllipse(color, imagePos.X - 2, imagePos.Y - 2, 4f, 4f);

                var viewRadius = Math.Remap(player.ViewRadius, 0, worldSize.X, 0, ImageWidth);
                gr.DrawEllipse(Pens.Blue, imagePos.X - viewRadius, imagePos.Y - viewRadius, viewRadius * 2, viewRadius * 2);
            }

            if (viewEvents.Checked)
            {
                foreach (var ev in simulation.Events)
                {
                    var imagePos = SimPosToBitmapPos(ev.Position);
                    var radius = Math.Remap(ev.Radius, 0, worldSize.X, 0, ImageWidth);

                    gr.DrawEllipse(Pens.Red, imagePos.X - radius, imagePos.Y - radius, radius * 2, radius * 2);
                }
            }

            if (Tool.Active != null)
            {
                var mousePos = simCanvas.PointToClient(MousePosition);
                var imagePos = simCanvas.TranslateToImagePosition(mousePos);
                var simPos = BitmapPosToSimPos(imagePos.X, imagePos.Y);

                if (simPos.X < simulation.WorldMins.X)
                    return;
                if (simPos.Y < simulation.WorldMins.Y)
                    return;
                if (simPos.X > simulation.WorldMaxs.X)
                    return;
                if (simPos.Y > simulation.WorldMaxs.Y)
                    return;

                Tool.Active.DrawPreview(simCanvas, gr, simPos);
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

        private void simCanvas_MouseClick(object sender, MouseEventArgs e)
        {
            var simulation = Simulation.Instance;

            var imagePos = simCanvas.TranslateToImagePosition(e.Location);
            var simPos = BitmapPosToSimPos(imagePos.X, imagePos.Y);

            if (simPos.X < simulation.WorldMins.X)
                return;
            if (simPos.Y < simulation.WorldMins.Y)
                return;
            if (simPos.X > simulation.WorldMaxs.X)
                return;
            if (simPos.Y > simulation.WorldMaxs.Y)
                return;

            if (Tool.Active != null)
            {
                Tool.Active.OnClick(simPos);
            }

            Tool.Active = null;
            simCanvas.Cursor = Cursors.Default;
        }

        private void OnClickSoundEmit(object sender, EventArgs e)
        {
            Tool.Active = new SoundEventTool();
            simCanvas.Cursor = Cursors.Cross;
        }

        private void OnGroupSelection(object sender, EventArgs e)
        {
            var groupIdx = listProcessorGroups.SelectedIndex;

            groupProps.Visible = groupIdx != -1;
            groupProcessors.Visible = groupIdx != -1;
            groupParameter.Visible = false;

            if (groupIdx == -1)
            {
                return;
            }

            var selectedGroup = CurrentConfig.Processors[groupIdx];

            inputMovementGroup.Value = selectedGroup.Group;
            inputMovementSpeed.Value = (decimal)selectedGroup.SpeedScale;

            listProcessors.Items.Clear();
            foreach (var processor in selectedGroup.Entries)
            {
                listProcessors.Items.Add(processor.Type);
            }
        }

        private void OnProcessorSelectionChanged(object sender, EventArgs e)
        {
            var groupIdx = listProcessorGroups.SelectedIndex;
            if (groupIdx == -1)
            {
                return;
            }

            var processorIdx = listProcessors.SelectedIndex;
            groupParameter.Visible = processorIdx != -1;
            if (processorIdx == -1)
            {
                return;
            }

            var selectedGroup = CurrentConfig.Processors[groupIdx];
            var selectedProcessor = selectedGroup.Entries[processorIdx];

            inputProcessorDistance.Value = (decimal)selectedProcessor.Distance;
            inputProcessorPower.Value = (decimal)selectedProcessor.Power;
        }
    }
}
