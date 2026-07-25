// E:\hungryHub\hungry-fast-food\admin-panel\HungryFastFoodAdmin\Models\Product.cs

using System.Collections.Generic;

namespace HungryFastFoodAdmin.Models
{
    public class Category
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
    }

    public class Product
    {
        public string Id { get; set; }
        public string CategoryId { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public string Description { get; set; }
        public decimal BasePrice { get; set; }
        public decimal? DiscountPrice { get; set; }
        public bool HasVariations { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeal { get; set; }
        public string ImageUrl { get; set; }
        public int DisplayOrder { get; set; }
        public string CategoryName { get; set; }
        public List<ProductVariation> Variations { get; set; } = new List<ProductVariation>();
    }

    public class ProductVariation
    {
        public string Id { get; set; }
        public string ProductId { get; set; }
        public string VariationType { get; set; } // size, flavor, option
        public string VariationName { get; set; }
        public decimal PriceAdjustment { get; set; }
        public bool IsDefault { get; set; }
    }

    public class Deal
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public string Description { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal? DiscountPrice { get; set; }
        public bool IsActive { get; set; }
        public bool IsFeatured { get; set; }
        public string ImageUrl { get; set; }
        public List<DealItem> Items { get; set; } = new List<DealItem>();
    }

    public class DealItem
    {
        public string Id { get; set; }
        public string DealId { get; set; }
        public string ProductId { get; set; }
        public string VariationId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string ProductName { get; set; }
        public string VariationName { get; set; }
    }
}