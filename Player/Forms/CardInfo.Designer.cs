namespace Player.Forms
{
    partial class CardInfo
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.labelNumber = new System.Windows.Forms.Label();
            this.labelID = new System.Windows.Forms.Label();
            this.labelButtons = new System.Windows.Forms.Label();
            this.labelFields = new System.Windows.Forms.Label();
            this.checkBoxCardMarked = new System.Windows.Forms.CheckBox();
            this.checkBoxDontSearchCard = new System.Windows.Forms.CheckBox();
            this.checkBoxCantDeleteCard = new System.Windows.Forms.CheckBox();
            this.buttonScript = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.textBoxCardName = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Card Name:";
            // 
            // labelNumber
            // 
            this.labelNumber.AutoSize = true;
            this.labelNumber.Location = new System.Drawing.Point(13, 38);
            this.labelNumber.Name = "labelNumber";
            this.labelNumber.Size = new System.Drawing.Size(118, 13);
            this.labelNumber.TabIndex = 1;
            this.labelNumber.Text = "Card number: 1 out of 9";
            // 
            // labelID
            // 
            this.labelID.AutoSize = true;
            this.labelID.Location = new System.Drawing.Point(13, 54);
            this.labelID.Name = "labelID";
            this.labelID.Size = new System.Drawing.Size(73, 13);
            this.labelID.TabIndex = 2;
            this.labelID.Text = "Card ID: 5698";
            // 
            // labelButtons
            // 
            this.labelButtons.AutoSize = true;
            this.labelButtons.Location = new System.Drawing.Point(13, 91);
            this.labelButtons.Name = "labelButtons";
            this.labelButtons.Size = new System.Drawing.Size(125, 13);
            this.labelButtons.TabIndex = 4;
            this.labelButtons.Text = "Contains 14 card buttons";
            // 
            // labelFields
            // 
            this.labelFields.AutoSize = true;
            this.labelFields.Location = new System.Drawing.Point(13, 75);
            this.labelFields.Name = "labelFields";
            this.labelFields.Size = new System.Drawing.Size(108, 13);
            this.labelFields.TabIndex = 3;
            this.labelFields.Text = "Contains 7 card fields";
            // 
            // checkBoxCardMarked
            // 
            this.checkBoxCardMarked.AutoSize = true;
            this.checkBoxCardMarked.Location = new System.Drawing.Point(16, 116);
            this.checkBoxCardMarked.Name = "checkBoxCardMarked";
            this.checkBoxCardMarked.Size = new System.Drawing.Size(87, 17);
            this.checkBoxCardMarked.TabIndex = 5;
            this.checkBoxCardMarked.Text = "Card Marked";
            this.checkBoxCardMarked.UseVisualStyleBackColor = true;
            // 
            // checkBoxDontSearchCard
            // 
            this.checkBoxDontSearchCard.AutoSize = true;
            this.checkBoxDontSearchCard.Location = new System.Drawing.Point(16, 133);
            this.checkBoxDontSearchCard.Name = "checkBoxDontSearchCard";
            this.checkBoxDontSearchCard.Size = new System.Drawing.Size(113, 17);
            this.checkBoxDontSearchCard.TabIndex = 6;
            this.checkBoxDontSearchCard.Text = "Don\'t Search Card";
            this.checkBoxDontSearchCard.UseVisualStyleBackColor = true;
            // 
            // checkBoxCantDeleteCard
            // 
            this.checkBoxCantDeleteCard.AutoSize = true;
            this.checkBoxCantDeleteCard.Location = new System.Drawing.Point(16, 150);
            this.checkBoxCantDeleteCard.Name = "checkBoxCantDeleteCard";
            this.checkBoxCantDeleteCard.Size = new System.Drawing.Size(109, 17);
            this.checkBoxCantDeleteCard.TabIndex = 7;
            this.checkBoxCantDeleteCard.Text = "Can\'t Delete Card";
            this.checkBoxCantDeleteCard.UseVisualStyleBackColor = true;
            // 
            // buttonScript
            // 
            this.buttonScript.Location = new System.Drawing.Point(16, 174);
            this.buttonScript.Name = "buttonScript";
            this.buttonScript.Size = new System.Drawing.Size(75, 23);
            this.buttonScript.TabIndex = 8;
            this.buttonScript.Text = "Script...";
            this.buttonScript.UseVisualStyleBackColor = true;
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(186, 174);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 9;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(186, 144);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 10;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // textBoxCardName
            // 
            this.textBoxCardName.Location = new System.Drawing.Point(82, 10);
            this.textBoxCardName.Name = "textBoxCardName";
            this.textBoxCardName.Size = new System.Drawing.Size(177, 20);
            this.textBoxCardName.TabIndex = 11;
            // 
            // CardInfo
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(271, 207);
            this.Controls.Add(this.textBoxCardName);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonScript);
            this.Controls.Add(this.checkBoxCantDeleteCard);
            this.Controls.Add(this.checkBoxDontSearchCard);
            this.Controls.Add(this.checkBoxCardMarked);
            this.Controls.Add(this.labelButtons);
            this.Controls.Add(this.labelFields);
            this.Controls.Add(this.labelID);
            this.Controls.Add(this.labelNumber);
            this.Controls.Add(this.label1);
            this.Name = "CardInfo";
            this.Text = "Card Info";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labelNumber;
        private System.Windows.Forms.Label labelID;
        private System.Windows.Forms.Label labelButtons;
        private System.Windows.Forms.Label labelFields;
        private System.Windows.Forms.CheckBox checkBoxCardMarked;
        private System.Windows.Forms.CheckBox checkBoxDontSearchCard;
        private System.Windows.Forms.CheckBox checkBoxCantDeleteCard;
        private System.Windows.Forms.Button buttonScript;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.TextBox textBoxCardName;
    }
}