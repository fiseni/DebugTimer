`DebugTimer` is a lightweight **source-only** library for measuring elapsed time in development diagnostics.

It is designed to have near-zero impact when disabled: all timing calls are marked with `[Conditional("DEBUG_TIMER")]`, so they are omitted by the compiler unless the symbol is defined.

## Features

- Source-only package (no runtime assembly)
- Simple `Start` / `Stop` API
- Supports integer and string tags
- Global key API for custom scenarios
- Optional custom logger callback
- Fallback logging with `Debug.Print`

## Installation

```bash
dotnet add package DebugTimer
```

## Enabling timing

Timing methods execute only when `DEBUG_TIMER` is defined.

This package includes a `build/DebugTimer.props` file that adds `DEBUG_TIMER` for **Debug** configuration:

```xml
<PropertyGroup Condition="'$(Configuration)' == 'Debug'">
  <DefineConstants>$(DefineConstants);DEBUG_TIMER</DefineConstants>
</PropertyGroup>
```

If needed, you can define `DEBUG_TIMER` manually in your project.

## Usage

### Basic usage

```csharp
using DebugTimer;

public sealed class UserService
{
    public void GetUsers()
    {
        DebugTimer.Start();

        // code to measure

        DebugTimer.Stop();
    }
}
```

Example output:

```text
DebugTimer - Elapsed:       12 ms | UserService - GetUsers
```

### Usage with int tag

```csharp
using DebugTimer;

public sealed class ReportService
{
    public void BuildReport()
    {
        DebugTimer.Start(1);
        // code to measure

        DebugTimer.Start(2);
        // inner code to measure
        DebugTimer.Stop(2);

        DebugTimer.Stop(1);
    }
}
```

Example output:

```text
DebugTimer - Elapsed:       10 ms | ReportService - BuildReport - 2
DebugTimer - Elapsed:       31 ms | ReportService - BuildReport - 1
```

### Usage with string tag

```csharp
using DebugTimer;

public sealed class ImportService
{
    public void LoadData()
    {
        DebugTimer.Start("Outer");
        // code to measure

        DebugTimer.Start("Inner");
        // inner code to measure
        DebugTimer.Stop("Inner);

        DebugTimer.Stop("Outer");
    }
}
```

Example output:

```text
DebugTimer - Elapsed:       10 ms | ReportService - BuildReport - Inner
DebugTimer - Elapsed:       31 ms | ReportService - BuildReport - Outer
```

### Global key usage across different classes/methods

```csharp
using DebugTimer;

public sealed class JobCoordinator
{
    public static string StartJob(string jobId)
    {
        var key = $"JobPipeline - {jobId}";
        DebugTimer.StartGlobal(key);
        return key;
    }
}

public sealed class JobWorker
{
    public static void FinishJob(string key)
    {
        // potentially in another class/method/layer
        DebugTimer.StopGlobal(key);
    }
}
```

Example output:

```text
DebugTimer - Elapsed:      203 ms | JobPipeline - 42
```

### Custom logger

```csharp
using DebugTimer;

DebugTimer.Initialize((template, source, elapsedMs, key) =>
{
    Console.WriteLine($"{source}: {elapsedMs,8} ms | {key}");
});
```

Serilog example:

```csharp
using DebugTimer;
using Serilog;

DebugTimer.Initialize(Log.Information);
```

## Notes

- Use matching `Start` / `Stop` tags and caller context.
- `Start` called multiple times for the same key restarts the timer.
- `Stop` for a missing key does nothing.
