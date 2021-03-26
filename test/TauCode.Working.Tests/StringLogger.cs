using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using TauCode.Infrastructure.Time;

// todo clean
namespace TauCode.Working.Tests
{
    public class StringLogger : ILogger
    {
        private readonly StringBuilder _stringBuilder;
        private readonly List<LogEntry> _entries;

        public StringLogger(StringBuilder stringBuilder)
        {
            _stringBuilder = stringBuilder ?? throw new ArgumentNullException(nameof(stringBuilder));
            _entries = new List<LogEntry>();
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

            var timeStamp = TimeProvider.GetCurrentTime();
            var timeStampString = timeStamp.ToString("yyyy-MM-dd HH:mm:ss+00:00");
            var message = formatter(state, exception);
            var exceptionString = exception == null ? "" : exception.StackTrace;

            var logRecord = $"[{timeStampString}] [{logLevel}] {message} {exceptionString}";
            _stringBuilder.AppendLine(logRecord);

            var entry = new LogEntry(timeStamp, logLevel, message, exception);

            _entries.Add(entry);
        }

        public IReadOnlyList<LogEntry> Entries => _entries;

        public override string ToString() => _stringBuilder.ToString();
    }
}
