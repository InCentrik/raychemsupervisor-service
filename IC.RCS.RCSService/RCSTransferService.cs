using IC.RCS.RCSCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace IC.RCS.RCSService
{
    public partial class RCSTransferService : ServiceBase
    {
        public RCSCore.EHTSQLClient sqlClient;
        public ServiceHost host;
        private RCSLogHandler _logger = new RCSLogHandler("Service");

        public RCSTransferService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                RCSWCFService service = new RCSWCFService();

                ServiceHost host = new ServiceHost(service, new Uri[] { new Uri("net.pipe://localhost/RCSTransferService") });
                host.Description.Behaviors.Find<ServiceBehaviorAttribute>().InstanceContextMode = InstanceContextMode.Single;
                host.AddServiceEndpoint(typeof(IRCSWCFService), new NetNamedPipeBinding(), "RCSTransferService");

                host.Open();

                _logger.Log(RCSLogLevel.Information, "RCS transfer service started");
            } catch
            {

            }
            

        }

        protected override void OnStop()
        {
            try 
            { 
                host.Close();
                _logger.Log(RCSLogLevel.Information, "RCS transfer service stopped");
            }
            catch 
            { 
            
            }
        }
    }
}
