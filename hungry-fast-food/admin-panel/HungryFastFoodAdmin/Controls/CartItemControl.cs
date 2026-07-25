// E:\hungryHub\hungry-fast-food\admin-panel\HungryFastFoodAdmin\Controls\CartItemControl.cs

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HungryFastFoodAdmin.Models;
using HungryFastFoodAdmin.Services;
using HungryFastFoodAdmin.Forms;

namespace HungryFastFoodAdmin.Controls
{
    public class CartItemControl : UserControl
    {
        public CartItem Item { get; }

        // Events
        public event EventHandler QuantityChanged;
        public event EventHandler ItemRemoved;

        // UI Controls
        private PictureBox picProduct;
        private Label lblName;
        private Label lblPrice;
        private FlowLayoutPanel flowQtyControls;
        private RoundedButton btnMinus;
        private Label lblQty;
        private RoundedButton btnPlus;
        private RoundedButton btnRemove;

        private Image _productImage;

        public CartItemControl(CartItem item)
        {
            this.Item = item;
            InitializeComponent();
            SetupUI();
            LoadImage();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(350, 60);
            this.BackColor = Color.White;
            this.DoubleBuffered = true;
        }

        private void SetupUI()
        {
            // PictureBox (left side, 40x40px, rounded corners)
            picProduct = new PictureBox
            {
                Size = new Size(40, 40),
                Location = new Point(10, 10),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(245, 245, 245)
            };
            picProduct.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = new Rectangle(0, 0, picProduct.Width, picProduct.Height);
                var path = GetRoundedRectPath(rect, 6);

                if (_productImage != null)
                {
                    g.SetClip(path);
                    g.DrawImage(_productImage, rect);
                    g.ResetClip();
                }
                else
                {
                    g.SetClip(path);
                    using (var brush = new LinearGradientBrush(
                        rect,
                        Color.FromArgb(244, 162, 97), // Secondary #F4A261
                        Color.FromArgb(230, 57, 70),  // Primary #E63946
                        LinearGradientMode.ForwardDiagonal))
                    {
                        g.FillRectangle(brush, rect);
                    }
                    g.ResetClip();

                    string letter = string.IsNullOrEmpty(Item.ProductName) ? "?" : Item.ProductName[0].ToString().ToUpper();
                    using (var font = new Font("Segoe UI", 13, FontStyle.Bold))
                    using (var brush = new SolidBrush(Color.White))
                    {
                        var size = g.MeasureString(letter, font);
                        g.DrawString(letter, font, brush, (picProduct.Width - size.Width) / 2, (picProduct.Height - size.Height) / 2);
                    }
                }
            };

            // Labels for Name & Price
            lblName = new Label
            {
                Text = Item.ProductName,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(53, 57, 59),
                Location = new Point(60, 10),
                Size = new Size(150, 20),
                AutoEllipsis = true
            };

            lblPrice = new Label
            {
                Text = $"PKR {Item.UnitPrice:F0} × {Item.Quantity} = PKR {Item.UnitPrice * Item.Quantity:F0}",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = Color.Gray,
                Location = new Point(60, 32),
                Size = new Size(150, 18)
            };

            // FlowLayoutPanel for Quantity Controls
            flowQtyControls = new FlowLayoutPanel
            {
                Size = new Size(125, 34),
                Location = new Point(215, 13),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Color.Transparent
            };

            btnMinus = new RoundedButton
            {
                Text = "−",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Size = new Size(24, 24),
                NormalBackColor = Color.FromArgb(240, 240, 240),
                NormalForeColor = Color.FromArgb(53, 57, 59),
                HoverBackColor = Color.FromArgb(220, 220, 220),
                Margin = new Padding(0, 3, 3, 0)
            };
            btnMinus.Click += BtnMinus_Click;

            lblQty = new Label
            {
                Text = Item.Quantity.ToString(),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Size = new Size(24, 24),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.FromArgb(53, 57, 59),
                Margin = new Padding(0, 3, 3, 0)
            };

            btnPlus = new RoundedButton
            {
                Text = "+",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Size = new Size(24, 24),
                NormalBackColor = Color.FromArgb(42, 157, 143), // Accent Green
                NormalForeColor = Color.White,
                HoverBackColor = Color.FromArgb(34, 128, 116),
                Margin = new Padding(0, 3, 3, 0)
            };
            btnPlus.Click += BtnPlus_Click;

