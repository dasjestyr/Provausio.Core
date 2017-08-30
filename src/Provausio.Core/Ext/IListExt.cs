using System;
using System.Collections.Generic;
using System.Linq;

namespace Provausio.Core.Ext
{
    public static class ListExt
    {
        /// <summary>
        /// Replaces the specified old value and returns the index at which it was found. -1 if nothing was replaced.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">source</exception>
        public static int Replace<T>(this IList<T> source, T oldValue, T newValue)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var index = source.IndexOf(oldValue);
            if (index != -1)
                source[index] = newValue;
            return index;
        }

        /// <summary>
        /// Replaces the old value if it exists, or else it will add it.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        /// <returns></returns>
        public static int AddOrReplace<T>(this IList<T> source, T oldValue, T newValue)
        {
            var index = source.Replace(oldValue, newValue);
            if(index == -1)
                source.Add(newValue);

            return index;
        }

        /// <summary>
        /// Replaces all instances of the old value with the new value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        /// <exception cref="ArgumentNullException">source</exception>
        public static void ReplaceAll<T>(this IList<T> source, T oldValue, T newValue)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            int index;
            do
            {
                index = source.IndexOf(oldValue);
                if (index != -1)
                    source[index] = newValue;
            } while (index != -1);
        }


        /// <summary>
        /// Replaces the specified old value. Returns a new collection with the correct values.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">source</exception>
        public static IEnumerable<T> Replace<T>(this IEnumerable<T> source, T oldValue, T newValue)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return source.Select(x => EqualityComparer<T>.Default.Equals(x, oldValue) ? newValue : x);
        }
    }
}
