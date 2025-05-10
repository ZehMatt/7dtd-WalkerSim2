using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace WalkerSim.Editor
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
        private List<Config.PostSpawnBehavior> _postSpawnBehaviors = new List<Config.PostSpawnBehavior>();

        private Dictionary<string, ToolTip> _toolTips = new Dictionary<string, ToolTip>();

        private float _canvasScale = 0.5f;

        Simulation simulation = Simulation.Instance;
        Random prng;
        Bitmap bitmap;
        System.Drawing.Graphics gr;

        Brush[] GroupColors;
        Brush[] PlayerColors;

        void GenerateColorTable()
        {
            var groupCount = simulation.Agents.Count / CurrentConfig.GroupSize;
            GroupColors = new Brush[groupCount];

            for (int i = 0; i < groupCount; i++)
            {
                GroupColors[i] = new SolidBrush(simulation.GetGroupColor(i));
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

            CurrentConfig = Config.GetDefault();
            UpdateConfigFields();

            SetupLogging();
            SetupConfigChangeHandlers();
            SetupSpeedModifiers();
            SetupDrawingContext();
            SetupWorlds();
            SetupLimits();
            SetupChoices();
            ScrollWheelHack();
            SetupToolTips();
            //LoadDefaultConfiguration();

            CurrentConfig = Config.GetDefault();

            // Set world size to 6k as the default thing until the world is changed.
            simulation.SetWorldSize(WorldMins, WorldMaxs);
            simulation.EditorMode = true;

            UpdateConfigFields();
            ReconfigureSimulation();

            CenterCanvas();

#if DEBUG
            prng = new Random(1);
#else
            prng = new Random((int)DateTime.Now.Ticks);
#endif

            inputRandomSeed.Value = prng.Next();

            viewAgents.Click += (sender, e) => RenderSimulation();
            viewRoads.Click += (sender, e) => RenderSimulation();
            viewPrefabs.Click += (sender, e) => RenderSimulation();
            viewEvents.Click += (sender, e) => RenderSimulation();

            splitContainer1.Panel1.AutoScroll = true;
            splitContainer1.Panel1.MouseWheel += (sender, e) =>
            {
                if (ModifierKeys == Keys.Control)
                {
                    if (e.Delta > 0)
                    {
                        OnZoomInClick(sender, e);
                    }
                    else
                    {
                        OnZoomOutClick(sender, e);
                    }
                }
                ((HandledMouseEventArgs)e).Handled = true;
            };
            splitContainer1.MouseWheel += (sender, e) =>
            {
                ((HandledMouseEventArgs)e).Handled = true;
            };
        }

        private void SetToolTip(Control ctrl, string helpText)
        {
            if (_toolTips.TryGetValue(ctrl.Name, out var tooltip))
            {
                if (helpText == null)
                {
                    tooltip.Dispose();
                    _toolTips.Remove(ctrl.Name);
                }
                else
                {
                    tooltip.SetToolTip(ctrl, helpText);
                }
            }
            else
            {
                var newTooltip = new System.Windows.Forms.ToolTip();
                newTooltip.SetToolTip(ctrl, helpText);
                // Keep open for as long user is pointing at it.
                newTooltip.AutoPopDelay = 999999;

                _toolTips.Add(ctrl.Name, newTooltip);
            }
        }

        private void SetupToolTips()
        {
            SetToolTip(inputWorld, "It is recommended to select the world you are creating the configuration for. If the configuration is used for another world the preview will not match.");
            SetToolTip(inputRandomSeed, "The simulation uses a random number generator, this will be the starting seed. This seed has no relation to any random seeds from the game.");
            SetToolTip(inputMaxAgents, "The amount of agents per square kilometer (km²), so the total amount is square kilometer multiplied by density.\nExample: 6k map with density of 150 will be: 6 x 6 x 150 = 5400.\n");
            SetToolTip(inputGroupSize, "This specifies how many members a group will have, the total amount of groups is Max Agents divided by Group Size.\n\nNOTE: Setting this to 1 is the same as not having groups.");
            SetToolTip(inputStartPosition, "Specifies the starting position of agents in the simulation.");
            SetToolTip(inputRespawnPosition, "Specifies the position of where agents will respawn when killed.\n\nNOTE: When specifying 'None' it will disable their respawn.");
            SetToolTip(inputStartGrouped, "If enabled the agents will start close to members of their own group, this means the starting position is per group, if disabled the starting position is per agent.");
            SetToolTip(inputPauseDuringBloodmoon, "If enabled the simulation will pause during blood moon which means no new in-game zombies will spawn and will be resumed afterwards, this does not affect blood moon spawns.");
            SetToolTip(inputMovementGroup, "This specifies the group that this processor will affect, if set to -1 then all groups will be affected.\n\nNOTE: This is the index of the group.");
            SetToolTip(inputSpawnProtectionTime, "The amount of seconds the player requires to be alive before any agents will spawn.\n\nNOTE: This only applies to starting a new game and spawning for the first time.");
        }

        private void LogMsg(string text, Color color, bool switchToLog)
        {
            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    LogMsg(text, color, switchToLog);
                });
                return;
            }

            rtbLog.SuspendLayout();
            rtbLog.SelectionColor = color;
            rtbLog.AppendText($"{text}{Environment.NewLine}");
            rtbLog.ScrollToCaret();
            rtbLog.ResumeLayout();

            if (switchToLog)
            {
                tabSimulation.SelectTab(2); // Switch to log.
            }
        }

        private void LogInfo(string msg)
        {
            LogMsg(msg, Color.Black, false);
        }

        private void LogWrn(string msg)
        {
            LogMsg(msg, Color.Goldenrod, true);
        }

        private void LogErr(string msg)
        {
            LogMsg(msg, Color.DarkRed, true);
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
            var txt = control.Controls[1] as System.Windows.Forms.TextBox;
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
            inputSpawnProtectionTime.MouseWheel += ScrollHandlerFunction;
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
            var scaledWidth = ImageWidth * _canvasScale;
            var scaledHeight = ImageHeight * _canvasScale;

            bitmap = new Bitmap((int)scaledWidth, (int)scaledHeight);
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
            inputMaxAgents.Minimum = Simulation.Limits.MinDensity;
            inputMaxAgents.Maximum = Simulation.Limits.MaxDensity;
            inputGroupSize.Minimum = 1;
            inputGroupSize.Maximum = Simulation.Limits.MaxDensity;
            inputRandomSeed.Minimum = 1;
            inputRandomSeed.Maximum = UInt32.MaxValue;
            inputSpawnProtectionTime.Minimum = 0;
            inputSpawnProtectionTime.Maximum = 1200;
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

            var postSpawnChoices = Enum.GetValues(typeof(Config.PostSpawnBehavior)).Cast<Config.PostSpawnBehavior>();
            foreach (var choice in postSpawnChoices)
            {
                var name = Utils.GetPostSpawnBehaviorString(choice);
                inputPostSpawnBehavior.Items.Add(name);

                _postSpawnBehaviors.Add(choice);
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
            CheckMaxAgents();
        }

        private void SetConfigValues()
        {
            if (CurrentConfig == null || _updatingConfig)
            {
                return;
            }
            CurrentConfig.RandomSeed = (int)inputRandomSeed.Value;
            CurrentConfig.PopulationDensity = (int)inputMaxAgents.Value;
            CurrentConfig.GroupSize = (int)inputGroupSize.Value;
            CurrentConfig.SpawnProtectionTime = (int)inputSpawnProtectionTime.Value;
            CurrentConfig.StartAgentsGrouped = inputStartGrouped.Checked;
            CurrentConfig.FastForwardAtStart = inputFastForward.Checked;
            CurrentConfig.PauseDuringBloodmoon = inputPauseDuringBloodmoon.Checked;

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

            CheckMaxAgents();
        }

        private void SetupConfigChangeHandlers()
        {
            inputRandomSeed.ValueChanged += (sender, arg) => SetConfigValues();
            inputMaxAgents.ValueChanged += (sender, arg) => SetConfigValues();
            inputGroupSize.ValueChanged += (sender, arg) => SetConfigValues();
            inputSpawnProtectionTime.ValueChanged += (sender, arg) => SetConfigValues();
            inputStartGrouped.CheckedChanged += (sender, arg) => SetConfigValues();
            inputFastForward.CheckedChanged += (sender, arg) => SetConfigValues();
            inputPauseDuringBloodmoon.CheckedChanged += (sender, arg) => SetConfigValues();
            inputStartPosition.SelectedIndexChanged += (sender, arg) => SetConfigValues();
            inputRespawnPosition.SelectedIndexChanged += (sender, arg) => SetConfigValues();
            inputPostSpawnBehavior.SelectedIndexChanged += (sender, arg) => SetConfigValues();
        }

        private void UpdateConfigFields()
        {
            if (CurrentConfig == null)
                return;

            _updatingConfig = true;

            inputRandomSeed.Value = CurrentConfig.RandomSeed;
            inputMaxAgents.Value = CurrentConfig.PopulationDensity;
            inputGroupSize.Value = CurrentConfig.GroupSize;
            inputStartGrouped.Checked = CurrentConfig.StartAgentsGrouped;
            inputFastForward.Checked = CurrentConfig.FastForwardAtStart;
            inputPauseDuringBloodmoon.Checked = CurrentConfig.PauseDuringBloodmoon;
            inputSpawnProtectionTime.Value = CurrentConfig.SpawnProtectionTime;

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
            simulation.Reset(CurrentConfig);

            GenerateColorTable();
            simulation.Start();

            updateTimer.Start();
        }

        private void StopSimulation()
        {
            simulation.Stop();
            updateTimer.Stop();

            startToolStripMenuItem.Enabled = true;
            stopToolStripMenuItem.Enabled = false;
            pauseToolStripMenuItem.Enabled = false;
            resumeToolStripMenuItem.Enabled = false;
        }

        private void OnTick(object sender, EventArgs e)
        {
            simulation.GameUpdate(updateTimer.Interval / 1000.0f);

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

            var newX = Math.Remap(x, 0, simCanvas.Width, worldMins.X, worldMaxs.X);
            var newY = Math.Remap(y, 0, simCanvas.Height, worldMins.Y, worldMaxs.Y);

            return new Vector3(newX, newY);
        }

        private void RenderToBitmap()
        {
            gr.Clear(Color.Black);

            if (viewRoads.Checked)
            {
                Renderer.RenderRoads(gr, simulation);
            }

            if (viewPrefabs.Checked)
            {
                Renderer.RenderPrefabs(gr, simulation);
            }

            if (viewAgents.Checked)
            {
                Renderer.RenderAgents(gr, simulation, GroupColors);
            }

            if (viewActiveAgents.Checked)
            {
                Renderer.RenderActiveAgents(gr, simulation, GroupColors);
            }

            Renderer.RenderPlayers(gr, simulation, PlayerColors);

            if (viewEvents.Checked)
            {
                Renderer.RenderEvents(gr, simulation);
            }

            // Wind arrow.
            DrawingUtils.DrawArrow(gr, simulation.WindDirection, new PointF(26, 26), 16, 6);

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

            simCanvas.Refresh();
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
            buttonDuplicateGroup.Enabled = groupIdx != -1;
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
            if (selectedGroup.Color == "")
            {
                boxGroupColor.BackColor = System.Drawing.Color.Purple;
            }
            else
            {
                boxGroupColor.BackColor = Utils.ParseColor(selectedGroup.Color);
            }

            listProcessors.Items.Clear();
            foreach (var processor in selectedGroup.Entries)
            {
                listProcessors.Items.Add(processor.Type);
            }

            inputPostSpawnBehavior.SelectedIndex = (int)CurrentConfig.Processors[groupIdx].PostSpawnBehavior;

            buttonRemoveProcessor.Enabled = false;

            inputMovementGroup.Minimum = -1;
            inputMovementGroup.Maximum = simulation.GroupCount - 1;

            UpdateAffectedAgentsCount();
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
            inputWorld.Enabled = false;
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
            inputWorld.Enabled = true;
            inputStartGrouped.Enabled = true;
            pauseToolStripMenuItem.Enabled = false;
            resumeToolStripMenuItem.Enabled = false;
            stopToolStripMenuItem.Enabled = false;
            startToolStripMenuItem.Enabled = true;
            advanceOneTickToolStripMenuItem.Enabled = false;
        }

        private void ZoomReset()
        {
            // Depending on world size use appropriate scaling.
            var worldSize = simulation.WorldSize;
            var worldSizeX = worldSize.X;
            var worldSizeY = worldSize.Y;

            var size = System.Math.Max(worldSizeX, worldSizeY);
            if (size > 10000)
            {
                _canvasScale = 0.65f;
            }
            else if (size > 8000)
            {
                _canvasScale = 0.66f;
            }
            else if (size > 6000)
            {
                _canvasScale = 0.68f;
            }

            SetupDrawingContext();
            CenterCanvas();

            RenderSimulation();
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

            CheckMaxAgents();

            ReconfigureSimulation();
            simulation.Reset(CurrentConfig);
            GenerateColorTable();

            ZoomReset();
        }

        private void CheckMaxAgents()
        {
            var mapData = simulation.MapData;
            var maxAgents = inputMaxAgents.Value;

            var colorRed = 255 - (System.Math.Min((int)maxAgents, 180) / 180.0) * 255;
            var colorGreen = maxAgents <= 30
                ? (System.Math.Min((int)maxAgents, 30) / 30.0) * 128  // Green ramps up to 128 for orange
                : 180 - ((System.Math.Min((int)maxAgents, 180) - 30) / 110.0) * 128; // Green ramps down to 0
            var colorBlue = 0; // No blue to keep warm tones
            inputMaxAgents.ForeColor = Color.FromArgb((int)colorRed, (int)colorGreen, (int)colorBlue);

            Invalidate();
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
                var nextState = Tool.Active.OnClick(simPos);
                if (nextState == NextToolState.Stop)
                {
                    Tool.Active = null;
                    simCanvas.Cursor = Cursors.Default;
                }
            }
        }

        private void OnSimCanvasMouseMove(object sender, MouseEventArgs e)
        {
            RenderSimulation();
        }

        private void loadConfigurationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var browseFileDlg = new OpenFileDialog();
            browseFileDlg.Filter = "Config File (WalkerSim.xml)|*.xml|All files (*.*)|*.*";
            browseFileDlg.FileName = "WalkerSim.xml";
            browseFileDlg.RestoreDirectory = true;
            browseFileDlg.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
            browseFileDlg.Title = "Load configuration";
            if (browseFileDlg.ShowDialog() == DialogResult.OK)
            {
                if (simulation.Running)
                {
                    var answer = MessageBox.Show("The simulation is currently running, in order to import the configuration it has to be stopped.\nDo you wish to stop the simulation?",
                        "Simulation Running",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question
                        );

                    if (answer == DialogResult.No)
                    {
                        return;
                    }

                    StopSimulation();
                }

                LoadConfiguration(browseFileDlg.FileName);

                simulation.Reset(CurrentConfig);

                GenerateColorTable();
                RenderSimulation();
            }
        }

        private void ReconfigureSimulation()
        {
            simulation.ReloadConfig(CurrentConfig);
            GenerateColorTable();
        }

        private void OnAddGroupClick(object sender, EventArgs e)
        {
            if (CurrentConfig == null)
                return;

            var groups = CurrentConfig.Processors;
            var idx = groups.Count;

            var newGroup = new Config.MovementProcessorGroup()
            {
                Color = Utils.ColorToHexString(ColorTable.GetColorForIndex(idx))
            };
            groups.Add(newGroup);

            listProcessorGroups.Items.Add("Group #" + idx);

            ReconfigureSimulation();
            UpdateAffectedAgentsCount();
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

            ReconfigureSimulation();
        }

        private Config.MovementProcessorGroup GetSelectedGroupEntry()
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

            ReconfigureSimulation();
            UpdateAffectedAgentsCount();

            if (group.Group != -1)
            {
                // See if there is another group with the same ID.
                foreach (var proc in CurrentConfig.Processors)
                {
                    if (proc == group)
                        continue;

                    if (proc.Group == group.Group)
                    {
                        lblAffectedGroup.ForeColor = Color.Red;
                        SetToolTip(lblAffectedGroup, "There is another group with the same index, if you want to influence non-specific groups set it to -1.");
                    }
                    else
                    {
                        lblAffectedGroup.ForeColor = Color.Black;
                        SetToolTip(lblAffectedGroup, null);
                    }
                }
            }
            else
            {
                // Reset the color to default.
                lblAffectedGroup.ForeColor = Color.Black;
                SetToolTip(lblAffectedGroup, null);
            }
        }

        private void UpdateAffectedAgentsCount()
        {
            var group = GetSelectedGroupEntry();
            if (group == null)
            {
                return;
            }

            int affected = 0;
            int anyCount = 0;
            foreach (var agent in simulation.Agents)
            {
                if (agent.Group == group.Group || group.Group == -1)
                {
                    affected++;
                }
            }

            if (group.Group == -1)
            {
                // Count the number of groups with -1
                foreach (var grp in CurrentConfig.Processors)
                {
                    if (grp.Group == -1)
                    {
                        anyCount++;
                    }
                }

                affected /= anyCount;
            }

            lblAffected.Text = $"Affected agents: {affected}";
        }

        private void OnMovementSpeedChanged(object sender, EventArgs e)
        {
            var group = GetSelectedGroupEntry();
            if (group == null)
            {
                return;
            }

            group.SpeedScale = (float)inputMovementSpeed.Value;

            ReconfigureSimulation();
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

            ReconfigureSimulation();
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

            ReconfigureSimulation();
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

            ReconfigureSimulation();
        }

        private void OnPowerValueChanged(object sender, EventArgs e)
        {
            var entry = GetProcessorEntry();
            if (entry == null)
            {
                return;
            }
            entry.Power = (float)inputProcessorPower.Value;

            ReconfigureSimulation();
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

        private void OnGroupColorPickClick(object sender, EventArgs e)
        {
            var selectedGroup = GetSelectedGroupEntry();
            if (selectedGroup == null)
            {
                return;
            }

            if (selectedGroup.Color == "")
            {
                colorPickerDlg.Color = System.Drawing.Color.Purple;
            }
            else
            {
                colorPickerDlg.Color = Utils.ParseColor(selectedGroup.Color);
            }

            if (colorPickerDlg.ShowDialog() == DialogResult.OK)
            {
                boxGroupColor.BackColor = colorPickerDlg.Color;
                selectedGroup.Color = Utils.ColorToHexString(boxGroupColor.BackColor);

                ReconfigureSimulation();
            }
        }

        private void OnPostSpawnBehaviorSelectionChanged(object sender, EventArgs e)
        {
            var selectedGroup = GetSelectedGroupEntry();
            if (selectedGroup == null)
            {
                return;
            }

            var postSpawnChoice = inputPostSpawnBehavior.SelectedIndex;
            if (postSpawnChoice != -1)
            {
                selectedGroup.PostSpawnBehavior = (Config.PostSpawnBehavior)postSpawnChoice;
            }

            ReconfigureSimulation();
        }

        private void OnDuplicateGroupClick(object sender, EventArgs e)
        {
            var selectedGroup = GetSelectedGroupEntry();
            if (selectedGroup == null)
            {
                return;
            }

            var groups = CurrentConfig.Processors;
            var idx = groups.Count;

            var newGroup = new Config.MovementProcessorGroup()
            {
                Group = selectedGroup.Group,
                SpeedScale = selectedGroup.SpeedScale,
                Color = selectedGroup.Color,
                Entries = new List<Config.MovementProcessor>(),
            };

            foreach (var entry in selectedGroup.Entries)
            {
                var newEntry = new Config.MovementProcessor()
                {
                    Type = entry.Type,
                    Distance = entry.Distance,
                    Power = entry.Power,
                };
                newGroup.Entries.Add(newEntry);
            }

            groups.Add(newGroup);

            listProcessorGroups.Items.Add("Group #" + idx);

            ReconfigureSimulation();
        }

        private void OnExportConfigurationClick(object sender, EventArgs e)
        {
            var browseFileDlg = new SaveFileDialog();
            browseFileDlg.Filter = "Config File (WalkerSim.xml)|*.xml|All files (*.*)|*.*";
            browseFileDlg.DefaultExt = ".xml";
            browseFileDlg.FileName = "WalkerSim.xml";
            browseFileDlg.RestoreDirectory = true;
            browseFileDlg.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
            browseFileDlg.Title = "Export Configuration";
            if (browseFileDlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (var writer = System.IO.File.CreateText(browseFileDlg.FileName))
                    {
                        simulation.Config.Export(writer);
                    }
                }
                catch (Exception ex)
                {
                    Logging.Exception(ex);
                }
            }
        }

        private void OnRandSeedClick(object sender, EventArgs e)
        {
            inputRandomSeed.Value = prng.Next();
        }

        private void OnClickExit(object sender, EventArgs e)
        {
            Close();
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
        private void OnAddPlayerClick(object sender, EventArgs e)
        {
            Tool.Active = new AddPlayerTool();
            simCanvas.Cursor = Cursors.Cross;
        }

        private void OnSetPlayerPosClick(object sender, EventArgs e)
        {
            Tool.Active = new SetPlayerPositionTool();
            simCanvas.Cursor = Cursors.Cross;
        }

        private void CenterCanvas()
        {
            var panelCanvas = splitContainer1.Panel1;

            // Get the panel's client size (visible area)
            var panelWidth = panelCanvas.ClientSize.Width;
            var panelHeight = panelCanvas.ClientSize.Height;

            // Get the PictureBox size
            var imageWidth = simCanvas.Width;
            var imageHeight = simCanvas.Height;

            // Center horizontally if PictureBox width is smaller than panel width
            if (imageWidth < panelWidth)
            {
                int x = (panelWidth - imageWidth) / 2;
                simCanvas.Left = x;
            }

            // Center vertically if PictureBox height is smaller than panel height
            if (imageHeight < panelHeight)
            {
                int y = (panelHeight - imageHeight) / 2;
                simCanvas.Top = y;
            }
        }

        private void OnResizeCanvas(object sender, EventArgs e)
        {
            CenterCanvas();
        }

        private void OnSplitContainerMove(object sender, SplitterEventArgs e)
        {
            CenterCanvas();
        }

        private void OnZoomInClick(object sender, EventArgs e)
        {
            if (_canvasScale >= 4.0f)
                return;

            _canvasScale += 0.05f;
            SetupDrawingContext();
            CenterCanvas();
            RenderSimulation();
        }

        private void OnZoomOutClick(object sender, EventArgs e)
        {
            if (_canvasScale < 0.2f)
                return;

            _canvasScale -= 0.05f;
            SetupDrawingContext();
            CenterCanvas();
            RenderSimulation();
        }

        private void OnZoomResetClick(object sender, EventArgs e)
        {
            ZoomReset();
        }
    }
}
