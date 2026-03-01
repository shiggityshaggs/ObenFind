using System;
using System.Collections.Generic;
using UnityEngine;

namespace ObenFind
{
    internal static class Extensions
    {
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            var seen = new HashSet<TKey>();
            foreach (var element in source)
                if (seen.Add(keySelector(element)))
                    yield return element;
        }

        public static string GetPath(this Transform transform)
        {
            if (transform == null) return string.Empty;

            string path = transform.name;
            while (transform.parent != null)
            {
                transform = transform.parent;
                path = transform.name + "/" + path;
            }

            return path;
        }
    }
}
