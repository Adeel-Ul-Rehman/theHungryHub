// E:\hungryHub\hungry-fast-food\admin-panel\HungryFastFoodAdmin\Services\CloudinaryService.cs

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HungryFastFoodAdmin.Services
{
    public class UploadResult
    {
        [JsonProperty("secure_url")]
        public string SecureUrl { get; set; }

        [JsonProperty("public_id")]
        public string PublicId { get; set; }

        [JsonProperty("error")]
        public UploadError Error { get; set; }

        public bool Success => Error == null && !string.IsNullOrEmpty(SecureUrl);
    }

    public class UploadError
    {
        [JsonProperty("message")]
        public string Message { get; set; }
    }

    public class CloudinaryService
    {
        private readonly DatabaseService _db;
        private readonly HttpClient _client;

        // Progress reporting event
        public event Action<long, long> UploadProgress; // (bytesSent, totalBytes)

        public CloudinaryService()
        {
            _db = new DatabaseService();
            _client = new HttpClient { Timeout = TimeSpan.FromSeconds(120) };
        }

        private string CalculateSha1(string input)
        {
            using var sha1 = System.Security.Cryptography.SHA1.Create();
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = sha1.ComputeHash(inputBytes);
            var sb = new StringBuilder();
            foreach (byte b in hashBytes)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }

        // Resolves configuration setting with fallback
        private string GetConfigSetting(string appConfigKey, string dbConfigKey, string defaultValue)
        {
            string val = ConfigManager.GetAppSetting(appConfigKey, "");
            if (string.IsNullOrEmpty(val))
            {
                val = _db.GetSetting(dbConfigKey, defaultValue);
            }
            return val;
        }

        // Checks if file is valid image (jpeg, png, gif, webp) and under 5MB limit
        public bool ValidateImage(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                return false;
            }

            var info = new FileInfo(filePath);
            if (info.Length > 5 * 1024 * 1024) // 5MB
            {
                Logger.Log($"⚠️ File size exceeds 5MB limit: {info.Length} bytes ({filePath})");
                return false;
            }

            string ext = Path.GetExtension(filePath).ToLower();
            return ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".gif" || ext == ".webp";
        }

        // Uploads image to Cloudinary from local file path with folder structure
        public async Task<UploadResult> UploadImage(string filePath, string folder)
        {
            var uploadResult = new UploadResult();
            if (!ValidateImage(filePath))
            {
                uploadResult.Error = new UploadError { Message = "Invalid image file or file size exceeds 5MB limit." };
                return uploadResult;
            }

            int attempts = 3;
            int delayMs = 1000;

            for (int attempt = 1; attempt <= attempts; attempt++)
            {
                try
                {
                    string cloudName = GetConfigSetting("CloudName", "cloudinary_cloud_name", "ourl0wez");
                    string uploadPreset = GetConfigSetting("UploadPreset", "cloudinary_upload_preset", "ml_default");
                    string apiKey = GetConfigSetting("ApiKey", "cloudinary_api_key", "461368631868661");
                    string apiSecret = GetConfigSetting("ApiSecret", "cloudinary_api_secret", "PA97TqEm3kmuOVKgNlvrQrzTEu0");

                    Logger.Log($"☁️ [Cloudinary Debug] CloudName: '{cloudName}', UploadPreset: '{uploadPreset}', ApiKey: '{apiKey}', ApiSecret: '{apiSecret}'");

                    string timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
                    
                    var paramsToSign = new SortedDictionary<string, string>
                    {
                        { "timestamp", timestamp }
                    };
                    if (!string.IsNullOrEmpty(folder))
                    {
                        paramsToSign.Add("folder", folder);
                    }

                    var sb = new StringBuilder();
                    foreach (var kv in paramsToSign)
                    {
                        if (sb.Length > 0) sb.Append("&");
                        sb.Append($"{kv.Key}={kv.Value}");
                    }
                    sb.Append(apiSecret);
                    string signature = CalculateSha1(sb.ToString());
                    Logger.Log($"☁️ [Cloudinary Debug] String to Sign: '{sb}', Signature: '{signature}'");

                    string ext = Path.GetExtension(filePath).ToLower();
                    string mimeType = ext switch
                    {
                        ".png" => "image/png",
                        ".gif" => "image/gif",
                        ".webp" => "image/webp",
                        _ => "image/jpeg"
                    };

                    using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    using var form = new MultipartFormDataContent();

                    var progressContent = new ProgressStreamContent(fileStream, (sent, total) =>
                    {
                        UploadProgress?.Invoke(sent, total);
                    });
                    progressContent.Headers.ContentType = MediaTypeHeaderValue.Parse(mimeType);
                    progressContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                    {
                        Name = "\"file\"",
                        FileName = $"\"{Path.GetFileName(filePath)}\""
                    };
                    form.Add(progressContent);

                    AddFormParam(form, "timestamp", timestamp);
                    AddFormParam(form, "api_key", apiKey);
                    AddFormParam(form, "signature", signature);
                    if (!string.IsNullOrEmpty(folder))
                    {
                        AddFormParam(form, "folder", folder);
                    }

                    string url = $"https://api.cloudinary.com/v1_1/{cloudName}/image/upload";
                    var response = await _client.PostAsync(url, form);

                    var responseJson = await response.Content.ReadAsStringAsync();
                    if (response.IsSuccessStatusCode)
                    {
                        return JsonConvert.DeserializeObject<UploadResult>(responseJson);
                    }
                    else
                    {
                        dynamic errObj = JsonConvert.DeserializeObject(responseJson);
                        string msg = errObj?.error?.message?.ToString() ?? "Unknown Cloudinary API error.";
                        uploadResult.Error = new UploadError { Message = msg };
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError($"UploadImage attempt {attempt} failed: {ex.Message}", ex);
                    uploadResult.Error = new UploadError { Message = ex.Message };
                }

                if (attempt < attempts)
                {
                    Logger.Log($"⏳ [Cloudinary Upload] Retrying in {delayMs}ms...");
                    await Task.Delay(delayMs);
                    delayMs *= 2; // exponential backoff
                }
            }

            return uploadResult;
        }

        // Uploads image from byte array (for in-memory images)
        public async Task<UploadResult> UploadImageFromByteArray(byte[] byteArray, string fileName, string folder)
        {
            var uploadResult = new UploadResult();
            if (byteArray == null || byteArray.Length == 0 || byteArray.Length > 5 * 1024 * 1024)
            {
                uploadResult.Error = new UploadError { Message = "Invalid byte array or size exceeds 5MB limit." };
                return uploadResult;
            }

            int attempts = 3;
            int delayMs = 1000;

            for (int attempt = 1; attempt <= attempts; attempt++)
            {
                try
                {
                    string cloudName = GetConfigSetting("CloudName", "cloudinary_cloud_name", "ourl0wez");
                    string uploadPreset = GetConfigSetting("UploadPreset", "cloudinary_upload_preset", "ml_default");
                    string apiKey = GetConfigSetting("ApiKey", "cloudinary_api_key", "461368631868661");
                    string apiSecret = GetConfigSetting("ApiSecret", "cloudinary_api_secret", "PA97TqEm3kmuOVKgNlvrQrzTEu0");

                    string timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
                    
                    var paramsToSign = new SortedDictionary<string, string>
                    {
                        { "timestamp", timestamp }
                    };
                    if (!string.IsNullOrEmpty(folder))
                    {
                        paramsToSign.Add("folder", folder);
                    }

                    var sb = new StringBuilder();
                    foreach (var kv in paramsToSign)
                    {
                        if (sb.Length > 0) sb.Append("&");
                        sb.Append($"{kv.Key}={kv.Value}");
                    }
                    sb.Append(apiSecret);
                    string signature = CalculateSha1(sb.ToString());

                    string ext = Path.GetExtension(fileName ?? "").ToLower();
                    string mimeType = ext switch
                    {
                        ".png" => "image/png",
                        ".gif" => "image/gif",
                        ".webp" => "image/webp",
                        _ => "image/jpeg"
                    };

                    using var memoryStream = new MemoryStream(byteArray);
                    using var form = new MultipartFormDataContent();

                    var progressContent = new ProgressStreamContent(memoryStream, (sent, total) =>
                    {
                        UploadProgress?.Invoke(sent, total);
                    });
                    progressContent.Headers.ContentType = MediaTypeHeaderValue.Parse(mimeType);
                    progressContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                    {
                        Name = "\"file\"",
                        FileName = $"\"{fileName ?? "file.jpg"}\""
                    };
                    form.Add(progressContent);

                    AddFormParam(form, "timestamp", timestamp);
                    AddFormParam(form, "api_key", apiKey);
                    AddFormParam(form, "signature", signature);
                    if (!string.IsNullOrEmpty(folder))
                    {
                        AddFormParam(form, "folder", folder);
                    }

                    string url = $"https://api.cloudinary.com/v1_1/{cloudName}/image/upload";
                    var response = await _client.PostAsync(url, form);

                    var responseJson = await response.Content.ReadAsStringAsync();
                    if (response.IsSuccessStatusCode)
                    {
                        return JsonConvert.DeserializeObject<UploadResult>(responseJson);
                    }
                    else
                    {
                        dynamic errObj = JsonConvert.DeserializeObject(responseJson);
                        string msg = errObj?.error?.message?.ToString() ?? "Unknown Cloudinary API error.";
                        uploadResult.Error = new UploadError { Message = msg };
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError($"UploadImageFromByteArray attempt {attempt} failed: {ex.Message}", ex);
                    uploadResult.Error = new UploadError { Message = ex.Message };
                }

                if (attempt < attempts)
                {
                    Logger.Log($"⏳ [Cloudinary Upload] Retrying in {delayMs}ms...");
                    await Task.Delay(delayMs);
                    delayMs *= 2;
                }
            }

            return uploadResult;
        }

        // Deletes image from Cloudinary
        public async Task<bool> DeleteImage(string publicId)
        {
            if (string.IsNullOrEmpty(publicId)) return false;

            int attempts = 3;
            int delayMs = 1000;

            for (int attempt = 1; attempt <= attempts; attempt++)
            {
                try
                {
                    string cloudName = GetConfigSetting("CloudName", "cloudinary_cloud_name", "ourl0wez");
                    string apiKey = GetConfigSetting("ApiKey", "cloudinary_api_key", "461368631868661");
                    string apiSecret = GetConfigSetting("ApiSecret", "cloudinary_api_secret", "PA97TqEm3kmuOVKgNlvrQrzTEu0");

                    string timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
                    string stringToSign = $"public_id={publicId}&timestamp={timestamp}{apiSecret}";
                    string signature = CalculateSha1(stringToSign);

                    using var form = new MultipartFormDataContent();
                    form.Add(new StringContent(publicId), "public_id");
                    form.Add(new StringContent(timestamp), "timestamp");
                    form.Add(new StringContent(apiKey), "api_key");
                    form.Add(new StringContent(signature), "signature");

                    string url = $"https://api.cloudinary.com/v1_1/{cloudName}/image/destroy";
                    var response = await _client.PostAsync(url, form);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseJson = await response.Content.ReadAsStringAsync();
                        dynamic result = JsonConvert.DeserializeObject(responseJson);
                        return result?.result?.ToString() == "ok";
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError($"DeleteImage attempt {attempt} failed", ex);
                }

                if (attempt < attempts)
                {
                    await Task.Delay(delayMs);
                    delayMs *= 2;
                }
            }
            return false;
        }

        // Generates URL for existing image publicId
        public string GetImageUrl(string publicId)
        {
            if (string.IsNullOrEmpty(publicId)) return "";
            string cloudName = GetConfigSetting("CloudName", "cloudinary_cloud_name", "ourl0wez");
            return $"https://res.cloudinary.com/{cloudName}/image/upload/{publicId}";
        }

        // Backward compatibility method helper
        public virtual async Task<string> UploadImageAsync(string localPath)
        {
            var res = await UploadImage(localPath, "hungryhub/products");
            return res.Success ? res.SecureUrl : "";
        }

        private HttpContent CreateFormParam(string value)
        {
            var content = new StringContent(value, Encoding.UTF8);
            content.Headers.Remove("Content-Type");
            return content;
        }

        private void AddFormParam(MultipartFormDataContent form, string name, string value)
        {
            var content = new ByteArrayContent(Encoding.UTF8.GetBytes(value));
            content.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = $"\"{name}\""
            };
            form.Add(content);
        }

        // Custom Stream content class to calculate and fire progress
        private class ProgressStreamContent : HttpContent
        {
            private readonly Stream _stream;
            private readonly int _bufferSize;
            private readonly Action<long, long> _progress;

            public ProgressStreamContent(Stream stream, Action<long, long> progress, int bufferSize = 4096)
            {
                _stream = stream;
                _progress = progress;
                _bufferSize = bufferSize;
            }

            protected override async Task SerializeToStreamAsync(Stream stream, System.Net.TransportContext context)
            {
                var buffer = new byte[_bufferSize];
                long totalBytes = _stream.Length;
                long sentBytes = 0;
                int bytesRead;

                while ((bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await stream.WriteAsync(buffer, 0, bytesRead);
                    sentBytes += bytesRead;
                    _progress?.Invoke(sentBytes, totalBytes);
                }
            }

            protected override bool TryComputeLength(out long length)
            {
                length = _stream.Length;
                return true;
            }
        }
    }
}
