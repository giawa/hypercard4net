using System;
using System.Drawing;

public class Card15991
{
    public class CardButton5
    {
        public static void OnMouseUp(HyperCard.Part button)
        {
            var stack = button.Parent.Stack;

            stack.Renderer.SetStack(new HyperCard.Stack("Home"));
        }
    }

    public class CardButton6
    {
        public static void OnMouseUp(HyperCard.Part button)
        {
            var stack = button.Parent.Stack;

            stack.CurrentCard = stack.GetCardFromName("Stack Overview");
        }
    }
    
    public class CardButton1
    {
        public static void OnMouseUp(HyperCard.Part button)
        {
            var stack = button.Parent.Stack;

            stack.CurrentCard = stack.GetCardFromName("Bar");
        }
    }

    public class CardButton15
    {
        public static void OnMouseUp(HyperCard.Part button)
        {
            var stack = button.Parent.Stack;

            stack.CurrentCard = stack.GetCardFromName("Column");
        }
    }

    public class CardButton16
    {
        public static void OnMouseUp(HyperCard.Part button)
        {
            var stack = button.Parent.Stack;

            stack.CurrentCard = stack.GetCardFromName("Fever");
        }
    }

    public class CardButton17
    {
        public static void OnMouseUp(HyperCard.Part button)
        {
            var stack = button.Parent.Stack;

            stack.CurrentCard = stack.GetCardFromName("Pie");
        }
    }
}

public class Background11532
{
    public class BackgroundButton34
    {
        public static void OnMouseUp(HyperCard.Part button)
        {
            var stack = button.Parent.Stack;

            stack.CurrentCard = stack.GetCardFromName("Stack Overview");
        }
    }

    public class BackgroundButton10
    {
        public static void OnMouseUp(HyperCard.Part button)
        {
            var stack = button.Parent.Stack;

            stack.CurrentCard = stack.GetCardFromName("Bar");
        }
    }

    public class BackgroundButton11
    {
        public static void OnMouseUp(HyperCard.Part button)
        {
            var stack = button.Parent.Stack;

            stack.CurrentCard = stack.GetCardFromName("Column");
        }
    }

    public class BackgroundButton14
    {
        public static void OnMouseUp(HyperCard.Part button)
        {
            var stack = button.Parent.Stack;

            stack.CurrentCard = stack.GetCardFromName("Fever");
        }
    }

    public class BackgroundButton12
    {
        public static void OnMouseUp(HyperCard.Part button)
        {
            var stack = button.Parent.Stack;

            stack.CurrentCard = stack.GetCardFromName("Pie");
        }
    }

    public class BackgroundButton15
    {
        public static void OnMouseUp(HyperCard.Part button)
        {
            button.Parent.Stack.FirstCard();
        }
    }
}

