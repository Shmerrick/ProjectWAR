namespace Launcher
{
    partial class ApocLauncher
    {
        /// <summary>
        /// Variable needed by the designer.
        /// </ summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Cleaning the resources used.
        /// </ summary>
        /// <param name = "disposing"> true if the managed resources should be deleted; otherwise, false. </ param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur Windows Form

        /// <summary>
        /// Method required to support the designer - do not modify
        /// the contents of this method with the code editor.
        /// </ summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ApocLauncher));
            this.T_username = new System.Windows.Forms.TextBox();
            this.T_password = new System.Windows.Forms.MaskedTextBox();
            this.B_start = new System.Windows.Forms.Button();
            this.RealmName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Online = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Players = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Destruction = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Order = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.lblLauncherServer = new System.Windows.Forms.Label();
            this.bnTestServer = new System.Windows.Forms.Button();
            this.edHashCode = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.bnCreateUser = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.edPassword = new System.Windows.Forms.MaskedTextBox();
            this.edNewUserCode = new System.Windows.Forms.TextBox();
            this.lblConnection = new System.Windows.Forms.Label();
            this.bnClose = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // T_username
            // 
            this.T_username.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.T_username.BackColor = System.Drawing.Color.Black;
            this.T_username.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.T_username.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.T_username.ForeColor = System.Drawing.Color.DarkOrange;
            this.T_username.Location = new System.Drawing.Point(172, 648);
            this.T_username.Margin = new System.Windows.Forms.Padding(0);
            this.T_username.MaxLength = 50;
            this.T_username.Multiline = true;
            this.T_username.Name = "T_username";
            this.T_username.Size = new System.Drawing.Size(330, 50);
            this.T_username.TabIndex = 0;
            // 
            // T_password
            // 
            this.T_password.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.T_password.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.T_password.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.T_password.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.T_password.ForeColor = System.Drawing.Color.DarkOrange;
            this.T_password.Location = new System.Drawing.Point(779, 648);
            this.T_password.Margin = new System.Windows.Forms.Padding(0);
            this.T_password.Name = "T_password";
            this.T_password.Size = new System.Drawing.Size(294, 44);
            this.T_password.TabIndex = 1;
            this.T_password.UseSystemPasswordChar = true;
            
            // 
            // B_start
            // 
            this.B_start.BackColor = System.Drawing.Color.Transparent;
            this.B_start.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.B_start.ForeColor = System.Drawing.Color.Transparent;
            this.B_start.Location = new System.Drawing.Point(0, 12);
            this.B_start.Name = "B_start";
            this.B_start.Size = new System.Drawing.Size(119, 28);
            this.B_start.TabIndex = 4;
            this.B_start.Text = "LOCAL";
            this.B_start.UseVisualStyleBackColor = false;
            this.B_start.Visible = false;
            this.B_start.Click += new System.EventHandler(this.B_start_Click);
            // 
            // RealmName
            // 
            this.RealmName.Name = "RealmName";
            // 
            // Online
            // 
            this.Online.Name = "Online";
            // 
            // Players
            // 
            this.Players.Name = "Players";
            // 
            // Destruction
            // 
            this.Destruction.Name = "Destruction";
            // 
            // Order
            // 
            this.Order.Name = "Order";
            // 
            // lblLauncherServer
            // 
            this.lblLauncherServer.AutoSize = true;
            this.lblLauncherServer.Location = new System.Drawing.Point(12, 117);
            this.lblLauncherServer.Name = "lblLauncherServer";
            this.lblLauncherServer.Size = new System.Drawing.Size(0, 13);
            this.lblLauncherServer.TabIndex = 11;
            // 
            // bnTestServer
            // 
            this.bnTestServer.BackColor = System.Drawing.Color.Transparent;
            this.bnTestServer.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.bnTestServer.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.bnTestServer.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.bnTestServer.ForeColor = System.Drawing.Color.Transparent;
            this.bnTestServer.Location = new System.Drawing.Point(1084, 620);
            this.bnTestServer.Margin = new System.Windows.Forms.Padding(0);
            this.bnTestServer.Name = "bnTestServer";
            this.bnTestServer.Size = new System.Drawing.Size(196, 95);
            this.bnTestServer.TabIndex = 12;
            this.bnTestServer.UseVisualStyleBackColor = false;
            this.bnTestServer.Click += new System.EventHandler(this.bnTestServer_Click);
            // 
            // edHashCode
            // 
            this.edHashCode.Location = new System.Drawing.Point(6, 78);
            this.edHashCode.Name = "edHashCode";
            this.edHashCode.Size = new System.Drawing.Size(385, 20);
            this.edHashCode.TabIndex = 16;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 62);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(60, 13);
            this.label7.TabIndex = 15;
            this.label7.Text = "Hash Code";
            // 
            // bnCreateUser
            // 
            this.bnCreateUser.Location = new System.Drawing.Point(0, 0);
            this.bnCreateUser.Name = "bnCreateUser";
            this.bnCreateUser.Size = new System.Drawing.Size(75, 23);
            this.bnCreateUser.TabIndex = 0;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(195, 23);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 13);
            this.label5.TabIndex = 7;
            this.label5.Text = "Password";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 23);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(47, 13);
            this.label6.TabIndex = 6;
            this.label6.Text = "Account";
            // 
            // edPassword
            // 
            this.edPassword.Location = new System.Drawing.Point(198, 39);
            this.edPassword.Name = "edPassword";
            this.edPassword.Size = new System.Drawing.Size(193, 20);
            this.edPassword.TabIndex = 5;
            // 
            // edNewUserCode
            // 
            this.edNewUserCode.Location = new System.Drawing.Point(6, 39);
            this.edNewUserCode.Name = "edNewUserCode";
            this.edNewUserCode.Size = new System.Drawing.Size(175, 20);
            this.edNewUserCode.TabIndex = 4;
            // 
            // lblConnection
            // 
            this.lblConnection.AutoSize = true;
            this.lblConnection.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.lblConnection.Location = new System.Drawing.Point(1081, 718);
            this.lblConnection.Name = "lblConnection";
            this.lblConnection.Size = new System.Drawing.Size(0, 13);
            this.lblConnection.TabIndex = 13;
            // 
            // bnClose
            // 
            this.bnClose.BackColor = System.Drawing.Color.Transparent;
            this.bnClose.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.bnClose.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.bnClose.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.bnClose.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bnClose.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.bnClose.Location = new System.Drawing.Point(1229, -4);
            this.bnClose.Margin = new System.Windows.Forms.Padding(0);
            this.bnClose.Name = "bnClose";
            this.bnClose.Size = new System.Drawing.Size(51, 44);
            this.bnClose.TabIndex = 14;
            this.bnClose.Text = "X";
            this.bnClose.UseVisualStyleBackColor = false;
            this.bnClose.Click += new System.EventHandler(this.bnClose_Click);
            // 
            // Accueil
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.BackgroundImage = global::Launcher.Properties.Resources.final2;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.ClientSize = new System.Drawing.Size(1280, 740);
            this.Controls.Add(this.bnClose);
            this.Controls.Add(this.lblConnection);
            this.Controls.Add(this.bnTestServer);
            this.Controls.Add(this.lblLauncherServer);
            this.Controls.Add(this.B_start);
            this.Controls.Add(this.T_password);
            this.Controls.Add(this.T_username);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Accueil";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "WAR: APOCALYPSE";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Disconnect);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox T_username;
        private System.Windows.Forms.MaskedTextBox T_password;
        private System.Windows.Forms.Button B_start;
        private System.Windows.Forms.DataGridViewTextBoxColumn RealmName;
        private System.Windows.Forms.DataGridViewTextBoxColumn Online;
        private System.Windows.Forms.DataGridViewTextBoxColumn Players;
        private System.Windows.Forms.DataGridViewTextBoxColumn Destruction;
        private System.Windows.Forms.DataGridViewTextBoxColumn Order;
        private System.Windows.Forms.Label lblLauncherServer;
        private System.Windows.Forms.Button bnTestServer;
        private System.Windows.Forms.Button bnCreateUser;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.MaskedTextBox edPassword;
        private System.Windows.Forms.TextBox edNewUserCode;
        private System.Windows.Forms.TextBox edHashCode;
        private System.Windows.Forms.Label label7;
        public System.Windows.Forms.Label lblConnection;
        private System.Windows.Forms.Button bnClose;
    }
}

