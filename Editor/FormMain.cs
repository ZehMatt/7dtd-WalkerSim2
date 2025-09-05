using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WalkerSim.Editor.Properties;

namespace WalkerSim.Editor
{
    public partial class FormMain : Form, Logging.ISink
    {
        static readonly int ImageWidth = 768;
        static readonly int ImageHeight = 768;

        static readonly float WorldSizeX = 6000;
        static readonly float WorldSizeY = 6000;

        static readonly Vector3 WorldMins = new Vector3(-(WorldSizeX * 0.5f), -(WorldSizeY * 0.5f), 0);
        static readonly Vector3 WorldMaxs = new Vector3(WorldSizeX * 0.5f, WorldSizeY * 0.5f, 256);

        static WalkerSim.Config CurrentConfig;

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

        int GetTotalGroupCount()
        {
            return (simulation.Agents.Count + (CurrentConfig.GroupSize - 1)) / CurrentConfig.GroupSize;
        }

        void GenerateGroupColors()
        {
            var groupCount = GetTotalGroupCount();
            GroupColors = new Brush[groupCount];

            for (int i = 0; i < groupCount; i++)
            {
                var groupColor = simulation.GetGroupColor(i);
                var sysColor = System.Drawing.Color.FromArgb(groupColor.A, groupColor.R, groupColor.G, groupColor.B);
                GroupColors[i] = new SolidBrush(sysColor);
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

            this.Text = $"WalkerSim Editor v{BuildInfo.Version}";

            var defaultConfig = Encoding.UTF8.GetString(Resources.WalkerSimConfig);
            CurrentConfig = Config.LoadFromText(defaultConfig);

            // Set world size to 6k as the default thing until the world is changed.
            simulation.EditorMode = true;
            simulation.SetWorldSize(WorldMins, WorldMaxs);
            simulation.Reset(CurrentConfig);

            SetupLogging();
            SetupChoices();
            UpdateConfigFields();

            SetupConfigChangeHandlers();
            SetupSpeedModifiers();
            SetupDrawingContext();
            SetupWorlds();
            SetupLimits();
            ScrollWheelHack();
            SetupToolTips();

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
            viewBiomes.Click += (sender, e) => RenderSimulation();

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
            SetToolTip(inputAffectedGroup, "This specifies the group that this processor group will affect, if set to `Any` then all groups that don't specify this will be affected.");
            SetToolTip(inputSpawnProtectionTime, "The amount of seconds the player requires to be alive before any agents will spawn.\n\nNOTE: This only applies to starting a new game and spawning for the first time.");
            SetToolTip(inputActivationRadius, "The radius for the player in blocks/meters for when agents will spawn/despawn in the game world.\nDefault is 96, setting this too high can cause a lot of spawn failures, setting it to a lower value is not recommended.\n\nNOTE: This should not exceed the maximum view distance from serversettings.xml, view distance is specified in chunks and each chunk is 16x16x16.");
            SetToolTip(inputSoundAware, "Increases the awareness of \"spawned zombies\" to sound, this will make them react to sound such as gun shots causing them to wander towards the source.\n\nNOTE: Recommended to be enabled, the game is doing a poor job at this.");
        }

        public void Message(Logging.Level level, string message)
        {
            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    Message(level, message);
                });
                return;
            }

            Color color = Color.Black;
            switch (level)
            {
                case Logging.Level.Info:
                    color = Color.Black;
                    break;
                case Logging.Level.Warning:
                    color = Color.Yellow;
                    break;
                case Logging.Level.Error:
                    color = Color.Red;
                    break;
            }

            rtbLog.SuspendLayout();
            rtbLog.SelectionColor = color;
            rtbLog.AppendText($"{message}{Environment.NewLine}");
            rtbLog.ScrollToCaret();
            rtbLog.ResumeLayout();

