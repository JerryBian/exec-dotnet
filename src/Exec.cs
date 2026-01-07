using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ExecDotnet
{
    public static class Exec
    {
        public static async Task<ExecResult> RunAsync(string command, CancellationToken cancellationToken = default)
        {
            return await RunAsync(command, new ExecOption(), cancellationToken);
        }

        public static async Task<ExecResult> RunAsync(string command, ExecOption option, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                throw new ArgumentException("Command cannot be null or empty.", nameof(command));
            }

            ExecOption.Validate(option);
            CancellationTokenSource customCancellationTokenSource = null;
            CancellationTokenSource cts = null;
            
            // Create linked cancellation token source
            if (option.Timeout > TimeSpan.Zero && cancellationToken != default)
            {
                customCancellationTokenSource = new CancellationTokenSource(option.Timeout);
                cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, customCancellationTokenSource.Token);
            }
            else if (option.Timeout > TimeSpan.Zero)
            {
                customCancellationTokenSource = new CancellationTokenSource(option.Timeout);
                cts = CancellationTokenSource.CreateLinkedTokenSource(customCancellationTokenSource.Token);
            }
            else if (cancellationToken != default)
            {
                cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            }
            else
            {
                cts = new CancellationTokenSource();
            }

            var result = new ExecResult();
            var sb = new StringBuilder();
            var tempScriptFile = await CreateScriptFileAsync(command, option, cancellationToken);
            using var process = new Process();
            bool wasExternalCancellation = false;
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = option.Shell,
                    Arguments = $"{option.ShellParameter} \"{tempScriptFile}\"",
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                };
                process.StartInfo = startInfo;
                process.EnableRaisingEvents = true;
                
                process.OutputDataReceived += (sender, e) =>
                {
                    if (e.Data != null)
                    {
                        if (option.IsStreamed)
                        {
                            _ = option.OutputDataReceivedHandler(e.Data);
                        }
                        else
                        {
                            sb.AppendLine(e.Data);
                        }
                    }
                };

                process.ErrorDataReceived += (sender, e) =>
                {
                    if (e.Data != null)
                    {
                        if (option.IsStreamed)
                        {
                            _ = option.ErrorDataReceivedHandler(e.Data);
                        }
                        else
                        {
                            sb.AppendLine(e.Data);
                        }
                    }
                };

                process.Exited += (sender, e) =>
                {
                    _ = option.OnExitedHandler(process.ExitCode);
                };

                process.Start();
                process.BeginErrorReadLine();
                process.BeginOutputReadLine();

                await process.WaitForExitAsync(cts.Token);
                result.WasCancelled = false;
            }
            catch (OperationCanceledException) 
            {
                result.WasCancelled = true;
                
                // Check if external cancellation token was the cause (not timeout)
                if (cancellationToken != default && cancellationToken.IsCancellationRequested)
                {
                    wasExternalCancellation = true;
                }
                
                await option.OnCancelledHandler();
                
                // Only throw if it was external cancellation
                if (wasExternalCancellation)
                {
                    throw;
                }
            }
            finally
            {
                customCancellationTokenSource?.Dispose();
                cts?.Dispose();
                
                try
                {
                    if (File.Exists(tempScriptFile))
                    {
                        File.Delete(tempScriptFile);
                    }
                }
                catch
                {
                }

                try
                {
                    if (!process.HasExited)
                    {
                        process.Kill(true);
                        process.WaitForExit();
                    }
                }
                catch
                {
                }

                try
                {
                    result.ExitCode = process.ExitCode;
                    result.ExitTime = process.ExitTime;
                }
                catch
                {
                    // Process may not have valid exit code/time if killed
                    result.ExitCode = -1;
                    result.ExitTime = DateTime.MinValue;
                }
            }

            result.Output = sb.ToString();
            return result;
        }

        private static async Task<string> CreateScriptFileAsync(string command, ExecOption option, CancellationToken cancellationToken)
        {
            var file = Path.Combine(option.TempFileLocation, $"{Path.GetTempFileName()}{option.ShellExtension}");
            await File.WriteAllTextAsync(file, command, new UTF8Encoding(false), cancellationToken);
            return file;
        }
    }
}
