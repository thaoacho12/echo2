using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerApp.BLL.Services.ViewModels
{
    public class FilterRequest
    {
        public string Sort { get; set; } // "banchay", "giathap", "giacao"
        public List<string> Brands { get; set; }
        public List<string> Prices { get; set; }
        public List<string> ScreenSizes { get; set; }
        public List<string> InternalMemory { get; set; }

        // Thêm các thuộc tính phân trang
        public int? PageNumber { get; set; }  // Trang hiện tại, mặc định 1
        public int? PageSize { get; set; }    // Số lượng sản phẩm mỗi trang, mặc định 15

        public string Search { get; set; }
    }
}
