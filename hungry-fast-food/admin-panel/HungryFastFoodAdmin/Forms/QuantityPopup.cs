// E:\hungryHub\hungry-fast-food\admin-panel\HungryFastFoodAdmin\Forms\QuantityPopup.cs

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HungryFastFoodAdmin.Forms
{
    public class QuantityPopup : Form
    {
        public int Quantity { get; private set; } = 1;

        private Label lblTitle;
        private Button btnMinus;
        private Label lblQty;
        private Button btnPlus;
        private Button btnOK;
        private Button btnCancel;

        public QuantityPopup(string productName)
        {
            this.Text = "Select Quantity";
            this.Size = new Size(300, 200);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.White;
            this.KeyPreview = true;

            // Draw rounded borders
            this.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var path = GetRoundedRectPath(new Rectangle(0, 0, this.Width - 1, this.Height - 1), 16);
                using (var brush = new SolidBrush(Color.White))
                {
                    g.FillPath(brush, path);
                }
                using (var pen = new Pen(Color.FromArgb(230, 57, 70), 2)) // Primary Red #E63946 border
                {
                    g.DrawPath(pen, path);
                }
            };

            // Keyboard support
            this.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else if (e.KeyCode == Keys.Escape)
                {
                    this.DialogResult = DialogResult.Cancel;
                    this.Close();
                }
            };

            lblTitle = new Label
            {
                Text = productName,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(230, 57, 70), // Primary Red
                Location = new Point(15, 20),
                Size = new Size(270, 30),
                TextAlign = ContentAlignment.MiddleCenter
            };

            btnMinus = new Button
            {
                Text = "−",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Size = new Size(40, 40),
                Location = new Point(60, 65),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(240, 240, 240),
                ForeColor = Color.FromArgb(53, 57, 59),
                Cursor = Cursors.Hand
            };
            btnMinus.FlatAppearance.BorderSize = 0;
            btnMinus.Click += (s, e) =>
            {
                if (Quantity > 1)
                {
                    Quantity--;
                    lblQty.Text = Quantity.ToString();
                }
            };

            lblQty = new Label
            {
                Text = "1",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Size = new Size(60, 40),
                Location = new Point(110, 65),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.FromArgb(53, 57, 59)
            };

            btnPlus = new Button
            {
                Text = "+",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Size = new Size(40, 40),
                Location = new Point(180, 65),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(230, 57, 70),
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnPlus.FlatAppearance.BorderSize = 0;
            btnPlus.Click += (s, e) =>
            {
                if (Quantity < 99)
                {
                    Quantity++;
                    lblQty.Text = Quantity.ToString();
                }
            };

            btnOK = new Button
            {
                Text = "Add to Order",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Size = new Size(120, 36),
                Location = new Point(25, 135),
                BackColor = Color.FromArgb(42, 157, 143),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                DialogResult = DialogResult.OK
            };
            btnOK.FlatAppearance.BorderSize = 0;
            btnOK.Click += (s, e) => { this.Close(); };

            btnCancel = new Button
            {
                Text = "Cancel",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Size = new Size(120, 36),
                Location = new Point(155, 135),
                BackColor = Color.FromArgb(220, 220, 220),
                ForeColor = Color.FromArgb(53, 57, 59),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                DialogResult = DialogResult.Cancel
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => { this.Close(); };

            this.Controls.Add(lblTitle);
            this.Controls.Add(btnMinus);
            this.Controls.Add(lblQty);
            this.Controls.Add(btnPlus);
            this.Controls.Add(btnOK);
            this.Controls.Add(btnCancel);
        }

        private GraphicsPath GetRoundedRectPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            int diameter = radius * 2;
            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
