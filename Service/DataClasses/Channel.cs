using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Service.DataClasses
{
    public class Channel
    {
        public int Code { get; set; }
        public string Name { get; set; }
        public string Symbol { get; set; }
        public string Measure { get; set; }
        public double? Value { get; set; }
        public double? LimitHigh { get; set; }
        public double? LimitLow { get; set; }
        public double? AlarmHigh { get; set; }
        public double? AlarmLow { get; set; }
    }
}
