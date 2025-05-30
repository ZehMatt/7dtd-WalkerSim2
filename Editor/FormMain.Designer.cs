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
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.zoomSubMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.xToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.inToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.outToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.viewRoads = new System.Windows.Forms.ToolStripMenuItem();
            this.viewAgents = new System.Windows.Forms.ToolStripMenuItem();
            this.viewActiveAgents = new System.Windows.Forms.ToolStripMenuItem();
            this.viewEvents = new System.Windows.Forms.ToolStripMenuItem();
            this.viewPrefabs = new System.Windows.Forms.ToolStripMenuItem();
            this.simulationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.startToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.pauseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resumeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.speedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.advanceOneTickToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.emitSoundToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.killToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.addPlayerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setPlayerPositionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.simCanvas = new System.Windows.Forms.PictureBox();
            this.tabSimulation = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label13 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.inputSpawnProtectionTime = new System.Windows.Forms.NumericUpDown();
            this.panel1 = new System.Windows.Forms.Panel();
            this.inputRandomSeed = new System.Windows.Forms.NumericUpDown();
            this.btRand = new System.Windows.Forms.Button();
            this.inputRespawnPosition = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.inputWorld = new System.Windows.Forms.ComboBox();
            this.inputStartPosition = new System.Windows.Forms.ComboBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.inputGroupSize = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.inputStartGrouped = new System.Windows.Forms.CheckBox();
            this.inputPauseDuringBloodmoon = new System.Windows.Forms.CheckBox();
            this.label14 = new System.Windows.Forms.Label();
            this.inputFastForward = new System.Windows.Forms.CheckBox();
            this.inputMaxAgents = new System.Windows.Forms.NumericUpDown();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.buttonDuplicateGroup = new System.Windows.Forms.Button();
            this.groupProps = new System.Windows.Forms.GroupBox();
            this.inputPostSpawnBehavior = new System.Windows.Forms.ComboBox();
            this.label15 = new System.Windows.Forms.Label();
            this.lblAffected = new System.Windows.Forms.Label();
            this.boxGroupColor = new System.Windows.Forms.PictureBox();
            this.buttonGroupColor = new System.Windows.Forms.Button();
            this.label11 = new System.Windows.Forms.Label();
            this.inputMovementSpeed = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.lblAffectedGroup = new System.Windows.Forms.Label();
            this.inputMovementGroup = new System.Windows.Forms.NumericUpDown();
            this.groupProcessors = new System.Windows.Forms.GroupBox();
            this.buttonRemoveProcessor = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.groupParameter = new System.Windows.Forms.GroupBox();
            this.inputProcessorPower = new System.Windows.Forms.NumericUpDown();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.inputProcessorDistance = new System.Windows.Forms.NumericUpDown();
            this.listProcessors = new System.Windows.Forms.ListBox();
            this.buttonRemoveGroup = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.listProcessorGroups = new System.Windows.Forms.ListBox();
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
            this.tabPage2.SuspendLayout();
            this.groupProps.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.boxGroupColor)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.inputMovementSpeed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.inputMovementGroup)).BeginInit();
            this.groupProcessors.SuspendLayout();
            this.groupParameter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.inputProcessorPower)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.inputProcessorDistance)).BeginInit();
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
            this.toolsToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(855, 24);
            this.menuStrip1.TabIndex = 4;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadConfigurationToolStripMenuItem,
            this.exportConfigurationToolStripMenuItem,
            this.toolStripMenuItem1,
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
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(184, 6);
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
            this.viewRoads,
            this.viewAgents,
            this.viewActiveAgents,
            this.viewEvents,
            this.viewPrefabs});
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
            this.viewActiveAgents.CheckOnClick = true;
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
            // simulationToolStripMenuItem
            // 
            this.simulationToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.startToolStripMenuItem,
            this.stopToolStripMenuItem,
            this.toolStripMenuItem3,
            this.pauseToolStripMenuItem,
            this.resumeToolStripMenuItem,
            this.speedToolStripMenuItem,
            this.advanceOneTickToolStripMenuItem});
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
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Location = new System.Drawing.Point(0, 24);
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
            this.splitContainer1.Size = new System.Drawing.Size(855, 716);
            this.splitContainer1.SplitterDistance = 519;
            this.splitContainer1.TabIndex = 5;
            this.splitContainer1.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.OnSplitContainerMove);
            // 
            // simCanvas
            // 
            this.simCanvas.BackColor = System.Drawing.Color.Black;
            this.simCanvas.Location = new System.Drawing.Point(142, 34);
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
            this.tabSimulation.Size = new System.Drawing.Size(855, 193);
            this.tabSimulation.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.tableLayoutPanel1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(847, 167);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Base Parameters";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 6;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66736F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66736F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66736F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66736F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66736F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66319F));
            this.tableLayoutPanel1.Controls.Add(this.label13, 4, 2);
            this.tableLayoutPanel1.Controls.Add(this.label12, 4, 1);
            this.tableLayoutPanel1.Controls.Add(this.inputSpawnProtectionTime, 1, 5);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.inputRespawnPosition, 3, 2);
            this.tableLayoutPanel1.Controls.Add(this.label6, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.label5, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this.inputWorld, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.inputStartPosition, 3, 1);
            this.tableLayoutPanel1.Controls.Add(this.label10, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.label4, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.label3, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.inputGroupSize, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.inputStartGrouped, 5, 1);
            this.tableLayoutPanel1.Controls.Add(this.inputPauseDuringBloodmoon, 5, 2);
            this.tableLayoutPanel1.Controls.Add(this.label14, 4, 3);
            this.tableLayoutPanel1.Controls.Add(this.inputFastForward, 5, 3);
            this.tableLayoutPanel1.Controls.Add(this.inputMaxAgents, 1, 4);
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
            this.tableLayoutPanel1.Size = new System.Drawing.Size(841, 161);
            this.tableLayoutPanel1.TabIndex = 47;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label13.Location = new System.Drawing.Point(563, 35);
            this.label13.Name = "label13";
            this.label13.Padding = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.label13.Size = new System.Drawing.Size(134, 30);
            this.label13.TabIndex = 50;
            this.label13.Text = "Pause during Bloodmoon";
            this.label13.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label12.Location = new System.Drawing.Point(563, 5);
            this.label12.Name = "label12";
            this.label12.Padding = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.label12.Size = new System.Drawing.Size(134, 30);
            this.label12.TabIndex = 49;
            this.label12.Text = "Start Agents Grouped";
            this.label12.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // inputSpawnProtectionTime
            // 
            this.inputSpawnProtectionTime.Location = new System.Drawing.Point(140, 125);
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
            this.inputSpawnProtectionTime.Size = new System.Drawing.Size(140, 20);
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
            this.panel1.Location = new System.Drawing.Point(140, 35);
            this.panel1.Margin = new System.Windows.Forms.Padding(0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(140, 20);
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
            this.inputRandomSeed.Size = new System.Drawing.Size(115, 20);
            this.inputRandomSeed.TabIndex = 28;
            // 
            // btRand
            // 
            this.btRand.AutoSize = true;
            this.btRand.Dock = System.Windows.Forms.DockStyle.Right;
            this.btRand.Location = new System.Drawing.Point(115, 0);
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
            this.inputRespawnPosition.Location = new System.Drawing.Point(420, 35);
            this.inputRespawnPosition.Margin = new System.Windows.Forms.Padding(0);
            this.inputRespawnPosition.Name = "inputRespawnPosition";
            this.inputRespawnPosition.Size = new System.Drawing.Size(140, 21);
            this.inputRespawnPosition.TabIndex = 37;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label6.Location = new System.Drawing.Point(0, 125);
            this.label6.Margin = new System.Windows.Forms.Padding(0);
            this.label6.Name = "label6";
            this.label6.Padding = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.label6.Size = new System.Drawing.Size(140, 30);
            this.label6.TabIndex = 46;
            this.label6.Text = "Spawn Protection Time";
            this.label6.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label5.Location = new System.Drawing.Point(283, 35);
            this.label5.Name = "label5";
            this.label5.Padding = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.label5.Size = new System.Drawing.Size(134, 30);
            this.label5.TabIndex = 36;
            this.label5.Text = "Respawn Position";
            this.label5.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // inputWorld
            // 
            this.inputWorld.Dock = System.Windows.Forms.DockStyle.Fill;
            this.inputWorld.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.inputWorld.FormattingEnabled = true;
            this.inputWorld.Location = new System.Drawing.Point(140, 5);
            this.inputWorld.Margin = new System.Windows.Forms.Padding(0);
            this.inputWorld.Name = "inputWorld";
            this.inputWorld.Size = new System.Drawing.Size(140, 21);
            this.inputWorld.TabIndex = 41;
            this.inputWorld.SelectedIndexChanged += new System.EventHandler(this.OnWorldSelectionChanged);
            // 
            // inputStartPosition
            // 
            this.inputStartPosition.Dock = System.Windows.Forms.DockStyle.Fill;
            this.inputStartPosition.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.inputStartPosition.FormattingEnabled = true;
            this.inputStartPosition.Location = new System.Drawing.Point(420, 5);
            this.inputStartPosition.Margin = new System.Windows.Forms.Padding(0);
            this.inputStartPosition.Name = "inputStartPosition";
            this.inputStartPosition.Size = new System.Drawing.Size(140, 21);
            this.inputStartPosition.TabIndex = 35;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label10.Location = new System.Drawing.Point(0, 5);
            this.label10.Margin = new System.Windows.Forms.Padding(0);
            this.label10.Name = "label10";
            this.label10.Padding = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.label10.Size = new System.Drawing.Size(140, 30);
            this.label10.TabIndex = 40;
            this.label10.Text = "World";
            this.label10.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label4.Location = new System.Drawing.Point(283, 5);
            this.label4.Name = "label4";
            this.label4.Padding = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.label4.Size = new System.Drawing.Size(134, 30);
            this.label4.TabIndex = 34;
            this.label4.Text = "Start Position";
            this.label4.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(0, 35);
            this.label1.Margin = new System.Windows.Forms.Padding(0);
            this.label1.Name = "label1";
            this.label1.Padding = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.label1.Size = new System.Drawing.Size(140, 30);
            this.label1.TabIndex = 29;
            this.label1.Text = "Random Seed";
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label3.Location = new System.Drawing.Point(0, 65);
            this.label3.Margin = new System.Windows.Forms.Padding(0);
            this.label3.Name = "label3";
            this.label3.Padding = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.label3.Size = new System.Drawing.Size(140, 30);
            this.label3.TabIndex = 33;
            this.label3.Text = "Group Size";
            this.label3.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // inputGroupSize
            // 
            this.inputGroupSize.Dock = System.Windows.Forms.DockStyle.Fill;
            this.inputGroupSize.Location = new System.Drawing.Point(140, 65);
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
            this.inputGroupSize.Size = new System.Drawing.Size(140, 20);
            this.inputGroupSize.TabIndex = 32;
            this.inputGroupSize.Value = new decimal(new int[] {
            16,
            0,
            0,
            0});
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Location = new System.Drawing.Point(0, 95);
            this.label2.Margin = new System.Windows.Forms.Padding(0);
            this.label2.Name = "label2";
            this.label2.Padding = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.label2.Size = new System.Drawing.Size(140, 30);
            this.label2.TabIndex = 31;
            this.label2.Text = "Population Density";
            this.label2.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // inputStartGrouped
            // 
            this.inputStartGrouped.AutoSize = true;
            this.inputStartGrouped.Location = new System.Drawing.Point(703, 8);
            this.inputStartGrouped.Name = "inputStartGrouped";
            this.inputStartGrouped.Size = new System.Drawing.Size(15, 14);
            this.inputStartGrouped.TabIndex = 27;
            this.inputStartGrouped.UseVisualStyleBackColor = true;
            // 
            // inputPauseDuringBloodmoon
            // 
            this.inputPauseDuringBloodmoon.AutoSize = true;
            this.inputPauseDuringBloodmoon.Location = new System.Drawing.Point(703, 38);
            this.inputPauseDuringBloodmoon.Name = "inputPauseDuringBloodmoon";
            this.inputPauseDuringBloodmoon.Size = new System.Drawing.Size(15, 14);
            this.inputPauseDuringBloodmoon.TabIndex = 39;
            this.inputPauseDuringBloodmoon.UseVisualStyleBackColor = true;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label14.Location = new System.Drawing.Point(563, 65);
            this.label14.Name = "label14";
            this.label14.Padding = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.label14.Size = new System.Drawing.Size(134, 30);
            this.label14.TabIndex = 51;
            this.label14.Text = "Fast forward at start";
            this.label14.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // inputFastForward
            // 
            this.inputFastForward.AutoSize = true;
            this.inputFastForward.Location = new System.Drawing.Point(703, 68);
            this.inputFastForward.Name = "inputFastForward";
            this.inputFastForward.Size = new System.Drawing.Size(15, 14);
            this.inputFastForward.TabIndex = 44;
            this.inputFastForward.UseVisualStyleBackColor = true;
            // 
            // inputMaxAgents
            // 
            this.inputMaxAgents.AutoSize = true;
            this.inputMaxAgents.Dock = System.Windows.Forms.DockStyle.Fill;
            this.inputMaxAgents.Location = new System.Drawing.Point(140, 95);
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
            this.inputMaxAgents.Size = new System.Drawing.Size(140, 20);
            this.inputMaxAgents.TabIndex = 52;
            this.inputMaxAgents.Value = new decimal(new int[] {
            300,
            0,
            0,
            0});
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.buttonDuplicateGroup);
            this.tabPage2.Controls.Add(this.groupProps);
            this.tabPage2.Controls.Add(this.groupProcessors);
            this.tabPage2.Controls.Add(this.buttonRemoveGroup);
            this.tabPage2.Controls.Add(this.button1);
            this.tabPage2.Controls.Add(this.listProcessorGroups);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(847, 167);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Movement Processors";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // buttonDuplicateGroup
            // 
            this.buttonDuplicateGroup.Enabled = false;
            this.buttonDuplicateGroup.Location = new System.Drawing.Point(83, 138);
            this.buttonDuplicateGroup.Name = "buttonDuplicateGroup";
            this.buttonDuplicateGroup.Size = new System.Drawing.Size(62, 23);
            this.buttonDuplicateGroup.TabIndex = 7;
            this.buttonDuplicateGroup.Text = "Duplicate";
            this.buttonDuplicateGroup.UseVisualStyleBackColor = true;
            this.buttonDuplicateGroup.Click += new System.EventHandler(this.OnDuplicateGroupClick);
            // 
            // groupProps
            // 
            this.groupProps.Controls.Add(this.inputPostSpawnBehavior);
            this.groupProps.Controls.Add(this.label15);
            this.groupProps.Controls.Add(this.lblAffected);
            this.groupProps.Controls.Add(this.boxGroupColor);
            this.groupProps.Controls.Add(this.buttonGroupColor);
            this.groupProps.Controls.Add(this.label11);
            this.groupProps.Controls.Add(this.inputMovementSpeed);
            this.groupProps.Controls.Add(this.label7);
            this.groupProps.Controls.Add(this.lblAffectedGroup);
            this.groupProps.Controls.Add(this.inputMovementGroup);
            this.groupProps.Location = new System.Drawing.Point(225, 6);
            this.groupProps.Name = "groupProps";
            this.groupProps.Size = new System.Drawing.Size(237, 155);
            this.groupProps.TabIndex = 3;
            this.groupProps.TabStop = false;
            this.groupProps.Text = "Properties";
            this.groupProps.Visible = false;
            // 
            // inputPostSpawnBehavior
            // 
            this.inputPostSpawnBehavior.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.inputPostSpawnBehavior.FormattingEnabled = true;
            this.inputPostSpawnBehavior.Location = new System.Drawing.Point(118, 101);
            this.inputPostSpawnBehavior.Name = "inputPostSpawnBehavior";
            this.inputPostSpawnBehavior.Size = new System.Drawing.Size(113, 21);
            this.inputPostSpawnBehavior.TabIndex = 11;
            this.inputPostSpawnBehavior.SelectedIndexChanged += new System.EventHandler(this.OnPostSpawnBehaviorSelectionChanged);
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(6, 104);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(109, 13);
            this.label15.TabIndex = 10;
            this.label15.Text = "Post Spawn Behavior";
            // 
            // lblAffected
            // 
            this.lblAffected.AutoSize = true;
            this.lblAffected.Location = new System.Drawing.Point(6, 129);
            this.lblAffected.Name = "lblAffected";
            this.lblAffected.Size = new System.Drawing.Size(95, 13);
            this.lblAffected.TabIndex = 9;
            this.lblAffected.Text = "Affected Agents: 0";
            // 
            // boxGroupColor
            // 
            this.boxGroupColor.BackColor = System.Drawing.Color.Transparent;
            this.boxGroupColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.boxGroupColor.Location = new System.Drawing.Point(118, 74);
            this.boxGroupColor.Name = "boxGroupColor";
            this.boxGroupColor.Size = new System.Drawing.Size(91, 21);
            this.boxGroupColor.TabIndex = 8;
            this.boxGroupColor.TabStop = false;
            // 
            // buttonGroupColor
            // 
            this.buttonGroupColor.Location = new System.Drawing.Point(208, 73);
            this.buttonGroupColor.Name = "buttonGroupColor";
            this.buttonGroupColor.Size = new System.Drawing.Size(23, 23);
            this.buttonGroupColor.TabIndex = 7;
            this.buttonGroupColor.Text = "...";
            this.buttonGroupColor.UseVisualStyleBackColor = true;
            this.buttonGroupColor.Click += new System.EventHandler(this.OnGroupColorPickClick);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(6, 78);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(63, 13);
            this.label11.TabIndex = 6;
            this.label11.Text = "Group Color";
            // 
            // inputMovementSpeed
            // 
            this.inputMovementSpeed.DecimalPlaces = 5;
            this.inputMovementSpeed.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.inputMovementSpeed.Location = new System.Drawing.Point(119, 47);
            this.inputMovementSpeed.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.inputMovementSpeed.Name = "inputMovementSpeed";
            this.inputMovementSpeed.Size = new System.Drawing.Size(112, 20);
            this.inputMovementSpeed.TabIndex = 5;
            this.inputMovementSpeed.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.inputMovementSpeed.ValueChanged += new System.EventHandler(this.OnMovementSpeedChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 49);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(91, 13);
            this.label7.TabIndex = 2;
            this.label7.Text = "Movement Speed";
            // 
            // lblAffectedGroup
            // 
            this.lblAffectedGroup.AutoSize = true;
            this.lblAffectedGroup.Location = new System.Drawing.Point(6, 24);
            this.lblAffectedGroup.Name = "lblAffectedGroup";
            this.lblAffectedGroup.Size = new System.Drawing.Size(79, 13);
            this.lblAffectedGroup.TabIndex = 1;
            this.lblAffectedGroup.Text = "Affected Group";
            // 
            // inputMovementGroup
            // 
            this.inputMovementGroup.Location = new System.Drawing.Point(119, 21);
            this.inputMovementGroup.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.inputMovementGroup.Name = "inputMovementGroup";
            this.inputMovementGroup.Size = new System.Drawing.Size(112, 20);
            this.inputMovementGroup.TabIndex = 4;
            this.inputMovementGroup.Value = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.inputMovementGroup.ValueChanged += new System.EventHandler(this.OnGroupIdChanged);
            // 
            // groupProcessors
            // 
            this.groupProcessors.Controls.Add(this.buttonRemoveProcessor);
            this.groupProcessors.Controls.Add(this.button4);
            this.groupProcessors.Controls.Add(this.groupParameter);
            this.groupProcessors.Controls.Add(this.listProcessors);
            this.groupProcessors.Location = new System.Drawing.Point(468, 6);
            this.groupProcessors.Name = "groupProcessors";
            this.groupProcessors.Size = new System.Drawing.Size(415, 155);
            this.groupProcessors.TabIndex = 6;
            this.groupProcessors.TabStop = false;
            this.groupProcessors.Text = "Processors";
            this.groupProcessors.Visible = false;
            // 
            // buttonRemoveProcessor
            // 
            this.buttonRemoveProcessor.Enabled = false;
            this.buttonRemoveProcessor.Location = new System.Drawing.Point(114, 124);
            this.buttonRemoveProcessor.Name = "buttonRemoveProcessor";
            this.buttonRemoveProcessor.Size = new System.Drawing.Size(87, 23);
            this.buttonRemoveProcessor.TabIndex = 9;
            this.buttonRemoveProcessor.Text = "Remove";
            this.buttonRemoveProcessor.UseVisualStyleBackColor = true;
            this.buttonRemoveProcessor.Click += new System.EventHandler(this.OnRemoveProcessorClick);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(9, 124);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(99, 23);
            this.button4.TabIndex = 8;
            this.button4.Text = "Add Processor";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.OnAddProcessorClick);
            // 
            // groupParameter
            // 
            this.groupParameter.Controls.Add(this.inputProcessorPower);
            this.groupParameter.Controls.Add(this.label8);
            this.groupParameter.Controls.Add(this.label9);
            this.groupParameter.Controls.Add(this.inputProcessorDistance);
            this.groupParameter.Location = new System.Drawing.Point(207, 14);
            this.groupParameter.Name = "groupParameter";
            this.groupParameter.Size = new System.Drawing.Size(200, 133);
            this.groupParameter.TabIndex = 10;
            this.groupParameter.TabStop = false;
            this.groupParameter.Text = "Parameters";
            this.groupParameter.Visible = false;
            // 
            // inputProcessorPower
            // 
            this.inputProcessorPower.DecimalPlaces = 5;
            this.inputProcessorPower.Increment = new decimal(new int[] {
            1,
            0,
            0,
            327680});
            this.inputProcessorPower.Location = new System.Drawing.Point(119, 48);
            this.inputProcessorPower.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.inputProcessorPower.Name = "inputProcessorPower";
            this.inputProcessorPower.Size = new System.Drawing.Size(75, 20);
            this.inputProcessorPower.TabIndex = 12;
            this.inputProcessorPower.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.inputProcessorPower.ValueChanged += new System.EventHandler(this.OnPowerValueChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(6, 50);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(37, 13);
            this.label8.TabIndex = 10;
            this.label8.Text = "Power";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(6, 22);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(49, 13);
            this.label9.TabIndex = 9;
            this.label9.Text = "Distance";
            // 
            // inputProcessorDistance
            // 
            this.inputProcessorDistance.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.inputProcessorDistance.Location = new System.Drawing.Point(119, 22);
            this.inputProcessorDistance.Maximum = new decimal(new int[] {
            750,
            0,
            0,
            0});
            this.inputProcessorDistance.Name = "inputProcessorDistance";
            this.inputProcessorDistance.Size = new System.Drawing.Size(75, 20);
            this.inputProcessorDistance.TabIndex = 11;
            this.inputProcessorDistance.ValueChanged += new System.EventHandler(this.OnDistanceValueChanged);
            // 
            // listProcessors
            // 
            this.listProcessors.FormattingEnabled = true;
            this.listProcessors.Location = new System.Drawing.Point(10, 19);
            this.listProcessors.Name = "listProcessors";
            this.listProcessors.Size = new System.Drawing.Size(190, 95);
            this.listProcessors.TabIndex = 7;
            this.listProcessors.SelectedIndexChanged += new System.EventHandler(this.OnProcessorSelectionChanged);
            // 
            // buttonRemoveGroup
            // 
            this.buttonRemoveGroup.Enabled = false;
            this.buttonRemoveGroup.Location = new System.Drawing.Point(151, 138);
            this.buttonRemoveGroup.Name = "buttonRemoveGroup";
            this.buttonRemoveGroup.Size = new System.Drawing.Size(69, 23);
            this.buttonRemoveGroup.TabIndex = 2;
            this.buttonRemoveGroup.Text = "Remove";
            this.buttonRemoveGroup.UseVisualStyleBackColor = true;
            this.buttonRemoveGroup.Click += new System.EventHandler(this.OnRemoveGroupClick);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(8, 138);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(70, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "Add Group";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.OnAddGroupClick);
            // 
            // listProcessorGroups
            // 
            this.listProcessorGroups.FormattingEnabled = true;
            this.listProcessorGroups.Location = new System.Drawing.Point(8, 13);
            this.listProcessorGroups.Name = "listProcessorGroups";
            this.listProcessorGroups.Size = new System.Drawing.Size(211, 121);
            this.listProcessorGroups.TabIndex = 0;
            this.listProcessorGroups.SelectedIndexChanged += new System.EventHandler(this.OnGroupSelection);
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
            this.tabPage4.Size = new System.Drawing.Size(847, 167);
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
            this.tabPage3.Size = new System.Drawing.Size(847, 167);
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
            this.rtbLog.Size = new System.Drawing.Size(847, 167);
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
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(855, 740);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.menuStrip1);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "FormMain";
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
            this.tabPage2.ResumeLayout(false);
            this.groupProps.ResumeLayout(false);
            this.groupProps.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.boxGroupColor)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.inputMovementSpeed)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.inputMovementGroup)).EndInit();
            this.groupProcessors.ResumeLayout(false);
            this.groupParameter.ResumeLayout(false);
            this.groupParameter.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.inputProcessorPower)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.inputProcessorDistance)).EndInit();
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
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox inputStartPosition;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown inputGroupSize;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown inputRandomSeed;
        private System.Windows.Forms.CheckBox inputStartGrouped;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ListBox listProcessorGroups;
        private System.Windows.Forms.Button buttonRemoveGroup;
        private System.Windows.Forms.GroupBox groupProcessors;
        private System.Windows.Forms.ListBox listProcessors;
        private System.Windows.Forms.GroupBox groupProps;
        private System.Windows.Forms.NumericUpDown inputMovementSpeed;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label lblAffectedGroup;
        private System.Windows.Forms.NumericUpDown inputMovementGroup;
        private System.Windows.Forms.GroupBox groupParameter;
        private System.Windows.Forms.NumericUpDown inputProcessorPower;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.NumericUpDown inputProcessorDistance;
        private System.Windows.Forms.Button buttonRemoveProcessor;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.ComboBox inputWorld;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.ToolStripMenuItem speedToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem killToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadConfigurationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem stopToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.RichTextBox rtbLog;
        private System.Windows.Forms.ContextMenuStrip contextLog;
        private System.Windows.Forms.ToolStripMenuItem clearToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.PictureBox boxGroupColor;
        private System.Windows.Forms.Button buttonGroupColor;
        private System.Windows.Forms.ColorDialog colorPickerDlg;
        private System.Windows.Forms.Button buttonDuplicateGroup;
        private System.Windows.Forms.ToolStripMenuItem exportConfigurationToolStripMenuItem;
        private System.Windows.Forms.Button btRand;
        private System.Windows.Forms.ToolTip toolTipGroupSize;
        private System.Windows.Forms.Label lblAffected;
        private System.Windows.Forms.ToolStripMenuItem viewPrefabs;
        private System.Windows.Forms.CheckBox inputFastForward;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown inputSpawnProtectionTime;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.NumericUpDown inputMaxAgents;
        private System.Windows.Forms.Label label15;
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
    }
}

