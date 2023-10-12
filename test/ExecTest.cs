using System.Text;

namespace ExecDotnet.Test
{
    public class ExecTest
    {
        [Fact]
        public async Task Echo()
        {
            var output = await Exec.RunAsync("echo hello");
            Assert.Contains("hello", output);
        }

        [Fact]
        public async Task HandleOutputAsync()
        {
            var option = new ExecOption();
            option.IsStreamed = true;
            var sb = new StringBuilder();
            option.OutputDataReceivedHandler = async (d) =>
            {
                sb.AppendLine(d);
                await Task.CompletedTask;
            };
            var output = await Exec.RunAsync(@"echo hello
echo hello2
echo hello3", option);
            Assert.True(output == "");

            var sbStr = sb.ToString();
            Assert.Contains("hello", sbStr);
            Assert.Contains("hello2", sbStr);
            Assert.Contains("hello3", sbStr);
        }
    }
}
