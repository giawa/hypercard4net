﻿using System;
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

        private HyperCard.Stack stack;
        private HyperCard.Card card;
        private HyperCard.Background background;

        private HyperCard.Woba backgroundBitmap;
        private HyperCard.Woba cardBitmap;

        public void SetCard(HyperCard.Stack stack, HyperCard.Card card)
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
            if (this.Image == null || this.Image.Width != Width || this.Image.Height != Height)
                this.Image = new Bitmap(Width, Height);

            Graphics g = Graphics.FromImage(this.Image);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;

            if (stack == null || card == null) return;

            if (backgroundBitmap != null && backgroundBitmap.Image != null) g.DrawImage(backgroundBitmap.Image, new Point(0, 0));
            if (cardBitmap != null && cardBitmap.Image != null)
            {
                ImageAttributes attr = new ImageAttributes();
                attr.SetColorKey(Color.White, Color.White);

                g.DrawImage(cardBitmap.Image, this.ClientRectangle, 0, 0, cardBitmap.Image.Width, cardBitmap.Image.Height, GraphicsUnit.Pixel, attr);
            }

            foreach (var part in background.Parts)
            {
                if (part.Type == HyperCard.PartType.Button) RenderButton(part, g);
                else RenderField(part, g);
            }

            foreach (var part in card.Parts)
            {
                if (part.Type == HyperCard.PartType.Button) RenderButton(part, g);
                else RenderField(part, g);
            }

            base.OnPaint(pe);
        }

        private void RenderField(HyperCard.Part part, Graphics g)
        {
            // check if this part is not visible
            if (((byte)part.Flags & 0x80) == 0x80) return;
            if (string.IsNullOrWhiteSpace(part.Contents)) return;

            string fontFamily = "Arial";

            using (Brush blackBrush = new SolidBrush(Color.Black))
            using (System.Drawing.Font fieldFont = MacFont.GetFont(fontFamily, part.TextSize, (FontStyle)((int)part.TextStyle & 0x07)))
            {
                if (part.Lines != null)
                {
                    for (int i = 0; i < part.Lines.Length; i++)
                    {
                        if (string.IsNullOrWhiteSpace(part.Lines[i])) continue;
                        RenderLine(blackBrush, fieldFont, part, part.Lines[i], part.Rect.Top + part.TextHeight * i, g);
                    }
                }
                else
                {
                    RenderLine(blackBrush, fieldFont, part, part.Contents, part.Rect.Top, g);
                }
            }
        }

        private void RenderLine(Brush fontBrush, System.Drawing.Font font, HyperCard.Part part, string s, int y, Graphics g)
        {
            // measure the width of the string to calculate center/right alignment
            var textSize = g.MeasureString(s, font);
            float textX = part.Rect.Left;
            float textY = y;

            if (part.TextAlign == HyperCard.TextAlign.Center) textX += (part.Rect.Width >> 1) - textSize.Width / 2;
            else if (part.TextAlign == HyperCard.TextAlign.Right) textX += part.Rect.Width - textSize.Width;

            g.DrawString(s, font, fontBrush, textX, textY);
        }

        private Dictionary<HyperCard.Part, Bitmap> cachedParts = new Dictionary<HyperCard.Part, Bitmap>();

        private void RenderButton(HyperCard.Part part, Graphics cardBitmap)
        {
            // check if this part is not visible
            if (((byte)part.Flags & 0x80) == 0x80) return;

            if (!cachedParts.ContainsKey(part) || part.Dirty)
            {
                // make sure to clean up the old bitmap
                if (cachedParts.ContainsKey(part))
                {
                    cachedParts[part].Dispose();
                    cachedParts.Remove(part);
                }

                Bitmap cachedPart = new Bitmap(part.Rect.Width, part.Rect.Height);
                Graphics g = Graphics.FromImage(cachedPart);
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
                cachedParts.Add(part, cachedPart);

                if (part.Highlight && part.IconID == 0)
                {
                    cardBitmap.Flush();
                    g.DrawImage(this.Image, new Rectangle(0, 0, part.Rect.Width, part.Rect.Height), new Rectangle(part.Rect.Left, part.Rect.Top, part.Rect.Width, part.Rect.Height), GraphicsUnit.Pixel);
                }

                // draw the border around this buton
                switch (part.Style)
                {
                    case HyperCard.PartStyle.Transparent: break;
                    case HyperCard.PartStyle.Rectangle:
                        using (Pen blackPen = new Pen(Color.Black))
                            g.DrawRectangle(blackPen, new Rectangle(0, 0, part.Rect.Width - 1, part.Rect.Height - 1));
                        break;
                    case HyperCard.PartStyle.Opaque:
                        using (Brush whiteBrush = new SolidBrush(Color.White))
                            g.FillRectangle(whiteBrush, new Rectangle(0, 0, part.Rect.Width - 1, part.Rect.Height - 1));
                        break;
                    default:
                        Console.WriteLine("Unsupported button style: " + part.Style.ToString());
                        break;
                }
                int textHeight = 0;

                // draw the text in this button
                if (part.ShowName)
                {
                    using (Brush blackBrush = new SolidBrush(Color.Black))
                    using (System.Drawing.Font buttonFont = MacFont.GetFont("Arial", part.TextSize, (FontStyle)((int)part.TextStyle & 0x07)))
                    using (System.Drawing.Font iconFont = MacFont.GetFont("Arial", 10, (FontStyle)((int)part.TextStyle & 0x07)))
                    {
                        var font = (part.IconID != 0 ? iconFont : buttonFont);

                        // measure the width of the string to calculate center/right alignment
                        var textSize = g.MeasureString(part.Name, font);
                        float textX = 0;
                        float textY = (part.Rect.Height >> 1) - textSize.Height / 2;

                        if (part.TextAlign == HyperCard.TextAlign.Center) textX += (part.Rect.Width >> 1) - textSize.Width / 2;
                        else if (part.TextAlign == HyperCard.TextAlign.Right) textX += part.Rect.Width - textSize.Width;

                        if (part.IconID != 0) textY += 18;
                        textHeight = (int)textSize.Height;

                        if (part.IconID != 0)
                        {
                            using (Brush whiteBrush = new SolidBrush(Color.White))
                                g.FillRectangle(whiteBrush, new Rectangle((int)textX + 2, (int)textY + 1, (int)textSize.Width, (int)textSize.Height));
                        }
                        g.DrawString(part.Name, font, blackBrush, textX, textY);
                    }
                }

                // draw the icon in this button
                if (part.IconID != 0)
                {
                    // icons use 255, 255, 254 as their transparency key
                    ImageAttributes attr = new ImageAttributes();
                    attr.SetColorKey(Color.FromArgb(255, 255, 254), Color.FromArgb(255, 255, 254));

                    // try to get the resource - returns a sane default
                    var icon = (stack.IconResources.ContainsKey(part.IconID) ? stack.IconResources[part.IconID].Bitmap : Icon.PngFromID(part.IconID));

                    // calculate the correct position and then draw it
                    Point p = new Point(((part.Rect.Width - icon.Width) >> 1), ((part.Rect.Height - icon.Height) >> 1) - textHeight / 2);
                    g.DrawImage(icon, new Rectangle(p, icon.Size), 0, 0, icon.Width, icon.Height, GraphicsUnit.Pixel, attr);
                }

                part.Dirty = false;
            }

            if (part.Highlight) InvertColors(cardBitmap, new Point(part.Rect.Left, part.Rect.Top), cachedParts[part]);
            else cardBitmap.DrawImage(cachedParts[part], new Point(part.Rect.Left, part.Rect.Top));
        }

        private void InvertColors(Graphics g, Point location, Bitmap bitmap)
        {
            // Note:  This is pretty slow - would it be faster to LockBits and do this manually?
            float[][] colorMatrixElements = { 
                new float[] {-1,  0,  0,  0,  0},        // red scaling factor of -1
                new float[] { 0, -1,  0,  0,  0},        // green scaling factor of -1
                new float[] { 0,  0, -1,  0,  0},        // blue scaling factor of -1
                new float[] { 1,  1,  1,  1,  0},        // alpha scaling factor of 1
                new float[] { 0,  0,  0,  0,  1}};

            ColorMatrix colorMatrix = new ColorMatrix(colorMatrixElements);

            ImageAttributes imageAttributes = new ImageAttributes();
            imageAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

            g.DrawImage(bitmap, new Rectangle(location.X, location.Y, bitmap.Width, bitmap.Height), 0, 0, bitmap.Width, bitmap.Height, GraphicsUnit.Pixel, imageAttributes);
        }

        private HyperCard.Part selectedPart = null;
        private HyperCard.Part clickedPart = null;
        private bool mouseDown = false;

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            HyperCard.Part part = SelectedPart(e.Location);
            bool invalidate = false;

            if (selectedPart != part)
            {
                // OnMouseLeave
                if (selectedPart != null)
                {
                    if (mouseDown && selectedPart.AutoHighlight)
                    {
                        selectedPart.Highlight = false;
                        invalidate = true;
                    }
                }

                // OnMouseEnter
                if (part != null)
                {
                    
                }

                // Rehighlight a previously clicked part
                if (clickedPart == part && part != null)
                {
                    if (mouseDown && part.AutoHighlight)
                    {
                        part.Highlight = true;
                        invalidate = true;
                    }
                }
            }

            if (invalidate) Refresh();
            selectedPart = part;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (selectedPart == null) return;

            if (selectedPart.Type == HyperCard.PartType.Button && selectedPart.AutoHighlight)
            {
                clickedPart = selectedPart;
                selectedPart.Highlight = true;
                Refresh();
            }

            mouseDown = true;
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            clickedPart = null;
            mouseDown = false;

            if (selectedPart == null) return;

            if (selectedPart.Type == HyperCard.PartType.Button && selectedPart.AutoHighlight)
            {
                selectedPart.Highlight = false;
                Refresh();
            }
        }

        private HyperCard.Part SelectedPart(Point p)
        {
            for (int i = card.Parts.Count - 1; i >= 0; i--)
            {
                if (Contains(card.Parts[i], p)) return card.Parts[i];
            }

            for (int i = background.Parts.Count - 1; i >= 0; i--)
            {
                if (Contains(background.Parts[i], p)) return background.Parts[i];
            }

            return null;
        }

        private bool Contains(HyperCard.Part part, Point p)
        {
            return part.Rect.Left <= p.X && part.Rect.Right >= p.X && part.Rect.Top <= p.Y && part.Rect.Bottom >= p.Y;
        }
    }
}
