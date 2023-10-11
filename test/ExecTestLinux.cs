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
    }
}
