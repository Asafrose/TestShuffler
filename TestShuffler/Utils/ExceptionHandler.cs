using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace TestShuffler
{
    public sealed class ExceptionHandler
    {
        private static ILogger _logger;

        public static void Initialize()
        {
            _logger = LogManager.GetLogger<ExceptionHandler>();

            AppDomain.CurrentDomain.UnhandledException +=
                (_, args) =>
                {
                    _logger.LogInformation("caught unhandled exception");
                    Handle((Exception)args.ExceptionObject);
                };
        }

        public static void Handle(Exception exception)
        {
            if (exception == null)
            {
                return;
            }

            exception.Demystify();

            _logger.LogError(exception, "caught exception");
        }
    }
}