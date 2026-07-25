// E:\hungryHub\hungry-fast-food\admin-panel\HungryFastFoodAdmin\Forms\DealManagementForm.cs

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using HungryFastFoodAdmin.Models;
using HungryFastFoodAdmin.Services;

namespace HungryFastFoodAdmin.Forms
{
    public partial class DealManagementForm : BaseForm
    {
        private DatabaseService _dbService;
        private ApiService _apiService;
        private DataGridView dgvDeals;
        private Button btnAddDeal;
        private Button btnEditDeal;
        private Button btnDeleteDeal;
        private Button btnPublish;
        private ComboBox cmbStatusFilter;
        private TextBox txtSearch;

        public DealManagementForm()
        {
            InitializeComponent();
            _dbService = new DatabaseService();
            _apiService = new ApiService();
            SetupUI();
            LoadDeals();
        }

        private void InitializeComponent()
        {
            this.Text = "Deal Management - Hungry Hub";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Main Panel
            Panel mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

            // Top Panel (Filters + Buttons)
            Panel topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(245, 245, 245),
                Padding = new Padding(10)
            };

            // Search
            Label lblSearch = new Label
            {
                Text = "Search:",
                Location = new Point(10, 15),
                Size = new Size(60, 25),
                Font = new Font("Segoe UI", 10)
            };

            txtSearch = new TextBox
            {
                Location = new Point(75, 13),
                Size = new Size(200, 25),
                Font = new Font("Segoe UI", 10),
                PlaceholderText = "Search deals..."
            };
            txtSearch.TextChanged += (s, e) => LoadDeals();

            // Status Filter
            Label lblStatus = new Label
            {
                Text = "Status:",
                Location = new Point(290, 15),
                Size = new Size(60, 25),
                Font = new Font("Segoe UI", 10)
            };

            cmbStatusFilter = new ComboBox
            {
                Location = new Point(355, 13),
                Size = new Size(120, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10)
            };
            cmbStatusFilter.Items.AddRange(new object[] { "All", "Active", "Inactive", "Featured" });
            cmbStatusFilter.SelectedIndex = 0;
            cmbStatusFilter.SelectedIndexChanged += (s, e) => LoadDeals();

