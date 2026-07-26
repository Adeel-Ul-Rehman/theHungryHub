// E:\hungryHub\hungry-fast-food\admin-panel\HungryFastFoodAdmin\Services\SyncService.cs
//
// Event-driven sync strategy:
//   - Admin panel changes (add/edit/delete product/category/deal/settings) are pushed
//     immediately when they happen via DatabaseService.AddToSyncQueue → FlushSyncQueueAsync.
//   - The periodic timer ONLY polls for new incoming orders from the website.
//     No full-sync or timestamp comparison runs automatically.
//   - Full sync can still be triggered manually (e.g. "Sync Now" button on startup).

using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HungryFastFoodAdmin.Models;
using Newtonsoft.Json;

namespace HungryFastFoodAdmin.Services
{
    public class SyncService
    {
        // ─── Events ───────────────────────────────────────────────────────────────
        public event Action<bool, string> SyncStatusChanged;   // (isOnline, statusMessage)
        public event Action<List<Order>>  NewOrdersReceived;   // new orders pulled from cloud

        // ─── State ────────────────────────────────────────────────────────────────
        private readonly DatabaseService _db;
        private readonly IApiService     _api;
        private readonly Func<Task<bool>> _isOnlineCheck;
        private System.Threading.Timer   _orderPollTimer;
        private bool   _isSyncing;
        private bool   _lastOnlineState = false;
        private readonly int _orderPollSeconds;
        public bool IsSyncing => _isSyncing;

        // ─── Constructor ──────────────────────────────────────────────────────────
        public SyncService(DatabaseService databaseService = null, IApiService apiService = null, Func<bool> internetCheck = null)
        {
            _db              = databaseService ?? new DatabaseService();
            _api             = apiService ?? new ApiService();
            _isOnlineCheck   = internetCheck != null 
                ? () => Task.FromResult(internetCheck())
                : (Func<Task<bool>>)IsInternetAvailableAsync;
            _orderPollSeconds = Convert.ToInt32(
                ConfigManager.GetAppSetting("SyncIntervalSeconds", "30"));
        }

        // ─── Public API ───────────────────────────────────────────────────────────

        /// <summary>
        /// Starts the lightweight order-polling timer.
        /// This is the ONLY automatic background task. Admin panel changes sync themselves
        /// immediately via DatabaseService.FlushSyncQueueAsync — no timer needed for that.
        /// </summary>
        public void StartAutoSync()
        {
            _orderPollTimer = new System.Threading.Timer(
                async _ => await PollOrdersAsync(),
                null,
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(_orderPollSeconds)
            );
            Logger.Log($"✅ Auto-sync started (every {_orderPollSeconds}s)");
        }

        public void StopAutoSync()
        {
            _orderPollTimer?.Dispose();
            Logger.Log("⏹ Auto-sync stopped");
        }

        /// <summary>Manual trigger: flush any pending queue items AND pull orders.</summary>
        public async Task SyncNow()
        {
            await _db.FlushSyncQueueAsync();
            await PollOrdersAsync();
        }

        // Kept for compatibility — delegates to DatabaseService
        public void AddToSyncQueue(string operationType, string entityId, string entityType, string data)
        {
            _db.AddToSyncQueue(operationType, entityType, entityId, data);
        }

        // Updates product availability locally and immediately syncs to cloud
        public void UpdateProductStatus(string productId, bool isActive)
        {
            try
            {
                string sql = "UPDATE Products SET IsActive = @IsActive WHERE Id = @Id";
                using (var connection = new SQLiteConnection($"Data Source={_db.GetDatabasePath()};Version=3;"))
                {
                    connection.Open();
                    using var cmd = new SQLiteCommand(sql, connection);
                    cmd.Parameters.AddWithValue("@IsActive", isActive ? 1 : 0);
                    cmd.Parameters.AddWithValue("@Id", productId);
                    cmd.ExecuteNonQuery();
                }
                Logger.Log($"💾 Local product availability updated: {productId} → {(isActive ? "Available" : "Unavailable")}");
                _db.AddToSyncQueue("UPDATE", "Products", productId, "");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to update product status for {productId}", ex);
            }
        }

        // Image upload with retry — unchanged
        public async Task<string> UploadImageToCloudinary(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath) ||
                imagePath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                imagePath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return imagePath;
            }

            var cloudinary = CreateCloudinaryService();
            int attempts = 3;
            int delayMs = 1000;

