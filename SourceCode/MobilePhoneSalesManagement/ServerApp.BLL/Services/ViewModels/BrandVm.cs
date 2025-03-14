using ServerApp.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerApp.BLL.Services.ViewModels
{
    public class BrandVm
    {
        public int BrandId { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public int? ProductCount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        //public virtual ICollection<Product> Products { get; set; }
        [SkipValidation]
        public int? ImageId { get; set; }
        public virtual ImageRequest? Image { get; set; }
    }
    public class InputBrandVm
    {
        public string Name { get; set; }
        public bool IsActive { get; set; }
        [SkipValidation]
        public int? ImageId { get; set; }
        public virtual ImageRequest? Image { get; set; }

        //public virtual ICollection<Product> Products { get; set; }
    }
}
