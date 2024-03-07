using System;
using System.ServiceModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using IC.RCS;
using IC.RCS.RCSCore;
using System.ServiceModel.Channels;
using System.Data;
using System.Configuration.Install;
using System.Reflection;

namespace IC.RCS.RCSService
{
    internal static class Program
    {
        static void Main(string[] args)
        {
            bool IsDebugState = false;

            if (Environment.UserInteractive && !IsDebugState)
            {
                var parameter = string.Concat(args);
                switch (parameter)
                {
                    case "--install":
                        ManagedInstallerClass.InstallHelper(new[] { Assembly.GetExecutingAssembly().Location });
                        break;
                    case "--uninstall":
                        ManagedInstallerClass.InstallHelper(new[] { "/u", Assembly.GetExecutingAssembly().Location });
                        break;
                }
            }
            else
            {
                if (!IsDebugState)
                {
                    // Startup as service.
                    ServiceBase[] ServicesToRun;
                    ServicesToRun = new ServiceBase[]
                    {
                new RCSTransferService()
                    };
                    ServiceBase.Run(ServicesToRun);
                }
                else
                {

                    //System.Diagnostics.Debugger.Launch();
                    Console.WriteLine("Debug mode...");
                    EHTSQLClient sqlClient = new EHTSQLClient(".\\SQLExpress", "ehtplus", "ehtTransferService", "ehtTransferService");

                    TrendGroupConfigReaderWriter trendGroupConfigReaderWriter = new TrendGroupConfigReaderWriter(null);
                    RCSWCFService wcfService = new RCSWCFService();


                    ServiceHost host = new ServiceHost(typeof(RCSWCFService), new Uri[] { new Uri("net.pipe://localhost") });
                    host.AddServiceEndpoint(typeof(IRCSWCFService), new NetNamedPipeBinding(), "PipeReverse");
                    host.Open();

                    Console.WriteLine("Done. Press enter to close.");
                    Console.ReadLine();

                }
            }
            

        }



    }
}
