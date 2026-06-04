using Common.Enums;
using Common.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCFServer.Listeners
{
    public class WarningHandlingListener
    {
        public WarningHandlingListener() { }

        public void HandleWarningRaised(object sender, WarningRaisedEventArgs args)
        {
            string msg = "WARNING [" + args.WarningType + "]: " + args.Sample.Date.ToString("yyyy-MM-dd") + " | " + args.Sample.CountryCode;

            if (args.WarningType == WarningType.ForecastDeviationWarning)
                msg += " | Deviation: " + args.DeviationPct.ToString("F2") + "%";

            Console.WriteLine(msg);
        }
    }
}
