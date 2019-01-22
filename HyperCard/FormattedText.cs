using System;

namespace HyperCard
{
    public struct FormattedText
    {
        public string Text;
        public short[] Formatting;

        public FormattedText(string text, short[] formatting)
        {
            Text = text;
            Formatting = formatting;
        }
    }
}
