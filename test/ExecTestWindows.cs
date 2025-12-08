using System.Diagnostics;

namespace ExecDotnet.Test
{
    public class ExecTestWindows
    {
        [SkippableFact]
        public async Task InvlidCommand()
        {
            Skip.IfNot(OperatingSystem.IsWindows());

            var date = DateTime.Now;
            var output = await Exec.RunAsync("===?><::>>>>");
            Assert.NotEqual(0, output.ExitCode);
            Assert.True(output.ExitTime > date);
            Assert.False(string.IsNullOrEmpty(output.Output));
        }

        [SkippableFact]
        public async Task ListRootDriver()
        {
            Skip.IfNot(OperatingSystem.IsWindows());

            var date = DateTime.Now;
            var output = await Exec.RunAsync("dir C:\\");
            Assert.Equal(0, output.ExitCode);
            Assert.True(output.ExitTime > date);
            Assert.False(string.IsNullOrEmpty(output.Output));
        }

        [SkippableFact]
        public async Task Timeout()
        {
            Skip.IfNot(OperatingSystem.IsWindows());

            var date = DateTime.Now;
            var option = new ExecOption();
            option.Timeout = TimeSpan.FromSeconds(3);
            var sw = Stopwatch.StartNew();
            var output = await Exec.RunAsync("timeout 60", option);
            sw.Stop();
            Assert.NotEqual(0, output.ExitCode);
            Assert.True(output.ExitTime > date);
            Assert.True(sw.Elapsed.TotalSeconds < 5);
        }

        [SkippableFact]
        public async Task UsePowershell()
        {
            Skip.IfNot(OperatingSystem.IsWindows());

            var date = DateTime.Now;
            var option = new ExecOption();
            option.Shell = "pwsh";
            option.ShellParameter = "";
            option.ShellExtension = ".ps1";
            var command = @"function Test-Add {
[CmdletBinding()]
param (
[int]$Val1,
[int]$Val2
)

Write-Output $($Val1 + $Val2)
}

Test-Add 10 12";
            var output = await Exec.RunAsync(command, option);
            Assert.Equal(0, output.ExitCode);
            Assert.True(output.ExitTime > date);
            Assert.Contains("22", output.Output);
        }
    }
}
