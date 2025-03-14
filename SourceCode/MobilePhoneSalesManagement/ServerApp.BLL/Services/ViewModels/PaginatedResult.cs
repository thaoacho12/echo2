using Microsoft.EntityFrameworkCore;

namespace BusinessLogic
{
    public class PaginatedResult<T>
    {
        public int PageIndex { get; private set; } // Chỉ số trang hiện tại
        public int TotalPages { get; private set; } // Tổng số trang
        public T[] Items { get; private set; } = Array.Empty<T>(); // Dữ liệu trong trang hiện tại

        // Constructor
        public PaginatedResult(List<T> items, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize); // Tính tổng số trang
            Items = items.ToArray();
        }

        // Kiểm tra xem có trang trước hay không
        public bool HasPreviousPage => PageIndex > 1;

        // Kiểm tra xem có trang tiếp theo hay không
        public bool HasNextPage => PageIndex < TotalPages;

        // Phương thức tạo PaginatedResult từ IQueryable (phân trang trên server)
        public static async Task<PaginatedResult<T>> CreateAsync(IQueryable<T> query, int pageIndex, int pageSize)
        {
            var count = await query.CountAsync(); // Đếm tổng số phần tử
            var items = await query.Skip((pageIndex - 1) * pageSize) // Bỏ qua các phần tử không thuộc trang
                                    .Take(pageSize) // Lấy các phần tử của trang hiện tại
                                    .ToListAsync(); // Thực thi truy vấn
            return new PaginatedResult<T>(items, count, pageIndex, pageSize);
        }
    }
}
