using ServerApp.BLL.Services.Base;
using ServerApp.BLL.Services.ViewModels;
using ServerApp.DAL.Models;
using System.Linq.Expressions;

namespace ServerApp.BLL.Services.InterfaceServices
{
    public interface IProductService : IBaseService<Product>
    {
        Task<IEnumerable<ProductVm>> GetAllProductAsync(
            Expression<Func<Product, bool>>? filter = null,
            Func<IQueryable<Product>, IOrderedQueryable<Product>>? orderBy = null);
        Task<(IEnumerable<ProductVm>, int totalPages)> FilterProductsAsync(FilterRequest filterRequest);
        Task<ProductVm> AddProductAsync(InputProductVm brandVm);
        Task<ProductVm> UpdateProductAsync(int id, InputProductVm brandVm);
        Task<int> UpdateProductAsync(Product product);

        Task<ProductVm> DeleteProductAsync(int id);
        Task<ProductDetailVm> GetProductDetailsAsync(int productId);

        Task<ProductVm?> GetByProductIdAsync(int id);
        Task<IEnumerable<ProductSpecificationVm>> GetProductSpecificationsByProductIdAsync(int productId);
        Task<bool> AddProductToCartAsync(int productId, CartVm cartVm);
        Task<IEnumerable<ProductVm>> GetNewestProductsAsync();
        Task<PagedResult<ProductVm>> GetAllProductAsync(
        int? pageNumber, int? pageSize, string sortField = "updatedDate",
        Expression<Func<Product, bool>>? filter = null, bool orderBy = true);

        Task<IEnumerable<ProductVm>> SearchProductsByNameAsync(string name);
        Task<IEnumerable<ProductVm>> GetTopDiscountedProductsAsync();
    }
}
