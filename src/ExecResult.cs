using System;

namespace ExecDotnet
{
    public class ExecResult
    {
        public string Output { get; set; }

        public DateTime ExitTime { get; set; }

        public int ExitCode { get; set; }

        public bool WasCancelled { get; set; }
    }
}
