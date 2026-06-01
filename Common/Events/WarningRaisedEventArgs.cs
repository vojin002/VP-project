using Common.Enums;
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
    public class WarningRaisedEventArgs : EventArgs
    {
        public WarningType WarningType { get; set; }
        public DailyConsumptionSample Sample { get; set;  }

        public WarningRaisedEventArgs(WarningType warningType, DailyConsumptionSample sample)
        {
            WarningType = warningType;
            Sample = sample;
        }
    }
}
