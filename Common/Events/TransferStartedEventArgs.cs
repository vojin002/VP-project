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
    public class TransferStartedEventArgs : EventArgs
    {
        public SessionMeta SessionMeta { get; set; }

        public TransferStartedEventArgs(SessionMeta sessionMeta)
        {
            SessionMeta = sessionMeta;
        }
    }
}
