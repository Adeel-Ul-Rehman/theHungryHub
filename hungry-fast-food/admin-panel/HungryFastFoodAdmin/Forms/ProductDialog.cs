// E:\hungryHub\hungry-fast-food\admin-panel\HungryFastFoodAdmin\Forms\ProductDialog.cs

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using HungryFastFoodAdmin.Models;
using HungryFastFoodAdmin.Services;

namespace HungryFastFoodAdmin.Forms
{
    public class ProductDialog : Form
    {
        private ComboBox cmbCategory;
        private TextBox txtName;
        private TextBox txtSlug;
        private TextBox txtDescription;
        private TextBox txtImageUrl;
        private Button btnBrowseImage;
        private Button btnClearImage;
        private NumericUpDown numBasePrice;
        private NumericUpDown numDiscountPrice;
        private CheckBox chkHasVariations;
        private CheckBox chkActive;
        private DataGridView dgvVariations;
        private Button btnAddVar;
        private Button btnRemoveVar;
        private Button btnSave;
        private Button btnCancel;
        private PictureBox pbPreview;
        private Label lblUploadProgress;

        private List<Category> _categories;
        private List<ProductVariation> _variations = new List<ProductVariation>();
        
        private string _editImageUrl = "";
        private int _editDisplayOrder = 0;
        private bool _editIsDeal = false;
        private string _productId;

        public Product ProductData { get; private set; }

