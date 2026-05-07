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
            string csvPath = ConfigurationManager.AppSettings["CsvFilePath"];
            string countryCode = ConfigurationManager.AppSettings["CountryCode"];
            string rejectedPath = ConfigurationManager.AppSettings["RejectedFilePath"];

            Console.WriteLine("Reading from CSV file: " + csvPath);
            Console.WriteLine("Country Code: " + countryCode);

            List<DailyConsumptionSample> samples = new List<DailyConsumptionSample>();

            using (CsvReader reader = new CsvReader(csvPath, countryCode, rejectedPath))
            {
                samples = reader.LoadSamples();
            }

            Console.WriteLine("Samples loaded count: " + samples.Count);

            if (samples.Count == 0)
            {
                Console.WriteLine("No data for sending.");
                Console.ReadLine();
                return;
            }

            SessionMeta meta = new SessionMeta
            {
                CountryCode = countryCode,
                YearMonth = samples[0].Date.ToString("yyyy-MM"),
                SourceFileName = Path.GetFileName(csvPath),
                TotalDays = samples.Count
            };

            using (ConsumptionProxy proxy = new ConsumptionProxy())
            {
                proxy.StartSession(meta);
                Console.WriteLine("Session started.");

                foreach (DailyConsumptionSample sample in samples)
                {
                    try
                    {
                        proxy.PushSample(sample);
                        Console.WriteLine("[" + sample.RowIndex + "] " + sample.Date.ToString("yyyy-MM-dd") + "  Actual: " + sample.TotalActualMWh.ToString("F2") + " MWh, Forecast: " + sample.TotalForecastMWh.ToString("F2") + " MWh");
                    }
                    catch (System.ServiceModel.FaultException<ValidationFault> ex)
                    {
                        Console.WriteLine("Validation error: " + sample.Date.ToString("yyyy-MM-dd") + ": " + ex.Detail.Message);
                    }
                    catch (System.ServiceModel.FaultException<DataFormatFault> ex)
                    {
                        Console.WriteLine("Format error: " + sample.Date.ToString("yyyy-MM-dd") + ": " + ex.Detail.Message);
                    }
                }

                proxy.EndSession();
                Console.WriteLine("Session ended.");
            }

            Console.ReadLine();
        }
    }
}
