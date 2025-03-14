using ServerApp.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerApp.BLL.Services.ViewModels
{
    public class InputSpecificationTypeVm
    {
        public string Name { get; set; } // Tên thông số (vd: Màu sắc, Kích thước)
    }
    public class SpecificationTypeVm
    {
        public int SpecificationTypeId { get; set; } // Primary Key
        //[SkipValidation]
        public string Name { get; set; } // Tên thông số (vd: Màu sắc, Kích thước)
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
