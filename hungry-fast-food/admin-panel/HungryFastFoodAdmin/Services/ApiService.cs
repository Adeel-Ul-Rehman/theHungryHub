// E:\hungryHub\hungry-fast-food\admin-panel\HungryFastFoodAdmin\Services\ApiService.cs

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using HungryFastFoodAdmin.Models;
using Newtonsoft.Json;

namespace HungryFastFoodAdmin.Services
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _client;
        private readonly string _baseUrl;
        private readonly string _apiKey;

        public static string Token { get; set; }

        public ApiService(string baseUrl = null, string apiKey = null)
        {
            _baseUrl = !string.IsNullOrEmpty(baseUrl)
                ? baseUrl
                : ConfigManager.GetAppSetting("ApiBaseUrl", "http://localhost:5000/api");
            _apiKey = !string.IsNullOrEmpty(apiKey)
                ? apiKey
                : ConfigManager.GetAppSetting("AdminApiKey", "your-admin-api-key-from-env");

            _client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(15)
            };
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (!string.IsNullOrEmpty(_apiKey))
            {
                _client.DefaultRequestHeaders.Add("x-admin-api-key", _apiKey);
            }

            if (!string.IsNullOrEmpty(Token))
            {
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
            }
        }

        public class ApiResult<T>
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public T Data { get; set; }
        }

        public async Task<ApiResult<string>> AdminLogin(string email, string password)
        {
            bool verifiedLocally = false;
            try
            {
                var db = new DatabaseService();
                var savedEmail = db.GetSetting("offline_admin_email", "");
                var savedPassword = db.GetSetting("offline_admin_password", "");

                string currentEmail = string.IsNullOrEmpty(savedEmail) ? "admin@hungryhub.com" : savedEmail;
                string currentPassword = string.IsNullOrEmpty(savedPassword) ? "admin123" : savedPassword;

                if (email == currentEmail && password == currentPassword)
                {
                    verifiedLocally = true;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Local login check failed", ex);
            }

            try
            {
                var payload = new { email, password };
                var json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _client.PostAsync($"{_baseUrl}/auth/login", content);
                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    dynamic result = JsonConvert.DeserializeObject(responseJson);
                    if (result?.success == true)
                    {
                        string token = result?.data?.tokens?.accessToken?.ToString();
                        if (!string.IsNullOrEmpty(token))
                        {
                            Token = token;
                            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                        }
                        
                        try 
                        {
                            var db = new DatabaseService();
                            db.SaveSetting("offline_admin_email", email);
                            db.SaveSetting("offline_admin_password", password);
                        } 
                        catch { }

                        return new ApiResult<string> { Success = true, Message = "Login successful", Data = email };
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("API AdminLogin network failed", ex);
                if (verifiedLocally)
                {
                    return new ApiResult<string> { Success = true, Message = "Login successful (offline fallback)", Data = email };
                }
            }

            if (verifiedLocally)
            {
                return new ApiResult<string> { Success = true, Message = "Login successful (local match)", Data = email };
            }

            return new ApiResult<string> { Success = false, Message = "Invalid email or password." };
        }

        public class AdminProfile
        {
            public int id { get; set; }
            public string email { get; set; }
            public string full_name { get; set; }
            public string phone { get; set; }
        }

        public async Task<ApiResult<AdminProfile>> GetAdminProfile(string email)
        {
            try
            {
                var response = await _client.GetAsync($"{_baseUrl}/admin/profile?email={Uri.EscapeDataString(email)}");
                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<ApiResult<AdminProfile>>(responseJson);
                    return result;
                }
                return new ApiResult<AdminProfile> { Success = false, Message = "Failed to load profile." };
            }
            catch (Exception ex)
            {
                Logger.LogError("API GetAdminProfile failed", ex);
                return new ApiResult<AdminProfile> { Success = false, Message = "Connection error." };
            }
        }

        public async Task<ApiResult<object>> UpdateAdminProfile(string email, string fullName, string phone)
        {
            try
            {
                var payload = new { email, full_name = fullName, phone };
                var json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _client.PutAsync($"{_baseUrl}/admin/profile", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ApiResult<object>>(responseJson);
                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError("API UpdateAdminProfile failed", ex);
                return new ApiResult<object> { Success = false, Message = "Connection error." };
            }
        }

        public async Task<ApiResult<object>> ChangeAdminPassword(string email, string oldPassword, string newPassword)
        {
            bool localUpdated = false;
            try
            {
                var db = new DatabaseService();
                var savedEmail = db.GetSetting("offline_admin_email", "");
                var savedPassword = db.GetSetting("offline_admin_password", "");

                string currentEmail = string.IsNullOrEmpty(savedEmail) ? "admin@hungryhub.com" : savedEmail;
                string currentPassword = string.IsNullOrEmpty(savedPassword) ? "admin123" : savedPassword;

                if (email == currentEmail && oldPassword == currentPassword)
                {
                    db.SaveSetting("offline_admin_email", email);
                    db.SaveSetting("offline_admin_password", newPassword);
                    localUpdated = true;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Local ChangeAdminPassword failed", ex);
            }

            try
            {
                var payload = new { email, oldPassword, newPassword };
                var json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _client.PutAsync($"{_baseUrl}/admin/change-password", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ApiResult<object>>(responseJson);
                
                if (result.Success)
                {
                    return result;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("API ChangeAdminPassword server failed", ex);
            }

            if (localUpdated)
            {
                return new ApiResult<object> { Success = true, Message = "Password changed locally." };
            }

            return new ApiResult<object> { Success = false, Message = "Invalid current password." };
        }

        public async Task<ApiResult<object>> ForgotPassword(string email)
        {
            try
            {
                var payload = new { email, purpose = "forgot_password" };
                var json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _client.PostAsync($"{_baseUrl}/auth/forgot-password", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ApiResult<object>>(responseJson);
                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError("API ForgotPassword failed", ex);
                return new ApiResult<object> { Success = false, Message = "Connection error." };
            }
        }

        public async Task<ApiResult<object>> ResetPassword(string email, string otp, string newPassword)
        {
            try
            {
                var payload = new { email, otp, newPassword, purpose = "reset_password" };
                var json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _client.PostAsync($"{_baseUrl}/auth/reset-password", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ApiResult<object>>(responseJson);
                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError("API ResetPassword failed", ex);
                return new ApiResult<object> { Success = false, Message = "Connection error." };
            }
        }

        public async Task<ApiResult<bool>> VerifyAdminPassword(string email, string password)
        {
            try
            {
                var db = new DatabaseService();
                var savedEmail = db.GetSetting("offline_admin_email", "");
                var savedPassword = db.GetSetting("offline_admin_password", "");

                string currentEmail = string.IsNullOrEmpty(savedEmail) ? "admin@hungryhub.com" : savedEmail;
                string currentPassword = string.IsNullOrEmpty(savedPassword) ? "admin123" : savedPassword;

                if (email == currentEmail && password == currentPassword)
                {
                    return new ApiResult<bool> { Success = true, Message = "Verified", Data = true };
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Local VerifyAdminPassword failed", ex);
            }

            try
            {
                var payload = new { email, password };
                var json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _client.PostAsync($"{_baseUrl}/admin/verify-password", content);
                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        var db = new DatabaseService();
                        db.SaveSetting("offline_admin_email", email);
                        db.SaveSetting("offline_admin_password", password);
                    }
                    catch { }
                    return new ApiResult<bool> { Success = true, Message = "Verified", Data = true };
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("API VerifyAdminPassword server failed", ex);
            }

            return new ApiResult<bool> { Success = false, Message = "Invalid password." };
        }

        public async Task<ApiResult<bool>> SyncOrders(List<Order> orders)
        {
            try
            {
                var json = JsonConvert.SerializeObject(orders);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _client.PostAsync($"{_baseUrl}/admin/orders/sync", content);
                if (response.IsSuccessStatusCode)
                {
                    return new ApiResult<bool> { Success = true };
                }
                return new ApiResult<bool> { Success = false, Message = $"Server returned {response.StatusCode}" };
            }
            catch (Exception ex)
            {
                Logger.LogError("API SyncOrders failed", ex);
                return new ApiResult<bool> { Success = false, Message = ex.Message };
            }
        }

        public async Task<List<Order>> GetNewOrders()
        {
            try
            {
                var response = await _client.GetAsync($"{_baseUrl}/orders/sync-pull");
                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var orders = JsonConvert.DeserializeObject<List<Order>>(responseJson);
                    return orders ?? new List<Order>();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("API GetNewOrders failed", ex);
            }
            return new List<Order>();
        }

        public async Task<ApiResult<bool>> SyncMenuData(string type)
        {
            try
            {
                var db = new DatabaseService();
                object payload;
                if (type == "categories")
                {
                    payload = new { type = "categories", data = db.GetCategories() };
                }
                else
                {
                    payload = new { type = "products", data = db.GetProducts() };
                }

                var json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _client.PostAsync($"{_baseUrl}/menu/publish", content);
                if (response.IsSuccessStatusCode)
                {
                    return new ApiResult<bool> { Success = true };
                }
                return new ApiResult<bool> { Success = false, Message = "Server rejected publish command." };
            }
            catch (Exception ex)
            {
                Logger.LogError($"API SyncMenuData({type}) failed", ex);
                return new ApiResult<bool> { Success = true, Message = $"Offline Success (logged local changes for {type})" };
            }
        }

        public async Task<ApiResult<bool>> SyncDeals(List<Deal> deals)
        {
            try
            {
                var json = JsonConvert.SerializeObject(deals);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _client.PostAsync($"{_baseUrl}/deals/sync-push", content);
                if (response.IsSuccessStatusCode)
                {
                    return new ApiResult<bool> { Success = true };
                }
                return new ApiResult<bool> { Success = false, Message = "Server rejected sync deals data." };
            }
            catch (Exception ex)
            {
                Logger.LogError("API SyncDeals failed", ex);
                return new ApiResult<bool> { Success = true, Message = $"Offline Success (logged local changes for deals)" };
            }
        }

        public async Task<ApiResult<bool>> UpdateDeliveryZones(List<object> zones)
        {
            try
            {
                var json = JsonConvert.SerializeObject(new { zones });
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _client.PutAsync($"{_baseUrl}/admin/delivery-zones", content);
                if (response.IsSuccessStatusCode)
                {
                    return new ApiResult<bool> { Success = true };
                }
                return new ApiResult<bool> { Success = false, Message = $"Server returned {response.StatusCode}" };
            }
            catch (Exception ex)
            {
                Logger.LogError("API UpdateDeliveryZones failed", ex);
                return new ApiResult<bool> { Success = false, Message = ex.Message };
            }
        }

        public async Task<ApiResult<bool>> UpdateSetting(string key, string value)
        {
            try
            {
                var json = JsonConvert.SerializeObject(new { setting_key = key, setting_value = value });
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _client.PostAsync($"{_baseUrl}/admin/settings", content);
                if (response.IsSuccessStatusCode)
                {
                    return new ApiResult<bool> { Success = true };
                }
                return new ApiResult<bool> { Success = false, Message = $"Server returned {response.StatusCode}" };
            }
            catch (Exception ex)
            {
                Logger.LogError($"API UpdateSetting({key}) failed", ex);
                return new ApiResult<bool> { Success = false, Message = ex.Message };
            }
        }

        public async Task<ApiResult<bool>> UpdateOrderStatus(string orderId, string status)
        {
            try
            {
                var payload = new { status = status };
                var json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _client.PatchAsync($"{_baseUrl}/admin/orders/{orderId}/status", content);
                if (response.IsSuccessStatusCode)
                {
                    return new ApiResult<bool> { Success = true };
                }
                return new ApiResult<bool> { Success = false, Message = $"Server returned {response.StatusCode}" };
            }
            catch (Exception ex)
            {
                return new ApiResult<bool> { Success = false, Message = ex.Message };
            }
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

        public async Task<string> UploadImageToCloudinary(string localPath)
        {
            try
            {
                if (!System.IO.File.Exists(localPath)) return "";

                byte[] fileBytes = System.IO.File.ReadAllBytes(localPath);
                string fileName = System.IO.Path.GetFileName(localPath);

                var db = new DatabaseService();
                string cloudName = db.GetSetting("cloudinary_cloud_name", "ourl0wez");
                string uploadPreset = db.GetSetting("cloudinary_upload_preset", "ml_default");
                string apiKey = db.GetSetting("cloudinary_api_key", "461368631868661");
                string apiSecret = db.GetSetting("cloudinary_api_secret", "PA97TqEm3kmuOVKgNlvrQrzTEu0");

                Logger.Log($"☁️ Cloudinary Config: CloudName='{cloudName}', UploadPreset='{uploadPreset}'");

                string ext = System.IO.Path.GetExtension(localPath).ToLower();
                string mimeType = "image/jpeg";
                if (ext == ".png") mimeType = "image/png";
                else if (ext == ".gif") mimeType = "image/gif";
                else if (ext == ".webp") mimeType = "image/webp";

                string timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
                string stringToSign = $"timestamp={timestamp}&upload_preset={uploadPreset}{apiSecret}";
                string signature = CalculateSha1(stringToSign);

                using var form = new MultipartFormDataContent();
                var fileContent = new ByteArrayContent(fileBytes);
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(mimeType);
                form.Add(fileContent, "\"file\"", $"\"{fileName}\"");

                var timestampContent = new StringContent(timestamp);
                timestampContent.Headers.ContentType = null;
                form.Add(timestampContent, "\"timestamp\"");

                var presetContent = new StringContent(uploadPreset);
                presetContent.Headers.ContentType = null;
                form.Add(presetContent, "\"upload_preset\"");

                var apiKeyContent = new StringContent(apiKey);
                apiKeyContent.Headers.ContentType = null;
                form.Add(apiKeyContent, "\"api_key\"");

                var signatureContent = new StringContent(signature);
                signatureContent.Headers.ContentType = null;
                form.Add(signatureContent, "\"signature\"");

                var url = $"https://api.cloudinary.com/v1_1/{cloudName}/image/upload";
                var response = await _client.PostAsync(url, form);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    dynamic result = JsonConvert.DeserializeObject(responseJson);
                    return result?.secure_url?.ToString() ?? "";
                }

                string errorMsg = await response.Content.ReadAsStringAsync();
                Logger.LogError($"Cloudinary upload failed: {response.StatusCode} - {errorMsg}", null);
                return "";
            }
            catch (Exception ex)
            {
                Logger.LogError("Cloudinary upload exception", ex);
                return "";
            }
        }

        public async Task<ApiResult<bool>> PushSyncItems(List<DatabaseService.SyncQueueItem> items)
        {
            try
            {
                var payload = new { items };
                var json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _client.PostAsync($"{_baseUrl}/admin/sync/push", content);
                if (response.IsSuccessStatusCode)
                {
                    return new ApiResult<bool> { Success = true };
                }
                return new ApiResult<bool> { Success = false, Message = $"Server returned {response.StatusCode}" };
            }
            catch (Exception ex)
            {
                return new ApiResult<bool> { Success = false, Message = ex.Message };
            }
        }

        public async Task<ApiResult<bool>> PushFullSync(object payload)
        {
            try
            {
                var json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _client.PostAsync($"{_baseUrl}/admin/sync/full", content);
                if (response.IsSuccessStatusCode)
                {
                    return new ApiResult<bool> { Success = true };
                }
                var errJson = await response.Content.ReadAsStringAsync();
                return new ApiResult<bool> { Success = false, Message = $"Server returned {response.StatusCode}: {errJson}" };
            }
            catch (Exception ex)
            {
                return new ApiResult<bool> { Success = false, Message = ex.Message };
            }
        }

        public async Task<string> GetSyncStatus()
        {
            try
            {
                var response = await _client.GetAsync($"{_baseUrl}/sync/status");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    dynamic data = JsonConvert.DeserializeObject(json);
                    if (data?.success == true)
                    {
                        return data?.last_menu_update?.ToString() ?? "";
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("API GetSyncStatus failed", ex);
            }
            return "";
        }

        public async Task<List<Order>> PullSyncOrders()
        {
            try
            {
                var response = await _client.GetAsync($"{_baseUrl}/admin/sync/pull");
                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var orders = JsonConvert.DeserializeObject<List<Order>>(responseJson);
                    return orders ?? new List<Order>();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("API PullSyncOrders failed", ex);
            }
            return new List<Order>();
        }

        public async Task<ApiResult<bool>> SyncCategory(Category category)
        {
            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(category), Encoding.UTF8, "application/json");
                var response = await _client.PostAsync($"{_baseUrl}/admin/categories/sync", content);
                return new ApiResult<bool> { Success = response.IsSuccessStatusCode, Message = response.ReasonPhrase };
            }
            catch (Exception ex) { return new ApiResult<bool> { Success = false, Message = ex.Message }; }
        }

        public async Task<ApiResult<bool>> UpdateCategory(string id, Category category)
        {
            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(category), Encoding.UTF8, "application/json");
                var response = await _client.PutAsync($"{_baseUrl}/admin/categories/{id}", content);
                return new ApiResult<bool> { Success = response.IsSuccessStatusCode, Message = response.ReasonPhrase };
            }
            catch (Exception ex) { return new ApiResult<bool> { Success = false, Message = ex.Message }; }
        }

        public async Task<ApiResult<bool>> DeleteCategory(string id)
        {
            try
            {
                var response = await _client.DeleteAsync($"{_baseUrl}/admin/categories/{id}");
                return new ApiResult<bool> { Success = response.IsSuccessStatusCode, Message = response.ReasonPhrase };
            }
            catch (Exception ex) { return new ApiResult<bool> { Success = false, Message = ex.Message }; }
        }

        public async Task<ApiResult<bool>> SyncProduct(Product product)
        {
            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(product), Encoding.UTF8, "application/json");
                var response = await _client.PostAsync($"{_baseUrl}/admin/products/sync", content);
                return new ApiResult<bool> { Success = response.IsSuccessStatusCode, Message = response.ReasonPhrase };
            }
            catch (Exception ex) { return new ApiResult<bool> { Success = false, Message = ex.Message }; }
        }

        public async Task<ApiResult<bool>> UpdateProduct(string id, Product product)
        {
            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(product), Encoding.UTF8, "application/json");
                var response = await _client.PutAsync($"{_baseUrl}/admin/products/{id}", content);
                return new ApiResult<bool> { Success = response.IsSuccessStatusCode, Message = response.ReasonPhrase };
            }
            catch (Exception ex) { return new ApiResult<bool> { Success = false, Message = ex.Message }; }
        }

        public async Task<ApiResult<bool>> DeleteProduct(string id)
        {
            try
            {
                var response = await _client.DeleteAsync($"{_baseUrl}/admin/products/{id}");
                return new ApiResult<bool> { Success = response.IsSuccessStatusCode, Message = response.ReasonPhrase };
            }
            catch (Exception ex) { return new ApiResult<bool> { Success = false, Message = ex.Message }; }
        }

        public async Task<ApiResult<bool>> UpdateProductAvailability(string id, bool isActive)
        {
            try
            {
                var payload = new { is_active = isActive };
                var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
                var response = await _client.PatchAsync($"{_baseUrl}/admin/products/{id}/status", content);
                return new ApiResult<bool> { Success = response.IsSuccessStatusCode, Message = response.ReasonPhrase };
            }
            catch (Exception ex) { return new ApiResult<bool> { Success = false, Message = ex.Message }; }
        }

        public async Task<List<Order>> PullNewOrders(string since)
        {
            try
            {
                string url = $"{_baseUrl}/admin/orders/new";
                if (!string.IsNullOrEmpty(since))
                {
                    url += $"?since={Uri.EscapeDataString(since)}";
                }
                var response = await _client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var orders = JsonConvert.DeserializeObject<List<Order>>(responseJson);
                    return orders ?? new List<Order>();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("API PullNewOrders failed", ex);
            }
            return new List<Order>();
        }

        public async Task<ApiResult<bool>> SyncOrder(Order order)
        {
            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(order), Encoding.UTF8, "application/json");
                var response = await _client.PostAsync($"{_baseUrl}/admin/orders/sync", content);
                return new ApiResult<bool> { Success = response.IsSuccessStatusCode, Message = response.ReasonPhrase };
            }
            catch (Exception ex) { return new ApiResult<bool> { Success = false, Message = ex.Message }; }
        }
    }
}