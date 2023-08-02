using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace MelonLoader.LightJson.Serialization;

/// <summary>
///     Represents a writer that can write string representations of JsonValues.
/// </summary>
public sealed class JsonWriter : IDisposable
{
	/// <summary>
	///     Gets or sets the string representing a indent in the output.
	/// </summary>
	public string IndentString { get; set; }

	/// <summary>
	///     Gets or sets the string representing a space in the output.
	/// </summary>
	public string SpacingString { get; set; }

	/// <summary>
	///     Gets or sets the string representing a new line on the output.
	/// </summary>
	public string NewLineString { get; set; }

	/// <summary>
	///     Gets or sets a value indicating whether JsonObject properties should be written in a deterministic order.
	/// </summary>
	public bool SortObjects { get; set; }

	/// <summary>
	///     Initializes a new instance of JsonWriter.
	/// </summary>
	public JsonWriter() : this(false)
    { }

	/// <summary>
	///     Initializes a new instance of JsonWriter.
	/// </summary>
	/// <param name="pretty">
	///     A value indicating whether the output of the writer should be human-readable.
	/// </param>
	public JsonWriter(bool pretty)
    {
        if (pretty)
        {
            IndentString = "\t";
            SpacingString = " ";
            NewLineString = "\n";
        }
    }

	/// <summary>
	///     Releases all the resources used by this object.
	/// </summary>
	public void Dispose()
    {
        if (_writer != null)
        {
            _writer.Dispose();
        }
    }

	/// <summary>
	///     Returns a string representation of the given JsonValue.
	/// </summary>
	/// <param name="jsonValue">The JsonValue to serialize.</param>
	public string Serialize(JsonValue jsonValue)
    {
        Initialize();

        Render(jsonValue);

        return _writer.ToString();
    }

    private void Initialize()
    {
        _indent = 0;
        _isNewLine = true;
        _writer = new StringWriter();
        _renderingCollections = new HashSet<IEnumerable<JsonValue>>();
    }

    private void Write(string text)
    {
        if (_isNewLine)
        {
            _isNewLine = false;
            WriteIndentation();
        }

        _writer.Write(text);
    }

    private void WriteEncodedJsonValue(JsonValue value)
    {
        switch (value.Type)
        {
            case JsonValueType.Null:
                Write("null");
                break;

            case JsonValueType.Boolean:
                Write(value.AsString);
                break;

            case JsonValueType.Number:
                Write(((double)value).ToString(CultureInfo.InvariantCulture));
                break;

            case JsonValueType.String:
                WriteEncodedString(value);
                break;

            case JsonValueType.Object:
                Write(string.Format("JsonObject[{0}]", value.AsJsonObject.Count));
                break;

            case JsonValueType.Array:
                Write(string.Format("JsonArray[{0}]", value.AsJsonArray.Count));
                break;

            default:
                throw new InvalidOperationException("Invalid value type.");
        }
    }

    private void WriteEncodedString(string text)
    {
        Write("\"");

        for (var i = 0; i < text.Length; i += 1)
        {
            var currentChar = text[i];

            // Encoding special characters.
            switch (currentChar)
            {
                case '\\':
                    _writer.Write("\\\\");
                    break;

                case '\"':
                    _writer.Write("\\\"");
                    break;

                case '/':
                    _writer.Write("\\/");
                    break;

                case '\b':
                    _writer.Write("\\b");
                    break;

                case '\f':
                    _writer.Write("\\f");
                    break;

                case '\n':
                    _writer.Write("\\n");
                    break;

                case '\r':
                    _writer.Write("\\r");
                    break;

                case '\t':
                    _writer.Write("\\t");
                    break;

                default:
                    _writer.Write(currentChar);
                    break;
            }
        }

        _writer.Write("\"");
    }

    private void WriteIndentation()
    {
        for (var i = 0; i < _indent; i += 1)
        {
            Write(IndentString);
        }
    }

    private void WriteSpacing()
    {
        Write(SpacingString);
    }

    private void WriteLine()
    {
        Write(NewLineString);
        _isNewLine = true;
    }

    private void WriteLine(string line)
    {
        Write(line);
        WriteLine();
    }

    private void AddRenderingCollection(IEnumerable<JsonValue> value)
    {
        if (!_renderingCollections.Add(value))
        {
            throw new JsonSerializationException(JsonSerializationException.ErrorType.CircularReference);
        }
    }

    private void RemoveRenderingCollection(IEnumerable<JsonValue> value)
    {
        _renderingCollections.Remove(value);
    }

    private void Render(JsonValue value)
    {
        switch (value.Type)
        {
            case JsonValueType.Null:
            case JsonValueType.Boolean:
            case JsonValueType.Number:
            case JsonValueType.String:
                WriteEncodedJsonValue(value);
                break;

            case JsonValueType.Object:
                Render((JsonObject)value);
                break;

            case JsonValueType.Array:
                Render((JsonArray)value);
                break;

            default:
                throw new JsonSerializationException(JsonSerializationException.ErrorType.InvalidValueType);
        }
    }

    private void Render(JsonArray value)
    {
        AddRenderingCollection(value);

        WriteLine("[");

        _indent += 1;

        using (var enumerator = value.GetEnumerator())
        {
            var hasNext = enumerator.MoveNext();

            while (hasNext)
            {
                Render(enumerator.Current);

                hasNext = enumerator.MoveNext();

                if (hasNext)
                {
                    WriteLine(",");
                }
                else
                {
                    WriteLine();
                }
            }
        }

        _indent -= 1;

        Write("]");

        RemoveRenderingCollection(value);
    }

    private void Render(JsonObject value)
    {
        AddRenderingCollection(value);

        WriteLine("{");

        _indent += 1;

        using (var enumerator = GetJsonObjectEnumerator(value))
        {
            var hasNext = enumerator.MoveNext();

            while (hasNext)
            {
                WriteEncodedString(enumerator.Current.Key);
                Write(":");
                WriteSpacing();
                Render(enumerator.Current.Value);

                hasNext = enumerator.MoveNext();

                if (hasNext)
                {
                    WriteLine(",");
                }
                else
                {
                    WriteLine();
                }
            }
        }

        _indent -= 1;

        Write("}");

        RemoveRenderingCollection(value);
    }

    /// <summary>
    ///     Gets an JsonObject enumarator based on the configuration of this JsonWriter.
    ///     If JsonWriter.SortObjects is set to true, then a ordered enumerator is returned.
    ///     Otherwise, a faster non-deterministic enumerator is returned.
    /// </summary>
    /// <param name="jsonObject">The JsonObject for which to get an enumerator.</param>
    private IEnumerator<KeyValuePair<string, JsonValue>> GetJsonObjectEnumerator(JsonObject jsonObject)
    {
        if (SortObjects)
        {
            var sortedDictionary = new SortedDictionary<string, JsonValue>(StringComparer.Ordinal);

            foreach (var item in jsonObject)
            {
                sortedDictionary.Add(item.Key, item.Value);
            }

            return sortedDictionary.GetEnumerator();
        }

        return jsonObject.GetEnumerator();
    }

    private static bool IsValidNumber(double number)
    {
        return !(double.IsNaN(number) || double.IsInfinity(number));
    }

    private int _indent;
    private bool _isNewLine;
    private TextWriter _writer;

    /// <summary>
    ///     A set of containing all the collection objects (JsonObject/JsonArray) being rendered.
    ///     It is used to prevent circular references; since collections that contain themselves
    ///     will never finish rendering.
    /// </summary>
    private HashSet<IEnumerable<JsonValue>> _renderingCollections;
}