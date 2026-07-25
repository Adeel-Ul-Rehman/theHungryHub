using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using HungryFastFoodAdmin.Models;
using HungryFastFoodAdmin.Services;

namespace HungryFastFoodAdmin.Forms
{
    public partial class MenuManagementForm : BaseForm
    {
        private DatabaseService _dbService;
        private ApiService _apiService;

        private TabControl tabControl;
        private TabPage tabCategories;
        private TabPage tabProducts;

        // Categories controls
        private DataGridView dgvCategories;
        private Button btnAddCategory;
        private Button btnEditCategory;
        private Button btnDeleteCategory;
        private Button btnPublishCategories;

        // Products controls
        private ComboBox cmbProductCategory;
        private TextBox txtProductSearch;
        private DataGridView dgvProducts;
        private Button btnAddProduct;
        private Button btnEditProduct;
        private Button btnDeleteProduct;
        private Button btnPublishProducts;

        // Visual Theme colors (requested: Primary #E63946, Secondary #F4A261)
        private static readonly Color ColorPrimary = Color.FromArgb(230, 57, 70);    // #E63946 (Red)
        private static readonly Color ColorSecondary = Color.FromArgb(244, 162, 97); // #F4A261 (Peach)
        private static readonly Color ColorDark = Color.FromArgb(42, 157, 143);      // #2A9D8F (Teal accent)
        private static readonly Color ColorBackground = Color.FromArgb(250, 249, 246);

        public MenuManagementForm()
        {
            _dbService = new DatabaseService();
            _apiService = new ApiService();
            InitializeComponent();
            SetupUI();
            LoadCategories();
            LoadProducts();
        }

        private void InitializeComponent()
        {
            this.tabControl = new TabControl();
            this.tabCategories = new TabPage();
            this.tabProducts = new TabPage();
            
            this.tabControl.SuspendLayout();
            this.SuspendLayout();

            // tabControl
            this.tabControl.Controls.Add(this.tabCategories);
            this.tabControl.Controls.Add(this.tabProducts);
            this.tabControl.Dock = DockStyle.Fill;
            this.tabControl.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            this.tabControl.Location = new Point(0, 0);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new Size(1000, 600);
            this.tabControl.TabIndex = 0;

            // tabCategories
            this.tabCategories.BackColor = ColorBackground;
            this.tabCategories.Location = new Point(4, 26);
            this.tabCategories.Name = "tabCategories";
            this.tabCategories.Padding = new Padding(15);
            this.tabCategories.Size = new Size(992, 570);
            this.tabCategories.Text = "📁 Categories";

            // tabProducts
            this.tabProducts.BackColor = ColorBackground;
            this.tabProducts.Location = new Point(4, 26);
            this.tabProducts.Name = "tabProducts";
            this.tabProducts.Padding = new Padding(15);
            this.tabProducts.Size = new Size(992, 570);
            this.tabProducts.Text = "🍔 Products";

            // MenuManagementForm
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(1000, 600);
            this.Controls.Add(this.tabControl);
            this.Name = "MenuManagementForm";
            this.Text = "Menu Management";
            this.tabControl.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private void SetupUI()
        {
            // ============================================
            // CATEGORIES TAB LAYOUT
            // ============================================
            Panel pnlCatContainer = new Panel { Dock = DockStyle.Fill, BackColor = ColorBackground };
            tabCategories.Controls.Add(pnlCatContainer);

            // DataGridView Categories
            dgvCategories = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                GridColor = Color.FromArgb(233, 231, 225),
                RowHeadersVisible = false,
                Font = new Font("Segoe UI", 10F)
            };
            StyleDataGridView(dgvCategories);

            dgvCategories.Columns.Add("Id", "ID");
            dgvCategories.Columns["Id"].Visible = false;
            dgvCategories.Columns.Add("Name", "Name");
            dgvCategories.Columns.Add("Slug", "Slug");
            dgvCategories.Columns.Add("DisplayOrder", "Display Order");
            dgvCategories.Columns.Add("IsActive", "Active");

            // Categories Action Panel
            Panel pnlCatActions = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = Color.White,
                Padding = new Padding(10)
            };

            btnAddCategory = CreateButton("➕ Add Category", ColorPrimary, Color.White, new Point(10, 10), new Size(150, 40));
            btnAddCategory.Click += BtnAddCategory_Click;

            btnEditCategory = CreateButton("✏️ Edit Category", ColorSecondary, Color.White, new Point(170, 10), new Size(150, 40));
            btnEditCategory.Click += BtnEditCategory_Click;

