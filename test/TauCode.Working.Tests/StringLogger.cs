using Microsoft.Extensions.Logging;
using System;
using System.Text;
using TauCode.Infrastructure.Time;

// todo clean
namespace TauCode.Working.Tests
{
    public class StringLogger : ILogger
    {
        private readonly StringBuilder _stringBuilder;

        public StringLogger(StringBuilder stringBuilder)
        {
            _stringBuilder = stringBuilder ?? throw new ArgumentNullException(nameof(stringBuilder));
        }

        public StringLogger()
            : this(new StringBuilder())
        {
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.None;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }


            //var fullFilePath = _roundTheCodeLoggerFileProvider.Options.FolderPath + "/" + _roundTheCodeLoggerFileProvider.Options.FilePath.Replace("{date}", DateTimeOffset.UtcNow.ToString("yyyyMMdd"));
            var logRecord =
                $"{"[" + TimeProvider.GetCurrentTime().ToString("yyyy-MM-dd HH:mm:ss+00:00") + "]"} [{logLevel.ToString()}] {formatter(state, exception)} {(exception != null ? exception.StackTrace : "")}";

            _stringBuilder.AppendLine(logRecord);

            //using (var streamWriter = new StreamWriter(fullFilePath, true))
            //{
            //    streamWriter.WriteLine(logRecord);
            //}
        }
    }
}
