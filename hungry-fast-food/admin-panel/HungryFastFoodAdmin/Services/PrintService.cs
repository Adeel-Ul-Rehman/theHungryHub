// E:\hungryHub\hungry-fast-food\admin-panel\HungryFastFoodAdmin\Services\PrintService.cs

using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Text;
using System.Linq;
using HungryFastFoodAdmin.Models;

namespace HungryFastFoodAdmin.Services
{
    public class PrintService
    {
        private readonly string _printerName;
        private readonly int _paperWidth;
        private Order _currentOrder;
        private Order _kitchenOrder;
        private QRCodeService _qrService;

        private int PrintWidth => _paperWidth == 80 ? 280 : 226;
        private int RightMargin => PrintWidth - 10;

        public PrintService()
        {
            try
            {
                var db = new DatabaseService();
                var settings = db.GetSystemSettings();
                _printerName = settings.ContainsKey("printer_name") ? settings["printer_name"] : ConfigManager.GetAppSetting("PrinterName", "EPSON TM-T20");
                
                string widthStr = settings.ContainsKey("paper_width") ? settings["paper_width"] : ConfigManager.GetAppSetting("PrinterPaperWidth", "80");
                _paperWidth = Convert.ToInt32(widthStr);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading printer settings from database, using app.config fallback: {ex.Message}");
                _printerName = ConfigManager.GetAppSetting("PrinterName", "EPSON TM-T20");
                _paperWidth = Convert.ToInt32(ConfigManager.GetAppSetting("PrinterPaperWidth", "80"));
            }
            _qrService = new QRCodeService();
        }

        private string GetIslamabadFormattedTime(string createdAtUtc)
        {
            if (string.IsNullOrEmpty(createdAtUtc))
            {
                var pktTime = DateTime.UtcNow.AddHours(5);
                return pktTime.ToString("dd MMM yyyy hh:mm tt");
            }

            try
            {
                if (DateTime.TryParse(createdAtUtc, out DateTime parsed))
                {
                    DateTime pktTime;
                    if (parsed.Kind == DateTimeKind.Utc)
                    {
                        pktTime = parsed.AddHours(5);
                    }
                    else
                    {
                        pktTime = DateTime.SpecifyKind(parsed, DateTimeKind.Utc).AddHours(5);
                    }
                    return pktTime.ToString("dd MMM yyyy hh:mm tt");
                }
            }
            catch { }
            return createdAtUtc;
        }

        public void PrintBill(Order order)
        {
            _currentOrder = order;

            try
            {
                var printDoc = new PrintDocument();
                printDoc.PrinterSettings.PrinterName = _printerName;
                printDoc.PrintPage += PrintDoc_PrintPage;

                // Set paper size for thermal printer (dynamic width based on config)
                printDoc.DefaultPageSettings.PaperSize = new PaperSize("Thermal", PrintWidth, 1000);
                printDoc.DefaultPageSettings.Margins = new Margins(10, 10, 10, 10);

                printDoc.Print();
                Console.WriteLine($"🖨️ Bill printed for order {order.OrderNumber}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Print error: {ex.Message}");
                Logger.Log($"Print error: {ex.Message}");
                throw;
            }
        }

        private void PrintDoc_PrintPage(object sender, PrintPageEventArgs e)
        {
            DrawBill(e.Graphics, out _);
        }

