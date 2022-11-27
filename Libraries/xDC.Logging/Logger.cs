using Serilog;
using Serilog.Events;
using System;
using System.Data.SqlClient;
using Serilog.Exceptions;
using xDC.Utils;

namespace xDC.Logging
{
    public class Logger: IXDcLogger
    {
        private readonly ILogger _errorLogger;

        public Logger()
        {
            var logPath = !string.IsNullOrEmpty(Config.LoggerFilePathFormat) ? Config.LoggerFilePathFormat : "~/log-.txt";

            _errorLogger = new LoggerConfiguration()
                .Enrich.WithExceptionDetails()
                .WriteTo.File(logPath, rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }

        public void LogError(Exception error)
        {
            _errorLogger.Error("{@error}", error);
        }

        public void LogError(string error)
        {
            _errorLogger.Error("{@error}", error);
        }

        public void LogInfo(string info)
        {
            _errorLogger.Information("{@info}", info);
        }
    }
}
