using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ExecDotnet
{
    public static class Exec
    {
        public static async Task<string> RunAsync(string command, CancellationToken cancellationToken = default)
        {
            return await RunAsync(command, new ExecOption(), cancellationToken);
        }

        public static async Task<string> RunAsync(string command, ExecOption option, CancellationToken cancellationToken = default)
        {
            ExecOption.Validate(option);
            var linkedCancellationTokens = new List<CancellationToken> { cancellationToken };
            if (option.Timeout > TimeSpan.Zero)
            {
                var customCancellationTokenSource = new CancellationTokenSource(option.Timeout);
                linkedCancellationTokens.Add(customCancellationTokenSource.Token);
            }

            var sb = new StringBuilder();
            var tempScriptFile = await CreateScriptFileAsync(command, option, cancellationToken);
            var cts = CancellationTokenSource.CreateLinkedTokenSource(linkedCancellationTokens.ToArray());
            using var process = new Process();
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = option.Shell,
                    Arguments = $"{option.ShellParameter} \"{tempScriptFile}\"",
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                };
                process.StartInfo = startInfo;
                var outputTcs = new TaskCompletionSource<object>();
                var errorTcs = new TaskCompletionSource<object>();

                process.OutputDataReceived += async (sender, e) =>
                {
                    if (e.Data == null)
                    {
                        outputTcs.SetResult(new object());
                    }
                    else
                    {
                        if (option.IsStreamed)
                        {
                            await option.OutputDataReceived(e.Data);
                        }
                        else
                        {
                            sb.AppendLine(e.Data);
                        }
                    }
                };

                process.ErrorDataReceived += async (sender, e) =>
                {
                    if (e.Data == null)
                    {
                        errorTcs.SetResult(new object());
                    }
                    else
                    {
                        if (option.IsStreamed)
                        {
                            await option.ErrorDataReceived(e.Data);
                        }
                        else
                        {
                            sb.AppendLine(e.Data);
                        }
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
                    if (File.Exists(tempScriptFile))
                    {
                        File.Delete(tempScriptFile);
                    }
                }
                catch { }
            }

            return sb.ToString();
        }

        private static async Task<string> CreateScriptFileAsync(string command, ExecOption option, CancellationToken cancellationToken)
        {
            var file = Path.Combine(option.TempFileLocation, $"{Path.GetTempFileName()}{option.ShellExtension}");
            await File.WriteAllTextAsync(file, command, new UTF8Encoding(false), cancellationToken);
            return file;
        }
    }
}
