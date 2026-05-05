using Common.Faults;
using Common.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using WCFClient.Proxies;
using WCFClient.Readers;

namespace WCFClient
{
    public class Program
    {
        static void Main(string[] args)
        {
            string csvPutanja = ConfigurationManager.AppSettings["CsvFilePath"];
            string kodDrzave = ConfigurationManager.AppSettings["CountryCode"];
            string odbaceniPutanja = ConfigurationManager.AppSettings["RejectedFilePath"];

            Console.WriteLine("Citanje CSV fajla: " + csvPutanja);
            Console.WriteLine("Kod drzave: " + kodDrzave);

            List<DailyConsumptionSample> uzorci = new List<DailyConsumptionSample>();

            using (CsvReader reader = new CsvReader(csvPutanja, kodDrzave, odbaceniPutanja))
            {
                uzorci = reader.UcitajUzorke();
            }

            Console.WriteLine("Ucitano uzoraka: " + uzorci.Count);

            if (uzorci.Count == 0)
            {
                Console.WriteLine("Nema podataka za slanje.");
                Console.ReadLine();
                return;
            }

            SessionMeta meta = new SessionMeta
            {
                CountryCode = kodDrzave,
                YearMonth = uzorci[0].Date.ToString("yyyy-MM"),
                SourceFileName = Path.GetFileName(csvPutanja),
                TotalDays = uzorci.Count
            };

            using (ConsumptionProxy proxy = new ConsumptionProxy())
            {
                proxy.StartSession(meta);
                Console.WriteLine("Sesija zapoceta.");

                foreach (DailyConsumptionSample uzorak in uzorci)
                {
                    try
                    {
                        proxy.PushSample(uzorak);
                        Console.WriteLine("[" + uzorak.RowIndex + "] " + uzorak.Date.ToString("yyyy-MM-dd") + " - Actual: " + uzorak.TotalActualMWh.ToString("F2") + " MWh, Forecast: " + uzorak.TotalForecastMWh.ToString("F2") + " MWh");
                    }
                    catch (System.ServiceModel.FaultException<ValidationFault> ex)
                    {
                        Console.WriteLine("Greska validacije za " + uzorak.Date.ToString("yyyy-MM-dd") + ": " + ex.Detail.Message);
                    }
                    catch (System.ServiceModel.FaultException<DataFormatFault> ex)
                    {
                        Console.WriteLine("Greska formata za " + uzorak.Date.ToString("yyyy-MM-dd") + ": " + ex.Detail.Message);
                    }
                }

                proxy.EndSession();
                Console.WriteLine("Sesija zavrsena.");
            }

            Console.ReadLine();
        }
    }
}
