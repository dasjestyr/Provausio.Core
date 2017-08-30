using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Provausio.Core.Ext;

namespace Provausio.Core.Parsing.Csv.Mappers
{
    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
    public class ArrayPropertyMapper<T> : StringArrayMapper<T>
    {
        public override T Map(IReadOnlyList<string> source, T target)
        {
            var properties = GetDecoratedProperties(target);
            if (properties == null)
                return target;

            foreach (var property in properties)
            {
                var attribute = property.GetCustomAttribute<ArrayPropertyAttribute>();
                var index = attribute.Index;

                var value = GetValue(
                    index, 
                    source, 
                    attribute.ValidationPattern, 
                    property.CanBeNull(), 
                    property.PropertyType);

                property.SetValue(target, value, null);
            }

            return target;
        }

        private static List<PropertyInfo> GetDecoratedProperties(T target)
        {
            var properties = target
                .GetType()
                .GetTypeInfo()
                .DeclaredProperties
                .Where(prop => prop.GetCustomAttributes<ArrayPropertyAttribute>().Any())
                .ToList();

            return properties;
        }
    }
}
