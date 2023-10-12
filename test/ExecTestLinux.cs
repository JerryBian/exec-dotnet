using System.Diagnostics;

namespace ExecDotnet.Test
{
    public class ExecTestLinux
    {
        [SkippableFact]
        public async Task GrepUname()
        {
            Skip.If(OperatingSystem.IsWindows());

            var output = await Exec.RunAsync("uname");
            Assert.False(string.IsNullOrEmpty(output));
        }

        [SkippableFact]
        public async Task Timeout()
        {
            Skip.If(OperatingSystem.IsWindows());

            var option = new ExecOption();
            option.Timeout = TimeSpan.FromSeconds(3);
            var sw = Stopwatch.StartNew();
            var output = await Exec.RunAsync("sleep 60s", option);
            sw.Stop();
            Assert.True(sw.Elapsed.TotalSeconds < 5);
        }
    }
}
