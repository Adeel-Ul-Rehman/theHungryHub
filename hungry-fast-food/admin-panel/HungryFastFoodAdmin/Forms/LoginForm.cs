// E:\hungryHub\hungry-fast-food\admin-panel\HungryFastFoodAdmin\Forms\LoginForm.cs

using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using HungryFastFoodAdmin.Services;

namespace HungryFastFoodAdmin.Forms
{
    public partial class LoginForm : Form
    {
        private TextBox txtEmail;
        private TextBox txtPassword;
        private Button btnLogin;
        private Button btnExit;
        private Label lblTitle;
        private Label lblSubtitle;
        private Label lblError;
        private CheckBox chkRemember;
        private PictureBox picLogo;
        private LinkLabel lnkForgotPassword;
        private ApiService apiService;

        public LoginForm()
        {
            apiService = new ApiService();
            InitializeComponent();
            SetupUI();
        }

        private void InitializeComponent()
        {
            this.txtEmail = new TextBox();
            this.txtPassword = new TextBox();
            this.btnLogin = new Button();
            this.btnExit = new Button();
            this.lblTitle = new Label();
            this.lblSubtitle = new Label();
            this.lblError = new Label();
            this.chkRemember = new CheckBox();
            this.lnkForgotPassword = new LinkLabel();
            this.picLogo = new PictureBox();
            
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).BeginInit();
            this.SuspendLayout();
            
            // 
            // picLogo
            // 
            this.picLogo.Location = new Point(145, 25);
            this.picLogo.Name = "picLogo";
            this.picLogo.Size = new Size(160, 110);
            this.picLogo.SizeMode = PictureBoxSizeMode.Zoom;
            this.picLogo.TabIndex = 0;
            this.picLogo.TabStop = false;
            
            // 
            // lblTitle
            // 
            this.lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            this.lblTitle.ForeColor = Color.FromArgb(53, 57, 59); // Secondary Text/Charcoal
            this.lblTitle.Location = new Point(50, 140);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new Size(350, 30);
            this.lblTitle.TabIndex = 1;
            this.lblTitle.Text = "HUNGRY HUB";
            this.lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            
            // 
            // lblSubtitle
            // 
            this.lblSubtitle.Font = new Font("Segoe UI", 9.5F, FontStyle.Italic);
            this.lblSubtitle.ForeColor = Color.FromArgb(108, 117, 125);
            this.lblSubtitle.Location = new Point(50, 170);
            this.lblSubtitle.Name = "lblSubtitle";
            this.lblSubtitle.Size = new Size(350, 20);
            this.lblSubtitle.TabIndex = 2;
            this.lblSubtitle.Text = "Administrative Dashboard Access Gate";
            this.lblSubtitle.TextAlign = ContentAlignment.MiddleCenter;

            // 
            // txtEmail
            // 
            this.txtEmail.BackColor = Color.White;
            this.txtEmail.Font = new Font("Segoe UI", 11F);
            this.txtEmail.ForeColor = Color.FromArgb(53, 57, 59);
            this.txtEmail.Location = new Point(50, 210);
            this.txtEmail.Name = "txtEmail";
            this.txtEmail.PlaceholderText = "Admin Email Address";
            this.txtEmail.Size = new Size(350, 27);
            this.txtEmail.TabIndex = 3;
            this.txtEmail.Text = "admin@hungryhub.com";
            this.txtEmail.KeyDown += new KeyEventHandler(this.txtEmail_KeyDown);
            
            // 
            // txtPassword
            // 
            this.txtPassword.BackColor = Color.White;
            this.txtPassword.Font = new Font("Segoe UI", 11F);
            this.txtPassword.ForeColor = Color.FromArgb(53, 57, 59);
            this.txtPassword.Location = new Point(50, 255);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PlaceholderText = "Password";
            this.txtPassword.Size = new Size(350, 27);
            this.txtPassword.TabIndex = 4;
            this.txtPassword.UseSystemPasswordChar = true;
            this.txtPassword.KeyDown += new KeyEventHandler(this.txtPassword_KeyDown);

            // 
            // chkRemember
            // 
            this.chkRemember.Font = new Font("Segoe UI", 9.5F);
            this.chkRemember.ForeColor = Color.FromArgb(53, 57, 59);
            this.chkRemember.Location = new Point(50, 295);
            this.chkRemember.Name = "chkRemember";
            this.chkRemember.Size = new Size(200, 25);
            this.chkRemember.TabIndex = 5;
            this.chkRemember.Text = "Remember Login Session";
            this.chkRemember.UseVisualStyleBackColor = true;

            // 
            // lblError
            // 
            this.lblError.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            this.lblError.ForeColor = Color.FromArgb(230, 57, 70);
            this.lblError.Location = new Point(50, 330);
            this.lblError.Name = "lblError";
            this.lblError.Size = new Size(350, 22);
            this.lblError.TabIndex = 6;
            this.lblError.TextAlign = ContentAlignment.MiddleCenter;

