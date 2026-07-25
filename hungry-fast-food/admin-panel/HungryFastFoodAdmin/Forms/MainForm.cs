// E:\hungryHub\hungry-fast-food\admin-panel\HungryFastFoodAdmin\Forms\MainForm.cs

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HungryFastFoodAdmin.Services;
using HungryFastFoodAdmin.Models;

namespace HungryFastFoodAdmin.Forms
{
    public partial class MainForm : Form
    {
        // Sidebar Controls
        private Panel sidebarPanel;
        private PictureBox pbLogo;
        private Button btnDashboard;
        private Button btnOrderPunch;
        private Button btnOnlineOrders;
        private Button btnMenu;
        private Button btnDeals;
        private Button btnReports;
        private Button btnSettings;
        private Button btnLogout;

        // Top Bar Controls
        private Panel topBarPanel;
        private Label lblPageTitle;
        private Label lblDateTime;
        private Panel pnlSyncStatus;
        private Label lblLastSync;
        private Button btnSync;
        private Label lblAdminName;
        private System.Windows.Forms.Timer clockTimer;

        // Main Content Panel
        private Panel contentPanel;

        private DatabaseService dbService;
        private SyncService syncService;
        private SocketSyncService socketSyncService;
        private string adminEmail;

        public MainForm(string adminEmail)
        {
            this.adminEmail = adminEmail;
            dbService = new DatabaseService();
            syncService = new SyncService();

            InitializeComponent();
            SetupDashboardUI();
            
            // Start clock timer
            clockTimer = new System.Windows.Forms.Timer();
            clockTimer.Interval = 1000;
            clockTimer.Tick += (s, e) => { lblDateTime.Text = DateTime.Now.ToString("dd MMM yyyy, hh:mm:ss tt"); };
            clockTimer.Start();

            // Wire sync events
            syncService.SyncStatusChanged += (isOnline, state) =>
            {
                if (InvokeRequired)
                    Invoke(new Action(() => UpdateSyncIndicator(isOnline, state)));
                else
                    UpdateSyncIndicator(isOnline, state);
            };

            syncService.NewOrdersReceived += (orders) =>
            {
                if (InvokeRequired)
                    Invoke(new Action(() => ShowNewOrderNotification(orders.Count)));
                else
                    ShowNewOrderNotification(orders.Count);
            };

            // Start background sync
            syncService.StartAutoSync();

            // Start realtime socket sync listener to react to website events immediately
            socketSyncService = new SocketSyncService();
            socketSyncService.ConnectionChanged += connected =>
            {
                Logger.Log($"🔌 Socket connection {(connected ? "established" : "lost")}");
            };
            socketSyncService.SocketEventReceived += (eventName, payload) =>
            {
                Task.Run(async () =>
                {
                    Logger.Log($"📡 Socket event received: {eventName}");
                    if (eventName == "order_placed" || eventName == "order_status_updated" || eventName == "category_added" || eventName == "product_added")
                    {
                        await syncService.SyncNow();
                    }
                });
            };
            Task.Run(async () => await socketSyncService.StartAsync());

            // Set default view (Dashboard)
            LoadForm(new DashboardForm(), "Dashboard", btnDashboard);
        }

        private void InitializeComponent()
        {
            this.sidebarPanel = new Panel();
            this.pbLogo = new PictureBox();
            
            this.btnDashboard = new Button();
            this.btnOrderPunch = new Button();
            this.btnOnlineOrders = new Button();
            this.btnMenu = new Button();
            this.btnDeals = new Button();
            this.btnReports = new Button();
            this.btnSettings = new Button();
            this.btnLogout = new Button();

            this.topBarPanel = new Panel();
            this.lblPageTitle = new Label();
            this.lblDateTime = new Label();
            this.pnlSyncStatus = new Panel();
            this.lblLastSync = new Label();
            this.btnSync = new Button();
            this.lblAdminName = new Label();
            
            this.contentPanel = new Panel();

            this.SuspendLayout();

            // Form settings
            this.ClientSize = new Size(1300, 800);
            this.Name = "MainForm";
            this.Text = "Hungry Hub - Admin Panel";
            this.WindowState = FormWindowState.Maximized;
            this.ResumeLayout(false);
        }