            btnRemove = new RoundedButton
            {
                Text = "✕",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Size = new Size(24, 24),
                NormalBackColor = Color.FromArgb(250, 230, 230),
                NormalForeColor = Color.FromArgb(230, 57, 70), // Primary Red
                HoverBackColor = Color.FromArgb(230, 57, 70),
                Margin = new Padding(0, 3, 0, 0)
            };
            btnRemove.MouseEnter += (s, e) => btnRemove.NormalForeColor = Color.White;
            btnRemove.MouseLeave += (s, e) => btnRemove.NormalForeColor = Color.FromArgb(230, 57, 70);
            btnRemove.Click += BtnRemove_Click;

            flowQtyControls.Controls.Add(btnMinus);
            flowQtyControls.Controls.Add(lblQty);
            flowQtyControls.Controls.Add(btnPlus);
            flowQtyControls.Controls.Add(btnRemove);

            this.Controls.Add(picProduct);
            this.Controls.Add(lblName);
            this.Controls.Add(lblPrice);
            this.Controls.Add(flowQtyControls);
        }

        private void LoadImage()
        {
            System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    var db = new DatabaseService();
                    var prod = db.GetProductById(Item.ProductId);
                    if (prod != null && !string.IsNullOrEmpty(prod.ImageUrl))
                    {
                        Image img = null;
                        if (prod.ImageUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || 
                            prod.ImageUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                        {
                            using (var client = new System.Net.Http.HttpClient())
                            {
                                var data = client.GetByteArrayAsync(prod.ImageUrl).GetAwaiter().GetResult();
                                using (var ms = new System.IO.MemoryStream(data))
                                {
                                    img = Image.FromStream(ms);
                                }
                            }
                        }
                        else if (System.IO.File.Exists(prod.ImageUrl))
                        {
                            img = Image.FromFile(prod.ImageUrl);
                        }

                        if (img != null)
                        {
                            _productImage = img;
                            this.Invoke((MethodInvoker)delegate
                            {
                                picProduct.Invalidate();
                            });
                        }
                    }
                }
                catch { }
            });
        }

        private void BtnMinus_Click(object sender, EventArgs e)
        {
            if (Item.Quantity > 1)
            {
                Item.Quantity--;
                lblQty.Text = Item.Quantity.ToString();
                lblPrice.Text = $"PKR {Item.UnitPrice:F0} × {Item.Quantity} = PKR {Item.UnitPrice * Item.Quantity:F0}";
                QuantityChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void BtnPlus_Click(object sender, EventArgs e)
        {
            Item.Quantity++;
            lblQty.Text = Item.Quantity.ToString();
            lblPrice.Text = $"PKR {Item.UnitPrice:F0} × {Item.Quantity} = PKR {Item.UnitPrice * Item.Quantity:F0}";
            QuantityChanged?.Invoke(this, EventArgs.Empty);
        }

        private void BtnRemove_Click(object sender, EventArgs e)
        {
            ItemRemoved?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Draw rounded card border
            var path = GetRoundedRectPath(new Rectangle(0, 0, this.Width - 1, this.Height - 1), 8);
            using (var brush = new SolidBrush(Color.White))
            {
                g.FillPath(brush, path);
            }
            using (var pen = new Pen(Color.FromArgb(240, 240, 240), 1))
            {
                g.DrawPath(pen, path);
            }
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

    // ============================================
    // CIRCULAR/ROUNDED BUTTON CONTROL
    // ============================================
    public class RoundedButton : Button
    {
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public Color NormalBackColor { get; set; }

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public Color NormalForeColor { get; set; }

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public Color HoverBackColor { get; set; }

        private bool _isHovered;

        public RoundedButton()
        {
            this.FlatStyle = FlatStyle.Flat;
            this.FlatAppearance.BorderSize = 0;
            this.Cursor = Cursors.Hand;
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            _isHovered = true;
            this.Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _isHovered = false;
            this.Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            var g = pevent.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = new Rectangle(0, 0, this.Width, this.Height);
            var path = new GraphicsPath();
            path.AddEllipse(rect);

            Color backColor = _isHovered ? HoverBackColor : NormalBackColor;

            using (var brush = new SolidBrush(backColor))
            {
                g.FillPath(brush, path);
            }

            TextRenderer.DrawText(g, this.Text, this.Font, rect, NormalForeColor, 
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
    }
}
