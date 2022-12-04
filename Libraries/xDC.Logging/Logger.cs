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
    public class Logger: IXDcLogger
    {
        private readonly ILogger _errorLogger;

        public Logger()
        {
            var logPath = !string.IsNullOrEmpty(Config.LoggerFilePathFormat) ? Config.LoggerFilePathFormat : "~/log-.txt";

            _errorLogger = new LoggerConfiguration()
                .Enrich.WithExceptionDetails()
                .WriteTo.File(new CompactJsonFormatter(), logPath, rollingInterval: RollingInterval.Day)
                .WriteTo.Seq("http://localhost:5341")
                .CreateLogger();
        }

        public void LogError(Exception error)
        {
            if (error.InnerException != null)
            {
                _errorLogger.Error("{@error}", error.InnerException.Message);
            }
            _errorLogger.Error("{@error}", error.Message);
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