        private void SetupDashboardUI()
        {
            this.BackColor = Color.FromArgb(250, 249, 246); // Cream background

            // 1. SIDEBAR PANEL (Left, 220px width)
            sidebarPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 220,
                BackColor = Color.FromArgb(53, 57, 59), // Dark Slate Gray #35393b
                Padding = new Padding(0)
            };

            // Logo Image
            try
            {
                pbLogo = new PictureBox
                {
                    Image = Image.FromFile("logo.png"),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Location = new Point(20, 20),
                    Size = new Size(180, 80),
                    BackColor = Color.White
                };
                sidebarPanel.Controls.Add(pbLogo);
            }
            catch (Exception ex)
            {
                Logger.Log("Failed to load logo: " + ex.Message);
            }

            int yPos = 120;
            int btnHeight = 44;
            int spacing = 6;

            // Nav buttons instantiations
            btnDashboard = CreateSidebarButton("📊  Dashboard", yPos, btnDashboard_Click);
            sidebarPanel.Controls.Add(btnDashboard);
            yPos += btnHeight + spacing;

            btnOrderPunch = CreateSidebarButton("📦  Order Punch", yPos, btnOrderPunch_Click);
            sidebarPanel.Controls.Add(btnOrderPunch);
            yPos += btnHeight + spacing;

            btnOnlineOrders = CreateSidebarButton("📋  Online Orders", yPos, btnOnlineOrders_Click);
            sidebarPanel.Controls.Add(btnOnlineOrders);
            yPos += btnHeight + spacing;

            btnMenu = CreateSidebarButton("🍔  Menu Management", yPos, btnMenu_Click);
            sidebarPanel.Controls.Add(btnMenu);
            yPos += btnHeight + spacing;

            btnDeals = CreateSidebarButton("🔥  Deals", yPos, btnDeals_Click);
            sidebarPanel.Controls.Add(btnDeals);
            yPos += btnHeight + spacing;

            btnReports = CreateSidebarButton("📊  Reports", yPos, btnReports_Click);
            sidebarPanel.Controls.Add(btnReports);
            yPos += btnHeight + spacing;

            btnSettings = CreateSidebarButton("⚙️  Settings", yPos, btnSettings_Click);
            sidebarPanel.Controls.Add(btnSettings);

            // Bottom logout button
            btnLogout = new Button
            {
                Text = "🚪  Logout",
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                BackColor = Color.FromArgb(230, 57, 70), // Red Accent #E63946
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(190, 40),
                Location = new Point(15, sidebarPanel.Height - 65),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                Cursor = Cursors.Hand
            };
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.Click += btnLogout_Click;
            sidebarPanel.Controls.Add(btnLogout);

