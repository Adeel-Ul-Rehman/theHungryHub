using System;
using System.Drawing;
using System.Windows.Forms;
using HungryFastFoodAdmin.Services;

namespace HungryFastFoodAdmin.Forms
{
    public class ForgotPasswordForm : Form
    {
        private ApiService _apiService;
        private string _email;
        private TextBox txtOtp;
        private TextBox txtNewPassword;
        private TextBox txtConfirmPassword;
        private Button btnReset;
        private Button btnCancel;

        public ForgotPasswordForm(ApiService apiService, string email)
        {
            _apiService = apiService;
            _email = email;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Reset Password";
            this.Size = new Size(400, 350);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(250, 249, 246);

            Label lblInfo = new Label
            {
                Text = $"An OTP has been sent to {_email}.\nPlease enter it below along with your new password.",
                Location = new Point(30, 20),
                Size = new Size(320, 40),
                Font = new Font("Segoe UI", 9.5F),
                TextAlign = ContentAlignment.TopCenter
            };
            this.Controls.Add(lblInfo);

            Label lblOtp = new Label { Text = "OTP:", Location = new Point(30, 70), AutoSize = true, Font = new Font("Segoe UI", 9.5F) };
            txtOtp = new TextBox { Location = new Point(30, 95), Size = new Size(320, 25), Font = new Font("Segoe UI", 10) };
            this.Controls.Add(lblOtp);
            this.Controls.Add(txtOtp);

            Label lblNewPwd = new Label { Text = "New Password:", Location = new Point(30, 130), AutoSize = true, Font = new Font("Segoe UI", 9.5F) };
            txtNewPassword = new TextBox { Location = new Point(30, 155), Size = new Size(320, 25), Font = new Font("Segoe UI", 10), UseSystemPasswordChar = true };
            this.Controls.Add(lblNewPwd);
            this.Controls.Add(txtNewPassword);

            Label lblConfirmPwd = new Label { Text = "Confirm Password:", Location = new Point(30, 190), AutoSize = true, Font = new Font("Segoe UI", 9.5F) };
            txtConfirmPassword = new TextBox { Location = new Point(30, 215), Size = new Size(320, 25), Font = new Font("Segoe UI", 10), UseSystemPasswordChar = true };
            this.Controls.Add(lblConfirmPwd);
            this.Controls.Add(txtConfirmPassword);

            btnReset = new Button
            {
                Text = "Reset Password",
                Location = new Point(30, 260),
                Size = new Size(150, 35),
                BackColor = Color.FromArgb(253, 175, 38),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnReset.FlatAppearance.BorderSize = 0;
            btnReset.Click += BtnReset_Click;
            this.Controls.Add(btnReset);

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(200, 260),
                Size = new Size(150, 35),
                BackColor = Color.FromArgb(53, 57, 59),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => this.Close();
            this.Controls.Add(btnCancel);
        }

        private async void BtnReset_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtOtp.Text) || string.IsNullOrWhiteSpace(txtNewPassword.Text))
            {
                MessageBox.Show("Please fill all fields.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (txtNewPassword.Text != txtConfirmPassword.Text)
            {
                MessageBox.Show("Passwords do not match.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnReset.Enabled = false;
            btnReset.Text = "Resetting...";

            var result = await _apiService.ResetPassword(_email, txtOtp.Text, txtNewPassword.Text);

            if (result.Success)
            {
                MessageBox.Show("Password reset successfully! You can now login with your new password.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show(result.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnReset.Enabled = true;
                btnReset.Text = "Reset Password";
            }
        }
    }
}
