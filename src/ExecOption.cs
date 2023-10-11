using System;
using System.IO;
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

        public bool IsStreamed { get; set; }

        public static void Validate(ExecOption option)
        {
            if (option == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(option.Shell))
            {
                option.Shell = GetDefaultShell();
            }

            if (option.ShellParameter == null)
            {
                option.ShellParameter = GetDefaultShellParameter();
            }

            if (string.IsNullOrEmpty(option.ShellExtension))
            {
                option.ShellExtension = GetDefaultShellExtension();
            }

            if (string.IsNullOrEmpty(option.TempFileLocation))
            {
                option.TempFileLocation = Path.GetTempPath();
            }

            try
            {
                Directory.CreateDirectory(option.TempFileLocation);
            }
            catch (Exception ex)
            {
                throw new ExecException($"TempFileLocation {option.TempFileLocation} is invalid: {ex.Message}");
            }

            if (option.IsStreamed)
            {
                option.OutputDataReceived ??= d => Task.CompletedTask;
                option.ErrorDataReceived ??= d => Task.CompletedTask;
            }

            option.OnExited ??= c => Task.CompletedTask;
        }

        private static string GetDefaultShell()
        {
            if (OperatingSystem.IsWindows())
            {
                return "cmd.exe";
            }
            else if (OperatingSystem.IsMacOS() || OperatingSystem.IsLinux())
            {
                return "/bin/bash";
            }
            else
            {
                throw new NotSupportedException($"The platform({Environment.OSVersion}) is not supported yet.");
            }
        }

        private static string GetDefaultShellParameter()
        {
            if (OperatingSystem.IsWindows())
            {
                return "/q /c";
            }
            else if (OperatingSystem.IsMacOS() || OperatingSystem.IsLinux())
            {
                return "";
            }
            else
            {
                throw new NotSupportedException($"The platform({Environment.OSVersion}) is not supported yet.");
            }
        }

        private static string GetDefaultShellExtension()
        {
            if (OperatingSystem.IsWindows())
            {
                return ".bat";
            }
            else if (OperatingSystem.IsMacOS() || OperatingSystem.IsLinux())
            {
                return ".sh";
            }
            else
            {
                throw new NotSupportedException($"The platform({Environment.OSVersion}) is not supported yet.");
            }
        }
    }
}
