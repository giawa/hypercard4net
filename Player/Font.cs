using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Runtime.InteropServices;

namespace Player
{
    public static class MacFont
    {
        private static PrivateFontCollection fonts;

        public static void Init()
        {
            fonts = new PrivateFontCollection();

            LoadFont(Properties.Resources.ChicagoFLF);
        }

        private static void LoadFont(byte[] fontData)
        {
            GCHandle handle = GCHandle.Alloc(fontData, GCHandleType.Pinned);

            try
            {
                fonts.AddMemoryFont(handle.AddrOfPinnedObject(), fontData.Length);
            }
            finally
            {
                handle.Free();
            }
        }

        public static Font GetFont(string fontFamily, float size, FontStyle style)
        {
            if (fonts == null) Init();

            switch (fontFamily.ToLower())
            {
                case "chicago": return new Font(fonts.Families[0], size, style, GraphicsUnit.Pixel);
            }

            return new Font(fontFamily, size, style, GraphicsUnit.Pixel);
        }
    }
}
