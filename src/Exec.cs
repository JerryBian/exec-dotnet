using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ExecDotnet
{
    public static class Exec
    {
        public static async Task RunAsync(string command, ExecOption option, CancellationToken cancellationToken = default)
        {
            var linkedCancellationTokens = new List<CancellationToken> { cancellationToken};
            if(option.Timeout > TimeSpan.Zero)
            {
                var customCancellationTokenSource = new CancellationTokenSource(option.Timeout);
                linkedCancellationTokens.Add(customCancellationTokenSource.Token);
            }

            var cts = CancellationTokenSource.CreateLinkedTokenSource(linkedCancellationTokens.ToArray());
            using var process = new Process();
            try
            {
                process.StartInfo = await GetProcessStartInfoAsync(command, option, cts.Token);
                var outputTcs = new TaskCompletionSource<object>();
                process.OutputDataReceived += async (sender, e) =>
                {
                    if (e.Data == null)
                    {
                        outputTcs.SetResult(new object());
                    }
                    else
                    {
                        await option.OutputDataReceived(e.Data);
                    }
                };

                var errorTcs = new TaskCompletionSource<object>();
                process.ErrorDataReceived += async (sender, e) =>
                {
                    if (e.Data == null)
                    {
                        errorTcs.SetResult(new object());
                    }
                    else
                    {
                        await option.ErrorDataReceived(e.Data);
                    }
                };

                process.Exited += async (sender, e) =>
                {
                    await option.OnExited(process.ExitCode);
                };

                process.Start();
                process.BeginErrorReadLine();
                process.BeginOutputReadLine();

                await Task.WhenAll(outputTcs.Task, errorTcs.Task, process.WaitForExitAsync(cts.Token));
            }
            finally
            {
                try
                {
                    process.Kill(true);
                }
                catch { }
            }
        }
        private static async Task<ProcessStartInfo> GetProcessStartInfoAsync(
            string command, 
            ExecOption option, 
            CancellationToken cancellationToken)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = option.Shell,
                Arguments = $"{option.ShellParameter} {await CreateScriptFileAsync(command, option, cancellationToken)}",
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            return startInfo;
        }

        private static async Task<string> CreateScriptFileAsync(string command, ExecOption option, CancellationToken cancellationToken)
        {
            var file = Path.Combine($"{option.TempFileLocation}{option.ShellExtension}", Path.GetTempFileName());
            await File.WriteAllTextAsync(file, command, cancellationToken);
            return file;
        }
    }
}