            // 2. TOP BAR (60px height)
            topBarPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.White,
                Padding = new Padding(20, 0, 20, 0)
            };
            topBarPanel.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(230, 230, 230), 1))
                {
                    e.Graphics.DrawLine(pen, 0, topBarPanel.Height - 1, topBarPanel.Width, topBarPanel.Height - 1);
                }
            };

            lblPageTitle = new Label
            {
                Text = "Dashboard",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(53, 57, 59),
                Location = new Point(20, 15),
                Size = new Size(300, 30),
                TextAlign = ContentAlignment.MiddleLeft
            };
            topBarPanel.Controls.Add(lblPageTitle);

            // Right side Topbar components
            lblAdminName = new Label
            {
                Text = $"👤 {adminEmail}",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(53, 57, 59),
                Size = new Size(180, 30),
                Location = new Point(topBarPanel.Width - 200, 15),
                TextAlign = ContentAlignment.MiddleRight,
                Anchor = AnchorStyles.Right | AnchorStyles.Top
            };
            topBarPanel.Controls.Add(lblAdminName);

            // Sync Dropdown Button
            btnSync = new Button
            {
                Text = "Sync 🔄",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Size = new Size(95, 30),
                Location = new Point(topBarPanel.Width - 310, 15),
                BackColor = Color.FromArgb(42, 157, 143),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Right | AnchorStyles.Top,
                Cursor = Cursors.Hand
            };
            btnSync.FlatAppearance.BorderSize = 0;
            topBarPanel.Controls.Add(btnSync);

            // Context Menu Strip for Sync Button
            ContextMenuStrip ctxSync = new ContextMenuStrip();
            ctxSync.Items.Add("🔄 Sync Now", null, async (s, e) => {
                await syncService.SyncNow();
            });
            ctxSync.Items.Add("📜 View Sync Logs", null, (s, e) => {
                using (var logsDlg = new SyncLogsForm())
                {
                    logsDlg.ShowDialog(this);
                }
            });
            ctxSync.Items.Add("🔁 Retry Failed", null, async (s, e) => {
                dbService.RetryFailedSyncs();
                await syncService.SyncNow();
            });

            btnSync.Click += (s, e) => {
                ctxSync.Show(btnSync, new Point(0, btnSync.Height));
            };

            // Last Sync Status Label
            lblLastSync = new Label
            {
                Text = "Last Sync: never",
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                ForeColor = Color.Gray,
                Size = new Size(190, 30),
                Location = new Point(topBarPanel.Width - 515, 15),
                TextAlign = ContentAlignment.MiddleRight,
                Anchor = AnchorStyles.Right | AnchorStyles.Top
            };
            topBarPanel.Controls.Add(lblLastSync);

            // Sync Status Indicator Dot
            pnlSyncStatus = new Panel
            {
                Size = new Size(12, 12),
                Location = new Point(topBarPanel.Width - 535, 24),
                BackColor = Color.FromArgb(230, 57, 70), // red/offline by default
                Anchor = AnchorStyles.Right | AnchorStyles.Top
            };
            pnlSyncStatus.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var brush = new SolidBrush(pnlSyncStatus.BackColor))
                {
                    e.Graphics.FillEllipse(brush, 0, 0, pnlSyncStatus.Width - 1, pnlSyncStatus.Height - 1);
                }
            };
            topBarPanel.Controls.Add(pnlSyncStatus);

            lblDateTime = new Label
            {
                Text = DateTime.Now.ToString("dd MMM yyyy, hh:mm:ss tt"),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
                ForeColor = Color.Gray,
                Size = new Size(200, 30),
                Location = new Point(topBarPanel.Width - 750, 15),
                TextAlign = ContentAlignment.MiddleRight,
                Anchor = AnchorStyles.Right | AnchorStyles.Top
            };
            topBarPanel.Controls.Add(lblDateTime);

            // 3. CONTENT AREA
            contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(250, 249, 246)
            };

            // Add containers to the form
            this.Controls.Add(contentPanel);
            this.Controls.Add(topBarPanel);
            this.Controls.Add(sidebarPanel);
        }

        private Button CreateSidebarButton(string text, int yPosition, EventHandler clickHandler)
        {
            var btn = new Button
            {
                Text = text,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(15, yPosition),
                Size = new Size(190, 44),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 0, 0),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(229, 152, 7); // Hover Orange #E59807
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(253, 175, 38); // Active Orange #FDAF26
            btn.Click += clickHandler;
            return btn;
        }

        private void HighlightNavButton(Button activeBtn)
        {
            foreach (Control ctrl in sidebarPanel.Controls)
            {
                if (ctrl is Button btn && btn != btnLogout)
                {
                    btn.BackColor = Color.Transparent;
                    btn.ForeColor = Color.White;
                }
            }
            activeBtn.BackColor = Color.FromArgb(253, 175, 38); // selected color #FDAF26
            activeBtn.ForeColor = Color.White;
        }

        private void LoadForm(Form subForm, string title, Button activeBtn)
        {
            contentPanel.Controls.Clear();
            lblPageTitle.Text = title;
            HighlightNavButton(activeBtn);

            subForm.TopLevel = false;
            subForm.FormBorderStyle = FormBorderStyle.None;
            subForm.Dock = DockStyle.Fill;
            contentPanel.Controls.Add(subForm);
            subForm.Show();
        }

        // Navigation actions
        private void btnDashboard_Click(object sender, EventArgs e) => LoadForm(new DashboardForm(), "Dashboard", btnDashboard);
        private void btnOrderPunch_Click(object sender, EventArgs e) => LoadForm(new OrderPunchForm(), "Order Punch", btnOrderPunch);
        private void btnOnlineOrders_Click(object sender, EventArgs e) => LoadForm(new OnlineOrdersForm(adminEmail), "Online Orders", btnOnlineOrders);
        private void btnMenu_Click(object sender, EventArgs e) => LoadForm(new MenuManagementForm(), "Menu Management", btnMenu);
        private void btnDeals_Click(object sender, EventArgs e) => LoadForm(new DealManagementForm(), "Deals", btnDeals);
        private void btnReports_Click(object sender, EventArgs e) => LoadForm(new ReportsForm(), "Reports", btnReports);
        private void btnSettings_Click(object sender, EventArgs e) => LoadForm(new SettingsForm(adminEmail), "Settings", btnSettings);

        private void btnLogout_Click(object sender, EventArgs e)
        {
            clockTimer?.Stop();
            this.Close();
            var loginForm = new LoginForm();
            loginForm.Show();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            clockTimer?.Stop();
            syncService?.StopAutoSync();
            base.OnFormClosing(e);
        }

        private void UpdateSyncIndicator(bool isOnline, string state)
        {
            if (state == "Syncing")
            {
                pnlSyncStatus.BackColor = Color.FromArgb(244, 162, 97); // Orange/Yellow
                lblLastSync.Text = "Syncing...";
            }
            else if (state == "Synced")
            {
                pnlSyncStatus.BackColor = Color.FromArgb(42, 157, 143); // Teal
                lblLastSync.Text = $"Last Sync: {DateTime.Now:hh:mm:ss tt}";
            }
            else if (state == "Offline")
            {
                pnlSyncStatus.BackColor = Color.FromArgb(230, 57, 70); // Red
                lblLastSync.Text = "Offline";
            }
            else // Failed
            {
                pnlSyncStatus.BackColor = Color.FromArgb(230, 57, 70); // Red
                lblLastSync.Text = "Sync failed";

                // Toast Notification
                var toast = new ToastForm("Sync operation failed. Please check internet connectivity or view sync logs.", "Sync Failure");
                toast.Show();
            }

            pnlSyncStatus.Invalidate();
        }

        private void ShowNewOrderNotification(int count)
        {
            // Flash the Online Orders nav button briefly
            btnOnlineOrders.BackColor = Color.FromArgb(230, 57, 70);
            var resetTimer = new System.Windows.Forms.Timer { Interval = 3000 };
            resetTimer.Tick += (s, e) =>
            {
                btnOnlineOrders.BackColor = Color.Transparent;
                resetTimer.Stop();
                resetTimer.Dispose();
            };
            resetTimer.Start();

            MessageBox.Show(
                $"🔔 {count} new online order{(count > 1 ? "s" : "")} received! Go to Online Orders to review.",
                "New Order Alert",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        // Sliding/fading borderless Toast Form
        private class ToastForm : Form
        {
            private System.Windows.Forms.Timer closeTimer;
            public ToastForm(string message, string title = "Sync Alert")
            {
                this.FormBorderStyle = FormBorderStyle.None;
                this.ShowInTaskbar = false;
                this.TopMost = true;
                this.Size = new Size(320, 80);
                this.BackColor = Color.FromArgb(230, 57, 70); // #E63946 Primary Red
                this.ForeColor = Color.White;

                Label lblTitle = new Label { Text = "⚠️ " + title, Font = new Font("Segoe UI", 10.5f, FontStyle.Bold), Location = new Point(15, 12), AutoSize = true };
                Label lblMsg = new Label
                {
                    Text = message,
                    Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                    Location = new Point(15, 34),
                    Size = new Size(290, 40),
                    ForeColor = Color.FromArgb(245, 245, 245)
                };
                this.Controls.Add(lblTitle);
                this.Controls.Add(lblMsg);

                // Position bottom right
                Rectangle workingArea = Screen.GetWorkingArea(this);
                this.Location = new Point(workingArea.Right - this.Width - 15, workingArea.Bottom - this.Height - 15);

                closeTimer = new System.Windows.Forms.Timer { Interval = 4500 };
                closeTimer.Tick += (s, e) => { closeTimer.Stop(); this.Close(); };
                closeTimer.Start();
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                using (var pen = new Pen(Color.FromArgb(180, 0, 0), 1.5f))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, this.Width - 1, this.Height - 1);
                }
            }
        }
    }
}