            // Buttons
            btnAddDeal = new Button
            {
                Text = "➕ Add Deal",
                Location = new Point(600, 8),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(42, 157, 143),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnAddDeal.FlatAppearance.BorderSize = 0;
            btnAddDeal.Click += BtnAddDeal_Click;

            btnEditDeal = new Button
            {
                Text = "✏️ Edit",
                Location = new Point(730, 8),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(244, 162, 97),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnEditDeal.FlatAppearance.BorderSize = 0;
            btnEditDeal.Click += BtnEditDeal_Click;

            btnDeleteDeal = new Button
            {
                Text = "🗑️ Delete",
                Location = new Point(840, 8),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(230, 57, 70),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnDeleteDeal.FlatAppearance.BorderSize = 0;
            btnDeleteDeal.Click += BtnDeleteDeal_Click;

            btnPublish = new Button
            {
                Text = "🚀 Publish to Website",
                Location = new Point(700, 50),
                Size = new Size(200, 35),
                BackColor = Color.FromArgb(53, 57, 59),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnPublish.FlatAppearance.BorderSize = 0;
            btnPublish.Click += async (s, e) => await PublishDeals();

            topPanel.Controls.Add(lblSearch);
            topPanel.Controls.Add(txtSearch);
            topPanel.Controls.Add(lblStatus);
            topPanel.Controls.Add(cmbStatusFilter);
            topPanel.Controls.Add(btnAddDeal);
            topPanel.Controls.Add(btnEditDeal);
            topPanel.Controls.Add(btnDeleteDeal);
            topPanel.Controls.Add(btnPublish);

            // Deals DataGridView
            dgvDeals = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                Font = new Font("Segoe UI", 10)
            };
            StyleDataGridView(dgvDeals);

            dgvDeals.Columns.Add("Id", "ID");
            dgvDeals.Columns["Id"].Visible = false;
            dgvDeals.Columns.Add("Name", "Deal Name");
            dgvDeals.Columns.Add("Items", "Items");
            dgvDeals.Columns.Add("TotalPrice", "Total Price");
            dgvDeals.Columns.Add("DiscountPrice", "Deal Price");
            dgvDeals.Columns.Add("Savings", "Savings");
            dgvDeals.Columns.Add("Featured", "Featured");
            dgvDeals.Columns.Add("Status", "Status");

            // Style the grid
            dgvDeals.Columns["Name"].FillWeight = 30;
            dgvDeals.Columns["Items"].FillWeight = 25;
            dgvDeals.Columns["TotalPrice"].FillWeight = 15;
            dgvDeals.Columns["DiscountPrice"].FillWeight = 15;
            dgvDeals.Columns["Savings"].FillWeight = 10;
            dgvDeals.Columns["Featured"].FillWeight = 10;
            dgvDeals.Columns["Status"].FillWeight = 10;

            mainPanel.Controls.Add(dgvDeals);
            mainPanel.Controls.Add(topPanel);

            this.Controls.Add(mainPanel);
        }

        private void SetupUI()
        {
            // Any additional UI setup
        }

        private void StyleDataGridView(DataGridView dgv)
        {
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(230, 57, 70); // primary red
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv.ColumnHeadersHeight = 35;

            dgv.DefaultCellStyle.BackColor = Color.White;
            dgv.DefaultCellStyle.ForeColor = Color.FromArgb(53, 57, 59);
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(244, 162, 97); // secondary peach
            dgv.DefaultCellStyle.SelectionForeColor = Color.White;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 243, 238);
            dgv.RowTemplate.Height = 30;
        }

        private void LoadDeals()
        {
            dgvDeals.Rows.Clear();

            var deals = _dbService.GetDeals();
            string searchText = txtSearch.Text.Trim().ToLower();
            string statusFilter = cmbStatusFilter.SelectedItem?.ToString();

            // Apply filters
            if (!string.IsNullOrEmpty(searchText))
            {
                deals = deals.Where(d => d.Name.ToLower().Contains(searchText)).ToList();
            }

            if (statusFilter == "Active")
            {
                deals = deals.Where(d => d.IsActive).ToList();
            }
            else if (statusFilter == "Inactive")
            {
                deals = deals.Where(d => !d.IsActive).ToList();
            }
            else if (statusFilter == "Featured")
            {
                deals = deals.Where(d => d.IsFeatured).ToList();
            }

            foreach (var deal in deals)
            {
                // Calculate savings
                decimal savings = deal.TotalPrice - (deal.DiscountPrice ?? deal.TotalPrice);
                string savingsText = savings > 0 ? $"PKR {savings:F0}" : "-";

                // Get items count
                var items = _dbService.GetDealItems(deal.Id);
                string itemsText = string.Join(", ", items.Select(i => i.ProductName));

                dgvDeals.Rows.Add(
                    deal.Id,
                    deal.Name,
                    itemsText.Length > 40 ? itemsText.Substring(0, 40) + "..." : itemsText,
                    $"PKR {deal.TotalPrice:F0}",
                    deal.DiscountPrice.HasValue ? $"PKR {deal.DiscountPrice.Value:F0}" : "-",
                    savingsText,
                    deal.IsFeatured ? "⭐ Featured" : "",
                    deal.IsActive ? "✅ Active" : "❌ Inactive"
                );

                // Color code featured deals
                if (deal.IsFeatured)
                {
                    int rowIndex = dgvDeals.Rows.Count - 1;
                    dgvDeals.Rows[rowIndex].DefaultCellStyle.BackColor = Color.FromArgb(255, 248, 225);
                    dgvDeals.Rows[rowIndex].DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                }
            }
        }

        private async void BtnAddDeal_Click(object sender, EventArgs e)
        {
            var products = _dbService.GetProducts();
            using (var dialog = new DealDialog(products))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    var deal = dialog.GetDeal();
                    _dbService.CreateDeal(deal);
                    LoadDeals();
                    try
                    {
                        await _apiService.SyncDeals(_dbService.GetDeals()); // Force sync
                        await new SyncService().SyncNow();
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError("Deal immediate sync failed", ex);
                    }
                }
            }
        }

