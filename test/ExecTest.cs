using System.Text;

namespace ExecDotnet.Test
{
    public class ExecTest
    {
        [Fact]
        public async Task Echo()
        {
            var date = DateTime.Now;
            var output = await Exec.RunAsync("echo hello");
            Assert.Equal(0, output.ExitCode);
            Assert.True(output.ExitTime > date);
            Assert.Contains("hello", output.Output);
        }

        [Fact]
        public async Task HandleOutputAsync()
        {
            var date = DateTime.Now;
            var sb = new StringBuilder();
            var option = new ExecOption();
            option.IsStreamed = true;
            option.OutputDataReceivedHandler = async (d) =>
            {
                sb.AppendLine(d);
                await Task.CompletedTask;
            };

            var output = await Exec.RunAsync(@"echo hello
echo hello2
echo hello3", option);

            Assert.Equal(0, output.ExitCode);
            Assert.True(output.ExitTime > date);
            Assert.True(output.Output == "");

            var sbStr = sb.ToString();
            Assert.Contains("hello", sbStr);
            Assert.Contains("hello2", sbStr);
            Assert.Contains("hello3", sbStr);
        }
    }
}
