using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Formatting.Compact;
using Serilog.Formatting.Json;
using System;
using xDC.Utils;

namespace xDC.Logging
{
    public static class Logger
    {
        private static readonly ILogger _errorLogger;

        static Logger()
        {
            var logPath = !string.IsNullOrEmpty(Config.LoggerFilePathFormat) ? Config.LoggerFilePathFormat + "log-.txt" : "~/log-.txt";
            var errorFilteredLogPath = !string.IsNullOrEmpty(Config.LoggerFilePathFormat) ? Config.LoggerFilePathFormat + "errorlog-.json" : "~/errorlog-.json";

            _errorLogger = new LoggerConfiguration()
                .Enrich.WithExceptionDetails()
                .WriteTo.Console()
                .WriteTo.File(new JsonFormatter(), errorFilteredLogPath, restrictedToMinimumLevel: LogEventLevel.Warning, rollingInterval: RollingInterval.Day)
                .WriteTo.File(logPath, rollingInterval: RollingInterval.Day)
                .MinimumLevel.Debug()
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
