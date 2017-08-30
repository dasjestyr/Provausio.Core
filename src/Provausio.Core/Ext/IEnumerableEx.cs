using System.Collections;

namespace Provausio.Core.Ext
{
    public static class EnumerableEx
    {
        /// <summary>
        ///   Checks whether or not collection is null or empty. Assumes colleciton can be safely enumerated multiple times.
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this IEnumerable @this)
        {
            if (@this != null)
                return !@this.GetEnumerator().MoveNext();
            return true;
        }
    }
}