        public void DrawBill(Graphics g, out int finalY)
        {
            var font = new Font("Segoe UI", 9);
            var boldFont = new Font("Segoe UI", 9, FontStyle.Bold);
            var headerFont = new Font("Segoe UI", 13, FontStyle.Bold);
            var detailFont = new Font("Segoe UI", 8.5F);
            var y = 10;

            int width = PrintWidth;
            int rightMargin = RightMargin;

            // Center Title
            string title = "HUNGRY HUB";
            float titleX = (width - g.MeasureString(title, headerFont).Width) / 2;
            g.DrawString(title, headerFont, Brushes.Black, titleX, y);
            y += 25;

            // Address split in two lines and centered
            string addrLine1 = "Zaki Plaza, Muslim Town,";
            string addrLine2 = "Rawalpindi";
            float addr1X = (width - g.MeasureString(addrLine1, detailFont).Width) / 2;
            g.DrawString(addrLine1, detailFont, Brushes.Black, addr1X, y);
            y += 16;
            float addr2X = (width - g.MeasureString(addrLine2, detailFont).Width) / 2;
            g.DrawString(addrLine2, detailFont, Brushes.Black, addr2X, y);
            y += 16;

            // Phone and Email centered
            string phoneStr = "Phone: 0336-0357333";
            float phoneX = (width - g.MeasureString(phoneStr, detailFont).Width) / 2;
            g.DrawString(phoneStr, detailFont, Brushes.Black, phoneX, y);
            y += 16;

            string emailStr = "Email: thehungryhub26@gmail.com";
            float emailX = (width - g.MeasureString(emailStr, detailFont).Width) / 2;
            g.DrawString(emailStr, detailFont, Brushes.Black, emailX, y);
            y += 25;

            // Thin separating line
            using (var pen = new Pen(Color.Black, 1))
            {
                g.DrawLine(pen, 10, y, rightMargin, y);
            }
            y += 12;

            // Left-aligned labels, X = 95 aligned values
            int valX = 95;
            g.DrawString("Order #:", boldFont, Brushes.Black, 10, y);
            g.DrawString(_currentOrder.OrderNumber, boldFont, Brushes.Black, valX, y);
            y += 18;

            g.DrawString("Type:", font, Brushes.Black, 10, y);
            g.DrawString(_currentOrder.OrderType.ToUpper(), font, Brushes.Black, valX, y);
            y += 18;

            g.DrawString("Customer:", font, Brushes.Black, 10, y);
            g.DrawString(_currentOrder.CustomerName, font, Brushes.Black, valX, y);
            y += 18;

            if (!string.IsNullOrEmpty(_currentOrder.CustomerPhone))
            {
                g.DrawString("Phone:", font, Brushes.Black, 10, y);
                g.DrawString(_currentOrder.CustomerPhone, font, Brushes.Black, valX, y);
                y += 18;
            }

            if (!string.IsNullOrEmpty(_currentOrder.DeliveryAddress))
            {
                g.DrawString("Address:", font, Brushes.Black, 10, y);
                int addrWidth = rightMargin - valX;
                int addrHeight = (int)g.MeasureString(_currentOrder.DeliveryAddress, font, addrWidth).Height;
                var rect = new RectangleF(valX, y, addrWidth, addrHeight + 5);
                g.DrawString(_currentOrder.DeliveryAddress, font, Brushes.Black, rect);
                y += Math.Max(18, addrHeight + 5);
            }

            // Customer Special Instructions
            if (!string.IsNullOrEmpty(_currentOrder.AdminNotes))
            {
                g.DrawString("Instructions:", boldFont, Brushes.Black, 10, y);
                int notesWidth = rightMargin - valX;
                int notesHeight = (int)g.MeasureString(_currentOrder.AdminNotes, font, notesWidth).Height;
                var rect = new RectangleF(valX, y, notesWidth, notesHeight + 5);
                g.DrawString(_currentOrder.AdminNotes, font, Brushes.Black, rect);
                y += Math.Max(18, notesHeight + 5);
            }
            y += 5;

            // Thin separating line
            using (var pen = new Pen(Color.Black, 1))
            {
                g.DrawLine(pen, 10, y, rightMargin, y);
            }
            y += 12;

            // Items columns
            g.DrawString("Item", boldFont, Brushes.Black, 10, y);
            
            string qtyHeader = "Qty";
            int colQtyX = rightMargin - 70;
            g.DrawString(qtyHeader, boldFont, Brushes.Black, colQtyX - g.MeasureString(qtyHeader, boldFont).Width, y);

            string rateHeader = "Rate";
            int colRateX = rightMargin - 40;
            g.DrawString(rateHeader, boldFont, Brushes.Black, colRateX - g.MeasureString(rateHeader, boldFont).Width, y);

            string totalHeader = "Total";
            int colTotalX = rightMargin;
            g.DrawString(totalHeader, boldFont, Brushes.Black, colTotalX - g.MeasureString(totalHeader, boldFont).Width, y);

            y += 18;

            using (var pen = new Pen(Color.Black, 1))
            {
                g.DrawLine(pen, 10, y, rightMargin, y);
            }
            y += 8;

            foreach (var item in _currentOrder.Items)
            {
                string name = item.ProductName;
                if (!string.IsNullOrEmpty(item.VariationName))
                {
                    name += $" ({item.VariationName})";
                }

                // Draw wrapped item name
                int nameWidth = colQtyX - 25;
                int nameH = (int)g.MeasureString(name, font, nameWidth).Height;
                var nameRect = new RectangleF(10, y, nameWidth, nameH + 5);
                g.DrawString(name, font, Brushes.Black, nameRect);

                string qStr = item.Quantity.ToString();
                g.DrawString(qStr, font, Brushes.Black, colQtyX - g.MeasureString(qStr, font).Width, y);

                string rateStr = item.UnitPrice.ToString("F0");
                g.DrawString(rateStr, font, Brushes.Black, colRateX - g.MeasureString(rateStr, font).Width, y);

                string totalStr = item.TotalPrice.ToString("F0");
                g.DrawString(totalStr, font, Brushes.Black, colTotalX - g.MeasureString(totalStr, font).Width, y);

                y += Math.Max(nameH, 16) + 6;
            }
            y += 5;

            // Thin separating line
            using (var pen = new Pen(Color.Black, 1))
            {
                g.DrawLine(pen, 10, y, rightMargin, y);
            }
            y += 12;

            // Totals: right aligned
            string subtotalStr = $"PKR {_currentOrder.Subtotal:F0}";
            g.DrawString("Subtotal", font, Brushes.Black, 10, y);
            g.DrawString(subtotalStr, font, Brushes.Black, colTotalX - g.MeasureString(subtotalStr, font).Width, y);
            y += 18;

            if (_currentOrder.DeliveryCharge > 0)
            {
                string deliveryStr = $"PKR {_currentOrder.DeliveryCharge:F0}";
                g.DrawString("Delivery", font, Brushes.Black, 10, y);
                g.DrawString(deliveryStr, font, Brushes.Black, colTotalX - g.MeasureString(deliveryStr, font).Width, y);
                y += 18;
            }

            string taxStr = $"PKR {_currentOrder.Tax:F0}";
            g.DrawString("Tax", font, Brushes.Black, 10, y);
            g.DrawString(taxStr, font, Brushes.Black, colTotalX - g.MeasureString(taxStr, font).Width, y);
            y += 18;

            using (var pen = new Pen(Color.Black, 1))
            {
                g.DrawLine(pen, 10, y, rightMargin, y);
            }
            y += 10;

            string totalStrVal = $"PKR {_currentOrder.Total:F0}";
            g.DrawString("TOTAL", boldFont, Brushes.Black, 10, y);
            g.DrawString(totalStrVal, boldFont, Brushes.Black, colTotalX - g.MeasureString(totalStrVal, boldFont).Width, y);
            y += 22;

            // Payment method and status
            string paymentMethod = _currentOrder.PaymentMethod?.ToUpper() ?? "CASH";
            bool isPaid = _currentOrder.PaymentStatus != null && 
                          (_currentOrder.PaymentStatus.Equals("completed", StringComparison.OrdinalIgnoreCase) || 
                           _currentOrder.PaymentStatus.Equals("paid", StringComparison.OrdinalIgnoreCase));
            string paymentStatusText = isPaid ? "PAID" : "UNPAID";
            string paymentStr = $"{paymentMethod} ({paymentStatusText})";
            g.DrawString("Payment:", font, Brushes.Black, 10, y);
            g.DrawString(paymentStr, font, Brushes.Black, colTotalX - g.MeasureString(paymentStr, font).Width, y);
            y += 18;

            string statusStr = _currentOrder.Status.ToUpper();
            g.DrawString("Status:", font, Brushes.Black, 10, y);
            g.DrawString(statusStr, font, Brushes.Black, colTotalX - g.MeasureString(statusStr, font).Width, y);
            y += 18;

            // Date/Time in Islamabad format (12-hour format)
            string dateStr = GetIslamabadFormattedTime(_currentOrder.CreatedAt);
            g.DrawString("Date:", font, Brushes.Black, 10, y);
            g.DrawString(dateStr, font, Brushes.Black, colTotalX - g.MeasureString(dateStr, font).Width, y);
            y += 22;

            // Thin separating line
            using (var pen = new Pen(Color.Black, 1))
            {
                g.DrawLine(pen, 10, y, rightMargin, y);
            }
            y += 15;

            // QR Code for ALL delivery orders that have a customer-provided location
            bool hasLocation = !string.IsNullOrEmpty(_currentOrder.DeliveryAddress) ||
                              (_currentOrder.DeliveryLatitude.HasValue && _currentOrder.DeliveryLongitude.HasValue &&
                               _currentOrder.DeliveryLatitude != 0 && _currentOrder.DeliveryLongitude != 0);

            if (_currentOrder.OrderType.ToLower() == "delivery" && hasLocation)
            {
                string mapsUrl = "";
                if (_currentOrder.DeliveryLatitude.HasValue && _currentOrder.DeliveryLongitude.HasValue &&
                    _currentOrder.DeliveryLatitude != 0 && _currentOrder.DeliveryLongitude != 0)
                {
                    mapsUrl = $"https://www.google.com/maps/search/?api=1&query={_currentOrder.DeliveryLatitude.Value},{_currentOrder.DeliveryLongitude.Value}";
                }
                else
                {
                    mapsUrl = $"https://www.google.com/maps/search/?api=1&query={Uri.EscapeDataString(_currentOrder.DeliveryAddress ?? "")}";
                }

                var qrImage = _qrService.GenerateQRCode(mapsUrl);
                if (qrImage != null)
                {
                    // QR heading
                    string qrHeading = "Scan for Delivery Location";
                    float qrHeadX = (width - g.MeasureString(qrHeading, boldFont).Width) / 2;
                    g.DrawString(qrHeading, boldFont, Brushes.Black, qrHeadX, y);
                    y += 18;

                    g.DrawImage(qrImage, new Rectangle((width - 100) / 2, y, 100, 100));
                    y += 110;

                    using (var pen = new Pen(Color.Black, 1))
                    {
                        g.DrawLine(pen, 10, y, rightMargin, y);
                    }
                    y += 15;
                }
            }

            // Footer centered
            string footer1 = "Thank you for your order!";
            float f1X = (width - g.MeasureString(footer1, boldFont).Width) / 2;
            g.DrawString(footer1, boldFont, Brushes.Black, f1X, y);
            y += 18;

            string footer2 = "Visit us at: hungryhub.com";
            float f2X = (width - g.MeasureString(footer2, font).Width) / 2;
            g.DrawString(footer2, font, Brushes.Black, f2X, y);
            y += 18;

            string footer3 = "Order confirmed via call";
            float f3X = (width - g.MeasureString(footer3, font).Width) / 2;
            g.DrawString(footer3, font, Brushes.Black, f3X, y);
            y += 25;

            // Cut paper command
            g.DrawString("\x1B\x69", font, Brushes.Black, 0, y);
            finalY = y;
        }

