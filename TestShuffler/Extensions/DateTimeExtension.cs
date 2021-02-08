using System;
using System.Collections.Generic;
using Humanizer;

namespace TestShuffler
{
    public static class DateTimeExtension
    {
        public static IReadOnlyCollection<DateTime> GetElapsedDates(this DateTime sourceDate)
        {
            var result = new List<DateTime>();
            var now = DateTime.UtcNow;

            while (sourceDate.Date <= now.Date)
            {
                result.Add(sourceDate.Date);
                sourceDate += 1.Days();
            }

            return result;
        }
    }
}