namespace ServerApp.DAL.Models
{
    public class Cart
    {
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; } = 1;
        public decimal TotalPrice { get; set; }
        public DateTime AddedAt { get; set; } = DateTime.Now;
        public string Status { get; set; } = "Added";

        public virtual User? User { get; set; }
        public virtual Product? Product { get; set; }
    }
}
