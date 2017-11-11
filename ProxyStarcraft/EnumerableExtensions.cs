using System.Collections;

namespace ProxyStarcraft
{
    public static class EnumerableExtensions
    {
        public static bool IsNullOrEmpty(this IList list)
        {
            return list == null || list.Count == 0;
        }

        public static bool NotNullOrEmpty(this IList list)
        {
            return !list.IsNullOrEmpty();
        }
    }
}
