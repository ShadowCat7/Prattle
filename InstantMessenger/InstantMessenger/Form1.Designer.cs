namespace InstantMessenger
{
    partial class ChatServer
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
            this.textBox = new System.Windows.Forms.RichTextBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.iPAddressToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sendButton = new System.Windows.Forms.Button();
            this.chatBox = new System.Windows.Forms.TextBox();
            this.userTextBox = new System.Windows.Forms.RichTextBox();
            this.serverToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBox
            // 
            this.textBox.AcceptsTab = true;
            this.textBox.Location = new System.Drawing.Point(35, 22);
            this.textBox.Name = "textBox";
            this.textBox.ReadOnly = true;
            this.textBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedVertical;
            this.textBox.Size = new System.Drawing.Size(441, 314);
            this.textBox.TabIndex = 5;
            this.textBox.TabStop = false;
            this.textBox.Text = "";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.serverToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(648, 24);
            this.menuStrip1.TabIndex = 3;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.iPAddressToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(40, 20);
            this.fileToolStripMenuItem.Text = "Info";
            // 
            // iPAddressToolStripMenuItem
            // 
            this.iPAddressToolStripMenuItem.Name = "iPAddressToolStripMenuItem";
            this.iPAddressToolStripMenuItem.Size = new System.Drawing.Size(138, 22);
            this.iPAddressToolStripMenuItem.Text = "IP Address...";
            // 
            // sendButton
            // 
            this.sendButton.Location = new System.Drawing.Point(570, 349);
            this.sendButton.Name = "sendButton";
            this.sendButton.Size = new System.Drawing.Size(66, 27);
            this.sendButton.TabIndex = 1;
            this.sendButton.Text = "Send";
            this.sendButton.UseVisualStyleBackColor = true;
            this.sendButton.Click += new System.EventHandler(this.sendButton_Click);
            // 
            // chatBox
            // 
            this.chatBox.Location = new System.Drawing.Point(35, 353);
            this.chatBox.Name = "chatBox";
            this.chatBox.Size = new System.Drawing.Size(513, 20);
            this.chatBox.TabIndex = 0;
            // 
            // userTextBox
            // 
            this.userTextBox.Location = new System.Drawing.Point(498, 22);
            this.userTextBox.Name = "userTextBox";
            this.userTextBox.ReadOnly = true;
            this.userTextBox.Size = new System.Drawing.Size(124, 314);
            this.userTextBox.TabIndex = 0;
            this.userTextBox.TabStop = false;
            this.userTextBox.Text = "";
            // 
            // serverToolStripMenuItem
            // 
            this.serverToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem});
            this.serverToolStripMenuItem.Name = "serverToolStripMenuItem";
            this.serverToolStripMenuItem.Size = new System.Drawing.Size(51, 20);
            this.serverToolStripMenuItem.Text = "Server";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // ChatServer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(648, 389);
            this.Controls.Add(this.userTextBox);
            this.Controls.Add(this.chatBox);
            this.Controls.Add(this.sendButton);
            this.Controls.Add(this.textBox);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "ChatServer";
            this.Text = "Instant Messenger";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ChatServer_KeyDown);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox textBox;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem iPAddressToolStripMenuItem;
        private System.Windows.Forms.Button sendButton;
        private System.Windows.Forms.TextBox chatBox;
        private System.Windows.Forms.RichTextBox userTextBox;
        private System.Windows.Forms.ToolStripMenuItem serverToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
    }
}

