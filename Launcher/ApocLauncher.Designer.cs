using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Launcher.Properties;

namespace Launcher
{
    partial class ApocLauncher
    {
        /// <summary>
        /// Variable needed by the designer.
        /// </ summary>
        private IContainer components = null;

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
            this.panelCreateAccount = new System.Windows.Forms.Panel();
            this.bnCreateLocal = new System.Windows.Forms.Button();
            this.buttonAccountClose = new System.Windows.Forms.Button();
            this.buttonCreate = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxPassword = new System.Windows.Forms.TextBox();
            this.textBoxUsername = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonPanelCreateAccount = new System.Windows.Forms.Button();
            this.lblVersion = new System.Windows.Forms.Label();
            this.bnConnectLocal = new System.Windows.Forms.Button();
            this.panelCreateAccount.SuspendLayout();
            this.SuspendLayout();
            // 
            // T_username
            // 
            this.T_username.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.T_username.BackColor = System.Drawing.Color.Black;
            this.T_username.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.T_username.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.T_username.ForeColor = System.Drawing.Color.DarkOrange;
            this.T_username.Location = new System.Drawing.Point(183, 649);
            this.T_username.Margin = new System.Windows.Forms.Padding(0);
            this.T_username.MaxLength = 50;
            this.T_username.Multiline = true;
            this.T_username.Name = "T_username";
            this.T_username.Size = new System.Drawing.Size(303, 44);
            this.T_username.TabIndex = 0;
            // 
            // T_password
            // 
            this.T_password.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.T_password.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.T_password.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.T_password.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.T_password.ForeColor = System.Drawing.Color.DarkOrange;
            this.T_password.Location = new System.Drawing.Point(789, 649);
            this.T_password.Margin = new System.Windows.Forms.Padding(0);
            this.T_password.Name = "T_password";
            this.T_password.Size = new System.Drawing.Size(282, 44);
            this.T_password.TabIndex = 1;
            this.T_password.UseSystemPasswordChar = true;
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
            this.bnTestServer.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bnTestServer.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.bnTestServer.Location = new System.Drawing.Point(1095, 648);
            this.bnTestServer.Margin = new System.Windows.Forms.Padding(0);
            this.bnTestServer.Name = "bnTestServer";
            this.bnTestServer.Size = new System.Drawing.Size(176, 50);
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
            this.lblConnection.Location = new System.Drawing.Point(1071, 701);
            this.lblConnection.Name = "lblConnection";
            this.lblConnection.Size = new System.Drawing.Size(0, 13);
            this.lblConnection.TabIndex = 13;
            this.lblConnection.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // bnClose
            // 
            this.bnClose.BackColor = System.Drawing.Color.Transparent;
            this.bnClose.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.bnClose.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.bnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bnClose.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bnClose.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.bnClose.Location = new System.Drawing.Point(1229, 0);
            this.bnClose.Margin = new System.Windows.Forms.Padding(0);
            this.bnClose.Name = "bnClose";
            this.bnClose.Size = new System.Drawing.Size(51, 44);
            this.bnClose.TabIndex = 14;
            this.bnClose.Text = "X";
            this.bnClose.UseVisualStyleBackColor = false;
            this.bnClose.Click += new System.EventHandler(this.bnClose_Click);
            // 
            // panelCreateAccount
            // 
            this.panelCreateAccount.BackColor = System.Drawing.Color.Transparent;
            this.panelCreateAccount.BackgroundImage = global::Launcher.Properties.Resources.background;
            this.panelCreateAccount.Controls.Add(this.bnCreateLocal);
            this.panelCreateAccount.Controls.Add(this.buttonAccountClose);
            this.panelCreateAccount.Controls.Add(this.buttonCreate);
            this.panelCreateAccount.Controls.Add(this.label3);
            this.panelCreateAccount.Controls.Add(this.label2);
            this.panelCreateAccount.Controls.Add(this.textBoxPassword);
            this.panelCreateAccount.Controls.Add(this.textBoxUsername);
            this.panelCreateAccount.Controls.Add(this.label1);
            this.panelCreateAccount.Location = new System.Drawing.Point(366, 177);
            this.panelCreateAccount.Name = "panelCreateAccount";
            this.panelCreateAccount.Size = new System.Drawing.Size(590, 309);
            this.panelCreateAccount.TabIndex = 15;
            this.panelCreateAccount.Visible = false;
            // 
            // bnCreateLocal
            // 
            this.bnCreateLocal.BackColor = System.Drawing.Color.Transparent;
            this.bnCreateLocal.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.bnCreateLocal.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.bnCreateLocal.Location = new System.Drawing.Point(385, 210);
            this.bnCreateLocal.Name = "bnCreateLocal";
            this.bnCreateLocal.Size = new System.Drawing.Size(176, 21);
            this.bnCreateLocal.TabIndex = 23;
            this.bnCreateLocal.Text = "LOCAL";
            this.bnCreateLocal.UseVisualStyleBackColor = false;
            this.bnCreateLocal.Visible = false;
            this.bnCreateLocal.Click += new System.EventHandler(this.bnCreateLocal_Click);
            // 
            // buttonAccountClose
            // 
            this.buttonAccountClose.BackColor = System.Drawing.Color.Transparent;
            this.buttonAccountClose.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.buttonAccountClose.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.buttonAccountClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonAccountClose.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonAccountClose.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.buttonAccountClose.Location = new System.Drawing.Point(539, 1);
            this.buttonAccountClose.Margin = new System.Windows.Forms.Padding(0);
            this.buttonAccountClose.Name = "buttonAccountClose";
            this.buttonAccountClose.Size = new System.Drawing.Size(51, 44);
            this.buttonAccountClose.TabIndex = 22;
            this.buttonAccountClose.Text = "X";
            this.buttonAccountClose.UseVisualStyleBackColor = false;
            this.buttonAccountClose.Click += new System.EventHandler(this.buttonAccountClose_Click);
            // 
            // buttonCreate
            // 
            this.buttonCreate.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.buttonCreate.Font = new System.Drawing.Font("Microsoft Sans Serif", 26.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonCreate.ForeColor = System.Drawing.Color.DarkOrange;
            this.buttonCreate.Location = new System.Drawing.Point(30, 237);
            this.buttonCreate.Name = "buttonCreate";
            this.buttonCreate.Size = new System.Drawing.Size(531, 51);
            this.buttonCreate.TabIndex = 16;
            this.buttonCreate.Text = "CREATE";
            this.buttonCreate.UseVisualStyleBackColor = true;
            this.buttonCreate.Click += new System.EventHandler(this.buttonCreate_Click);
            // 
            // label3
            // 
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 26.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.DarkOrange;
            this.label3.Location = new System.Drawing.Point(3, 153);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(272, 53);
            this.label3.TabIndex = 20;
            this.label3.Text = "PASSWORD";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label2
            // 
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 26.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.DarkOrange;
            this.label2.Location = new System.Drawing.Point(3, 78);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(272, 53);
            this.label2.TabIndex = 19;
            this.label2.Text = "USERNAME";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // textBoxPassword
            // 
            this.textBoxPassword.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBoxPassword.BackColor = System.Drawing.Color.Black;
            this.textBoxPassword.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxPassword.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxPassword.ForeColor = System.Drawing.Color.DarkOrange;
            this.textBoxPassword.Location = new System.Drawing.Point(277, 153);
            this.textBoxPassword.Margin = new System.Windows.Forms.Padding(0);
            this.textBoxPassword.MaxLength = 50;
            this.textBoxPassword.Multiline = true;
            this.textBoxPassword.Name = "textBoxPassword";
            this.textBoxPassword.PasswordChar = '*';
            this.textBoxPassword.Size = new System.Drawing.Size(284, 53);
            this.textBoxPassword.TabIndex = 17;
            // 
            // textBoxUsername
            // 
            this.textBoxUsername.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBoxUsername.BackColor = System.Drawing.Color.Black;
            this.textBoxUsername.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxUsername.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxUsername.ForeColor = System.Drawing.Color.DarkOrange;
            this.textBoxUsername.Location = new System.Drawing.Point(277, 78);
            this.textBoxUsername.Margin = new System.Windows.Forms.Padding(0);
            this.textBoxUsername.MaxLength = 50;
            this.textBoxUsername.Multiline = true;
            this.textBoxUsername.Name = "textBoxUsername";
            this.textBoxUsername.Size = new System.Drawing.Size(284, 52);
            this.textBoxUsername.TabIndex = 16;
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 26.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.DarkOrange;
            this.label1.Location = new System.Drawing.Point(3, 2);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(587, 49);
            this.label1.TabIndex = 18;
            this.label1.Text = "CREATE ACCOUNT";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // buttonPanelCreateAccount
            // 
            this.buttonPanelCreateAccount.BackColor = System.Drawing.Color.Transparent;
            this.buttonPanelCreateAccount.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonPanelCreateAccount.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonPanelCreateAccount.ForeColor = System.Drawing.Color.DarkOrange;
            this.buttonPanelCreateAccount.Location = new System.Drawing.Point(12, 22);
            this.buttonPanelCreateAccount.Name = "buttonPanelCreateAccount";
            this.buttonPanelCreateAccount.Size = new System.Drawing.Size(226, 49);
            this.buttonPanelCreateAccount.TabIndex = 21;
            this.buttonPanelCreateAccount.Text = "CREATE ACCOUNT";
            this.buttonPanelCreateAccount.UseVisualStyleBackColor = false;
            this.buttonPanelCreateAccount.Click += new System.EventHandler(this.buttonPanelCreateAccount_Click);
            // 
            // lblVersion
            // 
            this.lblVersion.AutoSize = true;
            this.lblVersion.ForeColor = System.Drawing.Color.DarkOrange;
            this.lblVersion.Location = new System.Drawing.Point(24, 698);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(0, 13);
            this.lblVersion.TabIndex = 22;
            // 
            // bnConnectLocal
            // 
            this.bnConnectLocal.BackColor = System.Drawing.Color.Transparent;
            this.bnConnectLocal.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.bnConnectLocal.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.bnConnectLocal.Location = new System.Drawing.Point(1095, 624);
            this.bnConnectLocal.Name = "bnConnectLocal";
            this.bnConnectLocal.Size = new System.Drawing.Size(176, 21);
            this.bnConnectLocal.TabIndex = 25;
            this.bnConnectLocal.Text = "LOCAL";
            this.bnConnectLocal.UseVisualStyleBackColor = false;
            this.bnConnectLocal.Visible = false;
            this.bnConnectLocal.Click += new System.EventHandler(this.button1_Click);
            // 
            // ApocLauncher
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.BackgroundImage = global::Launcher.Properties.Resources.final1;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.ClientSize = new System.Drawing.Size(1280, 720);
            this.Controls.Add(this.bnConnectLocal);
            this.Controls.Add(this.lblVersion);
            this.Controls.Add(this.buttonPanelCreateAccount);
            this.Controls.Add(this.panelCreateAccount);
            this.Controls.Add(this.bnClose);
            this.Controls.Add(this.lblConnection);
            this.Controls.Add(this.bnTestServer);
            this.Controls.Add(this.lblLauncherServer);
            this.Controls.Add(this.T_password);
            this.Controls.Add(this.T_username);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ApocLauncher";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "WAR: APOCALYPSE";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Disconnect);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.panelCreateAccount.ResumeLayout(false);
            this.panelCreateAccount.PerformLayout();
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
        private System.Windows.Forms.Panel panelCreateAccount;
        private System.Windows.Forms.TextBox textBoxUsername;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxPassword;
        private System.Windows.Forms.Button buttonCreate;
        private System.Windows.Forms.Button buttonPanelCreateAccount;
        private System.Windows.Forms.Label lblVersion;
        private System.Windows.Forms.Button bnCreateLocal;
        private System.Windows.Forms.Button buttonAccountClose;
        private Button bnConnectLocal;
    }
}

