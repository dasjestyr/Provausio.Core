using System;
using System.Collections.Generic;
using Provausio.Core.Ext;

namespace Provausio.Core
{
    internal static class DataTypeFormatterFactory
    {
        private static readonly IDictionary<Type, Type> Formatters = new Dictionary<Type, Type>
        {
            { typeof(DateTimeOffset), typeof(DateTimeOffsetTimestampFormatter) }
        };

        public static IObjectStringFormatter GetFormatter(object input)
        {
            var t = input.GetType();
            if(!Formatters.ContainsKey(t))
                return new DefaultObjectStringFormatter();

            return (IObjectStringFormatter) Activator.CreateInstance(Formatters[t]);
        }
    }
}