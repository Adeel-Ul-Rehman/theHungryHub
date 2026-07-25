// E:\hungryHub\hungry-fast-food\admin-panel\HungryFastFoodAdmin\Forms\ReportsForm.cs

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using HungryFastFoodAdmin.Models;
using HungryFastFoodAdmin.Services;

namespace HungryFastFoodAdmin.Forms
{
    public partial class ReportsForm : BaseForm
    {
        private readonly DatabaseService _dbService;

        // Filters
        private ComboBox cmbReportType;
        private ComboBox cmbOrderType;
        private ComboBox cmbChartType;
        private ComboBox cmbDataType;
        private DateTimePicker dtpStartDate;
        private DateTimePicker dtpEndDate;
        private Button btnGenerate;
        private Button btnExport;
        private Button btnRefresh;

        // Summary Cards
        private Label lblTotalOrders;
        private Label lblTotalRevenue;
        private Label lblPendingOrders;
        private Label lblCompletedOrders;
        private Label lblCancelledOrders;

        // Chart
        private Chart chartMain;
        private DataGridView dgvOrderList;
        private Panel chartContainer;
        private Panel chartControlPanel;
        private List<Order> _currentOrders = new List<Order>();

        public ReportsForm()
        {
            _dbService = new DatabaseService();
            InitializeComponent();
            SetupUI();
            LoadDefaultReport();
        }

        private void InitializeComponent()
        {
            this.Text = "📊 Reports & Analytics - Hungry Hub";
            this.Size = new Size(1200, 800);
            this.MinimumSize = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(248, 248, 248);

            Panel mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(15, 10, 15, 10),
                BackColor = Color.FromArgb(248, 248, 248),
                AutoScroll = true
            };
            this.Controls.Add(mainPanel);

