using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace MelonLoader.LightJson.Serialization;

/// <summary>
///     Represents a reader that can read JsonValues.
/// </summary>
public sealed class JsonReader
{
    private JsonReader(TextReader reader)
    {
        _scanner = new TextScanner(reader);
    }

    /// <summary>
    ///     Creates a JsonValue by using the given TextReader.
    /// </summary>
    /// <param name="reader">The TextReader used to read a JSON message.</param>
    public static JsonValue Parse(TextReader reader)
    {
        if (reader == null)
        {
            throw new ArgumentNullException("reader");
        }

        return new JsonReader(reader).Parse();
    }

    /// <summary>
    ///     Creates a JsonValue by reader the JSON message in the given string.
    /// </summary>
    /// <param name="source">The string containing the JSON message.</param>
    public static JsonValue Parse(string source)
    {
        if (source == null)
        {
            throw new ArgumentNullException("source");
        }

        using (var reader = new StringReader(source))
        {
            return new JsonReader(reader).Parse();
        }
    }

    /// <summary>
    ///     Creates a JsonValue by reading the given file.
    /// </summary>
    /// <param name="path">The file path to be read.</param>
    public static JsonValue ParseFile(string path)
    {
        if (path == null)
        {
            throw new ArgumentNullException("path");
        }

        using (var reader = new StreamReader(path))
        {
            return new JsonReader(reader).Parse();
        }
    }

    private string ReadJsonKey()
    {
        return ReadString();
    }

    private JsonValue ReadJsonValue()
    {
        _scanner.SkipWhitespace();

        var next = _scanner.Peek();

        if (char.IsNumber(next))
        {
            return ReadNumber();
        }

        switch (next)
        {
            case '{':
                return ReadObject();

            case '[':
                return ReadArray();

            case '"':
                return ReadString();

            case '-':
                return ReadNumber();

            case 't':
            case 'f':
                return ReadBoolean();

            case 'n':
                return ReadNull();

            default:
                throw new JsonParseException(
                    JsonParseException.ErrorType.InvalidOrUnexpectedCharacter,
                    _scanner.Position
                );
        }
    }

    private JsonValue ReadNull()
    {
        _scanner.Assert("null");
        return JsonValue.Null;
    }

    private JsonValue ReadBoolean()
    {
        switch (_scanner.Peek())
        {
            case 't':
                _scanner.Assert("true");
                return true;

            case 'f':
                _scanner.Assert("false");
                return false;

            default:
                throw new JsonParseException(
                    JsonParseException.ErrorType.InvalidOrUnexpectedCharacter,
                    _scanner.Position
                );
        }
    }

    private void ReadDigits(StringBuilder builder)
    {
        while (_scanner.CanRead && char.IsDigit(_scanner.Peek()))
        {
            builder.Append(_scanner.Read());
        }
    }

    private JsonValue ReadNumber()
    {
        var builder = new StringBuilder();

        if (_scanner.Peek() == '-')
        {
            builder.Append(_scanner.Read());
        }

        if (_scanner.Peek() == '0')
        {
            builder.Append(_scanner.Read());
        }
        else
        {
            ReadDigits(builder);
        }

        if (_scanner.CanRead && _scanner.Peek() == '.')
        {
            builder.Append(_scanner.Read());
            ReadDigits(builder);
        }

        if (_scanner.CanRead && char.ToLowerInvariant(_scanner.Peek()) == 'e')
        {
            builder.Append(_scanner.Read());

            var next = _scanner.Peek();

            switch (next)
            {
                case '+':
                case '-':
                    builder.Append(_scanner.Read());
                    break;
            }

            ReadDigits(builder);
        }

        return double.Parse(
            builder.ToString(),
            CultureInfo.InvariantCulture
        );
    }