            for (int i = 1; i <= attempts; i++)
            {
                try
                {
                    Logger.Log($"☁️ [Cloudinary] Uploading: {imagePath} (Attempt {i}/{attempts})");
                    string secureUrl = await cloudinary.UploadImageAsync(imagePath);
                    if (!string.IsNullOrEmpty(secureUrl))
                    {
                        Logger.Log($"✅ [Cloudinary] Uploaded: {secureUrl}");
                        return secureUrl;
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError($"❌ [Cloudinary] Attempt {i} failed", ex);
                }
                if (i < attempts)
                {
                    await Task.Delay(delayMs);
                    delayMs *= 2;
                }
            }
            throw new Exception($"Failed to upload image after {attempts} attempts.");
        }

        /// <summary>
        /// Performs a full sync of all local data to the backend.
        /// Called explicitly on startup or via "Sync Now" button — NOT on the auto-timer.
        /// Sanitizes empty-string UUID fields to null before sending.
        /// </summary>
        public async Task PerformFullSyncAsync()
        {
            if (!await _isOnlineCheck())
            {
                Logger.Log("⏳ Skipping Full Sync: No internet connection.");
                return;
            }

            try
            {
                Logger.Log("Performing Full System Sync...");
                var categories = _db.GetCategories();
                var products   = _db.GetProducts();
                var deals      = _db.GetDeals();
                var settings   = _db.GetSystemSettings();

                // Upload any local images first
                foreach (var p in products)
                {
                    if (!string.IsNullOrEmpty(p.ImageUrl) && !p.ImageUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    {
                        string cloudUrl = await UploadImageToCloudinary(p.ImageUrl);
                        if (!string.IsNullOrEmpty(cloudUrl)) { _db.UpdateProductImageUrl(p.Id, cloudUrl); p.ImageUrl = cloudUrl; }
                    }
                }
                foreach (var d in deals)
                {
                    if (!string.IsNullOrEmpty(d.ImageUrl) && !d.ImageUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    {
                        string cloudUrl = await UploadImageToCloudinary(d.ImageUrl);
                        if (!string.IsNullOrEmpty(cloudUrl)) { _db.UpdateDealImageUrl(d.Id, cloudUrl); d.ImageUrl = cloudUrl; }
                    }
                }

                var payload = new
                {
                    categories = categories.Select(c => new
                    {
                        id = c.Id,
                        name = c.Name,
                        slug = c.Slug,
                        display_order = c.DisplayOrder,
                        is_active = c.IsActive
                    }),
                    products = products.Select(p => new
                    {
                        id = p.Id,
                        category_id = p.CategoryId,
                        name = p.Name,
                        slug = p.Slug,
                        description = p.Description,
                        base_price = p.BasePrice,
                        discount_price = p.DiscountPrice,
                        has_variations = p.HasVariations,
                        is_active = p.IsActive,
                        is_deal = p.IsDeal,
                        image_url = p.ImageUrl,
                        display_order = p.DisplayOrder,
                        variations = p.Variations?.Select(v => new
                        {
                            id = v.Id,
                            product_id = v.ProductId,
                            variation_type = v.VariationType,
                            variation_name = v.VariationName,
                            price_adjustment = v.PriceAdjustment,
                            is_default = v.IsDefault
                        })
                    }),
                    deals = deals.Select(d => new
                    {
                        id = d.Id,
                        name = d.Name,
                        slug = d.Slug,
                        description = d.Description,
                        total_price = d.TotalPrice,
                        image_url = d.ImageUrl,
                        is_active = d.IsActive,
                        items = d.Items?.Select(i => new
                        {
                            id = i.Id,
                            deal_id = i.DealId,
                            product_id = i.ProductId,
                            // CRITICAL: send null for empty variation_id — Postgres UUID rejects ""
                            variation_id = string.IsNullOrEmpty(i.VariationId) ? (string)null : i.VariationId,
                            quantity   = i.Quantity,
                            unit_price = i.UnitPrice
                        })
                    }),
                    settings = settings.Select(kvp => new
                    {
                        setting_key   = kvp.Key,
                        setting_value = kvp.Value
                    })
                };

                var result = await _api.PushFullSync(payload);
                if (result.Success)
                    Logger.Log("🎉 Full System Sync completed successfully!");
                else
                    Logger.LogError($"❌ Full System Sync failed: {result.Message}", new Exception("Sync Failed"));
            }
            catch (Exception ex)
            {
                Logger.LogError("Exception in Full System Sync", ex);
            }
        }

        // Kept for compatibility
        public async Task ProcessSyncQueue()
        {
            await _db.FlushSyncQueueAsync();
        }

        // ─── Order Polling ─────────────────────────────────────────────────────────

        /// <summary>
        /// Pulls new orders from the backend API and saves them locally.
        /// Called only by the background timer.
        /// </summary>
        public async Task SyncNewOrders()
        {
            Logger.Log("📥 Checking incremental new online orders from Neon…");
            string since = _db.GetLatestOrderUpdateTimestamp();
            var orders = await _api.PullNewOrders(since);

            if (orders == null || orders.Count == 0)
            {
                Logger.Log("📥 No new incremental orders.");
                return;
            }

            Logger.Log($"📥 {orders.Count} order(s) fetched from Cloud API.");
            var saved = new List<Order>();

            foreach (var order in orders)
            {
                try
                {
                    var existing = _db.GetOrderById(order.Id);
                    if (existing != null)
                    {
                        DateTime existingTime = DateTime.MinValue;
                        DateTime newTime      = DateTime.MinValue;
                        DateTime.TryParse(existing.UpdatedAt, out existingTime);
                        DateTime.TryParse(order.UpdatedAt,   out newTime);

                        if (newTime <= existingTime)
                        {
                            Logger.Log($"⚔️ Conflict: local order #{existing.OrderNumber} is up-to-date. Skipping.");
                            continue;
                        }
                        Logger.Log($"⚔️ Conflict: pulling newer order #{order.OrderNumber} from website.");
                    }

                    if (string.IsNullOrEmpty(order.Id))
                        order.Id = Guid.NewGuid().ToString();

                    var items = order.Items ?? new List<OrderItem>();
                    _db.CreateOrder(order, items);
                    saved.Add(order);
                    Logger.Log($"💾 Saved/Updated online order #{order.OrderNumber} locally.");
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Failed to save online order {order.OrderNumber}", ex);
                }
            }

            if (saved.Count > 0)
                NewOrdersReceived?.Invoke(saved);
        }

        // ─── Private helpers ───────────────────────────────────────────────────────

        /// <summary>Lightweight cycle: check internet then poll for orders only.</summary>
        private async Task PollOrdersAsync()
        {
            if (_isSyncing) return;
            _isSyncing = true;
            SyncStatusChanged?.Invoke(_lastOnlineState, "Syncing");

            try
            {
                bool online = await _isOnlineCheck();
                if (!online)
                {
                    _lastOnlineState = false;
                    SyncStatusChanged?.Invoke(false, "Offline");
                    Logger.Log("⚠️ Order poll skipped – no internet.");
                    return;
                }

                _lastOnlineState = true;
                await SyncNewOrders();
                SyncStatusChanged?.Invoke(true, "Synced");
            }
            catch (Exception ex)
            {
                Logger.LogError("SyncService.PollOrdersAsync error", ex);
                SyncStatusChanged?.Invoke(_lastOnlineState, "Failed");
            }
            finally
            {
                _isSyncing = false;
            }
        }

        private async Task<bool> IsInternetAvailableAsync()
        {
            try
            {
                // Check if the actual backend API is reachable, not google.com
                string baseUrl = ConfigManager.GetAppSetting("ApiBaseUrl", "https://the-hungry-hub-xi.vercel.app/api");
                string healthUrl = baseUrl.TrimEnd('/').Replace("/api", "") + "/health";
                using var client = new System.Net.Http.HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", "HungryFastFoodAdmin-POS");
                client.Timeout = TimeSpan.FromSeconds(8);
                using var response = await client.GetAsync(healthUrl);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Logger.LogError("Internet check failed via backend health, trying fallback...", ex);
                try
                {
                    // Fallback to google generate_204 which is extremely fast and reliable
                    using var client = new System.Net.Http.HttpClient();
                    client.DefaultRequestHeaders.Add("User-Agent", "HungryFastFoodAdmin-POS");
                    client.Timeout = TimeSpan.FromSeconds(5);
                    using var response = await client.GetAsync("http://clients3.google.com/generate_204");
                    return response.IsSuccessStatusCode;
                }
                catch (Exception ex2)
                {
                    Logger.LogError("Internet check fallback failed", ex2);
                    return false;
                }
            }
        }

        protected virtual CloudinaryService CreateCloudinaryService()
        {
            return new CloudinaryService();
        }
    }
}