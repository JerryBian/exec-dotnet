# A .NET library for executing subprocess commands

[![master](https://github.com/JerryBian/exec-dotnet/actions/workflows/build.yml/badge.svg)](https://github.com/JerryBian/exec-dotnet/actions/workflows/build.yml)
[![NuGet](https://img.shields.io/nuget/v/Exec.svg)](https://www.nuget.org/packages/Exec)

## Installation

Install from [NuGet](https://www.nuget.org/packages/exec):

```bash
dotnet add package Exec
```

This library supports .NET 5 and later versions.

## Usage

### Basic usage

The simplest way to execute a command:

```csharp
var result = await Exec.RunAsync("echo hello");
Assert.Contains("hello", result.Output);
```

That's it! The library provides static methods on the `Exec` class with the following APIs:

```csharp
Task<ExecResult> RunAsync(string command, CancellationToken cancellationToken = default)

Task<ExecResult> RunAsync(string command, ExecOption option, CancellationToken cancellationToken = default)
```

### Configuration

Use `ExecOption` to customize behavior beyond the defaults:

| Property | Description | Default |
| --- | --- | --- |
| Shell | The shell application used to execute the command. | `cmd.exe` on Windows, `/bin/bash` on Linux and macOS. |
| ShellParameter | Shell parameters for executing the script file. | `/q /c` on Windows, empty string on Linux and macOS. |
| ShellExtension | Script file extension. | `.bat` on Windows, `.sh` on Linux and macOS. |
| TempFileLocation | Directory where temporary script files are created. | System temp directory. |
| Timeout | Maximum execution time for the command. | Zero (no timeout). |
| OutputDataReceivedHandler | Handler for each line of standard output. Only called when `IsStreamed=true`. | Output is discarded. |
| ErrorDataReceivedHandler | Handler for each line of standard error. Only called when `IsStreamed=true`. | Errors are discarded. |
| OnExitedHandler | Handler called when the process exits. | No action. |
| OnCancelledHandler | Handler called when the process is cancelled. | No action. |
| IsStreamed | Enable asynchronous streaming of output/error data. | `false` |

### Result

`ExecResult` provides the following information:

| Property | Description |
| --- | --- |
| Output | Combined standard output and standard error when `IsStreamed=false`. Empty when `IsStreamed=true`. |
| ExitCode | The process exit code. |
| ExitTime | The time when the process exited. |
| WasCancelled | `true` if the process was cancelled due to timeout or cancellation token; otherwise `false`. |

### Cancellation behavior

The library handles cancellation differently based on the source:

- **Timeout** (via `ExecOption.Timeout`): Returns `ExecResult` with `WasCancelled = true`
- **CancellationToken**: Throws `OperationCanceledException` (or `TaskCanceledException`)

This allows timeout scenarios to be handled gracefully while external cancellations propagate as exceptions.

## Advanced usage

### Execute PowerShell commands

```csharp
var option = new ExecOption
{
    Shell = "pwsh",
    ShellParameter = "",
    ShellExtension = ".ps1"
};

var command = @"function Test-Add {
    [CmdletBinding()]
    param (
        [int]$Val1,
        [int]$Val2
    )

    Write-Output $($Val1 + $Val2)
}

Test-Add 10 12";

var result = await Exec.RunAsync(command, option);
Assert.Equal(0, result.ExitCode);
Assert.Contains("22", result.Output);
```

### Execute commands with timeout

When using a timeout, the command returns a result with `WasCancelled = true`:

```csharp
var option = new ExecOption
{
    Timeout = TimeSpan.FromSeconds(3)
};

// Note: On Windows, use 'ping' instead of 'timeout' because timeout.exe doesn't work with redirected stdin
var result = await Exec.RunAsync("ping 127.0.0.1 -n 60", option);

// Process was terminated due to timeout
Assert.True(result.WasCancelled);
Assert.NotEqual(0, result.ExitCode);
```

### Stream stdout and stderr asynchronously

```csharp
var output = new StringBuilder();
var option = new ExecOption
{
    IsStreamed = true,
    OutputDataReceivedHandler = async (line) =>
    {
        output.AppendLine(line);
        await Task.CompletedTask;
    }
};

var result = await Exec.RunAsync(@"echo hello
echo hello2
echo hello3", option);

Assert.Equal(0, result.ExitCode);
Assert.Empty(result.Output); // Output is empty in streaming mode

var capturedOutput = output.ToString();
Assert.Contains("hello", capturedOutput);
Assert.Contains("hello2", capturedOutput);
Assert.Contains("hello3", capturedOutput);
```

### Use cancellation tokens

When using a cancellation token, the operation throws `OperationCanceledException`:

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

try
{
    var result = await Exec.RunAsync("long-running-command", cts.Token);
}
catch (OperationCanceledException)
{
    // Command was cancelled via the cancellation token
}
```

## Known limitations

- **Windows `timeout` command**: The Windows `timeout.exe` command doesn't work because it requires interactive input and fails with redirected stdin. Use `ping 127.0.0.1 -n <seconds>` as an alternative for delays.

## Versioning

This project uses automated semantic versioning. The major version is defined in the `VERSION` file, and the minor version is automatically incremented on each release. See [VERSIONING.md](./VERSIONING.md) for details.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

[MIT](./LICENSE)