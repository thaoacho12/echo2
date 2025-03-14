namespace ServerApp.DAL.Models
{
    public class SpecificationType
    {
        public int SpecificationTypeId { get; set; } // Primary Key
        public string Name { get; set; } // Tên thông số (vd: Màu sắc, Kích thước)
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual ICollection<ProductSpecification> ProductSpecifications { get; set; }
    }

}