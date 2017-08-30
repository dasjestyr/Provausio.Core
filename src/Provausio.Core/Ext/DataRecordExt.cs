using System;
using System.Data;

namespace Provausio.Core.Ext
{
    public static class DataRecordExt
    {
        /// <summary>
        /// Casts the database type to the specified type. NOTE: If the field is nullable in the database, use <see cref="DbCastNullable{T}"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rdr">The RDR.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static T DbCast<T>(this IDataRecord rdr, string name)
        {
            var obj = rdr[name];
            if (obj == null || obj == DBNull.Value) return default(T);
            if (!(obj is T))
            {
                throw new ArgumentException(
                    $"Specified type ({typeof(T)}) is not the same as target object ({obj.GetType()})");
            }

            // ReSharper disable once PossibleInvalidCastException
            return (T)Convert.ChangeType(obj, typeof(T));
        }

        /// <summary>
        /// Casts nullable database fields to the specified nullable value type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rdr">The RDR.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static T? DbCastNullable<T>(this IDataRecord rdr, string name)
            where T : struct
        {
            var obj = rdr[name];
            if (obj != null && !(obj is T) && !Convert.IsDBNull(obj))
            {
                throw new ArgumentException(
                    $"Specified type ({typeof(T)}) is not the same as target object ({obj.GetType().ToString()})");
            }

            // ReSharper disable once PossibleInvalidCastException
            return Convert.IsDBNull(obj) || obj == null
                ? null
                : new T?((T)obj);
        }
    }
}