            // 
            // btnLogin
            // 
            this.btnLogin.BackColor = Color.FromArgb(253, 175, 38); // Hungry Hub primary orange
            this.btnLogin.FlatAppearance.BorderSize = 0;
            this.btnLogin.FlatAppearance.MouseDownBackColor = Color.FromArgb(229, 152, 7);
            this.btnLogin.FlatAppearance.MouseOverBackColor = Color.FromArgb(254, 190, 80);
            this.btnLogin.FlatStyle = FlatStyle.Flat;
            this.btnLogin.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            this.btnLogin.ForeColor = Color.White;
            this.btnLogin.Location = new Point(50, 365);
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Size = new Size(165, 40);
            this.btnLogin.TabIndex = 7;
            this.btnLogin.Text = "Sign In";
            this.btnLogin.UseVisualStyleBackColor = false;
            this.btnLogin.Click += new EventHandler(this.btnLogin_Click);
            this.AcceptButton = this.btnLogin;

            // 
            // btnExit
            // 
            this.btnExit.BackColor = Color.FromArgb(53, 57, 59); // Charcoal background
            this.btnExit.FlatAppearance.BorderSize = 0;
            this.btnExit.FlatAppearance.MouseDownBackColor = Color.FromArgb(40, 43, 45);
            this.btnExit.FlatAppearance.MouseOverBackColor = Color.FromArgb(70, 75, 78);
            this.btnExit.FlatStyle = FlatStyle.Flat;
            this.btnExit.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            this.btnExit.ForeColor = Color.White;
            this.btnExit.Location = new Point(235, 365);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new Size(165, 40);
            this.btnExit.TabIndex = 8;
            this.btnExit.Text = "Exit Client";
            this.btnExit.UseVisualStyleBackColor = false;
            this.btnExit.Click += new EventHandler(this.btnExit_Click);

            // 
            // lnkForgotPassword
            // 
            this.lnkForgotPassword.Font = new Font("Segoe UI", 9.5F);
            this.lnkForgotPassword.Location = new Point(260, 298);
            this.lnkForgotPassword.Name = "lnkForgotPassword";
            this.lnkForgotPassword.Size = new Size(140, 20);
            this.lnkForgotPassword.TabIndex = 6;
            this.lnkForgotPassword.Text = "Forgot Password?";
            this.lnkForgotPassword.TextAlign = ContentAlignment.MiddleRight;
            this.lnkForgotPassword.LinkClicked += new LinkLabelLinkClickedEventHandler(this.lnkForgotPassword_LinkClicked);

            // 
            // LoginForm
            // 
            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.BackColor = Color.FromArgb(250, 249, 246); // Warm background (#FAF9F6)
            this.ClientSize = new Size(450, 450);
            this.Controls.Add(this.picLogo);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.lblSubtitle);
            this.Controls.Add(this.txtEmail);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.chkRemember);
            this.Controls.Add(this.lnkForgotPassword);
            this.Controls.Add(this.lblError);
            this.Controls.Add(this.btnLogin);
            this.Controls.Add(this.btnExit);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "LoginForm";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Admin Login - Hungry Hub";
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void SetupUI()
        {
            // Set up logo image if it exists
            string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logo.png");
            if (File.Exists(logoPath))
            {
                try
                {
                    picLogo.Image = Image.FromFile(logoPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error loading logo image: " + ex.Message);
                }
            }

            // Load saved credentials from DB settings table
            var db = new DatabaseService();
            string rememberEmail = db.GetSetting("RememberEmail", "");
            if (!string.IsNullOrEmpty(rememberEmail))
            {
                txtEmail.Text = rememberEmail;
                chkRemember.Checked = true;
            }
        }

        private void txtEmail_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                btnLogin.PerformClick();
            }
        }

        private void txtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true; // Prevent beep sound
                btnLogin.PerformClick();
            }
        }

        private async void lnkForgotPassword_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtEmail.Text) || txtEmail.Text == "admin@hungryhub.com")
            {
                MessageBox.Show("Please enter your actual admin email address first.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            lnkForgotPassword.Enabled = false;
            lnkForgotPassword.Text = "Sending OTP...";

            var result = await apiService.ForgotPassword(txtEmail.Text);

            lnkForgotPassword.Enabled = true;
            lnkForgotPassword.Text = "Forgot Password?";

            if (result.Success)
            {
                using (var resetForm = new ForgotPasswordForm(apiService, txtEmail.Text))
                {
                    resetForm.ShowDialog();
                }
            }
            else
            {
                MessageBox.Show(result.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                btnLogin.Enabled = false;
                btnLogin.Text = "Authenticating...";
                lblError.Text = "";

                var result = await apiService.AdminLogin(txtEmail.Text, txtPassword.Text);

                if (result.Success)
                {
                    // Save credentials if remember me
                    var db = new DatabaseService();
                    if (chkRemember.Checked)
                    {
                        db.SaveSetting("RememberEmail", txtEmail.Text);
                    }
                    else
                    {
                        db.SaveSetting("RememberEmail", "");
                    }

                    // Open main form
                    var mainForm = new MainForm(result.Data);
                    mainForm.Show();
                    this.Hide();
                }
                else
                {
                    lblError.Text = result.Message;
                }
            }
            catch (Exception ex)
            {
                lblError.Text = $"Error: {ex.Message}";
            }
            finally
            {
                btnLogin.Enabled = true;
                btnLogin.Text = "Sign In";
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}