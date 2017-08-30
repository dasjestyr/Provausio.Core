using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using Provausio.Core.Ext;

namespace Provausio.Core.Parsing.Csv.Mappers
{
    public abstract class StringArrayMapper<T> : IStringArrayMapper<T>
    {
        private readonly TryParseFactory _tryParseFactory = new TryParseFactory();

        public abstract T Map(IReadOnlyList<string> source, T target);

        public ParsedObjectReader<T> AttachedReader { get; set; }

        protected string GetValue(
            int index, 
            IReadOnlyList<string> source, 
            string validationPattern, 
            bool allowEmpty)
        {
            var value = source.Count < index + 1
                ? string.Empty
                : source[index];

            if (value.IsNullOrEmptyOrWhitespace() && !allowEmpty)
                throw new ArgumentException($"Expected non-empty value in source at position {index}.");

            if (!string.IsNullOrEmpty(validationPattern))
                Validate(value, validationPattern);

            return value;
        }

        protected object GetValue(
            int index, 
            IReadOnlyList<string> source, 
            string validationPattern,
            bool allowEmpty, 
            Type destinationType)
        {
            var value = GetValue(index, source, validationPattern, allowEmpty);
            return Convert(value, destinationType);
        }

        private static void Validate(string source, string pattern)
        {
            if (!Regex.IsMatch(source, pattern))
                throw new ArgumentException($"The source data ({source}) does not meet the specified regex pattern ({pattern})");
        }

        protected object Convert(string value, Type destinationType)
        {
            if (destinationType.GetTypeInfo().IsValueType)
            {
                var parsed = _tryParseFactory.TryParse(destinationType, value, out object parsedResult);
                if (!parsed)
                    throw new FormatException($"Could not parse value \"{value}\" as {destinationType}");

                return parsedResult;
            }

            var result = System.Convert.ChangeType(value, destinationType);
            return result;
        }

        public ParsedObjectReader<T> GetReader() => AttachedReader;
    }
}