using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Provausio.Core.Ext;

namespace Provausio.Core.Parsing.Csv.Mappers
{
    public class CustomMapper<T> : StringArrayMapper<T>
    {
        private readonly Dictionary<int, Expression<Func<T, object>>> _propertyDefinitions;
        private readonly Dictionary<int, Func<string, object>> _valueCallbacks;
        private readonly Dictionary<int, string> _validationPatterns;

        public int PropertyCount => _propertyDefinitions.Count;

        public CustomMapper()
        {
            _propertyDefinitions = new Dictionary<int, Expression<Func<T, object>>>();
            _valueCallbacks = new Dictionary<int, Func<string, object>>();
            _validationPatterns = new Dictionary<int, string>();
        }

        public override T Map(IReadOnlyList<string> source, T target)
        {
            var maxIndex = _propertyDefinitions.Max(m => m.Key);
            for (var index = 0; index < source.Count && index <= maxIndex; index++)
            {
                if (!_propertyDefinitions.ContainsKey(index))
                    continue;

                var propertyMapper = _propertyDefinitions[index];

                // account for value types (unary)
                var unary = propertyMapper.Body as UnaryExpression;
                var memberSelector = unary == null
                    ? propertyMapper.Body as MemberExpression
                    : unary.Operand as MemberExpression;

                var propertyInfo = memberSelector?.Member as PropertyInfo;
                if (propertyInfo == null)
                    continue;

                var pattern = _validationPatterns.ContainsKey(index) ? _validationPatterns[index] : null;
                var value = GetValue(index, source, pattern, propertyInfo.CanBeNull());

                if (_valueCallbacks.ContainsKey(index))
                {
                    // the client provided a callback so don't mess with the type
                    var callback = _valueCallbacks[index];
                    propertyInfo.SetValue(target, callback(value), null);
                }
                else
                {
                    var convertedValue = Convert(value, propertyInfo.PropertyType);
                    propertyInfo.SetValue(target, convertedValue, null);
                }
            }

            return target;
        }

        /// <summary>
        /// Defines a mapper for the value located at the specified index.
        /// </summary>
        /// <param name="index">The index of the value that will be mapped.</param>
        /// <param name="mapping">Expression defining the property on the target object to which the value will be mapped.</param>
        /// <param name="validationPattern">A regex pattern that will be used to validate the source data, if provided.</param>
        /// <exception cref="ArgumentNullException">mapping</exception>
        public CustomMapper<T> Define(int index, Expression<Func<T, object>> mapping, string validationPattern = null)
        {
            if (mapping == null)
                throw new ArgumentNullException(nameof(mapping));

            _propertyDefinitions.Add(index, mapping);

            if (!string.IsNullOrEmpty(validationPattern))
                _validationPatterns.Add(index, validationPattern);

            return this;
        }

        /// <summary>
        /// Defines a mapper for the value located at the specified index.
        /// </summary>
        /// <param name="index">The index of the value that will be mapped.</param>
        /// <param name="mapping">Expression defining the property on the target object to which the value will be mapped.</param>
        /// <param name="valueCallback">A callback that will be run against the value before assigning it to the target object. Use this for transforming the result value fetched from the source file.</param>
        /// <param name="validationPattern">A regex pattern that will be used to validate the source data, if provided.</param>
        /// <exception cref="ArgumentNullException">mapping</exception>
        /// <exception cref="ArgumentNullException">valueCallback</exception>
        public CustomMapper<T> Define(int index, Expression<Func<T, object>> mapping, Func<string, object> valueCallback, string validationPattern = null)
        {
            if (mapping == null)
                throw new ArgumentNullException(nameof(mapping));

            if (valueCallback == null)
                throw new ArgumentNullException(nameof(valueCallback));
            
            _propertyDefinitions.Add(index, mapping);
            _valueCallbacks.Add(index, valueCallback);

            if (!string.IsNullOrEmpty(validationPattern))
                _validationPatterns.Add(index, validationPattern);

            return this;
        }
    }
}
