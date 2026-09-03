using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using HungryFastFoodAdmin.Models;
using HungryFastFoodAdmin.Services;

namespace HungryFastFoodAdmin.Forms
{
    public class InventoryForm : BaseForm
    {
        private readonly DatabaseService _dbService;

        // KPI Stat Cards
        private Label lblTotalValue;
        private Label lblTotalItems;
        private Label lblLowStockCount;
        private Label lblTodayDeductions;

        // Tab Control
        private TabControl tabControl;
        private TabPage tabRawMaterials;
        private TabPage tabRecipes;
        private TabPage tabAuditLogs;

        // Raw Materials Tab Controls
        private DataGridView dgvRawMaterials;
        private TextBox txtSearchStock;
        private ComboBox cmbCategoryFilter;
        private Button btnAddMaterial;
        private Button btnBatchRestock;
        private Button btnRefreshStock;

        // Recipes Tab Controls
        private DataGridView dgvRecipes;
        private Button btnAddRecipe;
        private Button btnDeleteRecipe;

        // Audit Logs Controls
        private DataGridView dgvAuditLogs;
        private Button btnRefreshLogs;

        public InventoryForm()
        {
            _dbService = new DatabaseService();
            InitializeComponent();
            LoadAllInventoryData();
        }

        private void InitializeComponent()
        {
            this.Text = "Inventory & Recipe Management";
            this.Size = new Size(1150, 720);
            this.BackColor = Color.FromArgb(245, 246, 248);

            // ==========================================
            // 1. Header Panel
            // ==========================================
            Panel pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.White,
                Padding = new Padding(20, 12, 20, 12)
            };

            Label lblTitle = new Label
            {
                Text = "📦 Inventory & Raw Material Manager",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                AutoSize = true,
                Location = new Point(20, 12)
            };

            Label lblSubTitle = new Label
            {
                Text = "Domino's & KFC Style Recipe Mapping & Automated Stock Deduction System",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
                ForeColor = Color.FromArgb(100, 116, 139),
                AutoSize = true,
                Location = new Point(22, 42)
            };

            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblSubTitle);

            // ==========================================
            // 2. Top Summary KPI Cards Panel
            // ==========================================
            Panel pnlKPIs = new Panel
            {
                Dock = DockStyle.Top,
                Height = 95,
                Padding = new Padding(20, 10, 20, 10),
                BackColor = Color.FromArgb(245, 246, 248)
            };

