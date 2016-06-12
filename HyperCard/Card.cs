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
        NotShowPict = 4096,
        CantDelete = 8192
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
        Bold = 1,
        Italic = 2,
        Underline = 4,
        Outline = 8,
        Shadow = 16,
        Condense = 32,
        Extend = 64,
        Group = 128
    }

    public interface IPartContainer
    {
        Stack Stack { get; }

        int ID { get; }

        string Name { get; }

        Type CompiledScript { get; set; }

        void InvokeCompiledMethod(string methodName);
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

        public Stack Stack { get; private set; }

        public Type CompiledScript { get; set; }

        public Card(Stack stack, BigEndianBinaryReader reader, int cardChunkSize, int cardID)
        {
            Stack = stack;
            Parts = new List<Part>();

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
                Part.ProcessPartContents(Parts, reader);
            }

            Name = reader.ReadString();

            Script = reader.ReadString();
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
                if (part.Name == name) return part;
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
                Part.ProcessPartContents(Parts, reader);
            }

            Name = reader.ReadString();

            Script = reader.ReadString();
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
                if (part.Name == name) return part;
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

    public struct Rect
    {
        public short Top;
        public short Left;
        public short Bottom;
        public short Right;

        public short Width { get { return (short)(Right - Left); } }

        public short Height { get { return (short)(Bottom - Top); } }

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

    public class Part
    {
        public bool Dirty { get; set; }

        public short ID { get; private set; }

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

        public bool ShowName { get; set; }

        public bool AutoSelect { get; set; }

        private bool highlight = false;

        public bool Highlight
        {
            get { return highlight; }
            set
            {
                highlight = value;
                if (Style == PartStyle.CheckBox || Style == PartStyle.Transparent) Dirty = true;
            }
        }

        public bool ShowLines { get; set; }

        public bool AutoHighlight { get; set; }

        public bool WideMargins { get; set; }

        public bool NotSharedHighlight { get; set; }

        public bool MultipleLines { get; set; }

        public byte Family { get; set; }

        public string Contents { get; set; }

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

        public static void ProcessPartContents(List<Part> parts, BigEndianBinaryReader reader)
        {
            short partContentID = Math.Abs(reader.ReadInt16());
            short partContentSize = reader.ReadInt16();

            if (partContentSize > 1)
            {
                byte partContentType = reader.ReadByte();

                if (partContentType == 0)
                {
                    string temp = Utilities.FromMacRoman(reader.ReadBytes(partContentSize - 1), partContentSize - 1);
                    foreach (var part in parts)
                    {
                        if (part.ID == partContentID)
                        {
                            part.Contents = temp;
                            if (temp.Contains("\r")) part.Lines = temp.Split(new char[] { '\r' });
                        }
                    }
                }
                else reader.Position += (partContentSize - 1);
            }

            if ((reader.Position % 2) != 0) reader.Position += (reader.Position % 2);
        }

        #region Messages
        public Type CompiledScript { get; set; }

        internal void InvokeCompiledMethod(string methodName)
        {
            bool handled = Scripting.CSharpScripting.InvokeCompiledMethod(CompiledScript, methodName, this);

            //if (!handled) EscalateMessage(methodName);
        }

        private void EscalateMessage(string methodName)
        {
            if (Parent != null) Parent.InvokeCompiledMethod(methodName);
        }

        public void OnMouseDown()
        {
            InvokeCompiledMethod("OnMouseDown");
        }

        public void OnMouseStillDown()
        {
            InvokeCompiledMethod("OnMouseStillDown");
        }

        public void OnMouseUp()
        {
            InvokeCompiledMethod("OnMouseUp");
        }

        public void OnMouseEnter()
        {
            InvokeCompiledMethod("OnMouseEnter");
        }

        public void OnMouseWithin()
        {
            InvokeCompiledMethod("OnMouseWithin");
        }

        public void OnMouseLeave()
        {
            InvokeCompiledMethod("OnMouseLeave");
        }
        #endregion
    }
}
