using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.AlarmService.Responses
{
    public class AlarmResponse
    {
        public string Id { get; set; }
        public string AlarmName { get; set; }
        public string AlarmDescription { get; set; }
        public int Severity { get; set; }
        public DateTime AlarmCreateTime { get; set; }   
        public DateTime FixedDate { get; set; }
        public string DeviceId { get; set; }
        public bool IsAlarmActive { get; set; }
        public bool IsAlarmFixed { get; set; }
        public bool IsMasked { get; set; }
        public string AlarmCondition { get; set; }
        public string AlarmThreshold { get; set; }
        public string DeviceType { get; set; }
    }
}
