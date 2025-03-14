namespace ServerApp.BLL.Services.ViewModels
{
    public class WishListVm
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ImageUrl { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal DiscountedPrice { get; set; }
        public int DiscountPercentage { get; set; }
        public int ReviewCount { get; set; }
        public DateTime AddedAt { get; set; }
    }
}
