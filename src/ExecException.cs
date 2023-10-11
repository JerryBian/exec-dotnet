using System;

namespace ExecDotnet
{
    public class ExecException : Exception
    {
        public ExecException(string message) : base(message) { }
    }
}
