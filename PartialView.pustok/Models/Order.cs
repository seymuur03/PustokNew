namespace PartialView.pustok.Models
{
    public class Order:BaseEntity
    {
        public string ZipCode { get; set; }
        public string City { get; set; }
        public string Town { get; set; }
        public string Address { get; set; }
        public decimal TotalPrice { get; set; }
        public OrderStatus OrderStatus { get; set; } = OrderStatus.Pending;
        public string AppUserId { get; set; }
        public AppUser AppUser { get; set; }
        public List<OrderItem> OrderItems { get; set; }
    }
    public enum OrderStatus
    {
        Pending,
        Accepted,
        Rejected,
        
    }
}
