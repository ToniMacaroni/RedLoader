using System;
using System.Collections.Generic;

namespace RedLoader.Utils;

/// <summary>
/// Only processes items when a certain condition is met.
/// Otherwise it will buffer them until the condition is met.
/// </summary>
/// <typeparam name="T"></typeparam>
public class BufferedAdder<T>
{
    private readonly Queue<T> _queue = new();
    private readonly Func<bool> _canAddFunc;
    private readonly Action<T> _addFunc;
    
    /// <summary>
    /// </summary>
    /// <param name="canAddFunc">The function to determine if an item can be added</param>
    /// <param name="addFunc">The processing function</param>
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
    
    /// <summary>
    /// Process all items that are currently in the buffer and clear the buffer
    /// </summary>
    public void Flush()
    {
        while (_queue.Count > 0)
        {
            var item = _queue.Dequeue();
            _addFunc(item);
        }
    }
}