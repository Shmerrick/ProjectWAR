using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Launcher.Properties;

namespace Launcher
{
    partial class WarLauncher
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WarLauncher));
            this.T_username = new System.Windows.Forms.TextBox();
            this.T_password = new System.Windows.Forms.MaskedTextBox();
            this.RealmName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Online = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Players = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Destruction = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Order = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.lblLauncherServer = new System.Windows.Forms.Label();
            this.bnConnectToServer = new System.Windows.Forms.Button();
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
            this.lblDownloading = new System.Windows.Forms.Label();
            this.buttonPanelCreateAccount = new System.Windows.Forms.Button();
            this.lblVersion = new System.Windows.Forms.Label();
            this.bnConnectLocal = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.label4 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.bnMinimise = new System.Windows.Forms.Button();
            this.panelCreateAccount.SuspendLayout();
            this.SuspendLayout();
            // 
            // T_username
            // 
            this.T_username.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.T_username.BackColor = System.Drawing.Color.White;
            this.T_username.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.T_username.Font = new System.Drawing.Font("CaslonAntiqueVL", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.T_username.ForeColor = System.Drawing.Color.DimGray;
            this.T_username.Location = new System.Drawing.Point(41, 197);
            this.T_username.Margin = new System.Windows.Forms.Padding(0);
            this.T_username.MaxLength = 50;
            this.T_username.Multiline = true;
            this.T_username.Name = "T_username";
            this.T_username.Size = new System.Drawing.Size(183, 31);
            this.T_username.TabIndex = 0;
            this.T_username.KeyDown += new System.Windows.Forms.KeyEventHandler(this.T_username_KeyDown);
            // 
            // T_password
            // 
            this.T_password.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.T_password.BackColor = System.Drawing.Color.White;
            this.T_password.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.T_password.Font = new System.Drawing.Font("CaslonAntiqueVL", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.T_password.ForeColor = System.Drawing.Color.DimGray;
            this.T_password.Location = new System.Drawing.Point(41, 262);
            this.T_password.Margin = new System.Windows.Forms.Padding(0);
            this.T_password.Name = "T_password";
            this.T_password.PasswordChar = '*';
            this.T_password.Size = new System.Drawing.Size(183, 31);
            this.T_password.TabIndex = 1;
            this.T_password.KeyDown += new System.Windows.Forms.KeyEventHandler(this.T_password_KeyDown);
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
            // bnConnectToServer
            // 
            this.bnConnectToServer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bnConnectToServer.BackColor = System.Drawing.Color.Transparent;
            this.bnConnectToServer.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.bnConnectToServer.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.bnConnectToServer.FlatAppearance.BorderSize = 0;
            this.bnConnectToServer.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.bnConnectToServer.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.bnConnectToServer.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bnConnectToServer.Font = new System.Drawing.Font("CaslonAntiqueVL", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bnConnectToServer.ForeColor = System.Drawing.Color.White;
            this.bnConnectToServer.Image = global::Launcher.Properties.Resources.play_button_emp;
            this.bnConnectToServer.Location = new System.Drawing.Point(69, 341);
            this.bnConnectToServer.Margin = new System.Windows.Forms.Padding(0);
            this.bnConnectToServer.Name = "bnConnectToServer";
            this.bnConnectToServer.Size = new System.Drawing.Size(125, 43);
            this.bnConnectToServer.TabIndex = 12;
            this.bnConnectToServer.Text = "LOGIN";
            this.bnConnectToServer.UseVisualStyleBackColor = false;
            this.bnConnectToServer.Click += new System.EventHandler(this.bnConnectToServer_Click);
            this.bnConnectToServer.MouseLeave += new System.EventHandler(this.bnConnectToServer_MouseLeave);
            this.bnConnectToServer.MouseHover += new System.EventHandler(this.bnConnectToServer_MouseHover);
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
            this.lblConnection.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblConnection.BackColor = System.Drawing.Color.Transparent;
            this.lblConnection.Font = new System.Drawing.Font("CaslonAntiqueVL", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblConnection.ForeColor = System.Drawing.Color.SeaShell;
            this.lblConnection.Location = new System.Drawing.Point(295, 386);
            this.lblConnection.Name = "lblConnection";
            this.lblConnection.Size = new System.Drawing.Size(502, 24);
            this.lblConnection.TabIndex = 13;
            this.lblConnection.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // bnClose
            // 
            this.bnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bnClose.BackColor = System.Drawing.Color.Transparent;
            this.bnClose.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.bnClose.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.bnClose.FlatAppearance.BorderSize = 0;
            this.bnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bnClose.Font = new System.Drawing.Font("CaslonAntiqueVL", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bnClose.ForeColor = System.Drawing.Color.White;
            this.bnClose.Location = new System.Drawing.Point(777, 1);
            this.bnClose.Margin = new System.Windows.Forms.Padding(0);
            this.bnClose.Name = "bnClose";
            this.bnClose.Size = new System.Drawing.Size(20, 20);
            this.bnClose.TabIndex = 14;
            this.bnClose.Text = "X";
            this.bnClose.UseVisualStyleBackColor = false;
            this.bnClose.Click += new System.EventHandler(this.bnClose_Click);
            // 
            // panelCreateAccount
            // 
            this.panelCreateAccount.Anchor = System.Windows.Forms.AnchorStyles.None;
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
            this.panelCreateAccount.Location = new System.Drawing.Point(341, 74);
            this.panelCreateAccount.Name = "panelCreateAccount";
            this.panelCreateAccount.Size = new System.Drawing.Size(391, 305);
            this.panelCreateAccount.TabIndex = 15;
            this.panelCreateAccount.Visible = false;
            // 
            // bnCreateLocal
            // 
            this.bnCreateLocal.BackColor = System.Drawing.Color.Transparent;
            this.bnCreateLocal.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.bnCreateLocal.Font = new System.Drawing.Font("CaslonAntiqueVL", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bnCreateLocal.ForeColor = System.Drawing.Color.White;
            this.bnCreateLocal.Location = new System.Drawing.Point(204, 214);
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
            this.buttonAccountClose.FlatAppearance.BorderSize = 0;
            this.buttonAccountClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonAccountClose.Font = new System.Drawing.Font("CaslonAntiqueVL", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonAccountClose.ForeColor = System.Drawing.Color.White;
            this.buttonAccountClose.Location = new System.Drawing.Point(344, 0);
            this.buttonAccountClose.Margin = new System.Windows.Forms.Padding(0);
            this.buttonAccountClose.Name = "buttonAccountClose";
            this.buttonAccountClose.Size = new System.Drawing.Size(36, 38);
            this.buttonAccountClose.TabIndex = 22;
            this.buttonAccountClose.Text = "X";
            this.buttonAccountClose.UseVisualStyleBackColor = false;
            this.buttonAccountClose.Click += new System.EventHandler(this.buttonAccountClose_Click);
            // 
            // buttonCreate
            // 
            this.buttonCreate.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.buttonCreate.Font = new System.Drawing.Font("CaslonAntiqueVL", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonCreate.ForeColor = System.Drawing.Color.SeaShell;
            this.buttonCreate.Location = new System.Drawing.Point(10, 242);
            this.buttonCreate.Name = "buttonCreate";
            this.buttonCreate.Size = new System.Drawing.Size(370, 51);
            this.buttonCreate.TabIndex = 16;
            this.buttonCreate.Text = "Create";
            this.buttonCreate.UseVisualStyleBackColor = true;
            this.buttonCreate.Click += new System.EventHandler(this.buttonCreate_Click);
            // 
            // label3
            // 
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.Font = new System.Drawing.Font("CaslonAntiqueVL", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.SeaShell;
            this.label3.Location = new System.Drawing.Point(45, 152);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(93, 53);
            this.label3.TabIndex = 20;
            this.label3.Text = "Password:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label2
            // 
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.Font = new System.Drawing.Font("CaslonAntiqueVL", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.SeaShell;
            this.label2.Location = new System.Drawing.Point(45, 71);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(99, 53);
            this.label2.TabIndex = 19;
            this.label2.Text = "Username:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textBoxPassword
            // 
            this.textBoxPassword.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBoxPassword.BackColor = System.Drawing.Color.Black;
            this.textBoxPassword.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxPassword.Font = new System.Drawing.Font("CaslonAntiqueVL", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxPassword.ForeColor = System.Drawing.Color.SeaShell;
            this.textBoxPassword.Location = new System.Drawing.Point(161, 160);
            this.textBoxPassword.Margin = new System.Windows.Forms.Padding(0);
            this.textBoxPassword.MaxLength = 50;
            this.textBoxPassword.Multiline = true;
            this.textBoxPassword.Name = "textBoxPassword";
            this.textBoxPassword.PasswordChar = '*';
            this.textBoxPassword.Size = new System.Drawing.Size(206, 31);
            this.textBoxPassword.TabIndex = 17;
            // 
            // textBoxUsername
            // 
            this.textBoxUsername.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBoxUsername.BackColor = System.Drawing.Color.Black;
            this.textBoxUsername.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxUsername.Font = new System.Drawing.Font("CaslonAntiqueVL", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxUsername.ForeColor = System.Drawing.Color.SeaShell;
            this.textBoxUsername.Location = new System.Drawing.Point(161, 82);
            this.textBoxUsername.Margin = new System.Windows.Forms.Padding(0);
            this.textBoxUsername.MaxLength = 50;
            this.textBoxUsername.Multiline = true;
            this.textBoxUsername.Name = "textBoxUsername";
            this.textBoxUsername.Size = new System.Drawing.Size(206, 31);
            this.textBoxUsername.TabIndex = 16;
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.label1.Font = new System.Drawing.Font("CaslonAntiqueVL", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.SeaShell;
            this.label1.Location = new System.Drawing.Point(-5, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(396, 49);
            this.label1.TabIndex = 18;
            this.label1.Text = "Create Account";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblDownloading
            // 
            this.lblDownloading.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblDownloading.BackColor = System.Drawing.Color.Transparent;
            this.lblDownloading.Font = new System.Drawing.Font("CaslonAntiqueVL", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDownloading.ForeColor = System.Drawing.Color.SeaShell;
            this.lblDownloading.Location = new System.Drawing.Point(572, 424);
            this.lblDownloading.Name = "lblDownloading";
            this.lblDownloading.Size = new System.Drawing.Size(222, 21);
            this.lblDownloading.TabIndex = 27;
            this.lblDownloading.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // buttonPanelCreateAccount
            // 
            this.buttonPanelCreateAccount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonPanelCreateAccount.BackColor = System.Drawing.Color.Transparent;
            this.buttonPanelCreateAccount.FlatAppearance.BorderSize = 0;
            this.buttonPanelCreateAccount.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonPanelCreateAccount.Font = new System.Drawing.Font("CaslonAntiqueVL", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonPanelCreateAccount.ForeColor = System.Drawing.Color.SeaShell;
            this.buttonPanelCreateAccount.Location = new System.Drawing.Point(77, 387);
            this.buttonPanelCreateAccount.Margin = new System.Windows.Forms.Padding(0);
            this.buttonPanelCreateAccount.Name = "buttonPanelCreateAccount";
            this.buttonPanelCreateAccount.Size = new System.Drawing.Size(112, 22);
            this.buttonPanelCreateAccount.TabIndex = 21;
            this.buttonPanelCreateAccount.Text = "Create an account";
            this.buttonPanelCreateAccount.UseVisualStyleBackColor = false;
            this.buttonPanelCreateAccount.Click += new System.EventHandler(this.buttonPanelCreateAccount_Click);
            // 
            // lblVersion
            // 
            this.lblVersion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lblVersion.AutoSize = true;
            this.lblVersion.BackColor = System.Drawing.Color.Transparent;
            this.lblVersion.ForeColor = System.Drawing.Color.DarkOrange;
            this.lblVersion.Location = new System.Drawing.Point(616, 432);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(0, 13);
            this.lblVersion.TabIndex = 22;
            // 
            // bnConnectLocal
            // 
            this.bnConnectLocal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bnConnectLocal.BackColor = System.Drawing.Color.Transparent;
            this.bnConnectLocal.FlatAppearance.BorderSize = 0;
            this.bnConnectLocal.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bnConnectLocal.Font = new System.Drawing.Font("CaslonAntiqueVL", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bnConnectLocal.ForeColor = System.Drawing.Color.SeaShell;
            this.bnConnectLocal.Location = new System.Drawing.Point(104, 425);
            this.bnConnectLocal.Name = "bnConnectLocal";
            this.bnConnectLocal.Size = new System.Drawing.Size(55, 20);
            this.bnConnectLocal.TabIndex = 25;
            this.bnConnectLocal.Text = "LOCAL";
            this.bnConnectLocal.UseVisualStyleBackColor = false;
            this.bnConnectLocal.Visible = false;
            this.bnConnectLocal.Click += new System.EventHandler(this.bnConnectToLocal_Click);
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 25;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.AutoSize = true;
            this.label4.BackColor = System.Drawing.Color.Transparent;
            this.label4.Font = new System.Drawing.Font("CaslonAntiqueVL", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.SeaShell;
            this.label4.Location = new System.Drawing.Point(39, 175);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(62, 21);
            this.label4.TabIndex = 29;
            this.label4.Text = "Account:";
            // 
            // label8
            // 
            this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label8.AutoSize = true;
            this.label8.BackColor = System.Drawing.Color.Transparent;
            this.label8.Font = new System.Drawing.Font("CaslonAntiqueVL", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.ForeColor = System.Drawing.Color.SeaShell;
            this.label8.Location = new System.Drawing.Point(39, 240);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(66, 21);
            this.label8.TabIndex = 30;
            this.label8.Text = "Password:";
            // 
            // bnMinimise
            // 
            this.bnMinimise.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bnMinimise.BackColor = System.Drawing.Color.Transparent;
            this.bnMinimise.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.bnMinimise.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.bnMinimise.FlatAppearance.BorderSize = 0;
            this.bnMinimise.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bnMinimise.Font = new System.Drawing.Font("CaslonAntiqueVL", 7F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bnMinimise.ForeColor = System.Drawing.Color.White;
            this.bnMinimise.Location = new System.Drawing.Point(754, 2);
            this.bnMinimise.Margin = new System.Windows.Forms.Padding(0);
            this.bnMinimise.Name = "bnMinimise";
            this.bnMinimise.Size = new System.Drawing.Size(20, 20);
            this.bnMinimise.TabIndex = 31;
            this.bnMinimise.Text = "_";
            this.bnMinimise.UseVisualStyleBackColor = false;
            this.bnMinimise.Click += new System.EventHandler(this.bnMinimise_Click);
            // 
            // WarLauncher
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.BackgroundImage = global::Launcher.Properties.Resources.launcher_back;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.bnMinimise);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.lblDownloading);
            this.Controls.Add(this.bnConnectLocal);
            this.Controls.Add(this.lblVersion);
            this.Controls.Add(this.buttonPanelCreateAccount);
            this.Controls.Add(this.panelCreateAccount);
            this.Controls.Add(this.T_password);
            this.Controls.Add(this.bnClose);
            this.Controls.Add(this.lblConnection);
            this.Controls.Add(this.bnConnectToServer);
            this.Controls.Add(this.lblLauncherServer);
            this.Controls.Add(this.T_username);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(800, 450);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(800, 450);
            this.Name = "WarLauncher";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "WAR: Launcher";
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
        private System.Windows.Forms.DataGridViewTextBoxColumn RealmName;
        private System.Windows.Forms.DataGridViewTextBoxColumn Online;
        private System.Windows.Forms.DataGridViewTextBoxColumn Players;
        private System.Windows.Forms.DataGridViewTextBoxColumn Destruction;
        private System.Windows.Forms.DataGridViewTextBoxColumn Order;
        private System.Windows.Forms.Label lblLauncherServer;
        private System.Windows.Forms.Button bnConnectToServer;
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
        private Timer timer1;
        private Label lblDownloading;
        private Label label4;
        private Label label8;
        private Button bnMinimise;
    }
}

