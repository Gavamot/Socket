using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebService.DataClasses
{
    public class Device
    {
        public ushort Model { get; set; }
        public ushort Number  { get; set; }
        public ushort Release { get; set; }
        public ushort Serial { get; set; }
        public string Version { get; set; }
        public string Type { get; set; }
    }
}
