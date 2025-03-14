using ServerApp.BLL.Services.Base;
using ServerApp.BLL.Services.InterfaceServices;
using ServerApp.BLL.Services.ViewModels;
using ServerApp.DAL.Infrastructure;
using ServerApp.DAL.Models;
using System.Linq.Expressions;
using Image = ServerApp.DAL.Models.Image;

namespace ServerApp.BLL.Services
{
    public class BrandService : BaseService<Brand>, IBrandService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IImageService _imageService;

        public BrandService(IUnitOfWork unitOfWork, IImageService imageService) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _imageService = imageService;
        }

        public async Task<IEnumerable<BrandVm>> GetAllBrandAsync()
        {
            var brands = await GetAllAsync(
                includesProperties: "Products"
            );

            var BrandViewModels = brands.Select(brand => new BrandVm
            {
                BrandId = brand.BrandId,
                Name = brand.Name,
                ImageId = brand.ImageId,
                IsActive = brand.IsActive,
                ProductCount = brand.Products.Count,
                CreatedAt = brand.CreatedAt,
                UpdatedAt = brand.UpdatedAt
            });

            return BrandViewModels;
        }
        public async Task<PagedResult<BrandVm>> GetAllBrandAsync(int? pageNumber, int? pageSize, Expression<Func<Brand, bool>>? filter = null,
      string sortField = "updatedDate", bool orderBy = true)
        {
            int currentPage = pageNumber ?? 1;
            int currentPageSize = pageSize ?? 10;
            // Determine sorting logic based on input
            Func<IQueryable<Brand>, IOrderedQueryable<Brand>> sortExpression = sortField switch
            {
                "brandCode" => orderBy
                    ? query => query.OrderBy(p => p.BrandId)
                    : query => query.OrderByDescending(p => p.BrandId),
                "brandName" => orderBy
                    ? query => query.OrderBy(p => p.Name)
                    : query => query.OrderByDescending(p => p.Name),
                "brandProductCount" => orderBy
                    ? query => query.OrderBy(p => p.Products.Count)
                    : query => query.OrderByDescending(p => p.Products.Count),
                _ => orderBy
                    ? query => query.OrderBy(p => p.UpdatedAt)
                    : query => query.OrderByDescending(p => p.UpdatedAt)
            };

            var query = await GetAllBrandAsync(pageNumber, pageSize, filter, orderBy: sortExpression);

            var totalCount = query.Count();
            var paginatedBrands = query
                .Skip((currentPage - 1) * currentPageSize)
                .Take(currentPageSize)
                .ToList();

            var brandVms = paginatedBrands.Select(brand => new BrandVm
            {
                BrandId = brand.BrandId,
                Name = brand.Name,
                ImageId = brand.ImageId,
                IsActive = brand.IsActive,
                ProductCount = brand.ProductCount,
                CreatedAt = brand.CreatedAt,
                UpdatedAt = brand.UpdatedAt,
                Image = brand.Image != null ? new ImageRequest
                {
                    Name = brand.Image.Name,
                    ImageBase64 = brand.Image.ImageBase64
                } : null
            });


            return new PagedResult<BrandVm>
            {
                CurrentPage = currentPage,
                PageSize = currentPageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / currentPageSize),
                Items = brandVms
            };
        }

        public async Task<IEnumerable<BrandVm>> GetAllBrandAsync(int? pageNumber, int? pageSize,
        Expression<Func<Brand, bool>>? filter = null,
        Func<IQueryable<Brand>, IOrderedQueryable<Brand>>? orderBy = null)
        {
            int currentPage = pageNumber ?? 1;
            int currentPageSize = pageSize ?? 10;
            var query = await GetAllAsync(
                filter, orderBy,
            includesProperties: "Image,Products"
                );
            var paginatedBrands = query
                .Skip((currentPage - 1) * currentPageSize)
                .Take(currentPageSize)
                .ToList();
            var brandVms = paginatedBrands.Select(brand => new BrandVm
            {
                BrandId = brand.BrandId,
                Name = brand.Name,
                ImageId = brand.ImageId,
                IsActive = brand.IsActive,
                ProductCount = brand.Products?.Count ?? 0,
                CreatedAt = brand.CreatedAt,
                UpdatedAt = brand.UpdatedAt,
                Image = brand.Image != null ? new ImageRequest
                {
                    Name = brand.Image.Name,
                    ImageBase64 = Convert.ToBase64String(brand.Image.ImageData)
                } : null
            });

            return brandVms;
        }
        public async Task<BrandVm?> GetByBrandIdAsync(int id)
        {
            var brand = await GetOneAsync(b => b.BrandId == id,
                includesProperties: "Image,Products");
            if (brand == null)
            {
                throw new ExceptionNotFound("Brand not found");
            }
            var brandVm = new BrandVm
            {
                BrandId = brand.BrandId,
                Name = brand.Name,
                ImageId = brand.ImageId,
                IsActive = brand.IsActive,
                ProductCount = brand.Products?.Count ?? 0,
                CreatedAt = brand.CreatedAt,
                UpdatedAt = brand.UpdatedAt,
                Image = brand.Image != null ? new ImageRequest
                {
                    Name = brand.Image.Name,
                    ImageBase64 = Convert.ToBase64String(brand.Image.ImageData)
                } : null
            };

            return brandVm;

        }

        public async Task<PagedResult<BrandVm>> GetAllBrandAsync(int? pageNumber, int? pageSize, bool filter = true,
            string sortField = "updatedDate", bool orderBy = true)
        {
            int currentPage = pageNumber ?? 1;
            int currentPageSize = pageSize ?? 10;

            var query = await GetAllBrandAsync(pageNumber, pageSize, b =>
                filter,
                sortField, orderBy
            );

            return query;
        }
        public async Task<PagedResult<BrandVm>> GetAllBrandAsync(int? pageNumber, int? pageSize, string search,
            string sortField = "updatedDate", bool orderBy = true)
        {
            int currentPage = pageNumber ?? 1;
            int currentPageSize = pageSize ?? 10;

            var query = await GetAllBrandAsync(pageNumber, pageSize, b =>
                b.Name.Contains(search),
                sortField, orderBy
            );

            return query;
        }
        public async Task<BrandVm> AddBrandAsync(InputBrandVm brandVm)
        {
            //ValidateModelPropertiesWithAttribute(brandVm);

            //var findBrand = await _unitOfWork.GenericRepository<Brand>().GetAsync(b =>
            //    b.Name == brandVm.Name
            //);
            if (true)
            {
                var id = await _imageService.AddImageAsync(brandVm.Image);
                if (id != null)
                {
                    var brand = new Brand
                    {
                        Name = brandVm.Name,
                        ImageId = id,
                        IsActive = brandVm.IsActive,
                    };

                    if (await AddAsync(brand) > 0)
                    {
                        return new BrandVm
                        {
                            BrandId = brand.BrandId,
                            Name = brand.Name,
                            ImageId = brand.ImageId,
                            IsActive = brand.IsActive
                        };
                    }
                }

                throw new ArgumentException("Failed to update brand");
            }
            throw new ExceptionBusinessLogic("Brand name is already in use.");

        }

        public async Task<BrandVm> UpdateBrandAsync(int id, InputBrandVm brandVm)
        {
            ValidateModelPropertiesWithAttribute(brandVm);
            //Tìm brand
            var brand = await _unitOfWork.GenericRepository<Brand>().GetByIdAsync(id);
            if (brand == null)
            {
                throw new ExceptionNotFound("Brand not found");
            }

            //Tìm brand có tên trùng với dữ liệu nhập vào (trừ brand tìm được phía trên)
            var findBrand = await _unitOfWork.GenericRepository<Brand>().GetAsync(b =>
                b.BrandId != id &&
                b.Name == brandVm.Name
             );
            if (findBrand != null)
            {
                throw new ExceptionBusinessLogic("Brand name is already in use.");
            }
            brand.Name = brandVm.Name;
            brand.IsActive = brandVm.IsActive;

            await _imageService.UpdateImageAsync(brandVm.ImageId ?? 0, brandVm.Image);


            brand.UpdatedAt = DateTime.Now;
            var result = await _unitOfWork.GenericRepository<Brand>().ModifyAsync(brand);

            if (result <= 0)
            {
                throw new ArgumentException("Failed to update brand");
            }

            return new BrandVm
            {
                BrandId = brand.BrandId,
                Name = brand.Name,
                ImageId = brand.ImageId,
                ProductCount = brand.Products?.Count ?? 0,
                IsActive = brand.IsActive,
                CreatedAt = brand.CreatedAt
            };

        }
        public async Task<int> UpdateBrandAsync(Brand brand)
        {
            // Cập nhật thông tin Brand
            brand.Name = brand.Name;
            brand.ImageId = brand.ImageId;
            brand.IsActive = brand.IsActive;
            brand.UpdatedAt = DateTime.Now;

            // Cập nhật Brand vào cơ sở dữ liệu
            return await _unitOfWork.GenericRepository<Brand>().ModifyAsync(brand);
        }
        public async Task<BrandVm> DeleteBrandAsync(int id)
        {
            var brand = await _unitOfWork.GenericRepository<Brand>().GetByIdAsync(id);

            if (brand == null)
            {
                throw new ExceptionNotFound("Brand not found");
            }
            var isdelete = 0;
            if (brand.IsActive == true)
            {
                brand.IsActive = false;
                isdelete = await UpdateBrandAsync(brand);
            }
            else
            {
                _unitOfWork.GenericRepository<Brand>().Delete(id);
                _unitOfWork.GenericRepository<Image>().Delete(brand.ImageId ?? 0);
                isdelete = _unitOfWork.SaveChanges();
            }

            if (isdelete > 0)
            {
                return new BrandVm
                {
                    BrandId = brand.BrandId,
                    Name = brand.Name,
                    ProductCount = brand.Products?.Count ?? 0,
                    ImageId = brand.ImageId,
                    IsActive = brand.IsActive
                };
            }
            throw new ArgumentException("Failed to delete brand");
        }

    }

}
