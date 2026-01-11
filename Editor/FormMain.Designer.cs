using WalkerSim.Editor.Controls;

namespace WalkerSim.Editor
{
    partial class FormMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.updateTimer = new System.Windows.Forms.Timer(this.components);
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadConfigurationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportConfigurationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.zoomSubMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.xToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.inToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.outToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.viewBiomes = new System.Windows.Forms.ToolStripMenuItem();
            this.viewRoads = new System.Windows.Forms.ToolStripMenuItem();
            this.viewAgents = new System.Windows.Forms.ToolStripMenuItem();
            this.viewActiveAgents = new System.Windows.Forms.ToolStripMenuItem();
            this.viewEvents = new System.Windows.Forms.ToolStripMenuItem();
            this.viewPrefabs = new System.Windows.Forms.ToolStripMenuItem();
            this.viewCities = new System.Windows.Forms.ToolStripMenuItem();
            this.simulationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.startToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.pauseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resumeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.speedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.advanceOneTickToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.debugToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadStateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveStateToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.emitSoundToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.killToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.addPlayerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setPlayerPositionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.documentationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.simCanvas = new System.Windows.Forms.PictureBox();
            this.tabSimulation = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.lblPauseDuringBloodmoon = new WalkerSim.Editor.LabelWithHelp();
            this.lblStartAgentsGrouped = new WalkerSim.Editor.LabelWithHelp();
            this.inputSpawnProtectionTime = new System.Windows.Forms.NumericUpDown();
            this.panel1 = new System.Windows.Forms.Panel();
            this.inputRandomSeed = new System.Windows.Forms.NumericUpDown();
            this.btRand = new System.Windows.Forms.Button();
            this.inputRespawnPosition = new System.Windows.Forms.ComboBox();
            this.lblSpawnProtectionTime = new WalkerSim.Editor.LabelWithHelp();
            this.lblRespawnPosition = new WalkerSim.Editor.LabelWithHelp();
            this.inputWorld = new System.Windows.Forms.ComboBox();
            this.inputStartPosition = new System.Windows.Forms.ComboBox();
            this.lblWorld = new WalkerSim.Editor.LabelWithHelp();
            this.lblStartPosition = new WalkerSim.Editor.LabelWithHelp();
            this.lblRandomSeed = new WalkerSim.Editor.LabelWithHelp();
            this.lblGroupSize = new WalkerSim.Editor.LabelWithHelp();
            this.inputGroupSize = new System.Windows.Forms.NumericUpDown();
            this.lblPopulationDensity = new WalkerSim.Editor.LabelWithHelp();
            this.inputStartGrouped = new System.Windows.Forms.CheckBox();
            this.inputPauseDuringBloodmoon = new System.Windows.Forms.CheckBox();
            this.lblFastForward = new WalkerSim.Editor.LabelWithHelp();
            this.inputFastForward = new System.Windows.Forms.CheckBox();
            this.inputMaxAgents = new System.Windows.Forms.NumericUpDown();
            this.lblActivationRadius = new WalkerSim.Editor.LabelWithHelp();
            this.inputActivationRadius = new System.Windows.Forms.NumericUpDown();
            this.lblEnhancedSoundAwareness = new WalkerSim.Editor.LabelWithHelp();
            this.inputSoundAware = new System.Windows.Forms.CheckBox();
            this.labelWithHelp1 = new WalkerSim.Editor.LabelWithHelp();
            this.inputSoundDistanceScale = new System.Windows.Forms.NumericUpDown();
            this.labelWithHelp2 = new WalkerSim.Editor.LabelWithHelp();
            this.inputMaxSpawnedZombies = new WalkerSim.Editor.Controls.PercentageUpDown();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.listProcessorGroups = new System.Windows.Forms.ListBox();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.button1 = new System.Windows.Forms.Button();
            this.buttonRemoveGroup = new System.Windows.Forms.Button();
            this.buttonDuplicateGroup = new System.Windows.Forms.Button();
            this.groupProcessors = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel6 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel7 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel8 = new System.Windows.Forms.TableLayoutPanel();
            this.button4 = new System.Windows.Forms.Button();
            this.buttonRemoveProcessor = new System.Windows.Forms.Button();
            this.listProcessors = new System.Windows.Forms.ListBox();
            this.groupParameter = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel9 = new System.Windows.Forms.TableLayoutPanel();
            this.inputProcessorPower = new System.Windows.Forms.NumericUpDown();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.inputProcessorDistance = new System.Windows.Forms.NumericUpDown();
            this.groupProps = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.boxGroupColor = new System.Windows.Forms.PictureBox();
            this.buttonGroupColor = new System.Windows.Forms.Button();
            this.inputPostSpawnBehavior = new System.Windows.Forms.ComboBox();
            this.inputAffectedGroup = new System.Windows.Forms.ComboBox();
            this.lblPostSpawnWanderSpeed = new WalkerSim.Editor.LabelWithHelp();
            this.lblMovementSpeed = new WalkerSim.Editor.LabelWithHelp();
            this.inputMovementSpeed = new System.Windows.Forms.NumericUpDown();
            this.lblAffected = new System.Windows.Forms.Label();
            this.lblGroupColor = new WalkerSim.Editor.LabelWithHelp();
            this.lblPostSpawnBehavior = new WalkerSim.Editor.LabelWithHelp();
            this.inputWanderSpeed = new System.Windows.Forms.ComboBox();
            this.lblAffectedGroup = new WalkerSim.Editor.LabelWithHelp();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.lblStatGroups = new System.Windows.Forms.Label();
            this.label24 = new System.Windows.Forms.Label();
            this.lblStatUpdateTime = new System.Windows.Forms.Label();
            this.label22 = new System.Windows.Forms.Label();
            this.lblStatWindTarget = new System.Windows.Forms.Label();
            this.label20 = new System.Windows.Forms.Label();
            this.lblStatSimTime = new System.Windows.Forms.Label();
            this.label27 = new System.Windows.Forms.Label();
            this.lblStatWindChange = new System.Windows.Forms.Label();
            this.label25 = new System.Windows.Forms.Label();
            this.lblStatWindDir = new System.Windows.Forms.Label();
            this.label23 = new System.Windows.Forms.Label();
            this.lblStatTicks = new System.Windows.Forms.Label();
            this.label21 = new System.Windows.Forms.Label();
            this.lblStatActive = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.lblStatInactive = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.lblStatTotalAgents = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.rtbLog = new System.Windows.Forms.RichTextBox();
            this.contextLog = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.clearToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.colorPickerDlg = new System.Windows.Forms.ColorDialog();
            this.toolTipGroupSize = new System.Windows.Forms.ToolTip(this.components);
            this.btZoomIn = new System.Windows.Forms.Label();
            this.btZoomOut = new System.Windows.Forms.Label();
            this.label29 = new System.Windows.Forms.Label();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.simCanvas)).BeginInit();
            this.tabSimulation.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.inputSpawnProtectionTime)).BeginInit();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.inputRandomSeed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.inputGroupSize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.inputMaxAgents)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.inputActivationRadius)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.inputSoundDistanceScale)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.inputMaxSpawnedZombies)).BeginInit();
            this.tabPage2.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.groupProcessors.SuspendLayout();
            this.tableLayoutPanel6.SuspendLayout();
            this.tableLayoutPanel7.SuspendLayout();
            this.tableLayoutPanel8.SuspendLayout();
            this.groupParameter.SuspendLayout();
            this.tableLayoutPanel9.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.inputProcessorPower)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.inputProcessorDistance)).BeginInit();
            this.groupProps.SuspendLayout();
            this.tableLayoutPanel5.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.boxGroupColor)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.inputMovementSpeed)).BeginInit();
            this.tabPage4.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.contextLog.SuspendLayout();
            this.SuspendLayout();
            // 
            // updateTimer
            // 
            this.updateTimer.Interval = 16;
            this.updateTimer.Tick += new System.EventHandler(this.OnTick);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.viewToolStripMenuItem,
            this.simulationToolStripMenuItem,
            this.toolsToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(840, 24);
            this.menuStrip1.TabIndex = 4;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadConfigurationToolStripMenuItem,
            this.exportConfigurationToolStripMenuItem,
            this.toolStripSeparator4,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // loadConfigurationToolStripMenuItem
            // 
            this.loadConfigurationToolStripMenuItem.Name = "loadConfigurationToolStripMenuItem";
            this.loadConfigurationToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.loadConfigurationToolStripMenuItem.Text = "Import Configuration";
            this.loadConfigurationToolStripMenuItem.Click += new System.EventHandler(this.loadConfigurationToolStripMenuItem_Click);
            // 
            // exportConfigurationToolStripMenuItem
            // 
            this.exportConfigurationToolStripMenuItem.Name = "exportConfigurationToolStripMenuItem";
            this.exportConfigurationToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.exportConfigurationToolStripMenuItem.Text = "Export Configuration";
            this.exportConfigurationToolStripMenuItem.Click += new System.EventHandler(this.OnExportConfigurationClick);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(184, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.OnClickExit);
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.zoomSubMenu,
            this.toolStripSeparator3,
            this.viewBiomes,
            this.viewRoads,
            this.viewAgents,
            this.viewActiveAgents,
            this.viewEvents,
            this.viewPrefabs,
            this.viewCities});
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.viewToolStripMenuItem.Text = "View";
            // 
            // zoomSubMenu
            // 
            this.zoomSubMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.xToolStripMenuItem1,
            this.toolStripSeparator2,
            this.inToolStripMenuItem,
            this.outToolStripMenuItem});
            this.zoomSubMenu.Name = "zoomSubMenu";
            this.zoomSubMenu.Size = new System.Drawing.Size(155, 22);
            this.zoomSubMenu.Text = "Zoom";
            // 
            // xToolStripMenuItem1
            // 
            this.xToolStripMenuItem1.Name = "xToolStripMenuItem1";
            this.xToolStripMenuItem1.Size = new System.Drawing.Size(159, 22);
            this.xToolStripMenuItem1.Text = "Reset";
            this.xToolStripMenuItem1.Click += new System.EventHandler(this.OnZoomResetClick);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(156, 6);
            // 
            // inToolStripMenuItem
            // 
            this.inToolStripMenuItem.Name = "inToolStripMenuItem";
            this.inToolStripMenuItem.ShortcutKeyDisplayString = "";
            this.inToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Up)));
            this.inToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            this.inToolStripMenuItem.Text = "In";
            this.inToolStripMenuItem.Click += new System.EventHandler(this.OnZoomInClick);
            // 
            // outToolStripMenuItem
            // 
            this.outToolStripMenuItem.Name = "outToolStripMenuItem";
            this.outToolStripMenuItem.ShortcutKeyDisplayString = "";
            this.outToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Down)));
            this.outToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            this.outToolStripMenuItem.Text = "Out";
            this.outToolStripMenuItem.Click += new System.EventHandler(this.OnZoomOutClick);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(152, 6);
            // 
            // viewBiomes
            // 
            this.viewBiomes.CheckOnClick = true;
            this.viewBiomes.Name = "viewBiomes";
            this.viewBiomes.Size = new System.Drawing.Size(155, 22);
            this.viewBiomes.Text = "Biomes";
            // 
            // viewRoads
            // 
            this.viewRoads.Checked = true;
            this.viewRoads.CheckOnClick = true;
            this.viewRoads.CheckState = System.Windows.Forms.CheckState.Checked;
            this.viewRoads.Name = "viewRoads";
            this.viewRoads.Size = new System.Drawing.Size(155, 22);
            this.viewRoads.Text = "Roads";
            // 
            // viewAgents
            // 
            this.viewAgents.Checked = true;
            this.viewAgents.CheckOnClick = true;
            this.viewAgents.CheckState = System.Windows.Forms.CheckState.Checked;
            this.viewAgents.Name = "viewAgents";
            this.viewAgents.Size = new System.Drawing.Size(155, 22);
            this.viewAgents.Text = "Inactive Agents";
            // 
            // viewActiveAgents
            // 
            this.viewActiveAgents.Checked = true;
            this.viewActiveAgents.CheckOnClick = true;
            this.viewActiveAgents.CheckState = System.Windows.Forms.CheckState.Checked;
            this.viewActiveAgents.Name = "viewActiveAgents";
            this.viewActiveAgents.Size = new System.Drawing.Size(155, 22);
            this.viewActiveAgents.Text = "Active Agents";
            // 
            // viewEvents
            // 
            this.viewEvents.Checked = true;
            this.viewEvents.CheckOnClick = true;
            this.viewEvents.CheckState = System.Windows.Forms.CheckState.Checked;
            this.viewEvents.Name = "viewEvents";
            this.viewEvents.Size = new System.Drawing.Size(155, 22);
            this.viewEvents.Text = "Events";
            // 
            // viewPrefabs
            // 
            this.viewPrefabs.CheckOnClick = true;
            this.viewPrefabs.Name = "viewPrefabs";
            this.viewPrefabs.Size = new System.Drawing.Size(155, 22);
            this.viewPrefabs.Text = "Prefabs";
            // 
            // viewCities
            // 
            this.viewCities.Checked = true;
            this.viewCities.CheckOnClick = true;
            this.viewCities.CheckState = System.Windows.Forms.CheckState.Checked;
            this.viewCities.Name = "viewCities";
            this.viewCities.Size = new System.Drawing.Size(155, 22);
            this.viewCities.Text = "Cities";
            // 
            // simulationToolStripMenuItem
            // 
            this.simulationToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.startToolStripMenuItem,
            this.stopToolStripMenuItem,
            this.resetToolStripMenuItem,
            this.toolStripMenuItem3,
            this.pauseToolStripMenuItem,
            this.resumeToolStripMenuItem,
            this.speedToolStripMenuItem,
            this.advanceOneTickToolStripMenuItem,
            this.toolStripSeparator5,
            this.debugToolStripMenuItem});
            this.simulationToolStripMenuItem.Name = "simulationToolStripMenuItem";
            this.simulationToolStripMenuItem.Size = new System.Drawing.Size(76, 20);
            this.simulationToolStripMenuItem.Text = "Simulation";
            // 
            // startToolStripMenuItem
            // 
            this.startToolStripMenuItem.Name = "startToolStripMenuItem";
            this.startToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F5;
            this.startToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            this.startToolStripMenuItem.Text = "Start";
            this.startToolStripMenuItem.Click += new System.EventHandler(this.OnRestartClick);
            // 
            // stopToolStripMenuItem
            // 
            this.stopToolStripMenuItem.Enabled = false;
            this.stopToolStripMenuItem.Name = "stopToolStripMenuItem";
            this.stopToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.F5)));
            this.stopToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            this.stopToolStripMenuItem.Text = "Stop";
            this.stopToolStripMenuItem.Click += new System.EventHandler(this.OnStopClick);
            // 
            // resetToolStripMenuItem
            // 
            this.resetToolStripMenuItem.Name = "resetToolStripMenuItem";
            this.resetToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            this.resetToolStripMenuItem.Text = "Reset";
            this.resetToolStripMenuItem.Click += new System.EventHandler(this.OnResetClick);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(190, 6);
            // 
            // pauseToolStripMenuItem
            // 
            this.pauseToolStripMenuItem.Enabled = false;
            this.pauseToolStripMenuItem.Name = "pauseToolStripMenuItem";
            this.pauseToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Alt) 
            | System.Windows.Forms.Keys.Pause)));
            this.pauseToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            this.pauseToolStripMenuItem.Text = "Pause";
            this.pauseToolStripMenuItem.Click += new System.EventHandler(this.OnPauseClick);
            // 
            // resumeToolStripMenuItem
            // 
            this.resumeToolStripMenuItem.Enabled = false;
            this.resumeToolStripMenuItem.Name = "resumeToolStripMenuItem";
            this.resumeToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            this.resumeToolStripMenuItem.Text = "Resume";
            this.resumeToolStripMenuItem.Click += new System.EventHandler(this.OnResumeClick);
            // 
            // speedToolStripMenuItem
            // 
            this.speedToolStripMenuItem.Name = "speedToolStripMenuItem";
            this.speedToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            this.speedToolStripMenuItem.Text = "Speed";
            // 
            // advanceOneTickToolStripMenuItem
            // 
            this.advanceOneTickToolStripMenuItem.Enabled = false;
            this.advanceOneTickToolStripMenuItem.Name = "advanceOneTickToolStripMenuItem";
            this.advanceOneTickToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F10;
            this.advanceOneTickToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            this.advanceOneTickToolStripMenuItem.Text = "Advance one Tick";
            this.advanceOneTickToolStripMenuItem.Click += new System.EventHandler(this.OnAdvanceTick);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(190, 6);
            // 
            // debugToolStripMenuItem
            // 
            this.debugToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadStateToolStripMenuItem,
            this.saveStateToolStripMenuItem1});
            this.debugToolStripMenuItem.Name = "debugToolStripMenuItem";
            this.debugToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            this.debugToolStripMenuItem.Text = "Debug";
            // 
            // loadStateToolStripMenuItem
            // 
            this.loadStateToolStripMenuItem.Name = "loadStateToolStripMenuItem";
            this.loadStateToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
            this.loadStateToolStripMenuItem.Text = "Load State";
            this.loadStateToolStripMenuItem.Click += new System.EventHandler(this.OnLoadStateClick);
            // 
            // saveStateToolStripMenuItem1
            // 
            this.saveStateToolStripMenuItem1.Name = "saveStateToolStripMenuItem1";
            this.saveStateToolStripMenuItem1.Size = new System.Drawing.Size(129, 22);
            this.saveStateToolStripMenuItem1.Text = "Save State";
            this.saveStateToolStripMenuItem1.Click += new System.EventHandler(this.OnSaveStateClick);
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.emitSoundToolStripMenuItem,
            this.killToolStripMenuItem,
            this.toolStripSeparator1,
            this.addPlayerToolStripMenuItem,
            this.setPlayerPositionToolStripMenuItem});
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(46, 20);
            this.toolsToolStripMenuItem.Text = "Tools";
            // 
            // emitSoundToolStripMenuItem
            // 
            this.emitSoundToolStripMenuItem.Name = "emitSoundToolStripMenuItem";
            this.emitSoundToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            this.emitSoundToolStripMenuItem.Text = "Emit Sound";
            this.emitSoundToolStripMenuItem.Click += new System.EventHandler(this.OnClickSoundEmit);
            // 
            // killToolStripMenuItem
            // 
            this.killToolStripMenuItem.Name = "killToolStripMenuItem";
            this.killToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            this.killToolStripMenuItem.Text = "Kill";
            this.killToolStripMenuItem.Click += new System.EventHandler(this.OnClickKill);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(168, 6);
            // 
            // addPlayerToolStripMenuItem
            // 
            this.addPlayerToolStripMenuItem.Name = "addPlayerToolStripMenuItem";
            this.addPlayerToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            this.addPlayerToolStripMenuItem.Text = "Add Player";
            this.addPlayerToolStripMenuItem.Click += new System.EventHandler(this.OnAddPlayerClick);
            // 
            // setPlayerPositionToolStripMenuItem
            // 
            this.setPlayerPositionToolStripMenuItem.Name = "setPlayerPositionToolStripMenuItem";
            this.setPlayerPositionToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            this.setPlayerPositionToolStripMenuItem.Text = "Set Player Position";
            this.setPlayerPositionToolStripMenuItem.Click += new System.EventHandler(this.OnSetPlayerPosClick);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.documentationToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // documentationToolStripMenuItem
            // 
            this.documentationToolStripMenuItem.Name = "documentationToolStripMenuItem";
            this.documentationToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.documentationToolStripMenuItem.Text = "Online Documentaion";
            this.documentationToolStripMenuItem.Click += new System.EventHandler(this.OnDocOpen);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 24);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.AutoScroll = true;
            this.splitContainer1.Panel1.BackColor = System.Drawing.Color.Black;
            this.splitContainer1.Panel1.Controls.Add(this.simCanvas);
            this.splitContainer1.Panel1.SizeChanged += new System.EventHandler(this.OnResizeCanvas);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tabSimulation);
            this.splitContainer1.Size = new System.Drawing.Size(840, 692);
            this.splitContainer1.SplitterDistance = 461;
            this.splitContainer1.TabIndex = 5;
            this.splitContainer1.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.OnSplitContainerMove);
            // 
            // simCanvas
            // 
            this.simCanvas.BackColor = System.Drawing.Color.Black;
            this.simCanvas.Location = new System.Drawing.Point(147, 3);
            this.simCanvas.Name = "simCanvas";
            this.simCanvas.Size = new System.Drawing.Size(562, 448);
            this.simCanvas.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.simCanvas.TabIndex = 6;
            this.simCanvas.TabStop = false;
            this.simCanvas.MouseClick += new System.Windows.Forms.MouseEventHandler(this.OnSimCanvasClick);
            // 
            // tabSimulation
            // 
            this.tabSimulation.Controls.Add(this.tabPage1);
            this.tabSimulation.Controls.Add(this.tabPage2);
            this.tabSimulation.Controls.Add(this.tabPage4);
            this.tabSimulation.Controls.Add(this.tabPage3);
            this.tabSimulation.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabSimulation.Location = new System.Drawing.Point(0, 0);
            this.tabSimulation.Name = "tabSimulation";
            this.tabSimulation.SelectedIndex = 0;
            this.tabSimulation.Size = new System.Drawing.Size(840, 227);
            this.tabSimulation.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.tableLayoutPanel1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(832, 201);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Base Parameters";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 6;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 17.43341F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15.7385F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 18.64407F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 17.07022F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 22.76029F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8.837772F));
            this.tableLayoutPanel1.Controls.Add(this.lblPauseDuringBloodmoon, 4, 2);
            this.tableLayoutPanel1.Controls.Add(this.lblStartAgentsGrouped, 4, 1);
            this.tableLayoutPanel1.Controls.Add(this.inputSpawnProtectionTime, 1, 5);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.inputRespawnPosition, 3, 2);
            this.tableLayoutPanel1.Controls.Add(this.lblSpawnProtectionTime, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.lblRespawnPosition, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this.inputWorld, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.inputStartPosition, 3, 1);
            this.tableLayoutPanel1.Controls.Add(this.lblWorld, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.lblStartPosition, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.lblRandomSeed, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.lblGroupSize, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.inputGroupSize, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.lblPopulationDensity, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.inputStartGrouped, 5, 1);
            this.tableLayoutPanel1.Controls.Add(this.inputPauseDuringBloodmoon, 5, 2);
            this.tableLayoutPanel1.Controls.Add(this.lblFastForward, 4, 3);
            this.tableLayoutPanel1.Controls.Add(this.inputFastForward, 5, 3);
            this.tableLayoutPanel1.Controls.Add(this.inputMaxAgents, 1, 4);
            this.tableLayoutPanel1.Controls.Add(this.lblActivationRadius, 2, 3);
            this.tableLayoutPanel1.Controls.Add(this.inputActivationRadius, 3, 3);
            this.tableLayoutPanel1.Controls.Add(this.lblEnhancedSoundAwareness, 4, 4);
            this.tableLayoutPanel1.Controls.Add(this.inputSoundAware, 5, 4);
            this.tableLayoutPanel1.Controls.Add(this.labelWithHelp1, 2, 4);
            this.tableLayoutPanel1.Controls.Add(this.inputSoundDistanceScale, 3, 4);
            this.tableLayoutPanel1.Controls.Add(this.labelWithHelp2, 2, 5);
            this.tableLayoutPanel1.Controls.Add(this.inputMaxSpawnedZombies, 3, 5);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 7;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 5F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(826, 195);
            this.tableLayoutPanel1.TabIndex = 47;
            // 
            // lblPauseDuringBloodmoon
            // 
            this.lblPauseDuringBloodmoon.AutoSize = true;
            this.lblPauseDuringBloodmoon.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.lblPauseDuringBloodmoon.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblPauseDuringBloodmoon.HelpUrl = "https://7dtd-walkersim2.readthedocs.io/<version>/configuring/configuration/base.h" +
    "tml#pauseduringbloodmoon";
            this.lblPauseDuringBloodmoon.LabelText = "Pause during Bloodmoon";
            this.lblPauseDuringBloodmoon.Location = new System.Drawing.Point(581, 36);
            this.lblPauseDuringBloodmoon.Margin = new System.Windows.Forms.Padding(16, 1, 0, 2);
            this.lblPauseDuringBloodmoon.Name = "lblPauseDuringBloodmoon";
            this.lblPauseDuringBloodmoon.Padding = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.lblPauseDuringBloodmoon.Size = new System.Drawing.Size(171, 27);
            this.lblPauseDuringBloodmoon.TabIndex = 50;
            // 
            // lblStartAgentsGrouped
            // 
            this.lblStartAgentsGrouped.AutoSize = true;
            this.lblStartAgentsGrouped.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.lblStartAgentsGrouped.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblStartAgentsGrouped.HelpUrl = "https://7dtd-walkersim2.readthedocs.io/<version>/configuring/configuration/base.h" +
    "tml#startagentsgrouped";
            this.lblStartAgentsGrouped.LabelText = "Start Agents Grouped";
            this.lblStartAgentsGrouped.Location = new System.Drawing.Point(581, 6);
            this.lblStartAgentsGrouped.Margin = new System.Windows.Forms.Padding(16, 1, 0, 2);
            this.lblStartAgentsGrouped.Name = "lblStartAgentsGrouped";
            this.lblStartAgentsGrouped.Padding = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.lblStartAgentsGrouped.Size = new System.Drawing.Size(171, 27);
            this.lblStartAgentsGrouped.TabIndex = 49;
            // 
            // inputSpawnProtectionTime
            // 
            this.inputSpawnProtectionTime.Location = new System.Drawing.Point(143, 125);
            this.inputSpawnProtectionTime.Margin = new System.Windows.Forms.Padding(0);
            this.inputSpawnProtectionTime.Maximum = new decimal(new int[] {
            400,
            0,
            0,
            0});
            this.inputSpawnProtectionTime.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.inputSpawnProtectionTime.Name = "inputSpawnProtectionTime";
            this.inputSpawnProtectionTime.Size = new System.Drawing.Size(112, 20);
            this.inputSpawnProtectionTime.TabIndex = 45;
            this.inputSpawnProtectionTime.Value = new decimal(new int[] {
            300,
            0,
            0,
            0});
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.AutoSize = true;
            this.panel1.Controls.Add(this.inputRandomSeed);
            this.panel1.Controls.Add(this.btRand);
            this.panel1.Location = new System.Drawing.Point(143, 35);
            this.panel1.Margin = new System.Windows.Forms.Padding(0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(129, 20);
            this.panel1.TabIndex = 48;
            // 
            // inputRandomSeed
            // 
            this.inputRandomSeed.AutoSize = true;
            this.inputRandomSeed.Dock = System.Windows.Forms.DockStyle.Fill;
            this.inputRandomSeed.Location = new System.Drawing.Point(0, 0);
            this.inputRandomSeed.Margin = new System.Windows.Forms.Padding(0);
            this.inputRandomSeed.Maximum = new decimal(new int[] {
            -1,
            2147483647,
            0,
            0});
            this.inputRandomSeed.Name = "inputRandomSeed";
            this.inputRandomSeed.Size = new System.Drawing.Size(104, 20);
            this.inputRandomSeed.TabIndex = 28;
            // 
            // btRand
            // 
            this.btRand.AutoSize = true;
            this.btRand.Dock = System.Windows.Forms.DockStyle.Right;
            this.btRand.Location = new System.Drawing.Point(104, 0);
            this.btRand.Margin = new System.Windows.Forms.Padding(0);
            this.btRand.Name = "btRand";
            this.btRand.Size = new System.Drawing.Size(25, 20);
            this.btRand.TabIndex = 43;
            this.btRand.Text = "R";
            this.btRand.UseVisualStyleBackColor = true;
            this.btRand.Click += new System.EventHandler(this.OnRandSeedClick);
            // 
            // inputRespawnPosition
            // 
            this.inputRespawnPosition.Dock = System.Windows.Forms.DockStyle.Fill;
            this.inputRespawnPosition.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.inputRespawnPosition.FormattingEnabled = true;
            this.inputRespawnPosition.Location = new System.Drawing.Point(425, 35);
            this.inputRespawnPosition.Margin = new System.Windows.Forms.Padding(0);
            this.inputRespawnPosition.Name = "inputRespawnPosition";
            this.inputRespawnPosition.Size = new System.Drawing.Size(140, 21);
            this.inputRespawnPosition.TabIndex = 37;
            // 
            // lblSpawnProtectionTime
            // 
            this.lblSpawnProtectionTime.AutoSize = true;
            this.lblSpawnProtectionTime.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.lblSpawnProtectionTime.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSpawnProtectionTime.HelpUrl = "https://7dtd-walkersim2.readthedocs.io/<version>/configuring/configuration/base.h" +
    "tml#spawnprotectiontime";
            this.lblSpawnProtectionTime.LabelText = "Spawn Protection Time";
            this.lblSpawnProtectionTime.Location = new System.Drawing.Point(0, 126);
            this.lblSpawnProtectionTime.Margin = new System.Windows.Forms.Padding(0, 1, 0, 0);
            this.lblSpawnProtectionTime.Name = "lblSpawnProtectionTime";
            this.lblSpawnProtectionTime.Padding = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.lblSpawnProtectionTime.Size = new System.Drawing.Size(143, 29);
            this.lblSpawnProtectionTime.TabIndex = 46;
            // 
            // lblRespawnPosition
            // 
            this.lblRespawnPosition.AutoSize = true;
            this.lblRespawnPosition.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.lblRespawnPosition.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblRespawnPosition.HelpUrl = "https://7dtd-walkersim2.readthedocs.io/<version>/configuring/configuration/base.h" +
    "tml#agentrespawnposition";
            this.lblRespawnPosition.LabelText = "Respawn Position";
            this.lblRespawnPosition.Location = new System.Drawing.Point(288, 36);
            this.lblRespawnPosition.Margin = new System.Windows.Forms.Padding(16, 1, 0, 2);
            this.lblRespawnPosition.Name = "lblRespawnPosition";
            this.lblRespawnPosition.Padding = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.lblRespawnPosition.Size = new System.Drawing.Size(137, 27);
            this.lblRespawnPosition.TabIndex = 36;
            // 
            // inputWorld
            // 
            this.inputWorld.Dock = System.Windows.Forms.DockStyle.Fill;
            this.inputWorld.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.inputWorld.FormattingEnabled = true;
            this.inputWorld.Location = new System.Drawing.Point(143, 5);
            this.inputWorld.Margin = new System.Windows.Forms.Padding(0);
            this.inputWorld.Name = "inputWorld";
            this.inputWorld.Size = new System.Drawing.Size(129, 21);
            this.inputWorld.TabIndex = 41;
            this.inputWorld.SelectedIndexChanged += new System.EventHandler(this.OnWorldSelectionChanged);
            // 
            // inputStartPosition
            // 
            this.inputStartPosition.Dock = System.Windows.Forms.DockStyle.Fill;
            this.inputStartPosition.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.inputStartPosition.FormattingEnabled = true;
            this.inputStartPosition.Location = new System.Drawing.Point(425, 5);
            this.inputStartPosition.Margin = new System.Windows.Forms.Padding(0);
            this.inputStartPosition.Name = "inputStartPosition";
            this.inputStartPosition.Size = new System.Drawing.Size(140, 21);
            this.inputStartPosition.TabIndex = 35;
            // 
            // lblWorld
            // 
            this.lblWorld.AutoSize = true;
            this.lblWorld.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.lblWorld.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblWorld.HelpUrl = "https://7dtd-walkersim2.readthedocs.io/<version>/configuring/configuration/base.h" +
    "tml#startagentsgrouped";
            this.lblWorld.LabelText = "World";
            this.lblWorld.Location = new System.Drawing.Point(0, 6);
            this.lblWorld.Margin = new System.Windows.Forms.Padding(0, 1, 0, 0);
            this.lblWorld.Name = "lblWorld";
            this.lblWorld.Padding = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.lblWorld.Size = new System.Drawing.Size(143, 29);
            this.lblWorld.TabIndex = 40;
            // 
            // lblStartPosition
            // 
            this.lblStartPosition.AutoSize = true;
            this.lblStartPosition.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.lblStartPosition.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblStartPosition.HelpUrl = "https://7dtd-walkersim2.readthedocs.io/<version>/configuring/configuration/base.h" +
    "tml#agentstartposition";
            this.lblStartPosition.LabelText = "Start Position";
            this.lblStartPosition.Location = new System.Drawing.Point(288, 6);
            this.lblStartPosition.Margin = new System.Windows.Forms.Padding(16, 1, 0, 2);
            this.lblStartPosition.Name = "lblStartPosition";
            this.lblStartPosition.Padding = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.lblStartPosition.Size = new System.Drawing.Size(137, 27);
            this.lblStartPosition.TabIndex = 34;
            // 
            // lblRandomSeed
            // 
            this.lblRandomSeed.AutoSize = true;
            this.lblRandomSeed.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.lblRandomSeed.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblRandomSeed.HelpUrl = "https://7dtd-walkersim2.readthedocs.io/<version>/configuring/configuration/base.h" +
    "tml#randomseed";
            this.lblRandomSeed.LabelText = "Random Seed";
            this.lblRandomSeed.Location = new System.Drawing.Point(0, 36);
            this.lblRandomSeed.Margin = new System.Windows.Forms.Padding(0, 1, 0, 0);
            this.lblRandomSeed.Name = "lblRandomSeed";
            this.lblRandomSeed.Padding = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.lblRandomSeed.Size = new System.Drawing.Size(143, 29);
            this.lblRandomSeed.TabIndex = 29;
            // 
            // lblGroupSize
            // 
            this.lblGroupSize.AutoSize = true;
            this.lblGroupSize.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.lblGroupSize.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblGroupSize.HelpUrl = "https://7dtd-walkersim2.readthedocs.io/<version>/configuring/configuration/base.h" +
    "tml";
            this.lblGroupSize.LabelText = "Group Size";
            this.lblGroupSize.Location = new System.Drawing.Point(0, 66);
            this.lblGroupSize.Margin = new System.Windows.Forms.Padding(0, 1, 0, 0);
            this.lblGroupSize.Name = "lblGroupSize";
            this.lblGroupSize.Padding = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.lblGroupSize.Size = new System.Drawing.Size(143, 29);
            this.lblGroupSize.TabIndex = 33;
            // 
            // inputGroupSize
            // 
            this.inputGroupSize.Dock = System.Windows.Forms.DockStyle.Fill;
            this.inputGroupSize.Location = new System.Drawing.Point(143, 65);
            this.inputGroupSize.Margin = new System.Windows.Forms.Padding(0);
            this.inputGroupSize.Maximum = new decimal(new int[] {
            30000,
            0,
            0,
            0});
            this.inputGroupSize.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.inputGroupSize.Name = "inputGroupSize";
            this.inputGroupSize.Size = new System.Drawing.Size(129, 20);
            this.inputGroupSize.TabIndex = 32;
            this.inputGroupSize.Value = new decimal(new int[] {
            16,
            0,
            0,
            0});
            // 
            // lblPopulationDensity
            // 
            this.lblPopulationDensity.AutoSize = true;
            this.lblPopulationDensity.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.lblPopulationDensity.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblPopulationDensity.HelpUrl = "https://7dtd-walkersim2.readthedocs.io/<version>/configuring/configuration/base.h" +
    "tml#populationdensity";
            this.lblPopulationDensity.LabelText = "Population Density";
            this.lblPopulationDensity.Location = new System.Drawing.Point(0, 96);
            this.lblPopulationDensity.Margin = new System.Windows.Forms.Padding(0, 1, 0, 0);
            this.lblPopulationDensity.Name = "lblPopulationDensity";
            this.lblPopulationDensity.Padding = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.lblPopulationDensity.Size = new System.Drawing.Size(143, 29);
            this.lblPopulationDensity.TabIndex = 31;
            // 
            // inputStartGrouped
            // 
            this.inputStartGrouped.AutoSize = true;
            this.inputStartGrouped.Location = new System.Drawing.Point(755, 8);
            this.inputStartGrouped.Name = "inputStartGrouped";
            this.inputStartGrouped.Size = new System.Drawing.Size(15, 14);
            this.inputStartGrouped.TabIndex = 27;
            this.inputStartGrouped.UseVisualStyleBackColor = true;
            // 
            // inputPauseDuringBloodmoon
            // 
            this.inputPauseDuringBloodmoon.AutoSize = true;
            this.inputPauseDuringBloodmoon.Location = new System.Drawing.Point(755, 38);
            this.inputPauseDuringBloodmoon.Name = "inputPauseDuringBloodmoon";
            this.inputPauseDuringBloodmoon.Size = new System.Drawing.Size(15, 14);
            this.inputPauseDuringBloodmoon.TabIndex = 39;
            this.inputPauseDuringBloodmoon.UseVisualStyleBackColor = true;
            // 
            // lblFastForward
            // 
            this.lblFastForward.AutoSize = true;
            this.lblFastForward.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.lblFastForward.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblFastForward.HelpUrl = "https://7dtd-walkersim2.readthedocs.io/<version>/configuring/configuration/base.h" +
    "tml#fastforwardatstart";
            this.lblFastForward.LabelText = "Fast forward at start";
            this.lblFastForward.Location = new System.Drawing.Point(581, 66);
            this.lblFastForward.Margin = new System.Windows.Forms.Padding(16, 1, 0, 2);
            this.lblFastForward.Name = "lblFastForward";
            this.lblFastForward.Padding = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.lblFastForward.Size = new System.Drawing.Size(171, 27);
            this.lblFastForward.TabIndex = 51;
            // 
            // inputFastForward
            // 
            this.inputFastForward.AutoSize = true;
            this.inputFastForward.Location = new System.Drawing.Point(755, 68);
            this.inputFastForward.Name = "inputFastForward";
            this.inputFastForward.Size = new System.Drawing.Size(15, 14);
            this.inputFastForward.TabIndex = 44;
            this.inputFastForward.UseVisualStyleBackColor = true;
            // 
            // inputMaxAgents
            // 
            this.inputMaxAgents.AutoSize = true;
            this.inputMaxAgents.Dock = System.Windows.Forms.DockStyle.Fill;
            this.inputMaxAgents.Location = new System.Drawing.Point(143, 95);
            this.inputMaxAgents.Margin = new System.Windows.Forms.Padding(0);
            this.inputMaxAgents.Maximum = new decimal(new int[] {
            400,
            0,
            0,
            0});
            this.inputMaxAgents.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.inputMaxAgents.Name = "inputMaxAgents";
            this.inputMaxAgents.Size = new System.Drawing.Size(129, 20);
            this.inputMaxAgents.TabIndex = 52;
            this.inputMaxAgents.Value = new decimal(new int[] {
            300,
            0,
            0,
            0});
            // 
            // lblActivationRadius
            // 
            this.lblActivationRadius.AutoSize = true;
            this.lblActivationRadius.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.lblActivationRadius.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblActivationRadius.HelpUrl = "https://7dtd-walkersim2.readthedocs.io/<version>/configuring/configuration/base.h" +
    "tml#spawnactivationradius";
            this.lblActivationRadius.LabelText = "Activation Radius";
            this.lblActivationRadius.Location = new System.Drawing.Point(288, 66);
            this.lblActivationRadius.Margin = new System.Windows.Forms.Padding(16, 1, 0, 2);
            this.lblActivationRadius.Name = "lblActivationRadius";
            this.lblActivationRadius.Padding = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.lblActivationRadius.Size = new System.Drawing.Size(137, 27);
            this.lblActivationRadius.TabIndex = 53;
            // 
            // inputActivationRadius
            // 
            this.inputActivationRadius.Dock = System.Windows.Forms.DockStyle.Fill;
            this.inputActivationRadius.Increment = new decimal(new int[] {
            8,
            0,
            0,
            0});
            this.inputActivationRadius.Location = new System.Drawing.Point(425, 65);
            this.inputActivationRadius.Margin = new System.Windows.Forms.Padding(0);
            this.inputActivationRadius.Maximum = new decimal(new int[] {
            196,
            0,
            0,
            0});
            this.inputActivationRadius.Minimum = new decimal(new int[] {
            48,
            0,
            0,
            0});
            this.inputActivationRadius.Name = "inputActivationRadius";
            this.inputActivationRadius.Size = new System.Drawing.Size(140, 20);
            this.inputActivationRadius.TabIndex = 54;
            this.inputActivationRadius.Value = new decimal(new int[] {
            96,
            0,
            0,
            0});
            // 
            // lblEnhancedSoundAwareness
            // 
            this.lblEnhancedSoundAwareness.AutoSize = true;
            this.lblEnhancedSoundAwareness.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.lblEnhancedSoundAwareness.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblEnhancedSoundAwareness.HelpUrl = "https://7dtd-walkersim2.readthedocs.io/<version>/configuring/configuration/base.h" +
    "tml#enhancedsoundawareness";
            this.lblEnhancedSoundAwareness.LabelText = "Enhanced sound awareness";
            this.lblEnhancedSoundAwareness.Location = new System.Drawing.Point(581, 96);
            this.lblEnhancedSoundAwareness.Margin = new System.Windows.Forms.Padding(16, 1, 0, 2);
            this.lblEnhancedSoundAwareness.Name = "lblEnhancedSoundAwareness";
            this.lblEnhancedSoundAwareness.Padding = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.lblEnhancedSoundAwareness.Size = new System.Drawing.Size(171, 27);
            this.lblEnhancedSoundAwareness.TabIndex = 55;
            // 
            // inputSoundAware
            // 
            this.inputSoundAware.AutoSize = true;
            this.inputSoundAware.Location = new System.Drawing.Point(755, 98);
            this.inputSoundAware.Name = "inputSoundAware";
            this.inputSoundAware.Size = new System.Drawing.Size(15, 14);
            this.inputSoundAware.TabIndex = 56;
            this.inputSoundAware.UseVisualStyleBackColor = true;
            // 
            // labelWithHelp1
            // 
            this.labelWithHelp1.AutoSize = true;
            this.labelWithHelp1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.labelWithHelp1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelWithHelp1.HelpUrl = "https://7dtd-walkersim2.readthedocs.io/<version>/configuring/configuration/base.h" +
    "tml#sounddistancescale";
            this.labelWithHelp1.LabelText = "Sound Distance Scale";
            this.labelWithHelp1.Location = new System.Drawing.Point(288, 96);
            this.labelWithHelp1.Margin = new System.Windows.Forms.Padding(16, 1, 0, 2);
            this.labelWithHelp1.Name = "labelWithHelp1";
            this.labelWithHelp1.Padding = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.labelWithHelp1.Size = new System.Drawing.Size(137, 27);
            this.labelWithHelp1.TabIndex = 57;
            // 
            // inputSoundDistanceScale
            // 
            this.inputSoundDistanceScale.DecimalPlaces = 1;
            this.inputSoundDistanceScale.Dock = System.Windows.Forms.DockStyle.Fill;
            this.inputSoundDistanceScale.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.inputSoundDistanceScale.Location = new System.Drawing.Point(425, 95);
            this.inputSoundDistanceScale.Margin = new System.Windows.Forms.Padding(0);
            this.inputSoundDistanceScale.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.inputSoundDistanceScale.Name = "inputSoundDistanceScale";
            this.inputSoundDistanceScale.Size = new System.Drawing.Size(140, 20);
            this.inputSoundDistanceScale.TabIndex = 58;
            this.inputSoundDistanceScale.Value = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            // 
            // labelWithHelp2
            // 
            this.labelWithHelp2.AutoSize = true;
            this.labelWithHelp2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.labelWithHelp2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelWithHelp2.HelpUrl = "https://7dtd-walkersim2.readthedocs.io/<version>/configuring/configuration/base.h" +
    "tml#maxspawnedzombies";
            this.labelWithHelp2.LabelText = "Max Spawned Zombies";
            this.labelWithHelp2.Location = new System.Drawing.Point(288, 126);
            this.labelWithHelp2.Margin = new System.Windows.Forms.Padding(16, 1, 0, 2);
            this.labelWithHelp2.Name = "labelWithHelp2";
            this.labelWithHelp2.Padding = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.labelWithHelp2.Size = new System.Drawing.Size(137, 27);
            this.labelWithHelp2.TabIndex = 59;
            // 
            // inputMaxSpawnedZombies
            // 
            this.inputMaxSpawnedZombies.Dock = System.Windows.Forms.DockStyle.Fill;
            this.inputMaxSpawnedZombies.Location = new System.Drawing.Point(425, 125);
            this.inputMaxSpawnedZombies.Margin = new System.Windows.Forms.Padding(0);
            this.inputMaxSpawnedZombies.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.inputMaxSpawnedZombies.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.inputMaxSpawnedZombies.Name = "inputMaxSpawnedZombies";
            this.inputMaxSpawnedZombies.Size = new System.Drawing.Size(140, 20);
            this.inputMaxSpawnedZombies.TabIndex = 60;
            this.inputMaxSpawnedZombies.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.tableLayoutPanel2);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(832, 201);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Movement Systems";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 3;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 27.18447F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.98058F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 38.83495F));
            this.tableLayoutPanel2.Controls.Add(this.groupBox1, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.groupProcessors, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.groupProps, 1, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(826, 195);
            this.tableLayoutPanel2.TabIndex = 9;
            // 
            // groupBox1
            // 
            this.groupBox1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.groupBox1.Controls.Add(this.tableLayoutPanel3);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(218, 189);
            this.groupBox1.TabIndex = 8;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Systems";
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 1;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Controls.Add(this.listProcessorGroups, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.tableLayoutPanel4, 0, 1);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 16);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 2;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.Size = new System.Drawing.Size(212, 170);
            this.tableLayoutPanel3.TabIndex = 12;
            // 
            // listProcessorGroups
            // 
            this.listProcessorGroups.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listProcessorGroups.FormattingEnabled = true;
            this.listProcessorGroups.Location = new System.Drawing.Point(3, 8);
            this.listProcessorGroups.Margin = new System.Windows.Forms.Padding(3, 8, 3, 3);
            this.listProcessorGroups.Name = "listProcessorGroups";
            this.listProcessorGroups.Size = new System.Drawing.Size(206, 127);
            this.listProcessorGroups.TabIndex = 8;
            this.listProcessorGroups.SelectedIndexChanged += new System.EventHandler(this.OnGroupSelection);
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 3;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
            this.tableLayoutPanel4.Controls.Add(this.button1, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.buttonRemoveGroup, 2, 0);
            this.tableLayoutPanel4.Controls.Add(this.buttonDuplicateGroup, 1, 0);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(0, 138);
            this.tableLayoutPanel4.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 1;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel4.Size = new System.Drawing.Size(212, 32);
            this.tableLayoutPanel4.TabIndex = 9;
            // 
            // button1
            // 
            this.button1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.button1.Location = new System.Drawing.Point(3, 3);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(64, 26);
            this.button1.TabIndex = 9;
            this.button1.Text = "Add";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.OnAddGroupClick);
            // 
            // buttonRemoveGroup
            // 
            this.buttonRemoveGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonRemoveGroup.Enabled = false;
            this.buttonRemoveGroup.Location = new System.Drawing.Point(143, 3);
            this.buttonRemoveGroup.Name = "buttonRemoveGroup";
            this.buttonRemoveGroup.Size = new System.Drawing.Size(66, 26);
            this.buttonRemoveGroup.TabIndex = 10;
            this.buttonRemoveGroup.Text = "Remove";
            this.buttonRemoveGroup.UseVisualStyleBackColor = true;
            this.buttonRemoveGroup.Click += new System.EventHandler(this.OnRemoveGroupClick);
            // 
            // buttonDuplicateGroup
            // 
            this.buttonDuplicateGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonDuplicateGroup.Enabled = false;
            this.buttonDuplicateGroup.Location = new System.Drawing.Point(73, 3);
            this.buttonDuplicateGroup.Name = "buttonDuplicateGroup";
            this.buttonDuplicateGroup.Size = new System.Drawing.Size(64, 26);
            this.buttonDuplicateGroup.TabIndex = 11;
            this.buttonDuplicateGroup.Text = "Duplicate";
            this.buttonDuplicateGroup.UseVisualStyleBackColor = true;
            this.buttonDuplicateGroup.Click += new System.EventHandler(this.OnDuplicateGroupClick);
            // 
            // groupProcessors
            // 
            this.groupProcessors.Controls.Add(this.tableLayoutPanel6);
            this.groupProcessors.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupProcessors.Location = new System.Drawing.Point(507, 3);
            this.groupProcessors.Name = "groupProcessors";
            this.groupProcessors.Size = new System.Drawing.Size(316, 189);
            this.groupProcessors.TabIndex = 6;
            this.groupProcessors.TabStop = false;
            this.groupProcessors.Text = "Processors";
            this.groupProcessors.Visible = false;
            // 
            // tableLayoutPanel6
            // 
            this.tableLayoutPanel6.ColumnCount = 2;
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 51.45889F));
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 48.54111F));
            this.tableLayoutPanel6.Controls.Add(this.tableLayoutPanel7, 0, 0);
            this.tableLayoutPanel6.Controls.Add(this.groupParameter, 1, 0);
            this.tableLayoutPanel6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel6.Location = new System.Drawing.Point(3, 16);
            this.tableLayoutPanel6.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.tableLayoutPanel6.Name = "tableLayoutPanel6";
            this.tableLayoutPanel6.RowCount = 1;
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel6.Size = new System.Drawing.Size(310, 170);
            this.tableLayoutPanel6.TabIndex = 11;
            // 
            // tableLayoutPanel7
            // 
            this.tableLayoutPanel7.ColumnCount = 1;
            this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel7.Controls.Add(this.tableLayoutPanel8, 0, 1);
            this.tableLayoutPanel7.Controls.Add(this.listProcessors, 0, 0);
            this.tableLayoutPanel7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel7.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel7.Name = "tableLayoutPanel7";
            this.tableLayoutPanel7.RowCount = 2;
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel7.Size = new System.Drawing.Size(153, 164);
            this.tableLayoutPanel7.TabIndex = 12;
            // 
            // tableLayoutPanel8
            // 
            this.tableLayoutPanel8.ColumnCount = 2;
            this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel8.Controls.Add(this.button4, 0, 0);
            this.tableLayoutPanel8.Controls.Add(this.buttonRemoveProcessor, 1, 0);
            this.tableLayoutPanel8.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel8.Location = new System.Drawing.Point(0, 132);
            this.tableLayoutPanel8.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel8.Name = "tableLayoutPanel8";
            this.tableLayoutPanel8.RowCount = 1;
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel8.Size = new System.Drawing.Size(153, 32);
            this.tableLayoutPanel8.TabIndex = 13;
            // 
            // button4
            // 
            this.button4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.button4.Location = new System.Drawing.Point(3, 3);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(70, 26);
            this.button4.TabIndex = 8;
            this.button4.Text = "Add";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.OnAddProcessorClick);
            // 
            // buttonRemoveProcessor
            // 
            this.buttonRemoveProcessor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonRemoveProcessor.Enabled = false;
            this.buttonRemoveProcessor.Location = new System.Drawing.Point(79, 3);
            this.buttonRemoveProcessor.Name = "buttonRemoveProcessor";
            this.buttonRemoveProcessor.Size = new System.Drawing.Size(71, 26);
            this.buttonRemoveProcessor.TabIndex = 9;
            this.buttonRemoveProcessor.Text = "Remove";
            this.buttonRemoveProcessor.UseVisualStyleBackColor = true;
            this.buttonRemoveProcessor.Click += new System.EventHandler(this.OnRemoveProcessorClick);
            // 
            // listProcessors
            // 
            this.listProcessors.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listProcessors.FormattingEnabled = true;
            this.listProcessors.Location = new System.Drawing.Point(3, 3);
            this.listProcessors.Margin = new System.Windows.Forms.Padding(3, 3, 3, 2);
            this.listProcessors.Name = "listProcessors";
            this.listProcessors.Size = new System.Drawing.Size(147, 127);
            this.listProcessors.TabIndex = 7;
            this.listProcessors.SelectedIndexChanged += new System.EventHandler(this.OnProcessorSelectionChanged);
            // 
            // groupParameter
            // 
            this.groupParameter.Controls.Add(this.tableLayoutPanel9);
            this.groupParameter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupParameter.Location = new System.Drawing.Point(162, 0);
            this.groupParameter.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.groupParameter.Name = "groupParameter";
            this.groupParameter.Size = new System.Drawing.Size(145, 167);
            this.groupParameter.TabIndex = 10;
            this.groupParameter.TabStop = false;
            this.groupParameter.Text = "Parameters";
            this.groupParameter.Visible = false;
            // 
            // tableLayoutPanel9
            // 
            this.tableLayoutPanel9.ColumnCount = 2;
            this.tableLayoutPanel9.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel9.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel9.Controls.Add(this.inputProcessorPower, 1, 1);
            this.tableLayoutPanel9.Controls.Add(this.label9, 0, 0);
            this.tableLayoutPanel9.Controls.Add(this.label8, 0, 1);
            this.tableLayoutPanel9.Controls.Add(this.inputProcessorDistance, 1, 0);
            this.tableLayoutPanel9.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel9.Location = new System.Drawing.Point(3, 16);
            this.tableLayoutPanel9.Name = "tableLayoutPanel9";
            this.tableLayoutPanel9.RowCount = 2;
            this.tableLayoutPanel9.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel9.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel9.Size = new System.Drawing.Size(139, 148);
            this.tableLayoutPanel9.TabIndex = 12;
            // 
            // inputProcessorPower
            // 
            this.inputProcessorPower.DecimalPlaces = 5;
            this.inputProcessorPower.Increment = new decimal(new int[] {
            1,
            0,
            0,
            327680});
            this.inputProcessorPower.Location = new System.Drawing.Point(72, 29);
            this.inputProcessorPower.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.inputProcessorPower.Name = "inputProcessorPower";
            this.inputProcessorPower.Size = new System.Drawing.Size(64, 20);
            this.inputProcessorPower.TabIndex = 12;
            this.inputProcessorPower.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.inputProcessorPower.ValueChanged += new System.EventHandler(this.OnPowerValueChanged);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(3, 8);
            this.label9.Margin = new System.Windows.Forms.Padding(3, 8, 3, 3);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(49, 13);
            this.label9.TabIndex = 9;
            this.label9.Text = "Distance";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(3, 34);
            this.label8.Margin = new System.Windows.Forms.Padding(3, 8, 3, 3);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(37, 13);
            this.label8.TabIndex = 10;
            this.label8.Text = "Power";
            // 
            // inputProcessorDistance
            // 
            this.inputProcessorDistance.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.inputProcessorDistance.Location = new System.Drawing.Point(72, 3);
            this.inputProcessorDistance.Maximum = new decimal(new int[] {
            750,
            0,
            0,
            0});
            this.inputProcessorDistance.Name = "inputProcessorDistance";
            this.inputProcessorDistance.Size = new System.Drawing.Size(64, 20);
            this.inputProcessorDistance.TabIndex = 11;
            this.inputProcessorDistance.ValueChanged += new System.EventHandler(this.OnDistanceValueChanged);
            // 
            // groupProps
            // 
            this.groupProps.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.groupProps.Controls.Add(this.tableLayoutPanel5);
            this.groupProps.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupProps.Location = new System.Drawing.Point(227, 3);
            this.groupProps.Name = "groupProps";
            this.groupProps.Size = new System.Drawing.Size(274, 189);
            this.groupProps.TabIndex = 3;
            this.groupProps.TabStop = false;
            this.groupProps.Text = "Properties";
            this.groupProps.Visible = false;
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.ColumnCount = 2;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60.8209F));
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 39.1791F));
            this.tableLayoutPanel5.Controls.Add(this.panel2, 1, 2);
            this.tableLayoutPanel5.Controls.Add(this.inputPostSpawnBehavior, 1, 3);
            this.tableLayoutPanel5.Controls.Add(this.inputAffectedGroup, 1, 0);
            this.tableLayoutPanel5.Controls.Add(this.lblPostSpawnWanderSpeed, 0, 4);
            this.tableLayoutPanel5.Controls.Add(this.lblMovementSpeed, 0, 1);
            this.tableLayoutPanel5.Controls.Add(this.inputMovementSpeed, 1, 1);
            this.tableLayoutPanel5.Controls.Add(this.lblAffected, 0, 5);
            this.tableLayoutPanel5.Controls.Add(this.lblGroupColor, 0, 2);
            this.tableLayoutPanel5.Controls.Add(this.lblPostSpawnBehavior, 0, 3);
            this.tableLayoutPanel5.Controls.Add(this.inputWanderSpeed, 1, 4);
            this.tableLayoutPanel5.Controls.Add(this.lblAffectedGroup, 0, 0);
            this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel5.Location = new System.Drawing.Point(3, 16);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 6;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel5.Size = new System.Drawing.Size(268, 170);
            this.tableLayoutPanel5.TabIndex = 14;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.boxGroupColor);
            this.panel2.Controls.Add(this.buttonGroupColor);
            this.panel2.Location = new System.Drawing.Point(163, 53);
            this.panel2.Margin = new System.Windows.Forms.Padding(0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(105, 28);
            this.panel2.TabIndex = 15;
            // 
            // boxGroupColor
            // 
            this.boxGroupColor.BackColor = System.Drawing.Color.Transparent;
            this.boxGroupColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.boxGroupColor.Location = new System.Drawing.Point(3, 3);
            this.boxGroupColor.Name = "boxGroupColor";
            this.boxGroupColor.Size = new System.Drawing.Size(77, 21);
            this.boxGroupColor.TabIndex = 8;
            this.boxGroupColor.TabStop = false;
            // 
            // buttonGroupColor
            // 
            this.buttonGroupColor.Location = new System.Drawing.Point(80, 2);
            this.buttonGroupColor.Name = "buttonGroupColor";
            this.buttonGroupColor.Size = new System.Drawing.Size(23, 23);
            this.buttonGroupColor.TabIndex = 7;
            this.buttonGroupColor.Text = "...";
            this.buttonGroupColor.UseVisualStyleBackColor = true;
            this.buttonGroupColor.Click += new System.EventHandler(this.OnGroupColorPickClick);
            // 
            // inputPostSpawnBehavior
            // 
            this.inputPostSpawnBehavior.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.inputPostSpawnBehavior.FormattingEnabled = true;
            this.inputPostSpawnBehavior.Location = new System.Drawing.Point(166, 84);
            this.inputPostSpawnBehavior.Name = "inputPostSpawnBehavior";
            this.inputPostSpawnBehavior.Size = new System.Drawing.Size(99, 21);
            this.inputPostSpawnBehavior.TabIndex = 11;
            this.inputPostSpawnBehavior.SelectedIndexChanged += new System.EventHandler(this.OnPostSpawnBehaviorSelectionChanged);
            // 
            // inputAffectedGroup
            // 
            this.inputAffectedGroup.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.inputAffectedGroup.FormattingEnabled = true;
            this.inputAffectedGroup.Location = new System.Drawing.Point(166, 3);
            this.inputAffectedGroup.Name = "inputAffectedGroup";
            this.inputAffectedGroup.Size = new System.Drawing.Size(99, 21);
            this.inputAffectedGroup.TabIndex = 12;
            this.inputAffectedGroup.SelectionChangeCommitted += new System.EventHandler(this.OnGroupIdChanged);
            // 
            // lblPostSpawnWanderSpeed
            // 
            this.lblPostSpawnWanderSpeed.AutoSize = true;
            this.lblPostSpawnWanderSpeed.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.lblPostSpawnWanderSpeed.HelpUrl = "https://7dtd-walkersim2.readthedocs.io/<version>/configuring/configuration/system" +
    "s.html#postspawnwanderspeed";
            this.lblPostSpawnWanderSpeed.LabelText = "Post Spawn Wander Speed";
            this.lblPostSpawnWanderSpeed.Location = new System.Drawing.Point(3, 116);
            this.lblPostSpawnWanderSpeed.Margin = new System.Windows.Forms.Padding(3, 8, 3, 3);
            this.lblPostSpawnWanderSpeed.Name = "lblPostSpawnWanderSpeed";
            this.lblPostSpawnWanderSpeed.Padding = new System.Windows.Forms.Padding(4, 0, 0, 2);
            this.lblPostSpawnWanderSpeed.Size = new System.Drawing.Size(157, 15);
            this.lblPostSpawnWanderSpeed.TabIndex = 13;
            // 
            // lblMovementSpeed
            // 
            this.lblMovementSpeed.AutoSize = true;
            this.lblMovementSpeed.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.lblMovementSpeed.HelpUrl = "https://7dtd-walkersim2.readthedocs.io/<version>/configuring/configuration/system" +
    "s.html#speedscale";
            this.lblMovementSpeed.LabelText = "Movement Speed";
            this.lblMovementSpeed.Location = new System.Drawing.Point(3, 35);
            this.lblMovementSpeed.Margin = new System.Windows.Forms.Padding(3, 8, 3, 3);
            this.lblMovementSpeed.Name = "lblMovementSpeed";
            this.lblMovementSpeed.Padding = new System.Windows.Forms.Padding(4, 0, 0, 2);
            this.lblMovementSpeed.Size = new System.Drawing.Size(110, 15);
            this.lblMovementSpeed.TabIndex = 2;
            // 
            // inputMovementSpeed
            // 
            this.inputMovementSpeed.DecimalPlaces = 5;
            this.inputMovementSpeed.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.inputMovementSpeed.Location = new System.Drawing.Point(166, 30);
            this.inputMovementSpeed.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.inputMovementSpeed.Name = "inputMovementSpeed";
            this.inputMovementSpeed.Size = new System.Drawing.Size(99, 20);
            this.inputMovementSpeed.TabIndex = 5;
            this.inputMovementSpeed.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.inputMovementSpeed.ValueChanged += new System.EventHandler(this.OnMovementSpeedChanged);
            // 
            // lblAffected
            // 
            this.lblAffected.AutoSize = true;
            this.lblAffected.Location = new System.Drawing.Point(7, 143);
            this.lblAffected.Margin = new System.Windows.Forms.Padding(7, 8, 3, 0);
            this.lblAffected.Name = "lblAffected";
            this.lblAffected.Size = new System.Drawing.Size(95, 13);
            this.lblAffected.TabIndex = 9;
            this.lblAffected.Text = "Affected Agents: 0";
            // 
            // lblGroupColor
            // 
            this.lblGroupColor.AutoSize = true;
            this.lblGroupColor.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.lblGroupColor.HelpUrl = "https://7dtd-walkersim2.readthedocs.io/<version>/configuring/configuration/system" +
    "s.html#color";
            this.lblGroupColor.LabelText = "Group Color";
            this.lblGroupColor.Location = new System.Drawing.Point(3, 61);
            this.lblGroupColor.Margin = new System.Windows.Forms.Padding(3, 8, 3, 3);
            this.lblGroupColor.Name = "lblGroupColor";
            this.lblGroupColor.Padding = new System.Windows.Forms.Padding(4, 0, 0, 2);
            this.lblGroupColor.Size = new System.Drawing.Size(82, 15);
            this.lblGroupColor.TabIndex = 6;
            // 
            // lblPostSpawnBehavior
            // 
            this.lblPostSpawnBehavior.AutoSize = true;
            this.lblPostSpawnBehavior.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.lblPostSpawnBehavior.HelpUrl = "https://7dtd-walkersim2.readthedocs.io/<version>/configuring/configuration/system" +
    "s.html#postspawnbehavior";
            this.lblPostSpawnBehavior.LabelText = "Post Spawn Behavior";
            this.lblPostSpawnBehavior.Location = new System.Drawing.Point(3, 89);
            this.lblPostSpawnBehavior.Margin = new System.Windows.Forms.Padding(3, 8, 3, 3);
            this.lblPostSpawnBehavior.Name = "lblPostSpawnBehavior";
            this.lblPostSpawnBehavior.Padding = new System.Windows.Forms.Padding(4, 0, 0, 2);
            this.lblPostSpawnBehavior.Size = new System.Drawing.Size(128, 15);
            this.lblPostSpawnBehavior.TabIndex = 10;
            // 
            // inputWanderSpeed
            // 
            this.inputWanderSpeed.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.inputWanderSpeed.FormattingEnabled = true;
            this.inputWanderSpeed.Items.AddRange(new object[] {
            "No Override",
            "Walk",
            "Jog",
            "Run",
            "Sprint",
            "Nightmare"});
            this.inputWanderSpeed.Location = new System.Drawing.Point(166, 111);
            this.inputWanderSpeed.Name = "inputWanderSpeed";
            this.inputWanderSpeed.Size = new System.Drawing.Size(99, 21);
            this.inputWanderSpeed.TabIndex = 16;
            this.inputWanderSpeed.SelectedIndexChanged += new System.EventHandler(this.OnPostSpawnWanderSpeedSelectionChanged);
            // 
            // lblAffectedGroup
            // 
            this.lblAffectedGroup.AutoSize = true;
            this.lblAffectedGroup.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.lblAffectedGroup.HelpUrl = "https://7dtd-walkersim2.readthedocs.io/<version>/configuring/configuration/system" +
    "s.html#group-number";
            this.lblAffectedGroup.LabelText = "Affected Group";
            this.lblAffectedGroup.Location = new System.Drawing.Point(3, 8);
            this.lblAffectedGroup.Margin = new System.Windows.Forms.Padding(3, 8, 3, 3);
            this.lblAffectedGroup.Name = "lblAffectedGroup";
            this.lblAffectedGroup.Padding = new System.Windows.Forms.Padding(4, 0, 0, 2);
            this.lblAffectedGroup.Size = new System.Drawing.Size(98, 15);
            this.lblAffectedGroup.TabIndex = 17;
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.lblStatGroups);
            this.tabPage4.Controls.Add(this.label24);
            this.tabPage4.Controls.Add(this.lblStatUpdateTime);
            this.tabPage4.Controls.Add(this.label22);
            this.tabPage4.Controls.Add(this.lblStatWindTarget);
            this.tabPage4.Controls.Add(this.label20);
            this.tabPage4.Controls.Add(this.lblStatSimTime);
            this.tabPage4.Controls.Add(this.label27);
            this.tabPage4.Controls.Add(this.lblStatWindChange);
            this.tabPage4.Controls.Add(this.label25);
            this.tabPage4.Controls.Add(this.lblStatWindDir);
            this.tabPage4.Controls.Add(this.label23);
            this.tabPage4.Controls.Add(this.lblStatTicks);
            this.tabPage4.Controls.Add(this.label21);
            this.tabPage4.Controls.Add(this.lblStatActive);
            this.tabPage4.Controls.Add(this.label19);
            this.tabPage4.Controls.Add(this.lblStatInactive);
            this.tabPage4.Controls.Add(this.label17);
            this.tabPage4.Controls.Add(this.lblStatTotalAgents);
            this.tabPage4.Controls.Add(this.label16);
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage4.Size = new System.Drawing.Size(832, 201);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "Statistics";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // lblStatGroups
            // 
            this.lblStatGroups.AutoSize = true;
            this.lblStatGroups.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblStatGroups.Location = new System.Drawing.Point(126, 60);
            this.lblStatGroups.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.lblStatGroups.Name = "lblStatGroups";
            this.lblStatGroups.Padding = new System.Windows.Forms.Padding(2);
            this.lblStatGroups.Size = new System.Drawing.Size(17, 17);
            this.lblStatGroups.TabIndex = 19;
            this.lblStatGroups.Text = "0";
            // 
            // label24
            // 
            this.label24.AutoSize = true;
            this.label24.Location = new System.Drawing.Point(10, 60);
            this.label24.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label24.Name = "label24";
            this.label24.Padding = new System.Windows.Forms.Padding(2);
            this.label24.Size = new System.Drawing.Size(48, 17);
            this.label24.TabIndex = 18;
            this.label24.Text = "Groups:";
            // 
            // lblStatUpdateTime
            // 
            this.lblStatUpdateTime.AutoSize = true;
            this.lblStatUpdateTime.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblStatUpdateTime.Location = new System.Drawing.Point(622, 43);
            this.lblStatUpdateTime.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.lblStatUpdateTime.Name = "lblStatUpdateTime";
            this.lblStatUpdateTime.Padding = new System.Windows.Forms.Padding(2);
            this.lblStatUpdateTime.Size = new System.Drawing.Size(33, 17);
            this.lblStatUpdateTime.TabIndex = 17;
            this.lblStatUpdateTime.Text = "0 ms";
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Location = new System.Drawing.Point(522, 43);
            this.label22.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label22.Name = "label22";
            this.label22.Padding = new System.Windows.Forms.Padding(2);
            this.label22.Size = new System.Drawing.Size(75, 17);
            this.label22.TabIndex = 16;
            this.label22.Text = "Update Time:";
            // 
            // lblStatWindTarget
            // 
            this.lblStatWindTarget.AutoSize = true;
            this.lblStatWindTarget.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblStatWindTarget.Location = new System.Drawing.Point(352, 9);
            this.lblStatWindTarget.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.lblStatWindTarget.Name = "lblStatWindTarget";
            this.lblStatWindTarget.Padding = new System.Windows.Forms.Padding(2);
            this.lblStatWindTarget.Size = new System.Drawing.Size(26, 17);
            this.lblStatWindTarget.TabIndex = 15;
            this.lblStatWindTarget.Text = "0 0";
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(218, 9);
            this.label20.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label20.Name = "label20";
            this.label20.Padding = new System.Windows.Forms.Padding(2);
            this.label20.Size = new System.Drawing.Size(118, 17);
            this.label20.TabIndex = 14;
            this.label20.Text = "Wind Direction Target:";
            // 
            // lblStatSimTime
            // 
            this.lblStatSimTime.AutoSize = true;
            this.lblStatSimTime.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblStatSimTime.Location = new System.Drawing.Point(622, 26);
            this.lblStatSimTime.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.lblStatSimTime.Name = "lblStatSimTime";
            this.lblStatSimTime.Padding = new System.Windows.Forms.Padding(2);
            this.lblStatSimTime.Size = new System.Drawing.Size(33, 17);
            this.lblStatSimTime.TabIndex = 13;
            this.lblStatSimTime.Text = "0 ms";
            // 
            // label27
            // 
            this.label27.AutoSize = true;
            this.label27.Location = new System.Drawing.Point(522, 26);
            this.label27.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label27.Name = "label27";
            this.label27.Padding = new System.Windows.Forms.Padding(2);
            this.label27.Size = new System.Drawing.Size(88, 17);
            this.label27.TabIndex = 12;
            this.label27.Text = "Simulation Time:";
            // 
            // lblStatWindChange
            // 
            this.lblStatWindChange.AutoSize = true;
            this.lblStatWindChange.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblStatWindChange.Location = new System.Drawing.Point(352, 43);
            this.lblStatWindChange.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.lblStatWindChange.Name = "lblStatWindChange";
            this.lblStatWindChange.Padding = new System.Windows.Forms.Padding(2);
            this.lblStatWindChange.Size = new System.Drawing.Size(17, 17);
            this.lblStatWindChange.TabIndex = 11;
            this.lblStatWindChange.Text = "0";
            // 
            // label25
            // 
            this.label25.AutoSize = true;
            this.label25.Location = new System.Drawing.Point(218, 43);
            this.label25.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label25.Name = "label25";
            this.label25.Padding = new System.Windows.Forms.Padding(2);
            this.label25.Size = new System.Drawing.Size(101, 17);
            this.label25.TabIndex = 10;
            this.label25.Text = "Next Wind Change";
            // 
            // lblStatWindDir
            // 
            this.lblStatWindDir.AutoSize = true;
            this.lblStatWindDir.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblStatWindDir.Location = new System.Drawing.Point(352, 26);
            this.lblStatWindDir.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.lblStatWindDir.Name = "lblStatWindDir";
            this.lblStatWindDir.Padding = new System.Windows.Forms.Padding(2);
            this.lblStatWindDir.Size = new System.Drawing.Size(26, 17);
            this.lblStatWindDir.TabIndex = 9;
            this.lblStatWindDir.Text = "0 0";
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Location = new System.Drawing.Point(218, 26);
            this.label23.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label23.Name = "label23";
            this.label23.Padding = new System.Windows.Forms.Padding(2);
            this.label23.Size = new System.Drawing.Size(84, 17);
            this.label23.TabIndex = 8;
            this.label23.Text = "Wind Direction:";
            // 
            // lblStatTicks
            // 
            this.lblStatTicks.AutoSize = true;
            this.lblStatTicks.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblStatTicks.Location = new System.Drawing.Point(622, 9);
            this.lblStatTicks.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.lblStatTicks.Name = "lblStatTicks";
            this.lblStatTicks.Padding = new System.Windows.Forms.Padding(2);
            this.lblStatTicks.Size = new System.Drawing.Size(17, 17);
            this.lblStatTicks.TabIndex = 7;
            this.lblStatTicks.Text = "0";
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(522, 9);
            this.label21.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label21.Name = "label21";
            this.label21.Padding = new System.Windows.Forms.Padding(2);
            this.label21.Size = new System.Drawing.Size(40, 17);
            this.label21.TabIndex = 6;
            this.label21.Text = "Ticks:";
            // 
            // lblStatActive
            // 
            this.lblStatActive.AutoSize = true;
            this.lblStatActive.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblStatActive.Location = new System.Drawing.Point(126, 43);
            this.lblStatActive.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.lblStatActive.Name = "lblStatActive";
            this.lblStatActive.Padding = new System.Windows.Forms.Padding(2);
            this.lblStatActive.Size = new System.Drawing.Size(17, 17);
            this.lblStatActive.TabIndex = 5;
            this.lblStatActive.Text = "0";
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(10, 43);
            this.label19.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label19.Name = "label19";
            this.label19.Padding = new System.Windows.Forms.Padding(2);
            this.label19.Size = new System.Drawing.Size(80, 17);
            this.label19.TabIndex = 4;
            this.label19.Text = "Active Agents:";
            // 
            // lblStatInactive
            // 
            this.lblStatInactive.AutoSize = true;
            this.lblStatInactive.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblStatInactive.Location = new System.Drawing.Point(126, 26);
            this.lblStatInactive.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.lblStatInactive.Name = "lblStatInactive";
            this.lblStatInactive.Padding = new System.Windows.Forms.Padding(2);
            this.lblStatInactive.Size = new System.Drawing.Size(17, 17);
            this.lblStatInactive.TabIndex = 3;
            this.lblStatInactive.Text = "0";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(10, 26);
            this.label17.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label17.Name = "label17";
            this.label17.Padding = new System.Windows.Forms.Padding(2);
            this.label17.Size = new System.Drawing.Size(88, 17);
            this.label17.TabIndex = 2;
            this.label17.Text = "Inactive Agents:";
            // 
            // lblStatTotalAgents
            // 
            this.lblStatTotalAgents.AutoSize = true;
            this.lblStatTotalAgents.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblStatTotalAgents.Location = new System.Drawing.Point(126, 9);
            this.lblStatTotalAgents.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.lblStatTotalAgents.Name = "lblStatTotalAgents";
            this.lblStatTotalAgents.Padding = new System.Windows.Forms.Padding(2);
            this.lblStatTotalAgents.Size = new System.Drawing.Size(17, 17);
            this.lblStatTotalAgents.TabIndex = 1;
            this.lblStatTotalAgents.Text = "0";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(9, 9);
            this.label16.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label16.Name = "label16";
            this.label16.Padding = new System.Windows.Forms.Padding(2);
            this.label16.Size = new System.Drawing.Size(74, 17);
            this.label16.TabIndex = 0;
            this.label16.Text = "Total Agents:";
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.rtbLog);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(832, 201);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Log";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // rtbLog
            // 
            this.rtbLog.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtbLog.ContextMenuStrip = this.contextLog;
            this.rtbLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbLog.Font = new System.Drawing.Font("Lucida Console", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rtbLog.Location = new System.Drawing.Point(0, 0);
            this.rtbLog.Name = "rtbLog";
            this.rtbLog.ReadOnly = true;
            this.rtbLog.Size = new System.Drawing.Size(832, 201);
            this.rtbLog.TabIndex = 0;
            this.rtbLog.Text = "";
            // 
            // contextLog
            // 
            this.contextLog.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.clearToolStripMenuItem,
            this.copyToolStripMenuItem});
            this.contextLog.Name = "contextLog";
            this.contextLog.Size = new System.Drawing.Size(103, 48);
            // 
            // clearToolStripMenuItem
            // 
            this.clearToolStripMenuItem.Name = "clearToolStripMenuItem";
            this.clearToolStripMenuItem.Size = new System.Drawing.Size(102, 22);
            this.clearToolStripMenuItem.Text = "Clear";
            this.clearToolStripMenuItem.Click += new System.EventHandler(this.OnLogClearClick);
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(102, 22);
            this.copyToolStripMenuItem.Text = "Copy";
            // 
            // colorPickerDlg
            // 
            this.colorPickerDlg.AnyColor = true;
            // 
            // btZoomIn
            // 
            this.btZoomIn.BackColor = System.Drawing.Color.Black;
            this.btZoomIn.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.btZoomIn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btZoomIn.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btZoomIn.ForeColor = System.Drawing.Color.White;
            this.btZoomIn.Location = new System.Drawing.Point(35, 35);
            this.btZoomIn.Margin = new System.Windows.Forms.Padding(0);
            this.btZoomIn.Name = "btZoomIn";
            this.btZoomIn.Size = new System.Drawing.Size(22, 22);
            this.btZoomIn.TabIndex = 7;
            this.btZoomIn.Text = "+";
            this.btZoomIn.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btZoomIn.Click += new System.EventHandler(this.OnZoomInClick);
            // 
            // btZoomOut
            // 
            this.btZoomOut.BackColor = System.Drawing.Color.Black;
            this.btZoomOut.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.btZoomOut.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btZoomOut.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btZoomOut.ForeColor = System.Drawing.Color.White;
            this.btZoomOut.Location = new System.Drawing.Point(10, 35);
            this.btZoomOut.Margin = new System.Windows.Forms.Padding(0);
            this.btZoomOut.Name = "btZoomOut";
            this.btZoomOut.Size = new System.Drawing.Size(22, 22);
            this.btZoomOut.TabIndex = 8;
            this.btZoomOut.Text = "-";
            this.btZoomOut.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btZoomOut.Click += new System.EventHandler(this.OnZoomOutClick);
            // 
            // label29
            // 
            this.label29.BackColor = System.Drawing.Color.Black;
            this.label29.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label29.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.label29.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label29.ForeColor = System.Drawing.Color.White;
            this.label29.Location = new System.Drawing.Point(60, 35);
            this.label29.Margin = new System.Windows.Forms.Padding(0);
            this.label29.Name = "label29";
            this.label29.Size = new System.Drawing.Size(22, 22);
            this.label29.TabIndex = 9;
            this.label29.Text = "R";
            this.label29.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.label29.Click += new System.EventHandler(this.OnZoomResetClick);
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(840, 716);
            this.Controls.Add(this.label29);
            this.Controls.Add(this.btZoomOut);
            this.Controls.Add(this.btZoomIn);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.menuStrip1);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "FormMain";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.Text = "WalkerSim";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnKeyDown);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.simCanvas)).EndInit();
            this.tabSimulation.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.inputSpawnProtectionTime)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.inputRandomSeed)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.inputGroupSize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.inputMaxAgents)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.inputActivationRadius)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.inputSoundDistanceScale)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.inputMaxSpawnedZombies)).EndInit();
            this.tabPage2.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            this.groupProcessors.ResumeLayout(false);
            this.tableLayoutPanel6.ResumeLayout(false);
            this.tableLayoutPanel7.ResumeLayout(false);
            this.tableLayoutPanel8.ResumeLayout(false);
            this.groupParameter.ResumeLayout(false);
            this.tableLayoutPanel9.ResumeLayout(false);
            this.tableLayoutPanel9.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.inputProcessorPower)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.inputProcessorDistance)).EndInit();
            this.groupProps.ResumeLayout(false);
            this.tableLayoutPanel5.ResumeLayout(false);
            this.tableLayoutPanel5.PerformLayout();
            this.panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.boxGroupColor)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.inputMovementSpeed)).EndInit();
            this.tabPage4.ResumeLayout(false);
            this.tabPage4.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.contextLog.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Timer updateTimer;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem simulationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem startToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pauseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resumeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem advanceOneTickToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem emitSoundToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewRoads;
        private System.Windows.Forms.ToolStripMenuItem viewAgents;
        private System.Windows.Forms.ToolStripMenuItem viewEvents;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TabControl tabSimulation;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.CheckBox inputPauseDuringBloodmoon;
        private System.Windows.Forms.ComboBox inputRespawnPosition;
        private LabelWithHelp lblRespawnPosition;
        private System.Windows.Forms.ComboBox inputStartPosition;
        private LabelWithHelp lblGroupSize;
        private System.Windows.Forms.NumericUpDown inputGroupSize;
        private LabelWithHelp lblPopulationDensity;
        private LabelWithHelp lblRandomSeed;
        private System.Windows.Forms.NumericUpDown inputRandomSeed;
        private System.Windows.Forms.CheckBox inputStartGrouped;
        private System.Windows.Forms.GroupBox groupProps;
        private System.Windows.Forms.NumericUpDown inputMovementSpeed;
        private LabelWithHelp lblMovementSpeed;
        private System.Windows.Forms.ComboBox inputWorld;
        private LabelWithHelp lblWorld;
        private System.Windows.Forms.ToolStripMenuItem speedToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem killToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadConfigurationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem stopToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.RichTextBox rtbLog;
        private System.Windows.Forms.ContextMenuStrip contextLog;
        private System.Windows.Forms.ToolStripMenuItem clearToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private LabelWithHelp lblGroupColor;
        private System.Windows.Forms.PictureBox boxGroupColor;
        private System.Windows.Forms.Button buttonGroupColor;
        private System.Windows.Forms.ColorDialog colorPickerDlg;
        private System.Windows.Forms.ToolStripMenuItem exportConfigurationToolStripMenuItem;
        private System.Windows.Forms.Button btRand;
        private System.Windows.Forms.ToolTip toolTipGroupSize;
        private System.Windows.Forms.Label lblAffected;
        private System.Windows.Forms.ToolStripMenuItem viewPrefabs;
        private System.Windows.Forms.ToolStripMenuItem viewCities;
        private System.Windows.Forms.CheckBox inputFastForward;
        private LabelWithHelp lblSpawnProtectionTime;
        private System.Windows.Forms.NumericUpDown inputSpawnProtectionTime;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
        private LabelWithHelp lblStartPosition;
        private LabelWithHelp lblPauseDuringBloodmoon;
        private LabelWithHelp lblStartAgentsGrouped;
        private LabelWithHelp lblFastForward;
        private System.Windows.Forms.NumericUpDown inputMaxAgents;
        private LabelWithHelp lblPostSpawnBehavior;
        private System.Windows.Forms.ComboBox inputPostSpawnBehavior;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem addPlayerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem setPlayerPositionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewActiveAgents;
        private System.Windows.Forms.PictureBox simCanvas;
        private System.Windows.Forms.ToolStripMenuItem zoomSubMenu;
        private System.Windows.Forms.ToolStripMenuItem xToolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem inToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem outToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.Label lblStatTotalAgents;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label lblStatActive;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Label lblStatInactive;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label lblStatSimTime;
        private System.Windows.Forms.Label label27;
        private System.Windows.Forms.Label lblStatWindChange;
        private System.Windows.Forms.Label label25;
        private System.Windows.Forms.Label lblStatWindDir;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.Label lblStatTicks;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.Label lblStatWindTarget;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.Label lblStatUpdateTime;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.Label lblStatGroups;
        private System.Windows.Forms.Label label24;
        private LabelWithHelp lblActivationRadius;
        private System.Windows.Forms.NumericUpDown inputActivationRadius;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem resetToolStripMenuItem;
        private LabelWithHelp lblEnhancedSoundAwareness;
        private System.Windows.Forms.CheckBox inputSoundAware;
        private System.Windows.Forms.ToolStripMenuItem viewBiomes;
        private System.Windows.Forms.ComboBox inputAffectedGroup;
        private LabelWithHelp lblPostSpawnWanderSpeed;
        private System.Windows.Forms.ListBox listProcessorGroups;
        private System.Windows.Forms.Button buttonDuplicateGroup;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button buttonRemoveGroup;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.ComboBox inputWanderSpeed;
        private System.Windows.Forms.GroupBox groupProcessors;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel6;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel7;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel8;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button buttonRemoveProcessor;
        private System.Windows.Forms.ListBox listProcessors;
        private System.Windows.Forms.GroupBox groupParameter;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel9;
        private System.Windows.Forms.NumericUpDown inputProcessorPower;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.NumericUpDown inputProcessorDistance;
        private System.Windows.Forms.Label btZoomIn;
        private System.Windows.Forms.Label btZoomOut;
        private System.Windows.Forms.Label label29;
        private LabelWithHelp lblAffectedGroup;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem documentationToolStripMenuItem;
        private LabelWithHelp labelWithHelp1;
        private System.Windows.Forms.NumericUpDown inputSoundDistanceScale;
        private LabelWithHelp labelWithHelp2;
        private PercentageUpDown inputMaxSpawnedZombies;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem debugToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadStateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveStateToolStripMenuItem1;
    }
}
