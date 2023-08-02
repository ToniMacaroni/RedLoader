using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using MelonLoader.LightJson.Serialization;

namespace MelonLoader.LightJson;

/// <summary>
///     Represents an ordered collection of JsonValues.
/// </summary>
[DebuggerDisplay("Count = {Count}")]
[DebuggerTypeProxy(typeof(JsonArrayDebugView))]
public sealed class JsonArray : IEnumerable<JsonValue>
{
	/// <summary>
	///     Gets the number of values in this collection.
	/// </summary>
	public int Count => _items.Count;

	/// <summary>
	///     Gets or sets the value at the given index.
	/// </summary>
	/// <param name="index">The zero-based index of the value to get or set.</param>
	/// <remarks>
	///     The getter will return JsonValue.Null if the given index is out of range.
	/// </remarks>
	public JsonValue this[int index]
    {
        get
        {
            if (index >= 0 && index < _items.Count)
            {
                return _items[index];
            }

            return JsonValue.Null;
        }
        set => _items[index] = value;
    }

	/// <summary>
	///     Initializes a new instance of JsonArray.
	/// </summary>
	public JsonArray()
    {
        _items = new List<JsonValue>();
    }

	/// <summary>
	///     Initializes a new instance of JsonArray, adding the given values to the collection.
	/// </summary>
	/// <param name="values">The values to be added to this collection.</param>
	public JsonArray(params JsonValue[] values) : this()
    {
        if (values == null)
        {
            throw new ArgumentNullException(nameof(values));
        }

        foreach (var value in values)
        {
            _items.Add(value);
        }
    }

	/// <summary>
	///     Returns an enumerator that iterates through the collection.
	/// </summary>
	public IEnumerator<JsonValue> GetEnumerator()
    {
        return _items.GetEnumerator();
    }

	/// <summary>
	///     Returns an enumerator that iterates through the collection.
	/// </summary>
	IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

	/// <summary>
	///     Adds the given value to this collection.
	/// </summary>
	/// <param name="value">The value to be added.</param>
	/// <returns>Returns this collection.</returns>
	public JsonArray Add(JsonValue value)
    {
        _items.Add(value);
        return this;
    }

	/// <summary>
	///     Adds the given value to this collection only if the value is not null.
	/// </summary>
	/// <param name="value">The value to be added.</param>
	/// <returns>Returns this collection.</returns>
	public JsonArray AddIfNotNull(JsonValue value)
    {
        if (!value.IsNull)
        {
            Add(value);
        }

        return this;
    }

	/// <summary>
	///     Inserts the given value at the given index in this collection.
	/// </summary>
	/// <param name="index">The index where the given value will be inserted.</param>
	/// <param name="value">The value to be inserted into this collection.</param>
	/// <returns>Returns this collection.</returns>
	public JsonArray Insert(int index, JsonValue value)
    {
        _items.Insert(index, value);
        return this;
    }

	/// <summary>
	///     Inserts the given value at the given index in this collection.
	/// </summary>
	/// <param name="index">The index where the given value will be inserted.</param>
	/// <param name="value">The value to be inserted into this collection.</param>
	/// <returns>Returns this collection.</returns>
	public JsonArray InsertIfNotNull(int index, JsonValue value)
    {
        if (!value.IsNull)
        {
            Insert(index, value);
        }

        return this;
    }

	/// <summary>
	///     Removes the value at the given index.
	/// </summary>
	/// <param name="index">The index of the value to be removed.</param>
	/// <returns>Return this collection.</returns>
	public JsonArray Remove(int index)
    {
        _items.RemoveAt(index);
        return this;
    }

	/// <summary>
	///     Clears the contents of this collection.
	/// </summary>
	/// <returns>Returns this collection.</returns>
	public JsonArray Clear()
    {
        _items.Clear();
        return this;
    }

	/// <summary>
	///     Determines whether the given item is in the JsonArray.
	/// </summary>
	/// <param name="item">The item to locate in the JsonArray.</param>
	/// <returns>Returns true if the item is found; otherwise, false.</returns>
	public bool Contains(JsonValue item)
    {
        return _items.Contains(item);
    }

	/// <summary>
	///     Determines the index of the given item in this JsonArray.
	/// </summary>
	/// <param name="item">The item to locate in this JsonArray.</param>
	/// <returns>The index of the item, if found. Otherwise, returns -1.</returns>
	public int IndexOf(JsonValue item)
    {
        return _items.IndexOf(item);
    }

	/// <summary>
	///     Returns a JSON string representing the state of the array.
	/// </summary>
	/// <remarks>
	///     The resulting string is safe to be inserted as is into dynamically
	///     generated JavaScript or JSON code.
	/// </remarks>
	public override string ToString()
    {
        return ToString(false);
    }

	/// <summary>
	///     Returns a JSON string representing the state of the array.
	/// </summary>
	/// <remarks>
	///     The resulting string is safe to be inserted as is into dynamically
	///     generated JavaScript or JSON code.
	/// </remarks>
	/// <param name="pretty">
	///     Indicates whether the resulting string should be formatted for human-readability.
	/// </param>
	public string ToString(bool pretty)
    {
        using (var writer = new JsonWriter(pretty))
        {
            return writer.Serialize(this);
        }
    }

    private readonly IList<JsonValue> _items;

    private class JsonArrayDebugView
    {
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public JsonValue[] Items
        {
            get
            {
                var items = new JsonValue[_jsonArray.Count];

                for (var i = 0; i < _jsonArray.Count; i += 1)
                {
                    items[i] = _jsonArray[i];
                }

                return items;
            }
        }

        public JsonArrayDebugView(JsonArray jsonArray)
        {
            this._jsonArray = jsonArray;
        }

        private JsonArray _jsonArray;
    }
}