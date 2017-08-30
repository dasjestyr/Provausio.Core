using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Provausio.Core.Ext
{
    public static class ObjectExt
    {
        public static T FindAttribute<T>(this object target)
            where T : Attribute
        {
            var targetType = target.GetType();
            var attribute = targetType.GetTypeInfo().GetCustomAttribute(typeof(T));
            return attribute as T;
        }

        public static IEnumerable<T> FindAttributes<T>(this object target)
        {
            var targetType = target.GetType();
            var attributes = targetType
                .GetTypeInfo()
                .GetCustomAttributes(typeof(T))
                .Cast<T>();

            return attributes;
        }
    }
}
