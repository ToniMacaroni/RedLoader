using System;
using System.Collections.Generic;

namespace RedLoader.Utils;

public class BufferedAdder<T>
{
    private readonly Queue<T> _queue = new();
    private readonly Func<bool> _canAddFunc;
    private readonly Action<T> _addFunc;
    
    public BufferedAdder(Func<bool> canAddFunc, Action<T> addFunc)
    {
        _canAddFunc = canAddFunc;
        _addFunc = addFunc;
    }
    
    public void Add(T item)
    {
        if (_canAddFunc())
        {
            _addFunc(item);
        }
        else
        {
            _queue.Enqueue(item);
        }
    }
    
    public void Flush()
    {
        while (_queue.Count > 0)
        {
            var item = _queue.Dequeue();
            _addFunc(item);
        }
    }
}