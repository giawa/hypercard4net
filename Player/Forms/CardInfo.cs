using System.Windows.Forms;

namespace Player.Forms
{
    public partial class CardInfo : Form
    {
        public CardInfo(HyperCard.Card card)
        {
            InitializeComponent();

            textBoxCardName.Text = card.Name;
            labelID.Text = $"Card ID: {card.ID}";
            labelNumber.Text = $"Card number: {card.Stack.FindEntry() + 1} out of {card.Stack.Cards.Count}";

            int fieldCount = 0;
            int buttonCount = 0;
            foreach (var part in card.Parts)
            {
                if (part.Type == HyperCard.PartType.Button) buttonCount++;
                else fieldCount++;
            }
            labelFields.Text = $"Contains {fieldCount} card fields.";
            labelButtons.Text = $"Contains {buttonCount} card buttons.";

            checkBoxCardMarked.Checked = false;
            checkBoxDontSearchCard.Checked = (card.Flags & HyperCard.CardFlags.DontSearch) == HyperCard.CardFlags.DontSearch;
            checkBoxCantDeleteCard.Checked = (card.Flags & HyperCard.CardFlags.CantDelete) == HyperCard.CardFlags.CantDelete;

            buttonScript.Enabled = false;
        }

        private void buttonCancel_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void buttonOK_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }
    }
}
