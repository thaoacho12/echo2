namespace ServerApp.DAL.Models
{
    public class ProductSpecification
    {
        public int ProductId { get; set; } // FK to Product
        public int SpecificationTypeId { get; set; } // FK to SpecificationType
        public string Value { get; set; } // Giá trị thông số (vd: "Đỏ", "15cm")
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual Product Product { get; set; }
        public virtual SpecificationType SpecificationType { get; set; }
    }

}
