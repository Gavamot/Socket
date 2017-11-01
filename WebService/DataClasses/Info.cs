using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebService.DataClasses
{
    public class Info
    {
        //[JsonConverter(typeof(AscDateTimeConverter), "dd.MM.yyyyTHH:MM:ss")]
        public string Time { get; set; }
        public bool Video { get; set; }
        public bool Available { get; set; }
        public bool Active { get; set; }
        public Location Location { get; set; } 
    }
}
