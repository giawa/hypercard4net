using System;
using System.IO;

namespace HyperCard
{
    public class BigEndianBinaryReader : IDisposable
    {
        private BinaryReader reader;

        public long Position
        {
            get
            {
                return reader.BaseStream.Position;
            }
            set
            {
                reader.BaseStream.Seek(value, SeekOrigin.Begin);
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

            return Utilities.FromMacRoman(stringProcessor, i - 1);
        }

        public char[] ReadChars(int count)
        {
            return reader.ReadChars(count);
        }

        public byte[] ReadBytes(int count)
        {
            return reader.ReadBytes(count);
        }

        public byte ReadByte()
        {
            return reader.ReadByte();
        }

        public short ReadInt16()
        {
            var bytes = reader.ReadBytes(2);
            if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
            return BitConverter.ToInt16(bytes, 0);
        }

        public int ReadInt32()
        {
            var bytes = reader.ReadBytes(4);
            if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
            return BitConverter.ToInt32(bytes, 0);
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
