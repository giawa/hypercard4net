﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace Player
{
    public class CardRenderer : PictureBox, HyperCard.IStackRenderer
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

        public HyperCard.Stack Stack
        {
            get { return stack; }
        }

        public void SetStack(HyperCard.Stack stack)
        {
            if (this.Parent is Window)
            {
                this.Parent.ClientSize = new Size(stack.Width, stack.Height + 24);
                this.Parent.Text = "HyperCard .NET : " + stack.Name;
            }

            SetCard(stack, stack.CurrentCard);
        }

        public void SetCard(HyperCard.Stack stack, HyperCard.Card card)
        {
            stack.Renderer = this;
            if (this.stack == stack && this.card == card) return;

            cachedParts.Clear();

            this.stack = stack;
            this.card = card;

            this.cardBitmap = null;
            this.backgroundBitmap = null;

            foreach (var bkgnd in stack.Backgrounds)
            {
                if (bkgnd.ID == card.BackgroundID)
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
            g.Clear(Color.White);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;

            if (stack == null || card == null) return;
            if (card != stack.CurrentCard) SetCard(stack, stack.CurrentCard);

            if (backgroundBitmap != null && backgroundBitmap.Image != null) g.DrawImage(backgroundBitmap.Image, new Point(0, 0));

            foreach (var part in background.Parts)
            {
                if (part.Type == HyperCard.PartType.Button) RenderButton(part, g);
                else RenderField(part, g);
            }

            if (cardBitmap != null && cardBitmap.Image != null)
            {
                ImageAttributes attr = new ImageAttributes();
                if (cardBitmap.Mask == null) attr.SetColorKey(Color.White, Color.White);

                g.DrawImage(cardBitmap.Image, this.ClientRectangle, 0, 0, cardBitmap.Image.Width, cardBitmap.Image.Height, GraphicsUnit.Pixel, attr);
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

            switch (part.Style)
            {
                case HyperCard.PartStyle.Opaque:
                    using (Brush whiteBrush = new SolidBrush(Color.White))
                        g.FillRectangle(whiteBrush, part.Rect.ToRectangle());
                    break;
                case HyperCard.PartStyle.Rectangle:
                    using (Pen blackPen = new Pen(Color.Black))
                        g.DrawRectangle(blackPen, part.Rect.ToRectangle());
                    break;
                case HyperCard.PartStyle.Shadow:
                    using (Pen blackPen = new Pen(Color.Black))
                        g.DrawRectangle(blackPen, new Rectangle(part.Rect.Left, part.Rect.Top, part.Rect.Width - 2, part.Rect.Height - 2));
                    break;
                case HyperCard.PartStyle.Scrolling:
                    using (Pen blackPen = new Pen(Color.Black))
                        g.DrawRectangle(blackPen, part.Rect.ToRectangle());
                    break;
            }

            string fontFamily = "Arial";

            string[] lines = (stack.CurrentCard.BackgroundOverrides.ContainsKey(part.ID) ? stack.CurrentCard.BackgroundOverrides[part.ID].Text.Split(new char[] { '\r' }) : part.Lines);
            if (lines == null && string.IsNullOrWhiteSpace(part.Contents.Text)) return;

            using (Brush blackBrush = new SolidBrush(((int)part.TextStyle & 2048) == 2048 ? Color.Gray : Color.Black))
            using (Font fieldFont = MacFont.GetFont(fontFamily, part.TextSize, (FontStyle)(((int)part.TextStyle >> 8) & 7)))
            {
                if (lines != null)
                {
                    for (int i = 0, y = 0; i < lines.Length; i++)
                    {
                        if (string.IsNullOrWhiteSpace(lines[i]))
                        {
                            y++;
                            continue;
                        }
                        y += RenderTextLine(blackBrush, fieldFont, part, lines[i], part.Rect.Top + part.TextHeight * y, g);

                        if (part.TextHeight * (y + 1) > part.Rect.Height) break;
                    }
                }
                else
                {
                    RenderTextLine(blackBrush, fieldFont, part, part.Contents.Text, part.Rect.Top, g);
                }
            }
        }

        private int RenderTextLine(Brush fontBrush, Font font, HyperCard.Part part, string s, int y, Graphics g)
        {
            int lines = 0, i = 0;
            int margin = part.WideMargins ? 5 : 1;
            int lastSpace = -1;

            string remaining = s;

            while (remaining.Length > 0)
            {
                SizeF textSize;

                for (i = 0; i < remaining.Length; i++)
                {
                    if (remaining[i] == ' ') lastSpace = i;

                    textSize = g.MeasureString(remaining.Substring(0, i), font);

                    if (textSize.Width + 2 * margin > part.Rect.Width)
                    {
                        i = (lastSpace == -1 ? i - 1 : lastSpace + 1);
                        lastSpace = -1;
                        break;
                    }
                }

                if (i == 0) return lines;   // we couldn't fit any more

                textSize = g.MeasureString(remaining.Substring(0, i), font);

                float textX = part.Rect.Left;// +margin;
                float textY = y + margin;

                if (part.TextAlign == HyperCard.TextAlign.Center) textX += (part.Rect.Width >> 1) - textSize.Width / 2;
                else if (part.TextAlign == HyperCard.TextAlign.Right) textX += part.Rect.Width - textSize.Width - margin;
                else textX += margin;

                if (part.TextHeight * (lines + 1) > part.Rect.Height) return lines;
                g.DrawString(remaining.Substring(0, i), font, fontBrush, textX, textY + part.TextHeight * lines);
                lines++;

                if (i == remaining.Length) break;
                else remaining = remaining.Substring(i);
            }

            return lines;
        }

        private Dictionary<HyperCard.Part, Bitmap> cachedParts = new Dictionary<HyperCard.Part, Bitmap>();

        private void RenderButton(HyperCard.Part part, Graphics cardBitmap)
        {
            // check if this part is not visible
            if (((byte)part.Flags & 0x80) == 0x80) return;

            bool highlight = (stack.CurrentCard.BackgroundOverrides.ContainsKey(part.ID) ? stack.CurrentCard.BackgroundOverrides[part.ID].Text == "1" : part.Highlight);

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

                if (highlight && part.IconID == 0)
                {
                    cardBitmap.Flush();
                    g.DrawImage(this.Image, new Rectangle(0, 0, part.Rect.Width, part.Rect.Height), new Rectangle(part.Rect.Left, part.Rect.Top, part.Rect.Width, part.Rect.Height), GraphicsUnit.Pixel);
                }

                // draw the border around this buton
                switch (part.Style)
                {
                    case HyperCard.PartStyle.Transparent: break;
                    case HyperCard.PartStyle.Rectangle:
                        using (Brush whiteBrush = new SolidBrush(Color.White))
                            g.FillRectangle(whiteBrush, new Rectangle(0, 0, part.Rect.Width - 1, part.Rect.Height - 1));
                        using (Pen blackPen = new Pen(Color.Black))
                            g.DrawRectangle(blackPen, new Rectangle(0, 0, part.Rect.Width - 1, part.Rect.Height - 1));
                        break;
                    case HyperCard.PartStyle.Opaque:
                        using (Brush whiteBrush = new SolidBrush(Color.White))
                            g.FillRectangle(whiteBrush, new Rectangle(0, 0, part.Rect.Width - 1, part.Rect.Height - 1));
                        break;
                    case HyperCard.PartStyle.CheckBox:
                        using (Pen blackPen = new Pen(Color.Black))
                        {
                            g.DrawRectangle(blackPen, new Rectangle(0, (part.Rect.Height >> 1) - 6, 12, 12));
                            if (highlight) g.DrawRectangle(blackPen, new Rectangle(1, (part.Rect.Height >> 1) - 5, 10, 10));
                        }
                        break;
                    default:
                        Console.WriteLine("Unsupported button style: " + part.Style.ToString());
                        break;
                }
                int textHeight = 0;

                // draw the text in this button
                if (part.ShowName)
                {
                    using (Brush blackBrush = new SolidBrush(((int)part.TextStyle & 2048) == 2048 ? Color.Gray : Color.Black))
                    using (Font buttonFont = MacFont.GetFont("Arial", part.TextSize, (FontStyle)(((int)part.TextStyle >> 8) & 0x07)))
                    using (Font iconFont = MacFont.GetFont("Arial", 10, (FontStyle)(((int)part.TextStyle >> 8) & 0x07)))
                    {
                        var font = (part.IconID != 0 ? iconFont : buttonFont);

                        // measure the width of the string to calculate center/right alignment
                        var textSize = g.MeasureString(part.Name, font);
                        float textX = 0;
                        float textY = (part.Rect.Height >> 1) - textSize.Height / 2;

                        if (part.Style == HyperCard.PartStyle.CheckBox) textX += 16;
                        else if (part.TextAlign == HyperCard.TextAlign.Center) textX += (part.Rect.Width >> 1) - textSize.Width / 2;
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

            if (highlight && part.Style != HyperCard.PartStyle.CheckBox) InvertColors(cardBitmap, new Point(part.Rect.Left, part.Rect.Top), cachedParts[part]);
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

                    try
                    {
                        selectedPart.OnMouseLeave();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }

                // OnMouseEnter
                if (part != null)
                {
                    try
                    {
                        part.OnMouseEnter();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
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

            try
            {
                selectedPart.OnMouseDown();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

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

            try
            {
                selectedPart.OnMouseUp();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

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
                if ((card.Parts[i].Flags & HyperCard.PartFlags.NotVisible) == HyperCard.PartFlags.NotVisible) continue;
                if (Contains(card.Parts[i], p)) return card.Parts[i];
            }

            for (int i = background.Parts.Count - 1; i >= 0; i--)
            {
                if ((background.Parts[i].Flags & HyperCard.PartFlags.NotVisible) == HyperCard.PartFlags.NotVisible) continue;
                if (Contains(background.Parts[i], p)) return background.Parts[i];
            }

            return null;
        }

        private bool Contains(HyperCard.Part part, Point p)
        {
            return part.Rect.Left <= p.X && part.Rect.Right >= p.X && part.Rect.Top <= p.Y && part.Rect.Bottom >= p.Y;
        }

        protected override bool IsInputKey(Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Right:
                case Keys.Left:
                case Keys.Up:
                case Keys.Down:
                    return true;
                case Keys.Shift | Keys.Right:
                case Keys.Shift | Keys.Left:
                case Keys.Shift | Keys.Up:
                case Keys.Shift | Keys.Down:
                    return true;
            }
            return base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.KeyCode == Keys.Left)
            {
                Stack.PreviousCard();
            }
            else if (e.KeyCode == Keys.Right)
            {
                Stack.NextCard();
            }
        }
    }
}
