using System;

namespace TauCode.Working.TestDemo.Cui.Common
{
    public class ExceptionInfo
    {
        public string TypeName { get; set; }
        public string Message { get; set; }

        public static ExceptionInfo FromException(Exception ex)
        {
            return new ExceptionInfo
            {
                TypeName = ex.GetType().FullName,
                Message = ex.Message,
            };
        }
    }
}
