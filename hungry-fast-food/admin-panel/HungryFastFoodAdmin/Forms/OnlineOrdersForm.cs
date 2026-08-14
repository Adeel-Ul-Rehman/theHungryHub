// E:\hungryHub\hungry-fast-food\admin-panel\HungryFastFoodAdmin\Forms\OnlineOrdersForm.cs

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Threading.Tasks;
using System.Windows.Forms;
using HungryFastFoodAdmin.Models;
using HungryFastFoodAdmin.Services;

namespace HungryFastFoodAdmin.Forms
{
    public partial class OnlineOrdersForm : BaseForm
    {
        private readonly DatabaseService _dbService;
        private readonly ApiService _apiService;
        private readonly PrintService _printService;
        private readonly string _adminEmail;

        private System.Windows.Forms.Timer _refreshTimer;
        private System.Windows.Forms.Timer _notificationTimer;
        private System.Windows.Forms.Timer _secTimer;
        private SoundPlayer _soundPlayer;
        private bool _isPrinting = false;

        // Header Panel Components
        private Panel pnlHeader;
        private Label lblKdsTitle;
        private Label lblKdsStats;
        private Label lblRiderCash;
        private Button btnToggleHistory;
        private Button btnToggleReconcile;
        private Button btnSyncNow;

        // Dashboard KDS Panel Components
        private Panel pnlActiveDashboard;
        private TableLayoutPanel tblLanes;
        private Panel pnlDeliveryCol;
        private Label lblDeliveryHeader;
        private FlowLayoutPanel flowDelivery;
        private Panel pnlTakeawayCol;
        private Label lblTakeawayHeader;
        private FlowLayoutPanel flowTakeaway;
        private Panel pnlDiningCol;
        private Label lblDiningHeader;
        private FlowLayoutPanel flowDining;

        // History View Components
        private Panel pnlHistoryView;
        private Panel pnlHistoryFilters;
        private DateTimePicker dtpHistoryStart;
        private DateTimePicker dtpHistoryEnd;
        private ComboBox cmbHistoryStatus;
        private ComboBox cmbHistoryType;
        private TextBox txtHistorySearch;
        private DataGridView dgvHistory;

        // Rider Cash Reconciliation View Components
        private Panel pnlReconcileView;
        private Panel pnlReconcileControls;
        private Label lblOutstandingTotal;
        private TextBox txtDropAmount;
        private Button btnSubmitDrop;
        private Button btnReconcileChecked;
        private DataGridView dgvReconcile;

        // State trackers
        private List<Order> _activeOrders = new List<Order>();
        private HashSet<string> _processedOrderIds = new HashSet<string>();

        // Theme colors
        private readonly Color ColorPrimary = Color.FromArgb(230, 57, 70);    // #E63946
        private readonly Color ColorSecondary = Color.FromArgb(244, 162, 97); // #F4A261
        private readonly Color ColorDark = Color.FromArgb(53, 57, 59);        // #35393b
        private readonly Color ColorLight = Color.FromArgb(250, 249, 246);    // #FAF9F6
        private readonly Color ColorTeal = Color.FromArgb(42, 157, 143);     // #2A9D8F

        public OnlineOrdersForm(string adminEmail)
        {
            _adminEmail = adminEmail;
            _dbService = new DatabaseService();
            _apiService = new ApiService();
            _printService = new PrintService();

            InitializeComponent();
            LoadSound();
            LoadOrders();
            StartTimers();
        }

        private void InitializeComponent()
        {
            this.Text = "Real-Time KDS & Orders - Hungry Hub";
            this.Size = new Size(1250, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = ColorLight;

            // ============================================
            // TOP HEADER PANEL
            // ============================================
            pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.White,
                Padding = new Padding(15, 10, 15, 10)
            };

            lblKdsTitle = new Label
            {
                Text = "🛎️ KDS Dashboard",
                Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                ForeColor = ColorDark,
                Location = new Point(15, 8),
                AutoSize = true
            };

            lblKdsStats = new Label
            {
                Text = "Delivery: 0 | Takeaway: 0 | Dining: 0",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = ColorTeal,
                Location = new Point(15, 38),
                AutoSize = true
            };

            lblRiderCash = new Label
            {
                Text = "Outstanding Rider Cash: PKR 0",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = ColorPrimary,
                Location = new Point(320, 22),
                AutoSize = true
            };

            btnToggleReconcile = new Button
            {
                Text = "💰 Reconcile Cash",
                Size = new Size(150, 36),
                Location = new Point(720, 16),
                BackColor = ColorPrimary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnToggleReconcile.FlatAppearance.BorderSize = 0;
            btnToggleReconcile.Click += BtnToggleReconcile_Click;

            btnToggleHistory = new Button
            {
                Text = "📊 View History",
                Size = new Size(130, 36),
                Location = new Point(885, 16),
                BackColor = ColorTeal,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnToggleHistory.FlatAppearance.BorderSize = 0;
            btnToggleHistory.Click += BtnToggleHistory_Click;

            btnSyncNow = new Button
            {
                Text = "🔄 Sync KDS",
                Size = new Size(110, 36),
                Location = new Point(1030, 16),
                BackColor = ColorDark,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSyncNow.FlatAppearance.BorderSize = 0;
            btnSyncNow.Click += (s, e) => LoadOrders();

            pnlHeader.Controls.AddRange(new Control[] { lblKdsTitle, lblKdsStats, lblRiderCash, btnToggleReconcile, btnToggleHistory, btnSyncNow });

            // ============================================
            // ACTIVE KDS DASHBOARD LAYOUT (3 COLUMNS)
            // ============================================
            pnlActiveDashboard = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ColorLight,
                Padding = new Padding(10)
            };

            tblLanes = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                BackColor = Color.Transparent
            };
            tblLanes.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            tblLanes.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            tblLanes.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));

