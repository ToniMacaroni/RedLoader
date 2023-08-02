using System;
using System.IO;

namespace MelonLoader.LightJson.Serialization;

/// <summary>
///     Represents a text scanner that reads one character at a time.
/// </summary>
public sealed class TextScanner
{
	/// <summary>
	///     Gets the position of the scanner within the text.
	/// </summary>
	public TextPosition Position => _position;

	/// <summary>
	///     Gets a value indicating whether there are still characters to be read.
	/// </summary>
	public bool CanRead => _reader.Peek() != -1;

	/// <summary>
	///     Initializes a new instance of TextScanner.
	/// </summary>
	/// <param name="reader">The TextReader to read the text.</param>
	public TextScanner(TextReader reader)
    {
        this._reader = reader ?? throw new ArgumentNullException(nameof(reader));
    }

	/// <summary>
	///     Reads the next character in the stream without changing the current position.
	/// </summary>
	public char Peek()
    {
        var next = _reader.Peek();

        if (next == -1)
        {
            throw new JsonParseException(
                JsonParseException.ErrorType.IncompleteMessage,
                _position
            );
        }

        return (char)next;
    }

	/// <summary>
	///     Reads the next character in the stream, advancing the text position.
	/// </summary>
	public char Read()
    {
        var next = _reader.Read();

        if (next == -1)
        {
            throw new JsonParseException(
                JsonParseException.ErrorType.IncompleteMessage,
                _position
            );
        }

        switch (next)
        {
            case '\r':
                // Normalize '\r\n' line encoding to '\n'.
                if (_reader.Peek() == '\n')
                {
                    _reader.Read();
                }

                goto case '\n';

            case '\n':
                _position.Line += 1;
                _position.Column = 0;
                return '\n';

            default:
                _position.Column += 1;
                return (char)next;
        }
    }

	/// <summary>
	///     Advances the scanner to next non-whitespace character.
	/// </summary>
	public void SkipWhitespace()
    {
        while (char.IsWhiteSpace(Peek()))
        {
            Read();
        }
    }

	/// <summary>
	///     Verifies that the given character matches the next character in the stream.
	///     If the characters do not match, an exception will be thrown.
	/// </summary>
	/// <param name="next">The expected character.</param>
	public void Assert(char next)
    {
        if (Peek() == next)
        {
            Read();
        }
        else
        {
            throw new JsonParseException(
                string.Format("Parser expected '{0}'", next),
                JsonParseException.ErrorType.InvalidOrUnexpectedCharacter,
                _position
            );
        }
    }

	/// <summary>
	///     Verifies that the given string matches the next characters in the stream.
	///     If the strings do not match, an exception will be thrown.
	/// </summary>
	/// <param name="next">The expected string.</param>
	public void Assert(string next)
    {
        try
        {
            for (var i = 0; i < next.Length; i += 1)
            {
                Assert(next[i]);
            }
        }
        catch (JsonParseException e) when (e.Type == JsonParseException.ErrorType.InvalidOrUnexpectedCharacter)
        {
            throw new JsonParseException(
                string.Format("Parser expected '{0}'", next),
                JsonParseException.ErrorType.InvalidOrUnexpectedCharacter,
                _position
            );
        }
    }

    private readonly TextReader _reader;
    private TextPosition _position;
}