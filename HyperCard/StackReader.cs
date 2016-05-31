using System;
using System.Collections.Generic;
using System.IO;

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

    public class StackReader
    {
        public List<Woba> Bitmaps = new List<Woba>();

        public List<Card> Cards = new List<Card>();

        public List<Background> Backgrounds = new List<Background>();

        public HyperCardFormat Format { get; set; }

        public short UserLevel { get; set; }

        public StackFlags StackFlags { get; set; }

        public int CreateVersion { get; private set; }

        public int CompactVersion { get; private set; }

        public int ModifyVersion { get; private set; }

        public int OpenVersion { get; private set; }

        public short WindowTop { get; private set; }
        public short WindowLeft { get; private set; }
        public short WindowBottom { get; private set; }
        public short WindowRight { get; private set; }

        public short ScreenTop { get; private set; }
        public short ScreenLeft { get; private set; }
        public short ScreenBottom { get; private set; }
        public short ScreenRight { get; private set; }

        public short ScrollY { get; private set; }
        public short ScrollX { get; private set; }

        public short Width { get; private set; }
        public short Height { get; private set; }

        public string Script { get; set; }

        private int firstBackgroundId = -1;
        private int firstCardId = -1;
        private int password = 0;

        public string Name { get; private set; }

        public StackReader(string filename)
        {
            if (!File.Exists(filename)) return;

            FileInfo info = new FileInfo(filename);
            this.Name = info.Name;

            using (BigEndianBinaryReader reader = new BigEndianBinaryReader(filename))
            {
                while (reader.Position < reader.Length)
                {
                    int blockSize = reader.ReadInt32();
                    string blockType = new string(reader.ReadChars(4));
                    int blockID = reader.ReadInt32();

                    long startBlock = reader.Position - 12;
                    long nextBlock = startBlock + blockSize;

                    Console.WriteLine("Found {0} block", blockType);

                    if (blockType == "BMAP")
                    {
                        Bitmaps.Add(new Woba(reader, blockSize, blockID));
                    }
                    else if (blockType == "CARD")
                    {
                        Cards.Add(new Card(reader, blockSize, blockID));
                    }
                    else if (blockType == "BKGD")
                    {
                        Backgrounds.Add(new Background(reader, blockSize, blockID));
                    }
                    else if (blockType == "STAK")
                    {
                        reader.ReadBytes(4);    // filler
                        Format = (HyperCardFormat)reader.ReadInt32();
                        reader.ReadBytes(16);

                        reader.ReadBytes(4); // number of BKGD blocks
                        firstBackgroundId = reader.ReadInt32(); // ID number of the first BKGD block
                        reader.ReadBytes(4); // number of CARD blocks
                        firstCardId = reader.ReadInt32(); // ID number of the first CARD block
                        reader.ReadBytes(16);   // listId, freeCount, freeSize, printId
                        password = reader.ReadInt32(); // password hash for the Protect Stack command; not the same as the ask password hash
                        UserLevel = reader.ReadInt16(); // maximum userLevel allowed by the stack
                        reader.ReadBytes(2);    // something
                        StackFlags = (StackFlags)reader.ReadInt16();
                        reader.ReadBytes(18);   // something

                        CreateVersion = reader.ReadInt32();
                        CompactVersion = reader.ReadInt32();
                        ModifyVersion = reader.ReadInt32();
                        OpenVersion = reader.ReadInt32();

                        reader.ReadBytes(8);    // checksum and something

                        // this stores information about where the stack window will be wrt the screen
                        WindowTop = reader.ReadInt16();
                        WindowLeft = reader.ReadInt16();
                        WindowBottom = reader.ReadInt16();
                        WindowRight = reader.ReadInt16();
                        ScreenTop = reader.ReadInt16();
                        ScreenLeft = reader.ReadInt16();
                        ScreenBottom = reader.ReadInt16();
                        ScreenRight = reader.ReadInt16();
                        ScrollY = reader.ReadInt16();
                        ScrollX = reader.ReadInt16();
                        
                        // seek to font and card information
                        reader.Position = startBlock + 0x01B0;
                        reader.ReadBytes(8);    // fontTableId and styleTableId
                        Height = reader.ReadInt16();
                        Width = reader.ReadInt16();

                        if (Height == 0) Height = (short)(WindowBottom - WindowTop);
                        if (Width == 0) Width = (short)(WindowRight - WindowLeft);

                        // seek to patterns
                        reader.Position = startBlock + 0x02C0;

                        // seek to script
                        reader.Position = startBlock + 0x0600;
                        Script = new string(reader.ReadChars((int)(nextBlock - reader.Position))).Replace((char)65533, '∞');
                    }
                    else
                    {
                        reader.Position = nextBlock;
                    }
                }
            }
        }
    }
}
