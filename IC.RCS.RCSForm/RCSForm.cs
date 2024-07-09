using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.ServiceModel;
using IC.RCS.RCSCore;
using System.Configuration;
using System.ServiceProcess;

namespace IC.RCS.RCSForm
{
    public partial class RCSForm : Form
    {
        public ChannelFactory<IRCSWCFService> serviceChannelFactory;
        public ServiceController serviceController = new ServiceController("RCSTransferService");
        private ServiceHost _formServiceHost;
        private RCSLogHandler _logger = new RCSLogHandler("Form");

        public RCSForm()
        {

            InitializeComponent();

        }

        private Configuration GetServiceConfiguration()
        {
            try
            {
                IRCSWCFService client = serviceChannelFactory.CreateChannel();
                string serviceExePath = client.GetExePath();
                return ConfigurationManager.OpenExeConfiguration(serviceExePath);
            }
            catch (Exception ex)
            {
                _logger.Log(RCSLogLevel.Error, "Unable to get service configuration");
                return null;
            }

        }

        private TrendGroupConfig GetTrendGroupsConfig()
        {
            try
            {
                Configuration config = GetServiceConfiguration();
                if (config != null)
                {
                    return (TrendGroupConfig)config.GetSection("trendGroupsConfig");
                }
                else
                {
                    return null;
                }

            }
            catch (Exception ex)
            {
                _logger.Log(RCSLogLevel.Error, "Unable to get trend groups configuration");
                return null;
            }


        }

        private void GetSqlClientConnectionCredentials()
        {
            try
            {
                IRCSWCFService client = serviceChannelFactory.CreateChannel();
                string serviceExePath = client.GetExePath();
                Configuration config = GetServiceConfiguration();

                txtSQLServer.Text = config.AppSettings.Settings["servername"].Value;
                txtSQLUsername.Text = config.AppSettings.Settings["username"].Value;
                txtSQLPassword.Text = config.AppSettings.Settings["password"].Value;
            }
            catch (Exception ex)
            {
                _logger.Log(RCSLogLevel.Error, "Unable to get SQL client credentials");
            }

        }

        private void SetSqlClientConnectionCredentials(string serverName, string databaseName, string userName, string password)
        {
            //TODO:use this after checking that the credentials work to set the credentials used by the service and restart it
            try
            {
                Configuration config = GetServiceConfiguration();

                config.AppSettings.Settings["servername"].Value = txtSQLServer.Text;
                config.AppSettings.Settings["username"].Value = txtSQLUsername.Text;
                config.AppSettings.Settings["password"].Value = txtSQLPassword.Text;

                config.Save(ConfigurationSaveMode.Modified);

                config = null;
            }
            catch (Exception ex)
            {
                _logger.Log(RCSLogLevel.Error, "Unable to set SQL client credentials");
            }

        }

