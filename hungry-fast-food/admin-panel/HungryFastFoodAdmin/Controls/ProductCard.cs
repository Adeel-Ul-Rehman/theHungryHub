// E:\hungryHub\hungry-fast-food\admin-panel\HungryFastFoodAdmin\Controls\ProductCard.cs

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HungryFastFoodAdmin.Models;

namespace HungryFastFoodAdmin.Controls
{
    public class ProductCard : UserControl
    {
        // Properties
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public string ProductId { get; set; }

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public new string ProductName { get; set; }

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public decimal Price { get; set; }

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public string ImageUrl { get; set; }

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public Product ProductData { get; set; }

        // Events
        public event EventHandler AddClicked;

        private bool _isHovered;
        private Image _productImage;

        public ProductCard(Product product)
        {
            this.ProductData = product;
            this.ProductId = product.Id;
            this.ProductName = product.Name;
            this.Price = product.DiscountPrice ?? product.BasePrice;
            this.ImageUrl = product.ImageUrl;

            InitializeComponent();
            SetupUI();
            LoadImage();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(180, 250);
            this.BackColor = Color.White;
            this.Cursor = Cursors.Hand;
            this.DoubleBuffered = true;
        }

        private void SetupUI()
        {
            this.MouseEnter += (s, e) => { _isHovered = true; this.Invalidate(); };
            this.MouseLeave += (s, e) =>
            {
                Point clientMouse = this.PointToClient(Cursor.Position);
                if (!this.ClientRectangle.Contains(clientMouse))
                {
                    _isHovered = false;
                    this.Invalidate();
                }
            };
        }

        private void LoadImage()
        {
            if (!string.IsNullOrEmpty(ImageUrl))
            {
                System.Threading.Tasks.Task.Run(() =>
                {
                    try
                    {
                        Image img = null;
                        if (ImageUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || 
                            ImageUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                        {
                            using (var client = new System.Net.Http.HttpClient())
                            {
                                var data = client.GetByteArrayAsync(ImageUrl).GetAwaiter().GetResult();
                                using (var ms = new System.IO.MemoryStream(data))
                                {
                                    img = Image.FromStream(ms);
                                }
                            }
                        }
                        else if (System.IO.File.Exists(ImageUrl))
                        {
                            img = Image.FromFile(ImageUrl);
                        }

                        if (img != null)
                        {
                            this.Invoke((MethodInvoker)delegate
                            {
                                _productImage = img;
                                this.Invalidate();
                            });
                        }
                    }
                    catch { }
                });
            }
        }

        protected override void OnClick(EventArgs e)
        {
            AddClicked?.Invoke(this, EventArgs.Empty);
            base.OnClick(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int radius = 12;
            Rectangle rect = new Rectangle(0, 0, this.Width, this.Height);

            if (_isHovered)
            {
                // Shadow draw
                using (var shadowBrush = new SolidBrush(Color.FromArgb(30, 0, 0, 0)))
                {
                    g.FillPath(shadowBrush, GetRoundedRectPath(new Rectangle(2, 2, this.Width - 4, this.Height - 4), radius));
                }
                rect = new Rectangle(1, 1, this.Width - 2, this.Height - 2);
            }
            else
            {
                // Normal shadow
                using (var shadowBrush = new SolidBrush(Color.FromArgb(15, 0, 0, 0)))
                {
                    g.FillPath(shadowBrush, GetRoundedRectPath(new Rectangle(1, 1, this.Width - 2, this.Height - 2), radius));
                }
            }

            // Draw Background
            var cardPath = GetRoundedRectPath(new Rectangle(0, 0, rect.Width, rect.Height), radius);
            using (var bgBrush = new SolidBrush(Color.White))
            {
                g.FillPath(bgBrush, cardPath);
            }

            // Product Image (Height 120px)
            Rectangle imgRect = new Rectangle(0, 0, rect.Width, 120);
            if (_productImage != null)
            {
                g.SetClip(GetRoundedTopRectPath(imgRect, radius));
                g.DrawImage(_productImage, imgRect);
                g.ResetClip();
            }
            else
            {
                g.SetClip(GetRoundedTopRectPath(imgRect, radius));
                using (var linGrBrush = new LinearGradientBrush(
                   imgRect,
                   Color.FromArgb(244, 162, 97), // Secondary #F4A261
                   Color.FromArgb(230, 57, 70),  // Primary #E63946
                   LinearGradientMode.ForwardDiagonal))
                {
                    g.FillRectangle(linGrBrush, imgRect);
                }
                g.ResetClip();

                string letter = string.IsNullOrEmpty(ProductName) ? "?" : ProductName[0].ToString().ToUpper();
                using (var font = new Font("Segoe UI", 32, FontStyle.Bold))
                using (var brush = new SolidBrush(Color.White))
                {
                    var size = g.MeasureString(letter, font);
                    g.DrawString(letter, font, brush, (rect.Width - size.Width) / 2, (120 - size.Height) / 2);
                }
            }

            // Product Name
            using (var font = new Font("Segoe UI", 10, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.FromArgb(53, 57, 59)))
            {
                var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                g.DrawString(ProductName, font, brush, new RectangleF(5, 130, rect.Width - 10, 40), sf);
            }

            // Product Price
            string priceText = $"PKR {Price:F0}";
            using (var font = new Font("Segoe UI", 11, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.FromArgb(230, 57, 70)))
            {
                var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                g.DrawString(priceText, font, brush, new RectangleF(5, 170, rect.Width - 10, 25), sf);
            }

            // Add Button
            Rectangle btnRect = new Rectangle(20, 205, rect.Width - 40, 32);
            var btnPath = GetRoundedRectPath(btnRect, 8);
            using (var btnBrush = new SolidBrush(_isHovered ? Color.FromArgb(244, 162, 97) : Color.FromArgb(230, 57, 70)))
            {
                g.FillPath(btnBrush, btnPath);
            }

            using (var font = new Font("Segoe UI", 9.5f, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.White))
            {
                var size = g.MeasureString("Add to Order", font);
                g.DrawString("Add to Order", font, brush, btnRect.X + (btnRect.Width - size.Width) / 2, btnRect.Y + (btnRect.Height - size.Height) / 2);
            }

            // Outer Border
            using (var pen = new Pen(Color.FromArgb(240, 240, 240), 1))
            {
                g.DrawPath(pen, cardPath);
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

        private GraphicsPath GetRoundedTopRectPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            int diameter = radius * 2;
            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddLine(rect.Right, rect.Bottom, rect.X, rect.Bottom);
            path.CloseFigure();
            return path;
        }
    }
}
