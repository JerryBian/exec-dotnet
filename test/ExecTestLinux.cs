using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ExecDotnet.Test
{
    public class ExecTestLinux
    {
        [SkippableFact]
        public async Task GrepUname()
        {
            Skip.If(OperatingSystem.IsWindows());

            var date = DateTime.Now;
            var output = await Exec.RunAsync("uname");
            Assert.Equal(0, output.ExitCode);
            Assert.True(output.ExitTime > date);
            Assert.False(string.IsNullOrEmpty(output.Output));
        }

        [SkippableFact]
        public async Task Timeout()
        {
            Skip.If(OperatingSystem.IsWindows());

            var date = DateTime.Now;
            var option = new ExecOption();
            option.Timeout = TimeSpan.FromSeconds(3);
            var sw = Stopwatch.StartNew();
            var output = await Exec.RunAsync("sleep 60s", option);
            sw.Stop();
            Assert.NotEqual(0, output.ExitCode);
            Assert.True(output.ExitTime > date);
            Assert.True(sw.Elapsed.TotalSeconds < 5);
        }
    }
}
