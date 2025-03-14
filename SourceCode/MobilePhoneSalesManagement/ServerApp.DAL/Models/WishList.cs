namespace ServerApp.DAL.Models
{
    public class WishList
    {
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public DateTime AddedAt { get; set; } = DateTime.Now;

        public virtual User User { get; set; }
        public virtual Product Product { get; set; }
    }
}
