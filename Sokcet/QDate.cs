using System;
using System.Collections.Generic;
using System.Text;

namespace Test
{
    class QDate
    {
        private static string GetString<T>(T v)
        {
            return $"{{ julian day = {v.ToString()} }}";
        }

        public static string AllTime => GetString(-9223372036854775808);

        public static string GetString(DateTime datetime)
        {
            DateTime start = new DateTime(1, 1, 1);
            TimeSpan interval = datetime - start;
            long v = (long)interval.TotalDays + 1721427;
            return GetString(v);
        }
    }
}
