using System;
using System.Collections.Generic;
using System.Linq;

namespace TestShuffler
{
    public static class Ensure
    {
        public static TValue NotNull<TValue>(string propertyName, TValue value)
            where TValue : class =>
                value ?? throw new ArgumentNullException(propertyName);

        public static string NotNullOrWhitespace(string propertyName, string value) =>
            string.IsNullOrWhiteSpace(value)
                ? throw new ArgumentException("string is null or whitespace", propertyName)
                : value;

        public static TEnumerable NotNullOrEmpty<TEnumerable, TItem>(string propertyName, TEnumerable enumerable)
            where TEnumerable : IEnumerable<TItem> =>
                enumerable == null || !enumerable.Any()
                    ? throw new ArgumentException("value is null or empty", propertyName)
                    : enumerable;
    }
}