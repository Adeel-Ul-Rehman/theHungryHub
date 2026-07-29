// E:\hungryHub\hungry-fast-food\admin-panel\HungryFastFoodAdmin\Services\ImageHelper.cs
using System;
using System.IO;
using System.Drawing;
using System.Net.Http;
using System.Threading.Tasks;

namespace HungryFastFoodAdmin.Services
{
    public static class ImageHelper
    {
        public static string GetLocalImagePath(string pathOrUrl)
        {
            if (string.IsNullOrEmpty(pathOrUrl)) return null;

            // 1. If it's already a local path and exists, use it
            if (File.Exists(pathOrUrl))
            {
                return pathOrUrl;
            }

            // 2. If it's a web URL, check if the file exists in the local bundled images directory
            if (pathOrUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                pathOrUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    string fileName = Path.GetFileName(pathOrUrl);
                    if (!string.IsNullOrEmpty(fileName))
                    {
                        // Check in C:\HungryFastFood\images\
                        string localBundlePath = Path.Combine(@"C:\HungryFastFood\images", fileName);
                        if (File.Exists(localBundlePath))
                        {
                            return localBundlePath;
                        }
                    }
                }
                catch { }
            }

            return null;
        }

        public static Image LoadImage(string pathOrUrl)
        {
            if (string.IsNullOrEmpty(pathOrUrl)) return null;

            // Try resolving to local path first
            string localPath = GetLocalImagePath(pathOrUrl);
            if (!string.IsNullOrEmpty(localPath) && File.Exists(localPath))
            {
                try
                {
                    // Use file stream to avoid locking the image file
                    using (var fs = new FileStream(localPath, FileMode.Open, FileAccess.Read))
                    {
                        return Image.FromStream(fs);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading local image {localPath}: {ex.Message}");
                }
            }

            // If not found locally, fetch over network with bypassed SSL verification
            if (pathOrUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                pathOrUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var handler = new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
                    };
                    using (var client = new HttpClient(handler))
                    {
                        client.Timeout = TimeSpan.FromSeconds(15);
                        var data = client.GetByteArrayAsync(pathOrUrl).GetAwaiter().GetResult();
                        using (var ms = new MemoryStream(data))
                        {
                            return Image.FromStream(ms);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error downloading image {pathOrUrl}: {ex.Message}");
                }
            }

            return null;
        }
    }
}
