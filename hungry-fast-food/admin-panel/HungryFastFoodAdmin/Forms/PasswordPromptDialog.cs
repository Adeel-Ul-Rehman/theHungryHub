using System;
using System.Drawing;
using System.Windows.Forms;

namespace HungryFastFoodAdmin.Forms
{
    public class PasswordPromptDialog : Form
    {
        private TextBox txtPassword;
        private Button btnOk;
        private Button btnCancel;

        public string Password => txtPassword.Text;

        public PasswordPromptDialog(string promptText = "Enter admin password to confirm:")
        {
            this.Text = "Security Verification";
            this.Size = new Size(350, 170);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;

            Label lblPrompt = new Label
            {
                Text = promptText,
                Location = new Point(20, 15),
                Size = new Size(300, 20),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(53, 57, 59) // ColorDark
            };

            txtPassword = new TextBox
            {
                Location = new Point(20, 45),
                Size = new Size(290, 25),
                Font = new Font("Segoe UI", 10F),
                UseSystemPasswordChar = true
            };

            btnOk = new Button
            {
                Text = "Verify",
                Location = new Point(120, 85),
                Size = new Size(85, 32),
                BackColor = Color.FromArgb(42, 157, 143), // ColorTeal
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                DialogResult = DialogResult.OK
            };
            btnOk.FlatAppearance.BorderSize = 0;

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(220, 85),
                Size = new Size(85, 32),
                BackColor = Color.FromArgb(230, 57, 70), // ColorPrimary
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                DialogResult = DialogResult.Cancel
            };
            btnCancel.FlatAppearance.BorderSize = 0;

            this.Controls.AddRange(new Control[] { lblPrompt, txtPassword, btnOk, btnCancel });
            this.AcceptButton = btnOk;
            this.CancelButton = btnCancel;
        }
    }
}
