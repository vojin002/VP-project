using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    [DataContract]
    public class DailyConsumptionSample
    {
        [DataMember]
        public DateTime Date { get; set; }
        [DataMember]
        public double TotalActualMWh { get; set; }
        [DataMember]
        public double TotalForecastMWh { get; set; }
        [DataMember]
        public DateTime PeakTime { get; set; }
        [DataMember]
        public double PeakActualMW { get; set; }
        [DataMember]
        public string CountryCode { get; set; }
        [DataMember]
        public int RowIndex { get; set; }

    }
}
