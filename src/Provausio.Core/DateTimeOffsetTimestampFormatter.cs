using System;
using Provausio.Core.Ext;

namespace Provausio.Core
{
    internal class DateTimeOffsetTimestampFormatter : IObjectStringFormatter
    {
        public string ToString(object input)
        {
            var dt = (DateTimeOffset) input;
            return dt.ToUnixTimeMilliseconds().ToString();
        }
    }
}