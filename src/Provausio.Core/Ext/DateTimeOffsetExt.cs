using System;

namespace Provausio.Core.Ext
{
    public static class DateTimeOffsetExt
    {
        /// <summary>
        /// Returns a DateTimeOffset equal to the first millisecond of the date.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static DateTimeOffset GetFirstMillisecond(this DateTimeOffset date)
        {
            var newDate = new DateTimeOffset(date.Year, date.Month, date.Day, 0, 0, 0, date.Offset);
            return newDate;
        }

        /// <summary>
        /// Returns a DateTimeOffset equal to the last millisecond of the date.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static DateTimeOffset GetLastMillisecond(this DateTimeOffset date)
        {
            return date.Date
                .AddDays(1)
                .AddMilliseconds(-1)
                .ToDateTimeOffset(date.Offset);
        }
    }
}
