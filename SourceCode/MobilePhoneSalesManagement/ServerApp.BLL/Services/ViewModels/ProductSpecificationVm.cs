using ServerApp.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerApp.BLL.Services.ViewModels
{
    public class InputProductSpecificationVm
    {
        public int SpecificationTypeId { get; set; }
        public string Value { get; set; } // Giá trị thông số (vd: "Đỏ", "15cm")

        // Navigation properties
        //public virtual Product Product { get; set; }
        //public virtual InputSpecificationTypeVm SpecificationType { get; set; }
    }
    public class ProductSpecificationVm
    {        
        public int ProductId { get; set; } 
        public int SpecificationTypeId { get; set; } 
        public string Value { get; set; } // Giá trị thông số (vd: "Đỏ", "15cm")
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        //public virtual Product Product { get; set; }
        public virtual SpecificationTypeVm SpecificationType { get; set; }
    }
}
