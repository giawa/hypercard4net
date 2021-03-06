﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace HyperCard
{
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
}
