A dotnet library to fire subprocess command call.

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