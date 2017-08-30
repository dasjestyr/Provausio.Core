using System;

namespace Provausio.Core.Ext
{
    public static class DateTimeEx
    {
        /// <summary>
        /// <para>
        /// Returns a <see cref="DateTimeOffset"/> representation of the target DateTime value. This will automatically assume the source date is in system time.
        /// </para>
        /// </summary>
        /// <para>
        ///     Keep in mind that when you parse an ISO 8601 formatted DateTime string (i.e. DateTime.Parse(isoFormat);), it will automatically
        ///     Adjust the value to system time. For example '1997-07-16T19:20+01:00' will be adjusted to 
        ///     July 16, 1997 at 11:20 AM. If you need to preserve a specific offset, use the overload that takes
        ///     <see cref="TimeSpan"/>
        /// </para>
        /// <param name="dt">The target datetiem that will be converted</param>
        /// <returns></returns>
        public static DateTimeOffset ToDateTimeOffset(this DateTime dt)
        {
            return new DateTimeOffset(dt);
        }

        /// <summary>
        /// Uses the source <see cref="DateTime"/> to build a new <see cref="DateTimeOffset"/>
        /// </summary>
        /// <param name="dt">Source <see cref="DateTime"/></param>
        /// <param name="offset">Specify the offset that will be used to describe the provided <see cref="DateTime"/></param>
        /// <returns></returns>
        public static DateTimeOffset ToDateTimeOffset(this DateTime dt, TimeSpan offset)
        {
            return new DateTimeOffset(
                dt.Year,
                dt.Month,
                dt.Day,
                dt.Hour,
                dt.Minute,
                dt.Second,
                dt.Millisecond,
                offset);
        }

        public static DateTime StartOf(this DateTime dt, DateInterval interval, DayOfWeek firstDayOfWeek = DayOfWeek.Sunday)
        {
            DateTime result;
            switch (interval)
            {
                case DateInterval.Second:
                    result = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, 0, dt.Kind);
                    break;
                case DateInterval.Minute:
                    result = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 0, 0, dt.Kind);
                    break;
                case DateInterval.Hour:
                    result = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 0, 0, 0, dt.Kind);
                    break;
                case DateInterval.Day:
                    result = new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0, 0, dt.Kind);
                    break;
                case DateInterval.Week:
                    var thisWeeksDay = new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0, 0, dt.Kind);
                    var diff = dt.DayOfWeek - firstDayOfWeek;
                    if (diff < 0) diff += 7;
                    result = thisWeeksDay.AddDays(-1 * diff);
                    break;
                case DateInterval.Month:
                    result = new DateTime(dt.Year, dt.Month, 1, 0, 0, 0, 0, dt.Kind);
                    break;
                case DateInterval.Year:
                    result = new DateTime(dt.Year, 1, 1, 0, 0, 0, 0, dt.Kind);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(interval), interval, null);
            }

            return result;
        }
    }
}
