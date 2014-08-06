using System;
using JetBrains.Annotations;

namespace SimpleInjector.Advanced.Helpers
{
    /// <summary>
    /// Extensions with some useful string methods.
    /// </summary>
    internal static class StringExtensions
    {
        // STRIPED DOWN copy of my StringExtensions from Masb.SystemExtensions

        [NotNull]
        public static string RemoveEnd([NotNull] this string source, [NotNull] string[] endings, StringComparison comparisonType)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (endings == null)
                throw new ArgumentNullException("endings");

            foreach (var ending in endings)
            {
                if (source.EndsWith(ending, comparisonType))
                    return source.Substring(0, source.Length - ending.Length);
            }

            return source;
        }
    }
}