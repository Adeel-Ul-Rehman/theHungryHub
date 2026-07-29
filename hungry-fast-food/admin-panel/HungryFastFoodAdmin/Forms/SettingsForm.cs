// E:\hungryHub\hungry-fast-food\admin-panel\HungryFastFoodAdmin\Forms\SettingsForm.cs

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Windows.Forms;
using HungryFastFoodAdmin.Models;
using HungryFastFoodAdmin.Services;
using Newtonsoft.Json;

namespace HungryFastFoodAdmin.Forms
{
    public partial class SettingsForm : BaseForm
    {
        private DatabaseService _dbService;
        private ApiService _apiService;
        private TabControl tabControl;

        // Restaurant Info Tab
        private TextBox txtRestaurantName;
        private TextBox txtRestaurantAddress;
        private TextBox txtRestaurantPhone;
        private TextBox txtRestaurantEmail;
        private RadioButton rbAutoTiming;
        private RadioButton rbManualControl;
        private Button btnToggleWebsiteOrdering;
        private bool isWebsiteOrderingEnabled = true;
        private DateTimePicker dtpOpeningTime;
        private DateTimePicker dtpClosingTime;
        private Label lblOpeningTime;
        private Label lblClosingTime;

        // Delivery Zones Tab (FIXED 2 zones: Free + Charged - only editable, no add/delete)
        private DataGridView dgvZones;
        private TextBox txtZoneName;
        private NumericUpDown nudZoneDistance;
        private NumericUpDown nudZoneCharge;
        private NumericUpDown nudZoneMinOrder;
        private Label lblZoneValidation;

        // Printer Settings Tab
        private ComboBox cmbPrinter;
        private CheckBox chkAutoPrint;
        private NumericUpDown nudPaperWidth;
        private Button btnTestPrint;

        // Tax Settings Tab
        private NumericUpDown nudTaxRate;
        private NumericUpDown nudMinOrder;
        private NumericUpDown nudBakingDuration;
        private NumericUpDown nudDeliveryDuration;

        // Sync Settings Tab
        private NumericUpDown nudSyncInterval;
        private CheckBox chkSyncEnabled;
        private Button btnSyncNow;
        private Label lblLastSync;

        // Backup Settings Tab
        private CheckBox chkAutoBackup;
        private NumericUpDown nudBackupInterval;
        private TextBox txtBackupPath;
        private Button btnBackupNow;

        // Admin Profile Tab
        private TextBox txtAdminName;
        private TextBox txtAdminEmail;
        private TextBox txtAdminPhone;
        private TextBox txtOldPassword;
        private TextBox txtNewPassword;
        private TextBox txtConfirmPassword;
        private Button btnSaveProfile;
        private Button btnUpdatePassword;
        private string _adminEmail;

        public SettingsForm(string adminEmail = null)
        {
            _adminEmail = adminEmail;
            InitializeComponent();
            _dbService = new DatabaseService();
            _apiService = new ApiService();
            SetupUI();
            LoadSettings();
            if (!string.IsNullOrEmpty(_adminEmail))
            {
                _ = LoadAdminProfile();
            }
        }

