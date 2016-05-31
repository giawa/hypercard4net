using System;
using System.IO;
using System.Drawing;
using System.Runtime.InteropServices;

namespace HyperCard
{
    public static class Utilities
    {
        [DllImport("gdi32.dll")]
        public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);
        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateSolidBrush(int crColor);
        [DllImport("gdi32.dll")]
        public static extern bool ExtFloodFill(IntPtr hdc, int nXStart, int nYStart, int crColor, uint fuFillType);
        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);
        [DllImport("gdi32.dll")]
        public static extern int GetPixel(IntPtr hdc, int x, int y);
        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleDC(IntPtr hdc);
        [DllImport("gdi32.dll")]
        private static extern bool DeleteDC(IntPtr hdc);
        [DllImport("gdi32.dll")]
        private static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, int dwRop);

        public static void IconToPng(string filename)
        {
            using (BigEndianBinaryReader reader = new BigEndianBinaryReader(filename))
            {
                byte[] pixelData = reader.ReadBytes((int)reader.Length);

                int[] expandedData = new int[pixelData.Length * 8];

                // convert a 1bpp bitmap into a 32bpp bitmap
                for (int i = 0; i < pixelData.Length; i++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        if (((pixelData[i] >> j) & 0x01) == 0x00)
                        {
                            int pos = i * 8 + (7 - j);

                            expandedData[pos] = unchecked((int)0x00ffffff);
                        }
                    }
                }

                // compute how many pixels each side of the icon contains (assume a square NxN icon)
                int side = (int)Math.Sqrt(pixelData.Length * 8);

                // convert our int array into a bitmap that can be used by GDI+ or even saved to a file
                Bitmap bitmap = new Bitmap(side, side);
                var bits = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                Marshal.Copy(expandedData, 0, bits.Scan0, expandedData.Length);
                bitmap.UnlockBits(bits);

                // create a bitmap with one pixel on each side to allow a flood fille
                Bitmap fillBitmap = new Bitmap(side + 2, side + 2);

                // create a graphics object, clear the large bitmap, and then draw our bitmap in the center
                Graphics g = Graphics.FromImage(fillBitmap);
                g.Clear(Color.White);
                g.DrawImage(bitmap, new Point(1, 1));
                g.Flush();

                // this is a pain - C# GDI+ does not have a flood fill, so we get it from GDI instead of rolling once from scratch
                IntPtr hdc1 = g.GetHdc();
                IntPtr hdc2 = CreateCompatibleDC(hdc1);
                SelectObject(hdc2, fillBitmap.GetHbitmap());
                IntPtr gdiBrsh = CreateSolidBrush(ColorTranslator.ToWin32(Color.FromArgb(255, 255, 254)));
                IntPtr previousBrush = SelectObject(hdc2, gdiBrsh);
                int temp = GetPixel(hdc2, 0, 0);
                bool r = ExtFloodFill(hdc2, 0, 0, temp, 1);

                // we had to draw to a compatible DC, so now we have to blit back to our larger birmap
                BitBlt(hdc1, 0, 0, fillBitmap.Width, fillBitmap.Height, hdc2, 0, 0, 0x00CC0020);

                // try to dispose of everything and set it back the way it was
                SelectObject(hdc2, previousBrush);
                DeleteObject(gdiBrsh);
                DeleteDC(hdc2);
                g.ReleaseHdc();
                g.Dispose();

                // now we have a fillBitmap which has an off-white color for transparency, which we will use as a key later
                // so draw that fillBitmap into the correct position in our original bitmap
                g = Graphics.FromImage(bitmap);
                g.DrawImage(fillBitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height), new Rectangle(1, 1, bitmap.Width, bitmap.Height), GraphicsUnit.Pixel);
                g.Dispose();

                // finally write it out to a file - what a mess!
                bitmap.Save(filename.Replace(".ICON", ".png"), System.Drawing.Imaging.ImageFormat.Png);
            }
        }
    }
}
