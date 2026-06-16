using Common.Enums;
using Common.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCFServer.Listeners
{
    public class FileStorageListener
    {
        private readonly string _rootFolder;
        private readonly string _sessionFileName = "session.csv";
        private readonly string _rejectsFileName = "rejects.csv";

        public FileStorageListener(string rootFolder) 
        {
            _rootFolder = rootFolder;
            if(!Directory.Exists(_rootFolder))
            {
                Directory.CreateDirectory(_rootFolder);
            }
        }

        public void HandleTransferStarted(object sender, TransferStartedEventArgs args)
        {
            string folderPath = Path.Combine(_rootFolder, args.SessionMeta.CountryCode, args.SessionMeta.YearMonth);
            if(!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string sessionFilePath = Path.Combine(folderPath, _sessionFileName);
            File.WriteAllText(sessionFilePath, "Date,TotalActualMWh,TotalForecastMWh" + Environment.NewLine);
            
            string rejectsFilePath = Path.Combine(folderPath, _rejectsFileName);
            File.WriteAllText(rejectsFilePath, "Date,TotalActualMWh,TotalForecastMWh,Reason" + Environment.NewLine);

            Console.WriteLine("SERVER: Session started: " + args.SessionMeta.CountryCode + ", " + args.SessionMeta.YearMonth + ", total days: " + args.SessionMeta.TotalDays + ", file: " + args.SessionMeta.SourceFileName);
        }

        public void HandleSampleReceived(object sender, SampleReceivedEventArgs args)
        {
            Console.WriteLine($"Sample received: {args.Sample.CountryCode} {args.Sample.PeakActualMW} in State: {args.SampleState}");

            string folderPath = Path.Combine(_rootFolder, args.SessionMeta.CountryCode, args.SessionMeta.YearMonth);
            if(args.SampleState == ReceivedSampleState.Rejected)
            {
                string rejectsPath = Path.Combine(folderPath, _rejectsFileName);
                if(File.Exists(rejectsPath))
                {
                    File.AppendAllText(rejectsPath, $"{args.Sample.Date},{args.Sample.TotalActualMWh},{args.Sample.TotalForecastMWh},{args.Note}" + Environment.NewLine);
                }
                else
                {
                    Console.WriteLine($"Error in handling received rejected sample: {rejectsPath} does not exists.");
                }
            } 
            else
            {
                string sessionPath = Path.Combine(folderPath, _sessionFileName);
                if(File.Exists(sessionPath))
                {
                    File.AppendAllText(sessionPath, $"{args.Sample.Date},{args.Sample.TotalActualMWh},{args.Sample.TotalForecastMWh}" + Environment.NewLine);
                }
                else
                {
                    Console.WriteLine($"Error in handling received valid sample: {sessionPath} does not exists.");
                }
            }  
        }

        public void HandleTransferCompleted(object sender, TransferCompletedEventArgs args)
        {
            Console.WriteLine("Transfer completed for CountryCode: " + args.SessionMeta.CountryCode);
        }
    }
}
