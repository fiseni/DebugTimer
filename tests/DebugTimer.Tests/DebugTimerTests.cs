using Pozitron.Diagnostics;
using System;
using System.Reflection;
using System.Threading;
using Xunit;

namespace Tests;

[CollectionDefinition("DebugTimerNonParallel", DisableParallelization = true)]
public class DebugTimerCollectionDefinition
{
}

[Collection("DebugTimerNonParallel")]
public class DebugTimerTests
{
    [Fact]
    public void StopGlobal_LogsElapsed_WhenTimerExists()
    {
        string? messageTemplate = null;
        string? source = null;
        long elapsed = -1;
        string? loggedKey = null;

        DebugTimer.Initialize((template, src, ms, key) =>
        {
            messageTemplate = template;
            source = src;
            elapsed = ms;
            loggedKey = key;
        });

        var key = "global-key-" + Guid.NewGuid().ToString("N");

        DebugTimer.StartGlobal(key);
        Thread.Sleep(10);
        DebugTimer.StopGlobal(key);

        Assert.Equal("{DebugTimer} - Elapsed: {DebugTimerElapsed,8} ms | {DebugTimerKey}", messageTemplate);
        Assert.Equal("DebugTimer", source);
        Assert.Equal(key, loggedKey);
        Assert.True(elapsed >= 0);
    }

    [Fact]
    public void StopGlobal_DoesNotLog_WhenTimerDoesNotExist()
    {
        var callCount = 0;

        DebugTimer.Initialize((_, _, _, _) => callCount++);

        DebugTimer.StopGlobal("missing-" + Guid.NewGuid().ToString("N"));

        Assert.Equal(0, callCount);
    }

    [Fact]
    public void StopGlobal_LogsOnlyOnce_ForSameKey()
    {
        var callCount = 0;
        var key = "single-stop-" + Guid.NewGuid().ToString("N");

        DebugTimer.Initialize((_, _, _, _) => callCount++);

        DebugTimer.StartGlobal(key);
        DebugTimer.StopGlobal(key);
        DebugTimer.StopGlobal(key);

        Assert.Equal(1, callCount);
    }

    [Fact]
    public void GenerateKey_WithTag_ReturnsExpectedKey()
    {
        var method = typeof(DebugTimer).GetMethod("GenerateKey", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        var key = (string)method!.Invoke(null, new object?[] { "SampleFile", "Run", "MyTag" })!;

        Assert.Equal("SampleFile - Run - MyTag", key);
    }

    [Fact]
    public void GenerateKey_WithoutTag_DoesNotAddTrailingSeparator()
    {
        var method = typeof(DebugTimer).GetMethod("GenerateKey", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        var key = (string)method!.Invoke(null, new object?[] { "SampleFile", "Run", null })!;

        Assert.Equal("SampleFile - Run", key);
    }

    [Fact]
    public void StartStop_WithStringTag_LogsExpectedKey()
    {
        string? loggedKey = null;
        var callCount = 0;

        DebugTimer.Initialize((_, _, _, key) =>
        {
            callCount++;
            loggedKey = key;
        });

        DebugTimer.Start("TAG-A", filePath: @"C:\Temp\SampleFile.cs", caller: "MethodA");
        Thread.Sleep(10);
        DebugTimer.Stop("TAG-A", filePath: @"C:\Temp\SampleFile.cs", caller: "MethodA");

        Assert.Equal(1, callCount);
        Assert.Equal("SampleFile - MethodA - TAG-A", loggedKey);
    }

    [Fact]
    public void StartStop_WithIntTag_LogsExpectedKey()
    {
        string? loggedKey = null;
        var callCount = 0;

        DebugTimer.Initialize((_, _, _, key) =>
        {
            callCount++;
            loggedKey = key;
        });

        DebugTimer.Start(42, filePath: @"C:\Temp\SampleFile.cs", caller: "MethodB");
        Thread.Sleep(10);
        DebugTimer.Stop(42, filePath: @"C:\Temp\SampleFile.cs", caller: "MethodB");

        Assert.Equal(1, callCount);
        Assert.Equal("SampleFile - MethodB - 42", loggedKey);
    }
}