        public Bitmap GenerateBillBitmap(Order order)
        {
            _currentOrder = order;
            int bmpWidth = PrintWidth;
            using (var tempBmp = new Bitmap(bmpWidth, 2500))
            {
                using (var g = Graphics.FromImage(tempBmp))
                {
                    g.Clear(Color.White);
                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    DrawBill(g, out int finalY);

                    int height = Math.Min(2500, finalY + 40);
                    var cropped = new Bitmap(bmpWidth, height);
                    using (var destG = Graphics.FromImage(cropped))
                    {
                        destG.Clear(Color.White);
                        destG.DrawImage(tempBmp, new Rectangle(0, 0, bmpWidth, height), new Rectangle(0, 0, bmpWidth, height), GraphicsUnit.Pixel);
                    }
                    return cropped;
                }
            }
        }

        public bool TestPrinter()
        {
            try
            {
                var printDoc = new PrintDocument();
                printDoc.PrinterSettings.PrinterName = _printerName;

                // Check if printer exists
                if (!PrinterSettings.InstalledPrinters.Contains(_printerName))
                {
                    throw new Exception($"Printer '{_printerName}' not found");
                }

                // Print test page
                printDoc.PrintPage += (sender, e) =>
                {
                    e.Graphics.DrawString("Test Print - Hungry Hub", new Font("Arial", 12), Brushes.Black, 10, 10);
                    e.Graphics.DrawString($"Date: {DateTime.Now}", new Font("Arial", 10), Brushes.Black, 10, 40);
                    e.Graphics.DrawString("Printer is working correctly!", new Font("Arial", 10), Brushes.Black, 10, 70);
                };

                printDoc.Print();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Printer test error: {ex.Message}");
                return false;
            }
        }

