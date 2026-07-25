using System;
using System.Drawing;
using System.Windows.Forms;
using HungryFastFoodAdmin.Models;
using HungryFastFoodAdmin.Services;

namespace HungryFastFoodAdmin.Forms
{
    public class ReceiptPreviewDialog : Form
    {
        private readonly Order _order;
        private readonly PrintService _printService;

        private Panel topPanel;
        private Button btnShowBill;
        private Button btnShowKitchen;

        private Panel scrollPanel;
        private PictureBox pbReceipt;

        private Panel bottomPanel;
        private Button btnPrint;
        private Button btnClose;

        private Bitmap _billBmp;
        private Bitmap _kitchenBmp;
        private bool _isShowingBill = true;

        public ReceiptPreviewDialog(Order order)
        {
            _order = order;
            _printService = new PrintService();

            this.Text = $"Receipt Preview - Order #{_order.OrderNumber}";
            this.Size = new Size(380, 680);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;

            InitializeComponent();
            LoadReceiptImages();
            ShowBillReceipt();
        }

        private void InitializeComponent()
        {
            // Top Selection Bar
            topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.FromArgb(240, 238, 233)
            };

            btnShowBill = new Button
            {
                Text = "📋 Customer Bill",
                Location = new Point(10, 10),
                Size = new Size(160, 30),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnShowBill.FlatAppearance.BorderSize = 0;
            btnShowBill.Click += (s, e) => ShowBillReceipt();

            btnShowKitchen = new Button
            {
                Text = "🍳 Kitchen Slip",
                Location = new Point(180, 10),
                Size = new Size(160, 30),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnShowKitchen.FlatAppearance.BorderSize = 0;
            btnShowKitchen.Click += (s, e) => ShowKitchenReceipt();

            topPanel.Controls.Add(btnShowBill);
            topPanel.Controls.Add(btnShowKitchen);

            // Center Scroll Panel
            scrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.FromArgb(230, 228, 220),
                Padding = new Padding(15)
            };

            pbReceipt = new PictureBox
            {
                SizeMode = PictureBoxSizeMode.AutoSize,
                BackColor = Color.White
            };

            scrollPanel.Controls.Add(pbReceipt);
            scrollPanel.Resize += (s, e) => CenterReceiptPaper();

            // Bottom Actions Panel
            bottomPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = Color.FromArgb(240, 238, 233)
            };

            btnPrint = new Button
            {
                Text = "🖨️ Print Slip",
                Location = new Point(20, 14),
                Size = new Size(150, 32),
                BackColor = Color.FromArgb(42, 157, 143), // Teal
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnPrint.FlatAppearance.BorderSize = 0;
            btnPrint.Click += BtnPrint_Click;

            btnClose = new Button
            {
                Text = "Close",
                Location = new Point(190, 14),
                Size = new Size(150, 32),
                BackColor = Color.FromArgb(230, 57, 70), // Primary red
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                DialogResult = DialogResult.Cancel,
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;

            bottomPanel.Controls.Add(btnPrint);
            bottomPanel.Controls.Add(btnClose);

            this.Controls.Add(scrollPanel);
            this.Controls.Add(topPanel);
            this.Controls.Add(bottomPanel);
        }

        private void LoadReceiptImages()
        {
            try
            {
                _billBmp = _printService.GenerateBillBitmap(_order);
                _kitchenBmp = _printService.GenerateKitchenSlipBitmap(_order);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error rendering print layout: {ex.Message}", "Preview Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowBillReceipt()
        {
            _isShowingBill = true;
            pbReceipt.Image = _billBmp;

            btnShowBill.BackColor = Color.FromArgb(53, 57, 59); // Dark select
            btnShowBill.ForeColor = Color.White;
            btnShowKitchen.BackColor = Color.FromArgb(220, 218, 210);
            btnShowKitchen.ForeColor = Color.FromArgb(53, 57, 59);

            CenterReceiptPaper();
            scrollPanel.VerticalScroll.Value = 0;
        }

        private void ShowKitchenReceipt()
        {
            _isShowingBill = false;
            pbReceipt.Image = _kitchenBmp;

            btnShowKitchen.BackColor = Color.FromArgb(53, 57, 59);
            btnShowKitchen.ForeColor = Color.White;
            btnShowBill.BackColor = Color.FromArgb(220, 218, 210);
            btnShowBill.ForeColor = Color.FromArgb(53, 57, 59);

            CenterReceiptPaper();
            scrollPanel.VerticalScroll.Value = 0;
        }

        private void CenterReceiptPaper()
        {
            if (pbReceipt.Image == null) return;
            pbReceipt.Left = Math.Max(15, (scrollPanel.ClientSize.Width - pbReceipt.Width) / 2);
            pbReceipt.Top = 15;
        }

        private void BtnPrint_Click(object sender, EventArgs e)
        {
            try
            {
                if (_isShowingBill)
                {
                    _printService.PrintBill(_order);
                    MessageBox.Show("Customer Bill printed successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    _printService.PrintKitchenSlip(_order);
                    MessageBox.Show("Kitchen Slip printed successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to execute print command: {ex.Message}", "Print Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            _billBmp?.Dispose();
            _kitchenBmp?.Dispose();
        }
    }
}