            // ============================================
            // 1. SUMMARY CARDS - TOP
            // ============================================
            Panel summaryPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 75,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 6, 0, 6)
            };

            Color[] cardColors = new Color[]
            {
                Color.FromArgb(52, 152, 219),
                Color.FromArgb(46, 204, 113),
                Color.FromArgb(241, 196, 15),
                Color.FromArgb(46, 204, 113),
                Color.FromArgb(231, 76, 60)
            };

            string[] cardTitles = new string[]
            {
                "📦 Total Orders",
                "💰 Total Revenue",
                "⏳ Pending",
                "✅ Completed",
                "❌ Cancelled"
            };

            string[] cardSubtitles = new string[]
            {
                "Total orders in range",
                "Completed sales revenue",
                "Awaiting action",
                "Successfully delivered",
                "Cancelled orders"
            };

            int cardWidth = 190;
            int cardHeight = 62;
            int spacingCards = 8;
            int startX = 5;

            lblTotalOrders = CreateSummaryCard(cardTitles[0], "0", cardSubtitles[0], new Point(startX, 4), cardColors[0], summaryPanel, cardWidth, cardHeight);
            startX += cardWidth + spacingCards;
            lblTotalRevenue = CreateSummaryCard(cardTitles[1], "PKR 0", cardSubtitles[1], new Point(startX, 4), cardColors[1], summaryPanel, cardWidth, cardHeight);
            startX += cardWidth + spacingCards;
            lblPendingOrders = CreateSummaryCard(cardTitles[2], "0", cardSubtitles[2], new Point(startX, 4), cardColors[2], summaryPanel, cardWidth, cardHeight);
            startX += cardWidth + spacingCards;
            lblCompletedOrders = CreateSummaryCard(cardTitles[3], "0", cardSubtitles[3], new Point(startX, 4), cardColors[3], summaryPanel, cardWidth, cardHeight);
            startX += cardWidth + spacingCards;
            lblCancelledOrders = CreateSummaryCard(cardTitles[4], "0", cardSubtitles[4], new Point(startX, 4), cardColors[4], summaryPanel, cardWidth, cardHeight);

            mainPanel.Controls.Add(summaryPanel);

            // ============================================
            // 2. FILTERS
            // ============================================
            Panel filterRow1 = new Panel
            {
                Dock = DockStyle.Top,
                Height = 42,
                BackColor = Color.White,
                Padding = new Padding(12, 6, 12, 6)
            };

            filterRow1.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(220, 220, 220), 1))
                {
                    e.Graphics.DrawLine(pen, 0, filterRow1.Height - 1, filterRow1.Width, filterRow1.Height - 1);
                }
            };

            int yPos = 4;
            int xPos = 5;
            int spacing = 6;

            // Period
            Label lblPeriod = new Label
            {
                Text = "Period:",
                Location = new Point(xPos, yPos + 1),
                Size = new Size(48, 20),
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = Color.FromArgb(53, 57, 59)
            };

            cmbReportType = new ComboBox
            {
                Location = new Point(xPos + 50, yPos),
                Size = new Size(100, 24),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 8),
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            cmbReportType.Items.AddRange(new object[] { "Daily", "Weekly", "Monthly", "Yearly", "Custom" });
            cmbReportType.SelectedIndex = 0;
            cmbReportType.SelectedIndexChanged += CmbReportType_SelectedIndexChanged;

            xPos += 50 + 100 + spacing;

            // From
            Label lblFrom = new Label
            {
                Text = "From:",
                Location = new Point(xPos, yPos + 1),
                Size = new Size(32, 20),
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = Color.FromArgb(53, 57, 59)
            };

            dtpStartDate = new DateTimePicker
            {
                Location = new Point(xPos + 34, yPos),
                Size = new Size(100, 24),
                Format = DateTimePickerFormat.Short,
                Font = new Font("Segoe UI", 8)
            };

            xPos += 34 + 100 + 4;

            // To
            Label lblTo = new Label
            {
                Text = "To:",
                Location = new Point(xPos, yPos + 1),
                Size = new Size(24, 20),
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = Color.FromArgb(53, 57, 59)
            };

            dtpEndDate = new DateTimePicker
            {
                Location = new Point(xPos + 26, yPos),
                Size = new Size(100, 24),
                Format = DateTimePickerFormat.Short,
                Font = new Font("Segoe UI", 8)
            };

            xPos += 26 + 100 + spacing;

            // Channel
            Label lblChannel = new Label
            {
                Text = "Channel:",
                Location = new Point(xPos, yPos + 1),
                Size = new Size(52, 20),
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = Color.FromArgb(53, 57, 59)
            };

            cmbOrderType = new ComboBox
            {
                Location = new Point(xPos + 54, yPos),
                Size = new Size(105, 24),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 8),
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            cmbOrderType.Items.AddRange(new object[] { "All Channels", "Dining", "Delivery", "Takeaway" });
            cmbOrderType.SelectedIndex = 0;
            cmbOrderType.SelectedIndexChanged += (s, e) => LoadReport();

            filterRow1.Controls.Add(lblPeriod);
            filterRow1.Controls.Add(cmbReportType);
            filterRow1.Controls.Add(lblFrom);
            filterRow1.Controls.Add(dtpStartDate);
            filterRow1.Controls.Add(lblTo);
            filterRow1.Controls.Add(dtpEndDate);
            filterRow1.Controls.Add(lblChannel);
            filterRow1.Controls.Add(cmbOrderType);

            mainPanel.Controls.Add(filterRow1);

            // ============================================
            // 3. BUTTONS (3 Buttons Only)
            // ============================================
            Panel filterRow2 = new Panel
            {
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = Color.White,
                Padding = new Padding(12, 4, 12, 4)
            };

            filterRow2.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(220, 220, 220), 1))
                {
                    e.Graphics.DrawLine(pen, 0, filterRow2.Height - 1, filterRow2.Width, filterRow2.Height - 1);
                }
            };

            int btnY = 4;

            // Generate Button - Green
            btnGenerate = new Button
            {
                Text = "📊 Generate",
                Size = new Size(105, 28),
                BackColor = Color.FromArgb(42, 157, 143),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Location = new Point(5, btnY)
            };
            btnGenerate.FlatAppearance.BorderSize = 0;
            btnGenerate.Click += BtnGenerate_Click;

            // Refresh Button - Blue
            btnRefresh = new Button
            {
                Text = "🔄 Refresh",
                Size = new Size(90, 28),
                BackColor = Color.FromArgb(53, 57, 59),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Location = new Point(115, btnY)
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += (s, e) => LoadReport();

            // Export Button - Orange (Single Export)
            btnExport = new Button
            {
                Text = "📄 Export Report",
                Size = new Size(110, 28),
                BackColor = Color.FromArgb(244, 162, 97),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Location = new Point(210, btnY)
            };
            btnExport.FlatAppearance.BorderSize = 0;
            btnExport.Click += BtnExport_Click;

            filterRow2.Controls.Add(btnGenerate);
            filterRow2.Controls.Add(btnRefresh);
            filterRow2.Controls.Add(btnExport);

            mainPanel.Controls.Add(filterRow2);

            // ============================================
            // 4. REPORT ANALYTICS HEADER
            // ============================================
            Panel reportHeaderPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 35,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 8, 0, 4)
            };

            Label lblReportHeader = new Label
            {
                Text = "📈 Report Analytics",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(53, 57, 59),
                Location = new Point(5, 5),
                AutoSize = true
            };

            reportHeaderPanel.Controls.Add(lblReportHeader);
            mainPanel.Controls.Add(reportHeaderPanel);

            // ============================================
            // 5. GRAPH CONTAINER WITH DROPDOWNS
            // ============================================
            chartControlPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 35,
                BackColor = Color.White,
                Padding = new Padding(12, 4, 12, 4)
            };

            chartControlPanel.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(220, 220, 220), 1))
                {
                    e.Graphics.DrawLine(pen, 0, chartControlPanel.Height - 1, chartControlPanel.Width, chartControlPanel.Height - 1);
                }
            };

            int ctrlY = 4;
            int ctrlX = 5;

            Label lblChartType = new Label
            {
                Text = "Chart Type:",
                Location = new Point(ctrlX, ctrlY + 1),
                Size = new Size(70, 20),
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = Color.FromArgb(53, 57, 59)
            };

            cmbChartType = new ComboBox
            {
                Location = new Point(ctrlX + 75, ctrlY),
                Size = new Size(120, 24),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 8),
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            cmbChartType.Items.AddRange(new object[] { "Column Chart", "Line Chart", "Pie Chart", "Doughnut Chart", "Bar Chart" });
            cmbChartType.SelectedIndex = 0;
            cmbChartType.SelectedIndexChanged += CmbChartType_SelectedIndexChanged;

            ctrlX += 75 + 120 + 20;

            Label lblDataType = new Label
            {
                Text = "Data Type:",
                Location = new Point(ctrlX, ctrlY + 1),
                Size = new Size(65, 20),
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = Color.FromArgb(53, 57, 59)
            };

            cmbDataType = new ComboBox
            {
                Location = new Point(ctrlX + 70, ctrlY),
                Size = new Size(130, 24),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 8),
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            cmbDataType.Items.AddRange(new object[] { "Revenue", "Orders", "Order Type Mix", "Payment Mix" });
            cmbDataType.SelectedIndex = 0;
            cmbDataType.SelectedIndexChanged += CmbDataType_SelectedIndexChanged;

            chartControlPanel.Controls.Add(lblChartType);
            chartControlPanel.Controls.Add(cmbChartType);
            chartControlPanel.Controls.Add(lblDataType);
            chartControlPanel.Controls.Add(cmbDataType);

            mainPanel.Controls.Add(chartControlPanel);

            // Chart Container
            chartContainer = new Panel
            {
                Dock = DockStyle.Top,
                Height = 320,
                BackColor = Color.White,
                Padding = new Padding(8),
                Margin = new Padding(0, 0, 0, 5)
            };

            chartContainer.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(220, 220, 220), 1))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, chartContainer.Width - 1, chartContainer.Height - 1);
                }
            };

            chartContainer.Controls.Add(CreateMainChart());
            mainPanel.Controls.Add(chartContainer);

            // ============================================
            // ORDER DETAILS (Bottom)
            // ============================================
            Panel orderPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(8),
                Margin = new Padding(0, 5, 0, 0)
            };

            orderPanel.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(220, 220, 220), 1))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, orderPanel.Width - 1, orderPanel.Height - 1);
                }
            };

            Label lblOrderTitle = new Label
            {
                Text = "📋 Recent Orders",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(53, 57, 59),
                Dock = DockStyle.Top,
                Height = 25
            };

            orderPanel.Controls.Add(lblOrderTitle);
            orderPanel.Controls.Add(CreateOrderDetailsPanel());

            mainPanel.Controls.Add(orderPanel);
        }

        private Label CreateSummaryCard(string title, string value, string subtitle, Point location, Color color, Panel parent, int width, int height)
        {
            Panel card = new Panel
            {
                Location = location,
                Size = new Size(width, height),
                BackColor = Color.White,
                Padding = new Padding(3)
            };

            card.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var pen = new Pen(Color.FromArgb(220, 220, 220), 1))
                {
                    var rect = new Rectangle(0, 0, card.Width - 1, card.Height - 1);
                    g.DrawRectangle(pen, rect);
                }
                using (var pen = new Pen(color, 3))
                {
                    g.DrawLine(pen, 0, 0, 0, card.Height);
                }
            };

            Label lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 8, FontStyle.Regular),
                ForeColor = Color.FromArgb(100, 100, 100),
                Location = new Point(8, 3),
                Size = new Size(width - 20, 14)
            };

            Label lblValue = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = color,
                Location = new Point(8, 20),
                Size = new Size(width - 20, 24),
                TextAlign = ContentAlignment.MiddleLeft
            };

            Label lblSubtitle = new Label
            {
                Text = subtitle,
                Font = new Font("Segoe UI", 7),
                ForeColor = Color.FromArgb(148, 163, 184),
                Location = new Point(8, 44),
                Size = new Size(width - 20, 12),
                TextAlign = ContentAlignment.MiddleLeft
            };

            card.Controls.Add(lblTitle);
            card.Controls.Add(lblValue);
            card.Controls.Add(lblSubtitle);
            parent.Controls.Add(card);

            return lblValue;
        }

        private Chart CreateMainChart()
        {
            chartMain = new Chart
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Name = "chartMain"
            };

            ChartArea chartArea = new ChartArea
            {
                Name = "MainChart",
                BackColor = Color.White,
                AxisX = {
                    Title = "Date",
                    TitleFont = new Font("Segoe UI", 8, FontStyle.Bold),
                    LabelStyle = { Font = new Font("Segoe UI", 7), Angle = -30 }
                },
                AxisY = {
                    Title = "Value",
                    TitleFont = new Font("Segoe UI", 8, FontStyle.Bold),
                    LabelStyle = { Font = new Font("Segoe UI", 7) }
                }
            };
            chartMain.ChartAreas.Add(chartArea);

            Series series = new Series
            {
                Name = "Data",
                ChartType = SeriesChartType.Column,
                Color = Color.FromArgb(42, 157, 143),
                IsValueShownAsLabel = true,
                LabelFormat = "{0:N0}",
                Font = new Font("Segoe UI", 7, FontStyle.Bold)
            };
            chartMain.Series.Add(series);

            chartMain.Titles.Add(new Title
            {
                Text = "Revenue Trend",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(53, 57, 59),
                Docking = Docking.Top
            });

            return chartMain;
        }

        private DataGridView CreateOrderDetailsPanel()
        {
            dgvOrderList = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                Font = new Font("Segoe UI", 8),
                AlternatingRowsDefaultCellStyle = { BackColor = Color.FromArgb(248, 248, 248) },
                GridColor = Color.FromArgb(230, 230, 230),
                Name = "dgvOrderDetails"
            };

            dgvOrderList.Columns.Add("OrderNo", "Order #");
            dgvOrderList.Columns.Add("Type", "Type");
            dgvOrderList.Columns.Add("Customer", "Customer");
            dgvOrderList.Columns.Add("Items", "Items");
            dgvOrderList.Columns.Add("Total", "Total");
            dgvOrderList.Columns.Add("Payment", "Payment");
            dgvOrderList.Columns.Add("Status", "Status");
            dgvOrderList.Columns.Add("Date", "Date");

            dgvOrderList.EnableHeadersVisualStyles = false;
            dgvOrderList.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(53, 57, 59);
            dgvOrderList.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvOrderList.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8, FontStyle.Bold);
            dgvOrderList.ColumnHeadersHeight = 28;

            return dgvOrderList;
        }

        private void SetupUI()
        {
            dtpStartDate.Value = DateTime.Now.AddDays(-30);
            dtpEndDate.Value = DateTime.Now;
            cmbChartType.SelectedIndex = 0;
            cmbDataType.SelectedIndex = 0;
        }

        private void CmbReportType_SelectedIndexChanged(object sender, EventArgs e)
        {
            string reportType = cmbReportType.SelectedItem?.ToString();
            DateTime now = DateTime.Now;

            switch (reportType)
            {
                case "Daily":
                    dtpStartDate.Value = now.Date;
                    dtpEndDate.Value = now.Date;
                    break;
                case "Weekly":
                    dtpStartDate.Value = now.AddDays(-(int)now.DayOfWeek);
                    dtpEndDate.Value = now;
                    break;
                case "Monthly":
                    dtpStartDate.Value = new DateTime(now.Year, now.Month, 1);
                    dtpEndDate.Value = now;
                    break;
                case "Yearly":
                    dtpStartDate.Value = new DateTime(now.Year, 1, 1);
                    dtpEndDate.Value = now;
                    break;
                case "Custom":
                    break;
            }
        }

        private void CmbChartType_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateChart();
        }

        private void CmbDataType_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateChart();
        }

        private void UpdateChart()
        {
            if (chartMain == null) return;

            string chartType = cmbChartType.SelectedItem?.ToString();
            string dataType = cmbDataType.SelectedItem?.ToString();

            chartMain.Series.Clear();

            Series series = new Series
            {
                Name = "Data",
                IsValueShownAsLabel = true,
                LabelFormat = "{0:N0}",
                Font = new Font("Segoe UI", 7, FontStyle.Bold)
            };

            switch (chartType)
            {
                case "Column Chart":
                    series.ChartType = SeriesChartType.Column;
                    series.Color = Color.FromArgb(42, 157, 143);
                    break;
                case "Line Chart":
                    series.ChartType = SeriesChartType.Line;
                    series.Color = Color.FromArgb(230, 57, 70);
                    series.BorderWidth = 3;
                    series.MarkerStyle = MarkerStyle.Circle;
                    series.MarkerSize = 8;
                    break;
                case "Pie Chart":
                    series.ChartType = SeriesChartType.Pie;
                    break;
                case "Doughnut Chart":
                    series.ChartType = SeriesChartType.Doughnut;
                    break;
                case "Bar Chart":
                    series.ChartType = SeriesChartType.Bar;
                    series.Color = Color.FromArgb(42, 157, 143);
                    break;
                default:
                    series.ChartType = SeriesChartType.Column;
                    series.Color = Color.FromArgb(42, 157, 143);
                    break;
            }

            chartMain.Series.Add(series);
            UpdateChartData(dataType);
            chartMain.Invalidate();
        }

        private void UpdateChartData(string dataType)
        {
            string startDate = dtpStartDate.Value.ToString("yyyy-MM-dd");
            string endDate = dtpEndDate.Value.ToString("yyyy-MM-dd");
            string selectedChannel = cmbOrderType.SelectedItem?.ToString();
            string orderType = selectedChannel == null || selectedChannel == "All Channels"
                ? null
                : selectedChannel.ToLowerInvariant();

            var orders = _dbService.GetOrders(orderType: orderType, startDate: startDate, endDate: endDate);

            chartMain.Series["Data"].Points.Clear();

            switch (dataType)
            {
                case "Revenue":
                    UpdateRevenueData(orders);
                    chartMain.Titles[0].Text = "Revenue Trend";
                    break;
                case "Orders":
                    UpdateOrdersData(orders);
                    chartMain.Titles[0].Text = "Orders Trend";
                    break;
                case "Order Type Mix":
                    UpdateOrderTypeMixData(orders);
                    chartMain.Titles[0].Text = "Order Type Distribution";
                    break;
                case "Payment Mix":
                    UpdatePaymentMixData(orders);
                    chartMain.Titles[0].Text = "Payment Method Distribution";
                    break;
                default:
                    UpdateRevenueData(orders);
                    chartMain.Titles[0].Text = "Revenue Trend";
                    break;
            }

            chartMain.Invalidate();
        }

        private void UpdateRevenueData(List<Order> orders)
        {
            var grouped = orders
                .Where(o => o.Status == "completed")
                .GroupBy(o => DateTime.Parse(o.CreatedAt).Date)
                .Select(g => new { Date = g.Key, Value = g.Sum(o => o.Total) })
                .OrderBy(x => x.Date)
                .ToList();

            if (grouped.Count == 0)
            {
                chartMain.Series["Data"].Points.AddXY("No Data", 0);
            }
            else
            {
                foreach (var item in grouped)
                {
                    chartMain.Series["Data"].Points.AddXY(item.Date.ToShortDateString(), item.Value);
                }
            }
        }

        private void UpdateOrdersData(List<Order> orders)
        {
            var grouped = orders
                .GroupBy(o => DateTime.Parse(o.CreatedAt).Date)
                .Select(g => new { Date = g.Key, Value = g.Count() })
                .OrderBy(x => x.Date)
                .ToList();

            if (grouped.Count == 0)
            {
                chartMain.Series["Data"].Points.AddXY("No Data", 0);
            }
            else
            {
                foreach (var item in grouped)
                {
                    chartMain.Series["Data"].Points.AddXY(item.Date.ToShortDateString(), item.Value);
                }
            }
        }

        private void UpdateOrderTypeMixData(List<Order> orders)
        {
            var breakdown = orders
                .GroupBy(o => NormalizeOrderType(o.OrderType))
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .ToList();

            var colors = new Color[] {
                Color.FromArgb(230, 57, 70),
                Color.FromArgb(42, 157, 143),
                Color.FromArgb(244, 162, 97)
            };

            if (breakdown.Count == 0)
            {
                int idx = chartMain.Series["Data"].Points.AddXY("No Orders", 1);
                chartMain.Series["Data"].Points[idx].Color = Color.FromArgb(200, 200, 200);
                chartMain.Series["Data"].Points[idx].Label = "No Orders";
            }
            else
            {
                int index = 0;
                foreach (var item in breakdown)
                {
                    int idx = chartMain.Series["Data"].Points.AddXY(item.Type, item.Count);
                    chartMain.Series["Data"].Points[idx].Color = colors[index % colors.Length];
                    chartMain.Series["Data"].Points[idx].Label = $"{item.Type}\n{item.Count}";
                    index++;
                }
            }
        }

        private void UpdatePaymentMixData(List<Order> orders)
        {
            var breakdown = orders
                .Where(o => !string.IsNullOrEmpty(o.PaymentMethod))
                .GroupBy(o => o.PaymentMethod)
                .Select(g => new { Method = g.Key, Count = g.Count() })
                .ToList();

            var colors = new Color[] {
                Color.FromArgb(46, 204, 113),
                Color.FromArgb(52, 152, 219),
                Color.FromArgb(241, 196, 15)
            };

            if (breakdown.Count == 0)
            {
                int idx = chartMain.Series["Data"].Points.AddXY("No Data", 1);
                chartMain.Series["Data"].Points[idx].Color = Color.FromArgb(200, 200, 200);
                chartMain.Series["Data"].Points[idx].Label = "No Data";
            }
            else
            {
                int index = 0;
                foreach (var item in breakdown)
                {
                    string label = char.ToUpper(item.Method[0]) + item.Method.Substring(1);
                    int idx = chartMain.Series["Data"].Points.AddXY(label, item.Count);
                    chartMain.Series["Data"].Points[idx].Color = colors[index % colors.Length];
                    chartMain.Series["Data"].Points[idx].Label = $"{label}\n{item.Count}";
                    index++;
                }
            }
        }

        // ============================================
        // GENERATE BUTTON - Fetches data with current filters
        // ============================================
        private void BtnGenerate_Click(object sender, EventArgs e)
        {
            LoadReport();
        }

        // ============================================
        // EXPORT BUTTON - Single Export
        // ============================================
        private void BtnExport_Click(object sender, EventArgs e)
        {
            try
            {
                if (_currentOrders.Count == 0)
                {
                    MessageBox.Show("No data to export. Please generate a report first.",
                        "Export Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                using (SaveFileDialog saveDialog = new SaveFileDialog())
                {
                    saveDialog.Filter = "CSV Files (*.csv)|*.csv|Text Files (*.txt)|*.txt";
                    saveDialog.FileName = $"Report_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                    saveDialog.Title = "Export Report as CSV";

                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        ExportToCsv(saveDialog.FileName);
                        MessageBox.Show($"Report exported successfully to:\n{saveDialog.FileName}",
                            "Export Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting report: {ex.Message}",
                    "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportToCsv(string filePath)
        {
            try
            {
                using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
                {
                    // Write header
                    writer.WriteLine("Order #,Type,Customer,Items,Total,Payment,Status,Date");

                    // Write data
                    foreach (var order in _currentOrders)
                    {
                        string items = string.Join("; ", order.Items.Take(3).Select(i => $"{i.ProductName} x{i.Quantity}"));
                        if (order.Items.Count > 3) items += "...";

                        string payment = order.PaymentMethod != null ? char.ToUpper(order.PaymentMethod[0]) + order.PaymentMethod.Substring(1) : "Cash";
                        string status = order.Status != null ? char.ToUpper(order.Status[0]) + order.Status.Substring(1) : "Pending";
                        string date = DateTime.Parse(order.CreatedAt).ToString("yyyy-MM-dd HH:mm");

                        writer.WriteLine($"{order.OrderNumber},{NormalizeOrderType(order.OrderType)},{order.CustomerName},\"{items}\",PKR {order.Total:N0},{payment},{status},{date}");
                    }

                    // Add summary stats at the end
                    writer.WriteLine();
                    writer.WriteLine("Summary");
                    writer.WriteLine("=======");
                    writer.WriteLine($"Total Orders,{_currentOrders.Count}");

                    var completedOrders = _currentOrders.Where(o => o.Status == "completed").ToList();
                    writer.WriteLine($"Total Revenue,PKR {completedOrders.Sum(o => o.Total):N0}");
                    writer.WriteLine($"Completed Orders,{completedOrders.Count}");
                    writer.WriteLine($"Pending Orders,{_currentOrders.Where(o => o.Status == "pending").Count()}");
                    writer.WriteLine($"Cancelled Orders,{_currentOrders.Where(o => o.Status == "cancelled").Count()}");

                    writer.Flush();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to export: {ex.Message}");
            }
        }

        private void LoadDefaultReport()
        {
            LoadReport();
        }

        private void LoadReport()
        {
            try
            {
                string startDate = dtpStartDate.Value.ToString("yyyy-MM-dd");
                string endDate = dtpEndDate.Value.ToString("yyyy-MM-dd");
                string selectedChannel = cmbOrderType.SelectedItem?.ToString();
                string orderType = selectedChannel == null || selectedChannel == "All Channels"
                    ? null
                    : selectedChannel.ToLowerInvariant();

                _currentOrders = _dbService.GetOrders(orderType: orderType, startDate: startDate, endDate: endDate);

                UpdateSummary(_currentOrders);
                UpdateChartData(cmbDataType.SelectedItem?.ToString());
                UpdateOrderGrid(_currentOrders);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading report: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateSummary(List<Order> orders)
        {
            var completedOrders = orders.Where(o => o.Status == "completed").ToList();
            var pendingOrders = orders.Where(o => o.Status == "pending").ToList();
            var cancelledOrders = orders.Where(o => o.Status == "cancelled").ToList();

            lblTotalOrders.Text = orders.Count.ToString();
            lblTotalRevenue.Text = $"PKR {completedOrders.Sum(o => o.Total):N0}";
            lblPendingOrders.Text = pendingOrders.Count.ToString();
            lblCompletedOrders.Text = completedOrders.Count.ToString();
            lblCancelledOrders.Text = cancelledOrders.Count.ToString();
        }

        private void UpdateOrderGrid(List<Order> orders)
        {
            if (dgvOrderList == null) return;

            dgvOrderList.Rows.Clear();

            foreach (var order in orders.Take(50))
            {
                string items = string.Join(", ", order.Items.Take(2).Select(i => i.ProductName));
                if (order.Items.Count > 2) items += "...";

                int rowIndex = dgvOrderList.Rows.Add(
                    order.OrderNumber,
                    NormalizeOrderType(order.OrderType),
                    order.CustomerName,
                    items,
                    $"PKR {order.Total:N0}",
                    order.PaymentMethod != null ? char.ToUpper(order.PaymentMethod[0]) + order.PaymentMethod.Substring(1) : "Cash",
                    order.Status != null ? char.ToUpper(order.Status[0]) + order.Status.Substring(1) : "Pending",
                    DateTime.Parse(order.CreatedAt).ToString("dd MMM yyyy HH:mm")
                );

                var row = dgvOrderList.Rows[rowIndex];
                switch (order.Status?.ToLower())
                {
                    case "pending":
                        row.Cells["Status"].Style.ForeColor = Color.FromArgb(241, 196, 15);
                        row.Cells["Status"].Style.Font = new Font("Segoe UI", 8, FontStyle.Bold);
                        break;
                    case "completed":
                        row.Cells["Status"].Style.ForeColor = Color.FromArgb(46, 204, 113);
                        row.Cells["Status"].Style.Font = new Font("Segoe UI", 8, FontStyle.Bold);
                        break;
                    case "cancelled":
                        row.Cells["Status"].Style.ForeColor = Color.FromArgb(231, 76, 60);
                        row.Cells["Status"].Style.Font = new Font("Segoe UI", 8, FontStyle.Bold);
                        break;
                }
            }
        }

        private string NormalizeOrderType(string orderType)
        {
            if (string.IsNullOrWhiteSpace(orderType))
            {
                return "Other";
            }

            return orderType.Trim().ToLowerInvariant() switch
            {
                "delivery" => "Delivery",
                "dining" => "Dining",
                "takeaway" => "Takeaway",
                _ => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(orderType)
            };
        }
    }
}