            btnDeleteCategory = CreateButton("🗑️ Delete", Color.FromArgb(108, 117, 125), Color.White, new Point(330, 10), new Size(100, 40));
            btnDeleteCategory.Click += BtnDeleteCategory_Click;

            btnPublishCategories = CreateButton("🚀 Publish to Website", ColorDark, Color.White, new Point(780, 10), new Size(180, 40));
            btnPublishCategories.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            btnPublishCategories.Click += async (s, e) => await PublishChanges("categories");

            pnlCatActions.Controls.Add(btnAddCategory);
            pnlCatActions.Controls.Add(btnEditCategory);
            pnlCatActions.Controls.Add(btnDeleteCategory);
            pnlCatActions.Controls.Add(btnPublishCategories);

            pnlCatContainer.Controls.Add(dgvCategories);
            pnlCatContainer.Controls.Add(pnlCatActions);

            // ============================================
            // PRODUCTS TAB LAYOUT
            // ============================================
            Panel pnlProdContainer = new Panel { Dock = DockStyle.Fill, BackColor = ColorBackground };
            tabProducts.Controls.Add(pnlProdContainer);

            // Filters Top Panel
            Panel pnlProdFilters = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.White,
                Padding = new Padding(10)
            };

            Label lblFilterCat = new Label { Text = "Category:", Location = new Point(15, 20), AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            cmbProductCategory = new ComboBox
            {
                Location = new Point(90, 16),
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F)
            };
            cmbProductCategory.SelectedIndexChanged += (s, e) => LoadProducts();

            Label lblSearch = new Label { Text = "Search:", Location = new Point(315, 20), AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            txtProductSearch = new TextBox
            {
                Location = new Point(375, 16),
                Width = 250,
                Font = new Font("Segoe UI", 10F),
                PlaceholderText = "Search by name or slug..."
            };
            txtProductSearch.TextChanged += (s, e) => LoadProducts();

            pnlProdFilters.Controls.Add(lblFilterCat);
            pnlProdFilters.Controls.Add(cmbProductCategory);
            pnlProdFilters.Controls.Add(lblSearch);
            pnlProdFilters.Controls.Add(txtProductSearch);

            // DataGridView Products
            dgvProducts = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                GridColor = Color.FromArgb(233, 231, 225),
                RowHeadersVisible = false,
                Font = new Font("Segoe UI", 10F)
            };
            StyleDataGridView(dgvProducts);

            dgvProducts.Columns.Add("Id", "ID");
            dgvProducts.Columns["Id"].Visible = false;
            dgvProducts.Columns.Add("Name", "Name");
            dgvProducts.Columns.Add("Category", "Category");
            dgvProducts.Columns.Add("Price", "Base Price");
            dgvProducts.Columns.Add("Discount", "Discount Price");
            dgvProducts.Columns.Add("Variations", "Variations");
            dgvProducts.Columns.Add("IsActive", "Active");

