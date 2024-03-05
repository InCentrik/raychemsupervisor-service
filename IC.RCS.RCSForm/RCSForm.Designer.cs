namespace IC.RCS.RCSForm {
    partial class RCSForm
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabService = new System.Windows.Forms.TabPage();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.txtPathToLogDirectory = new System.Windows.Forms.Label();
            this.buttonChooseLogFolderDirectory = new System.Windows.Forms.Button();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.serviceConnectionStatusText = new System.Windows.Forms.Label();
            this.buttonCheckServiceConnection = new System.Windows.Forms.Button();
            this.serviceStatusLabel = new System.Windows.Forms.Label();
            this.buttonServiceStop = new System.Windows.Forms.Button();
            this.buttonStartService = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.sqlConnectionStatusText = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtSQLPassword = new System.Windows.Forms.TextBox();
            this.txtSQLUsername = new System.Windows.Forms.TextBox();
            this.buttonCheckSQLConnection = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txtSQLServer = new System.Windows.Forms.TextBox();
            this.tabTrendGroups = new System.Windows.Forms.TabPage();
            this.dataGridViewTrendGroups = new System.Windows.Forms.DataGridView();
            this.buttonTrendGroupsPullFromSQL = new System.Windows.Forms.Button();
            this.buttonTrendGroupsSave = new System.Windows.Forms.Button();
            this.buttonTrendGroupsRefresh = new System.Windows.Forms.Button();
            this.tabLog = new System.Windows.Forms.TabPage();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.operationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setupTrendGroupsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.startToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.trendGroupSelected = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.trendGroupName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.trendGroupScanRate = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.trendGroupDescription = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.trendGroupGuid = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.trendGroupLastRefreshTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.panel1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabService.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tabTrendGroups.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewTrendGroups)).BeginInit();
            this.tabLog.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.tabControl1);
            this.panel1.Controls.Add(this.statusStrip1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 26);
            this.panel1.Margin = new System.Windows.Forms.Padding(2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(806, 431);
            this.panel1.TabIndex = 0;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabService);
            this.tabControl1.Controls.Add(this.tabTrendGroups);
            this.tabControl1.Controls.Add(this.tabLog);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(2);
            this.tabControl1.Multiline = true;
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(806, 404);
            this.tabControl1.TabIndex = 1;
            this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
            // 
            // tabService
            // 
            this.tabService.Controls.Add(this.groupBox3);
            this.tabService.Controls.Add(this.groupBox2);
            this.tabService.Controls.Add(this.groupBox1);
            this.tabService.Location = new System.Drawing.Point(4, 25);
            this.tabService.Margin = new System.Windows.Forms.Padding(2);
            this.tabService.Name = "tabService";
            this.tabService.Padding = new System.Windows.Forms.Padding(2);
            this.tabService.Size = new System.Drawing.Size(798, 375);
            this.tabService.TabIndex = 0;
            this.tabService.Text = "Service";
            this.tabService.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.txtPathToLogDirectory);
            this.groupBox3.Controls.Add(this.buttonChooseLogFolderDirectory);
            this.groupBox3.Controls.Add(this.checkBox1);
            this.groupBox3.Location = new System.Drawing.Point(36, 267);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(744, 101);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Logging";
            this.groupBox3.Enter += new System.EventHandler(this.groupBox3_Enter);
            // 
            // txtPathToLogDirectory
            // 
            this.txtPathToLogDirectory.AutoSize = true;
            this.txtPathToLogDirectory.Location = new System.Drawing.Point(270, 60);
            this.txtPathToLogDirectory.Name = "txtPathToLogDirectory";
            this.txtPathToLogDirectory.Size = new System.Drawing.Size(144, 16);
            this.txtPathToLogDirectory.TabIndex = 5;
            this.txtPathToLogDirectory.Text = "C:\\PathToLogDirectory";
            // 
            // buttonChooseLogFolderDirectory
            // 
            this.buttonChooseLogFolderDirectory.Location = new System.Drawing.Point(36, 57);
            this.buttonChooseLogFolderDirectory.Name = "buttonChooseLogFolderDirectory";
            this.buttonChooseLogFolderDirectory.Size = new System.Drawing.Size(189, 23);
            this.buttonChooseLogFolderDirectory.TabIndex = 10;
            this.buttonChooseLogFolderDirectory.Text = "Choose log folder directory";
            this.buttonChooseLogFolderDirectory.UseVisualStyleBackColor = true;
            this.buttonChooseLogFolderDirectory.Click += new System.EventHandler(this.buttonChooseLogFolderDirectory_Click);
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(36, 31);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(189, 20);
            this.checkBox1.TabIndex = 9;
            this.checkBox1.Text = "Save log to folder directory";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.serviceConnectionStatusText);
            this.groupBox2.Controls.Add(this.buttonCheckServiceConnection);
            this.groupBox2.Controls.Add(this.serviceStatusLabel);
            this.groupBox2.Controls.Add(this.buttonServiceStop);
            this.groupBox2.Controls.Add(this.buttonStartService);
            this.groupBox2.Location = new System.Drawing.Point(36, 25);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(744, 109);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Commands";
            this.groupBox2.Enter += new System.EventHandler(this.groupBox2_Enter);
            // 
            // serviceConnectionStatusText
            // 
            this.serviceConnectionStatusText.AutoSize = true;
            this.serviceConnectionStatusText.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.serviceConnectionStatusText.Location = new System.Drawing.Point(510, 68);
            this.serviceConnectionStatusText.Name = "serviceConnectionStatusText";
            this.serviceConnectionStatusText.Size = new System.Drawing.Size(191, 16);
            this.serviceConnectionStatusText.TabIndex = 9;
            this.serviceConnectionStatusText.Text = "Disconnected from service";
            this.serviceConnectionStatusText.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // buttonCheckServiceConnection
            // 
            this.buttonCheckServiceConnection.Location = new System.Drawing.Point(222, 36);
            this.buttonCheckServiceConnection.Margin = new System.Windows.Forms.Padding(2);
            this.buttonCheckServiceConnection.Name = "buttonCheckServiceConnection";
            this.buttonCheckServiceConnection.Size = new System.Drawing.Size(171, 52);
            this.buttonCheckServiceConnection.TabIndex = 4;
            this.buttonCheckServiceConnection.Text = "Check Service Connection";
            this.buttonCheckServiceConnection.UseVisualStyleBackColor = true;
            this.buttonCheckServiceConnection.Click += new System.EventHandler(this.buttonCheckServiceConnection_Click);
            // 
            // serviceStatusLabel
            // 
            this.serviceStatusLabel.AutoSize = true;
            this.serviceStatusLabel.Location = new System.Drawing.Point(558, 39);
            this.serviceStatusLabel.Name = "serviceStatusLabel";
            this.serviceStatusLabel.Size = new System.Drawing.Size(93, 16);
            this.serviceStatusLabel.TabIndex = 2;
            this.serviceStatusLabel.Text = "Service Status";
            // 
            // buttonServiceStop
            // 
            this.buttonServiceStop.Location = new System.Drawing.Point(28, 65);
            this.buttonServiceStop.Name = "buttonServiceStop";
            this.buttonServiceStop.Size = new System.Drawing.Size(171, 23);
            this.buttonServiceStop.TabIndex = 1;
            this.buttonServiceStop.Text = "Stop";
            this.buttonServiceStop.UseVisualStyleBackColor = true;
            this.buttonServiceStop.Click += new System.EventHandler(this.buttonServiceStop_Click);
            // 
            // buttonStartService
            // 
            this.buttonStartService.Location = new System.Drawing.Point(28, 36);
            this.buttonStartService.Name = "buttonStartService";
            this.buttonStartService.Size = new System.Drawing.Size(171, 23);
            this.buttonStartService.TabIndex = 0;
            this.buttonStartService.Text = "Start";
            this.buttonStartService.UseVisualStyleBackColor = true;
            this.buttonStartService.Click += new System.EventHandler(this.buttonStartService_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.sqlConnectionStatusText);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.txtSQLPassword);
            this.groupBox1.Controls.Add(this.txtSQLUsername);
            this.groupBox1.Controls.Add(this.buttonCheckSQLConnection);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.txtSQLServer);
            this.groupBox1.Location = new System.Drawing.Point(36, 148);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox1.Size = new System.Drawing.Size(744, 109);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Database connection";
            // 
            // sqlConnectionStatusText
            // 
            this.sqlConnectionStatusText.AutoSize = true;
            this.sqlConnectionStatusText.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.sqlConnectionStatusText.Location = new System.Drawing.Point(510, 77);
            this.sqlConnectionStatusText.Name = "sqlConnectionStatusText";
            this.sqlConnectionStatusText.Size = new System.Drawing.Size(191, 16);
            this.sqlConnectionStatusText.TabIndex = 8;
            this.sqlConnectionStatusText.Text = "Disconnected from service";
            this.sqlConnectionStatusText.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(33, 83);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(70, 16);
            this.label3.TabIndex = 7;
            this.label3.Text = "Password:";
            this.label3.Click += new System.EventHandler(this.label3_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(30, 56);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(73, 16);
            this.label2.TabIndex = 6;
            this.label2.Text = "Username:";
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // txtSQLPassword
            // 
            this.txtSQLPassword.HideSelection = false;
            this.txtSQLPassword.Location = new System.Drawing.Point(169, 77);
            this.txtSQLPassword.Margin = new System.Windows.Forms.Padding(2);
            this.txtSQLPassword.Name = "txtSQLPassword";
            this.txtSQLPassword.PasswordChar = '*';
            this.txtSQLPassword.Size = new System.Drawing.Size(325, 22);
            this.txtSQLPassword.TabIndex = 5;
            this.txtSQLPassword.Text = "EHTTransferService";
            // 
            // txtSQLUsername
            // 
            this.txtSQLUsername.Location = new System.Drawing.Point(169, 52);
            this.txtSQLUsername.Margin = new System.Windows.Forms.Padding(2);
            this.txtSQLUsername.Name = "txtSQLUsername";
            this.txtSQLUsername.Size = new System.Drawing.Size(325, 22);
            this.txtSQLUsername.TabIndex = 4;
            this.txtSQLUsername.Text = "EHTTransferService";
            this.txtSQLUsername.TextChanged += new System.EventHandler(this.txtSQLUsername_TextChanged);
            // 
            // buttonCheckSQLConnection
            // 
            this.buttonCheckSQLConnection.Location = new System.Drawing.Point(513, 25);
            this.buttonCheckSQLConnection.Margin = new System.Windows.Forms.Padding(2);
            this.buttonCheckSQLConnection.Name = "buttonCheckSQLConnection";
            this.buttonCheckSQLConnection.Size = new System.Drawing.Size(171, 26);
            this.buttonCheckSQLConnection.TabIndex = 3;
            this.buttonCheckSQLConnection.Text = "Check SQL Connection";
            this.buttonCheckSQLConnection.UseVisualStyleBackColor = true;
            this.buttonCheckSQLConnection.Click += new System.EventHandler(this.btCheckSQLConnection_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(30, 30);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(119, 16);
            this.label1.TabIndex = 2;
            this.label1.Text = "SQL Server Name:";
            // 
            // txtSQLServer
            // 
            this.txtSQLServer.Location = new System.Drawing.Point(169, 27);
            this.txtSQLServer.Margin = new System.Windows.Forms.Padding(2);
            this.txtSQLServer.Name = "txtSQLServer";
            this.txtSQLServer.Size = new System.Drawing.Size(325, 22);
            this.txtSQLServer.TabIndex = 0;
            this.txtSQLServer.Text = ".\\SQLExpress01";
            this.txtSQLServer.TextChanged += new System.EventHandler(this.txSQLServer_TextChanged);
            // 
            // tabTrendGroups
            // 
            this.tabTrendGroups.AllowDrop = true;
            this.tabTrendGroups.Controls.Add(this.dataGridViewTrendGroups);
            this.tabTrendGroups.Controls.Add(this.buttonTrendGroupsPullFromSQL);
            this.tabTrendGroups.Controls.Add(this.buttonTrendGroupsSave);
            this.tabTrendGroups.Controls.Add(this.buttonTrendGroupsRefresh);
            this.tabTrendGroups.Location = new System.Drawing.Point(4, 25);
            this.tabTrendGroups.Margin = new System.Windows.Forms.Padding(2);
            this.tabTrendGroups.Name = "tabTrendGroups";
            this.tabTrendGroups.Padding = new System.Windows.Forms.Padding(2);
            this.tabTrendGroups.Size = new System.Drawing.Size(798, 375);
            this.tabTrendGroups.TabIndex = 1;
            this.tabTrendGroups.Text = "Trend Groups";
            // 
            // dataGridViewTrendGroups
            // 
            this.dataGridViewTrendGroups.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.ColumnHeader;
            this.dataGridViewTrendGroups.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewTrendGroups.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.trendGroupSelected,
            this.trendGroupName,
            this.trendGroupScanRate,
            this.trendGroupDescription,
            this.trendGroupGuid,
            this.trendGroupLastRefreshTime});
            this.dataGridViewTrendGroups.Location = new System.Drawing.Point(11, 43);
            this.dataGridViewTrendGroups.Name = "dataGridViewTrendGroups";
            this.dataGridViewTrendGroups.RowHeadersWidth = 51;
            this.dataGridViewTrendGroups.RowTemplate.Height = 24;
            this.dataGridViewTrendGroups.Size = new System.Drawing.Size(767, 313);
            this.dataGridViewTrendGroups.TabIndex = 4;
            // 
            // buttonTrendGroupsPullFromSQL
            // 
            this.buttonTrendGroupsPullFromSQL.Location = new System.Drawing.Point(287, 9);
            this.buttonTrendGroupsPullFromSQL.Name = "buttonTrendGroupsPullFromSQL";
            this.buttonTrendGroupsPullFromSQL.Size = new System.Drawing.Size(165, 28);
            this.buttonTrendGroupsPullFromSQL.TabIndex = 3;
            this.buttonTrendGroupsPullFromSQL.Text = "Pull from SQL";
            this.buttonTrendGroupsPullFromSQL.UseVisualStyleBackColor = true;
            this.buttonTrendGroupsPullFromSQL.Click += new System.EventHandler(this.buttonTrendGroupsPullFromSQL_Click);
            // 
            // buttonTrendGroupsSave
            // 
            this.buttonTrendGroupsSave.Location = new System.Drawing.Point(141, 9);
            this.buttonTrendGroupsSave.Name = "buttonTrendGroupsSave";
            this.buttonTrendGroupsSave.Size = new System.Drawing.Size(140, 28);
            this.buttonTrendGroupsSave.TabIndex = 2;
            this.buttonTrendGroupsSave.Text = "Save";
            this.buttonTrendGroupsSave.UseVisualStyleBackColor = true;
            this.buttonTrendGroupsSave.Click += new System.EventHandler(this.buttonTrendGroupsSave_Click);
            // 
            // buttonTrendGroupsRefresh
            // 
            this.buttonTrendGroupsRefresh.Location = new System.Drawing.Point(11, 9);
            this.buttonTrendGroupsRefresh.Name = "buttonTrendGroupsRefresh";
            this.buttonTrendGroupsRefresh.Size = new System.Drawing.Size(124, 28);
            this.buttonTrendGroupsRefresh.TabIndex = 1;
            this.buttonTrendGroupsRefresh.Text = "Refresh";
            this.buttonTrendGroupsRefresh.UseVisualStyleBackColor = true;
            this.buttonTrendGroupsRefresh.Click += new System.EventHandler(this.buttonTrendGroupsRefresh_Click);
            // 
            // tabLog
            // 
            this.tabLog.Controls.Add(this.richTextBox1);
            this.tabLog.Location = new System.Drawing.Point(4, 25);
            this.tabLog.Margin = new System.Windows.Forms.Padding(2);
            this.tabLog.Name = "tabLog";
            this.tabLog.Size = new System.Drawing.Size(798, 375);
            this.tabLog.TabIndex = 2;
            this.tabLog.Text = "Log";
            this.tabLog.UseVisualStyleBackColor = true;
            // 
            // richTextBox1
            // 
            this.richTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBox1.Location = new System.Drawing.Point(0, 0);
            this.richTextBox1.Margin = new System.Windows.Forms.Padding(2);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(798, 375);
            this.richTextBox1.TabIndex = 0;
            this.richTextBox1.Text = "";
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(28, 28);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.toolStripProgressBar1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 404);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 10, 0);
            this.statusStrip1.Size = new System.Drawing.Size(806, 27);
            this.statusStrip1.TabIndex = 0;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(52, 21);
            this.toolStripStatusLabel1.Text = "Status:";
            // 
            // toolStripProgressBar1
            // 
            this.toolStripProgressBar1.Name = "toolStripProgressBar1";
            this.toolStripProgressBar1.Size = new System.Drawing.Size(73, 19);
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(28, 28);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.operationToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(4, 1, 0, 1);
            this.menuStrip1.Size = new System.Drawing.Size(806, 26);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(46, 24);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(224, 26);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // operationToolStripMenuItem
            // 
            this.operationToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.setupTrendGroupsToolStripMenuItem,
            this.startToolStripMenuItem,
            this.stopToolStripMenuItem});
            this.operationToolStripMenuItem.Name = "operationToolStripMenuItem";
            this.operationToolStripMenuItem.Size = new System.Drawing.Size(90, 24);
            this.operationToolStripMenuItem.Text = "Operation";
            // 
            // setupTrendGroupsToolStripMenuItem
            // 
            this.setupTrendGroupsToolStripMenuItem.Name = "setupTrendGroupsToolStripMenuItem";
            this.setupTrendGroupsToolStripMenuItem.Size = new System.Drawing.Size(224, 26);
            this.setupTrendGroupsToolStripMenuItem.Text = "Setup TrendGroups";
            this.setupTrendGroupsToolStripMenuItem.Click += new System.EventHandler(this.setupTrendGroupsToolStripMenuItem_Click);
            // 
            // startToolStripMenuItem
            // 
            this.startToolStripMenuItem.Name = "startToolStripMenuItem";
            this.startToolStripMenuItem.Size = new System.Drawing.Size(224, 26);
            this.startToolStripMenuItem.Text = "Start";
            this.startToolStripMenuItem.Click += new System.EventHandler(this.startToolStripMenuItem_Click);
            // 
            // stopToolStripMenuItem
            // 
            this.stopToolStripMenuItem.Name = "stopToolStripMenuItem";
            this.stopToolStripMenuItem.Size = new System.Drawing.Size(224, 26);
            this.stopToolStripMenuItem.Text = "Stop";
            this.stopToolStripMenuItem.Click += new System.EventHandler(this.stopToolStripMenuItem_Click);
            // 
            // folderBrowserDialog1
            // 
            this.folderBrowserDialog1.HelpRequest += new System.EventHandler(this.folderBrowserDialog1_HelpRequest);
            // 
            // trendGroupSelected
            // 
            this.trendGroupSelected.HeaderText = "Monitored";
            this.trendGroupSelected.MinimumWidth = 6;
            this.trendGroupSelected.Name = "trendGroupSelected";
            this.trendGroupSelected.Width = 73;
            // 
            // trendGroupName
            // 
            this.trendGroupName.HeaderText = "Trend Group Name";
            this.trendGroupName.MinimumWidth = 6;
            this.trendGroupName.Name = "trendGroupName";
            this.trendGroupName.ReadOnly = true;
            this.trendGroupName.Width = 139;
            // 
            // trendGroupScanRate
            // 
            this.trendGroupScanRate.HeaderText = "Scan Rate (s)";
            this.trendGroupScanRate.MinimumWidth = 6;
            this.trendGroupScanRate.Name = "trendGroupScanRate";
            this.trendGroupScanRate.Width = 94;
            // 
            // trendGroupDescription
            // 
            this.trendGroupDescription.HeaderText = "Description";
            this.trendGroupDescription.MinimumWidth = 6;
            this.trendGroupDescription.Name = "trendGroupDescription";
            this.trendGroupDescription.ReadOnly = true;
            this.trendGroupDescription.Width = 104;
            // 
            // trendGroupGuid
            // 
            this.trendGroupGuid.HeaderText = "GUID";
            this.trendGroupGuid.MinimumWidth = 6;
            this.trendGroupGuid.Name = "trendGroupGuid";
            this.trendGroupGuid.ReadOnly = true;
            this.trendGroupGuid.Width = 69;
            // 
            // trendGroupLastRefreshTime
            // 
            this.trendGroupLastRefreshTime.HeaderText = "Last Refresh Time";
            this.trendGroupLastRefreshTime.MinimumWidth = 6;
            this.trendGroupLastRefreshTime.Name = "trendGroupLastRefreshTime";
            this.trendGroupLastRefreshTime.ReadOnly = true;
            this.trendGroupLastRefreshTime.Width = 133;
            // 
            // RCSForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(806, 457);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "RCSForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Raychem Supervisor Trend Data Table Transfer Utility";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabService.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tabTrendGroups.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewTrendGroups)).EndInit();
            this.tabLog.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabService;
        private System.Windows.Forms.TabPage tabTrendGroups;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtSQLServer;
        private System.Windows.Forms.Button buttonCheckSQLConnection;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar1;
        private System.Windows.Forms.ToolStripMenuItem operationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem startToolStripMenuItem;
        private System.Windows.Forms.TabPage tabLog;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.ToolStripMenuItem stopToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem setupTrendGroupsToolStripMenuItem;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtSQLPassword;
        private System.Windows.Forms.TextBox txtSQLUsername;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button buttonStartService;
        private System.Windows.Forms.Label serviceStatusLabel;
        private System.Windows.Forms.Button buttonServiceStop;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button buttonTrendGroupsRefresh;
        private System.Windows.Forms.Button buttonTrendGroupsSave;
        private System.Windows.Forms.DataGridView dataGridViewTrendGroups;
        private System.Windows.Forms.Button buttonTrendGroupsPullFromSQL;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.Button buttonChooseLogFolderDirectory;
        private System.Windows.Forms.Button buttonCheckServiceConnection;
        private System.Windows.Forms.Label txtPathToLogDirectory;
        private System.Windows.Forms.Label sqlConnectionStatusText;
        private System.Windows.Forms.Label serviceConnectionStatusText;
        private System.Windows.Forms.DataGridViewCheckBoxColumn trendGroupSelected;
        private System.Windows.Forms.DataGridViewTextBoxColumn trendGroupName;
        private System.Windows.Forms.DataGridViewTextBoxColumn trendGroupScanRate;
        private System.Windows.Forms.DataGridViewTextBoxColumn trendGroupDescription;
        private System.Windows.Forms.DataGridViewTextBoxColumn trendGroupGuid;
        private System.Windows.Forms.DataGridViewTextBoxColumn trendGroupLastRefreshTime;
    }
}

