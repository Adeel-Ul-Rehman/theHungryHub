// E:\hungryHub\hungry-fast-food\admin-panel\HungryFastFoodAdmin\Models\Order.cs

using System;
using System.Collections.Generic;

namespace HungryFastFoodAdmin.Models
{
    public class Order
    {
        public string Id { get; set; }
        public string OrderNumber { get; set; }
        public string OrderType { get; set; } // dining, delivery, takeaway
        public string UserId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerEmail { get; set; }
        public string DeliveryAddress { get; set; }
        public double? DeliveryLatitude { get; set; }
        public double? DeliveryLongitude { get; set; }
        public string Status { get; set; } // pending, confirmed, preparing, ready, completed, cancelled
        public decimal Subtotal { get; set; }
        public decimal DeliveryCharge { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }
        public string PaymentMethod { get; set; } // jazzcash, cod, cash
        public string PaymentStatus { get; set; } // pending, completed, failed
        public bool IsSuspicious { get; set; }
        public string AdminNotes { get; set; }
        public string CreatedAt { get; set; }
        public string UpdatedAt { get; set; }
        public string SyncedAt { get; set; }
        public bool IsSynced { get; set; }
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
    }

    public class OrderItem
    {
        public string Id { get; set; }
        public string OrderId { get; set; }
        public string ProductName { get; set; }
        public string VariationName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public bool IsFromDeal { get; set; }
        public string DealId { get; set; }
    }
}