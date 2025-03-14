namespace ServerApp.DAL.Models
{
    public class Brand
    {
        public int BrandId { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public virtual ICollection<Product> Products { get; set; }
        public int? ImageId { get; set; }
        public virtual Image? Image { get; set; }
    }
}
