// E:\hungryHub\hungry-fast-food\admin-panel\HungryFastFoodAdmin\Models\User.cs

namespace HungryFastFoodAdmin.Models
{
    public class User
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public bool IsVerified { get; set; }
        public string GoogleId { get; set; }
        public bool IsGuest { get; set; }
        public string CreatedAt { get; set; }
    }
}