            TableLayoutPanel tblKPI = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1,
                BackColor = Color.Transparent
            };
            tblKPI.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tblKPI.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tblKPI.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tblKPI.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));

            var cardVal = CreateKPICard("💰 Stock Valuation", "PKR 0", Color.FromArgb(37, 99, 235), out lblTotalValue);
            var cardCount = CreateKPICard("📦 Total Ingredients", "0 Items", Color.FromArgb(16, 185, 129), out lblTotalItems);
            var cardLow = CreateKPICard("⚠️ Low Stock Alerts", "0 Alerts", Color.FromArgb(239, 68, 68), out lblLowStockCount);
            var cardLogs = CreateKPICard("📉 Today's Deductions", "0 Entries", Color.FromArgb(139, 92, 246), out lblTodayDeductions);

            tblKPI.Controls.Add(cardVal, 0, 0);
            tblKPI.Controls.Add(cardCount, 1, 0);
            tblKPI.Controls.Add(cardLow, 2, 0);
            tblKPI.Controls.Add(cardLogs, 3, 0);

            pnlKPIs.Controls.Add(tblKPI);

            // ==========================================
            // 3. Tabbed Interface Setup
            // ==========================================
            tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Padding = new Point(16, 8)
            };

            tabRawMaterials = new TabPage("🥦 Raw Materials Stock");
            tabRecipes = new TabPage("🍔 Product Recipes (BOM)");
            tabAuditLogs = new TabPage("📜 Stock Consumption Log");

            SetupRawMaterialsTab();
            SetupRecipesTab();
            SetupAuditLogsTab();

            tabControl.TabPages.Add(tabRawMaterials);
            tabControl.TabPages.Add(tabRecipes);
            tabControl.TabPages.Add(tabAuditLogs);

            this.Controls.Add(tabControl);
            this.Controls.Add(pnlKPIs);
            this.Controls.Add(pnlHeader);
        }

        private Panel CreateKPICard(string title, string initialVal, Color accentColor, out Label valLabel)
        {
            Panel card = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(6),
                BackColor = Color.White,
                Padding = new Padding(12)
            };

            Label lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 116, 139),
                Dock = DockStyle.Top,
                Height = 20
            };

            valLabel = new Label
            {
                Text = initialVal,
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = accentColor,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            Panel accentBar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 4,
                BackColor = accentColor
            };

            card.Controls.Add(valLabel);
            card.Controls.Add(lblTitle);
            card.Controls.Add(accentBar);

            return card;
        }

        // =========================================================================
        // TAB 1: RAW MATERIALS STOCK CONTROL
        // =========================================================================
        private void SetupRawMaterialsTab()
        {
            Panel topBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 55,
                BackColor = Color.White,
                Padding = new Padding(10)
            };

            Label lblSearch = new Label { Text = "🔍 Search:", Location = new Point(12, 16), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            txtSearchStock = new TextBox { Location = new Point(78, 14), Size = new Size(180, 26), Font = new Font("Segoe UI", 9.5F) };
            txtSearchStock.TextChanged += (s, e) => FilterRawMaterials();

            Label lblCat = new Label { Text = "Category:", Location = new Point(275, 16), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            cmbCategoryFilter = new ComboBox { Location = new Point(345, 14), Size = new Size(150, 26), DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9.5F) };
            cmbCategoryFilter.Items.AddRange(new object[] { "All Categories", "Bakery", "Meat & Poultry", "Dairy", "Sauces & Spices", "Vegetables", "Pantry", "Packaging" });
            cmbCategoryFilter.SelectedIndex = 0;
            cmbCategoryFilter.SelectedIndexChanged += (s, e) => FilterRawMaterials();

            btnAddMaterial = new Button
            {
                Text = "+ Add Raw Material",
                Location = new Point(515, 12),
                Size = new Size(150, 30),
                BackColor = Color.FromArgb(16, 185, 129),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnAddMaterial.FlatAppearance.BorderSize = 0;
            btnAddMaterial.Click += BtnAddMaterial_Click;

            btnBatchRestock = new Button
            {
                Text = "📦 Restock Entry",
                Location = new Point(675, 12),
                Size = new Size(130, 30),
                BackColor = Color.FromArgb(37, 99, 235),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnBatchRestock.FlatAppearance.BorderSize = 0;
            btnBatchRestock.Click += BtnBatchRestock_Click;

            btnRefreshStock = new Button
            {
                Text = "🔄 Refresh",
                Location = new Point(815, 12),
                Size = new Size(90, 30),
                BackColor = Color.FromArgb(100, 116, 139),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnRefreshStock.FlatAppearance.BorderSize = 0;
            btnRefreshStock.Click += (s, e) => LoadAllInventoryData();

            topBar.Controls.Add(lblSearch);
            topBar.Controls.Add(txtSearchStock);
            topBar.Controls.Add(lblCat);
            topBar.Controls.Add(cmbCategoryFilter);
            topBar.Controls.Add(btnAddMaterial);
            topBar.Controls.Add(btnBatchRestock);
            topBar.Controls.Add(btnRefreshStock);

            // DataGrid Setup
            dgvRawMaterials = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                RowTemplate = { Height = 36 },
                Font = new Font("Segoe UI", 9F)
            };

            dgvRawMaterials.Columns.Add("Id", "ID");
            dgvRawMaterials.Columns["Id"].Visible = false;
            dgvRawMaterials.Columns.Add("Name", "Ingredient Name");
            dgvRawMaterials.Columns.Add("Category", "Category");
            dgvRawMaterials.Columns.Add("CurrentStock", "Current Stock");
            dgvRawMaterials.Columns.Add("Unit", "Unit");
            dgvRawMaterials.Columns.Add("MinThreshold", "Alert Level");
            dgvRawMaterials.Columns.Add("StatusBadge", "Stock Status");
            dgvRawMaterials.Columns.Add("CostPerUnit", "Unit Cost (PKR)");
            dgvRawMaterials.Columns.Add("TotalValue", "Total Value (PKR)");

            dgvRawMaterials.CellFormatting += DgvRawMaterials_CellFormatting;

            tabRawMaterials.Controls.Add(dgvRawMaterials);
            tabRawMaterials.Controls.Add(topBar);
        }

        private void DgvRawMaterials_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex == dgvRawMaterials.Columns["StatusBadge"].Index && e.Value != null)
            {
                string status = e.Value.ToString();
                if (status == "OUT OF STOCK")
                {
                    e.CellStyle.BackColor = Color.FromArgb(254, 226, 226);
                    e.CellStyle.ForeColor = Color.FromArgb(185, 28, 28);
                    e.CellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                }
                else if (status == "LOW STOCK")
                {
                    e.CellStyle.BackColor = Color.FromArgb(254, 243, 199);
                    e.CellStyle.ForeColor = Color.FromArgb(180, 83, 9);
                    e.CellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                }
                else
                {
                    e.CellStyle.BackColor = Color.FromArgb(209, 250, 229);
                    e.CellStyle.ForeColor = Color.FromArgb(4, 120, 87);
                    e.CellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                }
            }
        }

        // =========================================================================
        // TAB 2: PRODUCT RECIPE MANAGER (BOM)
        // =========================================================================
        private void SetupRecipesTab()
        {
            Panel topBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 55,
                BackColor = Color.White,
                Padding = new Padding(10)
            };

            Label lblInfo = new Label
            {
                Text = "🍔 Map Ingredient Deductions per Product Unit (Domino's / KFC Recipe Standard)",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                Location = new Point(12, 16),
                AutoSize = true
            };

            btnAddRecipe = new Button
            {
                Text = "+ Add Ingredient to Recipe",
                Location = new Point(620, 12),
                Size = new Size(190, 30),
                BackColor = Color.FromArgb(16, 185, 129),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnAddRecipe.FlatAppearance.BorderSize = 0;
            btnAddRecipe.Click += BtnAddRecipe_Click;

            btnDeleteRecipe = new Button
            {
                Text = "❌ Delete Selected Rule",
                Location = new Point(820, 12),
                Size = new Size(170, 30),
                BackColor = Color.FromArgb(239, 68, 68),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnDeleteRecipe.FlatAppearance.BorderSize = 0;
            btnDeleteRecipe.Click += BtnDeleteRecipe_Click;

            topBar.Controls.Add(lblInfo);
            topBar.Controls.Add(btnAddRecipe);
            topBar.Controls.Add(btnDeleteRecipe);

            dgvRecipes = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                RowTemplate = { Height = 34 },
                Font = new Font("Segoe UI", 9F)
            };

            dgvRecipes.Columns.Add("Id", "ID");
            dgvRecipes.Columns["Id"].Visible = false;
            dgvRecipes.Columns.Add("ProductName", "Menu Product");
            dgvRecipes.Columns.Add("VariationName", "Variation");
            dgvRecipes.Columns.Add("RawMaterialName", "Required Ingredient");
            dgvRecipes.Columns.Add("RequiredQuantity", "Deduction Quantity Per Order");
            dgvRecipes.Columns.Add("Unit", "Unit");

            tabRecipes.Controls.Add(dgvRecipes);
            tabRecipes.Controls.Add(topBar);
        }

        // =========================================================================
        // TAB 3: AUDIT LOGS
        // =========================================================================
        private void SetupAuditLogsTab()
        {
            Panel topBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 55,
                BackColor = Color.White,
                Padding = new Padding(10)
            };

            Label lblTitle = new Label
            {
                Text = "📜 Real-Time Stock Consumption & Restock Audit Trail",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                Location = new Point(12, 16),
                AutoSize = true
            };

            btnRefreshLogs = new Button
            {
                Text = "🔄 Refresh Activity Log",
                Location = new Point(780, 12),
                Size = new Size(160, 30),
                BackColor = Color.FromArgb(100, 116, 139),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnRefreshLogs.FlatAppearance.BorderSize = 0;
            btnRefreshLogs.Click += (s, e) => LoadAuditLogs();

            topBar.Controls.Add(lblTitle);
            topBar.Controls.Add(btnRefreshLogs);

            dgvAuditLogs = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                RowTemplate = { Height = 32 },
                Font = new Font("Segoe UI", 8.5F)
            };

            dgvAuditLogs.Columns.Add("Timestamp", "Date & Time");
            dgvAuditLogs.Columns.Add("RawMaterialName", "Ingredient");
            dgvAuditLogs.Columns.Add("ChangeAmount", "Quantity Change");
            dgvAuditLogs.Columns.Add("Type", "Activity Type");
            dgvAuditLogs.Columns.Add("ReferenceId", "Ref Order #");
            dgvAuditLogs.Columns.Add("Notes", "Details & Consumption Note");

            tabAuditLogs.Controls.Add(dgvAuditLogs);
            tabAuditLogs.Controls.Add(topBar);
        }

        // =========================================================================
        // DATA LOADING & CALCULATIONS
        // =========================================================================
        private List<RawMaterial> _allMaterials = new List<RawMaterial>();

        private void LoadAllInventoryData()
        {
            _allMaterials = _dbService.GetRawMaterials();
            FilterRawMaterials();
            LoadRecipes();
            LoadAuditLogs();
            UpdateKPISummary();
        }

        private void FilterRawMaterials()
        {
            dgvRawMaterials.Rows.Clear();
            string search = txtSearchStock.Text.ToLower().Trim();
            string cat = cmbCategoryFilter.SelectedItem?.ToString() ?? "All Categories";

            var filtered = _allMaterials.Where(m =>
                (string.IsNullOrEmpty(search) || m.Name.ToLower().Contains(search) || m.Category.ToLower().Contains(search)) &&
                (cat == "All Categories" || m.Category.Equals(cat, StringComparison.OrdinalIgnoreCase))
            ).ToList();

            foreach (var item in filtered)
            {
                decimal totalVal = (decimal)item.CurrentStock * item.CostPerUnit;
                dgvRawMaterials.Rows.Add(
                    item.Id,
                    item.Name,
                    item.Category,
                    $"{item.CurrentStock:N1}",
                    item.Unit,
                    $"{item.MinThreshold:N1}",
                    item.StatusBadge,
                    $"{item.CostPerUnit:F2}",
                    $"{totalVal:N0}"
                );
            }
        }

        private void LoadRecipes()
        {
            dgvRecipes.Rows.Clear();
            var recipes = _dbService.GetProductRecipes();
            foreach (var r in recipes)
            {
                dgvRecipes.Rows.Add(
                    r.Id,
                    r.ProductName,
                    string.IsNullOrEmpty(r.VariationName) ? "Standard" : r.VariationName,
                    r.RawMaterialName,
                    $"{r.RequiredQuantity:N1}",
                    r.Unit
                );
            }
        }

        private void LoadAuditLogs()
        {
            dgvAuditLogs.Rows.Clear();
            var logs = _dbService.GetInventoryLogs(100);
            foreach (var log in logs)
            {
                string changeDisplay = log.ChangeAmount > 0 ? $"+{log.ChangeAmount:N1}" : $"{log.ChangeAmount:N1}";
                dgvAuditLogs.Rows.Add(
                    log.Timestamp.ToString("yyyy-MM-dd hh:mm tt"),
                    log.RawMaterialName,
                    changeDisplay,
                    log.Type.ToUpper(),
                    log.ReferenceId,
                    log.Notes
                );
            }
        }

        private void UpdateKPISummary()
        {
            decimal totalVal = _allMaterials.Sum(m => (decimal)m.CurrentStock * m.CostPerUnit);
            int lowStockCount = _allMaterials.Count(m => m.CurrentStock <= m.MinThreshold);
            var logs = _dbService.GetInventoryLogs(100);
            int todayDeductions = logs.Count(l => l.Timestamp.Date == DateTime.Today && l.ChangeAmount < 0);

            lblTotalValue.Text = $"PKR {totalVal:N0}";
            lblTotalItems.Text = $"{_allMaterials.Count} Items";
            lblLowStockCount.Text = $"{lowStockCount} Alert{(lowStockCount == 1 ? "" : "s")}";
            lblTodayDeductions.Text = $"{todayDeductions} Deductions";
        }

        // =========================================================================
        // USER ACTIONS & DIALOGS
        // =========================================================================
        private void BtnAddMaterial_Click(object sender, EventArgs e)
        {
            using (var dlg = new Form())
            {
                dlg.Text = "Add New Raw Material";
                dlg.Size = new Size(400, 360);
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox = false;

                Label l1 = new Label { Text = "Material Name:", Location = new Point(20, 20), AutoSize = true };
                TextBox txtName = new TextBox { Location = new Point(140, 18), Width = 220 };

                Label l2 = new Label { Text = "Category:", Location = new Point(20, 60), AutoSize = true };
                ComboBox cmbCat = new ComboBox { Location = new Point(140, 58), Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };
                cmbCat.Items.AddRange(new object[] { "Bakery", "Meat & Poultry", "Dairy", "Sauces & Spices", "Vegetables", "Pantry", "Packaging" });
                cmbCat.SelectedIndex = 0;

                Label l3 = new Label { Text = "Initial Stock:", Location = new Point(20, 100), AutoSize = true };
                NumericUpDown nudStock = new NumericUpDown { Location = new Point(140, 98), Width = 220, Maximum = 100000, DecimalPlaces = 1 };

                Label l4 = new Label { Text = "Unit (g, kg, units):", Location = new Point(20, 140), AutoSize = true };
                TextBox txtUnit = new TextBox { Location = new Point(140, 138), Width = 220, Text = "g" };

                Label l5 = new Label { Text = "Min Alert Level:", Location = new Point(20, 180), AutoSize = true };
                NumericUpDown nudMin = new NumericUpDown { Location = new Point(140, 178), Width = 220, Maximum = 50000, DecimalPlaces = 1 };

                Label l6 = new Label { Text = "Unit Cost (PKR):", Location = new Point(20, 220), AutoSize = true };
                NumericUpDown nudCost = new NumericUpDown { Location = new Point(140, 218), Width = 220, Maximum = 10000, DecimalPlaces = 2 };

                Button btnSave = new Button
                {
                    Text = "💾 Save Material",
                    Location = new Point(140, 270),
                    Size = new Size(130, 32),
                    BackColor = Color.FromArgb(16, 185, 129),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };
                btnSave.Click += (s, args) =>
                {
                    if (string.IsNullOrWhiteSpace(txtName.Text))
                    {
                        MessageBox.Show("Please enter a material name.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    _dbService.SaveRawMaterial(new RawMaterial
                    {
                        Name = txtName.Text.Trim(),
                        Category = cmbCat.SelectedItem.ToString(),
                        CurrentStock = (double)nudStock.Value,
                        Unit = txtUnit.Text.Trim(),
                        MinThreshold = (double)nudMin.Value,
                        CostPerUnit = nudCost.Value
                    });

                    dlg.DialogResult = DialogResult.OK;
                };

                dlg.Controls.AddRange(new Control[] { l1, txtName, l2, cmbCat, l3, nudStock, l4, txtUnit, l5, nudMin, l6, nudCost, btnSave });
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    LoadAllInventoryData();
                    MessageBox.Show("Raw material added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void BtnBatchRestock_Click(object sender, EventArgs e)
        {
            if (_allMaterials.Count == 0) return;

            using (var dlg = new Form())
            {
                dlg.Text = "📦 Restock Raw Material Inventory";
                dlg.Size = new Size(420, 320);
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;

                Label l1 = new Label { Text = "Select Ingredient:", Location = new Point(20, 20), AutoSize = true };
                ComboBox cmbMat = new ComboBox { Location = new Point(150, 18), Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };
                foreach (var m in _allMaterials)
                {
                    cmbMat.Items.Add($"{m.Name} ({m.Category})");
                }
                if (cmbMat.Items.Count > 0) cmbMat.SelectedIndex = 0;

                Label l2 = new Label { Text = "Quantity to Add:", Location = new Point(20, 70), AutoSize = true };
                NumericUpDown nudQty = new NumericUpDown { Location = new Point(150, 68), Width = 220, Maximum = 100000, DecimalPlaces = 1, Value = 10 };

                Label l3 = new Label { Text = "Invoice / Ref #:", Location = new Point(20, 120), AutoSize = true };
                TextBox txtRef = new TextBox { Location = new Point(150, 118), Width = 220, Text = "INV-" + DateTime.Now.ToString("MMdd") };

                Label l4 = new Label { Text = "Restock Notes:", Location = new Point(20, 170), AutoSize = true };
                TextBox txtNotes = new TextBox { Location = new Point(150, 168), Width = 220, Text = "Weekly Supplier Shipment Restock" };

                Button btnSave = new Button
                {
                    Text = "📦 Add Stock",
                    Location = new Point(150, 220),
                    Size = new Size(140, 34),
                    BackColor = Color.FromArgb(37, 99, 235),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };
                btnSave.Click += (s, args) =>
                {
                    int selectedIndex = cmbMat.SelectedIndex;
                    if (selectedIndex >= 0 && selectedIndex < _allMaterials.Count)
                    {
                        var mat = _allMaterials[selectedIndex];
                        _dbService.AddStockQuantity(mat.Id, (double)nudQty.Value, txtRef.Text.Trim(), txtNotes.Text.Trim());
                        dlg.DialogResult = DialogResult.OK;
                    }
                };

                dlg.Controls.AddRange(new Control[] { l1, cmbMat, l2, nudQty, l3, txtRef, l4, txtNotes, btnSave });
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    LoadAllInventoryData();
                    MessageBox.Show("Inventory restocked successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void BtnAddRecipe_Click(object sender, EventArgs e)
        {
            if (_allMaterials.Count == 0) return;

            using (var dlg = new Form())
            {
                dlg.Text = "Add Ingredient Rule to Product Recipe";
                dlg.Size = new Size(420, 280);
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;

                Label l1 = new Label { Text = "Menu Product Name:", Location = new Point(20, 20), AutoSize = true };
                TextBox txtProd = new TextBox { Location = new Point(160, 18), Width = 210, Text = "Zinger Burger" };

                Label l2 = new Label { Text = "Required Raw Material:", Location = new Point(20, 70), AutoSize = true };
                ComboBox cmbMat = new ComboBox { Location = new Point(160, 68), Width = 210, DropDownStyle = ComboBoxStyle.DropDownList };
                foreach (var m in _allMaterials)
                {
                    cmbMat.Items.Add($"{m.Name} ({m.Unit})");
                }
                if (cmbMat.Items.Count > 0) cmbMat.SelectedIndex = 0;

                Label l3 = new Label { Text = "Required Quantity:", Location = new Point(20, 120), AutoSize = true };
                NumericUpDown nudQty = new NumericUpDown { Location = new Point(160, 118), Width = 210, Maximum = 10000, DecimalPlaces = 1, Value = 1 };

                Button btnSave = new Button
                {
                    Text = "💾 Save Recipe Rule",
                    Location = new Point(160, 170),
                    Size = new Size(150, 32),
                    BackColor = Color.FromArgb(16, 185, 129),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };
                btnSave.Click += (s, args) =>
                {
                    int selIndex = cmbMat.SelectedIndex;
                    if (selIndex >= 0 && selIndex < _allMaterials.Count)
                    {
                        var mat = _allMaterials[selIndex];
                        _dbService.SaveProductRecipe(new ProductRecipe
                        {
                            ProductName = txtProd.Text.Trim(),
                            VariationName = "",
                            RawMaterialId = mat.Id,
                            RequiredQuantity = (double)nudQty.Value
                        });
                        dlg.DialogResult = DialogResult.OK;
                    }
                };

                dlg.Controls.AddRange(new Control[] { l1, txtProd, l2, cmbMat, l3, nudQty, btnSave });
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    LoadRecipes();
                    MessageBox.Show("Recipe rule added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void BtnDeleteRecipe_Click(object sender, EventArgs e)
        {
            if (dgvRecipes.SelectedRows.Count == 0) return;
            int recipeId = Convert.ToInt32(dgvRecipes.SelectedRows[0].Cells["Id"].Value);

            if (MessageBox.Show("Are you sure you want to delete this recipe deduction rule?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _dbService.DeleteProductRecipe(recipeId);
                LoadRecipes();
            }
        }
    }
}
