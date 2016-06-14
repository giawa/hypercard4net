using System;
using System.Drawing;
using System.Windows.Forms;

namespace Player
{
    public static class Menu
    {
        public static ToolStripItem BuildFileMenu(Window window)
        {
            ToolStripMenuItem file = new ToolStripMenuItem("File");

            file.DropDownItems.Add("New Stack");

            file.DropDownItems.Add("Open Stack");
            ((ToolStripMenuItem)file.DropDownItems[file.DropDownItems.Count - 1]).ShortcutKeys = Keys.O | Keys.Control;

            file.DropDownItems.Add("Close Stack");
            ((ToolStripMenuItem)file.DropDownItems[file.DropDownItems.Count - 1]).ShortcutKeys = Keys.W | Keys.Control;

            file.DropDownItems.Add("Save A Copy");

            file.DropDownItems.Add("-");

            file.DropDownItems.Add("Save A Copy");

            if (window.UserLevel >= HyperCard.UserLevel.Painting)
            {
                file.DropDownItems.Add("Protect Stack");

                file.DropDownItems.Add("Delete Stack");
            }

            file.DropDownItems.Add("-");

            file.DropDownItems.Add("Print Card");
            ((ToolStripMenuItem)file.DropDownItems[file.DropDownItems.Count - 1]).ShortcutKeys = Keys.P | Keys.Control;

            file.DropDownItems.Add("Print Stack");

            file.DropDownItems.Add("-");

            file.DropDownItems.Add("Quit HyperCard.NET");
            ((ToolStripMenuItem)file.DropDownItems[file.DropDownItems.Count - 1]).ShortcutKeys = Keys.Q | Keys.Control;
            ((ToolStripMenuItem)file.DropDownItems[file.DropDownItems.Count - 1]).Click += (sender, ea) => Application.Exit();

            return file;
        }

        public static ToolStripItem BuildEditMenu(Window window)
        {
            ToolStripMenuItem edit = new ToolStripMenuItem("Edit");

            edit.DropDownItems.Add("Undo");
            ((ToolStripMenuItem)edit.DropDownItems[edit.DropDownItems.Count - 1]).ShortcutKeys = Keys.Z | Keys.Control;

            edit.DropDownItems.Add("-");

            edit.DropDownItems.Add("Cut");
            ((ToolStripMenuItem)edit.DropDownItems[edit.DropDownItems.Count - 1]).ShortcutKeys = Keys.X | Keys.Control;

            edit.DropDownItems.Add("Copy");
            ((ToolStripMenuItem)edit.DropDownItems[edit.DropDownItems.Count - 1]).ShortcutKeys = Keys.C | Keys.Control;

            edit.DropDownItems.Add("Paste");
            ((ToolStripMenuItem)edit.DropDownItems[edit.DropDownItems.Count - 1]).ShortcutKeys = Keys.V | Keys.Control;

            edit.DropDownItems.Add("Clear");

            edit.DropDownItems.Add("-");

            edit.DropDownItems.Add("New Card");
            ((ToolStripMenuItem)edit.DropDownItems[edit.DropDownItems.Count - 1]).ShortcutKeys = Keys.N | Keys.Control;

            edit.DropDownItems.Add("Delete Card");

            edit.DropDownItems.Add("Cut Card");

            edit.DropDownItems.Add("Copy Card");

            edit.DropDownItems.Add("-");

            edit.DropDownItems.Add("Text Style");
            ((ToolStripMenuItem)edit.DropDownItems[edit.DropDownItems.Count - 1]).ShortcutKeys = Keys.T | Keys.Control;

            edit.DropDownItems.Add("Background");
            ((ToolStripMenuItem)edit.DropDownItems[edit.DropDownItems.Count - 1]).ShortcutKeys = Keys.B | Keys.Control;

            edit.DropDownItems.Add("Icon");
            ((ToolStripMenuItem)edit.DropDownItems[edit.DropDownItems.Count - 1]).ShortcutKeys = Keys.I | Keys.Control;

            edit.DropDownItems.Add("-");

            edit.DropDownItems.Add("Audio");

            edit.DropDownItems.Add("Audio Help");

            return edit;
        }

