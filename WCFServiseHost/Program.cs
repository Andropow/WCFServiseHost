using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel;
using System.ServiceModel.Description;
using ServiceAssembly;

namespace WCFServiseHost
{
    static class Program
    {        
        static void Main(string[] args)
        {
            var ip = Dns.GetHostEntry(Dns.GetHostName()).AddressList.First(i=>i.AddressFamily == AddressFamily.InterNetwork);
            var tcpUri = new Uri(string.Format("net.tcp://{0}:{1}/WPFHost/", ip, 7987));
            
            var serviceHost = new ServiceHost(typeof(ChatService));
 
            //serviceHost.AddServiceEndpoint(typeof (IMetadataExchange), MetadataExchangeBindings.CreateMexTcpBinding(),
            //   new Uri(string.Format("net.tcp://{0}:{1}/WPFHost/mex", ip, 7997)));
            try
            {
                serviceHost.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (serviceHost.State == CommunicationState.Opened)
                    Console.WriteLine("Service Opend\n\nIf You Want Close Service Enter x");
            }
            while (Console.ReadKey().Key != ConsoleKey.X) { }

            try
            {
                serviceHost.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }
    }
}
