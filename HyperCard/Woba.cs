using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;

namespace HyperCard
{
    public class Woba
    {
        public Woba(BigEndianBinaryReader reader, int bitmapChunkSize, int bitmapId)
        {
            BitmapID = bitmapId;

            ProcessWobaHeader(reader);

            if (MaskSize != 0) Mask = ProcessMask(reader.ReadBytes(MaskSize), MaskSize);
            if (ImageSize != 0) Image = ProcessImage(reader.ReadBytes(bitmapChunkSize - 64 - MaskSize), ImageSize);

            //if (Mask != null) Mask.Save(BitmapID + "_mask.png", System.Drawing.Imaging.ImageFormat.Png);
            //if (Image != null) Image.Save(BitmapID + ".png", System.Drawing.Imaging.ImageFormat.Png);
        }

        public int BitmapID { get; private set; }

        public short BitmapTop { get; private set; }
        public short BitmapLeft { get; private set; }
        public short BitmapBottom { get; private set; }
        public short BitmapRight { get; private set; }
        public short MaskBoundTop { get; private set; }
        public short MaskBoundLeft { get; private set; }
        public short MaskBoundBottom { get; private set; }
        public short MaskBoundRight { get; private set; }
        public short ImageBoundTop { get; private set; }
        public short ImageBoundLeft { get; private set; }
        public short ImageBoundBottom { get; private set; }
        public short ImageBoundRight { get; private set; }
        public int MaskSize { get; private set; }
        public int ImageSize { get; private set; }

        public Bitmap Mask { get; private set; }
        public Bitmap Image { get; private set; }

        private int rowlength;

        private void ProcessWobaHeader(BigEndianBinaryReader reader)
        {
            int filler = reader.ReadInt32();        // always zero
            reader.ReadBytes(8);                    // unknown
            BitmapTop = reader.ReadInt16();         // top of the card rectangle
            BitmapLeft = reader.ReadInt16();        // left of the card rectangle
            BitmapBottom = reader.ReadInt16();      // bottom of the card rectangle
            BitmapRight = reader.ReadInt16();       // right of the card rectangle
            MaskBoundTop = reader.ReadInt16();      // top of the mask bounding rectangle
            MaskBoundLeft = reader.ReadInt16();     // left of the mask bounding rectangle
            MaskBoundBottom = reader.ReadInt16();   // bottom of the mask bounding rectangle
            MaskBoundRight = reader.ReadInt16();    // right of the mask bounding rectangle
            ImageBoundTop = reader.ReadInt16();     // top of the image bounding rectangle
            ImageBoundLeft = reader.ReadInt16();    // left of the image bounding rectangle
            ImageBoundBottom = reader.ReadInt16();  // bottom of the image bounding rectangle
            ImageBoundRight = reader.ReadInt16();   // right of the image bounding rectangle
            reader.ReadBytes(8);                    // unknown
            MaskSize = reader.ReadInt32();          // size of the mask data
            ImageSize = reader.ReadInt32();         // size of the image data
        }

        private Bitmap ProcessMask(byte[] data, int size)
        {
            int bx8 = MaskBoundLeft & (~0x1F);
            int y = MaskBoundTop;
            int rowwidth8 = (((MaskBoundRight & 0x1F) != 0) ? ((MaskBoundRight | 0x1F) + 1) : MaskBoundRight) - (MaskBoundLeft & (~0x1F));
            int height = MaskBoundBottom - MaskBoundTop;

            byte[] maskData = ProcessWoba(data, size, bx8, y, rowwidth8, height);
            return ConvertToBitmap(maskData);
        }

        private Bitmap ProcessImage(byte[] data, int size)
        {
            int bx8 = ImageBoundLeft & (~0x1F);
            int y = ImageBoundTop;
            int rowwidth8 = (((ImageBoundRight & 0x1F) != 0) ? ((ImageBoundRight | 0x1F) + 1) : ImageBoundRight) - (ImageBoundLeft & (~0x1F));
            int height = ImageBoundBottom - ImageBoundTop;

            byte[] imageData = ProcessWoba(data, size, bx8, y, rowwidth8, height);
            return ConvertToBitmap(imageData);
        }

