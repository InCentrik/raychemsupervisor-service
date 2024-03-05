using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.ServiceModel;
using IC.RCS.RCSCore;
using System.Configuration;
using System.ServiceProcess;
using System.Data;
using System.Text;
using System.Drawing.Text;
using System.CodeDom;
using static System.Net.Mime.MediaTypeNames;

namespace IC.RCS.RCSForm
{
    public partial class RCSForm : Form
    {
        public ChannelFactory<IRCSWCFService> myChannelFactory;
        public ServiceController serviceController = new ServiceController("RCSTransferService");

        public RCSForm()
        {
            
            var binding = new NetNamedPipeBinding();
            var endpoint = new EndpointAddress("net.pipe://localhost/Design_Time_Addresses/IC.RCS.RCSCore/RCSCore");
            myChannelFactory = new ChannelFactory<IRCSWCFService>(new NetNamedPipeBinding(), new EndpointAddress("net.pipe://localhost/PipeReverse"));

            InitializeComponent();
        }

        private RCSCore.TrendGroupConfig GetTrendGroupsConfig()
        {
            IRCSWCFService client = myChannelFactory.CreateChannel();
            string serviceExePath = client.GetExePath();
            Configuration config = ConfigurationManager.OpenExeConfiguration(serviceExePath);
            return (RCSCore.TrendGroupConfig)config.GetSection("trendGroupsConfig");
        }

        private void ConvertTrendGroupConfigToDataGridView()
        {
            dataGridViewTrendGroups.Rows.Clear();

            RCSCore.TrendGroupConfig trendGroupsConfig = GetTrendGroupsConfig();

            int i = 0;
            foreach (TrendGroupElement trendGroup in trendGroupsConfig.TrendGroups)
            {
                dataGridViewTrendGroups.Rows.Add();
                dataGridViewTrendGroups.Rows[i].Cells[0].Value = trendGroup.IsMonitored == "true" ? true : false;
                dataGridViewTrendGroups.Rows[i].Cells[1].Value = trendGroup.Name;
                dataGridViewTrendGroups.Rows[i].Cells[2].Value = trendGroup.ScanRate;
                dataGridViewTrendGroups.Rows[i].Cells[3].Value = trendGroup.Description;
                dataGridViewTrendGroups.Rows[i].Cells[4].Value = trendGroup.Guid;
                dataGridViewTrendGroups.Rows[i].Cells[5].Value = trendGroup.LastRefreshTime;


                i++;
            }

        }

        private void ConvertDataGridViewtoTrendGroupConfig()
        {
            IRCSWCFService client = myChannelFactory.CreateChannel();
            string serviceExePath = client.GetExePath();

            Configuration config = ConfigurationManager.OpenExeConfiguration(serviceExePath);
            TrendGroupConfig trendGroupsConfig = (TrendGroupConfig)config.GetSection("trendGroupsConfig");


            int i = 0;

            foreach (TrendGroupElement trendGroup in trendGroupsConfig.TrendGroups)
            {
                trendGroup.IsMonitored = dataGridViewTrendGroups.Rows[i].Cells[0].Value.ToString().ToLower(); 
                trendGroup.ScanRate = dataGridViewTrendGroups.Rows[i].Cells[2].Value.ToString().ToLower();
                //dataGridViewTrendGroups.Rows[i].Cells[1].Value = trendGroup.Name;
                //dataGridViewTrendGroups.Rows[i].Cells[2].Value = trendGroup.Description;
                //dataGridViewTrendGroups.Rows[i].Cells[3].Value = trendGroup.Guid;
                //dataGridViewTrendGroups.Rows[i].Cells[4].Value = trendGroup.LastRefreshTime;

                i++;
            }
            config.Save();
        }

            private ServiceConnectionStatusEnum CheckServiceConnection()
        {
            bool serviceOn = true;
            bool wcfServiceConnected = true;

            try
            {
                bool canStop = serviceController.CanStop;
            } catch
            {
                serviceOn = false;
            }

            try {
                var trendGroupsConfig = GetTrendGroupsConfig();
                foreach (RCSCore.TrendGroupElement trendGroup in trendGroupsConfig.TrendGroups)
                {
                    System.Diagnostics.Debug.WriteLine("{0},{1},{2},{3}", trendGroup.Guid, trendGroup.Name, trendGroup.Description, trendGroup.IsMonitored);
                }
                wcfServiceConnected = true;
            }
            catch(Exception ex) {
                serviceOn = false;
            }

            int serviceOnInt = serviceOn ? 1 : 0;
            int wcfServiceConnectedInt = wcfServiceConnected ? 1 : 0;

            return (ServiceConnectionStatusEnum) serviceOnInt + wcfServiceConnectedInt;

        }

        private void buttonCheckServiceConnection_Click(object sender, EventArgs e)
        {
            string connectionStatusText;

            ServiceConnectionStatusEnum serviceConnectionStatus = CheckServiceConnection();
            switch (serviceConnectionStatus)
            {
                case ServiceConnectionStatusEnum.Connected:
                    connectionStatusText = "Connected to service";
                    break;
                case ServiceConnectionStatusEnum.CommunicationOff:
                    connectionStatusText = "Service is disconnected from GUI";
                    break;
                case ServiceConnectionStatusEnum.Disconnected:
                    connectionStatusText = "Service connected";
                    break;
                default:
                    connectionStatusText = "ERROR";
                    break;
            }

            serviceConnectionStatusText.Text = connectionStatusText;
            
        }

