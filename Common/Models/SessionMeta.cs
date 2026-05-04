using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    [DataContract]
    public class SessionMeta
    {
        [DataMember]
        public string CountryCode { get; set; }
        [DataMember]
        public string YearMonth { get; set; }
        [DataMember]
        public string SourceFileName { get; set; }
        [DataMember]
        public int TotalDays { get; set; }
    }
}
