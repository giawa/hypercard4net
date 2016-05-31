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
                    OpenStack(new HyperCard.StackReader(file.FullName));
                    return;
                }
            }
        }

        public Window(HyperCard.StackReader stack)
        {
            InitializeComponent();

            OpenStack(stack);
        }

        private void OpenStack(HyperCard.StackReader stack)
        {
            //this.Height = stack.Height + 24 + SystemInformation.Border3DSize.Width;
            //this.Width = stack.Width + SystemInformation.Border3DSize.Height;
            this.ClientSize = new System.Drawing.Size(stack.Width, stack.Height + 24);

            this.Text = "HyperCard .NET : " + stack.Name;

            this.cardRenderer1.SetCard(stack, stack.Cards[0]);
        }
    }
}
