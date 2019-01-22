using System.Collections.Generic;

namespace HyperCard
{
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
}
