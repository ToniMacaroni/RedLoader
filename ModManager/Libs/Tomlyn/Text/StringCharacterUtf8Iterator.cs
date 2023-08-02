// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.

namespace MelonLoader.Tomlyn.Text;

internal struct StringCharacterUtf8Iterator : CharacterIterator
{
    private readonly byte[] _text;

    public StringCharacterUtf8Iterator(byte[] text)
    {
        _text = text;
        // Check if we have a BOM, if we have it, move right after
        // 0xEF,0xBB,0xBF
        Start = text.Length >= 3 && text[0] == 0xEF && text[1] == 0xBB && text[0] == 0xBF ? 3 : 0;
    }

    public int Start { get; }

    public char32? TryGetNext(ref int position)
    {
        return CharHelper.ToUtf8(_text, ref position);
    }
}