using System;
using Provausio.Core.Ext;

namespace Provausio.Core
{
    public struct DateRange
    {
        /// <summary>
        /// Gets the start date.
        /// </summary>
        /// <value>
        /// The start date.
        /// </value>
        public DateTimeOffset StartDate { get; }
        
        /// <summary>
        /// Gets the end date.
        /// </summary>
        /// <value>
        /// The end date.
        /// </value>
        public DateTimeOffset EndDate { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="DateRange"/> is inclusive. If inclusive, the date range will cover from the first tick of the start date to the last tick of the end date.
        /// </summary>
        /// <value>
        ///   <c>true</c> if inclusive; otherwise, <c>false</c>.
        /// </value>
        public bool Inclusive { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DateRange"/> struct.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <param name="inclusive">if set to <c>true</c> [full range].</param>
        public DateRange(DateTimeOffset start, DateTimeOffset end, bool inclusive)
        {
            Inclusive = inclusive;
            StartDate = inclusive ? start.GetFirstMillisecond() : start;
            EndDate = inclusive ? end.GetLastMillisecond() : end;

            // validate afterwards so that 'inclusive' can be accounted for

            if (start > end)
                throw new ArgumentException("Start date must be less than end date");

            if (end < start)
                throw new ArgumentException("End date must be greater than start date");
        }

        /// <summary>
        /// Creates a range using interval values starting at the provided date
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="interval">The interval.</param>
        /// <param name="count">How many of the specified interval should be applied</param>
        /// <param name="inclusive">If true, range will include the first tick of the start date to the last tick of the end date.</param>
        /// <returns></returns>
        public static DateRange FromStart(DateTimeOffset start, DateInterval interval, int count, bool inclusive)
        {
            var intervalValue = new DateIntervalValue(interval, count);
            return FromStart(start, intervalValue, inclusive);
        }

        /// <summary>
        /// Creates a range using interval values starting at the provided date
        /// </summary>
        /// <param name="start"></param>
        /// <param name="interval"></param>
        /// <param name="inclusive"></param>
        /// <returns></returns>
        public static DateRange FromStart(DateTimeOffset start, DateIntervalValue interval, bool inclusive)
        {
            var startDate = inclusive ? start.GetFirstMillisecond() : start;
            var endDate = ApplyInterval(startDate, interval, inclusive);

            return new DateRange(startDate, endDate, inclusive);
        }

        /// <summary>
        /// Creates a range using interval values ending at the provided date
        /// </summary>
        /// <param name="endDate"></param>
        /// <param name="interval"></param>
        /// <param name="inclusive"></param>
        /// <returns></returns>
        public static DateRange FromEnd(DateTimeOffset endDate, DateIntervalValue interval, bool inclusive)
        {
            if(interval.Value > 0)
                interval = new DateIntervalValue(interval.Interval, -interval.Value);

            var startDate = ApplyInterval(endDate, interval, inclusive);
            return new DateRange(startDate, endDate, inclusive);
        }

        /// <summary>
        /// Creates a range using interval values ending at the provided date
        /// </summary>
        /// <param name="endDate"></param>
        /// <param name="interval"></param>
        /// <param name="count">The number o</param>
        /// <param name="inclusive"></param>
        /// <returns></returns>
        public static DateRange FromEnd(DateTimeOffset endDate, DateInterval interval, int count, bool inclusive)
        {
            var intervalValue = new DateIntervalValue(interval, count);
            return FromEnd(endDate, intervalValue, inclusive);
        }

        /// <summary>
        /// Returns a range relative to "This x" where x is hour, week, month, etc.
        /// </summary>
        /// <param name="interval"></param>
        /// <param name="firstDayOfWeek"></param>
        /// <returns></returns>
        public static DateRange FromThis(DateInterval interval, DayOfWeek firstDayOfWeek = DayOfWeek.Sunday)
        {
            return FromRelative(interval, DateTime.Today, firstDayOfWeek);
        }

        /// <summary>
        /// Returns a range relative to "This x" where x is hour, week, month, etc.
        /// </summary>
        /// <param name="interval"></param>
        /// <param name="relativeToDate"></param>
        /// <param name="firstDayOfWeek"></param>
        /// <returns></returns>
        public static DateRange FromRelative(
            DateInterval interval, 
            DateTimeOffset relativeToDate,
            DayOfWeek firstDayOfWeek = DayOfWeek.Sunday)
        {
            DateRange range;

            // inclusive is always false because we're handling the ranges manually
            switch (interval)
            {
                case DateInterval.Second:
                    var thisSecond = new DateTimeOffset(relativeToDate.Year, relativeToDate.Month, relativeToDate.Day, relativeToDate.Hour, relativeToDate.Minute, relativeToDate.Second, 0, relativeToDate.Offset);
                    range = new DateRange(thisSecond, thisSecond.AddSeconds(1).AddMilliseconds(-1), false);
                    break;
                case DateInterval.Minute:
                    var thisMinute = new DateTimeOffset(relativeToDate.Year, relativeToDate.Month, relativeToDate.Day, relativeToDate.Hour, relativeToDate.Minute, 0, 0, relativeToDate.Offset);
                    range = new DateRange(thisMinute, thisMinute.AddMinutes(1).AddMilliseconds(-1), false);
                    break;
                case DateInterval.Hour:
                    var thisHour = new DateTimeOffset(relativeToDate.Year, relativeToDate.Month, relativeToDate.Day, relativeToDate.Hour, 0, 0, relativeToDate.Offset);
                    range = new DateRange(thisHour, thisHour.AddHours(1).AddMilliseconds(-1), false);
                    break;
                case DateInterval.Day:
                    var thisDay = new DateTimeOffset(relativeToDate.Year, relativeToDate.Month, relativeToDate.Day, 0, 0, 0, relativeToDate.Offset);
                    range = new DateRange(thisDay, thisDay.AddDays(1).AddMilliseconds(-1), false);
                    break;
                case DateInterval.Week:
                    var thisWeeksDay = new DateTimeOffset(relativeToDate.Year, relativeToDate.Month, relativeToDate.Day, 0, 0, 0, relativeToDate.Offset);
                    var diff = relativeToDate.DayOfWeek - firstDayOfWeek;
                    if (diff < 0) diff += 7;
                    var startDate = thisWeeksDay.AddDays(-1 * diff);
                    range = new DateRange(startDate, startDate.AddDays(7).AddMilliseconds(-1), false);
                    break;
                case DateInterval.Month:
                    var thisMonth = new DateTimeOffset(relativeToDate.Year, relativeToDate.Month, 1, 0, 0, 0, relativeToDate.Offset);
                    range = new DateRange(thisMonth, thisMonth.AddMonths(1).AddMilliseconds(-1), false);
                    break;
                case DateInterval.Year:
                    var thisYear = new DateTimeOffset(relativeToDate.Year, 1, 1, 0, 0, 0, relativeToDate.Offset);
                    range = new DateRange(thisYear, thisYear.AddYears(1).AddMilliseconds(-1), false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(interval), interval, null);
            }

            return range;
        }

        #region -- Equality --

        public static bool operator ==(DateRange left, DateRange right)
        {
            return left.StartDate == right.StartDate
                && left.EndDate == right.EndDate;
        }

        public static bool operator !=(DateRange left, DateRange right)
        {
            return !(left == right);
        }

        public bool Equals(DateRange other)
        {
            return StartDate.Equals(other.StartDate) && EndDate.Equals(other.EndDate);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is DateRange && Equals((DateRange)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (StartDate.GetHashCode() * 397) ^ EndDate.GetHashCode();
            }
        }

        #endregion

        #region -- Math Overloads --

        public static DateRange operator +(DateRange range, DateIntervalValue interval)
        {
            var newEndDate = ApplyInterval(range.EndDate, interval, range.Inclusive);

            if (newEndDate < range.StartDate)
                newEndDate = range.StartDate.GetLastMillisecond();

            return new DateRange(range.StartDate, newEndDate, range.Inclusive);
        }

        public static DateRange operator -(DateRange range, DateIntervalValue interval)
        {
            var negativeInterval = new DateIntervalValue(interval.Interval, -interval.Value);
            var newEndDate = ApplyInterval(range.EndDate, negativeInterval, range.Inclusive);

            if (newEndDate < range.StartDate)
                newEndDate = range.StartDate.GetLastMillisecond();

            return new DateRange(range.StartDate, newEndDate, range.Inclusive);
        }

        #endregion

        private static DateTimeOffset ApplyInterval(DateTimeOffset start, DateIntervalValue interval, bool inclusive)
        {
            var startDate = inclusive ? start.Date : start;
            DateTimeOffset endDate;
            var value = interval.Value;
            switch (interval.Interval)
            {
                case DateInterval.Second:
                    endDate = startDate.AddSeconds(value);
                    break;
                case DateInterval.Minute:
                    endDate = startDate.AddMinutes(value);
                    break;
                case DateInterval.Day:
                    endDate = startDate.AddDays(value);
                    break;
                case DateInterval.Hour:
                    endDate = startDate.AddHours(value);
                    break;
                case DateInterval.Week:
                    endDate = startDate.AddDays(value * 7);
                    break;
                case DateInterval.Month:
                    endDate = startDate.AddMonths(value);
                    break;
                case DateInterval.Year:
                    endDate = startDate.AddYears(value);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (inclusive)
                endDate = endDate.GetLastMillisecond();

            return endDate;
        }
        
        public override string ToString()
        {
            return $"{StartDate:MM/dd/yyyy HH:mm:ss.fff zzz} - {EndDate:MM/dd/yyyy HH:mm:ss.fff zzz}";
        }
    }

    public enum DateInterval
    {
        Second,
        Minute,
        Hour,
        Day,
        Week,
        Month,
        Year
    }

    public struct DateIntervalValue
    {
        /// <summary>
        /// Gets the interval.
        /// </summary>
        /// <value>
        /// The interval.
        /// </value>
        public DateInterval Interval { get; }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public int Value { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DateIntervalValue"/> class.
        /// </summary>
        /// <param name="interval">The interval. Supports the following: second, minute, hour, day, week, month, year</param>
        /// <param name="value">The value.</param>
        public DateIntervalValue(DateInterval interval, int value)
        {
            Interval = interval;
            Value = value;
        }

        public static DateIntervalValue FromString(string interval, int value)
        {
            var i = interval.FindEnum<DateInterval>();
            return new DateIntervalValue(i, value);
        }

        #region

        public static bool operator ==(DateIntervalValue left, DateIntervalValue right)
        {
            return left.Interval == right.Interval
                   && left.Value == right.Value;
        }

        public static bool operator !=(DateIntervalValue left, DateIntervalValue right)
        {
            return !(left == right);
        }

        public bool Equals(DateIntervalValue other)
        {
            return Interval == other.Interval && Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is DateIntervalValue && Equals((DateIntervalValue)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)Interval * 397) ^ Value;
            }
        }

        #endregion
    }
}