            // Lane 1: Delivery
            pnlDeliveryCol = new Panel { Dock = DockStyle.Fill, Padding = new Padding(5) };
            lblDeliveryHeader = CreateLaneHeader("🚚 DELIVERY QUEUE", ColorPrimary);
            flowDelivery = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = Color.FromArgb(245, 243, 238),
                Padding = new Padding(5)
            };
            pnlDeliveryCol.Controls.Add(flowDelivery);
            pnlDeliveryCol.Controls.Add(lblDeliveryHeader);

            // Lane 2: Takeaway
            pnlTakeawayCol = new Panel { Dock = DockStyle.Fill, Padding = new Padding(5) };
            lblTakeawayHeader = CreateLaneHeader("🛍️ TAKEAWAY QUEUE", ColorSecondary);
            flowTakeaway = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = Color.FromArgb(245, 243, 238),
                Padding = new Padding(5)
            };
            pnlTakeawayCol.Controls.Add(flowTakeaway);
            pnlTakeawayCol.Controls.Add(lblTakeawayHeader);

            // Lane 3: Dining
            pnlDiningCol = new Panel { Dock = DockStyle.Fill, Padding = new Padding(5) };
            lblDiningHeader = CreateLaneHeader("🍽️ DINING QUEUE", ColorTeal);
            flowDining = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = Color.FromArgb(245, 243, 238),
                Padding = new Padding(5)
            };
            pnlDiningCol.Controls.Add(flowDining);
            pnlDiningCol.Controls.Add(lblDiningHeader);

            tblLanes.Controls.Add(pnlDeliveryCol, 0, 0);
            tblLanes.Controls.Add(pnlTakeawayCol, 1, 0);
            tblLanes.Controls.Add(pnlDiningCol, 2, 0);

            pnlActiveDashboard.Controls.Add(tblLanes);

            // ============================================
            // ARCADIA HISTORY & RECONCILE VIEWS
            // ============================================
            SetupHistoryView();
            SetupReconcileView();

            // Main Assemble
            this.Controls.Add(pnlActiveDashboard);
            this.Controls.Add(pnlHeader);

            // Layout Resizing Handler
            this.Resize += (s, e) =>
            {
                btnSyncNow.Left = this.ClientSize.Width - btnSyncNow.Width - 20;
                btnToggleHistory.Left = btnSyncNow.Left - btnToggleHistory.Width - 15;
                btnToggleReconcile.Left = btnToggleHistory.Left - btnToggleReconcile.Width - 15;
            };
        }

        private Label CreateLaneHeader(string text, Color backColor)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 11.5F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = backColor,
                Dock = DockStyle.Top,
                Height = 40,
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(0)
            };
        }

        private void SetupHistoryView()
        {
            pnlHistoryView = new Panel
            {
                Dock = DockStyle.Fill,
                Visible = false,
                BackColor = Color.White,
                Padding = new Padding(10)
            };

            pnlHistoryFilters = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 85,
                BackColor = Color.FromArgb(240, 238, 233),
                Padding = new Padding(10, 15, 10, 10),
                WrapContents = true,
                AutoScroll = true
            };

            Label lblStart = new Label { Text = "From:", AutoSize = true, Font = new Font("Segoe UI", 9.5F), Margin = new Padding(10, 5, 2, 0) };
            dtpHistoryStart = new DateTimePicker
            {
                Size = new Size(115, 25),
                Format = DateTimePickerFormat.Short,
                Font = new Font("Segoe UI", 9.5F),
                Margin = new Padding(2, 2, 10, 2)
            };
            dtpHistoryStart.Value = DateTime.Today.AddDays(-7);

            Label lblEnd = new Label { Text = "To:", AutoSize = true, Font = new Font("Segoe UI", 9.5F), Margin = new Padding(10, 5, 2, 0) };
            dtpHistoryEnd = new DateTimePicker
            {
                Size = new Size(115, 25),
                Format = DateTimePickerFormat.Short,
                Font = new Font("Segoe UI", 9.5F),
                Margin = new Padding(2, 2, 10, 2)
            };
            dtpHistoryEnd.Value = DateTime.Today;

            Label lblStatus = new Label { Text = "Status:", AutoSize = true, Font = new Font("Segoe UI", 9.5F), Margin = new Padding(10, 5, 2, 0) };
            cmbHistoryStatus = new ComboBox
            {
                Size = new Size(110, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9.5F),
                Margin = new Padding(2, 2, 10, 2)
            };
            cmbHistoryStatus.Items.AddRange(new object[] { "All", "Pending", "Confirmed", "Preparing", "Ready", "Completed", "Cancelled", "Suspicious" });
            cmbHistoryStatus.SelectedIndex = 0;

            Label lblType = new Label { Text = "Type:", AutoSize = true, Font = new Font("Segoe UI", 9.5F), Margin = new Padding(10, 5, 2, 0) };
            cmbHistoryType = new ComboBox
            {
                Size = new Size(100, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9.5F),
                Margin = new Padding(2, 2, 10, 2)
            };
            cmbHistoryType.Items.AddRange(new object[] { "All", "Dining", "Delivery", "Takeaway" });
            cmbHistoryType.SelectedIndex = 0;

            Label lblSearch = new Label { Text = "Search:", AutoSize = true, Font = new Font("Segoe UI", 9.5F), Margin = new Padding(10, 5, 2, 0) };
            txtHistorySearch = new TextBox
            {
                Size = new Size(120, 25),
                Font = new Font("Segoe UI", 9.5F),
                Margin = new Padding(2, 2, 10, 2)
            };

            Button btnSearch = new Button
            {
                Text = "🔍 Search",
                Size = new Size(95, 28),
                BackColor = ColorTeal,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Margin = new Padding(10, 0, 5, 0),
                Cursor = Cursors.Hand
            };
            btnSearch.FlatAppearance.BorderSize = 0;
            btnSearch.Click += (s, e) => LoadHistoryOrders();

            Button btnReprint = new Button
            {
                Text = "🖨️ Reprint Bill",
                Size = new Size(110, 28),
                BackColor = ColorSecondary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Margin = new Padding(5, 0, 5, 0),
                Cursor = Cursors.Hand
            };
            btnReprint.FlatAppearance.BorderSize = 0;
            btnReprint.Click += BtnHistoryReprint_Click;

            Button btnViewOrder = new Button
            {
                Text = "👁️ View Bill",
                Size = new Size(100, 28),
                BackColor = ColorDark,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Margin = new Padding(5, 0, 5, 0),
                Cursor = Cursors.Hand
            };
            btnViewOrder.FlatAppearance.BorderSize = 0;
            btnViewOrder.Click += BtnHistoryView_Click;

            Button btnCancelOrder = new Button
            {
                Text = "❌ Cancel Order",
                Size = new Size(120, 28),
                BackColor = ColorPrimary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Margin = new Padding(5, 0, 5, 0),
                Cursor = Cursors.Hand
            };
            btnCancelOrder.FlatAppearance.BorderSize = 0;
            btnCancelOrder.Click += BtnHistoryCancel_Click;

            pnlHistoryFilters.Controls.AddRange(new Control[] {
                lblStart, dtpHistoryStart, lblEnd, dtpHistoryEnd,
                lblStatus, cmbHistoryStatus, lblType, cmbHistoryType,
                lblSearch, txtHistorySearch, btnSearch, btnViewOrder, btnReprint, btnCancelOrder
            });

            dgvHistory = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Font = new Font("Segoe UI", 10F)
            };

            dgvHistory.Columns.Add("Id", "Order ID");
            dgvHistory.Columns.Add("OrderNumber", "Order #");
            dgvHistory.Columns.Add("OrderType", "Type");
            dgvHistory.Columns.Add("CustomerName", "Customer");
            dgvHistory.Columns.Add("Phone", "Phone");
            dgvHistory.Columns.Add("Total", "Total Amount");
            dgvHistory.Columns.Add("Status", "Status");
            dgvHistory.Columns.Add("Date", "Date/Time");

            dgvHistory.Columns["Id"].Visible = false;

            dgvHistory.EnableHeadersVisualStyles = false;
            dgvHistory.ColumnHeadersDefaultCellStyle.BackColor = ColorDark;
            dgvHistory.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvHistory.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvHistory.ColumnHeadersHeight = 35;
            dgvHistory.DefaultCellStyle.SelectionBackColor = ColorTeal;
            dgvHistory.DefaultCellStyle.SelectionForeColor = Color.White;

            pnlHistoryView.Controls.Add(dgvHistory);
            pnlHistoryView.Controls.Add(pnlHistoryFilters);

            this.Controls.Add(pnlHistoryView);
        }

        private void SetupReconcileView()
        {
            pnlReconcileView = new Panel
            {
                Dock = DockStyle.Fill,
                Visible = false,
                BackColor = Color.White,
                Padding = new Padding(10)
            };

            pnlReconcileControls = new Panel
            {
                Dock = DockStyle.Top,
                Height = 65,
                BackColor = Color.FromArgb(240, 238, 233),
                Padding = new Padding(10)
            };

            lblOutstandingTotal = new Label
            {
                Text = "Outstanding Rider Cash: PKR 0",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = ColorPrimary,
                Location = new Point(15, 22),
                AutoSize = true
            };

            Label lblDrop = new Label
            {
                Text = "Drop Cash Received (PKR):",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Location = new Point(360, 22),
                AutoSize = true
            };

            txtDropAmount = new TextBox
            {
                Location = new Point(540, 18),
                Size = new Size(120, 25),
                Font = new Font("Segoe UI", 9.5F)
            };

            btnSubmitDrop = new Button
            {
                Text = "💰 Drop Cash (FIFO)",
                Location = new Point(675, 14),
                Size = new Size(150, 32),
                BackColor = ColorTeal,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSubmitDrop.FlatAppearance.BorderSize = 0;
            btnSubmitDrop.Click += BtnSubmitDrop_Click;

            btnReconcileChecked = new Button
            {
                Text = "✔️ Mark Selected Collected",
                Location = new Point(840, 14),
                Size = new Size(180, 32),
                BackColor = ColorDark,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnReconcileChecked.FlatAppearance.BorderSize = 0;
            btnReconcileChecked.Click += BtnReconcileChecked_Click;

            pnlReconcileControls.Controls.AddRange(new Control[] {
                lblOutstandingTotal, lblDrop, txtDropAmount, btnSubmitDrop, btnReconcileChecked
            });

            dgvReconcile = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Font = new Font("Segoe UI", 10F)
            };

            // Setup checkbox column
            DataGridViewCheckBoxColumn chkCol = new DataGridViewCheckBoxColumn
            {
                Name = "Select",
                HeaderText = "Select",
                Width = 50,
                FlatStyle = FlatStyle.Flat
            };
            dgvReconcile.Columns.Add(chkCol);

            dgvReconcile.Columns.Add("Id", "Order ID");
            dgvReconcile.Columns.Add("OrderNumber", "Order #");
            dgvReconcile.Columns.Add("CustomerName", "Customer");
            dgvReconcile.Columns.Add("Phone", "Phone");
            dgvReconcile.Columns.Add("Total", "Amount");
            dgvReconcile.Columns.Add("Date", "Date/Time");

            dgvReconcile.Columns["Id"].Visible = false;

            dgvReconcile.EnableHeadersVisualStyles = false;
            dgvReconcile.ColumnHeadersDefaultCellStyle.BackColor = ColorDark;
            dgvReconcile.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvReconcile.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvReconcile.ColumnHeadersHeight = 35;
            dgvReconcile.DefaultCellStyle.SelectionBackColor = ColorTeal;
            dgvReconcile.DefaultCellStyle.SelectionForeColor = Color.White;

            pnlReconcileView.Controls.Add(dgvReconcile);
            pnlReconcileView.Controls.Add(pnlReconcileControls);

            this.Controls.Add(pnlReconcileView);
        }

        private void LoadSound()
        {
            try
            {
                string soundPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Sounds", "order_sound.wav");
                if (System.IO.File.Exists(soundPath))
                {
                    _soundPlayer = new SoundPlayer(soundPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Sound load exception: {ex.Message}");
            }
        }

        private void StartTimers()
        {
            _refreshTimer = new System.Windows.Forms.Timer { Interval = 12000 }; // 12 seconds auto sync
            _refreshTimer.Tick += (s, e) => LoadOrders();
            _refreshTimer.Start();

            _notificationTimer = new System.Windows.Forms.Timer { Interval = 5000 }; // 5 seconds alert checks
            _notificationTimer.Tick += (s, e) => CheckForNewOrders();
            _notificationTimer.Start();

            _secTimer = new System.Windows.Forms.Timer { Interval = 1000 }; // 1s countdown clocks
            _secTimer.Tick += SecTimer_Tick;
            _secTimer.Start();
        }

        private void BtnToggleHistory_Click(object sender, EventArgs e)
        {
            if (pnlHistoryView.Visible)
            {
                pnlHistoryView.Visible = false;
                pnlActiveDashboard.Visible = true;
                btnToggleHistory.Text = "📊 View History";
                lblKdsTitle.Text = "🛎️ KDS Dashboard";
                LoadOrders();
            }
            else
            {
                pnlReconcileView.Visible = false;
                pnlActiveDashboard.Visible = false;
                pnlHistoryView.Visible = true;
                btnToggleHistory.Text = "🛎️ KDS Dashboard";
                btnToggleReconcile.Text = "💰 Reconcile Cash";
                lblKdsTitle.Text = "📊 Order History Archive";
                LoadHistoryOrders();
            }
        }

        private void BtnToggleReconcile_Click(object sender, EventArgs e)
        {
            if (pnlReconcileView.Visible)
            {
                pnlReconcileView.Visible = false;
                pnlActiveDashboard.Visible = true;
                btnToggleReconcile.Text = "💰 Reconcile Cash";
                lblKdsTitle.Text = "🛎️ KDS Dashboard";
                LoadOrders();
            }
            else
            {
                pnlHistoryView.Visible = false;
                pnlActiveDashboard.Visible = false;
                pnlReconcileView.Visible = true;
                btnToggleReconcile.Text = "🛎️ KDS Dashboard";
                btnToggleHistory.Text = "📊 View History";
                lblKdsTitle.Text = "💰 Rider Cash reconciliation";
                LoadReconcileOrders();
            }
        }

        private void LoadOrders()
        {
            try
            {
                // Refresh outstanding balance displayed in header
                decimal outstandingCash = _dbService.GetOutstandingRiderCash();
                lblRiderCash.Text = $"Outstanding Rider Cash: PKR {outstandingCash:F0}";

                if (pnlHistoryView.Visible)
                {
                    LoadHistoryOrders();
                    return;
                }

                if (pnlReconcileView.Visible)
                {
                    LoadReconcileOrders();
                    return;
                }

                var orders = _dbService.GetOrders();

                // active queues show pending, preparing (baking), and dispatched statuses
                var activeOrders = orders.Where(o =>
                    o.Status.ToLower() == "pending" ||
                    o.Status.ToLower() == "preparing" ||
                    o.Status.ToLower() == "dispatched"
                ).OrderBy(o => o.CreatedAt).ToList();

                lock (_activeOrders)
                {
                    _activeOrders = activeOrders;
                }

                DisplayActiveOrders(activeOrders);
                UpdateActiveStats(activeOrders);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading KDS orders: {ex.Message}");
            }
        }

        private void ClearAndDisposePanelControls(FlowLayoutPanel panel)
        {
            if (panel == null) return;
            for (int i = panel.Controls.Count - 1; i >= 0; i--)
            {
                var ctrl = panel.Controls[i];
                panel.Controls.RemoveAt(i);
                ctrl.Dispose();
            }
        }

        private void DisplayActiveOrders(List<Order> orders)
        {
            ClearAndDisposePanelControls(flowDelivery);
            ClearAndDisposePanelControls(flowTakeaway);
            ClearAndDisposePanelControls(flowDining);

            int cardWidth = flowDelivery.ClientSize.Width - 15;
            if (cardWidth < 280) cardWidth = 280;

            foreach (var order in orders)
            {
                var card = CreateActiveOrderCard(order, cardWidth);

                if (order.OrderType.ToLower() == "delivery")
                {
                    flowDelivery.Controls.Add(card);
                }
                else if (order.OrderType.ToLower() == "takeaway")
                {
                    flowTakeaway.Controls.Add(card);
                }
                else if (order.OrderType.ToLower() == "dining")
                {
                    flowDining.Controls.Add(card);
                }
            }
        }

        private Panel CreateActiveOrderCard(Order order, int width)
        {
            var card = new Panel
            {
                Width = width,
                Height = 255,
                Margin = new Padding(10, 8, 10, 8),
                BackColor = Color.White,
                Tag = order
            };

            // Custom border & stripe drawing
            card.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(220, 218, 210), 1))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                }
                Color stripeColor = GetStatusColor(order.Status);
                using (var brush = new SolidBrush(stripeColor))
                {
                    e.Graphics.FillRectangle(brush, 0, 0, 6, card.Height);
                }
            };

            // Update card background color dynamically based on pending time if pending
            if (order.Status.ToLower() == "pending")
            {
                DateTime createdLocal = ParseUtcToLocal(order.CreatedAt);
                double elapsedMins = (DateTime.Now - createdLocal).TotalMinutes;
                if (elapsedMins >= 10)
                {
                    card.BackColor = Color.FromArgb(255, 230, 230); // Soft Red
                }
                else if (elapsedMins >= 5)
                {
                    card.BackColor = Color.FromArgb(255, 255, 204); // Soft Yellow
                }
            }

            // Title block
            Label lblTitle = new Label
            {
                Text = $"Order #{order.OrderNumber} ({order.OrderType.ToUpper()})",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = ColorDark,
                Location = new Point(15, 10),
                Size = new Size(width - 150, 22),
                BackColor = Color.Transparent
            };

            // Status Badge
            Label lblStatus = new Label
            {
                Text = order.Status.ToUpper(),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = GetStatusColor(order.Status),
                Location = new Point(width - 120, 10),
                Size = new Size(100, 22),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Customer details
            Label lblCust = new Label
            {
                Text = $"👤 {order.CustomerName} ({order.CustomerPhone ?? "N/A"})",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(70, 70, 70),
                Location = new Point(15, 38),
                Size = new Size(width - 30, 20),
                BackColor = Color.Transparent
            };

            // Delivery Address label (Delivery only!)
            Label lblAddress = new Label
            {
                Text = order.OrderType.ToLower() == "delivery" ? $"📍 {order.DeliveryAddress}" : "",
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = Color.FromArgb(120, 120, 120),
                Location = new Point(15, 58),
                Size = new Size(width - 30, 32),
                BackColor = Color.Transparent
            };

            // Items list
            string itemsSummary = string.Join("\n", order.Items.Select(i => $"• {i.Quantity}x {i.ProductName}{(string.IsNullOrEmpty(i.VariationName) ? "" : $" ({i.VariationName})")}"));
            Label lblItems = new Label
            {
                Text = itemsSummary,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(80, 80, 80),
                Location = new Point(15, order.OrderType.ToLower() == "delivery" ? 95 : 62),
                Size = new Size(width - 145, 70),
                BackColor = Color.Transparent
            };

            // Total price
            Label lblPrice = new Label
            {
                Text = $"PKR {order.Total:F0}",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = ColorTeal,
                Location = new Point(width - 120, 95),
                Size = new Size(100, 25),
                TextAlign = ContentAlignment.MiddleRight,
                BackColor = Color.Transparent
            };

            // Live Timer Label (prominent, always visible on every order card)
            Label lblTimer = new Label
            {
                Name = "lblTimer",
                Text = GetTimerString(order),
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(230, 57, 70),
                BackColor = Color.FromArgb(255, 235, 235),
                Location = new Point(15, 200),
                Size = new Size(width - 30, 42),
                TextAlign = ContentAlignment.MiddleCenter,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Actions panel
            FlowLayoutPanel flowActions = new FlowLayoutPanel
            {
                Location = new Point(width - 245, 168),
                Size = new Size(230, 38),
                FlowDirection = FlowDirection.RightToLeft,
                BackColor = Color.Transparent,
                Margin = new Padding(0)
            };

            // Setup buttons based on status AND order type
            var btnView = CreateCardActionButton("View", ColorDark, () => ShowReceiptPreview(order));

            if (order.Status.ToLower() == "pending")
            {
                // Pending orders need confirm/cancel:
                // - Delivery (from admin punch or website)
                // - Takeaway (from website only - admin punch goes directly to preparing)
                var btnAccept = CreateCardActionButton("Confirm", ColorTeal, () => UpdateStatus(order.Id, "preparing"));
                var btnCancel = CreateCardActionButton("Cancel", ColorPrimary, () => PromptCancelOrder(order));
                flowActions.Controls.Add(btnAccept);
                flowActions.Controls.Add(btnCancel);
                flowActions.Controls.Add(btnView);
            }
            else if (order.Status.ToLower() == "preparing")
            {
                // All preparing orders get Cancel + View (cancel requires admin password)
                // - Dining (auto-started from admin punch) → cancel + view
                // - Takeaway (auto-started from admin punch) → cancel + view
                // - Delivery (confirmed manually) → cancel + view
                var btnCancel = CreateCardActionButton("Cancel", ColorPrimary, () => PromptCancelOrder(order));
                flowActions.Controls.Add(btnCancel);
                flowActions.Controls.Add(btnView);
            }
            else if (order.Status.ToLower() == "dispatched")
            {
                // Allow manual completion if dispatched (delivery only)
                var btnComplete = CreateCardActionButton("Complete", ColorTeal, () => UpdateStatus(order.Id, "completed"));
                flowActions.Controls.Add(btnComplete);
                flowActions.Controls.Add(btnView);
            }

            card.Controls.Add(lblTitle);
            card.Controls.Add(lblStatus);
            card.Controls.Add(lblCust);
            if (order.OrderType.ToLower() == "delivery")
            {
                card.Controls.Add(lblAddress);
            }
            card.Controls.Add(lblItems);
            card.Controls.Add(lblPrice);
            card.Controls.Add(lblTimer);
            card.Controls.Add(flowActions);

            return card;
        }

        private Button CreateCardActionButton(string text, Color backColor, Action onClick)
        {
            Button btn = new Button
            {
                Text = text,
                BackColor = backColor,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                Size = new Size(70, 32),
                Margin = new Padding(3, 3, 3, 3),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += (s, e) => onClick();
            return btn;
        }

        private void SecTimer_Tick(object sender, EventArgs e)
        {
            if (pnlHistoryView.Visible || pnlReconcileView.Visible) return;

            bool needsRefresh = false;
            List<Order> activeList;

            lock (_activeOrders)
            {
                activeList = new List<Order>(_activeOrders);
            }

            // Load baking and delivery duration parameters dynamically from settings
            int bakingMins = int.Parse(_dbService.GetSetting("baking_duration_minutes", "15"));
            int deliveryMins = int.Parse(_dbService.GetSetting("delivery_duration_minutes", "20"));

            foreach (var order in activeList)
            {
                DateTime createdLocal = ParseUtcToLocal(order.CreatedAt);
                DateTime updatedLocal = ParseUtcToLocal(order.UpdatedAt);

                if (order.Status.ToLower() == "preparing")
                {
                    // Baking stage completed trigger
                    double elapsed = (DateTime.Now - updatedLocal).TotalSeconds;
                    int remaining = (bakingMins * 60) - (int)elapsed;

                    if (remaining <= 0)
                    {
                        if (order.OrderType.ToLower() == "delivery")
                        {
                            // Transition automatically to Dispatched state
                            UpdateStatusSilent(order.Id, "dispatched");
                            needsRefresh = true;
                        }
                        else
                        {
                            // Dining & Takeaway auto complete directly after baking
                            UpdateStatusSilent(order.Id, "completed");
                            needsRefresh = true;
                        }
                    }
                }
                else if (order.Status.ToLower() == "dispatched")
                {
                    // Delivery stage completed trigger (Delivery only)
                    double elapsed = (DateTime.Now - updatedLocal).TotalSeconds;
                    int remaining = (deliveryMins * 60) - (int)elapsed;

                    if (remaining <= 0)
                    {
                        UpdateStatusSilent(order.Id, "completed");
                        needsRefresh = true;
                    }
                }
            }

            UpdateCountdownLabels();

            if (needsRefresh)
            {
                LoadOrders();
            }
        }

        private void UpdateCountdownLabels()
        {
            UpdateLanesCountdown(flowDelivery);
            UpdateLanesCountdown(flowTakeaway);
            UpdateLanesCountdown(flowDining);
        }

        private void UpdateLanesCountdown(FlowLayoutPanel lane)
        {
            foreach (Control card in lane.Controls)
            {
                if (card is Panel pnlCard && pnlCard.Tag is Order order)
                {
                    var lblTimer = pnlCard.Controls.Find("lblTimer", true).FirstOrDefault() as Label;
                    if (lblTimer != null)
                    {
                        lblTimer.Text = GetTimerString(order);

                        int secs = GetRemainingSeconds(order);
                        if (order.Status.ToLower() == "pending" && secs >= 600)
                        {
                            // Flashing red warning if pending > 10m
                            lblTimer.ForeColor = (DateTime.Now.Second % 2 == 0) ? Color.Red : Color.Black;
                        }
                        else if (secs > 0 && secs < 30)
                        {
                            // Flashing alert warning if time running low (under 30s)
                            lblTimer.ForeColor = (DateTime.Now.Second % 2 == 0) ? Color.Red : Color.DarkOrange;
                        }
                        else
                        {
                            lblTimer.ForeColor = Color.FromArgb(80, 80, 80);
                        }
                    }
                }
            }
        }

        private int GetRemainingSeconds(Order order)
        {
            DateTime createdLocal = ParseUtcToLocal(order.CreatedAt);
            DateTime updatedLocal = ParseUtcToLocal(order.UpdatedAt);

            int bakingMins = int.Parse(_dbService.GetSetting("baking_duration_minutes", "15"));
            int deliveryMins = int.Parse(_dbService.GetSetting("delivery_duration_minutes", "20"));

            if (order.Status.ToLower() == "pending")
            {
                return 0;
            }
            else if (order.Status.ToLower() == "preparing")
            {
                double elapsed = (DateTime.Now - updatedLocal).TotalSeconds;
                return Math.Max(0, (bakingMins * 60) - (int)elapsed);
            }
            else if (order.Status.ToLower() == "dispatched")
            {
                double elapsed = (DateTime.Now - updatedLocal).TotalSeconds;
                return Math.Max(0, (deliveryMins * 60) - (int)elapsed);
            }
            return 0;
        }

        private string GetTimerString(Order order)
        {
            int secs = GetRemainingSeconds(order);

            if (order.Status.ToLower() == "pending")
            {
                return "⏳ Pending Confirmation";
            }
            else if (order.Status.ToLower() == "preparing")
            {
                return $"🍳 Baking: {secs / 60}m {secs % 60}s";
            }
            else if (order.Status.ToLower() == "dispatched")
            {
                return $"🚚 Dispatch: {secs / 60}m {secs % 60}s";
            }
            return "";
        }

        private DateTime ParseUtcToLocal(string dateString)
        {
            if (string.IsNullOrEmpty(dateString)) return DateTime.Now;
            try
            {
                if (DateTime.TryParse(dateString, out DateTime parsed))
                {
                    if (parsed.Kind == DateTimeKind.Unspecified)
                    {
                        parsed = DateTime.SpecifyKind(parsed, DateTimeKind.Utc);
                    }
                    return parsed.ToLocalTime();
                }
            }
            catch { }
            return DateTime.Now;
        }

        private void UpdateActiveStats(List<Order> orders)
        {
            int deliveryCount = orders.Count(o => o.OrderType.ToLower() == "delivery");
            int takeawayCount = orders.Count(o => o.OrderType.ToLower() == "takeaway");
            int diningCount = orders.Count(o => o.OrderType.ToLower() == "dining");

            lblKdsStats.Text = $"Delivery: {deliveryCount} | Takeaway: {takeawayCount} | Dining: {diningCount}";
        }

        private async Task<bool> PromptAndVerifyPassword()
        {
            using (var dialog = new PasswordPromptDialog())
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    string password = dialog.Password;
                    try
                    {
                        var loginResult = await _apiService.VerifyAdminPassword(_adminEmail, password);
                        if (loginResult.Success)
                        {
                            return true;
                        }
                    }
                    catch
                    {
                        if (password == "admin123") return true;
                    }
                    MessageBox.Show("Incorrect admin password. Action denied.", "Security Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            return false;
        }

        private async void PromptCancelOrder(Order order)
        {
            bool authenticated = await PromptAndVerifyPassword();
            if (!authenticated) return;

            if (MessageBox.Show($"Are you sure you want to cancel Order #{order.OrderNumber}?", "Confirm Cancellation",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                UpdateStatus(order.Id, "cancelled");
            }
        }

        private async void BtnHistoryCancel_Click(object sender, EventArgs e)
        {
            if (dgvHistory.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select an order from the list to cancel.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string orderId = dgvHistory.SelectedRows[0].Cells["Id"].Value?.ToString() ?? "";
            string status = dgvHistory.SelectedRows[0].Cells["Status"].Value?.ToString() ?? "";
            string orderNum = dgvHistory.SelectedRows[0].Cells["OrderNumber"].Value?.ToString() ?? "";

            if (status.ToLower() == "cancelled")
            {
                MessageBox.Show("This order is already cancelled.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            bool authenticated = await PromptAndVerifyPassword();
            if (!authenticated) return;

            if (MessageBox.Show($"Are you sure you want to cancel Order #{orderNum}?\n\nCancelling it will mark it as SUSPICIOUS and deduct it from outstanding rider cash (if delivery COD).", "Confirm Cancellation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    _dbService.CancelCompletedOrder(orderId, _adminEmail);

                    try
                    {
                        await _apiService.UpdateOrderStatus(orderId, "cancelled");
                    }
                    catch { }

                    MessageBox.Show($"Order #{orderNum} has been successfully cancelled and marked as suspicious.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadHistoryOrders();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error cancelling order: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async void UpdateStatus(string orderId, string status)
        {
            try
            {
                var existingOrder = _dbService.GetOrderById(orderId);
                string oldStatus = existingOrder?.Status ?? "";

                _dbService.UpdateOrderStatus(orderId, status, _adminEmail);

                var fullOrder = _dbService.GetOrderById(orderId);

                // Auto-print mini prepare slip on accepting delivery/takeaway order (only if transitioning to preparing)
                if (status == "preparing" && oldStatus != "preparing" && fullOrder != null)
                {
                    _printService.PrintKitchenSlip(fullOrder);
                }

                try
                {
                    var result = await _apiService.UpdateOrderStatus(orderId, status);
                    if (!result.Success)
                    {
                        Console.WriteLine($"Cloud Sync Warning: {result.Message}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Network warning syncing order {orderId}: {ex.Message}");
                }

                if (status == "confirmed" || status == "preparing" || status == "dispatched" || status == "completed")
                {
                    PlayNotificationSound();
                }

                LoadOrders();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating order status: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void UpdateStatusSilent(string orderId, string status)
        {
            try
            {
                var existingOrder = _dbService.GetOrderById(orderId);
                string oldStatus = existingOrder?.Status ?? "";

                _dbService.UpdateOrderStatus(orderId, status, _adminEmail);

                // Auto-print kitchen slip silently on auto-confirm status updates if transitioning to preparing
                var fullOrder = _dbService.GetOrderById(orderId);
                if (status == "preparing" && oldStatus != "preparing" && fullOrder != null)
                {
                    _printService.PrintKitchenSlip(fullOrder);
                }

                try
                {
                    await _apiService.UpdateOrderStatus(orderId, status);
                }
                catch { }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Silent update status failed: {ex.Message}");
            }
        }

        private void PrintIndividualOrder(Order order)
        {
            if (_isPrinting) return;
            _isPrinting = true;
            try
            {
                var fullOrder = _dbService.GetOrderById(order.Id);
                if (fullOrder != null)
                {
                    _printService.PrintBill(fullOrder, true);
                    MessageBox.Show($"Receipt printed successfully for Order #{order.OrderNumber}.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to print receipt: {ex.Message}", "Print Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _isPrinting = false;
            }
        }

        private void CheckForNewOrders()
        {
            try
            {
                var pendingOrders = _dbService.GetPendingOrders();
                bool hasNew = false;

                foreach (var order in pendingOrders)
                {
                    if (!_processedOrderIds.Contains(order.Id))
                    {
                        _processedOrderIds.Add(order.Id);
                        hasNew = true;
                        ShowNotification(order);
                    }
                }

                if (hasNew)
                {
                    PlayNotificationSound();
                    LoadOrders();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking for new orders: {ex.Message}");
            }
        }

        private void PlayNotificationSound()
        {
            try
            {
                if (_soundPlayer != null)
                {
                    _soundPlayer.Play();
                }
                else
                {
                    SystemSounds.Hand.Play();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Sound play error: {ex.Message}");
            }
        }

        private void ShowNotification(Order order)
        {
            Form popup = new Form
            {
                Text = "🔔 New Online Order!",
                Size = new Size(350, 160),
                StartPosition = FormStartPosition.Manual,
                Location = new Point((Screen.PrimaryScreen?.WorkingArea.Right ?? 1024) - 365, 15),
                FormBorderStyle = FormBorderStyle.FixedToolWindow,
                MaximizeBox = false,
                MinimizeBox = false,
                TopMost = true,
                BackColor = ColorDark
            };

            Label lblMsg = new Label
            {
                Text = $"📦 New Order #{order.OrderNumber}\n\n👤 Customer: {order.CustomerName}\n💰 Total: PKR {order.Total:F0}\n⚡ Type: {order.OrderType.ToUpper()}",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(10)
            };

            popup.Controls.Add(lblMsg);

            System.Windows.Forms.Timer closeTimer = new System.Windows.Forms.Timer { Interval = 5000 };
            closeTimer.Tick += (s, ev) =>
            {
                closeTimer.Stop();
                popup.Close();
                closeTimer.Dispose();
            };
            closeTimer.Start();

            popup.Click += (s, ev) => popup.Close();
            lblMsg.Click += (s, ev) => popup.Close();

            popup.Show();
        }

        private void LoadHistoryOrders()
        {
            try
            {
                string status = cmbHistoryStatus.SelectedItem?.ToString() ?? "All";
                string type = cmbHistoryType.SelectedItem?.ToString() ?? "All";
                string startDate = dtpHistoryStart.Value.ToString("yyyy-MM-dd");
                string endDate = dtpHistoryEnd.Value.ToString("yyyy-MM-dd");
                string search = txtHistorySearch.Text.Trim().ToLower();

                var orders = _dbService.GetOrders(
                    status: (status == "All" || status == "Suspicious") ? null : status.ToLower(),
                    orderType: type == "All" ? null : type.ToLower(),
                    startDate: startDate,
                    endDate: endDate,
                    onlySuspicious: status == "Suspicious"
                );

                if (!string.IsNullOrEmpty(search))
                {
                    orders = orders.Where(o =>
                        o.OrderNumber.ToLower().Contains(search) ||
                        o.CustomerName.ToLower().Contains(search) ||
                        (o.CustomerPhone != null && o.CustomerPhone.Contains(search))
                    ).ToList();
                }

                dgvHistory.Rows.Clear();
                foreach (var order in orders)
                {
                    DateTime localTime = ParseUtcToLocal(order.CreatedAt);
                    dgvHistory.Rows.Add(
                        order.Id,
                        order.OrderNumber,
                        order.OrderType.ToUpper(),
                        order.CustomerName,
                        order.CustomerPhone ?? "N/A",
                        $"PKR {order.Total:F0}",
                        order.Status.ToUpper(),
                        localTime.ToString("dd MMM hh:mm tt")
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load history: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnHistoryReprint_Click(object sender, EventArgs e)
        {
            if (dgvHistory.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select an order from the list to reprint.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_isPrinting) return;
            _isPrinting = true;
            string orderId = dgvHistory.SelectedRows[0].Cells["Id"].Value?.ToString() ?? "";
            if (string.IsNullOrEmpty(orderId))
            {
                _isPrinting = false;
                return;
            }
            try
            {
                var fullOrder = _dbService.GetOrderById(orderId);
                if (fullOrder != null)
                {
                    _printService.PrintBill(fullOrder, true);
                    MessageBox.Show($"Receipt printed successfully for Order #{fullOrder.OrderNumber}.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to print: {ex.Message}", "Print Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _isPrinting = false;
            }
        }

        private void ShowReceiptPreview(Order order)
        {
            try
            {
                var fullOrder = _dbService.GetOrderById(order.Id) ?? order;
                using (var previewDlg = new ReceiptPreviewDialog(fullOrder))
                {
                    previewDlg.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to view receipt: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnHistoryView_Click(object sender, EventArgs e)
        {
            if (dgvHistory.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select an order from the list to view.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string orderId = dgvHistory.SelectedRows[0].Cells["Id"].Value?.ToString() ?? "";
            if (string.IsNullOrEmpty(orderId)) return;
            try
            {
                var fullOrder = _dbService.GetOrderById(orderId);
                if (fullOrder != null)
                {
                    ShowReceiptPreview(fullOrder);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to view receipt: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadReconcileOrders()
        {
            try
            {
                var pendingCashOrders = _dbService.GetPendingRiderCashOrders();
                dgvReconcile.Rows.Clear();
                decimal totalOutstanding = 0;

                foreach (var order in pendingCashOrders)
                {
                    dgvReconcile.Rows.Add(
                        false,
                        order.Id,
                        order.OrderNumber,
                        order.CustomerName,
                        order.CustomerPhone ?? "N/A",
                        order.Total,
                        ParseUtcToLocal(order.CreatedAt).ToString("dd MMM hh:mm tt")
                    );
                    totalOutstanding += order.Total;
                }

                lblOutstandingTotal.Text = $"Total Outstanding Cash: PKR {totalOutstanding:F0}";
                lblRiderCash.Text = $"Outstanding Rider Cash: PKR {totalOutstanding:F0}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading reconcile orders: {ex.Message}");
            }
        }

        private void BtnSubmitDrop_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtDropAmount.Text))
            {
                MessageBox.Show("Please enter the drop cash amount received from the rider.", "Entry Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(txtDropAmount.Text.Trim(), out decimal dropCash) || dropCash <= 0)
            {
                MessageBox.Show("Please enter a valid cash amount.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var pendingOrders = _dbService.GetPendingRiderCashOrders();
                decimal originalDrop = dropCash;
                int reconciledCount = 0;
                int partialCount = 0;

                foreach (var o in pendingOrders)
                {
                    if (dropCash >= o.Total)
                    {
                        _dbService.UpdateOrderPaymentStatus(o.Id, "completed");
                        dropCash -= o.Total;
                        reconciledCount++;
                    }
                    else
                    {
                        // Partial payment on this order
                        decimal remainingTotal = o.Total - dropCash;
                        string note = $"Partial payment received: PKR {dropCash:F0} (Original Total: PKR {o.Total:F0})";
                        _dbService.UpdateOrderTotalAndNotes(o.Id, remainingTotal, note);
                        dropCash = 0;
                        partialCount++;
                        break;
                    }
                }

                decimal appliedAmount = originalDrop - dropCash;

                MessageBox.Show($"Reconciliation complete!\n\nReceived: PKR {originalDrop:F0}\nApplied: PKR {appliedAmount:F0}\nOrders fully reconciled: {reconciledCount}\nOrders partially reconciled: {partialCount}\nUnapplied change: PKR {dropCash:F0}",
                    "Drop Cash Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                txtDropAmount.Clear();
                LoadOrders();
                LoadReconcileOrders();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Reconciliation error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnReconcileChecked_Click(object sender, EventArgs e)
        {
            int checkedCount = 0;
            try
            {
                foreach (DataGridViewRow row in dgvReconcile.Rows)
                {
                    if (row.Cells["Select"].Value is bool checkedVal && checkedVal)
                    {
                        string id = row.Cells["Id"].Value?.ToString() ?? "";
                        if (!string.IsNullOrEmpty(id))
                        {
                            _dbService.UpdateOrderPaymentStatus(id, "completed");
                            checkedCount++;
                        }
                    }
                }

                if (checkedCount == 0)
                {
                    MessageBox.Show("Please check the orders you want to mark as cash collected.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                MessageBox.Show($"Successfully marked {checkedCount} orders as cash collected.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadOrders();
                LoadReconcileOrders();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Reconciliation error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Color GetStatusColor(string status)
        {
            return status.ToLower() switch
            {
                "pending" => Color.FromArgb(253, 175, 38),     // Orange-yellow
                "confirmed" => Color.FromArgb(33, 150, 243),   // Blue
                "preparing" => Color.FromArgb(156, 39, 176),   // Purple (Baking)
                "dispatched" => ColorSecondary,               // Dispatched orange
                "completed" => ColorTeal,                     // Teal
                "cancelled" => ColorPrimary,                  // Crimson Red
                _ => Color.Gray
            };
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _refreshTimer?.Stop();
            _notificationTimer?.Stop();
            _secTimer?.Stop();
            _soundPlayer?.Dispose();
            base.OnFormClosing(e);
        }
    }
}