    private string ReadString()
    {
        var builder = new StringBuilder();

        _scanner.Assert('"');

        while (true)
        {
            var c = _scanner.Read();

            if (c == '\\')
            {
                c = _scanner.Read();

                switch (char.ToLower(c))
                {
                    case '"': // "
                    case '\\': // \
                    case '/': // /
                        builder.Append(c);
                        break;
                    case 'b':
                        builder.Append('\b');
                        break;
                    case 'f':
                        builder.Append('\f');
                        break;
                    case 'n':
                        builder.Append('\n');
                        break;
                    case 'r':
                        builder.Append('\r');
                        break;
                    case 't':
                        builder.Append('\t');
                        break;
                    case 'u':
                        builder.Append(ReadUnicodeLiteral());
                        break;
                    default:
                        throw new JsonParseException(
                            JsonParseException.ErrorType.InvalidOrUnexpectedCharacter,
                            _scanner.Position
                        );
                }
            }
            else if (c == '"')
            {
                break;
            }
            else
            {
                if (char.IsControl(c))
                {
                    throw new JsonParseException(
                        JsonParseException.ErrorType.InvalidOrUnexpectedCharacter,
                        _scanner.Position
                    );
                }

                builder.Append(c);
            }
        }

        return builder.ToString();
    }

    private int ReadHexDigit()
    {
        switch (char.ToUpper(_scanner.Read()))
        {
            case '0':
                return 0;

            case '1':
                return 1;

            case '2':
                return 2;

            case '3':
                return 3;

            case '4':
                return 4;

            case '5':
                return 5;

            case '6':
                return 6;

            case '7':
                return 7;

            case '8':
                return 8;

            case '9':
                return 9;

            case 'A':
                return 10;

            case 'B':
                return 11;

            case 'C':
                return 12;

            case 'D':
                return 13;

            case 'E':
                return 14;

            case 'F':
                return 15;

            default:
                throw new JsonParseException(
                    JsonParseException.ErrorType.InvalidOrUnexpectedCharacter,
                    _scanner.Position
                );
        }
    }

    private char ReadUnicodeLiteral()
    {
        var value = 0;

        value += ReadHexDigit() * 4096; // 16^3
        value += ReadHexDigit() * 256; // 16^2
        value += ReadHexDigit() * 16; // 16^1
        value += ReadHexDigit(); // 16^0

        return (char)value;
    }

    private JsonObject ReadObject()
    {
        return ReadObject(new JsonObject());
    }

    private JsonObject ReadObject(JsonObject jsonObject)
    {
        _scanner.Assert('{');

        _scanner.SkipWhitespace();

        if (_scanner.Peek() == '}')
        {
            _scanner.Read();
        }
        else
        {
            while (true)
            {
                _scanner.SkipWhitespace();

                var key = ReadJsonKey();

                if (jsonObject.ContainsKey(key))
                {
                    throw new JsonParseException(
                        JsonParseException.ErrorType.DuplicateObjectKeys,
                        _scanner.Position
                    );
                }

                _scanner.SkipWhitespace();

                _scanner.Assert(':');

                _scanner.SkipWhitespace();

                var value = ReadJsonValue();

                jsonObject.Add(key, value);

                _scanner.SkipWhitespace();

                var next = _scanner.Read();

                if (next == '}')
                {
                    break;
                }

                if (next == ',')
                {
                    continue;
                }

                throw new JsonParseException(
                    JsonParseException.ErrorType.InvalidOrUnexpectedCharacter,
                    _scanner.Position
                );
            }
        }

        return jsonObject;
    }

    private JsonArray ReadArray()
    {
        return ReadArray(new JsonArray());
    }

    private JsonArray ReadArray(JsonArray jsonArray)
    {
        _scanner.Assert('[');

        _scanner.SkipWhitespace();

        if (_scanner.Peek() == ']')
        {
            _scanner.Read();
        }
        else
        {
            while (true)
            {
                _scanner.SkipWhitespace();

                var value = ReadJsonValue();

                jsonArray.Add(value);

                _scanner.SkipWhitespace();

                var next = _scanner.Read();

                if (next == ']')
                {
                    break;
                }

                if (next == ',')
                {
                    continue;
                }

                throw new JsonParseException(
                    JsonParseException.ErrorType.InvalidOrUnexpectedCharacter,
                    _scanner.Position
                );
            }
        }

        return jsonArray;
    }

    private JsonValue Parse()
    {
        _scanner.SkipWhitespace();
        return ReadJsonValue();
    }

    private readonly TextScanner _scanner;
}