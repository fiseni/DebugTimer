using Pozitron.Diagnostics;

namespace SampleLib1;

public class TestRunner1
{
    public static void Run()
    {
        DebugTimer.StartGlobal("GlobalFromTestRunner1");
        DebugTimer.Start();
        Console.WriteLine("Hello from SampleLib1!");
        DebugTimer.Stop();
    }
}
