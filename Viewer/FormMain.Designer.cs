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
            this.updateTimer = new System.Windows.Forms.Timer(this.components);
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewRoads = new System.Windows.Forms.ToolStripMenuItem();
            this.viewAgents = new System.Windows.Forms.ToolStripMenuItem();
            this.viewEvents = new System.Windows.Forms.ToolStripMenuItem();
            this.simulationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.restartToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pauseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resumeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.speedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.xToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.xToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.xToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.xToolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.xToolStripMenuItem4 = new System.Windows.Forms.ToolStripMenuItem();
            this.xToolStripMenuItem5 = new System.Windows.Forms.ToolStripMenuItem();
            this.xToolStripMenuItem6 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.advanceOneTickToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.emitSoundToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.simCanvas = new System.Windows.Forms.PictureBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.inputWorld = new System.Windows.Forms.ComboBox();
            this.label10 = new System.Windows.Forms.Label();
            this.inputPauseDuringBloodmoon = new System.Windows.Forms.CheckBox();
            this.inputPausePlayerless = new System.Windows.Forms.CheckBox();
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
            this.groupProps = new System.Windows.Forms.GroupBox();
            this.inputMovementSpeed = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.inputMovementGroup = new System.Windows.Forms.NumericUpDown();
            this.groupProcessors = new System.Windows.Forms.GroupBox();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.groupParameter = new System.Windows.Forms.GroupBox();
            this.inputProcessorPower = new System.Windows.Forms.NumericUpDown();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.inputProcessorDistance = new System.Windows.Forms.NumericUpDown();
            this.listProcessors = new System.Windows.Forms.ListBox();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.listProcessorGroups = new System.Windows.Forms.ListBox();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.simCanvas)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.inputGroupSize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.inputMaxAgents)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.inputRandomSeed)).BeginInit();
            this.tabPage2.SuspendLayout();
            this.groupProps.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.inputMovementSpeed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.inputMovementGroup)).BeginInit();
            this.groupProcessors.SuspendLayout();
            this.groupParameter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.inputProcessorPower)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.inputProcessorDistance)).BeginInit();
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
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
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
            this.restartToolStripMenuItem,
            this.pauseToolStripMenuItem,
            this.resumeToolStripMenuItem,
            this.speedToolStripMenuItem,
            this.advanceOneTickToolStripMenuItem});
            this.simulationToolStripMenuItem.Name = "simulationToolStripMenuItem";
            this.simulationToolStripMenuItem.Size = new System.Drawing.Size(76, 20);
            this.simulationToolStripMenuItem.Text = "Simulation";
            // 
            // restartToolStripMenuItem
            // 
            this.restartToolStripMenuItem.Name = "restartToolStripMenuItem";
            this.restartToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.restartToolStripMenuItem.Text = "Restart";
            this.restartToolStripMenuItem.Click += new System.EventHandler(this.OnRestartClick);
            // 
            // pauseToolStripMenuItem
            // 
            this.pauseToolStripMenuItem.Name = "pauseToolStripMenuItem";
            this.pauseToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.pauseToolStripMenuItem.Text = "Pause";
            this.pauseToolStripMenuItem.Click += new System.EventHandler(this.OnPauseClick);
            // 
            // resumeToolStripMenuItem
            // 
            this.resumeToolStripMenuItem.Name = "resumeToolStripMenuItem";
            this.resumeToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.resumeToolStripMenuItem.Text = "Resume";
            this.resumeToolStripMenuItem.Click += new System.EventHandler(this.OnResumeClick);
            // 
            // speedToolStripMenuItem
            // 
            this.speedToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.xToolStripMenuItem,
            this.xToolStripMenuItem1,
            this.xToolStripMenuItem2,
            this.xToolStripMenuItem3,
            this.xToolStripMenuItem4,
            this.xToolStripMenuItem5,
            this.xToolStripMenuItem6,
            this.toolStripMenuItem2});
            this.speedToolStripMenuItem.Name = "speedToolStripMenuItem";
            this.speedToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.speedToolStripMenuItem.Text = "Speed";
            // 
            // xToolStripMenuItem
            // 
            this.xToolStripMenuItem.Name = "xToolStripMenuItem";
            this.xToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.xToolStripMenuItem.Text = "1x";
            this.xToolStripMenuItem.Click += new System.EventHandler(this.OnTimeScale1Click);
            // 
            // xToolStripMenuItem1
            // 
            this.xToolStripMenuItem1.Name = "xToolStripMenuItem1";
            this.xToolStripMenuItem1.Size = new System.Drawing.Size(180, 22);
            this.xToolStripMenuItem1.Text = "2x";
            this.xToolStripMenuItem1.Click += new System.EventHandler(this.OnTimeScale2Click);
            // 
            // xToolStripMenuItem2
            // 
            this.xToolStripMenuItem2.Name = "xToolStripMenuItem2";
            this.xToolStripMenuItem2.Size = new System.Drawing.Size(180, 22);
            this.xToolStripMenuItem2.Text = "4x";
            this.xToolStripMenuItem2.Click += new System.EventHandler(this.OnTimeScale4Click);
            // 
            // xToolStripMenuItem3
            // 
            this.xToolStripMenuItem3.Name = "xToolStripMenuItem3";
            this.xToolStripMenuItem3.Size = new System.Drawing.Size(180, 22);
            this.xToolStripMenuItem3.Text = "8x";
            this.xToolStripMenuItem3.Click += new System.EventHandler(this.OnTimeScale8Click);
            // 
            // xToolStripMenuItem4
            // 
            this.xToolStripMenuItem4.Name = "xToolStripMenuItem4";
            this.xToolStripMenuItem4.Size = new System.Drawing.Size(180, 22);
            this.xToolStripMenuItem4.Text = "16x";
            this.xToolStripMenuItem4.Click += new System.EventHandler(this.OnTimeScale16Click);
            // 
            // xToolStripMenuItem5
            // 
            this.xToolStripMenuItem5.Name = "xToolStripMenuItem5";
            this.xToolStripMenuItem5.Size = new System.Drawing.Size(180, 22);
            this.xToolStripMenuItem5.Text = "32x";
            this.xToolStripMenuItem5.Click += new System.EventHandler(this.OnTimeScale32Click);
            // 
            // xToolStripMenuItem6
            // 
            this.xToolStripMenuItem6.Name = "xToolStripMenuItem6";
            this.xToolStripMenuItem6.Size = new System.Drawing.Size(180, 22);
            this.xToolStripMenuItem6.Text = "64x";
            this.xToolStripMenuItem6.Click += new System.EventHandler(this.OnTimeScale64Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(180, 22);
            this.toolStripMenuItem2.Text = "128x";
            this.toolStripMenuItem2.Click += new System.EventHandler(this.OnTimeScale128Click);
            // 
            // advanceOneTickToolStripMenuItem
            // 
            this.advanceOneTickToolStripMenuItem.Name = "advanceOneTickToolStripMenuItem";
            this.advanceOneTickToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.advanceOneTickToolStripMenuItem.Text = "Advance one Tick";
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.emitSoundToolStripMenuItem});
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
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
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
            this.splitContainer1.Panel2.Controls.Add(this.tabControl1);
            this.splitContainer1.Size = new System.Drawing.Size(856, 693);
            this.splitContainer1.SplitterDistance = 481;
            this.splitContainer1.TabIndex = 5;
            // 
            // simCanvas
            // 
            this.simCanvas.BackColor = System.Drawing.Color.Black;
            this.simCanvas.Dock = System.Windows.Forms.DockStyle.Fill;
            this.simCanvas.Location = new System.Drawing.Point(0, 0);
            this.simCanvas.Name = "simCanvas";
            this.simCanvas.Size = new System.Drawing.Size(856, 481);
            this.simCanvas.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.simCanvas.TabIndex = 4;
            this.simCanvas.TabStop = false;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(856, 208);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.inputWorld);
            this.tabPage1.Controls.Add(this.label10);
            this.tabPage1.Controls.Add(this.inputPauseDuringBloodmoon);
            this.tabPage1.Controls.Add(this.inputPausePlayerless);
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
            this.tabPage1.Size = new System.Drawing.Size(848, 182);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Base Parameters";
            this.tabPage1.UseVisualStyleBackColor = true;
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
            this.inputPauseDuringBloodmoon.Location = new System.Drawing.Point(281, 62);
            this.inputPauseDuringBloodmoon.Name = "inputPauseDuringBloodmoon";
            this.inputPauseDuringBloodmoon.Size = new System.Drawing.Size(144, 17);
            this.inputPauseDuringBloodmoon.TabIndex = 39;
            this.inputPauseDuringBloodmoon.Text = "Pause during Bloodmoon";
            this.inputPauseDuringBloodmoon.UseVisualStyleBackColor = true;
            // 
            // inputPausePlayerless
            // 
            this.inputPausePlayerless.AutoSize = true;
            this.inputPausePlayerless.Location = new System.Drawing.Point(281, 39);
            this.inputPausePlayerless.Name = "inputPausePlayerless";
            this.inputPausePlayerless.Size = new System.Drawing.Size(130, 17);
            this.inputPausePlayerless.TabIndex = 38;
            this.inputPausePlayerless.Text = "Pause without Players";
            this.inputPausePlayerless.UseVisualStyleBackColor = true;
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
            this.inputGroupSize.Name = "inputGroupSize";
            this.inputGroupSize.Size = new System.Drawing.Size(120, 20);
            this.inputGroupSize.TabIndex = 32;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 66);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(63, 13);
            this.label2.TabIndex = 31;
            this.label2.Text = "Max Agents";
            // 
            // inputMaxAgents
            // 
            this.inputMaxAgents.Location = new System.Drawing.Point(113, 64);
            this.inputMaxAgents.Name = "inputMaxAgents";
            this.inputMaxAgents.Size = new System.Drawing.Size(120, 20);
            this.inputMaxAgents.TabIndex = 30;
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
            this.inputRandomSeed.Name = "inputRandomSeed";
            this.inputRandomSeed.Size = new System.Drawing.Size(121, 20);
            this.inputRandomSeed.TabIndex = 28;
            // 
            // inputStartGrouped
            // 
            this.inputStartGrouped.AutoSize = true;
            this.inputStartGrouped.Location = new System.Drawing.Point(281, 17);
            this.inputStartGrouped.Name = "inputStartGrouped";
            this.inputStartGrouped.Size = new System.Drawing.Size(128, 17);
            this.inputStartGrouped.TabIndex = 27;
            this.inputStartGrouped.Text = "Start Agents Grouped";
            this.inputStartGrouped.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.groupProps);
            this.tabPage2.Controls.Add(this.groupProcessors);
            this.tabPage2.Controls.Add(this.button2);
            this.tabPage2.Controls.Add(this.button1);
            this.tabPage2.Controls.Add(this.listProcessorGroups);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(848, 182);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Movement Processors";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // groupProps
            // 
            this.groupProps.Controls.Add(this.inputMovementSpeed);
            this.groupProps.Controls.Add(this.label7);
            this.groupProps.Controls.Add(this.label6);
            this.groupProps.Controls.Add(this.inputMovementGroup);
            this.groupProps.Location = new System.Drawing.Point(225, 6);
            this.groupProps.Name = "groupProps";
            this.groupProps.Size = new System.Drawing.Size(200, 145);
            this.groupProps.TabIndex = 4;
            this.groupProps.TabStop = false;
            this.groupProps.Text = "Properties";
            this.groupProps.Visible = false;
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
            this.inputMovementSpeed.TabIndex = 3;
            this.inputMovementSpeed.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
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
            this.inputMovementGroup.TabIndex = 0;
            this.inputMovementGroup.Value = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            // 
            // groupProcessors
            // 
            this.groupProcessors.Controls.Add(this.button3);
            this.groupProcessors.Controls.Add(this.button4);
            this.groupProcessors.Controls.Add(this.groupParameter);
            this.groupProcessors.Controls.Add(this.listProcessors);
            this.groupProcessors.Location = new System.Drawing.Point(431, 6);
            this.groupProcessors.Name = "groupProcessors";
            this.groupProcessors.Size = new System.Drawing.Size(409, 145);
            this.groupProcessors.TabIndex = 3;
            this.groupProcessors.TabStop = false;
            this.groupProcessors.Text = "Processors";
            this.groupProcessors.Visible = false;
            // 
            // button3
            // 
            this.button3.Enabled = false;
            this.button3.Location = new System.Drawing.Point(114, 113);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(87, 23);
            this.button3.TabIndex = 4;
            this.button3.Text = "Remove";
            this.button3.UseVisualStyleBackColor = true;
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(9, 113);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(99, 23);
            this.button4.TabIndex = 3;
            this.button4.Text = "Add Processor";
            this.button4.UseVisualStyleBackColor = true;
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
            this.groupParameter.TabIndex = 1;
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
            this.inputProcessorPower.TabIndex = 11;
            this.inputProcessorPower.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
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
            10,
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
            this.inputProcessorDistance.TabIndex = 8;
            // 
            // listProcessors
            // 
            this.listProcessors.FormattingEnabled = true;
            this.listProcessors.Location = new System.Drawing.Point(10, 19);
            this.listProcessors.Name = "listProcessors";
            this.listProcessors.Size = new System.Drawing.Size(190, 82);
            this.listProcessors.TabIndex = 0;
            this.listProcessors.SelectedIndexChanged += new System.EventHandler(this.OnProcessorSelectionChanged);
            // 
            // button2
            // 
            this.button2.Enabled = false;
            this.button2.Location = new System.Drawing.Point(112, 128);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(108, 23);
            this.button2.TabIndex = 2;
            this.button2.Text = "Remove";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(7, 128);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(99, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "Add Group";
            this.button1.UseVisualStyleBackColor = true;
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
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(856, 717);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "FormMain";
            this.Text = "WalkerSim";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.simCanvas)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.inputGroupSize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.inputMaxAgents)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.inputRandomSeed)).EndInit();
            this.tabPage2.ResumeLayout(false);
            this.groupProps.ResumeLayout(false);
            this.groupProps.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.inputMovementSpeed)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.inputMovementGroup)).EndInit();
            this.groupProcessors.ResumeLayout(false);
            this.groupParameter.ResumeLayout(false);
            this.groupParameter.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.inputProcessorPower)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.inputProcessorDistance)).EndInit();
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
        private System.Windows.Forms.ToolStripMenuItem restartToolStripMenuItem;
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
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.CheckBox inputPauseDuringBloodmoon;
        private System.Windows.Forms.CheckBox inputPausePlayerless;
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
        private System.Windows.Forms.Button button2;
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
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.ComboBox inputWorld;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.ToolStripMenuItem speedToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem xToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem xToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem xToolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem xToolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem xToolStripMenuItem4;
        private System.Windows.Forms.ToolStripMenuItem xToolStripMenuItem5;
        private System.Windows.Forms.ToolStripMenuItem xToolStripMenuItem6;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
    }
}

