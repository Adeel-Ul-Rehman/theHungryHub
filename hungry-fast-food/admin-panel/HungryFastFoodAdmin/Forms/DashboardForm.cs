// E:\hungryHub\hungry-fast-food\admin-panel\HungryFastFoodAdmin\Forms\DashboardForm.cs

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using HungryFastFoodAdmin.Models;
using HungryFastFoodAdmin.Services;

namespace HungryFastFoodAdmin.Forms
{
    public class DashboardForm : BaseForm
    {
        private DatabaseService _dbService;

        private TableLayoutPanel cardsPanel;
        private Panel cardTotalOrders;
        private Panel cardTotalRevenue;
        private Panel cardPendingOrders;
        private Panel cardTodaysOrders;

        private Label lblTotalOrdersVal;
        private Label lblTotalRevenueVal;
        private Label lblPendingOrdersVal;
        private Label lblTodaysOrdersVal;

        private Label lblRecentOrdersTitle;
        private DataGridView dgvRecentOrders;

        private Label lblChartTitle;
        private Chart chartWeeklySales;

        public DashboardForm()
        {
            _dbService = new DatabaseService();
            InitializeComponent();
            SetupDashboardUI();
            LoadDashboardData();
        }

        private void InitializeComponent()
        {
            this.Text = "Dashboard";
            this.Size = new Size(1000, 700);
        }

