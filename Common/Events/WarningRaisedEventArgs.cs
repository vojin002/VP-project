using Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common.Events
{
    [DataContract]
    public class WarningRaisedEventArgs : EventArgs
    {
        public WarningType WarningType { get; set; }

        public WarningRaisedEventArgs(WarningType warningType)
        {
            WarningType = warningType;
        }
    }
}
