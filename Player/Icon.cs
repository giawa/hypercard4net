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
                //case 2980: return Properties.Resources.Address;
                case 3430: return Properties.Resources.Address_Card_A;
                case 28810: return Properties.Resources.LEFT_tri;
                case 28811: return Properties.Resources.right_tri;
                //case 17214: return Properties.Resources.Readymade_Buttons;
                //case 21711: return Properties.Resources.Readymade_Fields;
                //case 16735: return Properties.Resources.Beanie_Copter;
                //case 29589: return Properties.Resources.Background_Art;
                //case 24753: return Properties.Resources.Stack_Templates;
                //case 31885: return Properties.Resources.HT_Reference;
                //case 28023: return Properties.Resources.Large_Home_Base;

                case 17937: return Properties.Resources.Wrong_Number;
                case 17890: return Properties.Resources.Big_Tone;
                case 17896: return Properties.Resources.Cordy;
                case 17838: return Properties.Resources.Robin_Stylee;
                case 10935: return Properties.Resources.Home_Brew;
                case 8323: return Properties.Resources.Home_Baked;
                case 13745: return Properties.Resources.Home_Alone;

                case 13744: return Properties.Resources.Home_Loan;
                case 2181: return Properties.Resources.TINY_Home;
                case 15993: return Properties.Resources.Home_Made;
                case 22978: return Properties.Resources.Home_Big_2;
                case 27328: return Properties.Resources.Home_Again;
                case 24081: return Properties.Resources.Addresses_;
                case 12722: return Properties.Resources.Appointments;

                case 6491: return Properties.Resources.National_Directory;
                case 6560: return Properties.Resources.HC_Tour;
                case 24694: return Properties.Resources.HC_Help;
                case 21576: return Properties.Resources.Juggler_4;
                case 21575: return Properties.Resources.Juggler_3;
                case 21574: return Properties.Resources.Juggler_2;
                case 21573: return Properties.Resources.Practice_Stack;

                case 18607: return Properties.Resources.Phone_Dialer;
                case 7142: return Properties.Resources.Graph_Maker;
                case 11260: return Properties.Resources.TrainSet;
                case 6544: return Properties.Resources.Puzzle;
                case 17214: return Properties.Resources.Readymade_Buttons;
                case 21711: return Properties.Resources.Readymade_Fields;
                case 16735: return Properties.Resources.Beanie_Copter;

                case 29589: return Properties.Resources.Background_Art;
                case 31885: return Properties.Resources.HT_Reference;
                case 21437: return Properties.Resources.Scanned_Art2;
                case 24753: return Properties.Resources.Stack_Templates;
                case 25309: return Properties.Resources.Art_Bits;
                case 2980: return Properties.Resources.Address;
                case 18223: return Properties.Resources.Info;

                case 18222: return Properties.Resources.Stack_Info;
                case 28022: return Properties.Resources.Small_Home_Base;
                case 28024: return Properties.Resources.Home_Base;
                case 28023: return Properties.Resources.Large_Home_Base;
                case 12195: return Properties.Resources.Document;
                case 17343: return Properties.Resources.Application;
                case 16344: return Properties.Resources.Draw_Doc;


                case 1005: return Properties.Resources.MacWrite;
                case 1004: return Properties.Resources.Write_Doc;
                case 1003: return Properties.Resources.MacPaint;
                case 1002: return Properties.Resources.Paint_Doc;
                case 1001: return Properties.Resources.HyperCard;
                case 1000: return Properties.Resources.Stack;
                case 2002: return Properties.Resources.Bill;
            }

            Console.WriteLine("icon with id {0} was not found", id);
            return Properties.Resources.Icon_Substitute;
        }
    }
}
