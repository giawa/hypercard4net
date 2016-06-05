using System;
using System.IO;
using System.Text;

namespace HyperCard
{
    public class BigEndianBinaryReader : IDisposable
    {
        private BinaryReader reader;

        private long position = 0;

        public long Position
        {
            get { return position; }
            set
            {
                position = value;
                reader.BaseStream.Seek(position, SeekOrigin.Begin);
            }
        }

        public long Length
        {
            get { return reader.BaseStream.Length; }
        }

        public BigEndianBinaryReader(string filename)
        {
            reader = new BinaryReader(File.OpenRead(filename));
        }

        private byte[] stringProcessor = new byte[32768];

        public string ReadString()
        {
            int i = 0;

            while ((stringProcessor[i++] = reader.ReadByte()) != 0) ;

            position += i;

            return Utilities.FromMacRoman(stringProcessor, i - 1);
        }

        public char[] ReadChars(int count)
        {
            position += count;
            return reader.ReadChars(count);
        }

        public byte[] ReadBytes(int count)
        {
            position += count;
            return reader.ReadBytes(count);
        }

        public byte ReadByte()
        {
            position++;
            return reader.ReadByte();
        }

        public short ReadInt16()
        {
            ushort value = reader.ReadUInt16();
            ushort swap = (ushort)(((value & 0x00ff) << 8) | ((value & 0xff00) >> 8));
            position += 2;

            return unchecked((short)swap);
        }

        public int ReadInt32()
        {
            uint value = reader.ReadUInt32();
            uint swap = ((value & 0x000000ff) << 24) | ((value & 0x0000ff00) << 8) | ((value & 0x00ff0000) >> 8) | ((value & 0xff000000) >> 24);
            position += 4;

            return unchecked((int)swap);
        }

        public void Dispose()
        {
            if (reader != null)
            {
                reader.Dispose();
                reader = null;
            }
        }
    }
}
