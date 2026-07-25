// E:\hungryHub\hungry-fast-food\admin-panel\HungryFastFoodAdmin\Utils\Constants.cs

namespace HungryFastFoodAdmin
{
    public static class Constants
    {
        public static class OrderStatus
        {
            public const string Pending = "pending";
            public const string Confirmed = "confirmed";
            public const string Preparing = "preparing";
            public const string Ready = "ready";
            public const string Completed = "completed";
            public const string Cancelled = "cancelled";
        }

        public static class OrderType
        {
            public const string Dining = "dining";
            public const string Delivery = "delivery";
            public const string Takeaway = "takeaway";
        }

        public static class PaymentMethod
        {
            public const string JazzCash = "jazzcash";
            public const string Cod = "cod";
            public const string Cash = "cash";
        }

        public static class PaymentStatus
        {
            public const string Pending = "pending";
            public const string Completed = "completed";
            public const string Failed = "failed";
        }
    }
}
