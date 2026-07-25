// E:\hungryHub\hungry-fast-food\admin-panel\HungryFastFoodAdmin\Controls\CategoryButton.cs

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HungryFastFoodAdmin.Controls
{
    public class CategoryButton : UserControl
    {
        // Properties
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public string CategoryId { get; set; }

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public string CategoryName { get; set; }

        private bool _isSelected;
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                this.Invalidate();
            }
        }

        // Custom Click Event passing category ID
        public new event EventHandler Click;

        private bool _isHovered;

        public CategoryButton()
        {
            InitializeComponent();
            SetupUI();
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.Transparent;
            this.Cursor = Cursors.Hand;
            this.DoubleBuffered = true;
            this.Font = new Font("Segoe UI", 11, FontStyle.Bold);
        }

        private void SetupUI()
        {
            this.MouseEnter += (s, e) => { _isHovered = true; this.Invalidate(); };
            this.MouseLeave += (s, e) => { _isHovered = false; this.Invalidate(); };
        }

        protected override void OnClick(EventArgs e)
        {
            Click?.Invoke(this, EventArgs.Empty);
            base.OnClick(e);
        }

        // WinForms Auto-sizing calculation based on text size
        public override Size GetPreferredSize(Size proposedSize)
        {
            using (var g = this.CreateGraphics())
            {
                var size = g.MeasureString(CategoryName ?? "Category", this.Font);
                int width = (int)size.Width + 48; // padding 24px each side
                int height = (int)size.Height + 24; // padding 12px each side
                return new Size(width, height);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int radius = 10;
            Rectangle rect = new Rectangle(0, 0, this.Width, this.Height);

            if (_isHovered && !IsSelected)
            {
                rect = new Rectangle(1, 1, this.Width - 2, this.Height - 2);
            }

            var path = GetRoundedRectPath(rect, radius);

            Color bg;
            Color fg;

            if (IsSelected)
            {
                bg = _isHovered ? Color.FromArgb(200, 40, 50) : Color.FromArgb(230, 57, 70); // Primary #E63946
                fg = Color.White;
            }
            else
            {
                bg = _isHovered ? Color.FromArgb(230, 230, 230) : Color.FromArgb(240, 240, 240); // Light Gray
                fg = Color.FromArgb(102, 102, 102); // #666
            }

            using (var brush = new SolidBrush(bg))
            {
                g.FillPath(brush, path);
            }

            TextRenderer.DrawText(g, CategoryName ?? "", this.Font, rect, fg, 
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        private GraphicsPath GetRoundedRectPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            int diameter = radius * 2;
            if (diameter > rect.Height) diameter = rect.Height;
            if (diameter > rect.Width) diameter = rect.Width;

            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