        private byte[] ProcessWoba(byte[] data, int size, int bx8, int y, int rowwidth8, int height)
        {
            byte[] patternBuffer = new byte[8];

            int bx = bx8 / 8;
            int rowwidth = rowwidth8 / 8;
            int dx = 0, dy = 0, x = 0;
            int repeat = 1;

            patternBuffer[0] = patternBuffer[2] = patternBuffer[4] = patternBuffer[6] = 170;
            patternBuffer[1] = patternBuffer[3] = patternBuffer[5] = patternBuffer[7] = 85;

            byte[] buffer1 = new byte[rowwidth];
            byte[] buffer2 = new byte[rowwidth];
            srcBlock = new uint[rowwidth / 4];
            destBlock = new uint[rowwidth / 4];
            destBlock8 = new ulong[rowwidth / 8];
            srcBlock8 = new ulong[rowwidth / 8];
            byte[] operandData = new byte[256];
            int j = 0, i = 0;
            int nd = 0, nz = 0;

            int width = BitmapRight - BitmapLeft;
            rowlength = (((width * 1) / 8) + ((((width * 1) % 8) != 0) ? 1 : 0));   // assume a depth of 1
            byte[] pixelData = new byte[rowlength * (BitmapBottom - BitmapTop)];

            if ((rowwidth & 0x07) == 0) xornstr = xornstr8;
            else if ((rowwidth & 0x03) == 0) xornstr = xornstr4;
            else xornstr = xornstr1;

            while (j < size)
            {
                byte opcode = data[i];
                byte operand = 0;

                i++; j++;
                if ((opcode & 0x80) == 0)
                {
                    /* zeros followed by data */
                    nd = opcode >> 4;
                    nz = opcode & 15;
                    /* std::cout<<"nd: "<<nd<<endl<<"nz: "<<nz<<endl; */
                    if (nd != 0)
                    {
                        //memcpy(operandata, woba + i, nd);
                        Array.Copy(data, i, operandData, 0, nd);
                        i += nd; j += nd;
                    }
                    while (repeat != 0)
                    {
                        for (int k = nz; k > 0; k--)
                        {
                            buffer1[x] = 0;
                            x++;
                        }
                        //memcpy(buffer1 + x, operandata, nd);
                        Array.Copy(operandData, 0, buffer1, x, nd);
                        x += nd;
                        repeat--;
                    }
                    repeat = 1;
                }
                else if ((opcode & 0xE0) == 0xC0)
                {
                    /* opcode & 1F * 8 bytes of data */
                    nd = (opcode & 0x1F) * 8;
                    /* std::cout<<"nd: "<<nd<<endl; */
                    if (nd != 0)
                    {
                        //memcpy(operandata, woba + i, nd);
                        Array.Copy(data, i, operandData, 0, nd);
                        i += nd; j += nd;
                    }
                    while (repeat != 0)
                    {
                        //memcpy(buffer1 + x, operandata, nd);
                        Array.Copy(operandData, 0, buffer1, x, nd);
                        x += nd;
                        repeat--;
                    }
                    repeat = 1;
                }
                else if ((opcode & 0xE0) == 0xE0)
                {
                    /* opcode & 1F * 16 bytes of zero */
                    nz = (opcode & 0x1F) * 16;
                    /* std::cout<<"nz: "<<nz<<endl; */
                    while (repeat != 0)
                    {
                        for (int k = nz; k > 0; k--)
                        {
                            if (x < buffer1.Length) buffer1[x] = 0;
                            x++;
                        }
                        repeat--;
                    }
                    repeat = 1;
                }

                if ((opcode & 0xE0) == 0xA0)
                {
                    /* repeat opcode */
                    repeat = (opcode & 0x1F);
                }
                else
                {
                    switch (opcode)
                    {
                        case 0x80: /* uncompressed data */
                            x = 0;
                            while (repeat != 0)
                            {
                                //p.maskmemcopyin(data[i], bx8, y, rowwidth);
                                for (int k = 0; k < rowwidth; k++)
                                {
                                    byte d = data[i + k];
                                    pixelData[bx + y * rowlength + k] = data[i + k];
                                }
                                y++;
                                repeat--;
                            }
                            repeat = 1;
                            i += rowwidth; j += rowwidth;
                            break;
                        case 0x81: /* white row */
                            x = 0;
                            while (repeat != 0)
                            {
                                //p.maskmemfill(0, bx8, y, rowwidth);
                                Array.Clear(pixelData, bx + y * rowlength, rowwidth);
                                y++;
                                repeat--;
                            }
                            repeat = 1;
                            break;
                        case 0x82: /* black row */
                            x = 0;
                            while (repeat != 0)
                            {
                                //p.maskmemfill(0xFF, bx8, y, rowwidth);
                                for (int k = 0; k < rowwidth; k++) pixelData[(bx + y * rowlength) + k] = 0xff;
                                y++;
                                repeat--;
                            }
                            repeat = 1;
                            break;
                        case 0x83: /* pattern */
                            operand = data[i];
                            /* std::cout<<"patt: "<<__hex(operand)<<endl; */
                            i++; j++;
                            x = 0;
                            while (repeat != 0)
                            {
                                patternBuffer[y & 7] = operand;
                                //p.maskmemfill(operand, bx8, y, rowwidth);
                                for (int k = 0; k < rowwidth; k++)
                                {
                                    pixelData[(bx + y * rowlength) + k] = operand;
                                }
                                y++;
                                repeat--;
                            }
                            repeat = 1;
                            break;
                        case 0x84: /* last pattern */
                            x = 0;
                            while (repeat != 0)
                            {
                                operand = patternBuffer[y & 7];
                                /* std::cout<<"patt: "<<__hex(operand)<<endl; */
                                //p.maskmemfill(operand, bx8, y, rowwidth);
                                for (int k = 0; k < rowwidth; k++)
                                {
                                    pixelData[(bx + y * rowlength) + k] = operand;
                                }
                                y++;
                                repeat--;
                            }
                            repeat = 1;
                            break;
                        case 0x85: /* previous row */
                            x = 0;
                            while (repeat != 0)
                            {
                                //p.maskcopyrow(y, y - 1);
                                for (int k = 0; k < rowwidth; k++) pixelData[(bx + y * rowlength) + k] = pixelData[(bx + (y - 1) * rowlength) + k];
                                y++;
                                repeat--;
                            }
                            repeat = 1;
                            break;
                        case 0x86: /* two rows back */
                            x = 0;
                            while (repeat != 0)
                            {
                                //p.maskcopyrow(y, y - 2);
                                for (int k = 0; k < rowwidth; k++) pixelData[(bx + y * rowlength) + k] = pixelData[(bx + (y - 2) * rowlength) + k];
                                y++;
                                repeat--;
                            }
                            repeat = 1;
                            break;
                        case 0x87: /* three rows back */
                            x = 0;
                            while (repeat != 0)
                            {
                                //p.maskcopyrow(y, y - 3);
                                for (int k = 0; k < rowwidth; k++) pixelData[(bx + y * rowlength) + k] = pixelData[(bx + (y - 3) * rowlength) + k];
                                y++;
                                repeat--;
                            }
                            repeat = 1;
                            break;
                        case 0x88:
                            dx = 16; dy = 0;
                            break;
                        case 0x89:
                            dx = 0; dy = 0;
                            break;
                        case 0x8A:
                            dx = 0; dy = 1;
                            break;
                        case 0x8B:
                            dx = 0; dy = 2;
                            break;
                        case 0x8C:
                            dx = 1; dy = 0;
                            break;
                        case 0x8D:
                            dx = 1; dy = 1;
                            break;
                        case 0x8E:
                            dx = 2; dy = 2;
                            break;
                        case 0x8F:
                            dx = 8; dy = 0;
                            break;
                        default: /* it's not a repeat or a whole row */
                            if (x >= rowwidth)
                            {
                                x = 0;
                                if (dx != 0)
                                {
                                    //memcpy(buffer2, buffer1, rowwidth);
                                    Array.Copy(buffer1, buffer2, rowwidth);
                                    for (int k = rowwidth8 / dx; k > 0; k--)
                                    {
                                        shiftnstr(buffer2, rowwidth, dx);
                                        xornstr(buffer1, buffer2, rowwidth);
                                    }
                                }
                                if (dy != 0)
                                {
                                    //p.maskmemcopyout(buffer2, bx8, y - dy, rowwidth);
                                    memcopyout(pixelData, buffer2, bx8, y - dy, rowwidth);
                                    xornstr(buffer1, buffer2, rowwidth);
                                }
                                //p.maskmemcopyin(buffer1, bx8, y, rowwidth);
                                memcopyin(pixelData, buffer1, bx8, y, rowwidth);
                                y++;
                            }
                            break;
                    }
                }
            }

            return pixelData;
        }

