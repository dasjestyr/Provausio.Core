using System;

namespace Provausio.Core
{
    /// <summary>
    /// Unified timestamp format works in Unix time (truncates ticks).
    /// </summary>
    public class Timestamp
    {
        public static DateTimeOffset LocalNow()
        {
            return TruncateTicks(DateTimeOffset.Now);
        }

        public static DateTimeOffset UtcNow()
        {
            return TruncateTicks(DateTimeOffset.UtcNow);
        }

        public static long LocalNowMilliseconds()
        {
            return LocalNow().ToUnixTimeMilliseconds();
        }

        public static long UtcNowMilliseconds()
        {
            return UtcNow().ToUnixTimeMilliseconds();
        }

        public static DateTimeOffset FromMilliseconds(long milliseconds)
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);
        }

        public static DateTimeOffset FromMilliseconds(string milliseconds)
        {
            var asLong = long.Parse(milliseconds);
            return FromMilliseconds(asLong);
        }

        public static bool AreEqual(long left, long right)
        {
            var d1 = FromMilliseconds(left);
            var d2 = FromMilliseconds(right);
            return d1.Equals(d2);
        }

        public static bool AreEqual(string left, string right)
        {
            var d1 = FromMilliseconds(left);
            var d2 = FromMilliseconds(right);
            return d1.Equals(d2);
        }

        public static bool AreEqual(DateTimeOffset left, long right)
        {
            var d2 = FromMilliseconds(right);
            left = TruncateTicks(left);
            return left.Equals(d2);
        }

        public static bool AreEqual(DateTimeOffset left, string right)
        {
            var d2 = FromMilliseconds(right);
            return left.Equals(d2);
        }

        private static DateTimeOffset TruncateTicks(DateTimeOffset input)
        {
            return input.AddTicks(-input.Ticks % TimeSpan.TicksPerSecond);
        }
    }
}
