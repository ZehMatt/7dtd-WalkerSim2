using System;
using System.Collections.Generic;
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

        static WalkerSim.Config CurrentConfig = Config.GetDefault();

        private int _selectedGroup = -1;
        private int _selectedProcessor = -1;
        private bool _updatingConfig = false;

        private List<Config.WorldLocation> _startPositions = new List<Config.WorldLocation>();
        private List<Config.WorldLocation> _respawnPositions = new List<Config.WorldLocation>();

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

            SetupLogging();
            SetupConfigChangeHandlers();
            SetupSpeedModifiers();
            SetupDrawingContext();
            SetupWorlds();
            SetupLimits();
            SetupChoices();
            LoadDefaultConfiguration();
            ScrollWheelHack();
        }

        private void LogMsg(string text, Color color, bool addNewLine = true)
        {
            rtbLog.SuspendLayout();
            rtbLog.SelectionColor = color;
            rtbLog.AppendText(addNewLine
                ? $"{text}{Environment.NewLine}"
                : text);
            rtbLog.ScrollToCaret();
            rtbLog.ResumeLayout();
        }

        private void LogInfo(string msg)
        {
            LogMsg(msg, Color.Black, true);
        }

        private void LogWrn(string msg)
        {
            LogMsg(msg, Color.Goldenrod, true);
            tabSimulation.SelectTab(2); // Switch to log.
        }

        private void LogErr(string msg)
        {
            LogMsg(msg, Color.DarkRed, true);
            tabSimulation.SelectTab(2); // Switch to log.
        }

        private void SetupLogging()
        {
            Logging.Prefixes = Logging.Prefix.Level | Logging.Prefix.Timestamp;

            Logging.SetHandler(Logging.Level.Info, LogInfo);
            Logging.SetHandler(Logging.Level.Warning, LogWrn);
            Logging.SetHandler(Logging.Level.Error, LogErr);

            Logging.Info("Initialized logging.");
        }

        // Fixes the increment being wrong when using the mouse wheel.
        private void ScrollHandlerFunction(object sender, MouseEventArgs e)
        {
            NumericUpDown control = (NumericUpDown)sender;
            ((HandledMouseEventArgs)e).Handled = true;
            decimal value = control.Value + ((e.Delta > 0) ? control.Increment : -control.Increment);

            // More ugly hacks to keep the caret at the same position.
            var txt = control.Controls[1] as TextBox;
            var sel = txt.SelectionStart;
            var selLen = txt.SelectionLength;
            control.Value = System.Math.Max(control.Minimum, System.Math.Min(value, control.Maximum));
            txt.SelectionStart = sel;
            txt.SelectionLength = selLen;
        }

        private void ScrollWheelHack()
        {
            inputMaxAgents.MouseWheel += ScrollHandlerFunction;
            inputRandomSeed.MouseWheel += ScrollHandlerFunction;
            inputGroupSize.MouseWheel += ScrollHandlerFunction;
            inputProcessorDistance.MouseWheel += ScrollHandlerFunction;
            inputProcessorPower.MouseWheel += ScrollHandlerFunction;
            inputMovementGroup.MouseWheel += ScrollHandlerFunction;
            inputMovementSpeed.MouseWheel += ScrollHandlerFunction;
        }

        private void SetupSpeedModifiers()
        {
            speedToolStripMenuItem.DropDownItems.Clear();

            int speed = 1;
            for (int i = 0; i < 9; i++)
            {
                var item = speedToolStripMenuItem.DropDownItems.Add(String.Format("{0}x", speed)) as ToolStripMenuItem;
                item.Checked = i == 0;

                var newSpeed = speed;
                item.Click += (sender, arg) =>
                {
                    foreach (ToolStripMenuItem other in speedToolStripMenuItem.DropDownItems)
                    {
                        other.Checked = false;
                    }
                    simulation.TimeScale = (float)newSpeed;
                    item.Checked = true;
                };
                speed <<= 1;
            }
        }

        private void SetupDrawingContext()
        {
            bitmap = new Bitmap(ImageWidth, ImageHeight);
            gr = System.Drawing.Graphics.FromImage(bitmap);
            simCanvas.Image = bitmap;
        }

        private void SetupWorlds()
        {
            Worlds.FindWorlds();

            inputWorld.Items.Clear();
            foreach (var worldPath in Worlds.WorldFolders)
            {
                var folderName = System.IO.Path.GetFileName(worldPath);
                inputWorld.Items.Add(folderName);
            }
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

                _startPositions.Add(choice);
            }

            foreach (var choice in startChoices)
            {
                var name = Utils.GetWorldLocationString(choice);
                inputRespawnPosition.Items.Add(name);

                _respawnPositions.Add(choice);
            }
        }

        private void LoadDefaultConfiguration()
        {
            LoadConfiguration("WalkerSim.xml");
        }

        private void LoadConfiguration(string file)
        {
            var loadedConfig = Config.LoadFromFile(file);
            if (loadedConfig == null)
            {
                MessageBox.Show("Failed to load the configuration file", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            CurrentConfig = loadedConfig;
            UpdateConfigFields();
        }

        private void SetConfigValues()
        {
            if (CurrentConfig == null || _updatingConfig)
            {
                return;
            }
            CurrentConfig.RandomSeed = (int)inputRandomSeed.Value;
            CurrentConfig.MaxAgents = (int)inputMaxAgents.Value;
            CurrentConfig.GroupSize = (int)inputGroupSize.Value;
            CurrentConfig.StartAgentsGrouped = inputStartGrouped.Checked;
            CurrentConfig.PauseDuringBloodmoon = inputPauseDuringBloodmoon.Checked;
            CurrentConfig.PauseWithoutPlayers = inputPausePlayerless.Checked;

            var startChoice = inputStartPosition.SelectedIndex;
            if (startChoice != -1)
            {
                CurrentConfig.StartPosition = _startPositions[startChoice];
            }

            var respawnChoice = inputRespawnPosition.SelectedIndex;
            if (respawnChoice != -1)
            {
                CurrentConfig.RespawnPosition = _respawnPositions[respawnChoice];
            }
        }

        private void SetupConfigChangeHandlers()
        {
            inputRandomSeed.ValueChanged += (sender, arg) => SetConfigValues();
            inputMaxAgents.ValueChanged += (sender, arg) => SetConfigValues();
            inputGroupSize.ValueChanged += (sender, arg) => SetConfigValues();
            inputStartGrouped.CheckedChanged += (sender, arg) => SetConfigValues();
            inputPauseDuringBloodmoon.CheckedChanged += (sender, arg) => SetConfigValues();
            inputPausePlayerless.CheckedChanged += (sender, arg) => SetConfigValues();
            inputStartPosition.SelectedIndexChanged += (sender, arg) => SetConfigValues();
            inputRespawnPosition.SelectedIndexChanged += (sender, arg) => SetConfigValues();
        }

        private void UpdateConfigFields()
        {
            if (CurrentConfig == null)
                return;

            _updatingConfig = true;

            reduceCPULoadToolStripMenuItem.Checked = CurrentConfig.ReduceCPULoad;

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
            for (int i = 0; i < groups.Count; i++)
            {
                listProcessorGroups.Items.Add("Group #" + i);
            }

            _updatingConfig = false;
        }

        private void StartSimulation()
        {
            simulation.SetWorldSize(WorldMins, WorldMaxs);
            simulation.Reset(CurrentConfig);

            GenerateColorTable();

            if (CurrentConfig.TicksToAdvanceOnStartup > 0)
            {
                simulation.FastAdvance(CurrentConfig.TicksToAdvanceOnStartup);
            }

            simulation.Start();
            updateTimer.Start();
        }

        private void StopSimulation()
        {
            simulation.Stop();
            updateTimer.Stop();
        }

        private void OnTick(object sender, EventArgs e)
        {
            simulation.Update(updateTimer.Interval / 1000.0f);

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

        private void ClearRoadBitmap()
        {
            if (roadBitmap == null)
            {
                return;
            }
            roadBitmap.Dispose();
            roadBitmap = null;
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
                    if (agent.CurrentState != Agent.State.Wandering)
                        continue;

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

        private void OnClickSoundEmit(object sender, EventArgs e)
        {
            Tool.Active = new SoundEventTool();
            simCanvas.Cursor = Cursors.Cross;
        }
        private void OnClickKill(object sender, EventArgs e)
        {
            Tool.Active = new KillTool();
            simCanvas.Cursor = Cursors.Cross;
        }

        private void OnGroupSelection(object sender, EventArgs e)
        {
            var groupIdx = listProcessorGroups.SelectedIndex;
            if (_selectedGroup == groupIdx)
            {
                return;
            }
            _selectedGroup = groupIdx;

            buttonRemoveGroup.Enabled = groupIdx != -1;
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

            buttonRemoveProcessor.Enabled = false;
        }

        private void OnProcessorSelectionChanged(object sender, EventArgs e)
        {
            var groupIdx = listProcessorGroups.SelectedIndex;
            if (groupIdx == -1)
            {
                return;
            }

            var processorIdx = listProcessors.SelectedIndex;
            if (processorIdx == _selectedProcessor)
            {
                return;
            }
            _selectedProcessor = -1;

            groupParameter.Visible = processorIdx != -1;
            buttonRemoveProcessor.Enabled = processorIdx != -1;

            if (processorIdx == -1)
            {
                return;
            }

            var selectedGroup = CurrentConfig.Processors[groupIdx];
            var selectedProcessor = selectedGroup.Entries[processorIdx];

            inputProcessorDistance.Value = (decimal)selectedProcessor.Distance;
            inputProcessorPower.Value = (decimal)selectedProcessor.Power;
        }

        private void OnPauseClick(object sender, EventArgs e)
        {
            simulation.SetPaused(true);
            pauseToolStripMenuItem.Enabled = false;
            resumeToolStripMenuItem.Enabled = true;
            advanceOneTickToolStripMenuItem.Enabled = true;
        }

        private void OnResumeClick(object sender, EventArgs e)
        {
            simulation.SetPaused(false);
            pauseToolStripMenuItem.Enabled = true;
            resumeToolStripMenuItem.Enabled = false;
            advanceOneTickToolStripMenuItem.Enabled = false;
        }

        private void OnRestartClick(object sender, EventArgs e)
        {
            inputRandomSeed.Enabled = false;
            inputMaxAgents.Enabled = false;
            inputGroupSize.Enabled = false;
            inputStartGrouped.Enabled = false;
            pauseToolStripMenuItem.Enabled = true;
            startToolStripMenuItem.Enabled = false;
            stopToolStripMenuItem.Enabled = true;

            StartSimulation();
        }

        private void OnStopClick(object sender, EventArgs e)
        {
            StopSimulation();

            inputRandomSeed.Enabled = true;
            inputMaxAgents.Enabled = true;
            inputGroupSize.Enabled = true;
            inputStartGrouped.Enabled = true;
            pauseToolStripMenuItem.Enabled = false;
            resumeToolStripMenuItem.Enabled = false;
            stopToolStripMenuItem.Enabled = false;
            startToolStripMenuItem.Enabled = true;
            advanceOneTickToolStripMenuItem.Enabled = false;
        }

        private void OnWorldSelectionChanged(object sender, EventArgs e)
        {
            var worldIdx = inputWorld.SelectedIndex;
            if (worldIdx == -1)
            {
                return;
            }

            var worldPath = Worlds.WorldFolders[worldIdx];
            simulation.LoadMapData(worldPath);

            ClearRoadBitmap();
            RenderSimulation();
        }

        private void OnSimCanvasClick(object sender, MouseEventArgs e)
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
        }

        private void OnReduceCPULoadClick(object sender, EventArgs e)
        {
            CurrentConfig.ReduceCPULoad = !CurrentConfig.ReduceCPULoad;
            reduceCPULoadToolStripMenuItem.Checked = CurrentConfig.ReduceCPULoad;
        }

        private void loadConfigurationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var browseFileDlg = new OpenFileDialog();
            browseFileDlg.Filter = "Config File (WalkerSim.xml)|WalkerSim.xml|All files (*.*)|*.*";
            browseFileDlg.RestoreDirectory = true;
            browseFileDlg.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
            browseFileDlg.Title = "Load configuration";
            if (browseFileDlg.ShowDialog() == DialogResult.OK)
            {
                LoadConfiguration(browseFileDlg.FileName);
            }
        }

        private void OnAddGroupClick(object sender, EventArgs e)
        {
            if (CurrentConfig == null)
                return;

            var groups = CurrentConfig.Processors;
            var newGroup = new Config.MovementProcessors();
            var idx = groups.Count;
            groups.Add(newGroup);

            listProcessorGroups.Items.Add("Group #" + idx);

            simulation.ReloadConfig(CurrentConfig);
        }

        private void OnRemoveGroupClick(object sender, EventArgs e)
        {
            if (CurrentConfig == null)
                return;

            var groupIdx = listProcessorGroups.SelectedIndex;
            if (groupIdx == -1)
            {
                return;
            }

            var groups = CurrentConfig.Processors;
            groups.RemoveAt(groupIdx);
            listProcessorGroups.Items.RemoveAt(groupIdx);

            if (groupIdx > 0)
            {
                listProcessorGroups.SelectedIndex = groupIdx - 1;
            }

            simulation.ReloadConfig(CurrentConfig);
        }

        private Config.MovementProcessors GetSelectedGroupEntry()
        {
            var groupIdx = listProcessorGroups.SelectedIndex;
            if (groupIdx == -1)
            {
                return null;
            }

            var groups = CurrentConfig.Processors;

            return groups[groupIdx];
        }

        private void OnGroupIdChanged(object sender, EventArgs e)
        {
            var group = GetSelectedGroupEntry();
            if (group == null)
            {
                return;
            }

            group.Group = (int)inputMovementGroup.Value;

            simulation.ReloadConfig(CurrentConfig);
        }

        private void OnMovementSpeedChanged(object sender, EventArgs e)
        {
            var group = GetSelectedGroupEntry();
            if (group == null)
            {
                return;
            }

            group.SpeedScale = (float)inputMovementSpeed.Value;

            simulation.ReloadConfig(CurrentConfig);
        }

        private void OnAddProcessorClick(object sender, EventArgs e)
        {
            var group = GetSelectedGroupEntry();
            if (group == null)
            {
                return;
            }

            var processorSelectDlg = new FormProcessorSelection();

            var res = processorSelectDlg.ShowDialog();
            if (res != DialogResult.OK)
            {
                return;
            }

            var processor = new Config.MovementProcessor();
            processor.Type = processorSelectDlg.Choice;

            var newIdx = group.Entries.Count;

            group.Entries.Add(processor);

            listProcessors.Items.Add(processor.Type);
            listProcessors.SelectedIndex = newIdx;

            simulation.ReloadConfig(CurrentConfig);
        }

        private void OnRemoveProcessorClick(object sender, EventArgs e)
        {
            var group = GetSelectedGroupEntry();
            if (group == null)
            {
                return;
            }

            var processorIdx = listProcessors.SelectedIndex;
            if (processorIdx == -1)
            {
                return;
            }

            group.Entries.RemoveAt(processorIdx);
            listProcessors.Items.RemoveAt(processorIdx);

            if (processorIdx > 0)
            {
                listProcessors.SelectedIndex = processorIdx - 1;
            }

            simulation.ReloadConfig(CurrentConfig);
        }

        private Config.MovementProcessor GetProcessorEntry()
        {
            var group = GetSelectedGroupEntry();
            if (group == null)
            {
                return null;
            }

            var processorIdx = listProcessors.SelectedIndex;
            if (processorIdx == -1)
            {
                return null;
            }

            return group.Entries[processorIdx];
        }

        private void OnDistanceValueChanged(object sender, EventArgs e)
        {
            var entry = GetProcessorEntry();
            if (entry == null)
            {
                return;
            }
            entry.Distance = (float)inputProcessorDistance.Value;

            simulation.ReloadConfig(CurrentConfig);
        }

        private void OnPowerValueChanged(object sender, EventArgs e)
        {
            var entry = GetProcessorEntry();
            if (entry == null)
            {
                return;
            }
            entry.Power = (float)inputProcessorPower.Value;

            simulation.ReloadConfig(CurrentConfig);
        }

        private void OnAdvanceTick(object sender, EventArgs e)
        {
            simulation.Tick();
        }

        private void OnLogClearClick(object sender, EventArgs e)
        {
            rtbLog.Clear();
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                Tool.Active = null;
                simCanvas.Cursor = Cursors.Default;
            }
            else if (e.KeyCode == Keys.Enter)
            {
                // Remove the ding.
                e.SuppressKeyPress = true;
            }
        }
    }
}
