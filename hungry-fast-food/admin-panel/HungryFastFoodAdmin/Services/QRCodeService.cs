// E:\hungryHub\hungry-fast-food\admin-panel\HungryFastFoodAdmin\Services\QRCodeService.cs

using System;
using System.Drawing;
using QRCoder;

namespace HungryFastFoodAdmin.Services
{
    public class QRCodeService
    {
        public Image GenerateQRCode(string text, int size = 150)
        {
            try
            {
                using var qrGenerator = new QRCodeGenerator();
                using var qrData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
                using var qrCode = new QRCode(qrData);
                var qrImage = qrCode.GetGraphic(20);
                return qrImage;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ QR Code generation error: {ex.Message}");
                return null;
            }
        }

        public string GenerateGoogleMapsLink(string address, double? latitude = null, double? longitude = null)
        {
            if (latitude.HasValue && longitude.HasValue)
            {
                return $"https://www.google.com/maps/search/?api=1&query={latitude.Value},{longitude.Value}";
            }
            return $"https://www.google.com/maps/search/?api=1&query={Uri.EscapeDataString(address)}";
        }
    }
}