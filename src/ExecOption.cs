using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ExecDotnet
{
    public class ExecOption
    {
        public string Shell { get; set; }

        public string ShellParameter { get; set; }

        public string ShellExtension { get; set; }

        public string TempFileLocation { get; set; }

        public TimeSpan Timeout { get; set; }

        public Func<string, Task> OutputDataReceived { get; set; }

        public Func<string, Task> ErrorDataReceived { get; set; }

        public Func<int, Task> OnExited { get; set; }
    }
}
