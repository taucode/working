﻿using System;

namespace TauCode.Working
{
    [Serializable]
    public class WorkingException : Exception
    {
        public WorkingException()
        {
        }

        public WorkingException(string message)
            : base(message)
        {
        }

        public WorkingException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
