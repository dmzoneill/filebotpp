using System;

namespace FileBotPP.Tree
{
    [Flags]
    public enum ItemStatusMask : short
    {
        None = 0,
        Empty = 1,
        Corrupted = 2,
        BadLocation = 4,
        Missing = 8,
        Quality = 16,
        DisallowedType = 32,
        BadName = 64,
        Extra = 128
    };
}