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
        }

        public void HandleTransferStarted(object sender, TransferStartedEventArgs args)
        {
            // napravi foldere Data/CountryCode/Year-Month/session.csv ako ne postoje
            // napravi foldere Data/CountryCode/Year-Month/rejects.csv ako ne postoje
            Console.WriteLine("SERVER: Session started: " + args.SessionMeta.CountryCode + ", " + args.SessionMeta.YearMonth + ", total days: " + args.SessionMeta.TotalDays + ", file: " + args.SessionMeta.SourceFileName);
        }

        public void HandleSampleReceived(object sender, SampleReceivedEventArgs args)
        {
            // proveri da li je sample state validan
            // ako je validan upisi u session 
            // ako nije upisi u rejects.csv I NAVEDI RAZLOG
            Console.WriteLine($"Sample received: {args.Sample.CountryCode} {args.Sample.PeakActualMW} in State: {args.SampleState}");
        }

        public void HandleTransferCompleted(object sender, TransferCompletedEventArgs args)
        {
            Console.WriteLine("Transfer completed for CountryCode: " + args.SessionMeta.CountryCode);
        }
    }
}