        public void PrintKitchenSlip(Order order)
        {
            _kitchenOrder = order;
            try
            {
                var printDoc = new PrintDocument();
                printDoc.PrinterSettings.PrinterName = _printerName;
                printDoc.PrintPage += PrintDoc_PrintKitchenPage;

                printDoc.DefaultPageSettings.PaperSize = new PaperSize("Thermal", PrintWidth, 1000);
                printDoc.DefaultPageSettings.Margins = new Margins(10, 10, 10, 10);

                printDoc.Print();
                Console.WriteLine($"🖨️ Kitchen Slip printed for order {order.OrderNumber}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Kitchen print error: {ex.Message}");
            }
        }

        private void PrintDoc_PrintKitchenPage(object sender, PrintPageEventArgs e)
        {
            DrawKitchenSlip(e.Graphics, out _);
        }

        public void DrawKitchenSlip(Graphics g, out int finalY)
        {
            var font = new Font("Segoe UI", 9.5F);
            var boldFont = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            var headerFont = new Font("Segoe UI", 13, FontStyle.Bold);
            var y = 10;

            int width = PrintWidth;
            int rightMargin = RightMargin;

            // Center KITCHEN SLIP
            string title = "*** KITCHEN SLIP ***";
            float titleX = (width - g.MeasureString(title, headerFont).Width) / 2;
            g.DrawString(title, headerFont, Brushes.Black, titleX, y);
            y += 30;

            int valX = 95;
            g.DrawString("Order #:", boldFont, Brushes.Black, 10, y);
            g.DrawString(_kitchenOrder.OrderNumber, boldFont, Brushes.Black, valX, y);
            y += 18;

            g.DrawString("Type:", font, Brushes.Black, 10, y);
            g.DrawString(_kitchenOrder.OrderType.ToUpper(), font, Brushes.Black, valX, y);
            y += 18;

            bool isKitchenPaid = _kitchenOrder.PaymentStatus != null && 
                                 (_kitchenOrder.PaymentStatus.Equals("completed", StringComparison.OrdinalIgnoreCase) || 
                                  _kitchenOrder.PaymentStatus.Equals("paid", StringComparison.OrdinalIgnoreCase));
            string kitchenPaymentStatusText = isKitchenPaid ? "PAID" : "UNPAID";
            g.DrawString("Payment:", font, Brushes.Black, 10, y);
            g.DrawString($"{_kitchenOrder.PaymentMethod?.ToUpper() ?? "CASH"} ({kitchenPaymentStatusText})", boldFont, Brushes.Black, valX, y);
            y += 18;

            g.DrawString("Customer:", font, Brushes.Black, 10, y);
            g.DrawString(_kitchenOrder.CustomerName, font, Brushes.Black, valX, y);
            y += 18;

            // Date/Time in Islamabad format (12-hour format)
            string dateStr = GetIslamabadFormattedTime(_kitchenOrder.CreatedAt);
            g.DrawString("Date/Time:", font, Brushes.Black, 10, y);
            g.DrawString(dateStr, font, Brushes.Black, valX, y);
            y += 22;

            // Special Instructions
            if (!string.IsNullOrEmpty(_kitchenOrder.AdminNotes))
            {
                g.DrawString("Note/Instr:", boldFont, Brushes.Black, 10, y);
                int notesWidth = rightMargin - valX;
                int notesHeight = (int)g.MeasureString(_kitchenOrder.AdminNotes, font, notesWidth).Height;
                var rect = new RectangleF(valX, y, notesWidth, notesHeight + 5);
                g.DrawString(_kitchenOrder.AdminNotes, font, Brushes.Black, rect);
                y += Math.Max(18, notesHeight + 5);
            }

            // Thin separating line
            using (var pen = new Pen(Color.Black, 1))
            {
                g.DrawLine(pen, 10, y, rightMargin, y);
            }
            y += 10;

            // Items columns
            g.DrawString("Item", boldFont, Brushes.Black, 10, y);
            string qtyHeader = "Qty";
            g.DrawString(qtyHeader, boldFont, Brushes.Black, rightMargin - g.MeasureString(qtyHeader, boldFont).Width, y);
            y += 18;

            using (var pen = new Pen(Color.Black, 1))
            {
                g.DrawLine(pen, 10, y, rightMargin, y);
            }
            y += 8;

            // Items list
            foreach (var item in _kitchenOrder.Items)
            {
                string itemName = item.ProductName;
                if (!string.IsNullOrEmpty(item.VariationName))
                {
                    itemName += $" ({item.VariationName})";
                }

                int nameWidth = rightMargin - 45;
                int nameH = (int)g.MeasureString(itemName, font, nameWidth).Height;
                var nameRect = new RectangleF(10, y, nameWidth, nameH + 5);
                g.DrawString(itemName, font, Brushes.Black, nameRect);

                string qStr = item.Quantity.ToString();
                g.DrawString(qStr, font, Brushes.Black, rightMargin - g.MeasureString(qStr, font).Width, y);

                y += Math.Max(nameH, 16) + 6;
            }

            y += 5;
            // Thin separating line
            using (var pen = new Pen(Color.Black, 1))
            {
                g.DrawLine(pen, 10, y, rightMargin, y);
            }
            y += 15;

            string prepareStr = "*** PREPARE ORDER ***";
            float prepX = (width - g.MeasureString(prepareStr, boldFont).Width) / 2;
            g.DrawString(prepareStr, boldFont, Brushes.Black, prepX, y);
            y += 25;

            finalY = y;
        }

        public Bitmap GenerateKitchenSlipBitmap(Order order)
        {
            _kitchenOrder = order;
            int bmpWidth = PrintWidth;
            using (var tempBmp = new Bitmap(bmpWidth, 2000))
            {
                using (var g = Graphics.FromImage(tempBmp))
                {
                    g.Clear(Color.White);
                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    DrawKitchenSlip(g, out int finalY);

                    int height = Math.Min(2000, finalY + 40);
                    var cropped = new Bitmap(bmpWidth, height);
                    using (var destG = Graphics.FromImage(cropped))
                    {
                        destG.Clear(Color.White);
                        destG.DrawImage(tempBmp, new Rectangle(0, 0, bmpWidth, height), new Rectangle(0, 0, bmpWidth, height), GraphicsUnit.Pixel);
                    }
                    return cropped;
                }
            }
        }
    }
}