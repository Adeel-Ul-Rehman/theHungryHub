// E:\hungryHub\hungry-fast-food\admin-panel\HungryFastFoodAdmin\Forms\OrderPunchForm.cs

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using HungryFastFoodAdmin.Models;
using HungryFastFoodAdmin.Services;
using HungryFastFoodAdmin.Controls;

namespace HungryFastFoodAdmin.Forms
{
    public partial class OrderPunchForm : BaseForm
    {
        private DatabaseService _dbService;
        private PrintService _printService;
        private QRCodeService _qrService;

        // UI Controls
        private Panel topPanel;
        private Panel searchContainer;
        private TextBox txtSearch;
        private Panel categoryContainer;
        private FlowLayoutPanel categoryPanel;
        private FlowLayoutPanel productPanel;

        // Cart Sidebar
        private Panel cartPanel;
        private Panel cartHeader;
        private Label lblCartTitle;
        private FlowLayoutPanel cartItemsPanel;
        private Panel cartFooter;

        // Cart Footer Controls
        private Label lblCustomerName;
        private TextBox txtCustomerName;
        private Label lblCustomerPhone;
        private TextBox txtCustomerPhone;
        private Label lblOrderType;
        private ComboBox cmbOrderType;
        private Label lblPaymentMethod;
        private ComboBox cmbPaymentMethod;
        private Label lblDeliveryAddress;
        private TextBox txtDeliveryAddress;
        private Panel separator;

        private Label lblSubtotal;
        private Label lblTax;
        private Label lblDeliveryCharge;
        private Label lblTotal;
        private Button btnPlaceOrder;
        private Button btnClearOrder;

        // Data Fields
        private List<Product> _products = new List<Product>();
        private List<Category> _categories = new List<Category>();
        private List<CartItem> _cartItems = new List<CartItem>();
        private string _currentOrderNumber;
        private string _currentOrderType = "dining";
        private decimal _deliveryCharge = 0;

        public OrderPunchForm()
        {
            _dbService = new DatabaseService();
            _printService = new PrintService();
            _qrService = new QRCodeService();

            InitializeComponent();
            SetupUI();

            LoadCategories();
            LoadCategoryTabs();
            LoadProducts();
            UpdateOrderNumber();

            this.KeyPreview = true;
            this.KeyDown += OrderPunchForm_KeyDown;
        }

        private void InitializeComponent()
        {
            this.topPanel = new Panel();
            this.categoryPanel = new FlowLayoutPanel();
            this.productPanel = new FlowLayoutPanel();
            this.cartPanel = new Panel();
            this.cartHeader = new Panel();
            this.lblCartTitle = new Label();
            this.cartItemsPanel = new FlowLayoutPanel();
            this.cartFooter = new Panel();

            this.lblCustomerName = new Label();
            this.txtCustomerName = new TextBox();
            this.lblCustomerPhone = new Label();
            this.txtCustomerPhone = new TextBox();
            this.lblOrderType = new Label();
            this.cmbOrderType = new ComboBox();
            this.lblPaymentMethod = new Label();
            this.cmbPaymentMethod = new ComboBox();
            this.lblDeliveryAddress = new Label();
            this.txtDeliveryAddress = new TextBox();
            this.separator = new Panel();

            this.lblSubtotal = new Label();
            this.lblTax = new Label();
            this.lblDeliveryCharge = new Label();
            this.lblTotal = new Label();
            this.btnPlaceOrder = new Button();
            this.btnClearOrder = new Button();

            this.SuspendLayout();

            // Basic Form settings
            this.ClientSize = new Size(1400, 850);
            this.Name = "OrderPunchForm";
            this.Text = "Order Punch - Hungry Hub";
            this.WindowState = FormWindowState.Maximized;
            this.ResumeLayout(false);
        }