        private Bitmap ConvertToBitmap(byte[] pixelData)
        {
            int[] expandedData = new int[pixelData.Length * 8];

            for (int i = 0; i < pixelData.Length; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (((pixelData[i] >> j) & 0x01) == 0x00)
                    {
                        int pos = i * 8 + (7 - j);

                        expandedData[pos] = unchecked((int)0xffffffff);
                    }
                }
            }

            Bitmap bitmap = new Bitmap(BitmapRight - BitmapLeft, BitmapBottom - BitmapTop);
            var bits = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            Marshal.Copy(expandedData, 0, bits.Scan0, expandedData.Length);
            bitmap.UnlockBits(bits);

            return bitmap;
        }

        private void memcopyin(byte[] bitmapData, byte[] src, int x, int y, int count)
        {
            if (x / 8 + rowlength * y + count >= bitmapData.Length) return;
            Array.Copy(src, 0, bitmapData, x / 8 + rowlength * y, count);
        }

        private void memcopyout(byte[] bitmapData, byte[] dest, int x, int y, int count)
        {
            if (x / 8 + rowlength * y + count >= bitmapData.Length) return;
            if (count >= dest.Length) count = dest.Length;
            Array.Copy(bitmapData, x / 8 + rowlength * y, dest, 0, count);
        }

