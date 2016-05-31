using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace Player
{
    public class CardRenderer : PictureBox
    {
        public CardRenderer()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw | ControlStyles.Selectable |
                     ControlStyles.SupportsTransparentBackColor | ControlStyles.UserMouse |
                     ControlStyles.UserPaint, true);
        }

        private HyperCard.StackReader stack;
        private HyperCard.Card card;
        private HyperCard.Background background;

        private HyperCard.Woba backgroundBitmap;
        private HyperCard.Woba cardBitmap;

        public void SetCard(HyperCard.StackReader stack, HyperCard.Card card)
        {
            this.stack = stack;
            this.card = card;

            this.cardBitmap = null;
            this.backgroundBitmap = null;

            foreach (var bkgnd in stack.Backgrounds)
            {
                if (bkgnd.BackgroundID == card.BackgroundID)
                {
                    background = bkgnd;

                    foreach (var bmp in stack.Bitmaps)
                        if (bmp.BitmapID == background.BitmapID)
                            backgroundBitmap = bmp;
                }
            }

            foreach (var bmp in stack.Bitmaps)
                if (bmp.BitmapID == card.BitmapID)
                    cardBitmap = bmp;

            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);

            if (stack == null || card == null) return;

            if (backgroundBitmap != null && backgroundBitmap.Image != null) pe.Graphics.DrawImage(backgroundBitmap.Image, new Point(0, 0));
            if (cardBitmap != null && cardBitmap.Image != null)
            {
                ImageAttributes attr = new ImageAttributes();
                attr.SetColorKey(Color.White, Color.White);

                pe.Graphics.DrawImage(cardBitmap.Image, this.ClientRectangle, 0, 0, cardBitmap.Image.Width, cardBitmap.Image.Height, GraphicsUnit.Pixel, attr);
            }

            foreach (var part in background.Parts)
            {
                Color c = (part.Type == HyperCard.PartType.Button ? Color.DodgerBlue : Color.Orange);

                using (Brush brush = new SolidBrush(Color.FromArgb(100, c)))
                {
                    pe.Graphics.FillRectangle(brush, part.Rect.Left, part.Rect.Top, part.Rect.Width, part.Rect.Height);

                    if (part.Type == HyperCard.PartType.Button)// && part.Highlighted)
                    {
                    }
                }
            }

            foreach (var part in card.Parts)
            {
                Color c = (part.Type == HyperCard.PartType.Button ? Color.DodgerBlue : Color.Orange);

                using (Brush brush = new SolidBrush(Color.FromArgb(100, c)))
                {
                    pe.Graphics.FillRectangle(brush, part.Rect.Left, part.Rect.Top, part.Rect.Width, part.Rect.Height);

                    if (part.Type == HyperCard.PartType.Button)// && part.Highlighted)
                    {
                    }
                }
            }
        }
    }
}