        private void SetupDashboardUI()
        {
            // Title Header Label
            Label lblHeader = new Label
            {
                Text = "Dashboard Overview",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(53, 57, 59),
                Location = new Point(20, 15),
                Size = new Size(300, 35)
            };
            this.Controls.Add(lblHeader);

            // 1. STATS CARDS ROW (TableLayoutPanel for 4 columns)
            cardsPanel = new TableLayoutPanel
            {
                Location = new Point(20, 60),
                Size = new Size(960, 110),
                ColumnCount = 4,
                RowCount = 1,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            cardsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            cardsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            cardsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            cardsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            this.Controls.Add(cardsPanel);

            // Card 1: Total Orders (Blue)
            cardTotalOrders = CreateStatCard("Total Orders", "0", Color.FromArgb(52, 152, 219), out lblTotalOrdersVal);
            cardsPanel.Controls.Add(cardTotalOrders, 0, 0);

            // Card 2: Total Revenue (Green)
            cardTotalRevenue = CreateStatCard("Total Revenue", "PKR 0", Color.FromArgb(46, 204, 113), out lblTotalRevenueVal);
            cardsPanel.Controls.Add(cardTotalRevenue, 1, 0);

            // Card 3: Pending Orders (Orange)
            cardPendingOrders = CreateStatCard("Pending Orders", "0", Color.FromArgb(230, 126, 34), out lblPendingOrdersVal);
            cardsPanel.Controls.Add(cardPendingOrders, 2, 0);

            // Card 4: Today's Orders (Purple)
            cardTodaysOrders = CreateStatCard("Today's Orders", "0", Color.FromArgb(155, 89, 182), out lblTodaysOrdersVal);
            cardsPanel.Controls.Add(cardTodaysOrders, 3, 0);

            // 2. RECENT ORDERS GRID
            lblRecentOrdersTitle = new Label
            {
                Text = "📋 Recent Orders (Last 5)",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(53, 57, 59),
                Location = new Point(20, 190),
                Size = new Size(300, 25)
            };
            this.Controls.Add(lblRecentOrdersTitle);

            dgvRecentOrders = new DataGridView
            {
                Location = new Point(20, 225),
                Size = new Size(500, 280),
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            this.Controls.Add(dgvRecentOrders);

            // 3. WEEKLY SALES CHART
            lblChartTitle = new Label
            {
                Text = "📈 Weekly Sales Revenue (Last 7 Days)",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(53, 57, 59),
                Location = new Point(540, 190),
                Size = new Size(400, 25),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            this.Controls.Add(lblChartTitle);

            chartWeeklySales = new Chart
            {
                Location = new Point(540, 225),
                Size = new Size(440, 280),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            ChartArea chartArea = new ChartArea("SalesArea");
            chartArea.AxisX.MajorGrid.LineColor = Color.LightGray;
            chartArea.AxisY.MajorGrid.LineColor = Color.LightGray;
            chartWeeklySales.ChartAreas.Add(chartArea);

            Series series = new Series("Revenue")
            {
                ChartType = SeriesChartType.Column,
                Color = Color.FromArgb(42, 157, 143), // Accent Teal
                XValueType = ChartValueType.String
            };
            chartWeeklySales.Series.Add(series);

            this.Controls.Add(chartWeeklySales);

            // Hook resize to keep proportions clean
            this.Resize += (s, e) =>
            {
                int middleWidth = (this.Width - 60) / 2;
                dgvRecentOrders.Width = middleWidth;
                lblChartTitle.Left = dgvRecentOrders.Right + 20;
                chartWeeklySales.Left = dgvRecentOrders.Right + 20;
                chartWeeklySales.Width = this.Width - dgvRecentOrders.Right - 40;
            };
        }

        private Panel CreateStatCard(string title, string defaultVal, Color color, out Label valLabel)
        {
            Panel card = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(8),
                BackColor = Color.White
            };

            // Rounded corners on Paint
            card.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = new Rectangle(0, 0, card.Width - 1, card.Height - 1);
                var path = new GraphicsPath();
                int radius = 12;
                path.AddArc(rect.X, rect.Y, radius * 2, radius * 2, 180, 90);
                path.AddArc(rect.Right - radius * 2, rect.Y, radius * 2, radius * 2, 270, 90);
                path.AddArc(rect.Right - radius * 2, rect.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
                path.AddArc(rect.X, rect.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
                path.CloseFigure();
                
                using (var brush = new SolidBrush(Color.White))
                {
                    g.FillPath(brush, path);
                }
                using (var pen = new Pen(Color.FromArgb(235, 235, 235), 1))
                {
                    g.DrawPath(pen, path);
                }
                // Left border strip in solid stat color
                using (var stripBrush = new SolidBrush(color))
                {
                    g.FillRectangle(stripBrush, 0, 0, 6, card.Height);
                }
            };

            Label lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(107, 114, 128), // Gray text
                Location = new Point(16, 15),
                Size = new Size(180, 20)
            };

            valLabel = new Label
            {
                Text = defaultVal,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(53, 57, 59),
                Location = new Point(16, 40),
                Size = new Size(180, 35)
            };

            card.Controls.Add(lblTitle);
            card.Controls.Add(valLabel);

            return card;
        }

        private void LoadDashboardData()
        {
            try
            {
                // 1. Load Stats
                var stats = _dbService.GetDetailedDashboardStats();
                lblTotalOrdersVal.Text = stats["TotalOrders"].ToString();
                lblTotalRevenueVal.Text = $"PKR {Convert.ToDecimal(stats["TotalRevenue"]):F0}";
                lblPendingOrdersVal.Text = stats["PendingOrders"].ToString();
                lblTodaysOrdersVal.Text = stats["TodaysOrders"].ToString();

                // 2. Load Recent Orders
                var orders = _dbService.GetOrders();
                var recent = orders.OrderByDescending(o => o.CreatedAt).Take(5).Select(o => new
                {
                    OrderNo = o.OrderNumber,
                    Customer = o.CustomerName,
                    Total = $"PKR {o.Total:F0}",
                    Status = o.Status.ToUpper(),
                    Date = o.CreatedAt
                }).ToList();

                dgvRecentOrders.DataSource = recent;

                // 3. Load Weekly Sales Chart
                string start = DateTime.Now.AddDays(-6).ToString("yyyy-MM-dd");
                string end = DateTime.Now.ToString("yyyy-MM-dd");
                var sales = _dbService.GetDailyReportData(start, end);

                chartWeeklySales.Series["Revenue"].Points.Clear();
                // Ensure all 7 days have a point even if 0
                for (int i = -6; i <= 0; i++)
                {
                    string dateStr = DateTime.Now.AddDays(i).ToString("yyyy-MM-dd");
                    decimal val = sales.ContainsKey(dateStr) ? sales[dateStr] : 0m;
                    string label = DateTime.Now.AddDays(i).ToString("ddd"); // Mon, Tue, etc.
                    chartWeeklySales.Series["Revenue"].Points.AddXY(label, (double)val);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading dashboard data: {ex.Message}");
            }
        }
    }
}
