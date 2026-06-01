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
            // TODO: handle warnings raised
            Console.WriteLine("Warning occured");
        }
    }
}
