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
    }
}
