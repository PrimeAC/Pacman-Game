namespace pacman
{
    partial class PuppetMasterWindow
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
            this.components = new System.ComponentModel.Container();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.tbMsg = new System.Windows.Forms.TextBox();
            this.tbChat = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // tbMsg
            // 
            this.tbMsg.Location = new System.Drawing.Point(12, 315);
            this.tbMsg.Name = "tbMsg";
            this.tbMsg.Size = new System.Drawing.Size(455, 20);
            this.tbMsg.TabIndex = 143;
            this.tbMsg.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tbMsg_KeyDown);
            // 
            // tbChat
            // 
            this.tbChat.Enabled = false;
            this.tbChat.Location = new System.Drawing.Point(12, 40);
            this.tbChat.Multiline = true;
            this.tbChat.Name = "tbChat";
            this.tbChat.Size = new System.Drawing.Size(455, 255);
            this.tbChat.TabIndex = 144;
            // 
            // PuppetMasterWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(479, 344);
            this.Controls.Add(this.tbChat);
            this.Controls.Add(this.tbMsg);
            this.Name = "PuppetMasterWindow";
            this.Text = "DADman";
            this.Load += new System.EventHandler(this.PuppetMasterWindow_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.PictureBox pacman;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.TextBox tbMsg;
        private System.Windows.Forms.TextBox tbChat;
    }
}