            // Products Action Panel
            Panel pnlProdActions = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = Color.White,
                Padding = new Padding(10)
            };

            btnAddProduct = CreateButton("➕ Add Product", ColorPrimary, Color.White, new Point(10, 10), new Size(150, 40));
            btnAddProduct.Click += BtnAddProduct_Click;

            btnEditProduct = CreateButton("✏️ Edit Product", ColorSecondary, Color.White, new Point(170, 10), new Size(150, 40));
            btnEditProduct.Click += BtnEditProduct_Click;

            btnDeleteProduct = CreateButton("🗑️ Delete", Color.FromArgb(108, 117, 125), Color.White, new Point(330, 10), new Size(100, 40));
            btnDeleteProduct.Click += BtnDeleteProduct_Click;

            btnPublishProducts = CreateButton("🚀 Publish to Website", ColorDark, Color.White, new Point(780, 10), new Size(180, 40));
            btnPublishProducts.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            btnPublishProducts.Click += async (s, e) => await PublishChanges("products");

            pnlProdActions.Controls.Add(btnAddProduct);
            pnlProdActions.Controls.Add(btnEditProduct);
            pnlProdActions.Controls.Add(btnDeleteProduct);
            pnlProdActions.Controls.Add(btnPublishProducts);

            pnlProdContainer.Controls.Add(dgvProducts);
            pnlProdContainer.Controls.Add(pnlProdFilters);
            pnlProdContainer.Controls.Add(pnlProdActions);
        }

        private void StyleDataGridView(DataGridView dgv)
        {
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = ColorPrimary;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv.ColumnHeadersHeight = 35;

            dgv.DefaultCellStyle.BackColor = Color.White;
            dgv.DefaultCellStyle.ForeColor = Color.FromArgb(53, 57, 59);
            dgv.DefaultCellStyle.SelectionBackColor = ColorSecondary;
            dgv.DefaultCellStyle.SelectionForeColor = Color.White;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 243, 238);
            dgv.RowTemplate.Height = 30;
        }

        private Button CreateButton(string text, Color backColor, Color foreColor, Point location, Size size)
        {
            var btn = new Button
            {
                Text = text,
                BackColor = backColor,
                ForeColor = foreColor,
                Location = location,
                Size = size,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        // ============================================
        // DATABASE LOADING HELPERS
        // ============================================
        private void LoadCategories()
        {
            try
            {
                dgvCategories.Rows.Clear();
                var categories = _dbService.GetCategories();

                foreach (var cat in categories)
                {
                    dgvCategories.Rows.Add(
                        cat.Id,
                        cat.Name,
                        cat.Slug,
                        cat.DisplayOrder,
                        cat.IsActive ? "✅ Active" : "❌ Inactive"
                    );
                }

                // Update combo box filter in Products
                cmbProductCategory.SelectedIndexChanged -= (s, e) => LoadProducts();
                string prevSelected = cmbProductCategory.SelectedItem?.ToString();
                
                cmbProductCategory.Items.Clear();
                cmbProductCategory.Items.Add("All Categories");

                foreach (var cat in categories)
                {
                    cmbProductCategory.Items.Add(cat.Name);
                }

                if (!string.IsNullOrEmpty(prevSelected) && cmbProductCategory.Items.Contains(prevSelected))
                {
                    cmbProductCategory.SelectedItem = prevSelected;
                }
                else
                {
                    cmbProductCategory.SelectedIndex = 0;
                }

                cmbProductCategory.SelectedIndexChanged += (s, e) => LoadProducts();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading categories: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadProducts()
        {
            try
            {
                dgvProducts.Rows.Clear();
                var products = _dbService.GetProducts();
                var categories = _dbService.GetCategories();

                // Apply Category Filter
                if (cmbProductCategory.SelectedIndex > 0)
                {
                    string catName = cmbProductCategory.SelectedItem.ToString();
                    var category = categories.FirstOrDefault(c => c.Name == catName);
                    if (category != null)
                    {
                        products = products.Where(p => p.CategoryId == category.Id).ToList();
                    }
                }

                // Apply Search Filter
                string query = txtProductSearch.Text.Trim().ToLower();
                if (!string.IsNullOrEmpty(query))
                {
                    products = products.Where(p => p.Name.ToLower().Contains(query) || p.Slug.ToLower().Contains(query)).ToList();
                }

                foreach (var p in products)
                {
                    var catName = categories.FirstOrDefault(c => c.Id == p.CategoryId)?.Name ?? "Uncategorized";
                    dgvProducts.Rows.Add(
                        p.Id,
                        p.Name,
                        catName,
                        $"PKR {p.BasePrice:N0}",
                        p.DiscountPrice.HasValue ? $"PKR {p.DiscountPrice.Value:N0}" : "-",
                        p.HasVariations ? "✅ Yes" : "❌ No",
                        p.IsActive ? "✅ Active" : "❌ Inactive"
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading products: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ============================================
        // CATEGORY CRUDS
        // ============================================
        private void BtnAddCategory_Click(object sender, EventArgs e)
        {
            using (var dlg = new CategoryDialog())
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var newCat = dlg.CategoryData;

                        var existingCats = _dbService.GetCategories();
                        if (existingCats.Any(c => c.Name.Equals(newCat.Name, StringComparison.OrdinalIgnoreCase)))
                        {
                            MessageBox.Show("A category with this name already exists.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        newCat.Id = Guid.NewGuid().ToString();
                        _dbService.CreateCategory(newCat);
                        LoadCategories();
                        
                        // Immediate background sync trigger
                        System.Threading.Tasks.Task.Run(async () => {
                            try { await new SyncService().SyncNow(); } catch {}
                        });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to add category: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnEditCategory_Click(object sender, EventArgs e)
        {
            if (dgvCategories.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a category to edit.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string catId = dgvCategories.SelectedRows[0].Cells["Id"].Value.ToString();
            var categories = _dbService.GetCategories();
            var target = categories.FirstOrDefault(c => c.Id == catId);

            if (target == null) return;

            using (var dlg = new CategoryDialog(target))
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var updated = dlg.CategoryData;

                        var existingCats = _dbService.GetCategories();
                        if (existingCats.Any(c => c.Id != catId && c.Name.Equals(updated.Name, StringComparison.OrdinalIgnoreCase)))
                        {
                            MessageBox.Show("A category with this name already exists.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        if (!updated.IsActive)
                        {
                            var products = _dbService.GetProducts(catId);
                            if (products.Any(p => p.IsActive))
                            {
                                MessageBox.Show("Cannot mark category as inactive because it contains active products.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }
                        }

                        updated.Id = catId;
                        _dbService.UpdateCategory(updated);
                        LoadCategories();

                        // Immediate background sync trigger
                        System.Threading.Tasks.Task.Run(async () => {
                            try { await new SyncService().SyncNow(); } catch {}
                        });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to edit category: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnDeleteCategory_Click(object sender, EventArgs e)
        {
            if (dgvCategories.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a category to delete.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string catId = dgvCategories.SelectedRows[0].Cells["Id"].Value.ToString();
            string catName = dgvCategories.SelectedRows[0].Cells["Name"].Value.ToString();

            var products = _dbService.GetProducts(catId);
            if (products != null && products.Count > 0)
            {
                MessageBox.Show("Cannot delete category because it contains products.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var confirm = MessageBox.Show($"Are you sure you want to delete category '{catName}' permanently?", 
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirm == DialogResult.Yes)
            {
                try
                {
                    _dbService.DeleteCategory(catId);
                    LoadCategories();

                    // Immediate background sync trigger
                    System.Threading.Tasks.Task.Run(async () => {
                        try { await new SyncService().SyncNow(); } catch {}
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to delete category: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ============================================
        // PRODUCT CRUDS
        // ============================================
        private void BtnAddProduct_Click(object sender, EventArgs e)
        {
            var categories = _dbService.GetCategories();
            if (categories.Count == 0)
            {
                MessageBox.Show("Please add at least one category before adding products.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var dlg = new ProductDialog(categories))
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var newProd = dlg.ProductData;

                        var existingProds = _dbService.GetProducts("all");
                        if (existingProds.Any(p => p.Name.Equals(newProd.Name, StringComparison.OrdinalIgnoreCase)))
                        {
                            MessageBox.Show("A product with this name already exists.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        if (string.IsNullOrEmpty(newProd.Id))
                        {
                            newProd.Id = Guid.NewGuid().ToString();
                        }
                        
                        // Assign the product ID to all of its variations
                        if (newProd.Variations != null)
                        {
                            foreach (var v in newProd.Variations)
                            {
                                v.ProductId = newProd.Id;
                            }
                        }

                        _dbService.CreateProduct(newProd);
                        LoadProducts();

                        // Immediate background sync trigger
                        System.Threading.Tasks.Task.Run(async () => {
                            try { await new SyncService().SyncNow(); } catch {}
                        });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to add product: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnEditProduct_Click(object sender, EventArgs e)
        {
            if (dgvProducts.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a product to edit.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string prodId = dgvProducts.SelectedRows[0].Cells["Id"].Value.ToString();
            var target = _dbService.GetProductById(prodId);

            if (target == null) return;

            var categories = _dbService.GetCategories();

            using (var dlg = new ProductDialog(categories, target))
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var updated = dlg.ProductData;

                        var existingProds = _dbService.GetProducts("all");
                        if (existingProds.Any(p => p.Id != prodId && p.Name.Equals(updated.Name, StringComparison.OrdinalIgnoreCase)))
                        {
                            MessageBox.Show("A product with this name already exists.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        updated.Id = prodId;

                        if (updated.Variations != null)
                        {
                            foreach (var v in updated.Variations)
                            {
                                v.ProductId = prodId;
                            }
                        }

                        _dbService.UpdateProduct(updated);
                        LoadProducts();

                        // Immediate background sync trigger
                        System.Threading.Tasks.Task.Run(async () => {
                            try { await new SyncService().SyncNow(); } catch {}
                        });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to edit product: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnDeleteProduct_Click(object sender, EventArgs e)
        {
            if (dgvProducts.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a product to delete.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string prodId = dgvProducts.SelectedRows[0].Cells["Id"].Value.ToString();
            string prodName = dgvProducts.SelectedRows[0].Cells["Name"].Value.ToString();

            var confirm = MessageBox.Show($"Are you sure you want to delete product '{prodName}' permanently?", 
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirm == DialogResult.Yes)
            {
                try
                {
                    _dbService.DeleteProduct(prodId);
                    LoadProducts();

                    // Immediate background sync trigger
                    System.Threading.Tasks.Task.Run(async () => {
                        try { await new SyncService().SyncNow(); } catch {}
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to delete product: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async System.Threading.Tasks.Task PublishChanges(string type)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;
                var syncService = new SyncService();
                await syncService.SyncNow();
                MessageBox.Show($"✅ Menu {type} published and synchronized with Cloud API successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Sync Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }
    }

    // ============================================
    // DIALOG: CATEGORY EDITOR
    // ============================================
    public class CategoryDialog : Form
    {
        private TextBox txtName;
        private TextBox txtSlug;
        private NumericUpDown numDisplayOrder;
        private CheckBox chkActive;
        private Button btnSave;
        private Button btnCancel;

        public Category CategoryData { get; private set; }

        public CategoryDialog(Category category = null)
        {
            InitializeComponent();
            StyleDialogControls();

            if (category != null)
            {
                this.Text = "✏️ Edit Category";
                txtName.Text = category.Name;
                txtSlug.Text = category.Slug;
                numDisplayOrder.Value = category.DisplayOrder;
                chkActive.Checked = category.IsActive;
            }
            else
            {
                this.Text = "➕ Add Category";
                txtName.Text = "";
                txtSlug.Text = "";
                numDisplayOrder.Value = 0;
                chkActive.Checked = true;
            }
        }

        private void InitializeComponent()
        {
            this.Size = new Size(420, 290);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            int y = 20;

            Label lblName = new Label { Text = "Name:", Location = new Point(20, y), AutoSize = true };
            txtName = new TextBox { Location = new Point(130, y - 3), Width = 250 };
            txtName.TextChanged += TxtName_TextChanged;
            y += 40;

            Label lblSlug = new Label { Text = "Slug:", Location = new Point(20, y), AutoSize = true };
            txtSlug = new TextBox { Location = new Point(130, y - 3), Width = 250 };
            y += 40;

            Label lblOrder = new Label { Text = "Display Order:", Location = new Point(20, y), AutoSize = true };
            numDisplayOrder = new NumericUpDown { Location = new Point(130, y - 3), Width = 100, Maximum = 10000 };
            y += 40;

            chkActive = new CheckBox { Text = "Active", Location = new Point(130, y), Checked = true };
            y += 45;

            btnSave = new Button { Text = "Save", Location = new Point(170, y), Width = 100, Height = 35, DialogResult = DialogResult.OK };
            btnCancel = new Button { Text = "Cancel", Location = new Point(280, y), Width = 100, Height = 35, DialogResult = DialogResult.Cancel };

            this.Controls.Add(lblName);
            this.Controls.Add(txtName);
            this.Controls.Add(lblSlug);
            this.Controls.Add(txtSlug);
            this.Controls.Add(lblOrder);
            this.Controls.Add(numDisplayOrder);
            this.Controls.Add(chkActive);
            this.Controls.Add(btnSave);
            this.Controls.Add(btnCancel);

            btnSave.Click += BtnSave_Click;
        }

        private void StyleDialogControls()
        {
            this.Font = new Font("Segoe UI", 10F);
            this.BackColor = Color.FromArgb(250, 249, 246);

            btnSave.BackColor = Color.FromArgb(230, 57, 70); // primary
            btnSave.ForeColor = Color.White;
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Font = new Font("Segoe UI", 10F, FontStyle.Bold);

            btnCancel.BackColor = Color.FromArgb(108, 117, 125);
            btnCancel.ForeColor = Color.White;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        }

        private void TxtName_TextChanged(object sender, EventArgs e)
        {
            string name = txtName.Text.Trim().ToLower();
            name = name.Replace("&", "and");
            name = name.Replace("'", "");
            name = name.Replace("\"", "");
            
            var sb = new System.Text.StringBuilder();
            foreach (char c in name)
            {
                if (char.IsLetterOrDigit(c))
                {
                    sb.Append(c);
                }
                else if (c == ' ' || c == '-' || c == '_' || c == '/' || c == '\\')
                {
                    sb.Append('-');
                }
            }

            string slug = sb.ToString();
            while (slug.Contains("--"))
            {
                slug = slug.Replace("--", "-");
            }
            slug = slug.Trim('-');

            txtSlug.Text = slug;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.DialogResult = DialogResult.None;
                return;
            }

            CategoryData = new Category
            {
                Name = txtName.Text.Trim(),
                Slug = string.IsNullOrWhiteSpace(txtSlug.Text) ? txtName.Text.Trim().ToLower().Replace(" ", "-") : txtSlug.Text.Trim(),
                DisplayOrder = (int)numDisplayOrder.Value,
                IsActive = chkActive.Checked
            };
        }
    }

    // ============================================
    // DIALOG: PRODUCT EDITOR


    // ============================================
    // DIALOG: VARIATION EDITOR
    // ============================================
    public class VariationDialog : Form
    {
        private ComboBox cmbType;
        private ComboBox cmbName;
        private NumericUpDown numPriceAdjustment;
        private CheckBox chkDefault;
        private Button btnSave;
        private Button btnCancel;

        public ProductVariation VariationData { get; private set; }

        public VariationDialog()
        {
            InitializeComponent();
            StyleDialogControls();
            
            cmbType.Items.Clear();
            cmbType.Items.Add("Size");
            cmbType.Items.Add("Flavor");
            cmbType.Items.Add("Option");
            cmbType.SelectedIndex = 0;

            cmbType.SelectedIndexChanged += (s, e) =>
            {
                cmbName.Items.Clear();
                string selectedType = cmbType.SelectedItem.ToString();
                if (selectedType == "Size")
                {
                    cmbName.Items.AddRange(new object[] { "S", "M", "L", "XL", "XXL", "Regular", "Large", "Personal", "Family", "Standard" });
                }
                else if (selectedType == "Flavor")
                {
                    cmbName.Items.AddRange(new object[] { "Spicy", "Mild", "Original", "Tikka", "Fajita", "Cheese", "Mayo" });
                }
            };

            cmbType.SelectedIndex = 0;
        }

        private void InitializeComponent()
        {
            this.Size = new Size(380, 250);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Text = "➕ Add Variation";

            int y = 20;

            Label lblType = new Label { Text = "Type:", Location = new Point(20, y), AutoSize = true };
            cmbType = new ComboBox { Location = new Point(140, y - 3), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            y += 35;

            Label lblName = new Label { Text = "Variation Name:", Location = new Point(20, y), AutoSize = true };
            cmbName = new ComboBox { Location = new Point(140, y - 3), Width = 200, DropDownStyle = ComboBoxStyle.DropDown };
            y += 35;

            Label lblPrice = new Label { Text = "Price Adjustment:", Location = new Point(20, y), AutoSize = true };
            numPriceAdjustment = new NumericUpDown { Location = new Point(140, y - 3), Width = 120, Minimum = -99999, Maximum = 999999 };
            y += 35;

            chkDefault = new CheckBox { Text = "Is Default Option", Location = new Point(140, y) };
            y += 40;

            btnSave = new Button { Text = "Add", Location = new Point(130, y), Width = 100, Height = 35, DialogResult = DialogResult.OK };
            btnCancel = new Button { Text = "Cancel", Location = new Point(240, y), Width = 100, Height = 35, DialogResult = DialogResult.Cancel };

            this.Controls.Add(lblType);
            this.Controls.Add(cmbType);
            this.Controls.Add(lblName);
            this.Controls.Add(cmbName);
            this.Controls.Add(lblPrice);
            this.Controls.Add(numPriceAdjustment);
            this.Controls.Add(chkDefault);
            this.Controls.Add(btnSave);
            this.Controls.Add(btnCancel);

            btnSave.Click += BtnSave_Click;
        }

        private void StyleDialogControls()
        {
            this.Font = new Font("Segoe UI", 10F);
            this.BackColor = Color.FromArgb(250, 249, 246);

            btnSave.BackColor = Color.FromArgb(230, 57, 70); // primary
            btnSave.ForeColor = Color.White;
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Font = new Font("Segoe UI", 10F, FontStyle.Bold);

            btnCancel.BackColor = Color.FromArgb(108, 117, 125);
            btnCancel.ForeColor = Color.White;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(cmbName.Text))
            {
                MessageBox.Show("Variation Name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.DialogResult = DialogResult.None;
                return;
            }

            VariationData = new ProductVariation
            {
                Id = Guid.NewGuid().ToString(),
                VariationType = cmbType.SelectedItem.ToString().ToLower(),
                VariationName = cmbName.Text.Trim(),
                PriceAdjustment = numPriceAdjustment.Value,
                IsDefault = chkDefault.Checked
            };
        }
    }
}
