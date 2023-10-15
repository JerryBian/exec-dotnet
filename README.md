A dotnet library to fire subprocess command call.

[![master](https://github.com/JerryBian/exec-dotnet/actions/workflows/build.yml/badge.svg)](https://github.com/JerryBian/exec-dotnet/actions/workflows/build.yml)

## Usage

Install from [NuGet](https://www.nuget.org/packages/exec)

```
dotnet add package Exec
```

This library supports .NET 5 and version onwards.

### Basic usage

```csharp
var output = await Exec.RunAsync("echo hello");
Assert.Contains("hello", output);
```

That's all. 

Application just call the static methods of `Exec` class, along with below APIs.

```csharp
Task<string> RunAsync(string command, CancellationToken cancellationToken = default)

Task<string> RunAsync(string command, ExecOption option, CancellationToken cancellationToken = default)
```
The `ExecOption` can accept customized parameters in case of the default behavior not meet your requirements.

| Property | Comment | Default |
| --- | --- | --- |
| Shell | The shell application used to execute command passed in. | In windows default to `cmd.exe`, and `/bin/sh` for Linux and macOS. |
| ShellParameter | Shell parameter for carrying out the script file. | In windows default to `/q/ c`, and empty string for Linxu and macOS. |
| ShellExtension | Script file extension. | In windows default to `.bat`, and `.sh` for Linxu and macOS. |
| TempFileLocation | Directory for script file. | User temp file directory. |
| Timeout | Command execution timeout. | Zero. Means no timeout applied. |
| OutputDataReceivedHandler | Handler for new line of output data. Only works while `IsStreamed=true`. | Discard the output data. |
| ErrorDataReceivedHandler | Handler for new line of error data. Only works while `IsStreamed=true`. | Discard the error data. |
| OnExitedHandler | Handler for process exited. | Do nothing. |
| OnCancelledHandler | Handler for process was cancelled. | Do nothing. |
| IsStreamed | Switch for asynchronous output/error data handling. | false |

### Advanced usage

### Execute PowerShell command

```csharp
var option = new ExecOption();
option.Shell = "pwsh";
option.ShellParameter = "";
option.ShellExtension = ".ps1";
var command = @"function Test-Add {
[CmdletBinding()]
param (
[int]$Val1,
[int]$Val2
)

Write-Output $($Val1 + $Val2)
}

Test-Add 10 12";
var output = await Exec.RunAsync(command, option);
Assert.Contains("22", output);
```

### Execute command with timeout

```csharp
var option = new ExecOption();
option.Timeout = TimeSpan.FromSeconds(3);
var sw = Stopwatch.StartNew();
var output = await Exec.RunAsync("timeout 60", option);
sw.Stop();
Assert.True(sw.Elapsed.TotalSeconds < 5);
```

### Handler stdout and stderr asynchronously

```csharp
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

Assert.True(output == "");

var sbStr = sb.ToString();
Assert.Contains("hello", sbStr);
Assert.Contains("hello2", sbStr);
Assert.Contains("hello3", sbStr);
```

## License

[MIT](./LICENSE)