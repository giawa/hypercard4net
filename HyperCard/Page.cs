using System.Collections.Generic;

namespace HyperCard
{
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