        public ProductDialog(List<Category> categories, Product product = null)
        {
            _categories = categories;
            _productId = product != null ? product.Id : Guid.NewGuid().ToString();

            InitializeComponent();
            StyleDialogControls();

            // Populate categories dropdown
            cmbCategory.Items.Clear();
            foreach (var cat in _categories)
            {
                cmbCategory.Items.Add(cat.Name);
            }
            if (cmbCategory.Items.Count > 0) cmbCategory.SelectedIndex = 0;

            if (product != null)
            {
                this.Text = "✏️ Edit Product";
                txtName.Text = product.Name;
                txtSlug.Text = product.Slug;
                txtDescription.Text = product.Description ?? "";
                numBasePrice.Value = product.BasePrice;
                numDiscountPrice.Value = product.DiscountPrice ?? 0;
                chkHasVariations.Checked = product.HasVariations;
                chkActive.Checked = product.IsActive;

                _editImageUrl = product.ImageUrl ?? "";
                txtImageUrl.Text = _editImageUrl;
                _editDisplayOrder = product.DisplayOrder;
                _editIsDeal = product.IsDeal;
                _variations = product.Variations ?? new List<ProductVariation>();

                LoadPreviewImage(_editImageUrl);
            }
            else
            {
                this.Text = "➕ Add Product";
                chkActive.Checked = true;
            }

            RefreshVariationsGrid();
            ToggleVariationsUI();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(880, 580);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            int y = 20;

            // --- Left Panel Content (Width: 600) ---
            Label lblCategory = new Label { Text = "Category:", Location = new Point(20, y), AutoSize = true };
            cmbCategory = new ComboBox { Location = new Point(140, y - 3), Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };
            
            Label lblActive = new Label { Text = "Status:", Location = new Point(360, y), AutoSize = true };
            chkActive = new CheckBox { Text = "Is Active Product", Location = new Point(440, y - 2), Checked = true };
            y += 35;

            Label lblName = new Label { Text = "Product Name:", Location = new Point(20, y), AutoSize = true };
            txtName = new TextBox { Location = new Point(140, y - 3), Width = 440 };
            txtName.TextChanged += TxtName_TextChanged;
            y += 35;

            Label lblSlug = new Label { Text = "Slug:", Location = new Point(20, y), AutoSize = true };
            txtSlug = new TextBox { Location = new Point(140, y - 3), Width = 440, ReadOnly = true };
            y += 35;

            Label lblDesc = new Label { Text = "Description:", Location = new Point(20, y), AutoSize = true };
            txtDescription = new TextBox { Location = new Point(140, y - 3), Width = 440 };
            y += 35;

            Label lblBasePrice = new Label { Text = "Base Price:", Location = new Point(20, y), AutoSize = true };
            numBasePrice = new NumericUpDown { Location = new Point(140, y - 3), Width = 140, Maximum = 999999 };
            
            Label lblDiscPrice = new Label { Text = "Discount Price:", Location = new Point(300, y), AutoSize = true };
            numDiscountPrice = new NumericUpDown { Location = new Point(420, y - 3), Width = 160, Maximum = 999999 };
            y += 35;

            chkHasVariations = new CheckBox { Text = "Product has Size/Flavor variations (disables base price calculation)", Location = new Point(140, y), Width = 440 };
            chkHasVariations.CheckedChanged += ChkHasVariations_CheckedChanged;
            y += 35;

            // Variations Section Panel
            Label lblVars = new Label { Text = "Variations:", Location = new Point(20, y), AutoSize = true };
            
            dgvVariations = new DataGridView
            {
                Location = new Point(140, y),
                Size = new Size(330, 150),
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                BackgroundColor = Color.White,
                RowHeadersVisible = false
            };
            dgvVariations.Columns.Add("Type", "Type");
            dgvVariations.Columns.Add("Name", "Variation Name");
            dgvVariations.Columns.Add("PriceAdj", "Price Adj.");
            dgvVariations.Columns.Add("Default", "Default");

            btnAddVar = new Button { Text = "➕ Add", Location = new Point(485, y), Width = 95, Height = 30 };
            btnAddVar.Click += BtnAddVar_Click;

            btnRemoveVar = new Button { Text = "🗑️ Remove", Location = new Point(485, y + 35), Width = 95, Height = 30 };
            btnRemoveVar.Click += BtnRemoveVar_Click;

            // --- Right Panel Content (Width: 260) ---
            Label lblImageTitle = new Label { Text = "Product Image:", Location = new Point(620, 20), AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            
            pbPreview = new PictureBox
            {
                Location = new Point(620, 50),
                Size = new Size(220, 220),
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(240, 238, 233)
            };

            txtImageUrl = new TextBox
            {
                Location = new Point(620, 285),
                Width = 220,
                ReadOnly = true,
                Font = new Font("Segoe UI", 8F)
            };

            lblUploadProgress = new Label
            {
                Location = new Point(620, 315),
                Width = 220,
                Height = 20,
                ForeColor = Color.FromArgb(230, 57, 70),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Visible = false
            };

            btnBrowseImage = new Button { Text = "📂 Browse...", Location = new Point(620, 345), Width = 105, Height = 35 };
            btnBrowseImage.Click += BtnBrowseImage_Click;

            btnClearImage = new Button { Text = "🧹 Clear", Location = new Point(735, 345), Width = 105, Height = 35 };
            btnClearImage.Click += BtnClearImage_Click;

            // --- Bottom Dialog Buttons ---
            y = 480;
            btnSave = new Button { Text = "Save", Location = new Point(620, y), Width = 105, Height = 40 };
            btnCancel = new Button { Text = "Cancel", Location = new Point(735, y), Width = 105, Height = 40, DialogResult = DialogResult.Cancel };

            // Left panel additions
            this.Controls.Add(lblCategory);
            this.Controls.Add(cmbCategory);
            this.Controls.Add(lblActive);
            this.Controls.Add(chkActive);
            this.Controls.Add(lblName);
            this.Controls.Add(txtName);
            this.Controls.Add(lblSlug);
            this.Controls.Add(txtSlug);
            this.Controls.Add(lblDesc);
            this.Controls.Add(txtDescription);
            this.Controls.Add(lblBasePrice);
            this.Controls.Add(numBasePrice);
            this.Controls.Add(lblDiscPrice);
            this.Controls.Add(numDiscountPrice);
            this.Controls.Add(chkHasVariations);
            this.Controls.Add(lblVars);
            this.Controls.Add(dgvVariations);
            this.Controls.Add(btnAddVar);
            this.Controls.Add(btnRemoveVar);

            // Right panel additions
            this.Controls.Add(lblImageTitle);
            this.Controls.Add(pbPreview);
            this.Controls.Add(txtImageUrl);
            this.Controls.Add(lblUploadProgress);
            this.Controls.Add(btnBrowseImage);
            this.Controls.Add(btnClearImage);

            // Actions additions
            this.Controls.Add(btnSave);
            this.Controls.Add(btnCancel);

            btnSave.Click += BtnSave_Click;
        }

        private void StyleDialogControls()
        {
            this.Font = new Font("Segoe UI", 10F);
            this.BackColor = Color.FromArgb(250, 249, 246);

            btnSave.BackColor = Color.FromArgb(230, 57, 70); // primary #E63946
            btnSave.ForeColor = Color.White;
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Font = new Font("Segoe UI", 10F, FontStyle.Bold);

            btnCancel.BackColor = Color.FromArgb(108, 117, 125);
            btnCancel.ForeColor = Color.White;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);

            btnBrowseImage.BackColor = Color.FromArgb(42, 157, 143);
            btnBrowseImage.ForeColor = Color.White;
            btnBrowseImage.FlatStyle = FlatStyle.Flat;
            btnBrowseImage.FlatAppearance.BorderSize = 0;
            btnBrowseImage.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

            btnClearImage.BackColor = Color.FromArgb(244, 162, 97);
            btnClearImage.ForeColor = Color.White;
            btnClearImage.FlatStyle = FlatStyle.Flat;
            btnClearImage.FlatAppearance.BorderSize = 0;
            btnClearImage.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

            btnAddVar.BackColor = Color.FromArgb(42, 157, 143);
            btnAddVar.ForeColor = Color.White;
            btnAddVar.FlatStyle = FlatStyle.Flat;
            btnAddVar.FlatAppearance.BorderSize = 0;
            btnAddVar.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

            btnRemoveVar.BackColor = Color.FromArgb(244, 162, 97);
            btnRemoveVar.ForeColor = Color.White;
            btnRemoveVar.FlatStyle = FlatStyle.Flat;
            btnRemoveVar.FlatAppearance.BorderSize = 0;
            btnRemoveVar.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

            // Style variations grid slightly
            dgvVariations.EnableHeadersVisualStyles = false;
            dgvVariations.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(230, 57, 70);
            dgvVariations.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvVariations.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgvVariations.RowTemplate.Height = 25;
            dgvVariations.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
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

        private void TxtName_TextChanged(object sender, EventArgs e)
        {
            string name = txtName.Text.Trim().ToLower();
            name = name.Replace("&", "and");
            name = name.Replace("@", "at");
            
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

        private void ChkHasVariations_CheckedChanged(object sender, EventArgs e)
        {
            ToggleVariationsUI();
        }

        private void ToggleVariationsUI()
        {
            bool hasVar = chkHasVariations.Checked;
            dgvVariations.Enabled = hasVar;
            btnAddVar.Enabled = hasVar;
            btnRemoveVar.Enabled = hasVar;
            
            if (hasVar)
            {
                dgvVariations.DefaultCellStyle.BackColor = Color.White;
                dgvVariations.DefaultCellStyle.ForeColor = Color.Black;
            }
            else
            {
                dgvVariations.DefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);
                dgvVariations.DefaultCellStyle.ForeColor = Color.Gray;
            }
        }

        private void RefreshVariationsGrid()
        {
            dgvVariations.Rows.Clear();
            foreach (var v in _variations)
            {
                dgvVariations.Rows.Add(
                    v.VariationType,
                    v.VariationName,
                    v.PriceAdjustment >= 0 ? $"+ PKR {v.PriceAdjustment:N0}" : $"- PKR {Math.Abs(v.PriceAdjustment):N0}",
                    v.IsDefault ? "✅ Yes" : "❌ No"
                );
            }
        }

        private void BtnAddVar_Click(object sender, EventArgs e)
        {
            using (var dlg = new VariationDialog())
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    var newVar = dlg.VariationData;
                    newVar.ProductId = _productId;

                    // If default is checked, uncheck previous defaults of the same type
                    if (newVar.IsDefault)
                    {
                        foreach (var v in _variations.Where(x => x.VariationType == newVar.VariationType))
                        {
                            v.IsDefault = false;
                        }
                    }

                    _variations.Add(newVar);
                    RefreshVariationsGrid();
                }
            }
        }

