using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebService.DataClasses
{
    public class AscDateTimeConverter : IsoDateTimeConverter
    {
        public AscDateTimeConverter()
        {
            base.DateTimeFormat = "dd.MM.yyyyTHH:MM:ss";
        }

    }
}
