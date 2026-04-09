using Pozitron.Diagnostics;
using SampleLib1;

Run();

Console.WriteLine("Completed");

static void Run()
{
    TestRunner1.Run();

    DebugTimer.Start();
    Thread.Sleep(1000);
    DebugTimer.Stop();

    DebugTimer.StopGlobal("GlobalFromTestRunner1");
}
