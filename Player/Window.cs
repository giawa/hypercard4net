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

        private HyperCard.Stack stack;

        private void OpenStack(HyperCard.Stack stack)
        {
            this.stack = stack;

            this.ClientSize = new System.Drawing.Size(stack.Width, stack.Height + 24);

            this.Text = "HyperCard .NET : " + stack.Name;

            //this.cardRenderer1.SetCard(stack, stack.GetCardFromID(stack.List.Pages[0].PageEntries[0]));
            this.cardRenderer1.SetCard(stack, stack.CurrentCard);
        }

        public HyperCard.UserLevel UserLevel = 0;

        private void BuildMenu()
        {
            //this.menuStrip1.Items.Clear();

            //this.menuStrip1.Items.Add(Player.Menu.BuildFileMenu(this));
            //this.menuStrip1.Items.Add(Player.Menu.BuildToolsMenu(this, this.menuStrip1));
        }

        private int pageIndex = 0;
        private int entryIndex = 0;

        private void FindEntry()
        {
            for (pageIndex = 0; pageIndex < stack.Pages.Count; pageIndex++)
            {
                for (entryIndex = 0; entryIndex < stack.Pages[pageIndex].PageEntries.Count; entryIndex++)
                {
                    if (stack.CurrentCard == stack.GetCardFromID(stack.List.Pages[pageIndex].PageEntries[entryIndex]))
                        return;
                }
            }

            // we never found the card we were looking for, so return the first card in the stack
            pageIndex = 0;
            entryIndex = 0;
        }

        public void NextCard()
        {
            FindEntry();
            entryIndex++;

            if (stack.Pages[pageIndex].PageEntries.Count == entryIndex)
            {
                pageIndex++;

                if (stack.Pages.Count == pageIndex)
                {
                    pageIndex = 0;
                }

                entryIndex = 0;
            }

            stack.CurrentCard = stack.GetCardFromID(stack.List.Pages[pageIndex].PageEntries[entryIndex]);
            cardRenderer1.Invalidate();
        }

        public void PreviousCard()
        {
            FindEntry();
            entryIndex--;

            if (entryIndex < 0)
            {
                pageIndex--;

                if (pageIndex < 0)
                {
                    pageIndex = stack.Pages.Count - 1;
                }

                entryIndex = stack.Pages[pageIndex].PageEntries.Count - 1;
            }

            stack.CurrentCard = stack.GetCardFromID(stack.List.Pages[pageIndex].PageEntries[entryIndex]);
            cardRenderer1.Invalidate();
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
                PreviousCard();
            }
            else if (e.KeyCode == Keys.Right)
            {
                NextCard();
            }
        }
    }
}
