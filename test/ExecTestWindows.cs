namespace ExecDotnet.Test
{
    public class ExecTestWindows
    {
        [SkippableFact]
        public async Task ListRootDriver()
        {
            Skip.IfNot(OperatingSystem.IsWindows());

            var output = await Exec.RunAsync("dir C:\\");
            Assert.False(string.IsNullOrEmpty(output));
        }
    }
}
