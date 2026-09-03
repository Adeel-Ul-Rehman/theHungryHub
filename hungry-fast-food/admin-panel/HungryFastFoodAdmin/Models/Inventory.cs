using System;

namespace HungryFastFoodAdmin.Models
{
    public class RawMaterial
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Category { get; set; } = "General";
        public double CurrentStock { get; set; }
        public string Unit { get; set; } = "units"; // g, kg, ml, L, units
        public double MinThreshold { get; set; }
        public decimal CostPerUnit { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.Now;

        public string StatusBadge
        {
            get
            {
                if (CurrentStock <= 0) return "OUT OF STOCK";
                if (CurrentStock <= MinThreshold) return "LOW STOCK";
                return "IN STOCK";
            }
        }
    }

    public class ProductRecipe
    {
        public int Id { get; set; }
        public string ProductName { get; set; } = "";
        public string VariationName { get; set; } = "";
        public int RawMaterialId { get; set; }
        public string RawMaterialName { get; set; } = "";
        public double RequiredQuantity { get; set; }
        public string Unit { get; set; } = "";
    }

    public class InventoryLog
    {
        public int Id { get; set; }
        public string RawMaterialName { get; set; } = "";
        public double ChangeAmount { get; set; }
        public string Type { get; set; } = "deduction_order"; // deduction_order, manual_restock, adjustment
        public string ReferenceId { get; set; } = ""; // e.g. Order # or Invoice #
        public string Notes { get; set; } = "";
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
