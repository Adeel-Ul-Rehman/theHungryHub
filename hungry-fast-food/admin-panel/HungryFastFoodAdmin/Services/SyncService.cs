// E:\hungryHub\hungry-fast-food\admin-panel\HungryFastFoodAdmin\Services\SyncService.cs

using System;
using System.Collections.Generic;
using System.Data.SQLite;
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
        private readonly Func<bool>      _isOnlineCheck;
        private System.Threading.Timer _timer;
        private bool    _isSyncing;
        private bool    _lastOnlineState = false;
        private readonly int _intervalSeconds;
        public bool IsSyncing => _isSyncing;

        // ─── Constructor ──────────────────────────────────────────────────────────
        public SyncService(DatabaseService databaseService = null, IApiService apiService = null, Func<bool> internetCheck = null)
        {
            _db              = databaseService ?? new DatabaseService();
            _api             = apiService ?? new ApiService();
            _isOnlineCheck   = internetCheck ?? IsInternetAvailable;
            _intervalSeconds = Convert.ToInt32(
                ConfigManager.GetAppSetting("SyncIntervalSeconds", "30"));
        }

        // ─── Public API ───────────────────────────────────────────────────────────

        public void StartAutoSync()
        {
            _timer = new System.Threading.Timer(
                async _ => await RunSyncCycle(),
                null,
                TimeSpan.FromSeconds(5),          // first run after 5s
                TimeSpan.FromSeconds(_intervalSeconds)
            );
            Logger.Log($"✅ Auto-sync started (every {_intervalSeconds}s)");
        }

        public void StopAutoSync()
        {
            _timer?.Dispose();
            Logger.Log("⏹ Auto-sync stopped");
        }

        /// <summary>Manual trigger from UI or on-startup.</summary>
        public async Task SyncNow()
        {
            await RunSyncCycle();
        }

        // Adds a sync operation to SyncLogs table
        public void AddToSyncQueue(string operationType, string entityId, string entityType, string data)
        {
            _db.AddSyncLog(operationType, entityId, entityType, data);
        }

        // Updates product availability locally and queues sync operation
        public void UpdateProductStatus(string productId, bool isActive)
        {
            try
            {
                // Update local database first (offline-first)
                string sql = "UPDATE Products SET IsActive = @IsActive WHERE Id = @Id";
                using (var connection = new SQLiteConnection($"Data Source={_db.GetDatabasePath()};Version=3;"))
                {
                    connection.Open();
                    using var cmd = new SQLiteCommand(sql, connection);
                    cmd.Parameters.AddWithValue("@IsActive", isActive ? 1 : 0);
                    cmd.Parameters.AddWithValue("@Id", productId);
                    cmd.ExecuteNonQuery();
                }

                Logger.Log($"💾 Local product availability updated: Product {productId} is {(isActive ? "Available" : "Unavailable")}");

                // Now queue the sync operation (Smart Sync)
                _db.AddToSyncQueue("UPDATE", "Products", productId, "");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to update product status for {productId}", ex);
            }
        }

        // Uploads image to Cloudinary with retry (3 attempts)
        public async Task<string> UploadImageToCloudinary(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath) || 
                imagePath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || 
                imagePath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return imagePath; // Already uploaded or empty URL
            }

            var cloudinary = CreateCloudinaryService();
            int attempts = 3;
            int delayMs = 1000;

            for (int i = 1; i <= attempts; i++)
            {
                try
                {
                    Logger.Log($"☁️ [Cloudinary Upload] Uploading image: {imagePath} (Attempt {i}/{attempts})");
                    string secureUrl = await cloudinary.UploadImageAsync(imagePath);
                    if (!string.IsNullOrEmpty(secureUrl))
                    {
                        Logger.Log($"✅ [Cloudinary Upload] Successfully uploaded to Cloudinary: {secureUrl}");
                        return secureUrl;
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError($"❌ [Cloudinary Upload] Attempt {i} failed: {ex.Message}", ex);
                }

                if (i < attempts)
                {
                    Logger.Log($"⏳ [Cloudinary Upload] Retrying in {delayMs}ms...");
                    await Task.Delay(delayMs);
                    delayMs *= 2; // exponential backoff
                }
            }

            throw new Exception($"Failed to upload image to Cloudinary after {attempts} attempts.");
        }

        // Individual Sync methods (SyncCategory, SyncProduct, etc.) removed to simplify the Smart Sync approach

        public async Task PerformFullSyncAsync()
        {
            if (!_isOnlineCheck())
            {
                Logger.Log("⏳ Skipping Full Sync: No internet connection.");
                return;
            }

            try
            {
                Logger.Log("Performing Full System Sync...");
                var categories = _db.GetCategories();
                var products = _db.GetProducts();
                var deals = _db.GetDeals();
                var settings = _db.GetSystemSettings();

                // Ensure local images are uploaded before syncing payload
                foreach (var p in products)
                {
                    if (!string.IsNullOrEmpty(p.ImageUrl) && !p.ImageUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    {
                        string cloudUrl = await UploadImageToCloudinary(p.ImageUrl);
                        if (!string.IsNullOrEmpty(cloudUrl))
                        {
                            _db.UpdateProductImageUrl(p.Id, cloudUrl);
                            p.ImageUrl = cloudUrl;
                        }
                    }
                }
                
                foreach (var d in deals)
                {
                    if (!string.IsNullOrEmpty(d.ImageUrl) && !d.ImageUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    {
                        string cloudUrl = await UploadImageToCloudinary(d.ImageUrl);
                        if (!string.IsNullOrEmpty(cloudUrl))
                        {
                            _db.UpdateDealImageUrl(d.Id, cloudUrl);
                            d.ImageUrl = cloudUrl;
                        }
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
                            variation_id = i.VariationId,
                            quantity = i.Quantity
                        })
                    }),
                    settings = settings.Select(kvp => new
                    {
                        setting_key = kvp.Key,
                        setting_value = kvp.Value
                    })
                };

                var result = await _api.PushFullSync(payload);
                if (result.Success)
                {
                    Logger.Log("🎉 Full System Sync completed successfully!");
                }
                else
                {
                    Logger.LogError($"❌ Full System Sync failed: {result.Message}", new Exception("Sync Failed"));
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Exception in Full System Sync", ex);
            }
        }

        // Sync Queue removed in favor of Smart Full Sync
        public async Task ProcessSyncQueue()
        {
            // Deprecated
        }

        // Pulls new orders from Backend API, saves to local SQLite, and triggers notification
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
                    // Conflict Resolution: Timestamp-based (newer wins)
                    var existing = _db.GetOrderById(order.Id);
                    if (existing != null)
                    {
                        DateTime existingTime = DateTime.MinValue;
                        DateTime newTime = DateTime.MinValue;

                        DateTime.TryParse(existing.UpdatedAt, out existingTime);
                        DateTime.TryParse(order.UpdatedAt, out newTime);

                        if (newTime <= existingTime)
                        {
                            Logger.Log($"⚔️ Conflict resolution: Local order #{existing.OrderNumber} is newer or same. Skipping pull.");
                            continue;
                        }
                        else
                        {
                            Logger.Log($"⚔️ Conflict resolution: Pulling newer order status #{order.OrderNumber} from website.");
                        }
                    }

                    if (string.IsNullOrEmpty(order.Id))
                        order.Id = Guid.NewGuid().ToString();

                    var items = order.Items ?? new List<OrderItem>();
                    
                    // Upsert order locally
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
            {
                NewOrdersReceived?.Invoke(saved);
            }
        }

        // Helper to retrieve last attempt time from the database
        private DateTime GetLastAttemptTime(string logId)
        {
            try
            {
                using var connection = new SQLiteConnection($"Data Source={_db.GetDatabasePath()};Version=3;");
                connection.Open();
                using var cmd = new SQLiteCommand("SELECT SyncedAt, CreatedAt FROM SyncLogs WHERE Id = @Id", connection);
                cmd.Parameters.AddWithValue("@Id", logId);
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    string val = reader["SyncedAt"] != DBNull.Value ? reader["SyncedAt"]?.ToString() : reader["CreatedAt"]?.ToString();
                    if (DateTime.TryParse(val, out DateTime t))
                    {
                        return t;
                    }
                }
            }
            catch {}
            return DateTime.MinValue;
        }

        protected virtual CloudinaryService CreateCloudinaryService()
        {
            return new CloudinaryService();
        }

        // Connectivity check
        private bool IsInternetAvailable()
        {
            try
            {
                using var client = new System.Net.Http.HttpClient();
                using var response = client.GetAsync("https://www.google.com").GetAwaiter().GetResult();
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        // Internal timer run cycle
        private async Task RunSyncCycle()
        {
            if (_isSyncing) return;
            _isSyncing = true;

            // Trigger "Syncing" status (Yellow)
            SyncStatusChanged?.Invoke(_lastOnlineState, "Syncing");

            try
            {
                bool online = _isOnlineCheck();

                if (!online)
                {
                    _lastOnlineState = false;
                    SyncStatusChanged?.Invoke(false, "Offline");
                    Logger.Log("⚠️ Sync skipped – no internet.");
                    return;
                }

                _lastOnlineState = true;
                SyncStatusChanged?.Invoke(true, "Syncing");

                // ── Step 1: Sync new online orders ───────────────────────────────
                await SyncNewOrders();

                // ── Step 2: Sync dirty state (Timestamp Check) ───────────────────────
                string localTimestampStr = _db.GetSetting("last_menu_update", "");
                string remoteTimestampStr = await _api.GetSyncStatus();
                
                DateTime localTimestamp = DateTime.MinValue;
                DateTime remoteTimestamp = DateTime.MinValue;
                
                DateTime.TryParse(localTimestampStr, out localTimestamp);
                DateTime.TryParse(remoteTimestampStr, out remoteTimestamp);

                if (localTimestamp > remoteTimestamp)
                {
                    Logger.Log($"Local menu (Updated: {localTimestamp}) is newer than Remote ({remoteTimestamp}). Pushing Full Sync.");
                    await PerformFullSyncAsync();
                }
                else
                {
                    // No changes needed
                }

                // Completed successfully (Green)
                SyncStatusChanged?.Invoke(true, "Synced");
            }
            catch (Exception ex)
            {
                Logger.LogError("SyncService.RunSyncCycle error", ex);
                SyncStatusChanged?.Invoke(_lastOnlineState, "Failed");
            }
            finally
            {
                _isSyncing = false;
            }
        }
    }
}