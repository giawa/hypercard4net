using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace HyperCard
{
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

    public interface IPart
    {
        string Name { get; }

        int ID { get; }

        Type CompiledScript { get; set; }

        void InvokeCompiledMethod(string methodName);

        Scripting.HypertalkScripting.HScript HyperTalkScript { get; }
    }

    public interface IPartContainer : IPart
    {
        Stack Stack { get; }

        Part GetPartFromID(int id);
    }

    public struct FormattedText
    {
        public string Text;
        public short[] Formatting;

        public FormattedText(string text, short[] formatting)
        {
            Text = text;
            Formatting = formatting;
        }
    }

    public class Card : IPartContainer
    {
        public int ID { get; private set; }

        public int BitmapID { get; private set; }

        public int BackgroundID { get; private set; }

        public int PageID { get; private set; }

        public CardFlags Flags { get; set; }

        public List<Part> Parts { get; private set; }

        public string Name { get; set; }

        public string Script { get; set; }

        public Scripting.HypertalkScripting.HScript HyperTalkScript { get; set; }

        public Stack Stack { get; private set; }

        public Type CompiledScript { get; set; }

        public Dictionary<int, FormattedText> BackgroundOverrides { get; private set; }

        public Card(Stack stack, BigEndianBinaryReader reader, int cardChunkSize, int cardID)
        {
            Stack = stack;
            Parts = new List<Part>();
            BackgroundOverrides = new Dictionary<int, FormattedText>();

            long nextBlock = reader.Position + cardChunkSize - 12;

            //CardID = reader.ReadInt32();
            ID = cardID;
            reader.ReadInt32(); // filler
            BitmapID = reader.ReadInt32();
            Flags = (CardFlags)reader.ReadInt16();

            reader.ReadBytes(10);

            PageID = reader.ReadInt32();
            BackgroundID = reader.ReadInt32();
            int partCount = reader.ReadInt16();

            reader.ReadBytes(6);
            int partContentCount = reader.ReadInt16();
            reader.ReadBytes(4);

            for (int i = 0; i < partCount; i++)
            {
                Parts.Add(new Part(this, reader));
            }

            for (int i = 0; i < partContentCount; i++)
            {
                Part.ProcessPartContents(this, reader, BackgroundOverrides);
            }

            Name = reader.ReadString();

            Script = reader.ReadString();
            HyperTalkScript = Scripting.HypertalkScripting.ParseScript(Script, this);

            reader.Position = nextBlock;
        }

        public Part GetPartFromID(int id)
        {
            foreach (var part in Parts)
                if (part.ID == id) return part;
            return null;
        }

        public Part GetPartFromName(string name)
        {
            foreach (var part in Parts)
                if (part.Name.ToLower() == name.ToLower()) return part;
            return null;
        }

        public Background GetBackground()
        {
            foreach (var background in Stack.Backgrounds)
                if (background.ID == BackgroundID)
                    return background;

            return null;
        }

        public void InvokeCompiledMethod(string methodName)
        {
            bool handled = Scripting.CSharpScripting.InvokeCompiledMethod(CompiledScript, methodName, this);

            if (!handled) EscalateMessage(methodName);
        }

        private void EscalateMessage(string methodName)
        {
            var background = GetBackground();

            if (background != null) background.InvokeCompiledMethod(methodName);
            else Stack.InvokeCompiledMethod(methodName);
        }
    }

    public class Background : IPartContainer
    {
        public int ID { get; private set; }

        public int BitmapID { get; private set; }

        public int NextBackgroundID { get; private set; }

        public int PreviousBackgroundID { get; private set; }

        public CardFlags Flags { get; set; }

        public List<Part> Parts { get; private set; }

        public string Name { get; set; }

        public string Script { get; set; }

        public Scripting.HypertalkScripting.HScript HyperTalkScript { get; set; }

        public Stack Stack { get; private set; }

        public Type CompiledScript { get; set; }

        public Background(Stack stack, BigEndianBinaryReader reader, int cardChunkSize, int cardID)
        {
            Stack = stack;
            Parts = new List<Part>();

            long nextBlock = reader.Position + cardChunkSize - 12;

            ID = cardID;
            reader.ReadInt32(); // filler
            BitmapID = reader.ReadInt32();
            Flags = (CardFlags)reader.ReadInt16();

            reader.ReadBytes(6);

            NextBackgroundID = reader.ReadInt32();
            PreviousBackgroundID = reader.ReadInt32();
            int partCount = reader.ReadInt16();

            reader.ReadBytes(6);
            int partContentCount = reader.ReadInt16();
            reader.ReadBytes(4);

            for (int i = 0; i < partCount; i++)
            {
                Parts.Add(new Part(this, reader));
            }

            for (int i = 0; i < partContentCount; i++)
            {
                Part.ProcessPartContents(this, reader, null);
            }

            Name = reader.ReadString();

            Script = reader.ReadString();
            HyperTalkScript = Scripting.HypertalkScripting.ParseScript(Script, this);

            reader.Position = nextBlock;
        }

        public Part GetPartFromID(int id)
        {
            foreach (var part in Parts)
                if (part.ID == id) return part;
            return null;
        }

        public Part GetPartFromName(string name)
        {
            foreach (var part in Parts)
                if (part.Name.ToLower() == name.ToLower()) return part;
            return null;
        }

        public void InvokeCompiledMethod(string methodName)
        {
            bool handled = Scripting.CSharpScripting.InvokeCompiledMethod(CompiledScript, methodName, this);

            if (!handled) EscalateMessage(methodName);
        }

        private void EscalateMessage(string methodName)
        {
            Stack.InvokeCompiledMethod(methodName);
        }
    }

    public class Rect
    {
        public short Top;
        public short Left;
        public short Bottom;
        public short Right;

        public short Width { get { return (short)Math.Abs(Right - Left); } }

        public short Height { get { return (short)Math.Abs(Bottom - Top); } }

        public Rect(BigEndianBinaryReader reader)
        {
            Top = reader.ReadInt16();
            Left = reader.ReadInt16();
            Bottom = reader.ReadInt16();
            Right = reader.ReadInt16();
        }

        public System.Drawing.Rectangle ToRectangle()
        {
            return new System.Drawing.Rectangle(Left, Top, Width - 1, Height - 1);
        }
    }

    public class Part : IPart
    {
        public bool Dirty { get; set; }

        public int ID { get; private set; }

        public PartType Type { get; private set; }

        public PartFlags Flags { get; set; }

        public Rect Rect { get; set; }

        public PartStyle Style { get; set; }

        private short titleWidthOrLastSelectedLine;
        private short iconIDOrFirstSelectedLine;

        public short TitleWidth
        {
            get { return titleWidthOrLastSelectedLine; }
            set { titleWidthOrLastSelectedLine = value; }
        }

        public short LastSelectedLine
        {
            get { return titleWidthOrLastSelectedLine; }
            set { titleWidthOrLastSelectedLine = value; }
        }

        public short IconID
        {
            get { return iconIDOrFirstSelectedLine; }
            set { iconIDOrFirstSelectedLine = value; }
        }

        public short FirstSelectedLine
        {
            get { return iconIDOrFirstSelectedLine; }
            set { iconIDOrFirstSelectedLine = value; }
        }

        public TextAlign TextAlign { get; set; }

        public short TextFont { get; set; }

        public short TextSize { get; set; }

        public TextStyle TextStyle { get; set; }

        public short TextHeight { get; set; }

        public string Name { get; set; }

        public string Script { get; set; }

        public Scripting.HypertalkScripting.HScript HyperTalkScript { get; set; }

        public bool ShowName { get; set; }

        public bool AutoSelect { get; set; }

        private bool highlight = false;

        public bool Highlight
        {
            get { return highlight; }
            set
            {
                highlight = value;
                //if (Style == PartStyle.CheckBox || Style == PartStyle.Transparent) Dirty = true;
                Dirty = true;
            }
        }

        public bool ShowLines { get; set; }

        public bool AutoHighlight { get; set; }

        public bool WideMargins { get; set; }

        public bool NotSharedHighlight { get; set; }

        public bool MultipleLines { get; set; }

        public byte Family { get; set; }

        public FormattedText Contents { get; set; }

        public string[] Lines { get; set; }

        public bool Click { get; set; }

        public IPartContainer Parent { get; set; }

        public Part(IPartContainer parent, BigEndianBinaryReader reader)
        {
            short size = reader.ReadInt16();
            long nextBlock = reader.Position + size - 2;

            Parent = parent;
            ID = reader.ReadInt16();
            Type = (PartType)reader.ReadByte();
            Flags = (PartFlags)reader.ReadByte();
            Rect = new Rect(reader);
            byte moreFlags = reader.ReadByte();
            Style = (PartStyle)reader.ReadByte();
            titleWidthOrLastSelectedLine = reader.ReadInt16();
            iconIDOrFirstSelectedLine = reader.ReadInt16();
            TextAlign = (TextAlign)reader.ReadInt16();
            TextFont = reader.ReadInt16();
            TextSize = reader.ReadInt16();
            TextStyle = (TextStyle)reader.ReadInt16();
            TextHeight = reader.ReadInt16();
            Name = reader.ReadString();
            reader.ReadByte();  // always zero
            Script = reader.ReadString();
            HyperTalkScript = Scripting.HypertalkScripting.ParseScript(Script, this);

            reader.Position = nextBlock;
            //Console.WriteLine("nextBlock: {0}   actual position: {1}", nextBlock, reader.Position);

            ShowName = AutoSelect = (moreFlags & 0x80) == 0x80;
            Highlight = ShowLines = (moreFlags & 0x40) == 0x40;
            AutoHighlight = WideMargins = (moreFlags & 0x20) == 0x20;
            NotSharedHighlight = MultipleLines = (moreFlags & 0x10) == 0x10;
            Family = (byte)(moreFlags & 0x0f);
        }

        public override string ToString()
        {
            return string.Format("{0} : {1}", Type, Name);
        }

        public static void ProcessPartContents(IPartContainer card, BigEndianBinaryReader reader, Dictionary<int, FormattedText> backgroundOverrides)
        {
            short partContentID = Math.Abs(reader.ReadInt16());
            short partContentSize = reader.ReadInt16();

            if (partContentSize > 1)
            {
                byte partContentType = reader.ReadByte();

                short[] formatting = null;

                if ((partContentType & 0x80) == 0x80)
                {
                    int size = ((partContentType & 0x7f) << 8) | reader.ReadByte();
                    formatting = new short[(size - 2) / 2];
                    for (int i = 0; i < (size - 2) / 4; i++)
                    {
                        short textPosition = reader.ReadInt16();
                        short styleId = reader.ReadInt16();
                        formatting[i * 2] = textPosition;
                        formatting[i * 2 + 1] = styleId;
                        Console.WriteLine($"Syle {styleId} at text position {textPosition}");
                    }

                    partContentSize -= (short)(size - 1);
                }

                string text = Utilities.FromMacRoman(reader.ReadBytes(partContentSize - 1), partContentSize - 1);

                var part = card.GetPartFromID(partContentID);

                if (part != null)
                {
                    part.Contents = new FormattedText(text, formatting);
                    if (text.Contains("\r")) part.Lines = text.Split(new char[] { '\r' });
                }
                else if (backgroundOverrides != null)
                {
                    backgroundOverrides.Add(partContentID, new FormattedText(text, formatting));
                }
            }

            if ((reader.Position % 2) != 0) reader.Position += (reader.Position % 2);
        }

        public bool Visible
        {
            get { return (Flags & PartFlags.NotVisible) == 0; }
            set
            {
                Flags = (Flags & ~PartFlags.NotVisible);
                if (!value) Flags |= PartFlags.NotVisible;
            }
        }

        #region Messages
        public Type CompiledScript { get; set; }

        public void InvokeCompiledMethod(string methodName)
        {
            bool handled = false;

            if (CompiledScript != null)
            {
                handled = Scripting.CSharpScripting.InvokeCompiledMethod(CompiledScript, methodName, this);
            }
            else if (HyperTalkScript != null && HyperTalkScript.Methods.ContainsKey(methodName.ToLower()))
            {
                HyperTalkScript.Methods[methodName.ToLower()].Interpret(this);
            }

            //if (!handled) EscalateMessage(methodName);
        }

        private void EscalateMessage(string methodName)
        {
            if (Parent != null) Parent.InvokeCompiledMethod(methodName);
        }

        public void OnMouseDown()
        {
            InvokeCompiledMethod("mouseDown");
        }

        public void OnMouseStillDown()
        {
            InvokeCompiledMethod("mouseStillDown");
        }

        public void OnMouseUp()
        {
            InvokeCompiledMethod("mouseUp");
        }

        public void OnMouseEnter()
        {
            InvokeCompiledMethod("mouseEnter");
        }

        public void OnMouseWithin()
        {
            InvokeCompiledMethod("mouseWithin");
        }

        public void OnMouseLeave()
        {
            InvokeCompiledMethod("mouseLeave");
        }
        #endregion
    }
}