        private void btCheckSQLConnection_Click(object sender, EventArgs e)
        {
            string connectionStatusText;
            string username = txtSQLUsername.Text;
            string password = txtSQLPassword.Text;
            string serverName = txtSQLServer.Text;

            ServiceConnectionStatusEnum serviceConnectionStatus = CheckServiceConnection();

            if (serviceConnectionStatus == ServiceConnectionStatusEnum.Connected)
            {
                IRCSWCFService client = myChannelFactory.CreateChannel();
                if (client.ConfigureSQLConnection(serverName,"ehtplus",username,password))
                {
                    connectionStatusText = "SQL connected to service";
                } else
                {
                    connectionStatusText = "SQL connection failed";
                }
            } else
            {
                connectionStatusText = "No connection from GUI to service";
            } 

            sqlConnectionStatusText.Text = connectionStatusText;


        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            
        }

        private void btStartStopMonitor_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        #region worker thread
        private delegate void ShowHideCursorDelegate(bool waitstate);
        delegate void ShowStatusMessageDelegate(string msg);
        delegate void ShowMessageBoxDelegate(string msg);
        delegate void ShowClientExceptionMessageDelegate(Exception ex);
        delegate void JobFailedDelegate(Exception ex);
        delegate void AppendMessageToDiagnosticsWindowDelegate(string msg);
        delegate void UpdateListViewItemsDelegate(string msg);

        // worker thread routine for UI threading calls
        private void ShowHideWaitCursor(bool waitstate)
        {
            if (InvokeRequired)
            {
                this.BeginInvoke(new ShowHideCursorDelegate(this.ShowHideWaitCursor), waitstate);
            }
            else
            {
            }
        }

        private void ShowStatusMessage(string msg)
        {
            if (InvokeRequired)
            {
                this.BeginInvoke(new ShowStatusMessageDelegate(this.ShowStatusMessage), new object[] { msg });
            }
            else
            {
            }
        }

        private void AppendMessageToDiagnosticsWindow(string msg)
        {
            if (InvokeRequired)
            {
                this.BeginInvoke(new AppendMessageToDiagnosticsWindowDelegate(this.AppendMessageToDiagnosticsWindow), new object[] { msg });
            }
            else
            {
            }
        }

        private void UpdateListViewItems(string msg)
        {
            if (InvokeRequired)
            {
                this.BeginInvoke(new UpdateListViewItemsDelegate(this.UpdateListViewItems), new object[] { msg });
            }
            else
            {
            }
        }

        private void ShowMessageBox(string msg)
        {
            if (InvokeRequired)
            {
                this.BeginInvoke(new ShowStatusMessageDelegate(this.ShowMessageBox), new object[] { msg });
            }
            else
            {
            }
        }

        private void ShowClientExceptionMessage(Exception ex)
        {
            if (InvokeRequired)
            {
                this.BeginInvoke(new ShowClientExceptionMessageDelegate(this.ShowClientExceptionMessage), new object[] { ex });
            }
            else
            {
            }
        }

        private void JobFailed(Exception ex)
        {
            if (InvokeRequired)
            {
                this.BeginInvoke(new JobFailedDelegate(this.JobFailed), new object[] { ex });
            }
            else
            {
            }
        }

        private void JobFinished()
        {
            if (InvokeRequired)
            {
                this.BeginInvoke(new MethodInvoker(this.JobFinished), null);
            }
            else
            {
            }
        }
        #endregion

        #region toolstrip menu

        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            {
            }
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void setupTrendGroupsToolStripMenuItem_Click(object sender, EventArgs e)
        {
          
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }
        #endregion

        private void cbScanInterval_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void cbDeleteOldestDay_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void txSQLServer_TextChanged(object sender, EventArgs e)
        {
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void groupBox3_Enter(object sender, EventArgs e)
        {

        }

        private void folderBrowserDialog1_HelpRequest(object sender, EventArgs e)
        {

        }

        private void buttonChooseLogFolderDirectory_Click(object sender, EventArgs e)
        {
            if(folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                txtPathToLogDirectory.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void buttonStartService_Click(object sender, EventArgs e)
        {
            //TODO: FIGURE OUT WHY NOT WORKING
            try {
                serviceController.Start();
            } 
            catch {

            }
        }

        private void buttonServiceStop_Click(object sender, EventArgs e)
        {
            //TODO: FIGURE OUT WHY NOT WORKING

            try
            {
                serviceController.Stop();
            }
            catch
            {

            }
        }

        private void txtSQLUsername_TextChanged(object sender, EventArgs e)
        {

        }

        private void buttonTrendGroupsRefresh_Click(object sender, EventArgs e)
        {
            TrendGroupConfig trendGroupConfig = GetTrendGroupsConfig();
            ConvertTrendGroupConfigToDataGridView();
        }

        private void buttonTrendGroupsSave_Click(object sender, EventArgs e)
        {
            ConvertDataGridViewtoTrendGroupConfig();
        }

        private void buttonTrendGroupsPullFromSQL_Click(object sender, EventArgs e)
        {
            string username = txtSQLUsername.Text;
            string password = txtSQLPassword.Text;
            string serverName = txtSQLServer.Text;
            string databaseName = "ehtplus";

            IRCSWCFService client = myChannelFactory.CreateChannel();
            client.PullTrendGroupsFromSQL(serverName,databaseName,username,password);
            ConvertTrendGroupConfigToDataGridView();
        }
    }
}