public class Card14091
{
    public static void DrawGraph(HyperCard.Card card)
    {
        double[] data = new double[] { 0.19, 0.39, 0.04, 0.06, 0.06, 0.08, 0.12, 0.02, 0.04 };
        string[] legend = new string[] { "USA", "Japan", "France", "Italy", "Canada", "Spain", "USSR", "Somewhere", "UK" };

        Graphics g = Graphics.FromImage(card.Stack.GetBitmapFromID(card.BitmapID));
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        g.Clear(Color.Transparent);

        HyperCard.Rect chartRect = card.GetBackground().GetPartFromName("Frame").Rect;
        HyperCard.Rect legendRect = card.GetBackground().GetPartFromName("Legend").Rect;

        // adjust chart rect to account for the labels
        int chartLeft = chartRect.Left + 10;
        int chartRight = chartRect.Right - 10;
        int chartTop = chartRect.Top + 25;
        int chartBottom = chartRect.Bottom - 8;
        int chartWidth = chartRight - chartLeft;
        int chartHeight = chartBottom - chartTop;
        int pieRadius = Math.Min(chartWidth, chartHeight) / 2;
        int pieCenterX = chartLeft + chartWidth / 2;
        int pieCenterY = chartTop + chartHeight / 2;

        int legendX = legendRect.Left;
        int legendY = legendRect.Top;

        // add a title to the graph
        using (Font font = new Font("Arial", 9f, FontStyle.Bold))
        using (Brush fontBrush = new SolidBrush(Color.Black))
        {
            var stringSize = g.MeasureString("Acme Sales Analysis", font);
            g.DrawString("Acme Sales Analysis", font, fontBrush, pieCenterX - stringSize.Width / 2, chartTop - 18);
        }

        using (Font fontBold = new Font("Arial", 9f, FontStyle.Bold))
        using (Font font = new Font("Arial", 8f))
        using (Brush fontBrush = new SolidBrush(Color.Black))
        using (Pen blackPen = new Pen(Color.Black))
        {
            // draw the legend
            for (int i = 0; i < data.Length; i++)
            {
                g.DrawString(legend[i], font, fontBrush, legendX + 25, legendY + 25 + 10 * i);
            }

            // fill the pie chart area
            using (Brush whiteBrush = new SolidBrush(Color.White))
                g.FillRectangle(whiteBrush, card.GetBackground().GetPartFromName("Frame").Rect.ToRectangle());

            // add a title to the graph
            var stringSize = g.MeasureString("Acme Sales Analysis", font);
            g.DrawString("Acme Sales Analysis", fontBold, fontBrush, pieCenterX - stringSize.Width / 2, chartTop - 18);

            // draw the pie chart
            double angle = -90;

            for (int i = 0; i < data.Length; i++)
            {
                
                int x = pieCenterX + (int)Math.Round(pieRadius * Math.Sin(angle));
                int y = pieCenterY - (int)Math.Round(pieRadius * Math.Cos(angle));
                double change = 360 * data[i];

                Color rainbow = HSL2RGB((double)i / data.Length, 0.5, 0.5);
                using (Brush brush = new SolidBrush(rainbow))
                    g.FillPie(brush, (float)pieCenterX - pieRadius, (float)pieCenterY - pieRadius, (float)pieRadius * 2, (float)pieRadius * 2, (float)angle, (float)change);
                g.DrawPie(blackPen, (float)pieCenterX - pieRadius, (float)pieCenterY - pieRadius, (float)pieRadius * 2, (float)pieRadius * 2, (float)angle, (float)change);

                angle += change;
            }
        }
    }

    public static void OpenCard(HyperCard.Card card)
    {
        card.GetBackground().GetPartFromName("Legend").Visible = true;

        DrawGraph(card);
    }

    public static void CloseCard(HyperCard.Card card)
    {
        card.GetBackground().GetPartFromName("Legend").Visible = false;
    }

    public static Color HSL2RGB(double h, double sl, double l)
    {
        double v;
        double r, g, b;

        r = l;   // default to gray
        g = l;
        b = l;
        v = (l <= 0.5) ? (l * (1.0 + sl)) : (l + sl - l * sl);
        if (v > 0)
        {
            double m;
            double sv;
            int sextant;
            double fract, vsf, mid1, mid2;

            m = l + l - v;
            sv = (v - m) / v;
            h *= 6.0;
            sextant = (int)h;
            fract = h - sextant;
            vsf = v * sv * fract;
            mid1 = m + vsf;
            mid2 = v - vsf;
            switch (sextant)
            {
                case 0:
                    r = v;
                    g = mid1;
                    b = m;
                    break;
                case 1:
                    r = mid2;
                    g = v;
                    b = m;
                    break;
                case 2:
                    r = m;
                    g = v;
                    b = mid1;
                    break;
                case 3:
                    r = m;
                    g = mid2;
                    b = v;
                    break;
                case 4:
                    r = mid1;
                    g = m;
                    b = v;
                    break;
                case 5:
                    r = v;
                    g = m;
                    b = mid2;
                    break;
            }
        }

        return Color.FromArgb((int)(255 * r), (int)(255 * g), (int)(255 * b));
    }
}