        private uint[] destBlock;
        private uint[] srcBlock;
        private ulong[] destBlock8;
        private ulong[] srcBlock8;

        private delegate void xornstrFunction(byte[] dest, byte[] src, int n);
        private xornstrFunction xornstr;

        private void xornstr8(byte[] dest, byte[] src, int n)
        {
            Buffer.BlockCopy(dest, 0, destBlock8, 0, n);
            Buffer.BlockCopy(src, 0, srcBlock8, 0, n);

            for (int i = 0; i < n / 8; i++)
            {
                destBlock8[i] ^= srcBlock8[i];
            }

            Buffer.BlockCopy(destBlock8, 0, dest, 0, n);
        }

        private void xornstr4(byte[] dest, byte[] src, int n)
        {
            Buffer.BlockCopy(dest, 0, destBlock, 0, n);
            Buffer.BlockCopy(src, 0, srcBlock, 0, n);

            for (int i = 0; i < n / 4; i++)
            {
                destBlock[i] ^= srcBlock[i];
            }

            Buffer.BlockCopy(destBlock, 0, dest, 0, n);
        }

        private void xornstr1(byte[] dest, byte[] src, int n)
        {
            for (int i = 0; i < n; i++)
            {
                dest[i] ^= src[i];
            }
        }

        private void shiftnstr(byte[] s, int n, int sh)
        {
            int x = 0;

            for (int i = 0; i < n; i++)
            {
                x += (s[i] << 16) >> sh;
                s[i] = (byte)(x >> 16);
                x = (x & 0x0000ffff) << 8;
            }
        }
    }
}
