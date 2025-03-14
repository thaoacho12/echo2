using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerApp.DAL.Models
{
    public class Image {
        public int ImageId { get; set; }
        public string Name { get; set; }
        public byte[]? ImageData { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now; // Thời điểm tạo
        public DateTime UpdatedAt { get; set; } = DateTime.Now; // Thời điểm cập nhật

    }
   
}
