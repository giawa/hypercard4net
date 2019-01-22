using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;

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

    public interface IStackRenderer
    {
        HyperCard.Stack Stack { get; }

        void SetStack(HyperCard.Stack stack);

        void SetCard(HyperCard.Stack stack, HyperCard.Card card);

        void Invalidate();
    }

    public class IconResource
    {
        public short ID { get; private set; }

        public string Name { get; set; }

        public Bitmap Bitmap { get; set; }

        public IconResource(short id, string name, string filename)
        {
            this.ID = id;
            this.Name = name;
            this.Bitmap = HyperCard.Utilities.IconToPng(filename);
        }
    }

    public class Stack
    {
        public IStackRenderer Renderer { get; set; }

        public List<Woba> Bitmaps = new List<Woba>();

        public List<Card> Cards = new List<Card>();

        public List<Background> Backgrounds = new List<Background>();

        public HyperCardFormat Format { get; set; }

        public UserLevel UserLevel { get; set; }

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

        public Dictionary<short, IconResource> IconResources = new Dictionary<short, IconResource>();

        public List List { get; private set; }

        public List<Page> Pages { get; private set; }

        private Card currentCard;
        public Card CurrentCard
        {
            get { return currentCard; }
            set
            {
                if (value != currentCard)
                {
                    if (currentCard != null)
                    {
                        currentCard.InvokeCompiledMethod("closeCard");
                    }

                    currentCard = value;
                    FindEntry();
                    currentCard.InvokeCompiledMethod("openCard");
                    if (Renderer != null) Renderer.Invalidate();
                }
            }
        }

        private void ProcessResources(string filename)
        {
            FileInfo info = new FileInfo(filename);
            string directory = info.Directory.FullName + "\\" + info.Name + ".rsrc";

            if (!Directory.Exists(directory)) return;

            foreach (var file in new DirectoryInfo(directory).GetFiles())
            {
                if (file.Name.EndsWith(".ICON"))
                {
                    string[] split = file.Name.Split(new char[] { '.' });

                    short id = short.Parse(split[0]);
                    string name = split[1];

                    if (!IconResources.ContainsKey(id)) IconResources.Add(id, new IconResource(id, name, file.FullName));
                }
            }
        }

        private Module stackModule;
        public Type CompiledScript { get; set; }

        private void CompileScript(string filename)
        {
            FileInfo info = new FileInfo(filename);
            string path = info.Directory.FullName + "\\" + info.Name + ".cs";

            if (!File.Exists(path)) return;

            stackModule = Scripting.CSharpScripting.CompileScript(File.ReadAllText(path));

            if (stackModule == null) return;

            // we'll be looking up types by name, so store them in a Dictionary for easy lookup
            Dictionary<string, Type> types = new Dictionary<string, Type>();
            foreach (var type in stackModule.GetTypes())
            {
                types.Add(type.FullName, type);
            }

            // now hook up all of the types
            CompiledScript = FindType(types, "Stack");

            foreach (var background in Backgrounds)
            {
                string prefix = string.Format("Background{0}", background.ID);
                background.CompiledScript = FindType(types, prefix);

                foreach (var part in background.Parts)
                    part.CompiledScript = FindType(types, string.Format(prefix + "+Background{0}{1}", part.Type, part.ID));
            }

            foreach (var card in Cards)
            {
                string prefix = string.Format("Card{0}", card.ID);
                card.CompiledScript = FindType(types, prefix);

                foreach (var part in card.Parts)
                    part.CompiledScript = FindType(types, string.Format(prefix + "+Card{0}{1}", part.Type, part.ID));
            }
        }

        private Type FindType(Dictionary<string, Type> types, string name)
        {
            if (types.ContainsKey(name)) return types[name];
            else return null;
        }

        public Stack(string filename)
        {
            if (!File.Exists(filename)) return;

            FileInfo info = new FileInfo(filename);
            this.Name = info.Name;

            this.Pages = new List<Page>();

            ProcessResources(filename);

            using (BigEndianBinaryReader reader = new BigEndianBinaryReader(filename))
            {
                while (reader.Position < reader.Length)
                {
                    int blockSize = reader.ReadInt32();
                    if ((blockSize % 32) != 0) blockSize += 32 - blockSize % 32;
                    string blockType = new string(reader.ReadChars(4));
                    int blockID = reader.ReadInt32();

                    long startBlock = reader.Position - 12;
                    long nextBlock = startBlock + blockSize;

                    Console.WriteLine("Found {0} block at 0x{1:x}", blockType, startBlock);

                    if (blockType == "MAST")
                    {
                        ProcessMAST(reader, blockSize, blockID);
                    }
                    else if (blockType == "BMAP")
                    {
                        Bitmaps.Add(new Woba(reader, blockSize, blockID));
                    }
                    else if (blockType == "CARD")
                    {
                        Cards.Add(new Card(this, reader, blockSize, blockID));
                    }
                    else if (blockType == "BKGD")
                    {
                        Backgrounds.Add(new Background(this, reader, blockSize, blockID));
                    }
                    // LIST gets processed as part of the MAST block
                    /*else if (blockType == "LIST")
                    {
                        List = new List(reader, blockSize, blockID);
                    }*/
                    else if (blockType == "PAGE")
                    {
                        Pages.Add(new Page(reader, blockSize, blockID, List));
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
                        UserLevel = (UserLevel)reader.ReadInt16(); // maximum userLevel allowed by the stack
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

                    reader.Position = nextBlock;
                }
            }

            CompileScript(filename);
            CurrentCard = GetCardFromID(List.Pages[0].PageEntries[0]);
        }

        private void ProcessMAST(BigEndianBinaryReader reader, int mastChunkSize, int mastID)
        {
            long nextBlock = reader.Position + mastChunkSize - 12;
            Dictionary<string, int> chunks = new Dictionary<string, int>();

            reader.ReadBytes(20);   // filler, something
            int[] offsets = new int[(mastChunkSize - 32) / 4];

            // find all of the chunk offsets (which are aligned to 32 byte boundaries)
            for (int i = 0; i < offsets.Length; i++)
            {
                offsets[i] = reader.ReadInt32();
            }

            // now scan through the file to each of the chunks, caching the name/offset
            for (int i = 0; i < offsets.Length; i++)
            {
                if ((offsets[i] & 0xffffff00) == 0) continue;
                if (((offsets[i] & 0xffffff00) >> 3) + 8 >= reader.Length) continue;

                reader.Position = ((offsets[i] & 0xffffff00) >> 3) + 4;
                string blockType = new string(reader.ReadChars(4));
                if (!IsValidChunk(blockType)) continue;

                if (!chunks.ContainsKey(blockType)) chunks.Add(blockType, (int)(reader.Position - 8));
            }

            // force the processing of the LIST block, because we need that information for PAGE blocks (which can be out of order)
            if (chunks.ContainsKey("LIST"))
            {
                reader.Position = chunks["LIST"];

                int blockSize = reader.ReadInt32();
                string blockType = new string(reader.ReadChars(4));
                int blockID = reader.ReadInt32();

                List = new List(reader, blockSize, blockID);
            }

            // read the page block if it exists with the MAST (this occurs in Graph Maker, for example)
            // since there can be multiple of these we should really process all of them...
            if (chunks.ContainsKey("PAGE") && chunks["PAGE"] < nextBlock)
            {
                reader.Position = chunks["PAGE"];

                int blockSize = reader.ReadInt32();
                string blockType = new string(reader.ReadChars(4));
                int blockID = reader.ReadInt32();

                Pages.Add(new Page(reader, blockSize, blockID, List));
            }

            // finally, return to the correct location to continue parsing
            reader.Position = nextBlock;
        }

        private bool IsValidChunk(string chunk)
        {
            switch (chunk)
            {
                case "STAK":
                case "MAST":
                case "LIST":
                case "PAGE":
                case "BKGD":
                case "CARD":
                case "BMAP":
                case "FREE":
                case "STBL":
                case "FTBL":
                case "PRNT":
                case "PRST":
                case "PRFT":
                case "TAIL": return true;
                default: return false;
            }
        }

        public Card GetCardFromIndex(int index)
        {
            int pageIndex = 0;
            int entryIndex = 0;

            for (int i = 0; i < index; i++)
            {
                if (Pages[pageIndex].PageEntries.Count == entryIndex)
                {
                    pageIndex++;

                    if (Pages.Count == pageIndex)
                    {
                        pageIndex = 0;
                    }

                    entryIndex = 0;
                }
            }

            return GetCardFromID(List.Pages[pageIndex].PageEntries[entryIndex]);
        }

        public Card GetCardFromID(int id)
        {
            foreach (var card in Cards)
                if (card.ID == id) return card;

            return null;
        }

        public Card GetCardFromName(string name)
        {
            foreach (var card in Cards)
                if (card.Name.ToLower() == name.ToLower()) return card;

            return null;
        }

        public Page GetPageFromID(int id)
        {
            foreach (var page in Pages)
                if (page.PageID == id) return page;

            return null;
        }

        public Bitmap GetBitmapFromID(int id)
        {
            foreach (var bitmap in Bitmaps)
                if (bitmap.BitmapID == id) return bitmap.Image;

            return null;
        }

        private int pageIndex, entryIndex;

        private void FindEntry()
        {
            for (pageIndex = 0; pageIndex < Pages.Count; pageIndex++)
            {
                for (entryIndex = 0; entryIndex < Pages[pageIndex].PageEntries.Count; entryIndex++)
                {
                    if (CurrentCard == GetCardFromID(List.Pages[pageIndex].PageEntries[entryIndex]))
                        return;
                }
            }

            // we never found the card we were looking for, so return the first card in the stack
            pageIndex = 0;
            entryIndex = 0;
        }

        public void FirstCard()
        {
            entryIndex = 0;
            pageIndex = 0;

            CurrentCard = GetCardFromID(List.Pages[pageIndex].PageEntries[entryIndex]);
        }

        public void LastCard()
        {
            pageIndex = Pages.Count - 1;
            entryIndex = Pages[pageIndex].PageEntries.Count - 1;

            CurrentCard = GetCardFromID(List.Pages[pageIndex].PageEntries[entryIndex]);
        }

        public void NextCard()
        {
            entryIndex++;

            if (Pages[pageIndex].PageEntries.Count == entryIndex)
            {
                pageIndex++;

                if (Pages.Count == pageIndex)
                {
                    pageIndex = 0;
                }

                entryIndex = 0;
            }

            CurrentCard = GetCardFromID(List.Pages[pageIndex].PageEntries[entryIndex]);
        }

        public void PreviousCard()
        {
            entryIndex--;

            if (entryIndex < 0)
            {
                pageIndex--;

                if (pageIndex < 0)
                {
                    pageIndex = Pages.Count - 1;
                }

                entryIndex = Pages[pageIndex].PageEntries.Count - 1;
            }

            CurrentCard = GetCardFromID(List.Pages[pageIndex].PageEntries[entryIndex]);
        }

        internal void InvokeCompiledMethod(string methodName)
        {
            bool handled = Scripting.CSharpScripting.InvokeCompiledMethod(CompiledScript, methodName, this);

            Console.WriteLine("Stack caught message " + methodName);
            //if (!handled) EscalateMessage(methodName);
        }
    }

    public class List
    {
        public int PageCount { get; set; }

        public int PageEntryTotal { get; set; }

        public int ListID { get; set; }

        public short PageEntrySize { get; set; }

        public Dictionary<int, short> PageEntryCount { get; private set; }

        public Page[] Pages { get; private set; }

        private int[] pageIDs;

        public List(BigEndianBinaryReader reader, int listChunkSize, int listID)
        {
            long nextBlock = reader.Position + listChunkSize - 12;

            PageEntryCount = new Dictionary<int, short>();

            this.ListID = listID;

            reader.ReadInt32(); // filler
            this.PageCount = reader.ReadInt32();
            reader.ReadInt32();
            this.PageEntryTotal = reader.ReadInt32();

            this.PageEntrySize = reader.ReadInt16();
            reader.ReadBytes(10);

            this.PageEntryTotal = reader.ReadInt32();
            reader.ReadInt32();

            pageIDs = new int[PageCount];
            Pages = new Page[PageCount];

            for (int i = 0; i < PageCount; i++)
            {
                pageIDs[i] = reader.ReadInt32();
                short pageEntryCount = reader.ReadInt16();

                PageEntryCount.Add(pageIDs[i], pageEntryCount);
            }

            reader.Position = nextBlock;
        }

        public void AddPage(Page page)
        {
            for (int i = 0; i < pageIDs.Length; i++)
                if (pageIDs[i] == page.PageID)
                    Pages[i] = page;
        }
    }

    public class Page
    {
        public int ListID { get; set; }

        public int PageID { get; set; }

        public List<int> PageEntries { get; set; }

        public Page(BigEndianBinaryReader reader, int pageChunkSize, int pageID, List list)
        {
            long nextBlock = reader.Position + pageChunkSize - 12;

            PageEntries = new List<int>();

            this.PageID = pageID;

            reader.ReadInt32(); // filler
            ListID = reader.ReadInt32();
            reader.ReadInt32(); // something

            short pageEntryCount = list.PageEntryCount[pageID];

            for (int i = 0; i < pageEntryCount; i++)
            {
                PageEntries.Add(reader.ReadInt32());
                reader.ReadBytes(list.PageEntrySize - 4);
            }

            // add this page to the list
            list.AddPage(this);

            reader.Position = nextBlock;
        }
    }
}
