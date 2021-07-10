using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShionBot.Utilities
{
    public class TimerUtil
    {
        public static bool CheckValidDaily(DateTime now, DateTime? lastTime)
        {
            if (lastTime == null)
                return true;

            TimeSpan span = now.Subtract(lastTime.Value);
            return span.TotalHours >= 23;
        }

        public static TimeSpan? GetTimeDifference(DateTime now, DateTime? lastTime)
        {
            if (lastTime == null)
                return null;

            TimeSpan span = now.Subtract(lastTime.Value);
            return span;
        }

        public static string FormattedSpan(TimeSpan span)
        {
            return $"{span:hh\\:mm\\:ss}";
        }
    }
}
