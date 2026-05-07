using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using WCFServer.Services;

namespace WCFServer
{
    public class Program
    {
        static void Main(string[] args)
        {
            ServiceHost host = new ServiceHost(typeof(ConsumptionService));
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
                }
                else
                {
                    host.Close();
                }
            }
        }
    }
}
