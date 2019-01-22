using System;
using System.Collections.Generic;

namespace HyperCard
{
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
}
