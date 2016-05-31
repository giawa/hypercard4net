using System;
using System.Drawing;

namespace Player
{
    public class Icon
    {
        public static Bitmap PngFromID(short id)
        {
            switch (id)
            {
                // These are all of the icons on a page of the Home stack for now - I'll eventually have to lookup all 201 icons
                case 2980: return Properties.Resources.Address;
                case 3430: return Properties.Resources.Address_Card_A;
                case 28810: return Properties.Resources.LEFT_tri;
                case 28811: return Properties.Resources.right_tri;
                case 17214: return Properties.Resources.Readymade_Buttons;
                case 21711: return Properties.Resources.Readymade_Fields;
                case 16735: return Properties.Resources.Beanie_Copter;
                case 29589: return Properties.Resources.Background_Art;
                case 24753: return Properties.Resources.Stack_Templates;
                case 31885: return Properties.Resources.HT_Reference;
                //case 28867: return Properties.Resources.HT_Reference;
                //case 30504: return Properties.Resources.HT_Reference;
                //case 11316: return Properties.Resources.HT_Reference;
                case 28023: return Properties.Resources.Large_Home_Base;
                //case 28226: return Properties.Resources.HT_Reference;
            }

            Console.WriteLine("icon with id {0} was not found", id);
            return Properties.Resources.Icon_Substitute;
        }
    }
}
