using System;

namespace Wice.Utilities
{
    [Flags]
    public enum DecamelizeTextOptions
    {
        None = 0,
        ForceFirstUpper = 1,
        ForceRestLower = 2,
        UnescapeUnicode = 4,
        UnescapeHexadecimal = 8,
        ReplaceSpacesByUnderscore = 0x10,
        ReplaceSpacesByMinus = 0x20,
        ReplaceSpacesByDot = 0x40,
        KeepFirstUnderscores = 0x80,
        DontDecamelizeNumbers = 0x100,
        KeepFormattingIndices = 0x200,
        Default = ForceFirstUpper | UnescapeUnicode | UnescapeHexadecimal | KeepFirstUnderscores,
    }
}
