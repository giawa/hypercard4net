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
            this.ClientSize = new System.Drawing.Size(stack.Width, stack.Height + 24);

            this.Text = "HyperCard .NET : " + stack.Name;

            this.cardRenderer1.SetCard(stack, stack.Cards[7]);
        }

        public HyperCard.UserLevel UserLevel = 0;

        private void BuildMenu()
        {
            //this.menuStrip1.Items.Clear();

            //this.menuStrip1.Items.Add(Player.Menu.BuildFileMenu(this));
            //this.menuStrip1.Items.Add(Player.Menu.BuildToolsMenu(this, this.menuStrip1));
        }
    }
}
