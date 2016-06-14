namespace Player
{
    partial class Window
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Window));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.cardRenderer1 = new Player.CardRenderer();
            ((System.ComponentModel.ISupportInitialize)(this.cardRenderer1)).BeginInit();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.menuStrip1.Size = new System.Drawing.Size(1058, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // cardRenderer1
            // 
            this.cardRenderer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cardRenderer1.Image = ((System.Drawing.Image)(resources.GetObject("cardRenderer1.Image")));
            this.cardRenderer1.Location = new System.Drawing.Point(0, 24);
            this.cardRenderer1.Name = "cardRenderer1";
            this.cardRenderer1.Size = new System.Drawing.Size(1058, 590);
            this.cardRenderer1.TabIndex = 1;
            this.cardRenderer1.TabStop = false;
            // 
            // Window
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1058, 614);
            this.Controls.Add(this.cardRenderer1);
            this.Controls.Add(this.menuStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Window";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "HyperCard.NET";
            ((System.ComponentModel.ISupportInitialize)(this.cardRenderer1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private CardRenderer cardRenderer1;
    }
}