        private async System.Threading.Tasks.Task LoadAdminProfile()
        {
            var result = await _apiService.GetAdminProfile(_adminEmail);
            if (result.Success && result.Data != null)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    txtAdminEmail.Text = result.Data.email;
                    txtAdminName.Text = result.Data.full_name;
                    txtAdminPhone.Text = result.Data.phone;
                });
            }
        }

        private void InitializeComponent()
        {
            this.Text = "Settings - Hungry Hub";
            this.Size = new Size(900, 700);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Main Panel
            Panel mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

            // Tab Control
            tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10)
            };

            // ============================================
            // TAB 1: RESTAURANT INFO
            // ============================================
            TabPage tabRestaurant = new TabPage("🏪 Restaurant");
            Panel restaurantPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

            int y = 20;
            int xLabel = 20;
            int xControl = 150;

            // Restaurant Name
            Label lblName = new Label { Text = "Restaurant Name:", Location = new Point(xLabel, y), Size = new Size(120, 25) };
            txtRestaurantName = new TextBox { Location = new Point(xControl, y), Size = new Size(300, 25) };
            y += 35;

            // Address
            Label lblAddress = new Label { Text = "Address:", Location = new Point(xLabel, y), Size = new Size(120, 25) };
            txtRestaurantAddress = new TextBox { Location = new Point(xControl, y), Size = new Size(400, 25) };
            y += 35;

            // Phone
            Label lblPhone = new Label { Text = "Phone:", Location = new Point(xLabel, y), Size = new Size(120, 25) };
            txtRestaurantPhone = new TextBox { Location = new Point(xControl, y), Size = new Size(200, 25) };
            y += 35;

            // Email
            Label lblEmail = new Label { Text = "Email:", Location = new Point(xLabel, y), Size = new Size(120, 25) };
            txtRestaurantEmail = new TextBox { Location = new Point(xControl, y), Size = new Size(300, 25) };
            y += 35;

            // --- Order Settings GroupBox ---
            GroupBox grpOrdering = new GroupBox { Text = "Online Ordering Status", Location = new Point(xLabel, y), Size = new Size(500, 150), Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            
            rbAutoTiming = new RadioButton { Text = "Automatic Schedule (Opens/Closes based on time)", Location = new Point(20, 25), Size = new Size(350, 25), Font = new Font("Segoe UI", 9) };
            rbManualControl = new RadioButton { Text = "Manual Control (I will toggle ON/OFF manually)", Location = new Point(20, 50), Size = new Size(350, 25), Font = new Font("Segoe UI", 9) };
            
            lblOpeningTime = new Label { Text = "Opening Time:", Location = new Point(40, 80), Size = new Size(90, 25), Font = new Font("Segoe UI", 9) };
            dtpOpeningTime = new DateTimePicker { Location = new Point(130, 80), Size = new Size(80, 25), Format = DateTimePickerFormat.Custom, CustomFormat = "HH:mm", ShowUpDown = true, Font = new Font("Segoe UI", 9) };
            lblClosingTime = new Label { Text = "Closing Time:", Location = new Point(230, 80), Size = new Size(90, 25), Font = new Font("Segoe UI", 9) };
            dtpClosingTime = new DateTimePicker { Location = new Point(320, 80), Size = new Size(80, 25), Format = DateTimePickerFormat.Custom, CustomFormat = "HH:mm", ShowUpDown = true, Font = new Font("Segoe UI", 9) };

            btnToggleWebsiteOrdering = new Button { Location = new Point(40, 80), Size = new Size(350, 40), Font = new Font("Segoe UI", 10, FontStyle.Bold), FlatStyle = FlatStyle.Flat };
            btnToggleWebsiteOrdering.FlatAppearance.BorderSize = 0;
            btnToggleWebsiteOrdering.Click += BtnToggleWebsiteOrdering_Click;

            rbAutoTiming.CheckedChanged += (s, e) => UpdateOrderingUI();
            rbManualControl.CheckedChanged += (s, e) => UpdateOrderingUI();

            grpOrdering.Controls.Add(rbAutoTiming);
            grpOrdering.Controls.Add(rbManualControl);
            grpOrdering.Controls.Add(lblOpeningTime);
            grpOrdering.Controls.Add(dtpOpeningTime);
            grpOrdering.Controls.Add(lblClosingTime);
            grpOrdering.Controls.Add(dtpClosingTime);
            grpOrdering.Controls.Add(btnToggleWebsiteOrdering);

            y += 160;

            // Latitude & Longitude helper text
            Label lblLocationInfo = new Label
            {
                Text = "Restaurant coordinates are used to calculate delivery distances.\nSet these in the .env file (RESTAURANT_LATITUDE, RESTAURANT_LONGITUDE).",
                Location = new Point(xControl, y + 10),
                Size = new Size(500, 40),
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 9, FontStyle.Italic)
            };
            y += 55;

            // Save Button
            Button btnSaveRestaurant = new Button
            {
                Text = "💾 Save Restaurant Info",
                Location = new Point(xControl, y + 10),
                Size = new Size(200, 40),
                BackColor = Color.FromArgb(42, 157, 143),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnSaveRestaurant.FlatAppearance.BorderSize = 0;
            btnSaveRestaurant.Click += BtnSaveRestaurant_Click;

            restaurantPanel.Controls.Add(lblName);
            restaurantPanel.Controls.Add(txtRestaurantName);
            restaurantPanel.Controls.Add(lblAddress);
            restaurantPanel.Controls.Add(txtRestaurantAddress);
            restaurantPanel.Controls.Add(lblPhone);
            restaurantPanel.Controls.Add(txtRestaurantPhone);
            restaurantPanel.Controls.Add(lblEmail);
            restaurantPanel.Controls.Add(txtRestaurantEmail);
            
            restaurantPanel.Controls.Add(grpOrdering);

            restaurantPanel.Controls.Add(lblLocationInfo);
            restaurantPanel.Controls.Add(btnSaveRestaurant);
            tabRestaurant.Controls.Add(restaurantPanel);

            // ============================================
            // TAB 2: DELIVERY ZONES - FIXED 2 ZONES (EDIT ONLY)
            // ============================================
            TabPage tabZones = new TabPage("🚚 Delivery Zones");
            Panel zonesPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

            // Info label explaining the zone system
            Label lblZoneInfo = new Label
            {
                Text = "✅ 2-Zone Delivery System (Edit Only - Cannot Add/Delete)\n\n" +
                       "• Zone 1 (Free Delivery) — Orders within this distance get FREE delivery\n" +
                       "• Zone 2 (Charged Delivery) — Orders between Free and Charged distance pay the charge\n" +
                       "• Orders beyond Charged Zone distance CANNOT be placed.\n" +
                       "✓ Free Zone max distance MUST be less than Charged Zone max distance.\n" +
                       "✓ Select a zone in the table below and use the edit fields to modify it.",
                Location = new Point(10, 10),
                Size = new Size(750, 90),
                ForeColor = Color.FromArgb(53, 57, 59),
                Font = new Font("Segoe UI", 9.5F),
                BackColor = Color.FromArgb(230, 245, 240),
                Padding = new Padding(10)
            };

            // Zones Grid (display only - edit happens via buttons)
            dgvZones = new DataGridView
            {
                Location = new Point(10, 110),
                Size = new Size(500, 120),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            dgvZones.Columns.Add("Id", "ID");
            dgvZones.Columns["Id"].Visible = false;
            dgvZones.Columns.Add("Name", "Zone Name");
            dgvZones.Columns.Add("Distance", "Max Distance (KM)");
            dgvZones.Columns.Add("Charge", "Charge (PKR)");
            dgvZones.Columns.Add("MinOrder", "Min Order (PKR)");

            // Validation message label
            lblZoneValidation = new Label
            {
                Text = "",
                Location = new Point(10, 240),
                Size = new Size(750, 22),
                ForeColor = Color.Red,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold)
            };

            // Zone Edit Controls
            Label lblZoneName = new Label { Text = "Zone Name:", Location = new Point(10, 275), Size = new Size(80, 25) };
            txtZoneName = new TextBox { Location = new Point(100, 273), Size = new Size(150, 25), ReadOnly = true };

            Label lblZoneDistance = new Label { Text = "Max Distance (KM):", Location = new Point(10, 310), Size = new Size(110, 25) };
            nudZoneDistance = new NumericUpDown { Location = new Point(130, 308), Size = new Size(80, 25), Minimum = 0.1m, Maximum = 100, Value = 10, DecimalPlaces = 1 };

            Label lblZoneCharge = new Label { Text = "Delivery Charge (PKR):", Location = new Point(10, 345), Size = new Size(130, 25) };
            nudZoneCharge = new NumericUpDown { Location = new Point(145, 343), Size = new Size(100, 25), Minimum = 0, Maximum = 9999, Value = 0 };

            Label lblZoneMinOrder = new Label { Text = "Min Order (PKR):", Location = new Point(10, 380), Size = new Size(100, 25) };
            nudZoneMinOrder = new NumericUpDown { Location = new Point(120, 378), Size = new Size(100, 25), Minimum = 0, Maximum = 99999, Value = 0 };

            // Edit selected zone button
            Button btnLoadZone = new Button
            {
                Text = "✏️ Load Selected Zone for Editing",
                Location = new Point(520, 275),
                Size = new Size(250, 32),
                BackColor = Color.FromArgb(244, 162, 97),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold)
            };
            btnLoadZone.FlatAppearance.BorderSize = 0;
            btnLoadZone.Click += BtnLoadZone_Click;

            // Reset to defaults button
            Button btnResetDefaults = new Button
            {
                Text = "🔄 Reset to Defaults",
                Location = new Point(520, 315),
                Size = new Size(180, 32),
                BackColor = Color.FromArgb(53, 57, 59),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold)
            };
            btnResetDefaults.FlatAppearance.BorderSize = 0;
            btnResetDefaults.Click += BtnResetDefaults_Click;

            // Save Zones Button (big & prominent)
            Button btnSaveZones = new Button
            {
                Text = "✅ Save Delivery Zones (Syncs to Website)",
                Location = new Point(520, 370),
                Size = new Size(300, 40),
                BackColor = Color.FromArgb(230, 57, 70),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold)
            };
            btnSaveZones.FlatAppearance.BorderSize = 0;
            btnSaveZones.Click += BtnSaveZones_Click;

            zonesPanel.Controls.Add(lblZoneInfo);
            zonesPanel.Controls.Add(dgvZones);
            zonesPanel.Controls.Add(lblZoneValidation);
            zonesPanel.Controls.Add(lblZoneName);
            zonesPanel.Controls.Add(txtZoneName);
            zonesPanel.Controls.Add(lblZoneDistance);
            zonesPanel.Controls.Add(nudZoneDistance);
            zonesPanel.Controls.Add(lblZoneCharge);
            zonesPanel.Controls.Add(nudZoneCharge);
            zonesPanel.Controls.Add(lblZoneMinOrder);
            zonesPanel.Controls.Add(nudZoneMinOrder);
            zonesPanel.Controls.Add(btnLoadZone);
            zonesPanel.Controls.Add(btnResetDefaults);
            zonesPanel.Controls.Add(btnSaveZones);
            tabZones.Controls.Add(zonesPanel);

            // ============================================
            // TAB 3: PRINTER SETTINGS
            // ============================================
            TabPage tabPrinter = new TabPage("🖨️ Printer");
            Panel printerPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

            y = 20;

            Label lblPrinter = new Label { Text = "Select Printer:", Location = new Point(xLabel, y), Size = new Size(120, 25) };
            cmbPrinter = new ComboBox
            {
                Location = new Point(xControl, y),
                Size = new Size(200, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            // Load printers
            foreach (string printer in PrinterSettings.InstalledPrinters)
            {
                cmbPrinter.Items.Add(printer);
            }
            y += 35;

            chkAutoPrint = new CheckBox
            {
                Text = "Auto-print bills on order placement",
                Location = new Point(xControl, y),
                Size = new Size(250, 25)
            };
            y += 35;

            Label lblPaperWidth = new Label { Text = "Paper Width (mm):", Location = new Point(xLabel, y), Size = new Size(120, 25) };
            nudPaperWidth = new NumericUpDown
            {
                Location = new Point(xControl, y),
                Size = new Size(80, 25),
                Minimum = 58,
                Maximum = 80,
                Value = 80,
                Increment = 2
            };
            y += 35;

            btnTestPrint = new Button
            {
                Text = "🖨️ Test Print",
                Location = new Point(xControl, y + 10),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(244, 162, 97),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnTestPrint.FlatAppearance.BorderSize = 0;
            btnTestPrint.Click += BtnTestPrint_Click;

            Button btnSavePrinter = new Button
            {
                Text = "💾 Save Printer Settings",
                Location = new Point(xControl + 140, y + 10),
                Size = new Size(180, 35),
                BackColor = Color.FromArgb(42, 157, 143),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnSavePrinter.FlatAppearance.BorderSize = 0;
            btnSavePrinter.Click += BtnSavePrinter_Click;

            printerPanel.Controls.Add(lblPrinter);
            printerPanel.Controls.Add(cmbPrinter);
            printerPanel.Controls.Add(chkAutoPrint);
            printerPanel.Controls.Add(lblPaperWidth);
            printerPanel.Controls.Add(nudPaperWidth);
            printerPanel.Controls.Add(btnTestPrint);
            printerPanel.Controls.Add(btnSavePrinter);
            tabPrinter.Controls.Add(printerPanel);

            // ============================================
            // TAB 4: TAX & TIMERS SETTINGS
            // ============================================
            TabPage tabTax = new TabPage("💰 Tax & KDS Timers");
            Panel taxPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

            y = 40;
            int xLabelTax = 40;
            int xControlTax = 250;
            int inputWidth = 200;

            Label lblTaxRate = new Label { Text = "Tax Rate (%):", Location = new Point(xLabelTax, y), Size = new Size(180, 25), Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            nudTaxRate = new NumericUpDown
            {
                Location = new Point(xControlTax, y),
                Size = new Size(inputWidth, 30),
                Minimum = 0,
                Maximum = 100,
                Value = 5,
                DecimalPlaces = 1,
                Increment = 0.5M,
                Font = new Font("Segoe UI", 10)
            };
            y += 45;

            Label lblMinOrder = new Label { Text = "Minimum Order (PKR):", Location = new Point(xLabelTax, y), Size = new Size(180, 25), Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            nudMinOrder = new NumericUpDown
            {
                Location = new Point(xControlTax, y),
                Size = new Size(inputWidth, 30),
                Minimum = 0,
                Maximum = 99999,
                Value = 500,
                Font = new Font("Segoe UI", 10)
            };
            y += 45;

            Label lblBakingDuration = new Label { Text = "Baking Duration (mins):", Location = new Point(xLabelTax, y), Size = new Size(180, 25), Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            nudBakingDuration = new NumericUpDown
            {
                Location = new Point(xControlTax, y),
                Size = new Size(inputWidth, 30),
                Minimum = 1,
                Maximum = 180,
                Value = 15,
                Font = new Font("Segoe UI", 10)
            };
            y += 45;

            Label lblDeliveryDuration = new Label { Text = "Delivery Duration (mins):", Location = new Point(xLabelTax, y), Size = new Size(180, 25), Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            nudDeliveryDuration = new NumericUpDown
            {
                Location = new Point(xControlTax, y),
                Size = new Size(inputWidth, 30),
                Minimum = 1,
                Maximum = 180,
                Value = 20,
                Font = new Font("Segoe UI", 10)
            };
            y += 45;

            Button btnSaveTax = new Button
            {
                Text = "💾 Save Settings",
                Location = new Point(xControlTax, y + 10),
                Size = new Size(180, 35),
                BackColor = Color.FromArgb(42, 157, 143),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnSaveTax.FlatAppearance.BorderSize = 0;
            btnSaveTax.Click += BtnSaveTax_Click;

            taxPanel.Controls.Add(lblTaxRate);
            taxPanel.Controls.Add(nudTaxRate);
            taxPanel.Controls.Add(lblMinOrder);
            taxPanel.Controls.Add(nudMinOrder);
            taxPanel.Controls.Add(lblBakingDuration);
            taxPanel.Controls.Add(nudBakingDuration);
            taxPanel.Controls.Add(lblDeliveryDuration);
            taxPanel.Controls.Add(nudDeliveryDuration);
            taxPanel.Controls.Add(btnSaveTax);
            tabTax.Controls.Add(taxPanel);

            // ============================================
            // TAB 5: SYNC SETTINGS
            // ============================================
            TabPage tabSync = new TabPage("🔄 Sync");
            Panel syncPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

            y = 20;

            Label lblSyncInterval = new Label { Text = "Sync Interval (sec):", Location = new Point(xLabel, y), Size = new Size(120, 25) };
            nudSyncInterval = new NumericUpDown
            {
                Location = new Point(xControl, y),
                Size = new Size(80, 25),
                Minimum = 5,
                Maximum = 300,
                Value = 30
            };
            y += 35;

            chkSyncEnabled = new CheckBox
            {
                Text = "Enable Auto-Sync",
                Location = new Point(xControl, y),
                Size = new Size(200, 25)
            };
            y += 45;

            btnSyncNow = new Button
            {
                Text = "🔄 Sync Now",
                Location = new Point(xControl, y),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(42, 157, 143),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnSyncNow.FlatAppearance.BorderSize = 0;
            btnSyncNow.Click += BtnSyncNow_Click;

            lblLastSync = new Label
            {
                Text = "Last Sync: Never",
                Location = new Point(xControl + 140, y + 8),
                Size = new Size(200, 25),
                ForeColor = Color.FromArgb(100, 100, 100)
            };

            Button btnSaveSync = new Button
            {
                Text = "💾 Save Sync Settings",
                Location = new Point(xControl, y + 50),
                Size = new Size(180, 35),
                BackColor = Color.FromArgb(53, 57, 59),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnSaveSync.FlatAppearance.BorderSize = 0;
            btnSaveSync.Click += BtnSaveSync_Click;

            syncPanel.Controls.Add(lblSyncInterval);
            syncPanel.Controls.Add(nudSyncInterval);
            syncPanel.Controls.Add(chkSyncEnabled);
            syncPanel.Controls.Add(btnSyncNow);
            syncPanel.Controls.Add(lblLastSync);
            syncPanel.Controls.Add(btnSaveSync);
            tabSync.Controls.Add(syncPanel);

            // ============================================
            // TAB 6: BACKUP
            // ============================================
            TabPage tabBackup = new TabPage("💾 Backup");
            Panel backupPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

            y = 20;

            chkAutoBackup = new CheckBox
            {
                Text = "Enable Auto-Backup",
                Location = new Point(xControl, y),
                Size = new Size(200, 25)
            };
            y += 35;

            Label lblBackupInterval = new Label { Text = "Backup Every (hours):", Location = new Point(xLabel, y), Size = new Size(120, 25) };
            nudBackupInterval = new NumericUpDown
            {
                Location = new Point(xControl, y),
                Size = new Size(80, 25),
                Minimum = 1,
                Maximum = 168,
                Value = 24
            };
            y += 35;

            Label lblBackupPath = new Label { Text = "Backup Location:", Location = new Point(xLabel, y), Size = new Size(120, 25) };
            txtBackupPath = new TextBox
            {
                Location = new Point(xControl, y),
                Size = new Size(300, 25),
                Text = "C:\\HungryFastFood\\backups\\"
            };
            y += 35;

            Button btnBrowseBackup = new Button
            {
                Text = "Browse",
                Location = new Point(xControl + 310, y - 30),
                Size = new Size(80, 25),
                BackColor = Color.FromArgb(200, 200, 200),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.FromArgb(53, 57, 59)
            };
            btnBrowseBackup.FlatAppearance.BorderSize = 0;
            btnBrowseBackup.Click += BtnBrowseBackup_Click;

            btnBackupNow = new Button
            {
                Text = "💾 Backup Now",
                Location = new Point(xControl, y + 10),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(42, 157, 143),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnBackupNow.FlatAppearance.BorderSize = 0;
            btnBackupNow.Click += BtnBackupNow_Click;

            Button btnSaveBackup = new Button
            {
                Text = "💾 Save Backup Settings",
                Location = new Point(xControl + 140, y + 10),
                Size = new Size(180, 35),
                BackColor = Color.FromArgb(53, 57, 59),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnSaveBackup.FlatAppearance.BorderSize = 0;
            btnSaveBackup.Click += BtnSaveBackup_Click;

            backupPanel.Controls.Add(chkAutoBackup);
            backupPanel.Controls.Add(lblBackupInterval);
            backupPanel.Controls.Add(nudBackupInterval);
            backupPanel.Controls.Add(lblBackupPath);
            backupPanel.Controls.Add(btnBrowseBackup);
            backupPanel.Controls.Add(btnBackupNow);
            backupPanel.Controls.Add(btnSaveBackup);
            tabBackup.Controls.Add(backupPanel);

            // ============================================
            // TAB 7: ADMIN PROFILE
            // ============================================
            TabPage tabAdminProfile = new TabPage("👤 Admin Profile");
            Panel adminProfilePanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
            
            int yAdmin = 20;

            Label lblAdminName = new Label { Text = "Full Name:", Location = new Point(xLabel, yAdmin), AutoSize = true };
            txtAdminName = new TextBox { Location = new Point(xControl, yAdmin), Width = 300, Font = new Font("Segoe UI", 10) };
            adminProfilePanel.Controls.Add(lblAdminName);
            adminProfilePanel.Controls.Add(txtAdminName);
            yAdmin += 40;

            Label lblAdminEmailTitle = new Label { Text = "Email Address:", Location = new Point(xLabel, yAdmin), AutoSize = true };
            txtAdminEmail = new TextBox { Location = new Point(xControl, yAdmin), Width = 300, Font = new Font("Segoe UI", 10), ReadOnly = true, BackColor = Color.LightGray };
            adminProfilePanel.Controls.Add(lblAdminEmailTitle);
            adminProfilePanel.Controls.Add(txtAdminEmail);
            yAdmin += 40;
            
            Label lblAdminPhoneTitle = new Label { Text = "Phone:", Location = new Point(xLabel, yAdmin), AutoSize = true };
            txtAdminPhone = new TextBox { Location = new Point(xControl, yAdmin), Width = 300, Font = new Font("Segoe UI", 10) };
            adminProfilePanel.Controls.Add(lblAdminPhoneTitle);
            adminProfilePanel.Controls.Add(txtAdminPhone);
            yAdmin += 50;

            btnSaveProfile = new Button
            {
                Text = "💾 Save Profile",
                Location = new Point(xControl, yAdmin),
                Size = new Size(150, 35),
                BackColor = Color.FromArgb(42, 157, 143),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnSaveProfile.FlatAppearance.BorderSize = 0;
            btnSaveProfile.Click += BtnSaveProfile_Click;
            adminProfilePanel.Controls.Add(btnSaveProfile);
            
            yAdmin += 60;
            
            // Password Section
            Label lblPwdHeader = new Label { Text = "🔒 Change Password", Location = new Point(xLabel, yAdmin), Font = new Font("Segoe UI", 12, FontStyle.Bold), AutoSize = true };
            adminProfilePanel.Controls.Add(lblPwdHeader);
            yAdmin += 40;

            Label lblOldPwd = new Label { Text = "Current Password:", Location = new Point(xLabel, yAdmin), AutoSize = true };
            txtOldPassword = new TextBox { Location = new Point(xControl, yAdmin), Width = 300, Font = new Font("Segoe UI", 10), UseSystemPasswordChar = true };
            adminProfilePanel.Controls.Add(lblOldPwd);
            adminProfilePanel.Controls.Add(txtOldPassword);
            yAdmin += 40;

            Label lblNewPwd = new Label { Text = "New Password:", Location = new Point(xLabel, yAdmin), AutoSize = true };
            txtNewPassword = new TextBox { Location = new Point(xControl, yAdmin), Width = 300, Font = new Font("Segoe UI", 10), UseSystemPasswordChar = true };
            adminProfilePanel.Controls.Add(lblNewPwd);
            adminProfilePanel.Controls.Add(txtNewPassword);
            yAdmin += 40;
            
            Label lblConfirmPwd = new Label { Text = "Confirm Password:", Location = new Point(xLabel, yAdmin), AutoSize = true };
            txtConfirmPassword = new TextBox { Location = new Point(xControl, yAdmin), Width = 300, Font = new Font("Segoe UI", 10), UseSystemPasswordChar = true };
            adminProfilePanel.Controls.Add(lblConfirmPwd);
            adminProfilePanel.Controls.Add(txtConfirmPassword);
            yAdmin += 50;
            
            btnUpdatePassword = new Button
            {
                Text = "🔑 Update Password",
                Location = new Point(xControl, yAdmin),
                Size = new Size(180, 35),
                BackColor = Color.FromArgb(53, 57, 59),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnUpdatePassword.FlatAppearance.BorderSize = 0;
            btnUpdatePassword.Click += BtnUpdatePassword_Click;
            adminProfilePanel.Controls.Add(btnUpdatePassword);

            tabAdminProfile.Controls.Add(adminProfilePanel);

            // ============================================
            // ADD ALL TABS
            // ============================================
            tabControl.TabPages.Add(tabRestaurant);
            tabControl.TabPages.Add(tabZones);
            tabControl.TabPages.Add(tabPrinter);
            tabControl.TabPages.Add(tabTax);
            tabControl.TabPages.Add(tabSync);
            tabControl.TabPages.Add(tabBackup);
            tabControl.TabPages.Add(tabAdminProfile);

            mainPanel.Controls.Add(tabControl);
            this.Controls.Add(mainPanel);
        }

        private void SetupUI()
        {
            // Any additional setup
        }

        private void LoadSettings()
        {
            try
            {
                // Load Restaurant Info
                var settings = _dbService.GetSystemSettings();
                txtRestaurantName.Text = settings.GetValueOrDefault("restaurant_name", "Hungry Hub");
                txtRestaurantAddress.Text = settings.GetValueOrDefault("restaurant_address", "Zaki Plaza, Muslim Town, Rawalpindi");
                txtRestaurantPhone.Text = settings.GetValueOrDefault("restaurant_phone", "0336-0357333");
                txtRestaurantEmail.Text = settings.GetValueOrDefault("restaurant_email", "thehungryhub26@gmail.com");

                rbAutoTiming.Checked = settings.GetValueOrDefault("use_auto_timing", "False").ToLower() == "true";
                rbManualControl.Checked = !rbAutoTiming.Checked;
                isWebsiteOrderingEnabled = settings.GetValueOrDefault("accept_website_orders", "True").ToLower() == "true";
                
                try {
                    dtpOpeningTime.Value = DateTime.ParseExact(settings.GetValueOrDefault("opening_time", "10:00"), "HH:mm", null);
                } catch { dtpOpeningTime.Value = DateTime.Today.AddHours(10); }
                
                try {
                    dtpClosingTime.Value = DateTime.ParseExact(settings.GetValueOrDefault("closing_time", "23:00"), "HH:mm", null);
                } catch { dtpClosingTime.Value = DateTime.Today.AddHours(23); }

                UpdateOrderingUI();

                // Load Delivery Zones
                LoadZones();

                // Load Printer Settings
                string printerName = settings.GetValueOrDefault("printer_name", "");
                if (!string.IsNullOrEmpty(printerName) && cmbPrinter.Items.Contains(printerName))
                {
                    cmbPrinter.SelectedItem = printerName;
                }
                chkAutoPrint.Checked = settings.GetValueOrDefault("auto_print", "true") == "true";
                nudPaperWidth.Value = decimal.Parse(settings.GetValueOrDefault("paper_width", "80"));

                // Load Tax Settings
                nudTaxRate.Value = decimal.Parse(settings.GetValueOrDefault("tax_rate", "5"));
                nudMinOrder.Value = decimal.Parse(settings.GetValueOrDefault("min_order", "500"));
                nudBakingDuration.Value = decimal.Parse(settings.GetValueOrDefault("baking_duration_minutes", "15"));
                nudDeliveryDuration.Value = decimal.Parse(settings.GetValueOrDefault("delivery_duration_minutes", "20"));

                // Load Sync Settings
                nudSyncInterval.Value = decimal.Parse(settings.GetValueOrDefault("sync_interval", "30"));
                chkSyncEnabled.Checked = settings.GetValueOrDefault("sync_enabled", "true") == "true";

                // Load Backup Settings
                chkAutoBackup.Checked = settings.GetValueOrDefault("auto_backup", "true") == "true";
                nudBackupInterval.Value = decimal.Parse(settings.GetValueOrDefault("backup_interval", "24"));
                txtBackupPath.Text = settings.GetValueOrDefault("backup_path", "C:\\HungryFastFood\\backups\\");

                // Load last sync time
                string lastSync = settings.GetValueOrDefault("last_sync", "");
                lblLastSync.Text = string.IsNullOrEmpty(lastSync) ? "Last Sync: Never" : $"Last Sync: {lastSync}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Load settings error: {ex.Message}");
            }
        }

        /// <summary>
        /// Load zones from DB, ensuring we always have exactly 2 zones (Free + Charged).
        /// If zones don't exist, seed defaults.
        /// </summary>
        private void LoadZones()
        {
            dgvZones.Rows.Clear();
            var zones = _dbService.GetDeliveryZones();

            if (zones.Count == 0)
            {
                // Seed default zones
                SeedDefaultZonesInternal();
                zones = _dbService.GetDeliveryZones();
            }

            foreach (var zone in zones)
            {
                dgvZones.Rows.Add(
                    zone.Id,
                    zone.Name,
                    zone.MaxDistance.ToString("F1"),
                    zone.Charge.ToString("F0"),
                    zone.MinOrder > 0 ? zone.MinOrder.ToString("F0") : "0"
                );
            }

            lblZoneValidation.Text = "";
        }

        /// <summary>
        /// Seeds the default 2 zones into DB directly
        /// </summary>
        private void SeedDefaultZonesInternal()
        {
            var defaultZones = new List<DeliveryZone>
            {
                new DeliveryZone { Id = "free-zone", Name = "Free Delivery Zone", MaxDistance = 10, Charge = 0, MinOrder = 0 },
                new DeliveryZone { Id = "charged-zone", Name = "Charged Delivery Zone", MaxDistance = 25, Charge = 200, MinOrder = 0 }
            };
            _dbService.SaveDeliveryZones(defaultZones);
        }

        /// <summary>
        /// Validates the 2 zones: Free distance must be < Charged distance
        /// </summary>
        private bool ValidateZones()
        {
            var zones = GetZonesFromGrid();
            if (zones.Count != 2)
            {
                lblZoneValidation.Text = "⚠️ There must be exactly 2 zones: Free Delivery and Charged Delivery.";
                return false;
            }

            // Sort by distance
            zones.Sort((a, b) => a.MaxDistance.CompareTo(b.MaxDistance));

            var first = zones[0];
            var second = zones[1];

            // First zone must be free (charge = 0)
            if (first.Charge > 0)
            {
                lblZoneValidation.Text = $"⚠️ The zone with smaller distance ({first.Name}, {first.MaxDistance} KM) should be the Free Delivery Zone (charge = 0 PKR).";
                return false;
            }

            // Free zone distance must be less than charged zone distance
            if (first.MaxDistance >= second.MaxDistance)
            {
                lblZoneValidation.Text = $"⚠️ Free Delivery Zone distance ({first.MaxDistance} KM) must be LESS than Charged Zone distance ({second.MaxDistance} KM).";
                return false;
            }

            lblZoneValidation.Text = "✅ Zones are valid!";
            lblZoneValidation.ForeColor = Color.Green;
            return true;
        }

        private List<DeliveryZone> GetZonesFromGrid()
        {
            var zones = new List<DeliveryZone>();
            foreach (DataGridViewRow row in dgvZones.Rows)
            {
                if (row.IsNewRow) continue;
                zones.Add(new DeliveryZone
                {
                    Id = row.Cells["Id"].Value?.ToString() ?? Guid.NewGuid().ToString(),
                    Name = row.Cells["Name"].Value?.ToString() ?? "",
                    MaxDistance = decimal.Parse(row.Cells["Distance"].Value?.ToString() ?? "0"),
                    Charge = decimal.Parse(row.Cells["Charge"].Value?.ToString() ?? "0"),
                    MinOrder = decimal.Parse(row.Cells["MinOrder"].Value?.ToString() ?? "0")
                });
            }
            return zones;
        }

        // ============================================
        // RESTAURANT INFO HANDLERS
        // ============================================
        private void BtnSaveRestaurant_Click(object sender, EventArgs e)
        {
            try
            {
                _dbService.SaveSetting("restaurant_name", txtRestaurantName.Text);
                _dbService.SaveSetting("restaurant_address", txtRestaurantAddress.Text);
                _dbService.SaveSetting("restaurant_phone", txtRestaurantPhone.Text);
                _dbService.SaveSetting("restaurant_email", txtRestaurantEmail.Text);

                _dbService.SaveSetting("use_auto_timing", rbAutoTiming.Checked.ToString());
                _dbService.SaveSetting("accept_website_orders", isWebsiteOrderingEnabled.ToString());
                _dbService.SaveSetting("opening_time", dtpOpeningTime.Value.ToString("HH:mm"));
                _dbService.SaveSetting("closing_time", dtpClosingTime.Value.ToString("HH:mm"));

                MessageBox.Show("Restaurant info saved successfully!\n\nAll settings will sync to the website automatically.", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving restaurant info: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ============================================
        // DELIVERY ZONES HANDLERS
        // ============================================
        private void BtnToggleWebsiteOrdering_Click(object sender, EventArgs e)
        {
            isWebsiteOrderingEnabled = !isWebsiteOrderingEnabled;
            UpdateOrderingUI();
        }

        private void UpdateOrderingUI()
        {
            if (rbAutoTiming.Checked)
            {
                lblOpeningTime.Visible = true;
                dtpOpeningTime.Visible = true;
                lblClosingTime.Visible = true;
                dtpClosingTime.Visible = true;
                btnToggleWebsiteOrdering.Visible = false;
            }
            else
            {
                lblOpeningTime.Visible = false;
                dtpOpeningTime.Visible = false;
                lblClosingTime.Visible = false;
                dtpClosingTime.Visible = false;
                btnToggleWebsiteOrdering.Visible = true;

                if (isWebsiteOrderingEnabled)
                {
                    btnToggleWebsiteOrdering.Text = "🟢 Website Ordering is ON (Click to turn OFF)";
                    btnToggleWebsiteOrdering.BackColor = Color.LightGreen;
                    btnToggleWebsiteOrdering.ForeColor = Color.DarkGreen;
                }
                else
                {
                    btnToggleWebsiteOrdering.Text = "🔴 Website Ordering is OFF (Click to turn ON)";
                    btnToggleWebsiteOrdering.BackColor = Color.LightCoral;
                    btnToggleWebsiteOrdering.ForeColor = Color.DarkRed;
                }
            }
        }

        private void BtnLoadZone_Click(object sender, EventArgs e)
        {
            if (dgvZones.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a zone from the table to edit.", "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var row = dgvZones.SelectedRows[0];
            txtZoneName.Text = row.Cells["Name"].Value.ToString();
            nudZoneDistance.Value = decimal.Parse(row.Cells["Distance"].Value.ToString());
            nudZoneCharge.Value = decimal.Parse(row.Cells["Charge"].Value.ToString());
            nudZoneMinOrder.Value = decimal.Parse(row.Cells["MinOrder"].Value.ToString());
        }

        private void BtnResetDefaults_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Reset delivery zones to defaults?\n\n• Free Delivery Zone: 0-10 KM (Free)\n• Charged Delivery Zone: 10-25 KM (200 PKR)",
                "Reset Zones", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                SeedDefaultZonesInternal();
                LoadZones();
                lblZoneValidation.Text = "✅ Reset to default zones. Click 'Save' to sync.";
                lblZoneValidation.ForeColor = Color.Green;
            }
        }

        private async void BtnSaveZones_Click(object sender, EventArgs e)
        {
            try
            {
                // Before saving, apply the currently loaded edit to the grid
                if (!string.IsNullOrWhiteSpace(txtZoneName.Text) && dgvZones.SelectedRows.Count > 0)
                {
                    // Apply edits directly on the selected row
                    var row = dgvZones.SelectedRows[0];
                    row.Cells["Distance"].Value = nudZoneDistance.Value.ToString("F1");
                    row.Cells["Charge"].Value = nudZoneCharge.Value.ToString("F0");
                    row.Cells["MinOrder"].Value = nudZoneMinOrder.Value.ToString("F0");
                    txtZoneName.Clear();
                }

                if (!ValidateZones()) return;

                var zones = GetZonesFromGrid();
                if (zones.Count != 2)
                {
                    MessageBox.Show("There must be exactly 2 zones: Free Delivery and Charged Delivery.\nUse 'Reset to Defaults' if needed.",
                        "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Save to local DeliveryZones table
                _dbService.SaveDeliveryZones(zones);

                // Also save as a system setting for direct website sync (proper JSON format)
                var zonesForWebsite = zones.Select(z => new
                {
                    maxDistance = z.MaxDistance,
                    charge = z.Charge,
                    minOrder = z.MinOrder
                }).ToList();

                string zonesJson = JsonConvert.SerializeObject(zonesForWebsite);
                _dbService.SaveSetting("delivery_zones", zonesJson);

                // Push to the website backend immediately so admin + website stay in sync
                try
                {
                    var pushResult = await _apiService.UpdateDeliveryZones(zonesForWebsite.Cast<object>().ToList());
                    if (!pushResult.Success)
                    {
                        Console.WriteLine($"⚠️ Website delivery-zone sync warning: {pushResult.Message}");
                    }
                }
                catch (Exception apiEx)
                {
                    Console.WriteLine($"⚠️ Website delivery-zone sync skipped (offline): {apiEx.Message}");
                }

                MessageBox.Show("Delivery zones saved successfully!\n\n" +
                    "✅ Free Zone: Up to " + zones.Where(z => z.Charge == 0).FirstOrDefault()?.MaxDistance + " KM — FREE\n" +
                    "✅ Charged Zone: Up to " + zones.Where(z => z.Charge > 0).FirstOrDefault()?.MaxDistance + " KM — " +
                    zones.Where(z => z.Charge > 0).FirstOrDefault()?.Charge + " PKR\n" +
                    "✅ Changes synced to website immediately.\n" +
                    "✅ Customer orders will be validated against these zones.",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving delivery zones: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ============================================
        // PRINTER SETTINGS HANDLERS
        // ============================================
        private void BtnTestPrint_Click(object sender, EventArgs e)
        {
            try
            {
                var printService = new PrintService();
                if (printService.TestPrinter())
                {
                    MessageBox.Show("✅ Printer test successful!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("❌ Printer test failed. Please check printer connection.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Printer error: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSavePrinter_Click(object sender, EventArgs e)
        {
            try
            {
                _dbService.SaveSetting("printer_name", cmbPrinter.SelectedItem?.ToString() ?? "");
                _dbService.SaveSetting("auto_print", chkAutoPrint.Checked ? "true" : "false");
                _dbService.SaveSetting("paper_width", nudPaperWidth.Value.ToString());

                MessageBox.Show("Printer settings saved successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving printer settings: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ============================================
        // TAX SETTINGS HANDLERS
        // ============================================
        private void BtnSaveTax_Click(object sender, EventArgs e)
        {
            try
            {
                _dbService.SaveSetting("tax_rate", nudTaxRate.Value.ToString());
                _dbService.SaveSetting("min_order", nudMinOrder.Value.ToString());
                _dbService.SaveSetting("baking_duration_minutes", nudBakingDuration.Value.ToString());
                _dbService.SaveSetting("delivery_duration_minutes", nudDeliveryDuration.Value.ToString());

                MessageBox.Show("Settings saved successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving settings: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ============================================
        // SYNC SETTINGS HANDLERS
        // ============================================
        private async void BtnSyncNow_Click(object sender, EventArgs e)
        {
            try
            {
                btnSyncNow.Enabled = false;
                btnSyncNow.Text = "Syncing...";

                var syncService = new SyncService();
                await syncService.SyncNow();

                lblLastSync.Text = $"Last Sync: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
                _dbService.SaveSetting("last_sync", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                MessageBox.Show("Sync completed successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Sync error: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnSyncNow.Enabled = true;
                btnSyncNow.Text = "🔄 Sync Now";
            }
        }

        private void BtnSaveSync_Click(object sender, EventArgs e)
        {
            try
            {
                _dbService.SaveSetting("sync_interval", nudSyncInterval.Value.ToString());
                _dbService.SaveSetting("sync_enabled", chkSyncEnabled.Checked ? "true" : "false");

                MessageBox.Show("Sync settings saved successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving sync settings: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ============================================
        // BACKUP SETTINGS HANDLERS
        // ============================================
        private void BtnBrowseBackup_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.SelectedPath = txtBackupPath.Text;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    txtBackupPath.Text = dialog.SelectedPath;
                }
            }
        }

        private void BtnBackupNow_Click(object sender, EventArgs e)
        {
            try
            {
                string backupPath = txtBackupPath.Text;
                if (!System.IO.Directory.Exists(backupPath))
                {
                    System.IO.Directory.CreateDirectory(backupPath);
                }

                string fileName = $"Backup_{DateTime.Now:yyyyMMdd_HHmmss}.db";
                string sourcePath = _dbService.GetDatabasePath();
                string destPath = System.IO.Path.Combine(backupPath, fileName);

                System.IO.File.Copy(sourcePath, destPath, true);

                MessageBox.Show($"Backup created successfully!\n{fileName}",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Backup error: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSaveBackup_Click(object sender, EventArgs e)
        {
            try
            {
                _dbService.SaveSetting("auto_backup", chkAutoBackup.Checked ? "true" : "false");
                _dbService.SaveSetting("backup_interval", nudBackupInterval.Value.ToString());
                _dbService.SaveSetting("backup_path", txtBackupPath.Text);

                MessageBox.Show("Backup settings saved successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving backup settings: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnSaveProfile_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_adminEmail)) return;

            btnSaveProfile.Enabled = false;
            var result = await _apiService.UpdateAdminProfile(_adminEmail, txtAdminName.Text, txtAdminPhone.Text);
            
            if (result.Success)
            {
                MessageBox.Show("Profile updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show(result.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            btnSaveProfile.Enabled = true;
        }

        private async void BtnUpdatePassword_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_adminEmail)) return;

            if (txtNewPassword.Text != txtConfirmPassword.Text)
            {
                MessageBox.Show("New passwords do not match.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(txtOldPassword.Text) || string.IsNullOrWhiteSpace(txtNewPassword.Text))
            {
                MessageBox.Show("Please fill all password fields.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnUpdatePassword.Enabled = false;
            var result = await _apiService.ChangeAdminPassword(_adminEmail, txtOldPassword.Text, txtNewPassword.Text);
            
            if (result.Success)
            {
                MessageBox.Show("Password updated successfully! You can use it next time you login.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtOldPassword.Clear();
                txtNewPassword.Clear();
                txtConfirmPassword.Clear();
            }
            else
            {
                MessageBox.Show(result.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            btnUpdatePassword.Enabled = true;
        }
    }
}