using System.Diagnostics;

namespace ExecDotnet.Test
{
    public class ExecTestWindows
    {
        [SkippableFact]
        public async Task InvalidCommand()
        {
            Skip.IfNot(OperatingSystem.IsWindows());

            var result = await Exec.RunAsync("===?><::>>>>");
            Assert.NotEqual(0, result.ExitCode);
            Assert.False(string.IsNullOrEmpty(result.Output));
            Assert.False(result.WasCancelled);
        }

        [SkippableFact]
        public async Task ListRootDirectory()
        {
            Skip.IfNot(OperatingSystem.IsWindows());

            var result = await Exec.RunAsync(@"dir C:\");
            Assert.Equal(0, result.ExitCode);
            Assert.False(string.IsNullOrEmpty(result.Output));
            Assert.False(result.WasCancelled);
        }

        [SkippableFact]
        public async Task Timeout()
        {
            Skip.IfNot(OperatingSystem.IsWindows());

            var option = new ExecOption
            {
                Timeout = TimeSpan.FromSeconds(3)
            };

            var stopwatch = Stopwatch.StartNew();
            // Use ping instead of timeout since timeout doesn't work with redirected stdin
            var result = await Exec.RunAsync("ping 127.0.0.1 -n 60", option);
            stopwatch.Stop();

            Assert.NotEqual(0, result.ExitCode);
            Assert.True(stopwatch.Elapsed.TotalSeconds < 5);
            Assert.True(result.WasCancelled);
        }

        [SkippableFact]
        public async Task UsePowerShell()
        {
            Skip.IfNot(OperatingSystem.IsWindows());

            var option = new ExecOption
            {
                Shell = "pwsh",
                ShellParameter = "",
                ShellExtension = ".ps1"
            };

            var command = @"function Test-Add {
    [CmdletBinding()]
    param (
        [int]$Val1,
        [int]$Val2
    )

    Write-Output $($Val1 + $Val2)
}

Test-Add 10 12";

            var result = await Exec.RunAsync(command, option);
            Assert.Equal(0, result.ExitCode);
            Assert.Contains("22", result.Output);
            Assert.False(result.WasCancelled);
        }

        [SkippableFact]
        public async Task StreamOutputAndError()
        {
            Skip.IfNot(OperatingSystem.IsWindows());

            var outputLines = new List<string>();
            var errorLines = new List<string>();
            var option = new ExecOption
            {
                IsStreamed = true,
                OutputDataReceivedHandler = async (line) =>
                {
                    outputLines.Add(line);
                    await Task.CompletedTask;
                },
                ErrorDataReceivedHandler = async (line) =>
                {
                    errorLines.Add(line);
                    await Task.CompletedTask;
                }
            };

            var result = await Exec.RunAsync(@"echo stdout message
echo stderr message 1>&2", option);

            Assert.Equal(0, result.ExitCode);
            Assert.Empty(result.Output);
            Assert.True(outputLines.Count > 0 || errorLines.Count > 0);
        }

        [SkippableFact]
        public async Task OnExitedHandler()
        {
            Skip.IfNot(OperatingSystem.IsWindows());

            var exitCode = -1;
            var option = new ExecOption
            {
                OnExitedHandler = async (code) =>
                {
                    exitCode = code;
                    await Task.CompletedTask;
                }
            };

            var result = await Exec.RunAsync("echo test", option);

            await Task.Delay(100);
            Assert.Equal(0, result.ExitCode);
            Assert.Equal(0, exitCode);
        }

        [SkippableFact]
        public async Task OnCancelledHandler()
        {
            Skip.IfNot(OperatingSystem.IsWindows());

            var wasCancelled = false;
            var option = new ExecOption
            {
                Timeout = TimeSpan.FromSeconds(1),
                OnCancelledHandler = async () =>
                {
                    wasCancelled = true;
                    await Task.CompletedTask;
                }
            };

            // Use ping instead of timeout since timeout doesn't work with redirected stdin
            var result = await Exec.RunAsync("ping 127.0.0.1 -n 60", option);

            Assert.True(result.WasCancelled);
            Assert.True(wasCancelled);
        }
    }
}
