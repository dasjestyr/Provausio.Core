using System;
using System.Reflection;

namespace Provausio.Core.Ext
{
    public static class PropertyInfoExt
    {
        public static bool CanBeNull(this PropertyInfo target)
        {
            var propertyType = target.PropertyType;
            return !propertyType.GetTypeInfo().IsValueType || (Nullable.GetUnderlyingType(propertyType) != null);
        }
    }
}