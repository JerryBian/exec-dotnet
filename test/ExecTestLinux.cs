using System.Diagnostics;

namespace ExecDotnet.Test
{
    public class ExecTestLinux
    {
        [SkippableFact]
        public async Task Uname()
        {
            Skip.If(OperatingSystem.IsWindows());

            var result = await Exec.RunAsync("uname");
            Assert.Equal(0, result.ExitCode);
            Assert.False(string.IsNullOrEmpty(result.Output));
            Assert.False(result.WasCancelled);
        }

        [SkippableFact]
        public async Task Timeout()
        {
            Skip.If(OperatingSystem.IsWindows());

            var option = new ExecOption
            {
                Timeout = TimeSpan.FromSeconds(3)
            };
            
            var stopwatch = Stopwatch.StartNew();
            var result = await Exec.RunAsync("sleep 60s", option);
            stopwatch.Stop();
            
            Assert.NotEqual(0, result.ExitCode);
            Assert.True(stopwatch.Elapsed.TotalSeconds < 5);
            Assert.True(result.WasCancelled);
        }

        [SkippableFact]
        public async Task ListFiles()
        {
            Skip.If(OperatingSystem.IsWindows());

            var result = await Exec.RunAsync("ls -la");
            Assert.Equal(0, result.ExitCode);
            Assert.False(string.IsNullOrEmpty(result.Output));
            Assert.False(result.WasCancelled);
        }

        [SkippableFact]
        public async Task StreamOutput()
        {
            Skip.If(OperatingSystem.IsWindows());

            var outputLines = new List<string>();
            var option = new ExecOption
            {
                IsStreamed = true,
                OutputDataReceivedHandler = async (line) =>
                {
                    outputLines.Add(line);
                    await Task.CompletedTask;
                }
            };

            var result = await Exec.RunAsync(@"echo line1
echo line2
echo line3", option);

            Assert.Equal(0, result.ExitCode);
            Assert.Empty(result.Output);
            Assert.True(outputLines.Count >= 3);
            Assert.Contains(outputLines, line => line.Contains("line1"));
            Assert.Contains(outputLines, line => line.Contains("line2"));
            Assert.Contains(outputLines, line => line.Contains("line3"));
        }

        [SkippableFact]
        public async Task OnExitedHandler()
        {
            Skip.If(OperatingSystem.IsWindows());

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
            Skip.If(OperatingSystem.IsWindows());

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

            var result = await Exec.RunAsync("sleep 60s", option);
            
            Assert.True(result.WasCancelled);
            Assert.True(wasCancelled);
        }

        [SkippableFact]
        public async Task InvalidCommand()
        {
            Skip.If(OperatingSystem.IsWindows());

            var result = await Exec.RunAsync("nonexistentcommand12345");
            Assert.NotEqual(0, result.ExitCode);
            Assert.False(result.WasCancelled);
        }
    }
}
