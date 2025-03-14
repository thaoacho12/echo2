using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerApp.BLL.Services.ViewModels
{
    public class ImageRequest
    {
        public string Name { get; set; }
        [SkipValidation]
        public string ImageBase64 { get; set; }
    }
}
