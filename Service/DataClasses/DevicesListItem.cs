using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Service.DataClasses
{
    public class DevicesListItem
    {
        public Device Device { get; set; }
        public Position Position { get; set; }
        public Archive Archive { get; set; }
        public Info Info { get; set; }
        public List<Channel> Channels { get; set; }
    }
}
