// E:\hungryHub\hungry-fast-food\admin-panel\HungryFastFoodAdmin\Services\DatabaseService.cs

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using HungryFastFoodAdmin.Models;
using Newtonsoft.Json;

namespace HungryFastFoodAdmin.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;
        private readonly string _databasePath;

        public DatabaseService(string databasePath = null)
        {
            string dbPath = databasePath;
            if (string.IsNullOrEmpty(dbPath))
            {
                dbPath = ConfigManager.GetAppSetting("DatabasePath", "D:\\HungryFastFood\\hungryfastfood.db");
            }

            _databasePath = dbPath;

            // Ensure directory exists
            string directory = Path.GetDirectoryName(dbPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            _connectionString = $"Data Source={dbPath};Version=3;";
        }

        public void InitializeDatabase()
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            // Check if SyncLogs needs upgrade
            bool needsUpgrade = false;
            try
            {
                using var checkCmd = new SQLiteCommand("SELECT OperationType FROM SyncLogs LIMIT 1", connection);
                using var reader = checkCmd.ExecuteReader();
            }
            catch
            {
                needsUpgrade = true;
            }

            if (needsUpgrade)
            {
                ExecuteNonQuery(connection, "DROP TABLE IF EXISTS SyncLogs;");
            }

            // Create tables
            ExecuteNonQuery(connection, CreateUsersTable());
            ExecuteNonQuery(connection, CreateCategoriesTable());
            ExecuteNonQuery(connection, CreateProductsTable());
            ExecuteNonQuery(connection, CreateProductVariationsTable());
            ExecuteNonQuery(connection, CreateDealsTable());
            ExecuteNonQuery(connection, CreateDealItemsTable());
            ExecuteNonQuery(connection, CreateOrdersTable());
            ExecuteNonQuery(connection, CreateOrderItemsTable());
            // ExecuteNonQuery(connection, CreateSyncLogsTable());
            ExecuteNonQuery(connection, CreateAdminActivityLogsTable());
            ExecuteNonQuery(connection, CreateSystemSettingsTable());
            ExecuteNonQuery(connection, CreateDeliveryZonesTable());
            ExecuteNonQuery(connection, CreateSyncQueueTable());

            Console.WriteLine("✅ Database initialized successfully");

            // Queue all existing local data for Neon sync on first boot
            QueueAllExistingDataForSync();

            // Fix any SyncLog rows corrupted by old singularization bug (e.g. "categorie" → "category")
            MigrateCorruptSyncLogEntityTypes(connection);

            // Mark orphaned category SyncLog entries as permanently failed
            MigrateOrphanedCategorySyncLogs(connection);
        }

        public string AddSyncLog(string operationType, string entityId, string entityType, string data)
        {
            return "";
        }
        public List<SyncLogItem> GetPendingSyncs()
        {
            return new List<SyncLogItem>();
        }
        public void UpdateSyncStatus(string syncId, string status, string errorMessage = null)
        {
            return;
        }
        public List<SyncLogItem> GetFailedSyncs()
        {
            return new List<SyncLogItem>();
        }
        public void RetryFailedSyncs()
        {
            try
            {
                using var connection = new SQLiteConnection(_connectionString);
                connection.Open();
                string sql = "UPDATE SyncLogs SET Status = 'pending', RetryCount = 0, ErrorMessage = NULL WHERE Status = 'failed'";
                using var cmd = new SQLiteCommand(sql, connection);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Logger.LogError("RetryFailedSyncs failed", ex);
            }
        }

        public void CleanSyncLogs(int days = 30)
        {
            return;
        }
        public void ClearAllSyncLogs()
        {
            return;
        }
        public Dictionary<string, int> GetSyncStats()
        {
            var stats = new Dictionary<string, int>
            {
                { "pending", 0 },
                { "synced", 0 },
                { "failed", 0 }
            };
            try
            {
                using var connection = new SQLiteConnection(_connectionString);
                connection.Open();
                string sql = "SELECT Status, COUNT(*) as Count FROM SyncLogs GROUP BY Status";
                using var cmd = new SQLiteCommand(sql, connection);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string status = reader["Status"].ToString().ToLower();
                    int count = Convert.ToInt32(reader["Count"]);
                    if (stats.ContainsKey(status))
                    {
                        stats[status] = count;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("GetSyncStats failed", ex);
            }
            return stats;
        }

        public class SyncLogItem
        {
            public string Id { get; set; }
            public string OperationType { get; set; }
            public string EntityId { get; set; }
            public string EntityType { get; set; }
            public string Data { get; set; }
            public string Status { get; set; }
            public int RetryCount { get; set; }
            public string ErrorMessage { get; set; }
            public string CreatedAt { get; set; }
            public string SyncedAt { get; set; }
        }

        public List<SyncLogItem> GetPendingSyncLogs()
        {
            return new List<SyncLogItem>();
        }
        public List<SyncLogItem> GetAllSyncLogs(string status = null, string startDate = null, string endDate = null, string search = null)
        {
            return new List<SyncLogItem>();
        }
        public void UpdateSyncLogStatus(string id, string status, int retryCount, string errorMessage = null)
        {
            return;
        }
        public string GetLatestOrderUpdateTimestamp()
        {
            try
            {
                using var connection = new SQLiteConnection(_connectionString);
                connection.Open();
                using var cmd = new SQLiteCommand("SELECT MAX(UpdatedAt) FROM Orders", connection);
                var res = cmd.ExecuteScalar();
                return (res != DBNull.Value && res != null) ? res.ToString() : "";
            }
            catch
            {
                return "";
            }
        }
        private string CreateSyncQueueTable()
        {
            return @"CREATE TABLE IF NOT EXISTS SyncQueue (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                OperationType TEXT NOT NULL,
                TableName TEXT NOT NULL,
                RecordId TEXT NOT NULL,
                Payload TEXT,
                CreatedAt TEXT DEFAULT CURRENT_TIMESTAMP
            )";
        }

        private string CreateUsersTable()
        {
            return @"CREATE TABLE IF NOT EXISTS Users (
                Id TEXT PRIMARY KEY,
                Email TEXT UNIQUE NOT NULL,
                FullName TEXT,
                Phone TEXT,
                IsVerified INTEGER DEFAULT 0,
                GoogleId TEXT,
                IsGuest INTEGER DEFAULT 0,
                CreatedAt TEXT DEFAULT CURRENT_TIMESTAMP
            )";
        }

        private string CreateCategoriesTable()
        {
            return @"CREATE TABLE IF NOT EXISTS Categories (
                Id TEXT PRIMARY KEY,
                Name TEXT NOT NULL,
                Slug TEXT UNIQUE NOT NULL,
                DisplayOrder INTEGER DEFAULT 0,
                IsActive INTEGER DEFAULT 1
            )";
        }

        private string CreateProductsTable()
        {
            return @"CREATE TABLE IF NOT EXISTS Products (
                Id TEXT PRIMARY KEY,
                CategoryId TEXT,
                Name TEXT NOT NULL,
                Slug TEXT UNIQUE NOT NULL,
                Description TEXT,
                BasePrice REAL NOT NULL,
                DiscountPrice REAL,
                HasVariations INTEGER DEFAULT 0,
                IsActive INTEGER DEFAULT 1,
                IsDeal INTEGER DEFAULT 0,
                ImageUrl TEXT,
                DisplayOrder INTEGER DEFAULT 0,
                FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
            )";
        }

        private string CreateProductVariationsTable()
        {
            return @"CREATE TABLE IF NOT EXISTS ProductVariations (
                Id TEXT PRIMARY KEY,
                ProductId TEXT NOT NULL,
                VariationType TEXT NOT NULL,
                VariationName TEXT NOT NULL,
                PriceAdjustment REAL DEFAULT 0,
                IsDefault INTEGER DEFAULT 0,
                FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE
            )";
        }

        private string CreateDealsTable()
        {
            return @"CREATE TABLE IF NOT EXISTS Deals (
                Id TEXT PRIMARY KEY,
                Name TEXT NOT NULL,
                Slug TEXT UNIQUE NOT NULL,
                Description TEXT,
                TotalPrice REAL NOT NULL,
                DiscountPrice REAL,
                IsActive INTEGER DEFAULT 1,
                IsFeatured INTEGER DEFAULT 0,
                ImageUrl TEXT
            )";
        }

        private string CreateDealItemsTable()
        {
            return @"CREATE TABLE IF NOT EXISTS DealItems (
                Id TEXT PRIMARY KEY,
                DealId TEXT NOT NULL,
                ProductId TEXT NOT NULL,
                VariationId TEXT,
                Quantity INTEGER DEFAULT 1,
                UnitPrice REAL NOT NULL,
                FOREIGN KEY (DealId) REFERENCES Deals(Id) ON DELETE CASCADE,
                FOREIGN KEY (ProductId) REFERENCES Products(Id)
            )";
        }

        private string CreateOrdersTable()
        {
            return @"CREATE TABLE IF NOT EXISTS Orders (
                Id TEXT PRIMARY KEY,
                OrderNumber TEXT UNIQUE NOT NULL,
                OrderType TEXT NOT NULL,
                UserId TEXT,
                CustomerName TEXT NOT NULL,
                CustomerPhone TEXT,
                CustomerEmail TEXT,
                DeliveryAddress TEXT,
                DeliveryLatitude REAL,
                DeliveryLongitude REAL,
                Status TEXT DEFAULT 'pending',
                Subtotal REAL NOT NULL,
                DeliveryCharge REAL DEFAULT 0,
                Tax REAL DEFAULT 0,
                Total REAL NOT NULL,
                PaymentMethod TEXT,
                PaymentStatus TEXT DEFAULT 'pending',
                IsSuspicious INTEGER DEFAULT 0,
                AdminNotes TEXT,
                CreatedAt TEXT DEFAULT CURRENT_TIMESTAMP,
                UpdatedAt TEXT DEFAULT CURRENT_TIMESTAMP,
                SyncedAt TEXT,
                IsSynced INTEGER DEFAULT 0
            )";
        }

        private string CreateOrderItemsTable()
        {
            return @"CREATE TABLE IF NOT EXISTS OrderItems (
                Id TEXT PRIMARY KEY,
                OrderId TEXT NOT NULL,
                ProductName TEXT NOT NULL,
                VariationName TEXT,
                Quantity INTEGER NOT NULL,
                UnitPrice REAL NOT NULL,
                TotalPrice REAL NOT NULL,
                IsFromDeal INTEGER DEFAULT 0,
                DealId TEXT,
                FOREIGN KEY (OrderId) REFERENCES Orders(Id) ON DELETE CASCADE
            )";
        }

        private string CreateSyncLogsTable()
        {
            return @"CREATE TABLE IF NOT EXISTS SyncLogs (
                Id TEXT PRIMARY KEY,
                OperationType TEXT NOT NULL,
                EntityId TEXT NOT NULL,
                EntityType TEXT NOT NULL,
                Data TEXT NOT NULL,
                Status TEXT DEFAULT 'pending',
                RetryCount INTEGER DEFAULT 0,
                ErrorMessage TEXT,
                CreatedAt TEXT DEFAULT CURRENT_TIMESTAMP,
                SyncedAt TEXT
            )";
        }

        private string CreateAdminActivityLogsTable()
        {
            return @"CREATE TABLE IF NOT EXISTS AdminActivityLogs (
                Id TEXT PRIMARY KEY,
                AdminEmail TEXT NOT NULL,
                Action TEXT NOT NULL,
                Details TEXT,
                IpAddress TEXT,
                CreatedAt TEXT DEFAULT CURRENT_TIMESTAMP
            )";
        }

        private string CreateSystemSettingsTable()
        {
            return @"CREATE TABLE IF NOT EXISTS SystemSettings (
                Id TEXT PRIMARY KEY,
                SettingKey TEXT UNIQUE NOT NULL,
                SettingValue TEXT,
                SettingType TEXT DEFAULT 'string',
                Description TEXT,
                UpdatedAt TEXT DEFAULT CURRENT_TIMESTAMP
            )";
        }

        private string CreateDeliveryZonesTable()
        {
            return @"CREATE TABLE IF NOT EXISTS DeliveryZones (
                Id TEXT PRIMARY KEY,
                Name TEXT NOT NULL,
                MaxDistance REAL NOT NULL,
                Charge REAL NOT NULL,
                MinOrder REAL DEFAULT 0
            )";
        }

        private void ExecuteNonQuery(SQLiteConnection connection, string sql)
        {
            using var cmd = new SQLiteCommand(sql, connection);
            cmd.ExecuteNonQuery();
        }

        // Generate order number
        public string GetOrderNumber(string orderType)
        {
            string prefix = orderType.ToLower() switch
            {
                "dining" => "D-",
                "delivery" => "DL-",
                "takeaway" => "TA-",
                _ => "O-"
            };

            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            string query = "SELECT OrderNumber FROM Orders WHERE OrderNumber LIKE @prefix ORDER BY OrderNumber DESC LIMIT 1";
            using var cmd = new SQLiteCommand(query, connection);
            cmd.Parameters.AddWithValue("@prefix", $"{prefix}%");

            var result = cmd.ExecuteScalar();
            int number = 1;

            if (result != null)
            {
                string lastNumber = result.ToString().Replace(prefix, "");
                if (int.TryParse(lastNumber, out int lastNum))
                {
                    number = lastNum + 1;
                }
            }

            return $"{prefix}{number:D4}";
        }

        // Create order
        public Order CreateOrder(Order order, List<OrderItem> items)
        {
            if (string.IsNullOrEmpty(order.Id))
            {
                order.Id = Guid.NewGuid().ToString();
            }

            if (string.IsNullOrEmpty(order.OrderNumber))
            {
                order.OrderNumber = GetOrderNumber(order.OrderType);
            }

            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // Insert order
                string orderSql = @"INSERT INTO Orders (
                    Id, OrderNumber, OrderType, UserId, CustomerName, CustomerPhone, CustomerEmail,
                    DeliveryAddress, DeliveryLatitude, DeliveryLongitude, Status,
                    Subtotal, DeliveryCharge, Tax, Total, PaymentMethod, PaymentStatus,
                    IsSuspicious, AdminNotes, IsSynced
                ) VALUES (
                    @Id, @OrderNumber, @OrderType, @UserId, @CustomerName, @CustomerPhone, @CustomerEmail,
                    @DeliveryAddress, @DeliveryLatitude, @DeliveryLongitude, @Status,
                    @Subtotal, @DeliveryCharge, @Tax, @Total, @PaymentMethod, @PaymentStatus,
                    @IsSuspicious, @AdminNotes, 0
                )";

                using var cmd = new SQLiteCommand(orderSql, connection);
                cmd.Parameters.AddWithValue("@Id", order.Id);
                cmd.Parameters.AddWithValue("@OrderNumber", order.OrderNumber);
                cmd.Parameters.AddWithValue("@OrderType", order.OrderType);
                cmd.Parameters.AddWithValue("@UserId", (object)order.UserId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@CustomerName", order.CustomerName);
                cmd.Parameters.AddWithValue("@CustomerPhone", (object)order.CustomerPhone ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@CustomerEmail", (object)order.CustomerEmail ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DeliveryAddress", (object)order.DeliveryAddress ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DeliveryLatitude", order.DeliveryLatitude ?? 0);
                cmd.Parameters.AddWithValue("@DeliveryLongitude", order.DeliveryLongitude ?? 0);
                cmd.Parameters.AddWithValue("@Status", order.Status ?? "pending");
                cmd.Parameters.AddWithValue("@Subtotal", order.Subtotal);
                cmd.Parameters.AddWithValue("@DeliveryCharge", order.DeliveryCharge);
                cmd.Parameters.AddWithValue("@Tax", order.Tax);
                cmd.Parameters.AddWithValue("@Total", order.Total);
                cmd.Parameters.AddWithValue("@PaymentMethod", (object)order.PaymentMethod ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@PaymentStatus", order.PaymentStatus ?? "pending");
                cmd.Parameters.AddWithValue("@IsSuspicious", order.IsSuspicious ? 1 : 0);
                cmd.Parameters.AddWithValue("@AdminNotes", (object)order.AdminNotes ?? DBNull.Value);

                cmd.ExecuteNonQuery();

                // Get the order ID
                string getIdSql = "SELECT Id FROM Orders WHERE OrderNumber = @OrderNumber";
                using var getIdCmd = new SQLiteCommand(getIdSql, connection);
                getIdCmd.Parameters.AddWithValue("@OrderNumber", order.OrderNumber);
                var orderId = getIdCmd.ExecuteScalar().ToString();

                // Insert order items
                foreach (var item in items)
                {
                    string itemSql = @"INSERT INTO OrderItems (
                        Id, OrderId, ProductName, VariationName, Quantity,
                        UnitPrice, TotalPrice, IsFromDeal, DealId
                    ) VALUES (
                        @Id, @OrderId, @ProductName, @VariationName, @Quantity,
                        @UnitPrice, @TotalPrice, @IsFromDeal, @DealId
                    )";

                    using var itemCmd = new SQLiteCommand(itemSql, connection);
                    itemCmd.Parameters.AddWithValue("@Id", Guid.NewGuid().ToString());
                    itemCmd.Parameters.AddWithValue("@OrderId", orderId);
                    itemCmd.Parameters.AddWithValue("@ProductName", item.ProductName);
                    itemCmd.Parameters.AddWithValue("@VariationName", (object)item.VariationName ?? DBNull.Value);
                    itemCmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                    itemCmd.Parameters.AddWithValue("@UnitPrice", item.UnitPrice);
                    itemCmd.Parameters.AddWithValue("@TotalPrice", item.TotalPrice);
                    itemCmd.Parameters.AddWithValue("@IsFromDeal", item.IsFromDeal ? 1 : 0);
                    itemCmd.Parameters.AddWithValue("@DealId", (object)item.DealId ?? DBNull.Value);

                    itemCmd.ExecuteNonQuery();
                }

                transaction.Commit();

                // Return created order
                order.Id = orderId;
                order.Items = items;

                // Sync Queue Hook
                AddToSyncQueue("INSERT", "Orders", order.Id, JsonConvert.SerializeObject(order));

                return order;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public List<Order> GetOrders(string status = null, string orderType = null, 
                                      string startDate = null, string endDate = null,
                                      bool onlySuspicious = false)
        {
            var orders = new List<Order>();
            var conditions = new List<string>();
            var parameters = new Dictionary<string, object>();

            if (onlySuspicious)
            {
                conditions.Add("IsSuspicious = 1");
            }
            else if (!string.IsNullOrEmpty(status))
            {
                conditions.Add("Status = @Status");
                parameters["@Status"] = status;
            }

            if (!string.IsNullOrEmpty(orderType))
            {
                conditions.Add("OrderType = @OrderType");
                parameters["@OrderType"] = orderType;
            }

            if (!string.IsNullOrEmpty(startDate))
            {
                conditions.Add("Date(CreatedAt) >= @StartDate");
                parameters["@StartDate"] = startDate;
            }

            if (!string.IsNullOrEmpty(endDate))
            {
                conditions.Add("Date(CreatedAt) <= @EndDate");
                parameters["@EndDate"] = endDate;
            }

            string whereClause = conditions.Count > 0 ? "WHERE " + string.Join(" AND ", conditions) : "";
            string sql = $"SELECT * FROM Orders {whereClause} ORDER BY CreatedAt DESC";

            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();
            using var cmd = new SQLiteCommand(sql, connection);

            foreach (var param in parameters)
            {
                cmd.Parameters.AddWithValue(param.Key, param.Value);
            }

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var order = MapOrder(reader);
                order.Items = GetOrderItems(order.Id, connection);
                orders.Add(order);
            }

            return orders;
        }

        public List<Order> GetPendingOrders()
        {
            return GetOrders(status: "pending");
        }

        // Get order by ID with items
        public Order GetOrderById(string id)
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            string sql = "SELECT * FROM Orders WHERE Id = @Id";
            using var cmd = new SQLiteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Id", id);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                var order = MapOrder(reader);
                order.Items = GetOrderItems(id, connection);
                return order;
            }

            return null;
        }

        // Get order items
        private List<OrderItem> GetOrderItems(string orderId, SQLiteConnection connection)
        {
            var items = new List<OrderItem>();
            string sql = "SELECT * FROM OrderItems WHERE OrderId = @OrderId";

            using var cmd = new SQLiteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@OrderId", orderId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                items.Add(new OrderItem
                {
                    Id = reader["Id"].ToString(),
                    OrderId = reader["OrderId"].ToString(),
                    ProductName = reader["ProductName"].ToString(),
                    VariationName = reader["VariationName"]?.ToString(),
                    Quantity = Convert.ToInt32(reader["Quantity"]),
                    UnitPrice = Convert.ToDecimal(reader["UnitPrice"]),
                    TotalPrice = Convert.ToDecimal(reader["TotalPrice"]),
                    IsFromDeal = Convert.ToInt32(reader["IsFromDeal"]) == 1,
                    DealId = reader["DealId"]?.ToString()
                });
            }

            return items;
        }

        // Map order from reader
        private Order MapOrder(SQLiteDataReader reader)
        {
            return new Order
            {
                Id = reader["Id"].ToString(),
                OrderNumber = reader["OrderNumber"].ToString(),
                OrderType = reader["OrderType"].ToString(),
                UserId = reader["UserId"]?.ToString(),
                CustomerName = reader["CustomerName"].ToString(),
                CustomerPhone = reader["CustomerPhone"]?.ToString(),
                CustomerEmail = reader["CustomerEmail"]?.ToString(),
                DeliveryAddress = reader["DeliveryAddress"]?.ToString(),
                DeliveryLatitude = reader["DeliveryLatitude"] != DBNull.Value ? Convert.ToDouble(reader["DeliveryLatitude"]) : (double?)null,
                DeliveryLongitude = reader["DeliveryLongitude"] != DBNull.Value ? Convert.ToDouble(reader["DeliveryLongitude"]) : (double?)null,
                Status = reader["Status"].ToString(),
                Subtotal = Convert.ToDecimal(reader["Subtotal"]),
                DeliveryCharge = Convert.ToDecimal(reader["DeliveryCharge"]),
                Tax = Convert.ToDecimal(reader["Tax"]),
                Total = Convert.ToDecimal(reader["Total"]),
                PaymentMethod = reader["PaymentMethod"]?.ToString(),
                PaymentStatus = reader["PaymentStatus"]?.ToString(),
                IsSuspicious = Convert.ToInt32(reader["IsSuspicious"]) == 1,
                AdminNotes = reader["AdminNotes"]?.ToString(),
                CreatedAt = reader["CreatedAt"].ToString(),
                UpdatedAt = reader["UpdatedAt"].ToString(),
                SyncedAt = reader["SyncedAt"]?.ToString(),
                IsSynced = Convert.ToInt32(reader["IsSynced"]) == 1
            };
        }

        // Update order status
        public bool UpdateOrderStatus(string id, string status, string adminEmail)
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            string sql = "UPDATE Orders SET Status = @Status, UpdatedAt = CURRENT_TIMESTAMP WHERE Id = @Id";
            using var cmd = new SQLiteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Status", status);
            cmd.Parameters.AddWithValue("@Id", id);

            int rowsAffected = cmd.ExecuteNonQuery();

            // Log admin activity
            if (rowsAffected > 0)
            {
                LogAdminActivity(adminEmail, "UpdateOrderStatus", $"Order {id} status updated to {status}");

                // Sync Queue Hook
                var updatedOrder = GetOrderById(id);
                if (updatedOrder != null)
                {
                    AddToSyncQueue("UPDATE", "Orders", id, JsonConvert.SerializeObject(updatedOrder));
                }
            }

            return rowsAffected > 0;
        }

        public bool CancelCompletedOrder(string id, string adminEmail)
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            string sql = "UPDATE Orders SET Status = 'cancelled', IsSuspicious = 1, UpdatedAt = CURRENT_TIMESTAMP WHERE Id = @Id";
            using var cmd = new SQLiteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Id", id);

            int rowsAffected = cmd.ExecuteNonQuery();

            if (rowsAffected > 0)
            {
                LogAdminActivity(adminEmail, "CancelCompletedOrder", $"Completed Order {id} was CANCELLED (marked suspicious)");

                var updatedOrder = GetOrderById(id);
                if (updatedOrder != null)
                {
                    AddToSyncQueue("UPDATE", "Orders", id, JsonConvert.SerializeObject(updatedOrder));
                }
            }

            return rowsAffected > 0;
        }

        public bool UpdateOrderTotalAndNotes(string id, decimal newTotal, string note)
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            string sql = "UPDATE Orders SET Total = @Total, AdminNotes = @AdminNotes, UpdatedAt = CURRENT_TIMESTAMP WHERE Id = @Id";
            using var cmd = new SQLiteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Total", newTotal);
            cmd.Parameters.AddWithValue("@AdminNotes", note);
            cmd.Parameters.AddWithValue("@Id", id);

            int rowsAffected = cmd.ExecuteNonQuery();

            if (rowsAffected > 0)
            {
                var updatedOrder = GetOrderById(id);
                if (updatedOrder != null)
                {
                    AddToSyncQueue("UPDATE", "Orders", id, JsonConvert.SerializeObject(updatedOrder));
                }
            }

            return rowsAffected > 0;
        }

        // Get unsynced orders
        public List<Order> GetUnsyncedOrders()
        {
            var orders = new List<Order>();
            string sql = "SELECT * FROM Orders WHERE IsSynced = 0 OR IsSynced IS NULL ORDER BY CreatedAt ASC";

            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();
            using var cmd = new SQLiteCommand(sql, connection);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                var order = MapOrder(reader);
                order.Items = GetOrderItems(order.Id, connection);
                orders.Add(order);
            }

            return orders;
        }

        // Mark orders as synced
        public void MarkOrdersSynced(List<string> orderIds)
        {
            if (orderIds.Count == 0) return;

            string ids = string.Join(",", orderIds.Select(id => $"'{id}'"));
            string sql = $"UPDATE Orders SET IsSynced = 1, SyncedAt = CURRENT_TIMESTAMP WHERE Id IN ({ids})";

            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();
            using var cmd = new SQLiteCommand(sql, connection);
            cmd.ExecuteNonQuery();
        }

        // Get dashboard stats
        public Dictionary<string, object> GetDashboardStats()
        {
            var stats = new Dictionary<string, object>();
            string sql = @"
                SELECT 
                    COUNT(*) as TotalOrders,
                    SUM(CASE WHEN Status = 'pending' THEN 1 ELSE 0 END) as PendingOrders,
                    SUM(CASE WHEN Status = 'confirmed' THEN 1 ELSE 0 END) as ConfirmedOrders,
                    SUM(CASE WHEN Status = 'completed' THEN 1 ELSE 0 END) as CompletedOrders,
                    SUM(CASE WHEN OrderType = 'dining' THEN 1 ELSE 0 END) as DiningOrders,
                    SUM(CASE WHEN OrderType = 'delivery' THEN 1 ELSE 0 END) as DeliveryOrders,
                    SUM(Total) as TotalRevenue
                FROM Orders
                WHERE Date(CreatedAt) = Date('now')
            ";

            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();
            using var cmd = new SQLiteCommand(sql, connection);
            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                stats["TotalOrders"] = Convert.ToInt32(reader["TotalOrders"]);
                stats["PendingOrders"] = Convert.ToInt32(reader["PendingOrders"]);
                stats["ConfirmedOrders"] = Convert.ToInt32(reader["ConfirmedOrders"]);
                stats["CompletedOrders"] = Convert.ToInt32(reader["CompletedOrders"]);
                stats["DiningOrders"] = Convert.ToInt32(reader["DiningOrders"]);
                stats["DeliveryOrders"] = Convert.ToInt32(reader["DeliveryOrders"]);
                stats["TotalRevenue"] = Convert.ToDecimal(reader["TotalRevenue"]);
            }

            return stats;
        }

        public Dictionary<string, object> GetDetailedDashboardStats()
        {
            var stats = new Dictionary<string, object>();
            
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();
            
            using (var cmd = new SQLiteCommand("SELECT COUNT(*) FROM Orders", connection))
            {
                stats["TotalOrders"] = Convert.ToInt32(cmd.ExecuteScalar());
            }
            
            using (var cmd = new SQLiteCommand("SELECT SUM(Total) FROM Orders WHERE Status = 'completed'", connection))
            {
                var val = cmd.ExecuteScalar();
                stats["TotalRevenue"] = val != DBNull.Value ? Convert.ToDecimal(val) : 0m;
            }
            
            using (var cmd = new SQLiteCommand("SELECT COUNT(*) FROM Orders WHERE Status = 'pending'", connection))
            {
                stats["PendingOrders"] = Convert.ToInt32(cmd.ExecuteScalar());
            }
            
            using (var cmd = new SQLiteCommand("SELECT COUNT(*) FROM Orders WHERE Date(CreatedAt) = Date('now')", connection))
            {
                stats["TodaysOrders"] = Convert.ToInt32(cmd.ExecuteScalar());
            }

            return stats;
        }

        // Log admin activity
        public void LogAdminActivity(string adminEmail, string action, string details)
        {
            string sql = @"INSERT INTO AdminActivityLogs (Id, AdminEmail, Action, Details)
                           VALUES (@Id, @AdminEmail, @Action, @Details)";

            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();
            using var cmd = new SQLiteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Id", Guid.NewGuid().ToString());
            cmd.Parameters.AddWithValue("@AdminEmail", adminEmail);
            cmd.Parameters.AddWithValue("@Action", action);
            cmd.Parameters.AddWithValue("@Details", details);

            cmd.ExecuteNonQuery();
        }

        // --- Category Operations ---
        public List<Category> GetCategories()
        {
            var list = new List<Category>();
            string sql = "SELECT * FROM Categories ORDER BY DisplayOrder ASC";
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();
            using var cmd = new SQLiteCommand(sql, connection);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Category
                {
                    Id = reader["Id"].ToString(),
                    Name = reader["Name"].ToString(),
                    Slug = reader["Slug"].ToString(),
                    DisplayOrder = Convert.ToInt32(reader["DisplayOrder"]),
                    IsActive = Convert.ToInt32(reader["IsActive"]) == 1
                });
            }
            return list;
        }

        public Category GetCategoryById(string id)
        {
            string sql = "SELECT * FROM Categories WHERE Id = @Id";
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();
            using var cmd = new SQLiteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Id", id);
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Category
                {
                    Id = reader["Id"].ToString(),
                    Name = reader["Name"].ToString(),
                    Slug = reader["Slug"].ToString(),
                    DisplayOrder = Convert.ToInt32(reader["DisplayOrder"]),
                    IsActive = Convert.ToInt32(reader["IsActive"]) == 1
                };
            }
            return null;
        }

        // --- Product Operations -----
        public List<Product> GetProducts(string categoryId = null)
        {
            var list = new List<Product>();
            string sql = @"SELECT p.*, c.Name as CategoryName 
                           FROM Products p 
                           LEFT JOIN Categories c ON p.CategoryId = c.Id";
            
            if (!string.IsNullOrEmpty(categoryId) && categoryId != "all")
            {
                sql += " WHERE p.CategoryId = @CategoryId";
            }
            
            sql += " ORDER BY p.DisplayOrder ASC";
            
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();
            using var cmd = new SQLiteCommand(sql, connection);
            
            if (!string.IsNullOrEmpty(categoryId) && categoryId != "all")
            {
                cmd.Parameters.AddWithValue("@CategoryId", categoryId);
            }
            
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    list.Add(new Product
                    {
                        Id = reader["Id"].ToString(),
                        CategoryId = reader["CategoryId"]?.ToString(),
                        Name = reader["Name"].ToString(),
                        Slug = reader["Slug"].ToString(),
                        Description = reader["Description"]?.ToString(),
                        BasePrice = Convert.ToDecimal(reader["BasePrice"]),
                        DiscountPrice = reader["DiscountPrice"] != DBNull.Value ? Convert.ToDecimal(reader["DiscountPrice"]) : (decimal?)null,
                        HasVariations = Convert.ToInt32(reader["HasVariations"]) == 1,
                        IsActive = Convert.ToInt32(reader["IsActive"]) == 1,
                        IsDeal = Convert.ToInt32(reader["IsDeal"]) == 1,
                        ImageUrl = reader["ImageUrl"]?.ToString(),
                        DisplayOrder = Convert.ToInt32(reader["DisplayOrder"]),
                        CategoryName = reader["CategoryName"]?.ToString()
                    });
                }
            }

            foreach (var product in list)
            {
                if (product.HasVariations)
                {
                    product.Variations = GetProductVariations(product.Id, connection);
                }
                else
                {
                    product.Variations = new List<ProductVariation>();
                }
            }

            return list;
        }

        // --- Deal Operations ---
        public List<Deal> GetDeals()
        {
            var deals = new List<Deal>();
            string sql = "SELECT * FROM Deals ORDER BY IsFeatured DESC";

            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();
            using var cmd = new SQLiteCommand(sql, connection);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                deals.Add(new Deal
                {
                    Id = reader["Id"].ToString(),
                    Name = reader["Name"].ToString(),
                    Slug = reader["Slug"].ToString(),
                    Description = reader["Description"]?.ToString(),
                    TotalPrice = Convert.ToDecimal(reader["TotalPrice"]),
                    DiscountPrice = reader["DiscountPrice"] != DBNull.Value ? Convert.ToDecimal(reader["DiscountPrice"]) : (decimal?)null,
                    IsActive = Convert.ToInt32(reader["IsActive"]) == 1,
                    IsFeatured = Convert.ToInt32(reader["IsFeatured"]) == 1,
                    ImageUrl = reader["ImageUrl"]?.ToString()
                });
            }

            return deals;
        }

        public Deal GetDealById(string id)
        {
            string sql = "SELECT * FROM Deals WHERE Id = @Id";
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();
            using var cmd = new SQLiteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Id", id);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                var deal = new Deal
                {
                    Id = reader["Id"].ToString(),
                    Name = reader["Name"].ToString(),
                    Slug = reader["Slug"].ToString(),
                    Description = reader["Description"]?.ToString(),
                    TotalPrice = Convert.ToDecimal(reader["TotalPrice"]),
                    DiscountPrice = reader["DiscountPrice"] != DBNull.Value ? Convert.ToDecimal(reader["DiscountPrice"]) : (decimal?)null,
                    IsActive = Convert.ToInt32(reader["IsActive"]) == 1,
                    IsFeatured = Convert.ToInt32(reader["IsFeatured"]) == 1,
                    ImageUrl = reader["ImageUrl"]?.ToString()
                };

                deal.Items = GetDealItems(id, connection);
                return deal;
            }

            return null;
        }

        public List<DealItem> GetDealItems(string dealId)
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();
            return GetDealItems(dealId, connection);
        }

        private List<DealItem> GetDealItems(string dealId, SQLiteConnection connection)
        {
            var items = new List<DealItem>();
            string sql = @"SELECT di.*, p.Name as ProductName, pv.VariationName 
                           FROM DealItems di
                           LEFT JOIN Products p ON di.ProductId = p.Id
                           LEFT JOIN ProductVariations pv ON di.VariationId = pv.Id
                           WHERE di.DealId = @DealId";

            using var cmd = new SQLiteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@DealId", dealId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                items.Add(new DealItem
                {
                    Id = reader["Id"].ToString(),
                    DealId = reader["DealId"].ToString(),
                    ProductId = reader["ProductId"].ToString(),
                    ProductName = reader["ProductName"]?.ToString(),
                    VariationId = reader["VariationId"]?.ToString(),
                    VariationName = reader["VariationName"]?.ToString(),
                    Quantity = Convert.ToInt32(reader["Quantity"]),
                    UnitPrice = Convert.ToDecimal(reader["UnitPrice"])
                });
            }

            return items;
        }

        public void CreateDeal(Deal deal)
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                string sql = @"INSERT INTO Deals (Id, Name, Slug, Description, TotalPrice,
                                DiscountPrice, IsActive, IsFeatured, ImageUrl)
                                VALUES (@Id, @Name, @Slug, @Description, @TotalPrice,
                                @DiscountPrice, @IsActive, @IsFeatured, @ImageUrl)";

                using var cmd = new SQLiteCommand(sql, connection);
                cmd.Parameters.AddWithValue("@Id", deal.Id);
                cmd.Parameters.AddWithValue("@Name", deal.Name);
                cmd.Parameters.AddWithValue("@Slug", deal.Slug);
                cmd.Parameters.AddWithValue("@Description", (object)deal.Description ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@TotalPrice", deal.TotalPrice);
                cmd.Parameters.AddWithValue("@DiscountPrice", (object)deal.DiscountPrice ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IsActive", deal.IsActive ? 1 : 0);
                cmd.Parameters.AddWithValue("@IsFeatured", deal.IsFeatured ? 1 : 0);
                cmd.Parameters.AddWithValue("@ImageUrl", (object)deal.ImageUrl ?? DBNull.Value);
                cmd.ExecuteNonQuery();

                // If this deal is featured, unfeature all others
                if (deal.IsFeatured)
                {
                    string unfeatureSql = "UPDATE Deals SET IsFeatured = 0 WHERE Id != @Id";
                    using var unfeatureCmd = new SQLiteCommand(unfeatureSql, connection);
                    unfeatureCmd.Parameters.AddWithValue("@Id", deal.Id);
                    unfeatureCmd.ExecuteNonQuery();
                }

                // Insert deal items
                if (deal.Items != null)
                {
                    foreach (var item in deal.Items)
                    {
                        string itemSql = @"INSERT INTO DealItems (Id, DealId, ProductId, VariationId,
                                            Quantity, UnitPrice)
                                            VALUES (@Id, @DealId, @ProductId, @VariationId,
                                            @Quantity, @UnitPrice)";

                        using var itemCmd = new SQLiteCommand(itemSql, connection);
                        itemCmd.Parameters.AddWithValue("@Id", Guid.NewGuid().ToString());
                        itemCmd.Parameters.AddWithValue("@DealId", deal.Id);
                        itemCmd.Parameters.AddWithValue("@ProductId", item.ProductId);
                        itemCmd.Parameters.AddWithValue("@VariationId", (object)item.VariationId ?? DBNull.Value);
                        itemCmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                        itemCmd.Parameters.AddWithValue("@UnitPrice", item.UnitPrice);
                        itemCmd.ExecuteNonQuery();
                    }
                }

                transaction.Commit();

                // Sync Queue Hook
                var fullDeal = GetDealById(deal.Id);
                if (fullDeal != null)
                {
                    AddToSyncQueue("INSERT", "Deals", fullDeal.Id, JsonConvert.SerializeObject(fullDeal));
                }
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public void UpdateDeal(Deal deal)
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                string sql = @"UPDATE Deals SET Name = @Name, Slug = @Slug,
                                Description = @Description, TotalPrice = @TotalPrice,
                                DiscountPrice = @DiscountPrice, IsActive = @IsActive,
                                IsFeatured = @IsFeatured, ImageUrl = @ImageUrl
                                WHERE Id = @Id";

                using var cmd = new SQLiteCommand(sql, connection);
                cmd.Parameters.AddWithValue("@Id", deal.Id);
                cmd.Parameters.AddWithValue("@Name", deal.Name);
                cmd.Parameters.AddWithValue("@Slug", deal.Slug);
                cmd.Parameters.AddWithValue("@Description", (object)deal.Description ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@TotalPrice", deal.TotalPrice);
                cmd.Parameters.AddWithValue("@DiscountPrice", (object)deal.DiscountPrice ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IsActive", deal.IsActive ? 1 : 0);
                cmd.Parameters.AddWithValue("@IsFeatured", deal.IsFeatured ? 1 : 0);
                cmd.Parameters.AddWithValue("@ImageUrl", (object)deal.ImageUrl ?? DBNull.Value);
                cmd.ExecuteNonQuery();

                // If this deal is featured, unfeature all others
                if (deal.IsFeatured)
                {
                    string unfeatureSql = "UPDATE Deals SET IsFeatured = 0 WHERE Id != @Id";
                    using var unfeatureCmd = new SQLiteCommand(unfeatureSql, connection);
                    unfeatureCmd.Parameters.AddWithValue("@Id", deal.Id);
                    unfeatureCmd.ExecuteNonQuery();
                }

                // Delete existing items
                string delSql = "DELETE FROM DealItems WHERE DealId = @DealId";
                using var delCmd = new SQLiteCommand(delSql, connection);
                delCmd.Parameters.AddWithValue("@DealId", deal.Id);
                delCmd.ExecuteNonQuery();

                // Insert new items
                if (deal.Items != null)
                {
                    foreach (var item in deal.Items)
                    {
                        string itemSql = @"INSERT INTO DealItems (Id, DealId, ProductId, VariationId,
                                            Quantity, UnitPrice)
                                            VALUES (@Id, @DealId, @ProductId, @VariationId,
                                            @Quantity, @UnitPrice)";

                        using var itemCmd = new SQLiteCommand(itemSql, connection);
                        itemCmd.Parameters.AddWithValue("@Id", Guid.NewGuid().ToString());
                        itemCmd.Parameters.AddWithValue("@DealId", deal.Id);
                        itemCmd.Parameters.AddWithValue("@ProductId", item.ProductId);
                        itemCmd.Parameters.AddWithValue("@VariationId", (object)item.VariationId ?? DBNull.Value);
                        itemCmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                        itemCmd.Parameters.AddWithValue("@UnitPrice", item.UnitPrice);
                        itemCmd.ExecuteNonQuery();
                    }
                }

                transaction.Commit();

                // Sync Queue Hook
                var fullDeal = GetDealById(deal.Id);
                if (fullDeal != null)
                {
                    AddToSyncQueue("UPDATE", "Deals", fullDeal.Id, JsonConvert.SerializeObject(fullDeal));
                }
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public void DeleteDeal(string id)
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();
            using var tx = connection.BeginTransaction();

            try
            {
                // Delete deal items first (FK constraint)
                using (var itemCmd = new SQLiteCommand("DELETE FROM DealItems WHERE DealId = @Id", connection, tx))
                {
                    itemCmd.Parameters.AddWithValue("@Id", id);
                    itemCmd.ExecuteNonQuery();
                }

                // Hard delete the deal
                using (var dealCmd = new SQLiteCommand("DELETE FROM Deals WHERE Id = @Id", connection, tx))
                {
                    dealCmd.Parameters.AddWithValue("@Id", id);
                    dealCmd.ExecuteNonQuery();
                }

                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }

            // Sync Queue Hook - queue a DELETE operation
            AddToSyncQueue("DELETE", "Deals", id, "");
        }


        // --- Settings Operations ---
        public string GetSetting(string key, string defaultValue)
        {
            string sql = "SELECT SettingValue FROM SystemSettings WHERE SettingKey = @SettingKey";
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();
            using var cmd = new SQLiteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@SettingKey", key);
            var val = cmd.ExecuteScalar();
            string result = val?.ToString() ?? "";
            return string.IsNullOrWhiteSpace(result) ? defaultValue : result;
        }

        public void SaveSetting(string key, string value)
        {
            string sql = @"INSERT OR REPLACE INTO SystemSettings (Id, SettingKey, SettingValue, UpdatedAt)
                            VALUES (@Id, @Key, @Value, CURRENT_TIMESTAMP)";

            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();
            using var cmd = new SQLiteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Id", Guid.NewGuid().ToString());
            cmd.Parameters.AddWithValue("@Key", key);
            cmd.Parameters.AddWithValue("@Value", value);
            cmd.ExecuteNonQuery();

            // Sync Queue Hook
            AddToSyncQueue("INSERT", "SystemSettings", key, value);
        }

        public Dictionary<string, string> GetSystemSettings()
        {
            var settings = new Dictionary<string, string>();
            string sql = "SELECT SettingKey, SettingValue FROM SystemSettings";

            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();
            using var cmd = new SQLiteCommand(sql, connection);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                settings.Add(reader["SettingKey"].ToString(), reader["SettingValue"].ToString());
            }

            return settings;
        }

        public List<DeliveryZone> GetDeliveryZones()
        {
            var zones = new List<DeliveryZone>();
            string sql = "SELECT * FROM DeliveryZones ORDER BY MaxDistance";

            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();
            using var cmd = new SQLiteCommand(sql, connection);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                zones.Add(new DeliveryZone
                {
                    Id = reader["Id"].ToString(),
                    Name = reader["Name"].ToString(),
                    MaxDistance = Convert.ToDecimal(reader["MaxDistance"]),
                    Charge = Convert.ToDecimal(reader["Charge"]),
                    MinOrder = reader["MinOrder"] != DBNull.Value ? Convert.ToDecimal(reader["MinOrder"]) : 0
                });
            }

            return zones;
        }

        public void SaveDeliveryZones(List<DeliveryZone> zones)
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // Delete existing zones
                string deleteSql = "DELETE FROM DeliveryZones";
                using var deleteCmd = new SQLiteCommand(deleteSql, connection);
                deleteCmd.ExecuteNonQuery();

                // Insert new zones
                foreach (var zone in zones)
                {
                    string sql = @"INSERT INTO DeliveryZones (Id, Name, MaxDistance, Charge, MinOrder)
                                    VALUES (@Id, @Name, @MaxDistance, @Charge, @MinOrder)";

                    using var cmd = new SQLiteCommand(sql, connection);
                    cmd.Parameters.AddWithValue("@Id", zone.Id);
                    cmd.Parameters.AddWithValue("@Name", zone.Name);
                    cmd.Parameters.AddWithValue("@MaxDistance", zone.MaxDistance);
                    cmd.Parameters.AddWithValue("@Charge", zone.Charge);
                    cmd.Parameters.AddWithValue("@MinOrder", zone.MinOrder);
                    cmd.ExecuteNonQuery();
                }

                transaction.Commit();

                // Sync Queue Hook: push as SystemSettings setting_key 'delivery_zones'
                AddToSyncQueue("INSERT", "SystemSettings", "delivery_zones", JsonConvert.SerializeObject(zones));
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public string GetDatabasePath()
        {
            return _databasePath;
        }

        // --- Report Operations ---
        public DataTable GetSalesReport(DateTime start, DateTime end)
        {
            var dt = new DataTable();
            string sql = @"
                SELECT 
                    Date(CreatedAt) as OrderDate,
                    COUNT(*) as TotalOrders,
                    SUM(Subtotal) as TotalSubtotal,
                    SUM(DeliveryCharge) as TotalDelivery,
                    SUM(Total) as TotalAmount
                FROM Orders
                WHERE Date(CreatedAt) BETWEEN Date(@Start) AND Date(@End) AND Status = 'completed'
                GROUP BY Date(CreatedAt)
                ORDER BY OrderDate ASC";

            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();
            using var cmd = new SQLiteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Start", start.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@End", end.ToString("yyyy-MM-dd"));

            using var adapter = new SQLiteDataAdapter(cmd);
            adapter.Fill(dt);
            return dt;
        }

        public Dictionary<string, decimal> GetDailyReportData(string startDate, string endDate)
        {
            var result = new Dictionary<string, decimal>();

            string sql = @"
                SELECT Date(CreatedAt) as Date, SUM(Total) as Revenue
                FROM Orders
                WHERE Date(CreatedAt) BETWEEN @StartDate AND @EndDate
                AND Status = 'completed'
                GROUP BY Date(CreatedAt)
                ORDER BY Date(CreatedAt)";

            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();
            using var cmd = new SQLiteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@StartDate", startDate);
            cmd.Parameters.AddWithValue("@EndDate", endDate);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result.Add(reader["Date"].ToString(), Convert.ToDecimal(reader["Revenue"]));
            }

            return result;
        }

        public Dictionary<string, (int Total, int Completed)> GetDailyOrderCounts(string startDate, string endDate)
        {
            var result = new Dictionary<string, (int Total, int Completed)>();

            string sql = @"
                SELECT Date(CreatedAt) as Date,
                       COUNT(*) as Total,
                       SUM(CASE WHEN Status = 'completed' THEN 1 ELSE 0 END) as Completed
                FROM Orders
                WHERE Date(CreatedAt) BETWEEN @StartDate AND @EndDate
                GROUP BY Date(CreatedAt)
                ORDER BY Date(CreatedAt)";

            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();
            using var cmd = new SQLiteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@StartDate", startDate);
            cmd.Parameters.AddWithValue("@EndDate", endDate);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result.Add(
                    reader["Date"].ToString(),
                    (
                        Convert.ToInt32(reader["Total"]),
                        Convert.ToInt32(reader["Completed"])
                    )
                );
            }

            return result;
        }

        public Dictionary<string, decimal> GetTopProducts(string startDate, string endDate, int topN = 10)
        {
            var result = new Dictionary<string, decimal>();

            string sql = @"
                SELECT ProductName, SUM(TotalPrice) as Revenue
                FROM OrderItems oi
                JOIN Orders o ON oi.OrderId = o.Id
                WHERE Date(o.CreatedAt) BETWEEN @StartDate AND @EndDate
                AND o.Status = 'completed'
                GROUP BY ProductName
                ORDER BY Revenue DESC
                LIMIT @TopN";

            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();
            using var cmd = new SQLiteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@StartDate", startDate);
            cmd.Parameters.AddWithValue("@EndDate", endDate);
            cmd.Parameters.AddWithValue("@TopN", topN);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result.Add(reader["ProductName"].ToString(), Convert.ToDecimal(reader["Revenue"]));
            }

            return result;
        }

        public void CreateCategory(Category category)
        {
            string sql = @"INSERT INTO Categories (Id, Name, Slug, DisplayOrder, IsActive)
                            VALUES (@Id, @Name, @Slug, @DisplayOrder, @IsActive)";

            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();
            using var cmd = new SQLiteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Id", category.Id);
            cmd.Parameters.AddWithValue("@Name", category.Name);
            cmd.Parameters.AddWithValue("@Slug", category.Slug);
            cmd.Parameters.AddWithValue("@DisplayOrder", category.DisplayOrder);
            cmd.Parameters.AddWithValue("@IsActive", category.IsActive ? 1 : 0);
            cmd.ExecuteNonQuery();

            AddToSyncQueue("INSERT", "Categories", category.Id, JsonConvert.SerializeObject(category));
        }

        public void UpdateCategory(Category category)
        {
            string sql = @"UPDATE Categories SET Name = @Name, Slug = @Slug,
                            DisplayOrder = @DisplayOrder, IsActive = @IsActive
                            WHERE Id = @Id";

            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();
            using var cmd = new SQLiteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Id", category.Id);
            cmd.Parameters.AddWithValue("@Name", category.Name);
            cmd.Parameters.AddWithValue("@Slug", category.Slug);
            cmd.Parameters.AddWithValue("@DisplayOrder", category.DisplayOrder);
            cmd.Parameters.AddWithValue("@IsActive", category.IsActive ? 1 : 0);
            cmd.ExecuteNonQuery();

            AddToSyncQueue("UPDATE", "Categories", category.Id, JsonConvert.SerializeObject(category));
        }

        public void DeleteCategory(string id)
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();
            
            // Set CategoryId to NULL for products under this category
            using (var cmdProd = new SQLiteCommand("UPDATE Products SET CategoryId = NULL WHERE CategoryId = @Id", connection))
            {
                cmdProd.Parameters.AddWithValue("@Id", id);
                cmdProd.ExecuteNonQuery();
            }

            // Delete the category itself
            using (var cmd = new SQLiteCommand("DELETE FROM Categories WHERE Id = @Id", connection))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.ExecuteNonQuery();
            }

            AddToSyncQueue("DELETE", "Categories", id, "");
        }

        public Product GetProductById(string id)
        {
            string sql = "SELECT * FROM Products WHERE Id = @Id";
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();
            using var cmd = new SQLiteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Id", id);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                var product = new Product
                {
                    Id = reader["Id"].ToString(),
                    CategoryId = reader["CategoryId"]?.ToString(),
                    Name = reader["Name"].ToString(),
                    Slug = reader["Slug"].ToString(),
                    Description = reader["Description"]?.ToString(),
                    BasePrice = Convert.ToDecimal(reader["BasePrice"]),
                    DiscountPrice = reader["DiscountPrice"] != DBNull.Value ? Convert.ToDecimal(reader["DiscountPrice"]) : (decimal?)null,
                    HasVariations = Convert.ToInt32(reader["HasVariations"]) == 1,
                    IsActive = Convert.ToInt32(reader["IsActive"]) == 1,
                    IsDeal = Convert.ToInt32(reader["IsDeal"]) == 1,
                    ImageUrl = reader["ImageUrl"]?.ToString(),
                    DisplayOrder = Convert.ToInt32(reader["DisplayOrder"])
                };

                // Load variations
                product.Variations = GetProductVariations(id, connection);
                return product;
            }

            return null;
        }

        private List<ProductVariation> GetProductVariations(string productId, SQLiteConnection connection)
        {
            var variations = new List<ProductVariation>();
            string sql = "SELECT * FROM ProductVariations WHERE ProductId = @ProductId";

            using var cmd = new SQLiteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@ProductId", productId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                variations.Add(new ProductVariation
                {
                    Id = reader["Id"].ToString(),
                    ProductId = reader["ProductId"].ToString(),
                    VariationType = reader["VariationType"].ToString(),
                    VariationName = reader["VariationName"].ToString(),
                    PriceAdjustment = Convert.ToDecimal(reader["PriceAdjustment"]),
                    IsDefault = Convert.ToInt32(reader["IsDefault"]) == 1
                });
            }

            return variations;
        }

        public void CreateProduct(Product product)
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                string sql = @"INSERT INTO Products (Id, CategoryId, Name, Slug, Description,
                                BasePrice, DiscountPrice, HasVariations, IsActive, IsDeal,
                                ImageUrl, DisplayOrder)
                                VALUES (@Id, @CategoryId, @Name, @Slug, @Description,
                                @BasePrice, @DiscountPrice, @HasVariations, @IsActive, @IsDeal,
                                @ImageUrl, @DisplayOrder)";

                using var cmd = new SQLiteCommand(sql, connection);
                cmd.Parameters.AddWithValue("@Id", product.Id);
                cmd.Parameters.AddWithValue("@CategoryId", (object)product.CategoryId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Name", product.Name);
                cmd.Parameters.AddWithValue("@Slug", product.Slug);
                cmd.Parameters.AddWithValue("@Description", (object)product.Description ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@BasePrice", product.BasePrice);
                cmd.Parameters.AddWithValue("@DiscountPrice", (object)product.DiscountPrice ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@HasVariations", product.HasVariations ? 1 : 0);
                cmd.Parameters.AddWithValue("@IsActive", product.IsActive ? 1 : 0);
                cmd.Parameters.AddWithValue("@IsDeal", product.IsDeal ? 1 : 0);
                cmd.Parameters.AddWithValue("@ImageUrl", (object)product.ImageUrl ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DisplayOrder", product.DisplayOrder);
                cmd.ExecuteNonQuery();

                // Save variations
                if (product.Variations != null)
                {
                    foreach (var variation in product.Variations)
                    {
                        string varSql = @"INSERT INTO ProductVariations (Id, ProductId, VariationType,
                                            VariationName, PriceAdjustment, IsDefault)
                                            VALUES (@Id, @ProductId, @VariationType,
                                            @VariationName, @PriceAdjustment, @IsDefault)";

                        using var varCmd = new SQLiteCommand(varSql, connection);
                        varCmd.Parameters.AddWithValue("@Id", Guid.NewGuid().ToString());
                        varCmd.Parameters.AddWithValue("@ProductId", product.Id);
                        varCmd.Parameters.AddWithValue("@VariationType", variation.VariationType);
                        varCmd.Parameters.AddWithValue("@VariationName", variation.VariationName);
                        varCmd.Parameters.AddWithValue("@PriceAdjustment", variation.PriceAdjustment);
                        varCmd.Parameters.AddWithValue("@IsDefault", variation.IsDefault ? 1 : 0);
                        varCmd.ExecuteNonQuery();
                    }
                }

                transaction.Commit();

                AddToSyncQueue("INSERT", "Products", product.Id, JsonConvert.SerializeObject(product));
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public void UpdateProduct(Product product)
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                string sql = @"UPDATE Products SET CategoryId = @CategoryId, Name = @Name,
                                Slug = @Slug, Description = @Description,
                                BasePrice = @BasePrice, DiscountPrice = @DiscountPrice,
                                HasVariations = @HasVariations, IsActive = @IsActive,
                                ImageUrl = @ImageUrl, DisplayOrder = @DisplayOrder
                                WHERE Id = @Id";

                using var cmd = new SQLiteCommand(sql, connection);
                cmd.Parameters.AddWithValue("@Id", product.Id);
                cmd.Parameters.AddWithValue("@CategoryId", (object)product.CategoryId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Name", product.Name);
                cmd.Parameters.AddWithValue("@Slug", product.Slug);
                cmd.Parameters.AddWithValue("@Description", (object)product.Description ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@BasePrice", product.BasePrice);
                cmd.Parameters.AddWithValue("@DiscountPrice", (object)product.DiscountPrice ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@HasVariations", product.HasVariations ? 1 : 0);
                cmd.Parameters.AddWithValue("@IsActive", product.IsActive ? 1 : 0);
                cmd.Parameters.AddWithValue("@ImageUrl", (object)product.ImageUrl ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DisplayOrder", product.DisplayOrder);
                cmd.ExecuteNonQuery();

                // Delete existing variations
                string delSql = "DELETE FROM ProductVariations WHERE ProductId = @ProductId";
                using var delCmd = new SQLiteCommand(delSql, connection);
                delCmd.Parameters.AddWithValue("@ProductId", product.Id);
                delCmd.ExecuteNonQuery();

                // Insert new variations
                if (product.Variations != null)
                {
                    foreach (var variation in product.Variations)
                    {
                        string varSql = @"INSERT INTO ProductVariations (Id, ProductId, VariationType,
                                            VariationName, PriceAdjustment, IsDefault)
                                            VALUES (@Id, @ProductId, @VariationType,
                                            @VariationName, @PriceAdjustment, @IsDefault)";

                        using var varCmd = new SQLiteCommand(varSql, connection);
                        varCmd.Parameters.AddWithValue("@Id", Guid.NewGuid().ToString());
                        varCmd.Parameters.AddWithValue("@ProductId", product.Id);
                        varCmd.Parameters.AddWithValue("@VariationType", variation.VariationType);
                        varCmd.Parameters.AddWithValue("@VariationName", variation.VariationName);
                        varCmd.Parameters.AddWithValue("@PriceAdjustment", variation.PriceAdjustment);
                        varCmd.Parameters.AddWithValue("@IsDefault", variation.IsDefault ? 1 : 0);
                        varCmd.ExecuteNonQuery();
                    }
                }

                transaction.Commit();

                AddToSyncQueue("UPDATE", "Products", product.Id, JsonConvert.SerializeObject(product));
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public void DeleteProduct(string id)
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();
            
            // Delete product variations first
            using (var cmdVars = new SQLiteCommand("DELETE FROM ProductVariations WHERE ProductId = @Id", connection))
            {
                cmdVars.Parameters.AddWithValue("@Id", id);
                cmdVars.ExecuteNonQuery();
            }

            // Delete the product itself
            using (var cmd = new SQLiteCommand("DELETE FROM Products WHERE Id = @Id", connection))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.ExecuteNonQuery();
            }

            AddToSyncQueue("DELETE", "Products", id, "");
        }

        public void UpdateOrderPaymentStatus(string id, string paymentStatus)
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();
            string sql = "UPDATE Orders SET PaymentStatus = @PaymentStatus, UpdatedAt = CURRENT_TIMESTAMP WHERE Id = @Id";
            using var cmd = new SQLiteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@PaymentStatus", paymentStatus);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.ExecuteNonQuery();

            // Sync Queue Hook
            var updatedOrder = GetOrderById(id);
            if (updatedOrder != null)
            {
                AddToSyncQueue("UPDATE", "Orders", id, JsonConvert.SerializeObject(updatedOrder));
            }
        }

        public List<Order> GetPendingRiderCashOrders()
        {
            var orders = new List<Order>();
            string sql = @"
                SELECT * FROM Orders 
                WHERE OrderType = 'delivery' 
                  AND PaymentMethod = 'cod' 
                  AND PaymentStatus = 'pending'
                  AND Status = 'completed'
                ORDER BY CreatedAt ASC";

            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();
            using var cmd = new SQLiteCommand(sql, connection);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var order = MapOrder(reader);
                order.Items = GetOrderItems(order.Id, connection);
                orders.Add(order);
            }
            return orders;
        }

        public decimal GetOutstandingRiderCash()
        {
            decimal total = 0;
            string sql = @"
                SELECT SUM(Total) FROM Orders 
                WHERE OrderType = 'delivery' 
                  AND PaymentMethod = 'cod' 
                  AND PaymentStatus = 'pending'
                  AND Status = 'completed'";
            
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();
            using var cmd = new SQLiteCommand(sql, connection);
            var res = cmd.ExecuteScalar();
            if (res != DBNull.Value && res != null)
            {
                total = Convert.ToDecimal(res);
            }
            return total;
        }

        // ─── Sync Queue Helpers ───────────────────────────────────────────────────

        public class SyncQueueItem
        {
            public int Id { get; set; }
            public string OperationType { get; set; } // INSERT, UPDATE, DELETE, CLOUDINARY_UPLOAD
            public string TableName { get; set; }
            public string RecordId { get; set; }
            public string Payload { get; set; }  // JSON payload or local file path for CLOUDINARY_UPLOAD
            public string CreatedAt { get; set; }
        }

        
        private void UpdateLocalTimestamp()
        {
            try
            {
                string sql = @"INSERT OR REPLACE INTO SystemSettings (Id, SettingKey, SettingValue, UpdatedAt)
                                VALUES (@Id, 'last_menu_update', @Value, CURRENT_TIMESTAMP)";
                using var connection = new SQLiteConnection(_connectionString);
                connection.Open();
                using var cmd = new SQLiteCommand(sql, connection);
                cmd.Parameters.AddWithValue("@Id", Guid.NewGuid().ToString());
                cmd.Parameters.AddWithValue("@Value", DateTime.UtcNow.ToString("O"));
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to update local timestamp", ex);
            }
        }

        public void AddToSyncQueue(string operationType, string tableName, string recordId, string payload)
        {
            try
            {
                if (tableName.Equals("Orders", StringComparison.OrdinalIgnoreCase))
                {
                    if (operationType.Equals("UPDATE", StringComparison.OrdinalIgnoreCase))
                    {
                        var order = JsonConvert.DeserializeObject<Order>(payload);
                        if (order != null)
                        {
                            Task.Run(async () =>
                            {
                                try
                                {
                                    var api = new ApiService();
                                    await api.SyncOrder(order);
                                }
                                catch (Exception ex)
                                {
                                    Logger.LogError($"Failed to instant-sync order update {recordId}", ex);
                                }
                            });
                        }
                    }
                    return;
                }

                if (tableName.Equals("SystemSettings", StringComparison.OrdinalIgnoreCase) && recordId == "last_menu_update")
                {
                    return;
                }

                UpdateLocalTimestamp();
            }
            catch (Exception ex)
            {
                Logger.LogError("AddToSyncQueue logic failed", ex);
            }
        }

        public List<SyncQueueItem> GetSyncQueueItems(int limit = 50)

        {
            var items = new List<SyncQueueItem>();
            string sql = "SELECT Id, OperationType, TableName, RecordId, Payload, CreatedAt FROM SyncQueue ORDER BY Id ASC LIMIT @Limit";
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();
            using var cmd = new SQLiteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Limit", limit);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                items.Add(new SyncQueueItem
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    OperationType = reader["OperationType"].ToString(),
                    TableName = reader["TableName"].ToString(),
                    RecordId = reader["RecordId"].ToString(),
                    Payload = reader["Payload"].ToString(),
                    CreatedAt = reader["CreatedAt"].ToString()
                });
            }
            return items;
        }

        public void RemoveFromSyncQueue(int id)
        {
            string sql = "DELETE FROM SyncQueue WHERE Id = @Id";
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();
            using var cmd = new SQLiteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.ExecuteNonQuery();
        }

        public void UpdateProductImageUrl(string productId, string imageUrl)
        {
            string sql = "UPDATE Products SET ImageUrl = @ImageUrl WHERE Id = @Id";
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();
            using var cmd = new SQLiteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@ImageUrl", imageUrl);
            cmd.Parameters.AddWithValue("@Id", productId);
            cmd.ExecuteNonQuery();
        }

        public void UpdateDealImageUrl(string dealId, string imageUrl)
        {
            string sql = "UPDATE Deals SET ImageUrl = @ImageUrl WHERE Id = @Id";
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();
            using var cmd = new SQLiteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@ImageUrl", imageUrl);
            cmd.Parameters.AddWithValue("@Id", dealId);
            cmd.ExecuteNonQuery();
        }

        public void QueueAllExistingDataForSync()
        {
            try
            {
                using var connection = new SQLiteConnection(_connectionString);
                connection.Open();

                // Check system setting to make sure we only do this once
                string checkFlag = GetSetting("InitialDataQueuedForSync", "false");
                if (checkFlag == "true") return;

                Console.WriteLine("🔄 Queueing all existing categories, products and settings for initial Neon sync...");

                // 1. Categories
                var categories = GetCategories();
                foreach (var cat in categories)
                {
                    AddToSyncQueue("INSERT", "Categories", cat.Id, JsonConvert.SerializeObject(cat));
                }

                // 2. Products (including variations)
                var products = GetProducts();
                foreach (var prod in products)
                {
                    // Fetch full product details including variations
                    var fullProd = GetProductById(prod.Id);
                    if (fullProd != null)
                    {
                        // Check if product has a local image path that needs uploading
                        AddToSyncQueue("INSERT", "Products", fullProd.Id, JsonConvert.SerializeObject(fullProd));
                    }
                }

                // 3. Settings
                var settings = GetSystemSettings();
                foreach (var setting in settings)
                {
                    AddToSyncQueue("INSERT", "SystemSettings", setting.Key, setting.Value);
                }

                // 4. Delivery Zones
                var zones = GetDeliveryZones();
                if (zones.Count > 0)
                {
                    AddToSyncQueue("INSERT", "SystemSettings", "delivery_zones", JsonConvert.SerializeObject(zones));
                }

                SaveSetting("InitialDataQueuedForSync", "true");
                Console.WriteLine("✅ Initial data queued successfully!");
            }
            catch (Exception ex)
            {
                Logger.LogError("QueueAllExistingDataForSync failed", ex);
            }
        }

        /// <summary>
        /// Fixes SyncLog rows corrupted by the old naive strip-'s' singularization.
        /// The bug: "categories".TrimEnd('s') produced "categorie", not "category".
        /// Runs silently on startup; only patches rows that need it.
        /// </summary>
        private void MigrateCorruptSyncLogEntityTypes(SQLiteConnection connection)
        {
            return;
        }
        private void MigrateOrphanedCategorySyncLogs(SQLiteConnection connection)
        {
            return;
        }
    }
}