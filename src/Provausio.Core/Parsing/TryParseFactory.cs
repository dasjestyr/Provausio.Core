using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Provausio.Core.Parsing
{
    public class TryParseFactory
    {
        public delegate bool TryParseDelegate<T>(string input, out T result);

        private readonly Dictionary<Type, Delegate> _tryParsers = new Dictionary<Type, Delegate>();

        public TryParseFactory()
        {
            Register<Guid>(Guid.TryParse);
            Register<int>(int.TryParse);
            Register<uint>(uint.TryParse);
            Register<long>(long.TryParse);
            Register<ulong>(ulong.TryParse);
            Register<decimal>(decimal.TryParse);
            Register<double>(double.TryParse);
            Register<float>(float.TryParse);
        }

        /// <summary>
        /// Adds a try parse delegate for the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tryParseDelegate"></param>
        public void Register<T>(TryParseDelegate<T> tryParseDelegate)
        {
            _tryParsers[typeof(T)] = tryParseDelegate;
        }

        /// <summary>
        /// Removes a try parse delegate for the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool Deregister<T>()
        {
            return _tryParsers.Remove(typeof(T));
        }

        /// <summary>
        /// Attempts to parse the specified value type from the provided string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public bool TryParse<T>(string input, out T result)
        {
            if (!_tryParsers.ContainsKey(typeof(T)))
            {
                throw new ArgumentException($"Does not contain parser for {typeof(T).FullName}.");
            }
            var tryParseDelegate = (TryParseDelegate<T>)_tryParsers[typeof(T)];
            return tryParseDelegate(input, out result);
        }

        /// <summary>
        /// Attempts to parse the specified value type from the provided string.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="input"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public bool TryParse(Type type, string input, out object result)
        {
            var method = GetType().GetTypeInfo()
                .GetDeclaredMethods("TryParse")
                .First(m => m.IsGenericMethod)
                .MakeGenericMethod(type);

            object outParameter = null;
            var arguments = new[] { input, outParameter };
            var parseSucceeded = (bool) method.Invoke(this, arguments);

            result = arguments[1];
            return parseSucceeded;
        }
    }
}
