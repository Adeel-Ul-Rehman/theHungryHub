using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using HungryFastFoodAdmin.Forms;
using HungryFastFoodAdmin.Services;

namespace HungryFastFoodAdmin
{
    internal static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("🚀 STARTING AUTOMATED SYNC RUNNER");
            try
            {
                // Debug config
                string apiKey = ConfigManager.GetAppSetting("AdminApiKey", "NOT_FOUND");
                string apiUrl = ConfigManager.GetAppSetting("ApiBaseUrl", "NOT_FOUND");
                Console.WriteLine($"🔍 API URL: {apiUrl}");
                Console.WriteLine($"🔍 API Key (first 10): {apiKey?.Substring(0, Math.Min(10, apiKey?.Length ?? 0))}...");

                // 1. Initialize database and clear old sync logs for full sync
                var dbService = new DatabaseService();
                dbService.InitializeDatabase();
                dbService.ClearAllSyncLogs();

                Console.WriteLine("=== SQLite Categories ===");
                var cats = dbService.GetCategories();
                foreach (var c in cats) Console.WriteLine($"Cat: {c.Id} - {c.Name} ({c.Slug})");

                Console.WriteLine("=== SQLite Products ===");
                var prods = dbService.GetProducts();
                foreach (var p in prods) Console.WriteLine($"Prod: {p.Id} - {p.Name} (Img: {p.ImageUrl})");

                Console.WriteLine("=== SQLite Settings ===");
                var settings = dbService.GetSystemSettings();
                foreach (var s in settings) Console.WriteLine($"Setting: {s.Key} = {s.Value}");

                Console.WriteLine("=== SQLite SyncQueue ===");
                var queue = dbService.GetSyncQueueItems();
                foreach (var q in queue) Console.WriteLine($"Queue Item: {q.Id} | {q.OperationType} | {q.TableName} | {q.RecordId}");

                // 2. Perform API Login to get the JWT token
                var apiService = new ApiService();
                Console.WriteLine("🔑 Logging in to backend...");
                var loginResult = Task.Run(async () => await apiService.AdminLogin("admin@hungryhub.com", "admin123")).GetAwaiter().GetResult();
                
                Console.WriteLine($"🔍 Token after login (null={ApiService.Token == null}): {ApiService.Token?.Substring(0, Math.Min(20, ApiService.Token?.Length ?? 0))}...");

                if (loginResult.Success)
                {
                    Console.WriteLine("✅ Login successful! Token set.");
                    
                    // 3. Trigger manual full sync
                    var syncService = new SyncService();
                    Console.WriteLine("🔄 Running database full sync...");
                    Task.Run(async () => await syncService.PerformFullSyncAsync()).GetAwaiter().GetResult();
                    Console.WriteLine("🎉 Database full sync cycle completed successfully!");
                }
                else
                {
                    Console.WriteLine($"❌ Backend login failed: {loginResult.Message}");
                    Console.WriteLine("💡 Trying full sync anyway (offline fallback login may have been used)...");
                    var syncService = new SyncService();
                    Task.Run(async () => await syncService.PerformFullSyncAsync()).GetAwaiter().GetResult();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 Sync runner failed: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
            finally
            {
                Console.WriteLine("🖥️ Launching WinForms Login Application...");
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new LoginForm());
            }
        }
    }
}