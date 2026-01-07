using System.Text;

namespace ExecDotnet.Test
{
    public class ExecTest
    {
        [Fact]
        public async Task Echo()
        {
            var result = await Exec.RunAsync("echo hello");
            Assert.Equal(0, result.ExitCode);
            Assert.Contains("hello", result.Output);
            Assert.False(result.WasCancelled);
        }

        [Fact]
        public async Task HandleOutputAsync()
        {
            var output = new StringBuilder();
            var option = new ExecOption
            {
                IsStreamed = true,
                OutputDataReceivedHandler = async (line) =>
                {
                    output.AppendLine(line);
                    await Task.CompletedTask;
                }
            };

            var result = await Exec.RunAsync(@"echo hello
echo hello2
echo hello3", option);

            Assert.Equal(0, result.ExitCode);
            Assert.Empty(result.Output);

            var capturedOutput = output.ToString();
            Assert.Contains("hello", capturedOutput);
            Assert.Contains("hello2", capturedOutput);
            Assert.Contains("hello3", capturedOutput);
        }

        [Fact]
        public async Task CancellationToken()
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));

            // TaskCanceledException is a subclass of OperationCanceledException
            await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            {
                if (OperatingSystem.IsWindows())
                {
                    // Use ping as a reliable way to wait on Windows (ping localhost 60 times with 1 second interval)
                    await Exec.RunAsync("ping 127.0.0.1 -n 60", cts.Token);
                }
                else
                {
                    await Exec.RunAsync("sleep 60", cts.Token);
                }
            });
        }

        [Fact]
        public async Task NullCommandThrowsException()
        {
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await Exec.RunAsync(null);
            });
        }

        [Fact]
        public async Task EmptyCommandThrowsException()
        {
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await Exec.RunAsync("");
            });
        }

        [Fact]
        public async Task WhitespaceCommandThrowsException()
        {
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await Exec.RunAsync("   ");
            });
        }
    }
}