        public static ToolStripItem BuildGoMenu(Window window)
        {
            ToolStripMenuItem go = new ToolStripMenuItem("Go");

            go.DropDownItems.Add("Back");
            ((ToolStripMenuItem)go.DropDownItems[go.DropDownItems.Count - 1]).ShortcutKeys = Keys.Oemtilde | Keys.Control;

            go.DropDownItems.Add("Home");
            ((ToolStripMenuItem)go.DropDownItems[go.DropDownItems.Count - 1]).ShortcutKeys = Keys.H | Keys.Control;

            go.DropDownItems.Add("Help");
            ((ToolStripMenuItem)go.DropDownItems[go.DropDownItems.Count - 1]).ShortcutKeys = Keys.OemQuestion | Keys.Control;

            go.DropDownItems.Add("Recent");
            ((ToolStripMenuItem)go.DropDownItems[go.DropDownItems.Count - 1]).ShortcutKeys = Keys.R | Keys.Control;

            go.DropDownItems.Add("-");

            go.DropDownItems.Add("First");
            ((ToolStripMenuItem)go.DropDownItems[go.DropDownItems.Count - 1]).ShortcutKeys = Keys.D1 | Keys.Control;

            go.DropDownItems.Add("Prev");
            ((ToolStripMenuItem)go.DropDownItems[go.DropDownItems.Count - 1]).ShortcutKeys = Keys.D2 | Keys.Control;

            go.DropDownItems.Add("Next");
            ((ToolStripMenuItem)go.DropDownItems[go.DropDownItems.Count - 1]).ShortcutKeys = Keys.D3 | Keys.Control;

            go.DropDownItems.Add("Last");
            ((ToolStripMenuItem)go.DropDownItems[go.DropDownItems.Count - 1]).ShortcutKeys = Keys.D4 | Keys.Control;

            go.DropDownItems.Add("-");

            go.DropDownItems.Add("Find");
            ((ToolStripMenuItem)go.DropDownItems[go.DropDownItems.Count - 1]).ShortcutKeys = Keys.F | Keys.Control;

            go.DropDownItems.Add("Message");
            ((ToolStripMenuItem)go.DropDownItems[go.DropDownItems.Count - 1]).ShortcutKeys = Keys.M | Keys.Control;

            go.DropDownItems.Add("Scroll");
            ((ToolStripMenuItem)go.DropDownItems[go.DropDownItems.Count - 1]).ShortcutKeys = Keys.E | Keys.Control;

            go.DropDownItems.Add("Next Window");
            ((ToolStripMenuItem)go.DropDownItems[go.DropDownItems.Count - 1]).ShortcutKeys = Keys.L | Keys.Control;

            return go;
        }

        private static ToolRenderer menuRenderer = new ToolRenderer();

        public static ToolStripItem BuildToolsMenu(Window window, MenuStrip menu)
        {
            ToolStripMenuItem tools = new ToolStripMenuItem("Tools");

            menu.Renderer = menuRenderer;
            tools.DropDownItems.Add("Tools");

            return tools;
        }

        public static ToolStripItem BuildObjectsMenu(Window window)
        {
            ToolStripMenuItem objects = new ToolStripMenuItem("Objects");

            objects.DropDownItems.Add("Button Info");

            objects.DropDownItems.Add("Field Info");

            objects.DropDownItems.Add("Card Info");

            objects.DropDownItems.Add("Background Info");

            objects.DropDownItems.Add("Stack Info");

            objects.DropDownItems.Add("-");

            objects.DropDownItems.Add("Bring Closer");
            ((ToolStripMenuItem)objects.DropDownItems[objects.DropDownItems.Count - 1]).ShortcutKeys = Keys.Oemplus | Keys.Control;

            objects.DropDownItems.Add("Send Farther");
            ((ToolStripMenuItem)objects.DropDownItems[objects.DropDownItems.Count - 1]).ShortcutKeys = Keys.OemMinus | Keys.Control;

            objects.DropDownItems.Add("-");

            objects.DropDownItems.Add("New Button");

            objects.DropDownItems.Add("New Field");

            objects.DropDownItems.Add("New Background");

            return objects;
        }

        public static ToolStripItem BuildFontMenu(Window window)
        {
            ToolStripMenuItem font = new ToolStripMenuItem("Font");

            return font;
        }

        public static ToolStripItem BuildStyleMenu(Window window)
        {
            ToolStripMenuItem style = new ToolStripMenuItem("Style");

            return style;
        }
    }

    public class ToolRenderer : ToolStripProfessionalRenderer
    {
        protected override void OnRenderDropDownButtonBackground(ToolStripItemRenderEventArgs e)
        {
            base.OnRenderDropDownButtonBackground(e);

            if (e.Item.Name == "Tools")
            {
                using (Brush whiteBrush = new SolidBrush(Color.White))
                {
                    e.Graphics.FillRectangle(whiteBrush, e.ToolStrip.ClientRectangle);
                }
            }
        }

        protected override void OnRenderItemBackground(ToolStripItemRenderEventArgs e)
        {
            Console.WriteLine("Background: " + e.Item.Name);
            base.OnRenderItemBackground(e);
        }
    }
}
