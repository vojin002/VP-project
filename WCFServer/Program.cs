using Common.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using WCFServer.Listeners;
using WCFServer.Services;

namespace WCFServer
{
    public class Program
    {
        static void Main(string[] args)
        {
            string rootFolder = "Data";
            
            ConsumptionService service = new ConsumptionService();

            FileStorageListener fileStorageListener = new FileStorageListener(rootFolder);

            service.OnTransferStarted += fileStorageListener.HandleTransferStarted;
            service.OnSampleReceived += fileStorageListener.HandleSampleReceived;
            service.OnTransferCompleted += fileStorageListener.HandleTransferCompleted;

            ServiceHost host = new ServiceHost(service);
            try
            {
                host.Open();
                Console.WriteLine("Service working??....");
                Console.ReadLine();
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
            }
            finally
            {
                if (host.State == CommunicationState.Faulted)
                {
                    host.Abort();
                    Console.WriteLine("Service closed successfully after connection was in a Faulted state");
                }
                else
                {
                    host.Close();
                    Console.WriteLine("Service closed successfully");
                }
            }

            Console.WriteLine("\n Exited... Press anything");
            Console.ReadLine();
        }
    }
}
