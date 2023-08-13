using System;
using System.Diagnostics;
using System.Drawing;

namespace MelonLoader.Utils;

public class TimingLogger : IDisposable
{
    private readonly string _name;
    private readonly Stopwatch _stopWatch;
    private readonly bool _printOnDispose;

    public TimingLogger(string name, bool printOnDispose = true)
    {
        _name = name;
        _stopWatch = new Stopwatch();
        _printOnDispose = printOnDispose;
    }
    
    public static TimingLogger StartNew(string name, bool printOnDispose = true)
    {
        var logger = new TimingLogger(name, printOnDispose);
        logger.Start();
        return logger;
    }
    
    public void Start()
    {
        _stopWatch.Start();
    }
    
    public void Stop(string operationName = null)
    {
        _stopWatch.Stop();
        Print(operationName);
    }
    
    public void Restart(string operationName = null)
    {
        Print(operationName);
        _stopWatch.Restart();
    }
    
    private void Print(string operationName = null)
    {
        var operation = operationName != null ? $" {operationName}" : string.Empty;
        MelonLogger.Msg(Color.Salmon, $"[{_name}]{operation} took {_stopWatch.ElapsedMilliseconds}ms");
    }

    public void Dispose()
    {
        if (_printOnDispose)
        {
            Print();
        }
        
        _stopWatch.Stop();
    }
}