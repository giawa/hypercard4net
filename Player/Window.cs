using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Player
{
    public partial class Window : Form
    {
        public Window()
        {
            InitializeComponent();

            // get files in current working directory
            var files = new DirectoryInfo(Environment.CurrentDirectory).GetFiles();
            foreach (var file in files)
            {
                if (file.Name.StartsWith("Home"))
                {
                    OpenStack(new HyperCard.Stack(file.FullName));
                    return;
                }
            }
        }

        public Window(HyperCard.Stack stack)
        {
            InitializeComponent();

            OpenStack(stack);

            BuildMenu();
        }

        private void OpenStack(HyperCard.Stack stack)
        {
            this.cardRenderer1.SetStack(stack);
        }

        public HyperCard.UserLevel UserLevel = 0;

        private void BuildMenu()
        {
            this.menuStrip1.Items.Clear();

            this.menuStrip1.Items.Add(Player.Menu.BuildFileMenu(this));
            this.menuStrip1.Items.Add(Player.Menu.BuildEditMenu(this));
            this.menuStrip1.Items.Add(Player.Menu.BuildGoMenu(this));
            this.menuStrip1.Items.Add(Player.Menu.BuildToolsMenu(this, this.menuStrip1));
            this.menuStrip1.Items.Add(Player.Menu.BuildObjectsMenu(this));
            this.menuStrip1.Items.Add(Player.Menu.BuildFontMenu(this));
            this.menuStrip1.Items.Add(Player.Menu.BuildStyleMenu(this));
        }

        /// <summary>
        /// Intercept the arrow keys buttons
        /// </summary>
        /// <param name="keyData"></param>
        /// <returns></returns>
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
                cardRenderer1.Stack.PreviousCard();
            }
            else if (e.KeyCode == Keys.Right)
            {
                cardRenderer1.Stack.NextCard();
            }
        }
    }
}