        private void SetupUI()
        {
            this.BackColor = Color.FromArgb(250, 249, 246);
            this.Dock = DockStyle.Fill;

            // ============================================
            // TOP PANEL (Search & Categories)
            // ============================================
            topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 110,
                BackColor = Color.FromArgb(250, 249, 246),
                Padding = new Padding(20, 10, 20, 10)
            };

            // Search Bar Container
            searchContainer = new Panel
            {
                Size = new Size(320, 36),
                Location = new Point(20, 15),
                BackColor = Color.White
            };
            searchContainer.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var path = GetRoundedRectPath(new Rectangle(0, 0, searchContainer.Width - 1, searchContainer.Height - 1), 8);
                using (var pen = new Pen(Color.FromArgb(200, 200, 200), 1))
                {
                    g.DrawPath(pen, path);
                }
            };

            Label lblSearchIcon = new Label
            {
                Text = "🔍",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Gray,
                Location = new Point(8, 8),
                Size = new Size(20, 20),
                BackColor = Color.White
            };

            txtSearch = new TextBox
            {
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 10.5f),
                Location = new Point(32, 9),
                Size = new Size(275, 20),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(53, 57, 59)
            };
            txtSearch.TextChanged += TxtSearch_TextChanged;

            searchContainer.Controls.Add(lblSearchIcon);
            searchContainer.Controls.Add(txtSearch);
            topPanel.Controls.Add(searchContainer);

            // Category tabs container
            categoryContainer = new Panel
            {
                Location = new Point(20, 60),
                Size = new Size(1000, 45),
                BackColor = Color.Transparent
            };

            var btnScrollLeft = new Button
            {
                Text = "◀",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Dock = DockStyle.Left,
                Width = 28,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(240, 240, 240),
                ForeColor = Color.FromArgb(53, 57, 59),
                Cursor = Cursors.Hand
            };
            btnScrollLeft.FlatAppearance.BorderSize = 0;

            var btnScrollRight = new Button
            {
                Text = "▶",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Dock = DockStyle.Right,
                Width = 28,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(240, 240, 240),
                ForeColor = Color.FromArgb(53, 57, 59),
                Cursor = Cursors.Hand
            };
            btnScrollRight.FlatAppearance.BorderSize = 0;

            var wrapper = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };

            categoryPanel = new FlowLayoutPanel
            {
                Location = new Point(0, 0),
                Size = new Size(wrapper.Width, 65),
                AutoScroll = true,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.Transparent,
                Margin = new Padding(0)
            };

            wrapper.Controls.Add(categoryPanel);
            categoryContainer.Controls.Add(wrapper);
            categoryContainer.Controls.Add(btnScrollLeft);
            categoryContainer.Controls.Add(btnScrollRight);

            topPanel.Controls.Add(categoryContainer);

            // Scroll button actions
            btnScrollLeft.Click += (s, e) =>
            {
                int newVal = Math.Max(0, categoryPanel.HorizontalScroll.Value - 120);
                categoryPanel.HorizontalScroll.Value = newVal;
                categoryPanel.Invalidate();
            };

            btnScrollRight.Click += (s, e) =>
            {
                int newVal = Math.Min(categoryPanel.HorizontalScroll.Maximum, categoryPanel.HorizontalScroll.Value + 120);
                categoryPanel.HorizontalScroll.Value = newVal;
                categoryPanel.Invalidate();
            };

            wrapper.Resize += (s, e) =>
            {
                categoryPanel.Width = wrapper.Width;
            };

            // ============================================
            // CART SIDEBAR (Right Sidebar)
            // ============================================
            cartPanel = new Panel
            {
                Dock = DockStyle.Right,
                Width = 380,
                BackColor = Color.White
            };
            cartPanel.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(220, 220, 220), 2))
                {
                    e.Graphics.DrawLine(pen, 0, 0, 0, cartPanel.Height);
                }
            };

            // Cart Header
            cartHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 55,
                BackColor = Color.FromArgb(230, 57, 70)
            };

            lblCartTitle = new Label
            {
                Text = "🛒 Current Order",
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            cartHeader.Controls.Add(lblCartTitle);

            // Cart Items Flow Panel
            cartItemsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(15, 10, 15, 10),
                BackColor = Color.White
            };

            // Cart Footer (Fixed bottom of sidebar)
            cartFooter = new Panel
            {
                Dock = DockStyle.Bottom,
                BackColor = Color.FromArgb(250, 249, 246),
                Padding = new Padding(15, 10, 15, 10)
            };

            // Form inputs setup inside Footer
            lblCustomerName = new Label
            {
                Text = "Customer Name *",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(53, 57, 59),
                Height = 18
            };
            txtCustomerName = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Height = 25
            };

            lblCustomerPhone = new Label
            {
                Text = "Phone",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(53, 57, 59),
                Height = 18
            };
            txtCustomerPhone = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Height = 25
            };

            lblOrderType = new Label
            {
                Text = "Order Type",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(53, 57, 59),
                Height = 18
            };
            cmbOrderType = new ComboBox
            {
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Height = 25
            };
            cmbOrderType.Items.AddRange(new object[] { "Dining", "Delivery", "Takeaway" });
            cmbOrderType.SelectedIndex = 0;
            cmbOrderType.SelectedIndexChanged += CmbOrderType_SelectedIndexChanged;

            lblPaymentMethod = new Label
            {
                Text = "Payment Method",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(53, 57, 59),
                Height = 18
            };
            cmbPaymentMethod = new ComboBox
            {
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Height = 25
            };
            cmbPaymentMethod.Items.AddRange(new object[] { "Cash", "JazzCash", "COD" });
            cmbPaymentMethod.SelectedIndex = 0;

            lblDeliveryAddress = new Label
            {
                Text = "Delivery Address *",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(53, 57, 59),
                Height = 18,
                Visible = false
            };
            txtDeliveryAddress = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Height = 50,
                Multiline = true,
                Visible = false
            };

            separator = new Panel
            {
                Height = 1,
                BackColor = Color.FromArgb(220, 220, 220)
            };

            lblSubtotal = new Label
            {
                Text = "Subtotal: PKR 0",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
                ForeColor = Color.Gray,
                Height = 20,
                TextAlign = ContentAlignment.MiddleRight
            };

            lblTax = new Label
            {
                Text = "Tax (5%): PKR 0",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
                ForeColor = Color.Gray,
                Height = 20,
                TextAlign = ContentAlignment.MiddleRight
            };

            lblDeliveryCharge = new Label
            {
                Text = "Delivery Charge: PKR 0",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(42, 157, 143),
                Height = 20,
                TextAlign = ContentAlignment.MiddleRight,
                Visible = false
            };

            lblTotal = new Label
            {
                Text = "TOTAL: PKR 0",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(53, 57, 59),
                Height = 30,
                TextAlign = ContentAlignment.MiddleRight
            };

            btnPlaceOrder = new Button
            {
                Text = "✅ Place Order",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(42, 157, 143),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Height = 40,
                Cursor = Cursors.Hand
            };
            btnPlaceOrder.FlatAppearance.BorderSize = 0;
            btnPlaceOrder.Click += BtnPlaceOrder_Click;

            btnClearOrder = new Button
            {
                Text = "🗑️ Clear",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(200, 200, 200),
                ForeColor = Color.FromArgb(53, 57, 59),
                FlatStyle = FlatStyle.Flat,
                Height = 40,
                Cursor = Cursors.Hand
            };
            btnClearOrder.FlatAppearance.BorderSize = 0;
            btnClearOrder.Click += BtnClearOrder_Click;

            // Add fields to cart footer
            cartFooter.Controls.Add(lblCustomerName);
            cartFooter.Controls.Add(txtCustomerName);
            cartFooter.Controls.Add(lblCustomerPhone);
            cartFooter.Controls.Add(txtCustomerPhone);
            cartFooter.Controls.Add(lblOrderType);
            cartFooter.Controls.Add(cmbOrderType);
            cartFooter.Controls.Add(lblPaymentMethod);
            cartFooter.Controls.Add(cmbPaymentMethod);
            cartFooter.Controls.Add(lblDeliveryAddress);
            cartFooter.Controls.Add(txtDeliveryAddress);
            cartFooter.Controls.Add(separator);
            cartFooter.Controls.Add(lblSubtotal);
            cartFooter.Controls.Add(lblTax);
            cartFooter.Controls.Add(lblDeliveryCharge);
            cartFooter.Controls.Add(lblTotal);
            cartFooter.Controls.Add(btnPlaceOrder);
            cartFooter.Controls.Add(btnClearOrder);

            // Hook layout updates
            cartFooter.Resize += (s, e) => LayoutFooter();
            LayoutFooter();

            // ============================================
            // CENTER PRODUCT GRID PANEL
            // ============================================
            productPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(15),
                BackColor = Color.FromArgb(250, 249, 246),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true
            };

            // Responsive categories tab sizing
            this.Resize += (s, e) =>
            {
                categoryContainer.Width = this.Width - cartPanel.Width - 40;
            };

            // ============================================
            // ADD CONTROLS TO FORM (NO BOTTOM BAR)
            // ============================================
            this.Controls.Add(productPanel);
            this.Controls.Add(topPanel);
            this.Controls.Add(cartPanel);

            cartPanel.Controls.Add(cartItemsPanel);
            cartPanel.Controls.Add(cartFooter);
            cartPanel.Controls.Add(cartHeader);
        }

        private void LayoutFooter()
        {
            int y = 10;
            int margin = 8;
            int controlWidth = cartFooter.Width - 30;

            // Customer Name
            lblCustomerName.Location = new Point(15, y);
            y += 18;
            txtCustomerName.Location = new Point(15, y);
            txtCustomerName.Width = controlWidth;
            y += txtCustomerName.Height + margin;

            // Customer Phone
            lblCustomerPhone.Location = new Point(15, y);
            y += 18;
            txtCustomerPhone.Location = new Point(15, y);
            txtCustomerPhone.Width = controlWidth;
            y += txtCustomerPhone.Height + margin;

            // Row for Order Type and Payment Method
            lblOrderType.Location = new Point(15, y);
            lblPaymentMethod.Location = new Point(15 + controlWidth / 2 + 5, y);
            y += 18;
            cmbOrderType.Location = new Point(15, y);
            cmbOrderType.Width = controlWidth / 2 - 5;
            cmbPaymentMethod.Location = new Point(15 + controlWidth / 2 + 5, y);
            cmbPaymentMethod.Width = controlWidth / 2 - 5;
            y += cmbOrderType.Height + margin;

            // Delivery Address
            if (_currentOrderType == "delivery")
            {
                lblDeliveryAddress.Visible = true;
                txtDeliveryAddress.Visible = true;
                lblDeliveryAddress.Location = new Point(15, y);
                y += 18;
                txtDeliveryAddress.Location = new Point(15, y);
                txtDeliveryAddress.Width = controlWidth;
                y += txtDeliveryAddress.Height + margin;
            }
            else
            {
                lblDeliveryAddress.Visible = false;
                txtDeliveryAddress.Visible = false;
            }

            // Separator
            separator.Location = new Point(15, y);
            separator.Width = controlWidth;
            y += separator.Height + margin;

            // Totals
            lblSubtotal.Location = new Point(15, y);
            lblSubtotal.Width = controlWidth;
            y += lblSubtotal.Height + 2;

            lblTax.Location = new Point(15, y);
            lblTax.Width = controlWidth;
            y += lblTax.Height + 2;

            if (_currentOrderType == "delivery")
            {
                lblDeliveryCharge.Visible = true;
                lblDeliveryCharge.Location = new Point(15, y);
                lblDeliveryCharge.Width = controlWidth;
                y += lblDeliveryCharge.Height + 2;
            }
            else
            {
                lblDeliveryCharge.Visible = false;
            }

            lblTotal.Location = new Point(15, y);
            lblTotal.Width = controlWidth;
            y += lblTotal.Height + margin;

            // Action Buttons
            btnPlaceOrder.Location = new Point(15, y);
            btnPlaceOrder.Width = (int)(controlWidth * 0.65);
            btnClearOrder.Location = new Point(15 + btnPlaceOrder.Width + 10, y);
            btnClearOrder.Width = controlWidth - btnPlaceOrder.Width - 10;
            y += btnPlaceOrder.Height + margin;

            // Resize panel
            cartFooter.Height = y + 10;
        }

        private void LoadCategories()
        {
            try
            {
                _categories = _dbService.GetCategories().Where(c => c.IsActive).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading categories: {ex.Message}");
            }

            // Ensure "All" virtual category is at the beginning
            var allCat = _categories.FirstOrDefault(c => c.Name == "All" || c.Slug == "all");
            if (allCat == null)
            {
                allCat = new Category { Id = "all", Name = "All", Slug = "all", IsActive = true };
                _categories.Insert(0, allCat);
            }
            else
            {
                _categories.Remove(allCat);
                _categories.Insert(0, allCat);
            }
        }

        private void LoadCategoryTabs()
        {
            categoryPanel.Controls.Clear();

            foreach (var category in _categories.Where(c => c.IsActive))
            {
                var btn = new CategoryButton
                {
                    CategoryId = category.Id,
                    CategoryName = category.Name,
                    IsSelected = category.Slug == "all",
                    Margin = new Padding(0, 0, 8, 0)
                };
                btn.AutoSize = true;
                btn.Click += CategoryButton_Click;
                categoryPanel.Controls.Add(btn);
            }
        }

        private void CategoryButton_Click(object sender, EventArgs e)
        {
            var clickedBtn = sender as CategoryButton;
            if (clickedBtn == null) return;

            foreach (Control ctrl in categoryPanel.Controls)
            {
                if (ctrl is CategoryButton btn)
                {
                    btn.IsSelected = (btn == clickedBtn);
                }
            }

            string categoryId = clickedBtn.CategoryId;
            LoadProducts(categoryId);
        }

        private void LoadProducts(string categoryId = null)
        {
            productPanel.Controls.Clear();

            try
            {
                var products = _dbService.GetProducts(categoryId).Where(p => p.IsActive).ToList();
                foreach (var product in products.Where(p => p.IsActive))
                {
                    var card = new ProductCard(product);
                    card.AddClicked += (s, e) =>
                    {
                        ShowQuantityAndAdd(product);
                    };
                    productPanel.Controls.Add(card);
                }

                // Show deals if selected category is deals, or if looking at "All"
                bool showDeals = false;
                if (string.IsNullOrEmpty(categoryId) || categoryId == "all")
                {
                    showDeals = true;
                }
                else
                {
                    var currentCat = _categories.FirstOrDefault(c => c.Id == categoryId);
                    if (currentCat != null && (currentCat.Name.ToLower().Contains("deal") || currentCat.Slug.ToLower().Contains("deal")))
                    {
                        showDeals = true;
                    }
                }

                if (showDeals)
                {
                    var deals = _dbService.GetDeals();
                    foreach (var deal in deals.Where(d => d.IsActive))
                    {
                        var dealProduct = new Product
                        {
                            Id = deal.Id,
                            CategoryId = categoryId ?? "deals",
                            Name = deal.Name,
                            Slug = deal.Slug,
                            Description = deal.Description,
                            BasePrice = deal.TotalPrice,
                            DiscountPrice = deal.DiscountPrice,
                            HasVariations = false,
                            IsActive = deal.IsActive,
                            IsDeal = true,
                            ImageUrl = deal.ImageUrl
                        };

                        var card = new ProductCard(dealProduct);
                        card.AddClicked += (s, e) =>
                        {
                            ShowQuantityAndAdd(dealProduct);
                        };
                        productPanel.Controls.Add(card);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading products: {ex.Message}");
            }
        }

        private decimal GetTaxRate()
        {
            try
            {
                string taxStr = _dbService.GetSetting("tax_rate", "5");
                if (decimal.TryParse(taxStr, out decimal parsedRate))
                {
                    return parsedRate / 100m;
                }
            }
            catch { }
            return 0.05m; // fallback 5%
        }

public void ShowQuantityAndAdd(Product product)
{
    var fullProduct = _dbService.GetProductById(product.Id) ?? product;

    if (fullProduct.HasVariations && fullProduct.Variations != null && fullProduct.Variations.Count > 0)
    {
        using (var sizeDialog = new SizeSelectionDialog(fullProduct))
        {
            if (sizeDialog.ShowDialog(this) == DialogResult.OK)
            {
                var selectedVar = sizeDialog.SelectedVariation;
                AddToCart(fullProduct, 1, selectedVar);
            }
        }
    }
    else
    {
        // Direct add with quantity 1 - NO POPUP
        AddToCart(fullProduct, 1, null);
    }
}

        private void AddToCart(Product product, int quantity, ProductVariation variation)
        {
            string displayName = product.Name;
            decimal price = product.DiscountPrice ?? product.BasePrice;

            if (variation != null)
            {
                displayName = $"{product.Name} ({variation.VariationName})";
                price += variation.PriceAdjustment;
            }

            var existing = _cartItems.FirstOrDefault(c => c.ProductId == product.Id &&
                                                          c.VariationName == (variation != null ? variation.VariationName : null));
            if (existing != null)
            {
                existing.Quantity += quantity;
            }
            else
            {
                _cartItems.Add(new CartItem
                {
                    ProductId = product.Id,
                    ProductName = displayName,
                    VariationName = variation != null ? variation.VariationName : null,
                    UnitPrice = price,
                    Quantity = quantity,
                    IsFromDeal = product.IsDeal,
                    IsDeal = product.IsDeal,
                    DealId = product.IsDeal ? product.Id : null
                });
            }

            UpdateCartUI();
        }

        private void UpdateTotals()
        {
            decimal subtotal = 0;
            foreach (var item in _cartItems)
            {
                subtotal += item.UnitPrice * item.Quantity;
            }

            decimal taxRate = GetTaxRate();
            decimal tax = subtotal * taxRate;
            decimal total = subtotal + tax + _deliveryCharge;

            lblSubtotal.Text = $"Subtotal: PKR {subtotal:F0}";
            lblTax.Text = $"Tax ({taxRate * 100:F0}%): PKR {tax:F0}";
            lblDeliveryCharge.Text = $"Delivery Charge: PKR {_deliveryCharge:F0}";
            lblTotal.Text = $"TOTAL: PKR {total:F0}";
        }

        private void UpdateCartUI()
        {
            cartItemsPanel.Controls.Clear();

            foreach (var item in _cartItems)
            {
                var ctrl = new CartItemControl(item);
                ctrl.QuantityChanged += (s, e) =>
                {
                    UpdateTotals();
                };
                ctrl.ItemRemoved += (s, e) =>
                {
                    _cartItems.Remove(item);
                    UpdateCartUI();
                };
                cartItemsPanel.Controls.Add(ctrl);
            }

            UpdateTotals();
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            string searchText = txtSearch.Text.Trim().ToLower();
            if (string.IsNullOrEmpty(searchText))
            {
                LoadProducts();
                return;
            }

            productPanel.Controls.Clear();
            try
            {
                var products = _dbService.GetProducts().Where(p => p.IsActive).ToList();
                var filtered = products.Where(p => p.Name.ToLower().Contains(searchText) && p.IsActive).ToList();

                foreach (var product in filtered)
                {
                    var card = new ProductCard(product);
                    card.AddClicked += (s, e2) =>
                    {
                        ShowQuantityAndAdd(product);
                    };
                    productPanel.Controls.Add(card);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Search error: {ex.Message}");
            }
        }

        private void CmbOrderType_SelectedIndexChanged(object sender, EventArgs e)
        {
            _currentOrderType = cmbOrderType.SelectedItem.ToString().ToLower();

            bool isDelivery = _currentOrderType == "delivery";
            lblDeliveryAddress.Visible = isDelivery;
            txtDeliveryAddress.Visible = isDelivery;

            if (isDelivery)
            {
                decimal charge = 0;
                string zonesJson = _dbService.GetSetting("delivery_zones", "");
                if (!string.IsNullOrEmpty(zonesJson))
                {
                    try
                    {
                        var zones = Newtonsoft.Json.JsonConvert.DeserializeObject<List<dynamic>>(zonesJson);
                        if (zones != null && zones.Count > 0)
                        {
                            var sortedZones = zones.OrderBy(z => (decimal)z.charge).ToList();
                            charge = (decimal)sortedZones.First().charge;
                        }
                    }
                    catch { }
                }
                _deliveryCharge = charge;
            }
            else
            {
                _deliveryCharge = 0;
            }

            LayoutFooter();
            UpdateCartUI();
        }

        private void ClearCart()
        {
            _cartItems.Clear();
            txtCustomerName.Clear();
            txtCustomerPhone.Clear();
            txtDeliveryAddress.Clear();
            cmbOrderType.SelectedIndex = 0;
            cmbPaymentMethod.SelectedIndex = 0;
            UpdateCartUI();
        }

        private void BtnClearOrder_Click(object sender, EventArgs e)
        {
            if (_cartItems.Count > 0 || !string.IsNullOrEmpty(txtCustomerName.Text))
            {
                if (MessageBox.Show("Are you sure you want to clear the current order?", "Confirm",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    ClearCart();
                }
            }
        }

        private void PlaceOrderFlow()
        {
            try
            {
                if (!ValidateOrder()) return;

                var order = CreateOrderFromCart();

                // =========================================================
                // Smart Status Assignment Based on Order Type
                // =========================================================
                // Dining & Takeaway (from admin punch): Auto-start baking timer
                //   → status = "preparing" → appears in KDS with timer running, no confirm/cancel
                // Delivery (from admin punch): Needs confirmation
                //   → status = "pending" → appears in KDS with confirm/cancel, timer starts after confirm
                // =========================================================
                bool isImmediateOrder = (_currentOrderType == "dining" || _currentOrderType == "takeaway");
                if (isImmediateOrder)
                {
                    order.Status = "preparing";
                    order.PaymentStatus = "paid";
                }
                else // delivery
                {
                    order.Status = "pending";
                    order.PaymentStatus = "pending";
                }

                var savedOrder = _dbService.CreateOrder(order, order.Items);

                // Play sound confirmation
                try
                {
                    System.Media.SystemSounds.Asterisk.Play();
                }
                catch { }

                // Print bill receipt
                _printService.PrintBill(savedOrder);

                // Print kitchen slip for immediate orders (dining/takeaway)
                if (isImmediateOrder)
                {
                    _printService.PrintKitchenSlip(savedOrder);
                }

                string orderTypeDisplay = char.ToUpper(_currentOrderType[0]) + _currentOrderType.Substring(1);
                MessageBox.Show($"Order {savedOrder.OrderNumber} placed successfully!\n\n" +
                    $"Type: {orderTypeDisplay}\n" +
                    $"Status: {(isImmediateOrder ? "Baking Started 🍳" : "Pending Confirmation ⏳")}",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                ClearCart();
                UpdateOrderNumber();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error placing order: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnPlaceOrder_Click(object sender, EventArgs e)
        {
            PlaceOrderFlow();
        }

        private bool ValidateOrder()
        {
            if (_cartItems.Count == 0)
            {
                MessageBox.Show("Your cart is empty!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtCustomerName.Text))
            {
                MessageBox.Show("Customer Name is required!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCustomerName.Focus();
                return false;
            }

            if (_currentOrderType == "delivery" && string.IsNullOrWhiteSpace(txtDeliveryAddress.Text))
            {
                MessageBox.Show("Delivery Address is required for Delivery orders!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtDeliveryAddress.Focus();
                return false;
            }

            return true;
        }

        private Order CreateOrderFromCart()
        {
            var order = new Order
            {
                OrderType = _currentOrderType,
                CustomerName = txtCustomerName.Text.Trim(),
                CustomerPhone = txtCustomerPhone.Text.Trim(),
                CustomerEmail = "",
                DeliveryAddress = _currentOrderType == "delivery" ? txtDeliveryAddress.Text.Trim() : "",
                Status = "pending",
                PaymentMethod = cmbPaymentMethod.SelectedItem.ToString().ToLower(),
                PaymentStatus = "pending",
                IsSuspicious = false,
                AdminNotes = "",
                Items = new List<OrderItem>()
            };

            decimal subtotal = 0;
            foreach (var item in _cartItems)
            {
                var orderItem = new OrderItem
                {
                    ProductName = item.ProductName,
                    VariationName = item.VariationName,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.UnitPrice * item.Quantity,
                    IsFromDeal = item.IsFromDeal,
                    DealId = item.DealId
                };
                order.Items.Add(orderItem);
                subtotal += orderItem.TotalPrice;
            }

            decimal taxRate = GetTaxRate();
            order.Subtotal = subtotal;
            order.Tax = subtotal * taxRate;
            order.DeliveryCharge = _deliveryCharge;
            order.Total = subtotal + order.Tax + order.DeliveryCharge;

            return order;
        }

        private void UpdateOrderNumber()
        {
            _currentOrderNumber = _dbService.GetOrderNumber("dining");
        }

        private void OrderPunchForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F1)
            {
                txtSearch.Focus();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.F2)
            {
                BtnClearOrder_Click(null, null);
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.F3)
            {
                BtnPlaceOrder_Click(null, null);
                e.Handled = true;
            }
        }

        private GraphicsPath GetRoundedRectPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            int diameter = radius * 2;
            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    // ============================================
    // HELPER CLASSES (Only these remain)
    // ============================================

    public class CartItem
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string VariationName { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public bool IsFromDeal { get; set; }
        public bool IsDeal { get; set; }
        public string DealId { get; set; }
    }

    public class QuantityDialog : Form
    {
        public int Quantity { get; private set; } = 1;
        private NumericUpDown nudQuantity;
        private Button btnOK;
        private Button btnCancel;

        public QuantityDialog()
        {
            this.Text = "Select Quantity";
            this.Size = new Size(300, 150);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            Label lblQty = new Label
            {
                Text = "Quantity:",
                Location = new Point(20, 30),
                Size = new Size(80, 25),
                Font = new Font("Segoe UI", 11)
            };

            nudQuantity = new NumericUpDown
            {
                Location = new Point(110, 30),
                Size = new Size(80, 25),
                Minimum = 1,
                Maximum = 99,
                Value = 1,
                Font = new Font("Segoe UI", 11)
            };

            btnOK = new Button
            {
                Text = "Add to Order",
                Location = new Point(80, 70),
                Size = new Size(120, 30),
                BackColor = Color.FromArgb(42, 157, 143),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                DialogResult = DialogResult.OK
            };
            btnOK.FlatAppearance.BorderSize = 0;
            btnOK.Click += (s, e) => { Quantity = (int)nudQuantity.Value; };

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(210, 70),
                Size = new Size(70, 30),
                BackColor = Color.FromArgb(200, 200, 200),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.FromArgb(53, 57, 59),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                DialogResult = DialogResult.Cancel
            };
            btnCancel.FlatAppearance.BorderSize = 0;

            this.Controls.Add(lblQty);
            this.Controls.Add(nudQuantity);
            this.Controls.Add(btnOK);
            this.Controls.Add(btnCancel);
        }
    }
}