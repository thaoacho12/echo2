using ServerApp.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerApp.BLL.Services.ViewModels
{
    public class ProductDetailVm
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; }  // Ví dụ: tên danh mục của sản phẩm
        public string Brand { get; set; }     // Thương hiệu của sản phẩm
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public int? ImageId { get; set; }
        public virtual Image? Image { get; set; }
    }
}
