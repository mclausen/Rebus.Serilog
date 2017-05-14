using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rebus.Config;
using Rebus.Extensions;
using Rebus.Logging;
using Serilog;

namespace Rebus.Serilog
{
    class SerilogLoggerFactory : AbstractRebusLoggerFactory
    {
        readonly ILogger _baseLogger;

        public SerilogLoggerFactory(LoggerConfiguration loggerConfiguration)
            : this(loggerConfiguration.CreateLogger())
        {
        }

        public SerilogLoggerFactory(ILogger baseLogger)
        {
            _baseLogger = baseLogger;
        }

        protected override ILog GetLogger(Type type)
        {
            return new SerilogLogger(_baseLogger.ForContext(type));
        }

        class SerilogLogger : ILog
        {
            readonly ILogger _logger;

            public SerilogLogger(ILogger logger)
            {
                _logger = logger;
            }

            public void Debug(string message, params object[] objs)
            {
                var exception = ExtractException(objs);
                if (exception == null)
                {
                    _logger.Debug(message, objs);
                }
                else
                {
                    _logger.Debug(exception, message, objs);
                }
            }

            public void Info(string message, params object[] objs)
            {
                var exception = ExtractException(objs);
                if (exception == null)
                {
                    _logger.Information(message, objs);
                }
                else
                {
                    _logger.Information(exception, message, objs);
                }
            }

            public void Warn(string message, params object[] objs)
            {
                var exception = ExtractException(objs);
                if (exception == null)
                {
                    _logger.Warning(message, objs);
                }
                else
                {
                    _logger.Warning(exception, message, objs);
                }
            }

            public void Error(Exception exception, string message, params object[] objs)
            {
                _logger.Error(exception, message, objs);
            }

            public void Error(string message, params object[] objs)
            {
                var exception = ExtractException(objs);
                if (exception == null)
                {
                    _logger.Error(message, objs);
                }
                else
                {
                    _logger.Error(exception, message, objs);
                }
            }

            private static Exception ExtractException(params object[] objs)
            {
                if (objs == null || objs.Length <= 0) return null;

                var exceptions = objs
                    .Where(obj => obj.GetType() == typeof(Exception) || obj.GetType().GetTypeInfo().IsSubclassOf(typeof(Exception)))
                    .ToList();

                var exeptionsCount = exceptions.Count();
                if (exeptionsCount == 0)
                    return null;

                if (exeptionsCount == 1)
                    return exceptions.SingleOrDefault() as Exception;

                return new AggregateException((IEnumerable<Exception>) exceptions);
            }
        }
    }
}
