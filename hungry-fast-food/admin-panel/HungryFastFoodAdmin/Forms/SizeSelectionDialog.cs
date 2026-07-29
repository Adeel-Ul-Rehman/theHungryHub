// E:\hungryHub\hungry-fast-food\admin-panel\HungryFastFoodAdmin\Forms\SizeSelectionDialog.cs

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HungryFastFoodAdmin.Models;

namespace HungryFastFoodAdmin.Forms
{
    public class SizeSelectionDialog : Form
    {
        public ProductVariation SelectedVariation { get; private set; }
        public bool IsExtraToppingSelected { get; private set; }
        private Product _product;
        private System.Collections.Generic.Dictionary<Button, ProductVariation> _buttonVariations = 
            new System.Collections.Generic.Dictionary<Button, ProductVariation>();

        public SizeSelectionDialog(Product product)
        {
            _product = product;
            InitializeComponent();
            SetupUI();
        }

        private bool IsPizzaProduct()
        {
            if (_product == null) return false;
            if (_product.Name.ToLower().Contains("pizza")) return true;
            if (!string.IsNullOrEmpty(_product.CategoryId))
            {
                try
                {
                    var db = new Services.DatabaseService();
                    var category = db.GetCategoryById(_product.CategoryId);
                    if (category != null)
                    {
                        return category.Name.ToLower().Contains("pizza") || category.Slug.ToLower().Contains("pizza");
                    }
                }
                catch { }
            }
            return false;
        }

        private void InitializeComponent()
        {
            bool isPizza = IsPizzaProduct();
            this.Size = new Size(400, isPizza ? 310 : 260);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.FromArgb(250, 249, 246);
        }

        private void SetupUI()
        {
            bool isPizza = IsPizzaProduct();
            this.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
                var path = GetRoundedRectPath(rect, 16);
                using (var brush = new SolidBrush(Color.White))
                {
                    g.FillPath(brush, path);
                }
                using (var pen = new Pen(Color.FromArgb(230, 57, 70), 2))
                {
                    g.DrawPath(pen, path);
                }
            };

            var lblTitle = new Label
            {
                Text = $"Select Size - {_product.Name}",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(53, 57, 59),
                Location = new Point(15, 20),
                Size = new Size(370, 30),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblTitle);

            var flowButtons = new FlowLayoutPanel
            {
                Location = new Point(20, 65),
                Size = new Size(360, 120),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                AutoScroll = true
            };
            this.Controls.Add(flowButtons);

            CheckBox chkExtraTopping = null;
            if (isPizza)
            {
                chkExtraTopping = new CheckBox
                {
                    Text = "Add Extra Topping (Cheese & Toppings)",
                    Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(230, 57, 70),
                    Location = new Point(25, 200),
                    Size = new Size(350, 30),
                    Cursor = Cursors.Hand
                };
                chkExtraTopping.CheckedChanged += (s, e) => UpdateButtonPrices(chkExtraTopping.Checked);
                this.Controls.Add(chkExtraTopping);
            }

            foreach (var v in _product.Variations)
            {
                var btn = new Button
                {
                    Text = $"{v.VariationName}\n(PKR {(_product.DiscountPrice ?? _product.BasePrice) + v.PriceAdjustment:F0})",
                    Size = new Size(160, 50),
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    BackColor = Color.FromArgb(240, 240, 240),
                    ForeColor = Color.FromArgb(53, 57, 59),
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.MouseEnter += (s, e) => { btn.BackColor = Color.FromArgb(230, 57, 70); btn.ForeColor = Color.White; };
                btn.MouseLeave += (s, e) => { btn.BackColor = Color.FromArgb(240, 240, 240); btn.ForeColor = Color.FromArgb(53, 57, 59); };
                
                btn.Click += (s, e) =>
                {
                    SelectedVariation = v;
                    IsExtraToppingSelected = chkExtraTopping != null && chkExtraTopping.Checked;
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                };
                
                _buttonVariations.Add(btn, v);
                flowButtons.Controls.Add(btn);
            }

            var btnCancel = new Button
            {
                Text = "Cancel",
                Size = new Size(120, 36),
                Location = new Point(140, isPizza ? 250 : 205),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                BackColor = Color.FromArgb(200, 200, 200),
                ForeColor = Color.FromArgb(53, 57, 59),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) =>
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            };
            this.Controls.Add(btnCancel);
        }

        private void UpdateButtonPrices(bool extraToppingChecked)
        {
            decimal basePrice = _product.DiscountPrice ?? _product.BasePrice;
            foreach (var kvp in _buttonVariations)
            {
                var btn = kvp.Key;
                var v = kvp.Value;
                decimal price = basePrice + v.PriceAdjustment;
                if (extraToppingChecked)
                {
                    price += GetToppingCost(v.VariationName);
                }
                btn.Text = $"{v.VariationName}\n(PKR {price:F0})";
            }
        }

        private decimal GetToppingCost(string varName)
        {
            if (string.IsNullOrEmpty(varName)) return 0;
            string name = varName.ToUpper();
            if (name == "S" || name == "SMALL") return 100;
            if (name == "M" || name == "MEDIUM") return 150;
            if (name == "L" || name == "LARGE") return 200;
            if (name == "XL" || name == "EXTRA LARGE") return 300;
            return 0;
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
