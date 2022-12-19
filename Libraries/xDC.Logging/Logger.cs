using Serilog;
using Serilog.Events;
using System;
using System.Data.SqlClient;
using Serilog.Exceptions;
using xDC.Utils;
using System.Diagnostics;
using Serilog.Formatting.Compact;

namespace xDC.Logging
{
    public static class Logger
    {
        private static readonly ILogger _errorLogger;

        static Logger()
        {
            var logPath = !string.IsNullOrEmpty(Config.LoggerFilePathFormat) ? Config.LoggerFilePathFormat : "~/log-.txt";

            _errorLogger = new LoggerConfiguration()
                .Enrich.WithExceptionDetails()
                .WriteTo.File(new CompactJsonFormatter(), logPath, rollingInterval: RollingInterval.Day)
#if DEBUG
                .WriteTo.Seq("http://localhost:5341")
#endif
                .CreateLogger();
        }

        public static void LogError(Exception error)
        {
            if (error.InnerException != null)
            {
                _errorLogger.Error("{@error}", error.InnerException.Message);
            }
            _errorLogger.Error("{@error}", error.Message);
        }

        public static void LogError(string error)
        {
            _errorLogger.Error("{@error}", error);
        }

        public static void LogInfo(string info)
        {
            _errorLogger.Information("{@info}", info);
        }
    }
}
