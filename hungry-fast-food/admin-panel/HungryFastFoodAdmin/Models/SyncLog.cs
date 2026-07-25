// E:\hungryHub\hungry-fast-food\admin-panel\HungryFastFoodAdmin\Models\SyncLog.cs

namespace HungryFastFoodAdmin.Models
{
    public class SyncLog
    {
        public string Id { get; set; }
        public string SyncType { get; set; }
        public string Status { get; set; }
        public int RecordsSynced { get; set; }
        public string ErrorMessage { get; set; }
        public string StartedAt { get; set; }
        public string CompletedAt { get; set; }
    }
}