            if (level == Logging.Level.Error)
            {
                // Switch to the log tab.
                tabSimulation.SelectedTab = tabSimulation.TabPages[3];
            }
        }

        private void SetupLogging()
        {
            Logging.AddSink(this);
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
            inputMovementSpeed.MouseWheel += ScrollHandlerFunction;
            inputSpawnProtectionTime.MouseWheel += ScrollHandlerFunction;
            inputActivationRadius.MouseWheel += ScrollHandlerFunction;
        }

        private void SetupSpeedModifiers()
        {
            speedToolStripMenuItem.DropDownItems.Clear();

            int speed = 1;
            for (int i = 0; i < 6; i++)
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

            // Draw current state.
            GenerateGroupColors();
            RenderSimulation();
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

        private void SetConfigValues(bool resetSimulation)
        {
            if (CurrentConfig == null || _updatingConfig)
            {
                return;
            }
            CurrentConfig.RandomSeed = (int)inputRandomSeed.Value;
            CurrentConfig.PopulationDensity = (int)inputMaxAgents.Value;
            CurrentConfig.SpawnActivationRadius = (int)inputActivationRadius.Value;
            CurrentConfig.EnhancedSoundAwareness = inputSoundAware.Checked;
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
            PopulateAffectedGroups();

            if (resetSimulation)
            {
                ReconfigureSimulation(true);
            }
        }

        private void SetupConfigChangeHandlers()
        {
            inputRandomSeed.ValueChanged += (sender, arg) => SetConfigValues(false);
            inputMaxAgents.ValueChanged += (sender, arg) => SetConfigValues(true);
            inputGroupSize.ValueChanged += (sender, arg) => SetConfigValues(true);
            inputSpawnProtectionTime.ValueChanged += (sender, arg) => SetConfigValues(false);
            inputStartGrouped.CheckedChanged += (sender, arg) => SetConfigValues(true);
            inputFastForward.CheckedChanged += (sender, arg) => SetConfigValues(false);
            inputPauseDuringBloodmoon.CheckedChanged += (sender, arg) => SetConfigValues(false);
            inputStartPosition.SelectedIndexChanged += (sender, arg) => SetConfigValues(true);
            inputRespawnPosition.SelectedIndexChanged += (sender, arg) => SetConfigValues(true);
            inputPostSpawnBehavior.SelectedIndexChanged += (sender, arg) => SetConfigValues(false);
            inputActivationRadius.ValueChanged += (sender, arg) => SetConfigValues(false);
        }

        private void UpdateConfigFields()
        {
            if (CurrentConfig == null)
                return;

            _updatingConfig = true;

            inputRandomSeed.Value = CurrentConfig.RandomSeed;
            inputMaxAgents.Value = CurrentConfig.PopulationDensity;
            inputActivationRadius.Value = CurrentConfig.SpawnActivationRadius;
            inputGroupSize.Value = CurrentConfig.GroupSize;
            inputStartGrouped.Checked = CurrentConfig.StartAgentsGrouped;
            inputSoundAware.Checked = CurrentConfig.EnhancedSoundAwareness;
            inputFastForward.Checked = CurrentConfig.FastForwardAtStart;
            inputPauseDuringBloodmoon.Checked = CurrentConfig.PauseDuringBloodmoon;
            inputSpawnProtectionTime.Value = CurrentConfig.SpawnProtectionTime;

            var spawnChoice = Utils.GetWorldLocationString(CurrentConfig.StartPosition);
            inputStartPosition.SelectedIndex = inputStartPosition.FindString(spawnChoice);

            var respawnChoice = Utils.GetWorldLocationString(CurrentConfig.RespawnPosition);
            inputRespawnPosition.SelectedIndex = inputRespawnPosition.FindString(respawnChoice);

            listProcessorGroups.Items.Clear();

            var groups = CurrentConfig.Processors;
            for (int i = 0; i < groups.Count; i++)
            {
                listProcessorGroups.Items.Add("Processor Group #" + i);
            }

            _updatingConfig = false;
        }

        private bool StartSimulation()
        {
            if (CurrentConfig.Processors.Count == 0)
            {
                MessageBox.Show("No processor groups are defined, simulation can not be started.", "No Processor Groups", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // Validate that all processors have valid entries.
            foreach (var group in CurrentConfig.Processors)
            {
                if (group.Entries.Count == 0)
                {
                    MessageBox.Show("One or more processor groups have no processors defined, simulation can not be started.", "No Processors", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }

            if (inputWorld.Text == "")
            {
                MessageBox.Show("No world was selected, the preview will not be accurate.", "No World Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            GenerateGroupColors();
            simulation.Start();

            updateTimer.Start();

            return true;
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

            RenderSimulation();
            UpdateStats();
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

            var newX = MathEx.Remap(x, 0, simCanvas.Width, worldMins.X, worldMaxs.X);
            var newY = MathEx.Remap(y, 0, simCanvas.Height, worldMins.Y, worldMaxs.Y);

            return new Vector3(newX, newY);
        }

        private void RenderToBitmap()
        {
            gr.Clear(Color.Black);

            if (viewBiomes.Checked)
            {
                Renderer.RenderBiomes(gr, simulation);
            }

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


            Renderer.DrawInformation(gr, simulation);


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

        private void UpdateStats()
        {
            lblStatTotalAgents.Text = simulation.AgentCount.ToString();
            lblStatInactive.Text = (simulation.AgentCount - simulation.ActiveCount).ToString();
            lblStatActive.Text = simulation.ActiveCount.ToString();
            lblStatGroups.Text = simulation.GroupCount.ToString();
            lblStatWindDir.Text = simulation.WindDirection.ToString();
            lblStatWindTarget.Text = simulation.WindDirectionTarget.ToString();
            lblStatWindChange.Text = simulation.TickNextWindChange.ToString();
            lblStatTicks.Text = simulation.Ticks.ToString();
            lblStatSimTime.Text = String.Format(
                "{0:0.00000} ms. ({1:0.000}/ps)",
                simulation.AverageSimTime * 1000.0,
                (simulation.AverageSimTime > 0 ? 1 / simulation.AverageSimTime : 0)
                );

            lblStatUpdateTime.Text = String.Format(
                "{0:0.00000} ms. ({1:0.000}/ps)",
                simulation.AverageUpdateTime * 1000.0,
                (simulation.AverageUpdateTime > 0 ? 1 / simulation.AverageUpdateTime : 0)
                );
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

            PopulateAffectedGroups();

            var selectedGroup = CurrentConfig.Processors[groupIdx];
            var itemText = "Any";

            if (selectedGroup.Group != -1)
            {
                itemText = selectedGroup.Group.ToString();
            }

            // Select the index corresponding to itemText.
            inputAffectedGroup.SelectedIndex = inputAffectedGroup.FindString(itemText);

            inputMovementSpeed.Value = (decimal)selectedGroup.SpeedScale;
            if (selectedGroup.Color == "")
            {
                boxGroupColor.BackColor = System.Drawing.Color.Purple;
            }
            else
            {
                boxGroupColor.BackColor = System.Drawing.ColorTranslator.FromHtml(selectedGroup.Color);
            }

            listProcessors.Items.Clear();
            foreach (var processor in selectedGroup.Entries)
            {
                listProcessors.Items.Add(processor.Type);
            }

            inputPostSpawnBehavior.SelectedIndex = (int)CurrentConfig.Processors[groupIdx].PostSpawnBehavior;

            inputWanderSpeed.SelectedIndex = (int)CurrentConfig.Processors[groupIdx].PostSpawnWanderSpeed;

            buttonRemoveProcessor.Enabled = false;

            UpdateAffectedAgentsCount();
        }

        private void PopulateAffectedGroups()
        {
            inputAffectedGroup.Items.Clear();
            inputAffectedGroup.Items.Add("Any");

            var currentGoup = GetSelectedGroupEntry();
            var processors = CurrentConfig.Processors;

            var totalGroups = GetTotalGroupCount();
            for (int i = 0; i < totalGroups; i++)
            {
                // See if any processors already has this.
                if (processors.Any(group => group.Group == i) && currentGoup.Group != i)
                    continue;

                inputAffectedGroup.Items.Add(i.ToString());
            }

            var itemText = "Any";
            if (currentGoup != null && currentGoup.Group != -1)
            {
                itemText = currentGoup.Group.ToString();
            }

            inputAffectedGroup.SelectedIndex = inputAffectedGroup.FindString(itemText);
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
            if (!StartSimulation())
            {
                return;
            }

            inputRandomSeed.Enabled = false;
            inputMaxAgents.Enabled = false;
            inputGroupSize.Enabled = false;
            inputWorld.Enabled = false;
            inputStartGrouped.Enabled = false;
            pauseToolStripMenuItem.Enabled = true;
            startToolStripMenuItem.Enabled = false;
            stopToolStripMenuItem.Enabled = true;
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
            else
            {
                _canvasScale = 0.5f;
            }

            SetupDrawingContext();
            CenterCanvas();
        }

        private void OnWorldSelectionChanged(object sender, EventArgs e)
        {
            var worldIdx = inputWorld.SelectedIndex;
            if (worldIdx == -1)
            {
                return;
            }

            var worldPath = Worlds.WorldFolders[worldIdx];
            var worldName = Path.GetFileName(worldPath);
            simulation.LoadMapData(worldPath, worldName);

            simulation.Reset(CurrentConfig);

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

                GenerateGroupColors();
                RenderSimulation();
            }
        }

        private void ReconfigureSimulation(bool reset = false)
        {
            if (reset)
                simulation.Reset(CurrentConfig);
            else
                simulation.ReloadConfig(CurrentConfig);

            GenerateGroupColors();
            UpdateStats();
            RenderSimulation();
        }

        private void OnAddGroupClick(object sender, EventArgs e)
        {
            if (CurrentConfig == null)
                return;

            var groups = CurrentConfig.Processors;
            var idx = groups.Count;

            var newGroup = new Config.MovementProcessorGroup()
            {
                Color = WalkerSim.Drawing.ColorTable.GetColorForIndex(idx).ToHtml()
            };
            groups.Add(newGroup);

            listProcessorGroups.Items.Add("Processor Group #" + idx);

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

            int selectedGroupId = -1;
            var selectedItem = inputAffectedGroup.Items[inputAffectedGroup.SelectedIndex] as string;
            if (selectedItem != "Any")
            {
                selectedGroupId = int.Parse(selectedItem);
            }

            group.Group = selectedGroupId;

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
                        SetToolTip(lblAffectedGroup, "There is another group with the same index, if you want to influence non-specific groups set it to 'Any'.");
                        break;
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

        Config.MovementProcessor CreateMovementProcessor(Config.MovementProcessorType type)
        {
            var processor = new Config.MovementProcessor();
            processor.Type = type;

            // Set some defaults based on type.
            switch (type)
            {
                case Config.MovementProcessorType.FlockAnyGroup:
                case Config.MovementProcessorType.AlignAnyGroup:
                case Config.MovementProcessorType.AvoidAnyGroup:
                case Config.MovementProcessorType.FlockSameGroup:
                case Config.MovementProcessorType.AlignSameGroup:
                case Config.MovementProcessorType.AvoidSameGroup:
                case Config.MovementProcessorType.FlockOtherGroup:
                case Config.MovementProcessorType.AlignOtherGroup:
                case Config.MovementProcessorType.AvoidOtherGroup:
                    processor.Distance = 30.0f;
                    processor.Power = 0.01f;
                    break;
                case Config.MovementProcessorType.Wind:
                    processor.Power = 0.01f;
                    break;
                case Config.MovementProcessorType.WindInverted:
                    processor.Power = 0.01f;
                    break;
                case Config.MovementProcessorType.StickToRoads:
                    processor.Power = 0.01f;
                    break;
                case Config.MovementProcessorType.AvoidRoads:
                    processor.Power = 0.01f;
                    break;
                case Config.MovementProcessorType.StickToPOIs:
                    processor.Power = 0.01f;
                    break;
                case Config.MovementProcessorType.AvoidPOIs:
                    processor.Power = 0.01f;
                    break;
                case Config.MovementProcessorType.WorldEvents:
                    processor.Power = 0.01f;
                    break;
                default:
                    break;
            }

            return processor;
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

            var processor = CreateMovementProcessor(processorSelectDlg.Choice);
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
                colorPickerDlg.Color = System.Drawing.ColorTranslator.FromHtml(selectedGroup.Color);
            }

            if (colorPickerDlg.ShowDialog() == DialogResult.OK)
            {
                boxGroupColor.BackColor = colorPickerDlg.Color;
                selectedGroup.Color = System.Drawing.ColorTranslator.ToHtml(boxGroupColor.BackColor);

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

        private void OnPostSpawnWanderSpeedSelectionChanged(object sender, EventArgs e)
        {
            var selectedGroup = GetSelectedGroupEntry();
            if (selectedGroup == null)
            {
                return;
            }

            var wanderSpeedChoice = inputWanderSpeed.SelectedIndex;
            if (wanderSpeedChoice != -1)
            {
                selectedGroup.PostSpawnWanderSpeed = (Config.WanderingSpeed)wanderSpeedChoice;
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

            listProcessorGroups.Items.Add("Processor Group #" + idx);

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
        }

        private void OnZoomOutClick(object sender, EventArgs e)
        {
            if (_canvasScale < 0.2f)
                return;

            _canvasScale -= 0.05f;
            SetupDrawingContext();
            CenterCanvas();
        }

        private void OnZoomResetClick(object sender, EventArgs e)
        {
            ZoomReset();
        }

        private void OnLoadStateClick(object sender, EventArgs e)
        {
            var browseFileDlg = new OpenFileDialog();
            browseFileDlg.Filter = "WalkerSim State File (WalkerSim.bin)|*.bin|All files (*.*)|*.*";
            browseFileDlg.FileName = "WalkerSim.bin";
            browseFileDlg.Title = "Load state save";
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

                if (!simulation.Load(browseFileDlg.FileName))
                {
                    MessageBox.Show("Failed to load the state save file", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var worldName = simulation.WorldName;
                if (Worlds.GetWorldPath(worldName, out var worldPath))
                {
                    // Update selection.
                    inputWorld.SelectedItem = worldName;
                }

                CurrentConfig = simulation.Config;
                UpdateConfigFields();
                CheckMaxAgents();

                GenerateGroupColors();
                RenderSimulation();
                ZoomReset();
                UpdateStats();
            }
        }

        private void OnResetClick(object sender, EventArgs e)
        {
            OnStopClick(sender, e);

            simulation.Reset(CurrentConfig);

            GenerateGroupColors();
            RenderSimulation();
            ZoomReset();
            UpdateStats();
        }

        private void OnSaveStateClick(object sender, EventArgs e)
        {
            var browseFileDlg = new SaveFileDialog();
            browseFileDlg.Filter = "WalkerSim State File (WalkerSim.bin)|*.bin|All files (*.*)|*.*";
            browseFileDlg.FileName = "WalkerSim.bin";
            browseFileDlg.Title = "Save state";
            if (browseFileDlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    simulation.Save(browseFileDlg.FileName);
                }
                catch (Exception ex)
                {
                    Logging.Exception(ex);
                }
            }
        }
    }
}
