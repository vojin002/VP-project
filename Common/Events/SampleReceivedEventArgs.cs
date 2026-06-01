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
    public class SampleReceivedEventArgs : EventArgs
    {
        public DailyConsumptionSample Sample { get; set; }
        public ReceivedSampleState SampleState { get; set; }
        public string Note { get; set; } = string.Empty;

        public SampleReceivedEventArgs(DailyConsumptionSample sample, ReceivedSampleState sampleState, string note)
        {
            Sample = sample;
            SampleState = sampleState;
            Note = note;
        }
    }
}
