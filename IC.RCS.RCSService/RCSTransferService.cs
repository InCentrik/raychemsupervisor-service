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

        public RCSTransferService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {

            host = new ServiceHost(typeof(RCSCore.RCSWCFService), new Uri[] { new Uri("net.pipe://localhost") });
            host.AddServiceEndpoint(typeof(RCSCore.IRCSWCFService), new NetNamedPipeBinding(), "PipeReverse");
            host.Open();

        }

        protected override void OnStop()
        {
            try 
            { 
                host.Close(); 
            } catch 
            { 
            
            }
        }
    }
}