        private async void BtnEditDeal_Click(object sender, EventArgs e)
        {
            if (dgvDeals.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a deal to edit.", "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string id = dgvDeals.SelectedRows[0].Cells["Id"].Value.ToString();
            var deal = _dbService.GetDealById(id);

            if (deal == null) return;

            var products = _dbService.GetProducts();
            using (var dialog = new DealDialog(products, deal))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    var updatedDeal = dialog.GetDeal();
                    updatedDeal.Id = id;
                    _dbService.UpdateDeal(updatedDeal);
                    LoadDeals();
                    try
                    {
                        await new SyncService().SyncNow();
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError("Deal immediate sync failed", ex);
                    }
                }
            }
        }

        private async void BtnDeleteDeal_Click(object sender, EventArgs e)
        {
            if (dgvDeals.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a deal to delete.", "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string name = dgvDeals.SelectedRows[0].Cells["Name"].Value.ToString();

            if (MessageBox.Show($"Delete deal '{name}'?", "Confirm",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                string id = dgvDeals.SelectedRows[0].Cells["Id"].Value.ToString();
                _dbService.DeleteDeal(id);
                LoadDeals();
                try
                {
                    await new SyncService().SyncNow();
                }
                catch (Exception ex)
                {
                    Logger.LogError("Deal immediate sync failed", ex);
                }
            }
        }

        private async System.Threading.Tasks.Task PublishDeals()
        {
            try
            {
                btnPublish.Enabled = false;
                btnPublish.Text = "Publishing...";

                var syncService = new SyncService();
                await syncService.SyncNow();

                MessageBox.Show("✅ Deals published and synchronized with Cloud API successfully!",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Sync error: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnPublish.Enabled = true;
                btnPublish.Text = "🚀 Publish to Website";
            }
        }
    }

    // ============================================
    // DEAL DIALOG
    // ============================================
    public class DealDialog : Form
    {
        private TextBox txtName;
        private TextBox txtSlug;
        private TextBox txtDescription;
        private NumericUpDown nudTotalPrice;
        private NumericUpDown nudDiscountPrice;
        private CheckBox chkIsActive;
        private CheckBox chkIsFeatured;
        private DataGridView dgvItems;
        private Button btnAddItem;
        private Button btnRemoveItem;
        private Button btnSave;
        private Button btnCancel;
        private Label lblCalculatedTotal;

        private TextBox txtImageUrl;
        private Button btnBrowseImage;
        private Button btnClearImage;
        private PictureBox pbPreview;
        private string _editImageUrl = "";

        private List<DealItem> _items = new List<DealItem>();
        private List<Product> _products;
        private string _editId;

        public DealDialog(List<Product> products, Deal deal = null)
        {
            _products = products;
            InitializeComponent();

            if (deal != null)
            {
                _editId = deal.Id;
                txtName.Text = deal.Name;
                txtSlug.Text = deal.Slug;
                txtDescription.Text = deal.Description;
                nudTotalPrice.Value = deal.TotalPrice;
                nudDiscountPrice.Value = deal.DiscountPrice ?? 0;
                chkIsActive.Checked = deal.IsActive;
                chkIsFeatured.Checked = deal.IsFeatured;
                _items = deal.Items ?? new List<DealItem>();
                _editImageUrl = deal.ImageUrl ?? "";
                txtImageUrl.Text = _editImageUrl;
                LoadPreviewImage(_editImageUrl);
                this.Text = "Edit Deal";
            }
            else
            {
                this.Text = "Add Deal";
                chkIsActive.Checked = true;
            }

            LoadItems();
            UpdateTotalDisplay();
        }

        private void InitializeComponent()
        {
            this.Text = "Deal";
            this.Size = new Size(820, 720);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            int y = 20;
            int x = 150;

            // Name
            Label lblName = new Label { Text = "Deal Name:", Location = new Point(20, y), Size = new Size(120, 25) };
            txtName = new TextBox { Location = new Point(x, y), Size = new Size(300, 25) };
            y += 35;

            // Slug
            Label lblSlug = new Label { Text = "Slug:", Location = new Point(20, y), Size = new Size(120, 25) };
            txtSlug = new TextBox { Location = new Point(x, y), Size = new Size(300, 25) };
            y += 35;

            // Description
            Label lblDescription = new Label { Text = "Description:", Location = new Point(20, y), Size = new Size(120, 25) };
            txtDescription = new TextBox { Location = new Point(x, y), Size = new Size(350, 60), Multiline = true };
            y += 70;

            // Total Price
            Label lblTotalPrice = new Label { Text = "Total Price (PKR):", Location = new Point(20, y), Size = new Size(120, 25) };
            nudTotalPrice = new NumericUpDown
            {
                Location = new Point(x, y),
                Size = new Size(120, 25),
                Minimum = 0,
                Maximum = 99999,
                Value = 0,
                DecimalPlaces = 0
            };
            nudTotalPrice.ValueChanged += (s, e) => UpdateTotalDisplay();
            y += 35;

            // Discount Price
            Label lblDiscountPrice = new Label { Text = "Discount Price (PKR):", Location = new Point(20, y), Size = new Size(120, 25) };
            nudDiscountPrice = new NumericUpDown
            {
                Location = new Point(x, y),
                Size = new Size(120, 25),
                Minimum = 0,
                Maximum = 99999,
                Value = 0,
                DecimalPlaces = 0
            };
            nudDiscountPrice.ValueChanged += (s, e) => UpdateTotalDisplay();
            y += 35;

            // Is Active
            chkIsActive = new CheckBox
            {
                Text = "Is Active",
                Location = new Point(x, y),
                Size = new Size(100, 25)
            };

            // Is Featured
            chkIsFeatured = new CheckBox
            {
                Text = "Is Featured (Highlighted)",
                Location = new Point(x + 120, y),
                Size = new Size(180, 25)
            };
            y += 35;

            // Items Label
            Label lblItems = new Label { Text = "Deal Items:", Location = new Point(20, y), Size = new Size(120, 25), Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            y += 25;

            // Items DataGridView
            dgvItems = new DataGridView
            {
                Location = new Point(20, y),
                Size = new Size(480, 220),
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                BackgroundColor = Color.White
            };
            dgvItems.Columns.Add("Id", "Id");
            dgvItems.Columns.Add("Product", "Product");
            dgvItems.Columns.Add("Variation", "Option");
            dgvItems.Columns.Add("Quantity", "Qty");
            dgvItems.Columns.Add("UnitPrice", "Price");
            dgvItems.Columns.Add("Total", "Total");
            dgvItems.Columns["Id"].Visible = false;
            dgvItems.Columns["Product"].FillWeight = 150;

            // Image Preview section
            Label lblImageTitle = new Label { Text = "Deal Image:", Location = new Point(530, 20), AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            pbPreview = new PictureBox
            {
                Location = new Point(530, 50),
                Size = new Size(230, 230),
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(240, 238, 233)
            };
            txtImageUrl = new TextBox
            {
                Location = new Point(530, 295),
                Width = 230,
                ReadOnly = true,
                Font = new Font("Segoe UI", 8F)
            };
            btnBrowseImage = new Button { Text = "📂 Browse...", Location = new Point(530, 330), Width = 110, Height = 35 };
            btnBrowseImage.Click += BtnBrowseImage_Click;
            btnClearImage = new Button { Text = "🧹 Clear", Location = new Point(650, 330), Width = 110, Height = 35 };
            btnClearImage.Click += BtnClearImage_Click;

            y += 230;

            // Calculated total label
            lblCalculatedTotal = new Label
            {
                Location = new Point(20, y),
                Size = new Size(480, 25),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Text = "Items Total: PKR 0 | Savings: PKR 0"
            };
            y += 30;

            // Item Buttons
            btnAddItem = new Button
            {
                Text = "Add Item",
                Location = new Point(20, y),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(42, 157, 143),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnAddItem.FlatAppearance.BorderSize = 0;
            btnAddItem.Click += BtnAddItem_Click;

            btnRemoveItem = new Button
            {
                Text = "Remove Item",
                Location = new Point(150, y),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(230, 57, 70),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnRemoveItem.FlatAppearance.BorderSize = 0;
            btnRemoveItem.Click += BtnRemoveItem_Click;

            y += 60;

            // Save/Cancel Buttons
            btnSave = new Button
            {
                Text = "Save Deal",
                Location = new Point(530, y),
                Size = new Size(110, 40),
                BackColor = Color.FromArgb(42, 157, 143),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold)
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += (s, e) => ValidateAndClose();

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(650, y),
                Size = new Size(110, 40),
                BackColor = Color.FromArgb(200, 200, 200),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.FromArgb(53, 57, 59),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                DialogResult = DialogResult.Cancel
            };
            btnCancel.FlatAppearance.BorderSize = 0;

            // Add all controls
            this.Controls.Add(lblName);
            this.Controls.Add(txtName);
            this.Controls.Add(lblSlug);
            this.Controls.Add(txtSlug);
            this.Controls.Add(lblDescription);
            this.Controls.Add(txtDescription);
            this.Controls.Add(lblTotalPrice);
            this.Controls.Add(nudTotalPrice);
            this.Controls.Add(lblDiscountPrice);
            this.Controls.Add(nudDiscountPrice);
            this.Controls.Add(chkIsActive);
            this.Controls.Add(chkIsFeatured);
            this.Controls.Add(lblItems);
            this.Controls.Add(dgvItems);
            this.Controls.Add(btnAddItem);
            this.Controls.Add(btnRemoveItem);
            this.Controls.Add(lblCalculatedTotal);
            this.Controls.Add(btnSave);
            this.Controls.Add(btnCancel);
            this.Controls.Add(lblImageTitle);
            this.Controls.Add(pbPreview);
            this.Controls.Add(txtImageUrl);
            this.Controls.Add(btnBrowseImage);
            this.Controls.Add(btnClearImage);

            // Auto-generate slug
            txtName.TextChanged += (s, e) =>
            {
                if (string.IsNullOrEmpty(txtSlug.Text) || txtSlug.Text == GenerateSlug(txtName.Text))
                {
                    txtSlug.Text = GenerateSlug(txtName.Text);
                }
            };
        }

        private void LoadPreviewImage(string pathOrUrl)
        {
            if (string.IsNullOrEmpty(pathOrUrl))
            {
                pbPreview.Image = null;
                return;
            }

            try
            {
                if (pathOrUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || 
                    pathOrUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    pbPreview.LoadAsync(pathOrUrl);
                }
                else if (File.Exists(pathOrUrl))
                {
                    pbPreview.Image = Image.FromFile(pathOrUrl);
                }
            }
            catch
            {
                pbPreview.Image = null;
            }
        }

        private void BtnBrowseImage_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.AutoUpgradeEnabled = false;
                ofd.Filter = "Image Files|*.png;*.jpg;*.jpeg";
                if (ofd.ShowDialog(this) == DialogResult.OK)
                {
                    var fileInfo = new FileInfo(ofd.FileName);
                    if (fileInfo.Length > 5 * 1024 * 1024)
                    {
                        MessageBox.Show("Image file size must be less than 5MB.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    string ext = Path.GetExtension(ofd.FileName).ToLower();
                    if (ext != ".jpg" && ext != ".jpeg" && ext != ".png")
                    {
                        MessageBox.Show("Invalid image file format. Only JPG and PNG are supported.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    txtImageUrl.Text = ofd.FileName;
                    _editImageUrl = ofd.FileName;
                    LoadPreviewImage(_editImageUrl);
                }
            }
        }

        private void BtnClearImage_Click(object sender, EventArgs e)
        {
            txtImageUrl.Text = "";
            _editImageUrl = "";
            pbPreview.Image = null;
        }

        private string GenerateSlug(string text)
        {
            return text.ToLower()
                .Replace(" ", "-")
                .Replace("&", "and")
                .Replace("'", "")
                .Replace("\"", "");
        }

        private void UpdateTotalDisplay()
        {
            decimal total = 0;
            foreach (DataGridViewRow row in dgvItems.Rows)
            {
                if (row.Cells["Total"].Value != null)
                {
                    string valStr = row.Cells["Total"].Value.ToString().Replace("PKR", "").Trim();
                    if (decimal.TryParse(valStr, out decimal parsedVal))
                    {
                        total += parsedVal;
                    }
                }
            }

            // If no items, use the entered total price
            if (dgvItems.Rows.Count == 0)
            {
                total = nudTotalPrice.Value;
            }

            decimal dealPrice = nudDiscountPrice.Value > 0 ? nudDiscountPrice.Value : total;
            decimal savings = total - dealPrice;

            // Update calculated total label
            lblCalculatedTotal.Text = $"Items Total: PKR {total:F0} | Deal Price: PKR {dealPrice:F0} | Savings: PKR {savings:F0}";

            // Color code savings
            if (savings > 0)
            {
                lblCalculatedTotal.ForeColor = Color.FromArgb(42, 157, 143);
            }
            else
            {
                lblCalculatedTotal.ForeColor = Color.FromArgb(230, 57, 70);
            }
        }

        private void BtnAddItem_Click(object sender, EventArgs e)
        {
            using (var dialog = new DealItemDialog(_products))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    var item = dialog.GetDealItem();
                    _items.Add(item);
                    LoadItems();
                    UpdateTotalDisplay();
                }
            }
        }

        private void BtnRemoveItem_Click(object sender, EventArgs e)
        {
            if (dgvItems.SelectedRows.Count > 0)
            {
                string id = dgvItems.SelectedRows[0].Cells["Id"].Value.ToString();
                _items.RemoveAll(i => i.Id == id);
                LoadItems();
                UpdateTotalDisplay();
            }
        }

        private void LoadItems()
        {
            dgvItems.Rows.Clear();

            foreach (var item in _items)
            {
                var product = _products.FirstOrDefault(p => p.Id == item.ProductId);
                string productName = product?.Name ?? "Unknown";
                string variationName = item.VariationName ?? "-";
                decimal total = item.UnitPrice * item.Quantity;

                dgvItems.Rows.Add(
                    item.Id,
                    productName,
                    variationName,
                    item.Quantity,
                    $"PKR {item.UnitPrice:F0}",
                    $"PKR {total:F0}"
                );
            }
        }

        private void ValidateAndClose()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Deal name is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_items.Count == 0)
            {
                MessageBox.Show("Please add at least one item to the deal.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        public Deal GetDeal()
        {
            return new Deal
            {
                Id = _editId ?? Guid.NewGuid().ToString(),
                Name = txtName.Text,
                Slug = txtSlug.Text,
                Description = txtDescription.Text,
                TotalPrice = nudTotalPrice.Value,
                DiscountPrice = nudDiscountPrice.Value > 0 ? nudDiscountPrice.Value : (decimal?)null,
                IsActive = chkIsActive.Checked,
                IsFeatured = chkIsFeatured.Checked,
                ImageUrl = _editImageUrl,
                Items = _items
            };
        }
    }

    // ============================================
    // DEAL ITEM DIALOG
    // ============================================
    public class DealItemDialog : Form
    {
        private ComboBox cmbProduct;
        private ComboBox cmbVariation;
        private NumericUpDown nudQuantity;
        private NumericUpDown nudUnitPrice;
        private Button btnSave;
        private Button btnCancel;
        private List<Product> _products;
        private List<ProductVariation> _variations = new List<ProductVariation>();

        public DealItemDialog(List<Product> products)
        {
            _products = products;
            InitializeComponent();

            // Load products
            cmbProduct.Items.Clear();
            foreach (var product in products.Where(p => p.IsActive))
            {
                cmbProduct.Items.Add(product.Name);
            }

            if (cmbProduct.Items.Count > 0) cmbProduct.SelectedIndex = 0;
        }

        private void InitializeComponent()
        {
            this.Text = "Add Deal Item";
            this.Size = new Size(450, 280);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            int y = 20;

            Label lblProduct = new Label { Text = "Product:", Location = new Point(20, y), Size = new Size(100, 25) };
            cmbProduct = new ComboBox
            {
                Location = new Point(130, y),
                Size = new Size(250, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbProduct.SelectedIndexChanged += CmbProduct_SelectedIndexChanged;
            y += 35;

            Label lblVariation = new Label { Text = "Variation:", Location = new Point(20, y), Size = new Size(100, 25) };
            cmbVariation = new ComboBox
            {
                Location = new Point(130, y),
                Size = new Size(250, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Enabled = false
            };
            cmbVariation.Items.Add("None");
            cmbVariation.SelectedIndex = 0;
            y += 35;

            Label lblQuantity = new Label { Text = "Quantity:", Location = new Point(20, y), Size = new Size(100, 25) };
            nudQuantity = new NumericUpDown
            {
                Location = new Point(130, y),
                Size = new Size(80, 25),
                Minimum = 1,
                Maximum = 99,
                Value = 1
            };
            y += 35;

            Label lblUnitPrice = new Label { Text = "Unit Price (PKR):", Location = new Point(20, y), Size = new Size(100, 25) };
            nudUnitPrice = new NumericUpDown
            {
                Location = new Point(130, y),
                Size = new Size(120, 25),
                Minimum = 0,
                Maximum = 99999,
                Value = 0,
                DecimalPlaces = 0
            };
            y += 45;

            btnSave = new Button
            {
                Text = "Add Item",
                Location = new Point(120, y),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(42, 157, 143),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                DialogResult = DialogResult.OK
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += (s, e) =>
            {
                if (cmbProduct.SelectedItem == null)
                {
                    MessageBox.Show("Please select a product.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (nudUnitPrice.Value == 0)
                {
                    MessageBox.Show("Please enter a unit price.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                this.DialogResult = DialogResult.OK;
                this.Close();
            };

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(230, y),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(200, 200, 200),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.FromArgb(53, 57, 59),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                DialogResult = DialogResult.Cancel
            };
            btnCancel.FlatAppearance.BorderSize = 0;

            this.Controls.Add(lblProduct);
            this.Controls.Add(cmbProduct);
            this.Controls.Add(lblVariation);
            this.Controls.Add(cmbVariation);
            this.Controls.Add(lblQuantity);
            this.Controls.Add(nudQuantity);
            this.Controls.Add(lblUnitPrice);
            this.Controls.Add(nudUnitPrice);
            this.Controls.Add(btnSave);
            this.Controls.Add(btnCancel);
        }

        private void CmbProduct_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbProduct.SelectedItem == null) return;

            string productName = cmbProduct.SelectedItem.ToString();
            var product = _products.FirstOrDefault(p => p.Name == productName);

            if (product != null)
            {
                _variations = product.Variations ?? new List<ProductVariation>();

                cmbVariation.Items.Clear();
                cmbVariation.Items.Add("None");

                foreach (var v in _variations)
                {
                    cmbVariation.Items.Add(v.VariationName);
                }

                cmbVariation.Enabled = _variations.Count > 0;
                cmbVariation.SelectedIndex = 0;

                // Set default unit price
                nudUnitPrice.Value = product.DiscountPrice ?? product.BasePrice;
            }
        }

        public DealItem GetDealItem()
        {
            string productName = cmbProduct.SelectedItem?.ToString();
            var product = _products.FirstOrDefault(p => p.Name == productName);

            string variationName = null;
            string variationId = null;
            if (cmbVariation.Enabled && cmbVariation.SelectedIndex > 0)
            {
                variationName = cmbVariation.SelectedItem.ToString();
                var variation = _variations.FirstOrDefault(v => v.VariationName == variationName);
                variationId = variation?.Id;
            }

            return new DealItem
            {
                Id = Guid.NewGuid().ToString(),
                ProductId = product?.Id,
                ProductName = product?.Name,
                VariationId = variationId,
                VariationName = variationName,
                Quantity = (int)nudQuantity.Value,
                UnitPrice = nudUnitPrice.Value
            };
        }
    }
}