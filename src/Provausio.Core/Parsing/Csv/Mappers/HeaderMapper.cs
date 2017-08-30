using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Provausio.Core.Ext;

namespace Provausio.Core.Parsing.Csv.Mappers
{
    public interface IHeaderMapper
    {
        /// <summary>
        /// When implemented, uses the list of headers to define the position of each header. Assumes this list is ordered.
        /// </summary>
        /// <param name="headers"></param>
        void SetHeaderPositions(string[] headers);
    }

    public class HeaderMapper<T> : StringArrayMapper<T>, IHeaderMapper
    {
        private readonly Dictionary<string, string> _validationPatterns = new Dictionary<string, string>();
        private readonly Dictionary<string, Expression<Func<T, object>>> _maps = new Dictionary<string, Expression<Func<T, object>>>();
        private readonly Dictionary<int, string> _headersByPosition = new Dictionary<int, string>();
        
        /// <summary>
        /// Defines a header to property mapping.
        /// </summary>
        /// <param name="sourceHeader">The name of the header in the source file</param>
        /// <param name="destination">The proeprty to which values in the header column will be mapped</param>
        /// <param name="validationPattern">Optional. A regex pattern that will be used to validate the source field.</param>
        /// <returns></returns>
        public HeaderMapper<T> Define(
            string sourceHeader, 
            Expression<Func<T, object>> destination,
            string validationPattern = null)
        {
            if(string.IsNullOrEmpty(sourceHeader))
                throw new ArgumentNullException(nameof(sourceHeader));

            if(destination == null)
                throw new ArgumentNullException(nameof(destination));

            _maps.Add(sourceHeader, destination);

            if(!validationPattern.IsNullOrEmptyOrWhitespace())
                _validationPatterns.Add(sourceHeader, validationPattern);

            return this;
        }

        /// <summary>
        /// Sets the position of defined headers. All property definitions must be set prior to running this method.
        /// </summary>
        /// <param name="headers"></param>
        public void SetHeaderPositions(string[] headers)
        {
            for (var i = 0; i < headers.Length; i++)
            {
                var isMapped = _maps.Keys.Any(key => key.Equals(headers[i], StringComparison.OrdinalIgnoreCase));
                if(isMapped)
                    _headersByPosition.Add(i, headers[i]);
            }
        }

        public override T Map(IReadOnlyList<string> source, T target)
        {
            for (var index = 0; index < source.Count; index++)
            {
                // accounts for for a deficient header collection
                // basically, this stops mapping once we run out of headers
                if (index > _headersByPosition.Count - 1)
                    continue;

                var headerName = _headersByPosition.ContainsKey(index)
                    ? _headersByPosition[index]
                    : null;

                // try to find a map, else skip to the next value
                var targetProperty = _maps.ContainsKey(headerName) 
                    ? _maps[headerName] 
                    : null;

                if(targetProperty == null)
                    continue;
                
                var property = GetPropertyInfo(targetProperty);
                if(property == null)
                    throw new InvalidOperationException($"Could not get property info from mapping ({headerName})");

                var validationPattern = _validationPatterns.ContainsKey(headerName)
                    ? _validationPatterns[headerName]
                    : null;

                var value = GetValue(
                    index, 
                    source, 
                    validationPattern, 
                    property.CanBeNull(), 
                    property.PropertyType);

                property.SetValue(target, value, null);
            }

            return target;
        }

        private static PropertyInfo GetPropertyInfo(Expression<Func<T, object>> map)
        {
            var unary = map.Body as UnaryExpression;
            var selector = unary == null
                ? map.Body as MemberExpression
                : unary.Operand as MemberExpression;

            var info = selector?.Member as PropertyInfo;
            return info;
        }
    }
}
