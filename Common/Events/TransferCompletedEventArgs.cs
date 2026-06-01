using Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common.Events
{
    [DataContract]
    public class TransferCompletedEventArgs : EventArgs
    {
        public SessionMeta SessionMeta { get; set; }

        public TransferCompletedEventArgs(SessionMeta sessionMeta)
        {
            SessionMeta = sessionMeta;
        }
    }
}
