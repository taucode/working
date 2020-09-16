using Serilog;
using Serilog.Events;
using System;
using System.Text;

namespace TauCode.Working
{
    public class ObjectLogger
    {
        #region Constructor

        public ObjectLogger(object host, string hostName)
        {
            this.Host = host ?? throw new ArgumentNullException(nameof(host));
            this.HostName = hostName;
        }

        #endregion

        #region Protected

        protected virtual ILogger GetSerilogLogger() => Log.ForContext("taucode.working", true);

        protected virtual string GetCallSignature(string methodName)
        {
            methodName ??= "<unknown_method>";
            var sb = new StringBuilder();
            sb.Append(this.Host.GetType().Name);
            sb.Append('(');
            if (this.HostName == null)
            {
                sb.Append("<null>");
            }
            else
            {
                sb.Append('\'');
                sb.Append(this.HostName);
                sb.Append('\'');
            }

            sb.Append(')');
            sb.Append('.');
            sb.Append(methodName);
            sb.Append(' ');

            return sb.ToString();
        }

        protected virtual void Write(LogEventLevel level, string message, string methodName, Exception ex)
        {
            if (!this.IsEnabled)
            {
                return;
            }

            var logger = this.GetSerilogLogger();
            var callSignature = this.GetCallSignature(methodName);

            if (ex == null)
            {
                logger.Write(level, $"{callSignature}{message}");
            }
            else
            {
                logger.Write(level, ex, $"{callSignature}{message}");
            }
        }

        #endregion

        #region Public

        public object Host { get; }

        public string HostName { get; }

        public bool IsEnabled { get; set; }

        public virtual void Verbose(string message, string methodName = null, Exception ex = null) =>
            this.Write(LogEventLevel.Verbose, message, methodName, ex);

        public virtual void Debug(string message, string methodName = null, Exception ex = null) =>
            this.Write(LogEventLevel.Debug, message, methodName, ex);

        public virtual void Information(string message, string methodName = null, Exception ex = null) =>
            this.Write(LogEventLevel.Information, message, methodName, ex);

        public virtual void Warning(string message, string methodName = null, Exception ex = null) =>
            this.Write(LogEventLevel.Warning, message, methodName, ex);

        public virtual void Error(string message, string methodName = null, Exception ex = null) =>
            this.Write(LogEventLevel.Error, message, methodName, ex);

        public virtual void Fatal(string message, string methodName = null, Exception ex = null) =>
            this.Write(LogEventLevel.Fatal, message, methodName, ex);

        #endregion
    }
}
