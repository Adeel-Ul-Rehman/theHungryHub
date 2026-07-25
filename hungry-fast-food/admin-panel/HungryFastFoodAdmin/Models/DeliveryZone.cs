// E:\hungryHub\hungry-fast-food\admin-panel\HungryFastFoodAdmin\Models\DeliveryZone.cs

namespace HungryFastFoodAdmin.Models
{
    public class DeliveryZone
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public decimal MaxDistance { get; set; }
        public decimal Charge { get; set; }
        public decimal MinOrder { get; set; }
    }
}
