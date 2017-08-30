using Provausio.Core.Ext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Provausio.Core
{
    public class PropertyTransform
    {
        public string PropertyName { get; }

        public Func<string, string> Transform { get; }

        public PropertyTransform(string propertyName, Func<string, string> transform)
        {
            PropertyName = propertyName;
            Transform = transform;
        }
    }

    public class ObjectPropertyCollection
    {
        private readonly Dictionary<string, Func<string, string>> _transforms = new Dictionary<string, Func<string, string>>();
        private readonly Dictionary<string, string> _properties = new Dictionary<string, string>();
        private readonly object _sourceObject;

        public int Count => _properties.Count;

        public IDictionary<string, string> Properties => _properties;

        public string this[string propertyName] => !_properties.ContainsKey(propertyName) ? null : _properties[propertyName];

        private ObjectPropertyCollection() { }

        public ObjectPropertyCollection(object sourceObject, params PropertyTransform[] transforms)
        {
            _sourceObject = sourceObject;

            foreach (var transform in transforms)
            {
                _transforms.Add(transform.PropertyName, transform.Transform);
            }

            SetProperties();
        }

        public static ObjectPropertyCollection FromKvpString(string input)
        {
            var coll = new ObjectPropertyCollection();
            var kvps = input.Split('&');
            foreach (var kvp in kvps)
            {
                var x = GetKvp(kvp);
                coll._properties.Add(x.Key, x.Value);
            }

            return coll;
        }

        private void SetProperties()
        {
            var properties = GetProperties();
            foreach (var property in properties)
            {
                var name = property.Name;

                var value = property.GetValue(_sourceObject);
                var valueString = value?.ToString(DataTypeFormatterFactory.GetFormatter(value));

                if (!string.IsNullOrEmpty(valueString) && _transforms.ContainsKey(name))
                    valueString = _transforms[name](valueString);

                _properties.Add(name, valueString);
            }
        }

        private static KeyValuePair<string, string> GetKvp(string kvpString)
        {
            string[] parts;
            if (!IsValidKvpFormat(kvpString, out parts))
                throw new FormatException($"Unexpected kvp string format ({kvpString}).");

            return new KeyValuePair<string, string>(parts[0], parts[1]);
        }

        private static bool IsValidKvpFormat(string input, out string[] parts)
        {
            parts = input.Split('=');
            var hasTooFew = input.IndexOf('=') == -1;
            var hasTooMany = parts.Length - 1 > 1;

            return !hasTooFew && !hasTooMany;
        }

        private IEnumerable<PropertyInfo> GetProperties()
        {
            var x = _sourceObject
                .GetType()
                .GetTypeInfo()
                .GetProperties().Where(p => p.CanRead);

            return x;
        }

        public override string ToString()
        {
            var kvpStrings = _properties
                .ToList() // list of kvp
                .Select(kvp => $"{kvp.Key}={kvp.Value}") // string kvps
                .ToList();

            return string.Join("&", kvpStrings);
        }
    }
}
