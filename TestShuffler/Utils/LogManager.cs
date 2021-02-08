using System;
using Microsoft.Extensions.Logging;
using TestShuffler;

namespace TestShuffler
{
    public static class LogManager
    {
        private static LoggerFactory _loggerFactory;

        public static void Initialize(LoggerFactory loggerFactory)
        {
            _loggerFactory = Ensure.NotNull(nameof(loggerFactory), loggerFactory);
        }

        public static ILogger<T> GetLogger<T>()
        {
            if (_loggerFactory == null)
            {
                throw new Exception($"{nameof(LogManager)} is not yet initialized");
            }

            return _loggerFactory.CreateLogger<T>();
        }
    }
}