        private void ConvertTrendGroupConfigToDataGridView()
        {
            try
            {
                dataGridViewTrendGroups.Rows.Clear();

                TrendGroupConfig trendGroupsConfig = GetTrendGroupsConfig();

                if (trendGroupsConfig != null)
                {
                    int i = 0;
                    foreach (TrendGroupElement trendGroup in trendGroupsConfig.TrendGroups)
                    {
                        dataGridViewTrendGroups.Rows.Add();
                        dataGridViewTrendGroups.Rows[i].Cells[0].Value = trendGroup.IsMonitored == "true" ? true : false;
                        dataGridViewTrendGroups.Rows[i].Cells[1].Value = trendGroup.Name;
                        dataGridViewTrendGroups.Rows[i].Cells[2].Value = trendGroup.ScanRate;
                        dataGridViewTrendGroups.Rows[i].Cells[3].Value = trendGroup.PullDays;
                        dataGridViewTrendGroups.Rows[i].Cells[4].Value = trendGroup.Description;
                        dataGridViewTrendGroups.Rows[i].Cells[5].Value = trendGroup.Guid;
                        dataGridViewTrendGroups.Rows[i].Cells[6].Value = trendGroup.LastRefreshTime;


                        i++;
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.Log(RCSLogLevel.Error, "Unable to populate trend group table");
            }


        }

        private void ConvertDataGridViewtoTrendGroupConfig()
        {
            try
            {
                Configuration config = GetServiceConfiguration();
                TrendGroupConfig trendGroupsConfig = (TrendGroupConfig)config.GetSection("trendGroupsConfig");

                int i = 0;

                foreach (TrendGroupElement trendGroup in trendGroupsConfig.TrendGroups)
                {
                    trendGroup.IsMonitored = dataGridViewTrendGroups.Rows[i].Cells[0].Value.ToString().ToLower();
                    trendGroup.ScanRate = dataGridViewTrendGroups.Rows[i].Cells[2].Value.ToString().ToLower();
                    trendGroup.PullDays = dataGridViewTrendGroups.Rows[i].Cells[3].Value.ToString().ToLower();

                    i++;
                }
                config.Save(ConfigurationSaveMode.Modified);

                config = null;
            }
            catch (Exception ex)
            {
                _logger.Log(RCSLogLevel.Error, "Unable to set service trend groups configuration");
            }

        }

        private ServiceConnectionStatusEnum CheckServiceConnection()
        {
            bool serviceOn = false;
            bool wcfServiceConnected = false;

            try
            {
                bool canStop = serviceController.CanStop;
                serviceOn = true;
            }
            catch
            {
                serviceOn = false;
            }

            try
            {
                var trendGroupsConfig = GetTrendGroupsConfig();
                if (trendGroupsConfig != null)
                {
                    foreach (TrendGroupElement trendGroup in trendGroupsConfig.TrendGroups)
                    {
                        System.Diagnostics.Debug.WriteLine("{0},{1},{2},{3}", trendGroup.Guid, trendGroup.Name, trendGroup.Description, trendGroup.IsMonitored);
                    }
                    wcfServiceConnected = true;
                }
                serviceOn = true;
            }
            catch (Exception ex)
            {

            }

            int serviceOnInt = serviceOn ? 1 : 0;
            int wcfServiceConnectedInt = wcfServiceConnected ? 1 : 0;

            return (ServiceConnectionStatusEnum)serviceOnInt + wcfServiceConnectedInt;

        }

        private void CheckServiceSqlConnection()
        {
            string connectionStatusText;
            string userName = txtSQLUsername.Text;
            string password = txtSQLPassword.Text;
            string serverName = txtSQLServer.Text;

            ServiceConnectionStatusEnum serviceConnectionStatus = CheckServiceConnection();

            if (serviceConnectionStatus == ServiceConnectionStatusEnum.Connected)
            {
                IRCSWCFService client = serviceChannelFactory.CreateChannel();
                if (client.TestSQLCredentials(serverName, "ehtplus", userName, password))
                {
                    SetSqlClientConnectionCredentials(serverName, "ehtplus", userName, password);
                    connectionStatusText = "Connected";
                }
                else
                {
                    connectionStatusText = "Failed";
                }
            }
            else
            {
                connectionStatusText = "Disconnected";
            }

            sqlConnectionStatusText.Text = connectionStatusText;
        }

        private void buttonCheckServiceConnection_Click(object sender, EventArgs e)
        {
            string connectionStatusText;

            ServiceConnectionStatusEnum serviceConnectionStatus = CheckServiceConnection();
            switch (serviceConnectionStatus)
            {
                case ServiceConnectionStatusEnum.Connected:
                    connectionStatusText = "Connected";
                    break;
                case ServiceConnectionStatusEnum.CommunicationOff:
                    connectionStatusText = "Disconnected";
                    break;
                case ServiceConnectionStatusEnum.Disconnected:
                    connectionStatusText = "Disconnected";
                    break;
                default:
                    connectionStatusText = "ERROR";
                    break;
            }

            serviceConnectionStatusText.Text = connectionStatusText;

        }

        private void btCheckSQLConnection_Click(object sender, EventArgs e)
        {

            CheckServiceSqlConnection();

        }

        private void RCSForm_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void RCSForm_Load(object sender, EventArgs e)
        {
            serviceChannelFactory = new ChannelFactory<IRCSWCFService>(new NetNamedPipeBinding(), new EndpointAddress("net.pipe://localhost/RCSTransferService/RCSTransferService"));

            RCSFormWCFService formService = new RCSFormWCFService(txtBoxLog);

            _formServiceHost = new ServiceHost(formService, new Uri[] { new Uri("net.pipe://localhost/RCSTransferForm") });
            _formServiceHost.Description.Behaviors.Find<ServiceBehaviorAttribute>().InstanceContextMode = InstanceContextMode.Single;
            _formServiceHost.AddServiceEndpoint(typeof(IRCSFormWCFService), new NetNamedPipeBinding(), "RCSTransferForm");

            _formServiceHost.Open();
        }

        private void RCSForm_Shown(object sender, EventArgs e)
        {
            buttonCheckServiceConnection.PerformClick();
            CheckServiceSqlConnection();
            GetSqlClientConnectionCredentials();
            GetLogFolderDirectory();
            ConvertTrendGroupConfigToDataGridView();

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

        private void GetLogFolderDirectory()
        {
            try
            {
                Configuration config = GetServiceConfiguration();
                if (config != null)
                {
                    txtPathToLogDirectory.Text = config.AppSettings.Settings["logfilepath"].Value;
                }

            }
            catch (Exception ex)
            {

            }
        }

        private void SetLogFolderDirectory(string path)
        {
            try
            {
                Configuration config = GetServiceConfiguration();

                if (config != null)
                {
                    config.AppSettings.Settings["logfilepath"].Value = path;
                    config.Save(ConfigurationSaveMode.Modified);
                    txtPathToLogDirectory.Text = folderBrowserDialog1.SelectedPath;
                    config = null;
                }

            }
            catch (Exception ex)
            {

            }
        }

        private void buttonChooseLogFolderDirectory_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {

                SetLogFolderDirectory(folderBrowserDialog1.SelectedPath);

            }
        }

        private void buttonStartService_Click(object sender, EventArgs e)
        {
            //TODO: FIGURE OUT WHY NOT WORKING
            try
            {
                serviceController.Start();
            }
            catch
            {

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
            ConvertTrendGroupConfigToDataGridView();
        }

        private void buttonTrendGroupsSave_Click(object sender, EventArgs e)
        {
            ConvertDataGridViewtoTrendGroupConfig();
        }

        private void buttonTrendGroupsPullFromSQL_Click(object sender, EventArgs e)
        {
            try
            {
                string username = txtSQLUsername.Text;
                string password = txtSQLPassword.Text;
                string serverName = txtSQLServer.Text;
                string databaseName = "ehtplus";

                IRCSWCFService client = serviceChannelFactory.CreateChannel();
                client.PullTrendGroupsFromSQL(serverName, databaseName, username, password);
                ConvertTrendGroupConfigToDataGridView();
            }
            catch
            {

            }

        }

        private void txtBoxLog_GotFocus(object sender, EventArgs e)
        {
            try
            {
                _logger.Log(RCSLogLevel.Information, "GOT FOCUS");
            } catch
            {

            }
        }
    }
}
