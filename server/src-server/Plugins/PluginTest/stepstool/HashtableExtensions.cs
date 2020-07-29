
namespace System.Collections
{
    internal static class HashtableExtensions
    {
        public static bool TryGetValue(this Hashtable table, object key, out object obj)
        {
            var hasValue = table.Contains(key);
            obj = hasValue ? table[key] : null;
            return hasValue;
        }

        public static bool TryGetValue<T>(this Hashtable table, object key, out T obj)
        {
            var hasValue = table.Contains(key);
            obj = hasValue && table[key] is T tmp ? tmp : default;
            return hasValue;
        }

        public static T TryGetValue<T>(this Hashtable table, object key)
            where T : class
        {
            var hasValue = table.Contains(key);
            return hasValue && table[key] is T tmp ? tmp : null;
        }
    }

    internal static class HashtableExtensions2
    {
        public static T? TryGetValue<T>(this Hashtable table, object key)
               where T : struct
        {
            var hasValue = table.Contains(key);
            return hasValue && table[key] is T tmp ? tmp : (T?)null;
        }
    }
}
