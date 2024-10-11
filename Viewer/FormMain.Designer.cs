namespace WalkerSim.Viewer
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
            this.viewRoads = new System.Windows.Forms.ToolStripMenuItem();
            this.viewAgents = new System.Windows.Forms.ToolStripMenuItem();
            this.viewEvents = new System.Windows.Forms.ToolStripMenuItem();
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.simCanvas = new System.Windows.Forms.PictureBox();
            this.tabSimulation = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.btRand = new System.Windows.Forms.Button();
            this.lblMaxAgentsInfo = new System.Windows.Forms.Label();
            this.inputWorld = new System.Windows.Forms.ComboBox();
            this.label10 = new System.Windows.Forms.Label();
            this.inputPauseDuringBloodmoon = new System.Windows.Forms.CheckBox();
            this.inputRespawnPosition = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.inputStartPosition = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.inputGroupSize = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.inputMaxAgents = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.inputRandomSeed = new System.Windows.Forms.NumericUpDown();
            this.inputStartGrouped = new System.Windows.Forms.CheckBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.buttonDuplicateGroup = new System.Windows.Forms.Button();
            this.groupProps = new System.Windows.Forms.GroupBox();
            this.boxGroupColor = new System.Windows.Forms.PictureBox();
            this.buttonGroupColor = new System.Windows.Forms.Button();
            this.label11 = new System.Windows.Forms.Label();
            this.inputMovementSpeed = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
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
            ((System.ComponentModel.ISupportInitialize)(this.inputGroupSize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.inputMaxAgents)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.inputRandomSeed)).BeginInit();
            this.tabPage2.SuspendLayout();
            this.groupProps.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.boxGroupColor)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.inputMovementSpeed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.inputMovementGroup)).BeginInit();
            this.groupProcessors.SuspendLayout();
            this.groupParameter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.inputProcessorPower)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.inputProcessorDistance)).BeginInit();
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
            this.menuStrip1.Size = new System.Drawing.Size(856, 24);
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
            this.viewRoads,
            this.viewAgents,
            this.viewEvents});
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.viewToolStripMenuItem.Text = "View";
            // 
            // viewRoads
            // 
            this.viewRoads.Checked = true;
            this.viewRoads.CheckOnClick = true;
            this.viewRoads.CheckState = System.Windows.Forms.CheckState.Checked;
            this.viewRoads.Name = "viewRoads";
            this.viewRoads.Size = new System.Drawing.Size(111, 22);
            this.viewRoads.Text = "Roads";
            // 
            // viewAgents
            // 
            this.viewAgents.Checked = true;
            this.viewAgents.CheckOnClick = true;
            this.viewAgents.CheckState = System.Windows.Forms.CheckState.Checked;
            this.viewAgents.Name = "viewAgents";
            this.viewAgents.Size = new System.Drawing.Size(111, 22);
            this.viewAgents.Text = "Agents";
            // 
            // viewEvents
            // 
            this.viewEvents.Checked = true;
            this.viewEvents.CheckOnClick = true;
            this.viewEvents.CheckState = System.Windows.Forms.CheckState.Checked;
            this.viewEvents.Name = "viewEvents";
            this.viewEvents.Size = new System.Drawing.Size(111, 22);
            this.viewEvents.Text = "Events";
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
            this.killToolStripMenuItem});
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(46, 20);
            this.toolsToolStripMenuItem.Text = "Tools";
            // 
            // emitSoundToolStripMenuItem
            // 
            this.emitSoundToolStripMenuItem.Name = "emitSoundToolStripMenuItem";
            this.emitSoundToolStripMenuItem.Size = new System.Drawing.Size(135, 22);
            this.emitSoundToolStripMenuItem.Text = "Emit Sound";
            this.emitSoundToolStripMenuItem.Click += new System.EventHandler(this.OnClickSoundEmit);
            // 
            // killToolStripMenuItem
            // 
            this.killToolStripMenuItem.Name = "killToolStripMenuItem";
            this.killToolStripMenuItem.Size = new System.Drawing.Size(135, 22);
            this.killToolStripMenuItem.Text = "Kill";
            this.killToolStripMenuItem.Click += new System.EventHandler(this.OnClickKill);
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
            this.splitContainer1.Panel1.Controls.Add(this.simCanvas);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tabSimulation);
            this.splitContainer1.Size = new System.Drawing.Size(856, 693);
            this.splitContainer1.SplitterDistance = 491;
            this.splitContainer1.TabIndex = 5;
            // 
            // simCanvas
            // 
            this.simCanvas.BackColor = System.Drawing.Color.Black;
            this.simCanvas.Dock = System.Windows.Forms.DockStyle.Fill;
            this.simCanvas.Location = new System.Drawing.Point(0, 0);
            this.simCanvas.Name = "simCanvas";
            this.simCanvas.Size = new System.Drawing.Size(856, 491);
            this.simCanvas.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.simCanvas.TabIndex = 4;
            this.simCanvas.TabStop = false;
            this.simCanvas.MouseClick += new System.Windows.Forms.MouseEventHandler(this.OnSimCanvasClick);
            // 
            // tabSimulation
            // 
            this.tabSimulation.Controls.Add(this.tabPage1);
            this.tabSimulation.Controls.Add(this.tabPage2);
            this.tabSimulation.Controls.Add(this.tabPage3);
            this.tabSimulation.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabSimulation.Location = new System.Drawing.Point(0, 0);
            this.tabSimulation.Name = "tabSimulation";
            this.tabSimulation.SelectedIndex = 0;
            this.tabSimulation.Size = new System.Drawing.Size(856, 198);
            this.tabSimulation.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.btRand);
            this.tabPage1.Controls.Add(this.lblMaxAgentsInfo);
            this.tabPage1.Controls.Add(this.inputWorld);
            this.tabPage1.Controls.Add(this.label10);
            this.tabPage1.Controls.Add(this.inputPauseDuringBloodmoon);
            this.tabPage1.Controls.Add(this.inputRespawnPosition);
            this.tabPage1.Controls.Add(this.label5);
            this.tabPage1.Controls.Add(this.inputStartPosition);
            this.tabPage1.Controls.Add(this.label4);
            this.tabPage1.Controls.Add(this.label3);
            this.tabPage1.Controls.Add(this.inputGroupSize);
            this.tabPage1.Controls.Add(this.label2);
            this.tabPage1.Controls.Add(this.inputMaxAgents);
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Controls.Add(this.inputRandomSeed);
            this.tabPage1.Controls.Add(this.inputStartGrouped);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(848, 172);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Base Parameters";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // btRand
            // 
            this.btRand.Location = new System.Drawing.Point(212, 37);
            this.btRand.Name = "btRand";
            this.btRand.Size = new System.Drawing.Size(22, 22);
            this.btRand.TabIndex = 43;
            this.btRand.Text = "R";
            this.btRand.UseVisualStyleBackColor = true;
            this.btRand.Click += new System.EventHandler(this.OnRandSeedClick);
            // 
            // lblMaxAgentsInfo
            // 
            this.lblMaxAgentsInfo.AutoSize = true;
            this.lblMaxAgentsInfo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.lblMaxAgentsInfo.Location = new System.Drawing.Point(262, 66);
            this.lblMaxAgentsInfo.Name = "lblMaxAgentsInfo";
            this.lblMaxAgentsInfo.Size = new System.Drawing.Size(91, 13);
            this.lblMaxAgentsInfo.TabIndex = 42;
            this.lblMaxAgentsInfo.Text = "<Recommended>";
            this.lblMaxAgentsInfo.Visible = false;
            // 
            // inputWorld
            // 
            this.inputWorld.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.inputWorld.FormattingEnabled = true;
            this.inputWorld.Location = new System.Drawing.Point(113, 13);
            this.inputWorld.Name = "inputWorld";
            this.inputWorld.Size = new System.Drawing.Size(121, 21);
            this.inputWorld.TabIndex = 41;
            this.inputWorld.SelectedIndexChanged += new System.EventHandler(this.OnWorldSelectionChanged);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(11, 16);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(35, 13);
            this.label10.TabIndex = 40;
            this.label10.Text = "World";
            // 
            // inputPauseDuringBloodmoon
            // 
            this.inputPauseDuringBloodmoon.AutoSize = true;
            this.inputPauseDuringBloodmoon.Location = new System.Drawing.Point(265, 39);
            this.inputPauseDuringBloodmoon.Name = "inputPauseDuringBloodmoon";
            this.inputPauseDuringBloodmoon.Size = new System.Drawing.Size(144, 17);
            this.inputPauseDuringBloodmoon.TabIndex = 39;
            this.inputPauseDuringBloodmoon.Text = "Pause during Bloodmoon";
            this.inputPauseDuringBloodmoon.UseVisualStyleBackColor = true;
            // 
            // inputRespawnPosition
            // 
            this.inputRespawnPosition.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.inputRespawnPosition.FormattingEnabled = true;
            this.inputRespawnPosition.Location = new System.Drawing.Point(112, 143);
            this.inputRespawnPosition.Name = "inputRespawnPosition";
            this.inputRespawnPosition.Size = new System.Drawing.Size(121, 21);
            this.inputRespawnPosition.TabIndex = 37;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(10, 146);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(92, 13);
            this.label5.TabIndex = 36;
            this.label5.Text = "Respawn Position";
            // 
            // inputStartPosition
            // 
            this.inputStartPosition.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.inputStartPosition.FormattingEnabled = true;
            this.inputStartPosition.Location = new System.Drawing.Point(112, 116);
            this.inputStartPosition.Name = "inputStartPosition";
            this.inputStartPosition.Size = new System.Drawing.Size(121, 21);
            this.inputStartPosition.TabIndex = 35;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(10, 119);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(69, 13);
            this.label4.TabIndex = 34;
            this.label4.Text = "Start Position";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 92);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(59, 13);
            this.label3.TabIndex = 33;
            this.label3.Text = "Group Size";
            // 
            // inputGroupSize
            // 
            this.inputGroupSize.Location = new System.Drawing.Point(113, 90);
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
            this.inputGroupSize.Size = new System.Drawing.Size(120, 20);
            this.inputGroupSize.TabIndex = 32;
            this.inputGroupSize.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 66);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(95, 13);
            this.label2.TabIndex = 31;
            this.label2.Text = "Population Density";
            // 
            // inputMaxAgents
            // 
            this.inputMaxAgents.Location = new System.Drawing.Point(113, 64);
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
            this.inputMaxAgents.Size = new System.Drawing.Size(120, 20);
            this.inputMaxAgents.TabIndex = 30;
            this.inputMaxAgents.Value = new decimal(new int[] {
            160,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 40);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(75, 13);
            this.label1.TabIndex = 29;
            this.label1.Text = "Random Seed";
            // 
            // inputRandomSeed
            // 
            this.inputRandomSeed.Location = new System.Drawing.Point(113, 38);
            this.inputRandomSeed.Maximum = new decimal(new int[] {
            -1,
            2147483647,
            0,
            0});
            this.inputRandomSeed.Name = "inputRandomSeed";
            this.inputRandomSeed.Size = new System.Drawing.Size(100, 20);
            this.inputRandomSeed.TabIndex = 28;
            // 
            // inputStartGrouped
            // 
            this.inputStartGrouped.AutoSize = true;
            this.inputStartGrouped.Location = new System.Drawing.Point(265, 16);
            this.inputStartGrouped.Name = "inputStartGrouped";
            this.inputStartGrouped.Size = new System.Drawing.Size(128, 17);
            this.inputStartGrouped.TabIndex = 27;
            this.inputStartGrouped.Text = "Start Agents Grouped";
            this.inputStartGrouped.UseVisualStyleBackColor = true;
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
            this.tabPage2.Size = new System.Drawing.Size(848, 172);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Movement Processors";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // buttonDuplicateGroup
            // 
            this.buttonDuplicateGroup.Enabled = false;
            this.buttonDuplicateGroup.Location = new System.Drawing.Point(83, 128);
            this.buttonDuplicateGroup.Name = "buttonDuplicateGroup";
            this.buttonDuplicateGroup.Size = new System.Drawing.Size(62, 23);
            this.buttonDuplicateGroup.TabIndex = 7;
            this.buttonDuplicateGroup.Text = "Duplicate";
            this.buttonDuplicateGroup.UseVisualStyleBackColor = true;
            this.buttonDuplicateGroup.Click += new System.EventHandler(this.OnDuplicateGroupClick);
            // 
            // groupProps
            // 
            this.groupProps.Controls.Add(this.boxGroupColor);
            this.groupProps.Controls.Add(this.buttonGroupColor);
            this.groupProps.Controls.Add(this.label11);
            this.groupProps.Controls.Add(this.inputMovementSpeed);
            this.groupProps.Controls.Add(this.label7);
            this.groupProps.Controls.Add(this.label6);
            this.groupProps.Controls.Add(this.inputMovementGroup);
            this.groupProps.Location = new System.Drawing.Point(225, 6);
            this.groupProps.Name = "groupProps";
            this.groupProps.Size = new System.Drawing.Size(200, 145);
            this.groupProps.TabIndex = 3;
            this.groupProps.TabStop = false;
            this.groupProps.Text = "Properties";
            this.groupProps.Visible = false;
            // 
            // boxGroupColor
            // 
            this.boxGroupColor.BackColor = System.Drawing.Color.Transparent;
            this.boxGroupColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.boxGroupColor.Location = new System.Drawing.Point(118, 72);
            this.boxGroupColor.Name = "boxGroupColor";
            this.boxGroupColor.Size = new System.Drawing.Size(52, 21);
            this.boxGroupColor.TabIndex = 8;
            this.boxGroupColor.TabStop = false;
            // 
            // buttonGroupColor
            // 
            this.buttonGroupColor.Location = new System.Drawing.Point(171, 71);
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
            this.label11.Location = new System.Drawing.Point(6, 73);
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
            this.inputMovementSpeed.Location = new System.Drawing.Point(119, 45);
            this.inputMovementSpeed.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.inputMovementSpeed.Name = "inputMovementSpeed";
            this.inputMovementSpeed.Size = new System.Drawing.Size(75, 20);
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
            this.label7.Location = new System.Drawing.Point(6, 47);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(91, 13);
            this.label7.TabIndex = 2;
            this.label7.Text = "Movement Speed";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 22);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(79, 13);
            this.label6.TabIndex = 1;
            this.label6.Text = "Affected Group";
            // 
            // inputMovementGroup
            // 
            this.inputMovementGroup.Location = new System.Drawing.Point(119, 19);
            this.inputMovementGroup.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.inputMovementGroup.Name = "inputMovementGroup";
            this.inputMovementGroup.Size = new System.Drawing.Size(75, 20);
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
            this.groupProcessors.Location = new System.Drawing.Point(431, 6);
            this.groupProcessors.Name = "groupProcessors";
            this.groupProcessors.Size = new System.Drawing.Size(409, 145);
            this.groupProcessors.TabIndex = 6;
            this.groupProcessors.TabStop = false;
            this.groupProcessors.Text = "Processors";
            this.groupProcessors.Visible = false;
            // 
            // buttonRemoveProcessor
            // 
            this.buttonRemoveProcessor.Enabled = false;
            this.buttonRemoveProcessor.Location = new System.Drawing.Point(114, 113);
            this.buttonRemoveProcessor.Name = "buttonRemoveProcessor";
            this.buttonRemoveProcessor.Size = new System.Drawing.Size(87, 23);
            this.buttonRemoveProcessor.TabIndex = 9;
            this.buttonRemoveProcessor.Text = "Remove";
            this.buttonRemoveProcessor.UseVisualStyleBackColor = true;
            this.buttonRemoveProcessor.Click += new System.EventHandler(this.OnRemoveProcessorClick);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(9, 113);
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
            this.groupParameter.Size = new System.Drawing.Size(200, 125);
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
            this.listProcessors.Size = new System.Drawing.Size(190, 82);
            this.listProcessors.TabIndex = 7;
            this.listProcessors.SelectedIndexChanged += new System.EventHandler(this.OnProcessorSelectionChanged);
            // 
            // buttonRemoveGroup
            // 
            this.buttonRemoveGroup.Enabled = false;
            this.buttonRemoveGroup.Location = new System.Drawing.Point(151, 128);
            this.buttonRemoveGroup.Name = "buttonRemoveGroup";
            this.buttonRemoveGroup.Size = new System.Drawing.Size(69, 23);
            this.buttonRemoveGroup.TabIndex = 2;
            this.buttonRemoveGroup.Text = "Remove";
            this.buttonRemoveGroup.UseVisualStyleBackColor = true;
            this.buttonRemoveGroup.Click += new System.EventHandler(this.OnRemoveGroupClick);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(7, 128);
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
            this.listProcessorGroups.Size = new System.Drawing.Size(211, 108);
            this.listProcessorGroups.TabIndex = 0;
            this.listProcessorGroups.SelectedIndexChanged += new System.EventHandler(this.OnGroupSelection);
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.rtbLog);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(848, 172);
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
            this.rtbLog.Size = new System.Drawing.Size(848, 172);
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
            // toolTipGroupSize
            // 
            this.toolTipGroupSize.ToolTipTitle = "Hello World";
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(856, 717);
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
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.simCanvas)).EndInit();
            this.tabSimulation.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.inputGroupSize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.inputMaxAgents)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.inputRandomSeed)).EndInit();
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
        private System.Windows.Forms.PictureBox simCanvas;
        private System.Windows.Forms.TabControl tabSimulation;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.CheckBox inputPauseDuringBloodmoon;
        private System.Windows.Forms.ComboBox inputRespawnPosition;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox inputStartPosition;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown inputGroupSize;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown inputMaxAgents;
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
        private System.Windows.Forms.Label label6;
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
        private System.Windows.Forms.Label lblMaxAgentsInfo;
        private System.Windows.Forms.Button btRand;
        private System.Windows.Forms.ToolTip toolTipGroupSize;
    }
}

