using System;
using System.Collections.Generic;

namespace HyperCard
{
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
                byte partStyleData = reader.ReadByte();

                short[] formatting = null;

                if ((partStyleData & 0x80) == 0x80)
                {
                    int size = ((partStyleData & 0x7f) << 8) | reader.ReadByte();
                    formatting = new short[(size - 2) / 2];
                    for (int i = 0; i < (size - 2) / 4; i++)
                    {
                        short textPosition = reader.ReadInt16();
                        short styleId = reader.ReadInt16();
                        formatting[i * 2] = textPosition;
                        formatting[i * 2 + 1] = styleId;
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
