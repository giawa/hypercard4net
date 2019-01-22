using System;

namespace HyperCard
{
    public enum HyperCardFormat
    {
        HyperCard1 = 8,
        HyperCard2PreRelease = 9,
        HyperCard2 = 10,
        HyperCardNet = 16
    }

    [Flags]
    public enum StackFlags : ushort
    {
        CantPeek = 1024,
        CantAbort = 2048,
        AlwaysOne = 4096,
        PrivateAccess = 8192,
        CantDelete = 16384,
        CantModify = 32768
    }

    public enum UserLevel : short
    {
        Browsing = 1,
        Typing = 2,
        Painting = 3,
        Authoring = 4,
        Scripting = 5
    }

    [Flags]
    public enum CardFlags : ushort
    {
        DontSearch = 2048,
        NotShowPict = 8192,
        CantDelete = 16384
    }

    [Flags]
    public enum PartFlags : byte
    {
        NotEnabledLockText = 1,
        AutoTab = 2,
        NotFixedLineHeight = 4,
        SharedText = 8,
        DontSearch = 16,
        DontWrap = 32,
        NotVisible = 128
    }

    [Flags]
    public enum PartStyle : byte
    {
        Transparent = 0,
        Opaque = 1,
        Rectangle = 2,
        RountRectangle = 3,
        Shadow = 4,
        CheckBox = 5,
        RadioButton = 6,
        Scrolling = 7,
        Standard = 8,
        Default = 9,
        Oval = 10,
        Popup = 11
    }

    public enum PartType : byte
    {
        Button = 1,
        Field = 2
    }

    public enum TextAlign : short
    {
        Left = 0,
        Center = 1,
        Right = -1
    }

    [Flags]
    public enum TextStyle : ushort
    {
        Bold = 256,
        Italic = 512,
        Underline = 1024,
        Outline = 2048,
        Shadow = 4096,
        Condense = 8192,
        Extend = 16384,
        Group = 32768
    }
}