        private void BtnRemoveVar_Click(object sender, EventArgs e)
        {
            if (dgvVariations.SelectedRows.Count == 0) return;
            int idx = dgvVariations.SelectedRows[0].Index;
            if (idx >= 0 && idx < _variations.Count)
            {
                _variations.RemoveAt(idx);
                RefreshVariationsGrid();
            }
        }

        private async void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Product Name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cmbCategory.SelectedIndex < 0)
            {
                MessageBox.Show("Please select a valid Category.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (numDiscountPrice.Value > 0 && numDiscountPrice.Value >= numBasePrice.Value)
            {
                MessageBox.Show("Discount Price must be strictly less than the Base Price.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(_editImageUrl))
            {
                MessageBox.Show("Please select a product image. Products must have an image.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string catName = cmbCategory.SelectedItem.ToString();
            var category = _categories.FirstOrDefault(c => c.Name == catName);
            
            if (category == null)
            {
                MessageBox.Show("Category mapping mismatch error.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // If Has Variations is checked, make sure there's at least one variation added
            if (chkHasVariations.Checked && _variations.Count == 0)
            {
                MessageBox.Show("Please add at least one Variation or uncheck the 'Product has Size/Flavor variations' option.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Save local file path directly (uploading is deferred to background SyncService when online)
            ProductData = new Product
            {
                Id = _productId,
                CategoryId = category.Id,
                Name = txtName.Text.Trim(),
                Slug = string.IsNullOrWhiteSpace(txtSlug.Text) ? txtName.Text.Trim().ToLower().Replace(" ", "-") : txtSlug.Text.Trim(),
                Description = txtDescription.Text.Trim(),
                BasePrice = numBasePrice.Value,
                DiscountPrice = numDiscountPrice.Value > 0 ? numDiscountPrice.Value : (decimal?)null,
                HasVariations = chkHasVariations.Checked,
                IsActive = chkActive.Checked,
                IsDeal = _editIsDeal,
                ImageUrl = _editImageUrl,
                DisplayOrder = _editDisplayOrder,
                Variations = _variations
            };

            this.Cursor = Cursors.Default;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void ResetUIAfterFailure()
        {
            this.Cursor = Cursors.Default;
            btnSave.Enabled = true;
            btnCancel.Enabled = true;
            btnBrowseImage.Enabled = true;
            btnClearImage.Enabled = true;
            lblUploadProgress.Visible = false;
        }
    }
}
