using Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WCFClient.Proxies;

namespace WCFClient
{
    public class Program
    {
        static void Main(string[] args)
        {
            using (ConsumptionProxy proxy = new ConsumptionProxy())
            {
                DailyConsumptionSample sample = new DailyConsumptionSample
                {
                    Date = DateTime.Now,
                    TotalForecastMWh = 12,
                    TotalActualMWh = 10,
                    PeakTime = DateTime.Now,
                    PeakActualMW = 5,
                    CountryCode = "RS",
                    RowIndex = 1
                };

                proxy.Send(sample);
            }

            Console.ReadLine();
        }
    }
}
