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
                if (part.Type == HyperCard.PartType.Button) RenderButton(part, pe.Graphics);
                else RenderField(part, pe.Graphics);
            }

            foreach (var part in card.Parts)
            {
                if (part.Type == HyperCard.PartType.Button) RenderButton(part, pe.Graphics);
                else RenderField(part, pe.Graphics);
            }
        }

        private void RenderField(HyperCard.Part part, Graphics g)
        {
            // check if this part is not visible
            if (((byte)part.Flags & 0x80) == 0x80) return;

            using (Brush blackBrush = new SolidBrush(Color.Black))
            using (System.Drawing.Font buttonFont = new Font("Chicago", part.TextSize - 1, (FontStyle)((int)part.TextStyle & 0x07), GraphicsUnit.Pixel))
            {
                // measure the width of the string to calculate center/right alignment
                var textSize = g.MeasureString(part.Name, buttonFont);
                float textX = part.Rect.Left;// +(part.Rect.Width >> 1);
                float textY = part.Rect.Top + (part.Rect.Height >> 1) - textSize.Height / 2;

                if (part.TextAlign == HyperCard.TextAlign.Center) textX += (part.Rect.Width >> 1) - textSize.Width / 2;
                else if (part.TextAlign == HyperCard.TextAlign.Right) textX += part.Rect.Width - textSize.Width;

                g.DrawString(part.Name, buttonFont, blackBrush, textX, textY);
            }
        }

        private void RenderButton(HyperCard.Part part, Graphics g)
        {
            // check if this part is not visible
            if (((byte)part.Flags & 0x80) == 0x80) return;

            int xoffset = 0, yoffset = 0;

            switch (part.Style)
            {
                case HyperCard.PartStyle.Transparent: break;
                case HyperCard.PartStyle.Rectangle:
                    using (Pen blackPen = new Pen(Color.Black))
                        g.DrawRectangle(blackPen, part.Rect.ToRectangle());
                    break;
                case HyperCard.PartStyle.Opaque:
                    using (Brush whiteBrush = new SolidBrush(Color.White))
                        g.FillRectangle(whiteBrush, part.Rect.ToRectangle());
                    break;
                default:
                    Console.WriteLine("Unsupported button style: " + part.Style.ToString());
                    break;
            }

            int textHeight = 0;

            if (part.ShowName)
            {
                using (Brush blackBrush = new SolidBrush(Color.Black))
                using (System.Drawing.Font buttonFont = new Font("Chicago", part.TextSize - 1, (FontStyle)((int)part.TextStyle & 0x07), GraphicsUnit.Pixel))
                {
                    // measure the width of the string to calculate center/right alignment
                    var textSize = g.MeasureString(part.Name, buttonFont);
                    float textX = part.Rect.Left;
                    float textY = part.Rect.Top + (part.Rect.Height >> 1) - textSize.Height / 2;

                    if (part.TextAlign == HyperCard.TextAlign.Center) textX += (part.Rect.Width >> 1) - textSize.Width / 2;
                    else if (part.TextAlign == HyperCard.TextAlign.Right) textX += part.Rect.Width - textSize.Width;

                    if (part.IconID != 0) textY += 16;
                    textHeight = (int)textSize.Height;

                    g.DrawString(part.Name, buttonFont, blackBrush, textX, textY);
                }
            }

            if (part.IconID != 0)
            {
                // icons use 255, 255, 254 as their transparency key
                ImageAttributes attr = new ImageAttributes();
                attr.SetColorKey(Color.FromArgb(255, 255, 254), Color.FromArgb(255, 255, 254));

                // try to get the resource - returns a sane default
                var icon = (stack.IconResources.ContainsKey(part.IconID) ? stack.IconResources[part.IconID].Bitmap : Icon.PngFromID(part.IconID));

                // calculate the correct position and then draw it
                // TODO:  Need to account for text spacing once text rendering is enabled
                Point p = new Point(part.Rect.Left + ((part.Rect.Width - icon.Width) >> 1), part.Rect.Top + ((part.Rect.Height - icon.Height) >> 1) - textHeight / 2);
                g.DrawImage(icon, new Rectangle(p, icon.Size), 0, 0, icon.Width, icon.Height, GraphicsUnit.Pixel, attr);
            }
        }
    }
}
