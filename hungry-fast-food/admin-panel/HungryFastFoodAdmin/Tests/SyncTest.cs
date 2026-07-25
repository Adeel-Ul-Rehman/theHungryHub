// E:\hungryHub\hungry-fast-food\admin-panel\HungryFastFoodAdmin\Tests\SyncTest.cs

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using HungryFastFoodAdmin.Models;
using HungryFastFoodAdmin.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace HungryFastFoodAdmin.Tests
{
    [TestClass]
    public class SyncTest
    {
        private string _tempDbPath;
        private DatabaseService _dbService;

        [TestInitialize]
        public void Initialize()
        {
            _tempDbPath = Path.Combine(Path.GetTempPath(), $"hungryhub_test_{Guid.NewGuid():N}.db");
            _dbService = new DatabaseService(_tempDbPath);
            _dbService.InitializeDatabase();
            ClearSyncLogs();
        }

        [TestCleanup]
        public void Cleanup()
        {
            try
            {
                if (File.Exists(_tempDbPath))
                {
                    File.Delete(_tempDbPath);
                }
            }
            catch { }
        }

        private void ClearSyncLogs()
        {
            using (var connection = new System.Data.SQLite.SQLiteConnection($"Data Source={_tempDbPath};Version=3;"))
            {
                connection.Open();
                using (var cmd = new System.Data.SQLite.SQLiteCommand("DELETE FROM SyncLogs", connection))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // ─── TEST CASES ──────────────────────────────────────────────────────────

        [TestMethod]
        public async Task AddCategory_Online_SyncsToDatabase()
        {
            // 1. Setup online state
            bool online = true;
            var fakeApi = new FakeApiService();
            var syncService = new TestSyncService(_dbService, fakeApi, () => online, new FakeCloudinaryService());

            var category = new Category
            {
                Id = "cat-online-1",
                Name = "Burgers",
                Slug = "burgers",
                DisplayOrder = 1,
                IsActive = true
            };

            // Queue the sync operation
            _dbService.CreateCategory(category);
            ClearSyncLogs();
            _dbService.AddSyncLog("add", category.Id, "category", JsonConvert.SerializeObject(category));

            // 2. Sync online
            await syncService.SyncNow();

            // 3. Verify in database/API
            var pending = _dbService.GetPendingSyncLogs();
            Assert.AreEqual(0, pending.Count, "Pending queue should be empty after online sync.");

            var logs = _dbService.GetAllSyncLogs("Synced");
            Assert.IsTrue(logs.Exists(x => x.EntityId == category.Id && x.Status == "synced"), "Sync log status should be updated to 'synced'.");

            Assert.AreEqual(1, fakeApi.SyncedCategories.Count, "API should have received the synced category.");
            Assert.AreEqual("Burgers", fakeApi.SyncedCategories[0].Name);
        }

        [TestMethod]
        public async Task AddProductWithImage_Online_UploadsToCloudinary()
        {
            // 1. Setup online state & temp image file
            bool online = true;
            var fakeApi = new FakeApiService();
            var fakeCloudinary = new FakeCloudinaryService();
            var syncService = new TestSyncService(_dbService, fakeApi, () => online, fakeCloudinary);

            string tempImage = Path.Combine(Path.GetTempPath(), $"hungryhub_test_img_{Guid.NewGuid():N}.jpg");
            File.WriteAllText(tempImage, "dummy image content");

            try
            {
                var category = new Category { Id = "cat-1", Name = "Burgers", Slug = "burgers" };
                _dbService.CreateCategory(category);
                ClearSyncLogs(); // Isolate product creation sync

                var product = new Product
                {
                    Id = "prod-online-1",
                    Name = "Cheese Burger",
                    Slug = "cheese-burger",
                    CategoryId = "cat-1",
                    BasePrice = 250,
                    ImageUrl = tempImage, // local path
                    IsActive = true
                };
                _dbService.CreateProduct(product); // Auto-queues INSERT sync log

                // 2. Sync online
                await syncService.SyncNow();

                // 3. Verify Cloudinary upload & API sync
                var pending = _dbService.GetPendingSyncLogs();
                Assert.AreEqual(0, pending.Count, "Sync queue should be empty.");

                Assert.AreEqual(1, fakeCloudinary.UploadAttempts, "Cloudinary upload should be invoked exactly once.");
                Assert.AreEqual(1, fakeApi.SyncedProducts.Count, "API should have received the synced product.");
                
                // Assert product synced with Cloudinary secure URL
                Assert.AreEqual("https://cloudinary.example.com/image.jpg", fakeApi.SyncedProducts[0].ImageUrl);

                // Verify local SQLite product was updated to Cloudinary URL
                var localProd = _dbService.GetProductById(product.Id);
                Assert.AreEqual("https://cloudinary.example.com/image.jpg", localProd.ImageUrl);
            }
            finally
            {
                if (File.Exists(tempImage)) File.Delete(tempImage);
            }
        }

        [TestMethod]
        public async Task AddCategory_Offline_SavesLocally_SyncsWhenOnline()
        {
            // 1. Setup offline state
            bool online = false;
            var fakeApi = new FakeApiService();
            var syncService = new TestSyncService(_dbService, fakeApi, () => online, new FakeCloudinaryService());

            var category = new Category
            {
                Id = "cat-offline-1",
                Name = "Pizza",
                Slug = "pizza",
                IsActive = true
            };

            // Queue sync
            _dbService.CreateCategory(category);
            ClearSyncLogs();
            _dbService.AddSyncLog("add", category.Id, "category", JsonConvert.SerializeObject(category));

            // 2. Sync while offline
            await syncService.SyncNow();

            // Verify queue remains pending
            var pending = _dbService.GetPendingSyncLogs();
            Assert.AreEqual(1, pending.Count, "Sync log should remain pending when offline.");
            Assert.AreEqual("pending", pending[0].Status);
            Assert.AreEqual(0, fakeApi.SyncedCategories.Count, "API should not receive any syncs when offline.");

            // 3. Toggle online state
            online = true;
            await syncService.SyncNow();

            // Verify sync is successful
            pending = _dbService.GetPendingSyncLogs();
            Assert.AreEqual(0, pending.Count, "Sync log should be cleared after going online.");
            Assert.AreEqual(1, fakeApi.SyncedCategories.Count, "Category should be successfully pushed to the API.");
            Assert.AreEqual("Pizza", fakeApi.SyncedCategories[0].Name);
        }

        [TestMethod]
        public async Task AddProductWithImage_Offline_SavesLocally_UploadsWhenOnline()
        {
            // 1. Setup offline state & temp image file
            bool online = false;
            var fakeApi = new FakeApiService();
            var fakeCloudinary = new FakeCloudinaryService();
            var syncService = new TestSyncService(_dbService, fakeApi, () => online, fakeCloudinary);

            string tempImage = Path.Combine(Path.GetTempPath(), $"hungryhub_test_offline_img_{Guid.NewGuid():N}.jpg");
            File.WriteAllText(tempImage, "dummy image content");

            try
            {
                var category = new Category { Id = "cat-1", Name = "Burgers", Slug = "burgers" };
                _dbService.CreateCategory(category);
                ClearSyncLogs(); // Isolate product creation sync

                var product = new Product
                {
                    Id = "prod-offline-1",
                    Name = "Zinger Burger",
                    Slug = "zinger-burger",
                    CategoryId = "cat-1",
                    BasePrice = 300,
                    ImageUrl = tempImage,
                    IsActive = true
                };
                _dbService.CreateProduct(product); // Auto-queues INSERT sync log

                // 2. Sync while offline
                await syncService.SyncNow();

                // Verify pending and no cloudinary attempts
                var pending = _dbService.GetPendingSyncLogs();
                Assert.AreEqual(1, pending.Count);
                Assert.AreEqual(0, fakeCloudinary.UploadAttempts, "No Cloudinary attempts should be made while offline.");

                // 3. Toggle online
                online = true;
                await syncService.SyncNow();

                // Verify synced
                pending = _dbService.GetPendingSyncLogs();
                Assert.AreEqual(0, pending.Count);
                Assert.AreEqual(1, fakeCloudinary.UploadAttempts, "Cloudinary upload should be invoked after going online.");
                Assert.AreEqual(1, fakeApi.SyncedProducts.Count);
                Assert.AreEqual("https://cloudinary.example.com/image.jpg", fakeApi.SyncedProducts[0].ImageUrl);
            }
            finally
            {
                if (File.Exists(tempImage)) File.Delete(tempImage);
            }
        }

        [TestMethod]
        public async Task UpdateProductStatus_SyncsToDatabase()
        {
            // 1. Setup online state
            bool online = true;
            var fakeApi = new FakeApiService();
            var syncService = new TestSyncService(_dbService, fakeApi, () => online, new FakeCloudinaryService());

            var category = new Category { Id = "cat-1", Name = "Drinks", Slug = "drinks" };
            _dbService.CreateCategory(category);

            var product = new Product
            {
                Id = "prod-status-1",
                Name = "Vanilla Shake",
                Slug = "vanilla-shake",
                CategoryId = "cat-1",
                BasePrice = 180,
                IsActive = true
            };
            _dbService.CreateProduct(product);
            ClearSyncLogs(); // Clear initial inserts so we only check the status update sync log

            // 2. Update status to inactive (calls syncService which does offline-first updates)
            syncService.UpdateProductStatus(product.Id, false);

            // Assert local DB updated immediately
            var localProd = _dbService.GetProductById(product.Id);
            Assert.IsFalse(localProd.IsActive, "Local database product status should update immediately.");

            // Assert sync log is queued
            var pending = _dbService.GetPendingSyncLogs();
            Assert.AreEqual(1, pending.Count);
            Assert.AreEqual("status", pending[0].OperationType);

            // 3. Run Sync online
            await syncService.SyncNow();

            // Assert synced to API
            pending = _dbService.GetPendingSyncLogs();
            Assert.AreEqual(0, pending.Count, "Sync queue should be flushed.");
            Assert.IsTrue(fakeApi.UpdatedProductStatus.ContainsKey(product.Id));
            Assert.IsFalse(fakeApi.UpdatedProductStatus[product.Id], "API should have updated status to false.");
        }

        [TestMethod]
        public async Task PlaceOrderOnWebsite_AppearsInAdminPanel()
        {
            // 1. Setup online state & orders mock pull result
            bool online = true;
            var fakeApi = new FakeApiService();
            var syncService = new TestSyncService(_dbService, fakeApi, () => online, new FakeCloudinaryService());

            var webOrder = new Order
            {
                Id = "web-order-99",
                OrderNumber = "ORD-WEB-999",
                OrderType = "dinein",
                Status = "pending",
                PaymentStatus = "pending",
                CustomerName = "John Doe",
                Subtotal = 500,
                Tax = 25,
                Total = 525,
                PaymentMethod = "cod",
                CreatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                UpdatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                Items = new List<OrderItem>
                {
                    new OrderItem
                    {
                        Id = "web-item-99",
                        OrderId = "web-order-99",
                        ProductName = "Vanilla Shake",
                        Quantity = 1,
                        UnitPrice = 180,
                        TotalPrice = 180
                    }
                }
            };
            fakeApi.NewOrders.Add(webOrder);

            // 2. Execute incremental order pulling
            await syncService.SyncNewOrders();

            // 3. Verify locally stored order details
            var savedOrder = _dbService.GetOrderById("web-order-99");
            Assert.IsNotNull(savedOrder, "Website order should be successfully pulled and stored locally.");
            Assert.AreEqual("ORD-WEB-999", savedOrder.OrderNumber);
            Assert.AreEqual("John Doe", savedOrder.CustomerName);
            Assert.AreEqual(1, savedOrder.Items.Count);
            Assert.AreEqual("Vanilla Shake", savedOrder.Items[0].ProductName);
        }

        [TestMethod]
        public async Task RealCloudinaryUploadTest()
        {
            var cloudinary = new CloudinaryService();
            string tempImage = Path.Combine(Path.GetTempPath(), "dummy_test_image.png");
            // Standard small 1x1 PNG file base64 data to bypass any image format decoder issues
            byte[] pngBytes = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==");
            File.WriteAllBytes(tempImage, pngBytes);

            try
            {
                var result = await cloudinary.UploadImage(tempImage, "hungryhub/products/a1b2c3d4-e5f6-7a8b-9c0d-1234567890ab");
                Logger.Log($"☁️ [Cloudinary Test Result] SUCCESS: {result.Success}");
                if (!result.Success)
                {
                    Logger.Log($"☁️ [Cloudinary Test Result] ERROR MESSAGE: {result.Error?.Message}");
                }
            }
            finally
            {
                if (File.Exists(tempImage)) File.Delete(tempImage);
            }
        }

        [TestMethod]
        public async Task MultiplePendingSyncs_ProcessesInOrder()
        {
            // 1. Queue multiple categories
            bool online = true;
            var fakeApi = new FakeApiService();
            var syncService = new TestSyncService(_dbService, fakeApi, () => online, new FakeCloudinaryService());

            var catA = new Category { Id = "cat-A", Name = "Drinks", Slug = "drinks" };
            var catB = new Category { Id = "cat-B", Name = "Desserts", Slug = "desserts" };

            // Queue desserts first (older timestamp), then drinks
            _dbService.CreateCategory(catA);
            _dbService.CreateCategory(catB);
            ClearSyncLogs();
            _dbService.AddSyncLog("add", catB.Id, "category", JsonConvert.SerializeObject(catB));
            _dbService.AddSyncLog("add", catA.Id, "category", JsonConvert.SerializeObject(catA));

            // 2. Sync online
            await syncService.SyncNow();

            // 3. Verify sequential execution (Desserts must hit first)
            Assert.AreEqual(2, fakeApi.SyncedCategories.Count);
            Assert.AreEqual("Desserts", fakeApi.SyncedCategories[0].Name, "Desserts (queued first) must be processed first.");
            Assert.AreEqual("Drinks", fakeApi.SyncedCategories[1].Name, "Drinks (queued second) must be processed second.");
        }

        [TestMethod]
        public async Task SyncFailure_RetriesWithExponentialBackoff()
        {
            // 1. Setup API failure state
            bool online = true;
            var fakeApi = new FakeApiService { FailOnSyncCategory = true };
            var syncService = new TestSyncService(_dbService, fakeApi, () => online, new FakeCloudinaryService());

            var category = new Category { Id = "cat-fail", Name = "Sides", Slug = "sides" };
            _dbService.CreateCategory(category);
            ClearSyncLogs();
            _dbService.AddSyncLog("add", category.Id, "category", JsonConvert.SerializeObject(category));

            // 2. Run sync: should attempt, fail, increment retry count to 1, status = pending
            await syncService.SyncNow();

            var logs = _dbService.GetAllSyncLogs();
            var item = logs.Find(x => x.EntityId == category.Id);
            Assert.IsNotNull(item);
            Assert.AreEqual("pending", item.Status);
            Assert.AreEqual(1, item.RetryCount);

            // 3. Clear failures so that a retry WOULD succeed
            fakeApi.SyncedCategories.Clear();
            fakeApi.FailOnSyncCategory = false;

            // Sync again immediately: should skip due to exponential backoff (RetryCount 1 needs 1 min backoff)
            await syncService.SyncNow();

            // Assert it remains pending and skipped
            var logsSecond = _dbService.GetAllSyncLogs();
            var itemSecond = logsSecond.Find(x => x.EntityId == category.Id);
            Assert.AreEqual("pending", itemSecondSecondCheck(itemSecond), "Sync log should remain pending because it was skipped.");
            Assert.AreEqual(1, itemSecond.RetryCount);
            Assert.AreEqual(0, fakeApi.SyncedCategories.Count, "API should not be called because it was skipped by exponential backoff.");
        }

        private string itemSecondSecondCheck(DatabaseService.SyncLogItem item)
        {
            return item?.Status;
        }

        // ─── HELPER CLASSES ───────────────────────────────────────────────────────

        private class FakeApiService : IApiService
        {
            public Task<string> GetSyncStatus()
            {
                return Task.FromResult("2026-07-22T00:00:00.000Z");
            }
            public List<Order> NewOrders { get; set; } = new List<Order>();
            
            public List<Category> SyncedCategories { get; } = new List<Category>();
            public List<Product> SyncedProducts { get; } = new List<Product>();
            public Dictionary<string, bool> UpdatedProductStatus { get; } = new Dictionary<string, bool>();
            public List<Order> SyncedOrders { get; } = new List<Order>();

            public bool FailOnSyncCategory { get; set; }
            public bool FailOnSyncProduct { get; set; }

            public Task<ApiService.ApiResult<bool>> DeleteCategory(string id)
            {
                return Task.FromResult(new ApiService.ApiResult<bool> { Success = true });
            }

            public Task<ApiService.ApiResult<bool>> DeleteProduct(string id)
            {
                return Task.FromResult(new ApiService.ApiResult<bool> { Success = true });
            }

            public Task<List<Order>> PullNewOrders(string since)
            {
                return Task.FromResult(NewOrders);
            }

            public Task<ApiService.ApiResult<bool>> PushSyncItems(List<DatabaseService.SyncQueueItem> items)
            {
                return Task.FromResult(new ApiService.ApiResult<bool> { Success = true });
            }

            public Task<ApiService.ApiResult<bool>> SyncCategory(Category category)
            {
                if (FailOnSyncCategory)
                {
                    return Task.FromResult(new ApiService.ApiResult<bool> { Success = false, Message = "API Sync failure simulated" });
                }
                SyncedCategories.Add(category);
                return Task.FromResult(new ApiService.ApiResult<bool> { Success = true });
            }

            public Task<ApiService.ApiResult<bool>> SyncOrder(Order order)
            {
                SyncedOrders.Add(order);
                return Task.FromResult(new ApiService.ApiResult<bool> { Success = true });
            }

            public Task<ApiService.ApiResult<bool>> PushFullSync(object payload)
            {
                return Task.FromResult(new ApiService.ApiResult<bool> { Success = true });
            }

            public Task<ApiService.ApiResult<bool>> SyncProduct(Product product)
            {
                if (FailOnSyncProduct)
                {
                    return Task.FromResult(new ApiService.ApiResult<bool> { Success = false, Message = "API Sync failure simulated" });
                }
                SyncedProducts.Add(product);
                return Task.FromResult(new ApiService.ApiResult<bool> { Success = true });
            }

            public Task<ApiService.ApiResult<bool>> UpdateCategory(string id, Category category)
            {
                return Task.FromResult(new ApiService.ApiResult<bool> { Success = true });
            }

            public Task<ApiService.ApiResult<bool>> UpdateProduct(string id, Product product)
            {
                return Task.FromResult(new ApiService.ApiResult<bool> { Success = true });
            }

            public Task<ApiService.ApiResult<bool>> UpdateProductAvailability(string id, bool isActive)
            {
                UpdatedProductStatus[id] = isActive;
                return Task.FromResult(new ApiService.ApiResult<bool> { Success = true });
            }
        }

        private class FakeCloudinaryService : CloudinaryService
        {
            public int UploadAttempts { get; private set; }
            public int FailAttemptsBeforeSuccess { get; set; }

            public override async Task<string> UploadImageAsync(string localPath)
            {
                UploadAttempts++;
                await Task.Yield();

                if (UploadAttempts <= FailAttemptsBeforeSuccess)
                {
                    return string.Empty;
                }

                return "https://cloudinary.example.com/image.jpg";
            }
        }

        private class TestSyncService : SyncService
        {
            private readonly CloudinaryService _cloudinaryService;

            public TestSyncService(DatabaseService dbService, IApiService apiService, Func<bool> internetCheck, CloudinaryService cloudinaryService)
                : base(dbService, apiService, internetCheck)
            {
                _cloudinaryService = cloudinaryService;
            }

            protected override CloudinaryService CreateCloudinaryService()
            {
                return _cloudinaryService;
            }
            public Task<string> GetSyncStatus()
            {
                return Task.FromResult("2026-07-22T00:00:00.000Z");
            }
        }
    }
}
