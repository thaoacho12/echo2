using Microsoft.EntityFrameworkCore;
using ServerApp.BLL.Services.Base;
using ServerApp.BLL.Services.InterfaceServices;
using ServerApp.BLL.Services.ViewModels;
using ServerApp.DAL.Infrastructure;
using ServerApp.DAL.Models;
using ServerApp.DAL.Repositories.Generic;
using System.Linq.Expressions;

namespace ServerApp.BLL.Services
{
    public class ProductService : BaseService<Product>, IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISpecificationTypeService _specificationTypeService;

        private readonly IGenericRepository<Product> _productRepository;
        private readonly IGenericRepository<Category> _categoryRepository;
        private readonly IGenericRepository<Brand> _brandRepository;
        private readonly IImageService _imageService;


        public ProductService(IUnitOfWork unitOfWork, ISpecificationTypeService specificationTypeService, IImageService imageService) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _specificationTypeService = specificationTypeService;
            _brandRepository = unitOfWork.GenericRepository<Brand>();
            _imageService = imageService;
        }

        private async Task<List<ProductSpecification>> ProcessSpecificationTypesAsync(
    IEnumerable<InputProductSpecificationVm> productSpecifications)
        {
            var productSpecificationsToAdd = new List<ProductSpecification>();

            foreach (var spec in productSpecifications)
            {
                // Kiểm tra SpecificationType có tồn tại không
                var specificationType = await _specificationTypeService.GetBySpecificationTypeIdAsync(spec.SpecificationTypeId);

                if (specificationType == null)
                {
                    throw new ArgumentException($"Failed to process SpecificationTypeId: {spec.SpecificationTypeId}");
                }

                // Thêm ProductSpecification vào danh sách
                productSpecificationsToAdd.Add(new ProductSpecification
                {
                    SpecificationTypeId = specificationType.SpecificationTypeId,
                    Value = spec.Value
                });
            }

            return productSpecificationsToAdd;
        }
        private async Task<List<ProductSpecification>> ProcessAndSyncSpecificationsAsync(int productId, IEnumerable<InputProductSpecificationVm> productSpecifications)
        {
            var productSpecificationsToAddOrUpdate = new List<ProductSpecification>();

            // Lấy tất cả các ProductSpecification hiện tại của sản phẩm từ cơ sở dữ liệu
            var existingProductSpecifications = await _unitOfWork.GenericRepository<ProductSpecification>()
                .GetAllAsync(ps => ps.ProductId == productId);

            // Xử lý các SpecificationType mới và cập nhật ProductSpecification
            foreach (var spec in productSpecifications)
            {
                var specificationType = await _specificationTypeService.GetBySpecificationTypeIdAsync(spec.SpecificationTypeId);

                if (specificationType == null)
                {
                    throw new ArgumentException($"Failed to process SpecificationTypeId : {spec.SpecificationTypeId}");
                }

                // Kiểm tra xem ProductSpecification này đã tồn tại chưa trong danh sách hiện tại
                var existingProductSpec = existingProductSpecifications
                    .FirstOrDefault(ps => ps.SpecificationTypeId == specificationType.SpecificationTypeId);

                if (existingProductSpec != null)
                {
                    // Cập nhật giá trị nếu đã tồn tại
                    existingProductSpec.Value = spec.Value;
                    productSpecificationsToAddOrUpdate.Add(existingProductSpec);
                }
                else
                {
                    // Thêm mới ProductSpecification nếu chưa tồn tại
                    var newProductSpecification = new ProductSpecification
                    {
                        ProductId = productId,
                        SpecificationTypeId = specificationType.SpecificationTypeId,
                        Value = spec.Value
                    };
                    productSpecificationsToAddOrUpdate.Add(newProductSpecification);
                }
            }

            // Xóa các ProductSpecification không còn tồn tại trong danh sách mới
            var productSpecificationsToDelete = existingProductSpecifications
                .Where(ps => !productSpecifications.Any(spec => spec.SpecificationTypeId == ps.SpecificationTypeId))
                .ToList();

            // Xóa các ProductSpecification không còn sử dụng
            foreach (var productSpecification in productSpecificationsToDelete)
            {
                _unitOfWork.GenericRepository<ProductSpecification>().Delete(productSpecification);
            }

            // Trả về danh sách các ProductSpecification cần thêm hoặc cập nhật
            return productSpecificationsToAddOrUpdate;
        }

        public async Task<ProductVm> AddProductAsync(InputProductVm productVm)
        {
            ValidateModelPropertiesWithAttribute(productVm);

            // Kiểm tra xem Product đã tồn tại hay chưa
            var findProduct = await _unitOfWork.GenericRepository<Product>().GetAsync(p =>
                p.Name == productVm.Name
            );

            if (findProduct != null)
            {
                throw new ExceptionBusinessLogic("Product name is already in use.");
            }

            // Bắt đầu transaction
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var idImage = await _imageService.AddImageAsync(productVm.Image);
                if (idImage == null)
                {
                    throw new ExceptionBusinessLogic("Image add Failed.");
                }
                // Xử lý SpecificationType và chuẩn bị danh sách ProductSpecification
                var productSpecificationsToAdd = await ProcessSpecificationTypesAsync(productVm.ProductSpecifications);

                // Tạo Product
                var product = new Product
                {
                    Name = productVm.Name,
                    Description = productVm.Description,
                    Price = productVm.Price,
                    OldPrice = productVm.OldPrice,
                    StockQuantity = productVm.StockQuantity,
                    BrandId = productVm.BrandId,
                    ImageId = idImage,
                    Manufacturer = productVm.Manufacturer,
                    IsActive = productVm.IsActive,
                    Colors = productVm.Colors,
                    Discount = productVm.Discount
                };

                // Thêm Product vào cơ sở dữ liệu
                await _unitOfWork.GenericRepository<Product>().AddAsync(product);
                await _unitOfWork.SaveChangesAsync();

                // Gắn ProductId vào từng ProductSpecification và thêm vào database
                foreach (var productSpecification in productSpecificationsToAdd)
                {
                    productSpecification.ProductId = product.ProductId;
                    await _unitOfWork.GenericRepository<ProductSpecification>().AddAsync(productSpecification);
                }

                // Lưu thay đổi vào cơ sở dữ liệu
                await _unitOfWork.SaveChangesAsync();

                // Commit transaction
                await _unitOfWork.CommitTransactionAsync();

                // Trả về ProductVm
                return new ProductVm
                {
                    ProductId = product.ProductId,
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    OldPrice = product.OldPrice,
                    StockQuantity = product.StockQuantity,
                    BrandId = product.BrandId,
                    ImageId = idImage,
                    Manufacturer = product.Manufacturer,
                    IsActive = product.IsActive,
                    Colors = product.Colors,
                    Discount = product.Discount,
                    CreatedAt = product.CreatedAt,
                    UpdatedAt = product.UpdatedAt
                };
            }
            catch (Exception ex)
            {
                // Log lỗi nếu cần
                Console.WriteLine($"Error: {ex.Message}");

                // Rollback transaction nếu có lỗi
                await _unitOfWork.RollbackTransactionAsync();

                // Ném lại ngoại lệ để bên ngoài xử lý
                throw new ArgumentException($"{ex.Message}", ex);
            }
        }


        public async Task<ProductVm> DeleteProductAsync(int id)
        {
            var product = await _unitOfWork.GenericRepository<Product>().GetByIdAsync(id);

            if (product == null)
            {
                throw new ExceptionNotFound("Product not found");
            }
            var isdelete = 0;
            if (product.IsActive == true)
            {
                product.IsActive = false;
                isdelete = await UpdateProductAsync(product);
            }
            else
            {
                _unitOfWork.GenericRepository<Product>().Delete(id);
                _unitOfWork.GenericRepository<Image>().Delete(product.ImageId ?? 0);
                isdelete = _unitOfWork.SaveChanges();
            }

            if (isdelete > 0)
            {
                return new ProductVm
                {
                    ProductId = product.ProductId,
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    OldPrice = product.OldPrice,
                    StockQuantity = product.StockQuantity,
                    BrandId = product.BrandId,
                    ImageId = product.ImageId,
                    Manufacturer = product.Manufacturer,
                    IsActive = product.IsActive,
                    Colors = product.Colors,
                    Discount = product.Discount,
                    CreatedAt = product.CreatedAt,
                    UpdatedAt = product.UpdatedAt,
                };
            }

            // Nếu lưu thất bại
            throw new ArgumentException("Failed to delete product");

        }

        public async Task<IEnumerable<ProductVm>> GetAllProductAsync(
            Expression<Func<Product, bool>>? filter = null,
            Func<IQueryable<Product>, IOrderedQueryable<Product>>? orderBy = null)
        {
            var resuilt = await GetAllAsync(
                filter, orderBy,
                    includesProperties: "Image,Brand,ProductSpecifications,ProductSpecifications.SpecificationType"
                );
            var productViewModels = resuilt.Select(product => new ProductVm
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                OldPrice = product.OldPrice,
                StockQuantity = product.StockQuantity,
                BrandId = product.BrandId,
                ImageId = product.ImageId,
                Manufacturer = product.Manufacturer,
                IsActive = product.IsActive,
                Colors = product.Colors,
                Discount = product.Discount,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt,
                Image = product.Image != null ? new ImageRequest
                {
                    Name = product.Image.Name,
                    ImageBase64 = Convert.ToBase64String(product.Image.ImageData)
                } : null,
                Brand = product.Brand != null ? new BrandVm
                {
                    BrandId = product.Brand.BrandId,
                    Name = product.Brand.Name,
                    IsActive = product.Brand.IsActive,
                    ImageId = product.Brand.ImageId,
                    ProductCount = product.Brand.Products.Count,
                    CreatedAt = product.CreatedAt,
                    UpdatedAt = product.Brand.UpdatedAt
                } : null, // Bao gồm dữ liệu liên kết Brand
                ProductSpecifications = product.ProductSpecifications.Select(ps => new ProductSpecificationVm
                {
                    ProductId = ps.ProductId,
                    SpecificationTypeId = ps.SpecificationTypeId,
                    Value = ps.Value,
                    CreatedAt = ps.CreatedAt,
                    UpdatedAt = ps.UpdatedAt,
                    SpecificationType = new SpecificationTypeVm
                    {
                        SpecificationTypeId = ps.SpecificationType.SpecificationTypeId,
                        Name = ps.SpecificationType.Name,
                        CreatedAt = ps.SpecificationType.CreatedAt,
                        UpdatedAt = ps.SpecificationType.UpdatedAt,
                    }
                }).ToList()
            });

            return productViewModels;
        }
        public async Task<PagedResult<ProductVm>> GetAllProductAsync(
        int? pageNumber, int? pageSize, string sortField = "updatedDate",
        Expression<Func<Product, bool>>? filter = null, bool orderBy = true)
        {
            // Determine sorting logic based on input
            Func<IQueryable<Product>, IOrderedQueryable<Product>> sortExpression = sortField switch
            {
                "productCode" => orderBy
                    ? query => query.OrderBy(p => p.ProductId)
                    : query => query.OrderByDescending(p => p.ProductId),
                "productName" => orderBy
                    ? query => query.OrderBy(p => p.Name)
                    : query => query.OrderByDescending(p => p.Name),
                "productstockQuantity" => orderBy
                    ? query => query.OrderBy(p => p.StockQuantity)
                    : query => query.OrderByDescending(p => p.Price),
                "productPrice" => orderBy
                    ? query => query.OrderBy(p => p.Price)
                    : query => query.OrderByDescending(p => p.Price),
                _ => orderBy
                    ? query => query.OrderBy(p => p.UpdatedAt)
                    : query => query.OrderByDescending(p => p.UpdatedAt)
            };

            // Pagination parameters
            int currentPage = pageNumber ?? 1;
            int currentPageSize = pageSize ?? 10;

            // Get data with filtering and sorting
            var query = await GetAllProductAsync(filter, orderBy: sortExpression);
            var totalCount = query.Count();

            var paginatedProducts = query
                .Skip((currentPage - 1) * currentPageSize)
                .Take(currentPageSize)
                .ToList();

            // Prepare paginated result
            return new PagedResult<ProductVm>
            {
                CurrentPage = currentPage,
                PageSize = currentPageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / currentPageSize),
                Items = paginatedProducts,
            };
        }


        public async Task<IEnumerable<ProductSpecificationVm>> GetProductSpecificationsByProductIdAsync(int productId)
        {
            // Lấy sản phẩm với ProductId và bao gồm ProductSpecifications cùng với SpecificationType
            var product = await GetByProductIdAsync(productId);

            if (product == null)
            {
                throw new ExceptionNotFound("Product not found");
            }

            // Lọc và chuyển đổi dữ liệu ProductSpecifications thành ProductSpecificationVm
            var productSpecificationViewModels = product.ProductSpecifications.Select(ps => new ProductSpecificationVm
            {
                ProductId = ps.ProductId,
                SpecificationTypeId = ps.SpecificationTypeId,
                Value = ps.Value,
                SpecificationType = new SpecificationTypeVm
                {
                    SpecificationTypeId = ps.SpecificationType.SpecificationTypeId,
                    Name = ps.SpecificationType.Name
                }
            }).ToList();

            return productSpecificationViewModels;
        }

        public async Task<ProductVm?> GetByProductIdAsync(int id)
        {
            var product = await GetOneAsync(p =>
                p.ProductId == id,
                    //&& p.ProductSpecifications.Select(ps=>ps.),
                    includesProperties: "Image,Brand,ProductSpecifications,ProductSpecifications.SpecificationType"
                );
            if (product == null)
            {
                throw new ExceptionNotFound("Product not found");
            }
            var productVm = new ProductVm
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                OldPrice = product.OldPrice,
                StockQuantity = product.StockQuantity,
                BrandId = product.BrandId,
                ImageId = product.ImageId,
                Manufacturer = product.Manufacturer,
                IsActive = product.IsActive,
                Colors = product.Colors,
                Discount = product.Discount, // Gán mặc định hoặc tính toán tùy theo logic
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt,
                Image = product.Image != null ? new ImageRequest
                {
                    Name = product.Image.Name,
                    ImageBase64 = Convert.ToBase64String(product.Image.ImageData)
                } : null,
                Brand = product.Brand != null ? new BrandVm
                {
                    BrandId = product.Brand.BrandId,
                    Name = product.Brand.Name,
                    IsActive = product.Brand.IsActive,
                    ImageId = product.Brand.ImageId,
                    ProductCount = product.Brand.Products.Count,
                    CreatedAt = product.CreatedAt,
                    UpdatedAt = product.Brand.UpdatedAt
                } : null, // Bao gồm dữ liệu liên kết Brand
                ProductSpecifications = product.ProductSpecifications.Select(ps => new ProductSpecificationVm
                {
                    ProductId = ps.ProductId,
                    SpecificationTypeId = ps.SpecificationTypeId,
                    Value = ps.Value,
                    SpecificationType = new SpecificationTypeVm
                    {
                        SpecificationTypeId = ps.SpecificationType.SpecificationTypeId,
                        Name = ps.SpecificationType.Name
                    }
                }).ToList()
            };

            return productVm;
        }

        public async Task<ProductVm> UpdateProductAsync(int id, InputProductVm productVm)
        {
            ValidateModelPropertiesWithAttribute(productVm);

            // Kiểm tra xem Product có tồn tại không
            var existingProduct = await _unitOfWork.GenericRepository<Product>().GetByIdAsync(id);
            if (existingProduct == null)
            {
                throw new ExceptionBusinessLogic("Product not found.");
            }

            // Bắt đầu transaction
            await _unitOfWork.BeginTransactionAsync();

            try
            {

                await _imageService.UpdateImageAsync(productVm.ImageId ?? 0, productVm.Image);
                // Xử lý và đồng bộ các ProductSpecification
                var productSpecificationsToAddOrUpdate = await ProcessAndSyncSpecificationsAsync(id, productVm.ProductSpecifications);

                // Cập nhật thông tin Product
                existingProduct.Name = productVm.Name;
                existingProduct.Description = productVm.Description;
                existingProduct.Price = productVm.Price;
                existingProduct.OldPrice = productVm.OldPrice;
                existingProduct.StockQuantity = productVm.StockQuantity;
                existingProduct.BrandId = productVm.BrandId;
                existingProduct.ImageId = productVm.ImageId;
                existingProduct.Manufacturer = productVm.Manufacturer;
                existingProduct.IsActive = productVm.IsActive;
                existingProduct.Colors = productVm.Colors;
                existingProduct.Discount = productVm.Discount;
                existingProduct.UpdatedAt = DateTime.Now;
                // Cập nhật Product vào cơ sở dữ liệu
                _unitOfWork.GenericRepository<Product>().Update(existingProduct);
                await _unitOfWork.SaveChangesAsync();

                // Lưu các ProductSpecification đã thay đổi vào cơ sở dữ liệu
                foreach (var productSpecification in productSpecificationsToAddOrUpdate)
                {
                    // Kiểm tra xem ProductSpecification có tồn tại trong cơ sở dữ liệu không
                    var existingSpec = await _unitOfWork.GenericRepository<ProductSpecification>().GetAsync(ps =>
                        ps.ProductId == id && ps.SpecificationTypeId == productSpecification.SpecificationTypeId);

                    if (existingSpec != null)
                    {
                        // Cập nhật nếu đã tồn tại
                        existingSpec.Value = productSpecification.Value;
                        _unitOfWork.GenericRepository<ProductSpecification>().Update(existingSpec);
                    }
                    else
                    {
                        // Thêm mới nếu chưa tồn tại
                        productSpecification.ProductId = id; // Gán ProductId
                        await _unitOfWork.GenericRepository<ProductSpecification>().AddAsync(productSpecification);
                    }
                }

                // Lưu thay đổi vào cơ sở dữ liệu
                await _unitOfWork.SaveChangesAsync();

                // Commit transaction
                await _unitOfWork.CommitTransactionAsync();

                // Trả về ProductVm
                return new ProductVm
                {
                    ProductId = existingProduct.ProductId,
                    Name = existingProduct.Name,
                    Description = existingProduct.Description,
                    Price = existingProduct.Price,
                    OldPrice = existingProduct.OldPrice,
                    StockQuantity = existingProduct.StockQuantity,
                    BrandId = existingProduct.BrandId,
                    ImageId = existingProduct.ImageId,
                    Manufacturer = existingProduct.Manufacturer,
                    IsActive = existingProduct.IsActive,
                    Colors = existingProduct.Colors,
                    Discount = existingProduct.Discount,
                    CreatedAt = existingProduct.CreatedAt,
                    UpdatedAt = existingProduct.UpdatedAt
                };
            }
            catch (Exception ex)
            {
                // Log lỗi nếu cần
                Console.WriteLine($"Error: {ex.Message}");

                // Rollback transaction nếu có lỗi
                await _unitOfWork.RollbackTransactionAsync();

                // Ném lại ngoại lệ để bên ngoài xử lý
                throw new ArgumentException($"{ex.Message}", ex);
            }


        }
        public async Task<int> UpdateProductAsync(Product product)
        {
            // Cập nhật thông tin Product
            product.Name = product.Name;
            product.Description = product.Description;
            product.Price = product.Price;
            product.OldPrice = product.OldPrice;
            product.StockQuantity = product.StockQuantity;
            product.BrandId = product.BrandId;
            product.ImageId = product.ImageId;
            product.Manufacturer = product.Manufacturer;
            product.IsActive = product.IsActive;
            product.Colors = product.Colors;
            product.Discount = product.Discount;
            product.UpdatedAt = DateTime.Now;
            // Cập nhật Product vào cơ sở dữ liệu

            return await _unitOfWork.GenericRepository<Product>().ModifyAsync(product);
        }
        public async Task<ProductDetailVm> GetProductDetailsAsync(int productId)
        {
            // Lấy sản phẩm từ database
            var product = await _productRepository.GetByIdAsync(productId);

            if (product == null)
            {
                return null;  // Trả về null nếu không tìm thấy sản phẩm
            }

            // Lấy thông tin danh mục và thương hiệu (nếu cần)

            var brand = await _brandRepository.GetByIdAsync((int)product.BrandId);

            // Tạo đối tượng ProductDetailVm để trả về
            var productDetailVm = new ProductDetailVm
            {
                ProductId = product.ProductId,
                ProductName = product.Name,
                Description = product.Description,
                Price = product.Price,
                ImageId = product.ImageId,

                // Lấy tên thương hiệu từ Brand
                CreatedDate = product.CreatedDate,
                UpdatedDate = product.UpdatedDate
            };

            return productDetailVm;
        }


        public async Task<(IEnumerable<ProductVm>, int totalPages)> FilterProductsAsync(FilterRequest filterRequest)
        {
            try
            {
                var query = await GetAllAsync(includesProperties: "Image,Brand,ProductSpecifications,ProductSpecifications.SpecificationType");

                // Lọc theo Hãng sản xuất
                if (filterRequest.Brands != null && filterRequest.Brands.Any())
                {
                    query = query.Where(p => p.Brand != null && filterRequest.Brands.Contains(p.Brand.Name));
                }

                // Lọc theo Giá bán
                if (filterRequest.Prices != null && filterRequest.Prices.Any())
                {
                    foreach (var price in filterRequest.Prices)
                    {
                        switch (price)
                        {
                            case "under2m":
                                query = query.Where(p => p.Price < 2000000);
                                break;
                            case "2to5m":
                                query = query.Where(p => p.Price >= 2000000 && p.Price <= 5000000);
                                break;
                            case "5to10m":
                                query = query.Where(p => p.Price >= 5000000 && p.Price <= 10000000);
                                break;
                            case "10to15m":
                                query = query.Where(p => p.Price >= 10000000 && p.Price <= 15000000);
                                break;
                            case "above15m":
                                query = query.Where(p => p.Price >= 15000000);
                                break;
                        }
                    }
                }

                // Lọc theo Màn hình
                if (filterRequest.ScreenSizes != null && filterRequest.ScreenSizes.Any())
                {
                    var screenSizeTypeId = await _unitOfWork.GenericRepository<SpecificationType>()
                                                             .GetQuery()
                                                             .Where(s => s.Name == "ScreenSize")
                                                             .Select(s => s.SpecificationTypeId)
                                                             .FirstOrDefaultAsync();
                    if (screenSizeTypeId == 0)
                    {
                        throw new Exception("ScreenSize SpecificationTypeId not found.");
                    }

                    foreach (var screenSize in filterRequest.ScreenSizes)
                    {
                        switch (screenSize)
                        {
                            case "under5":
                                query = query.Where(p => p.ProductSpecifications
                                    .Any(ps => ps.SpecificationTypeId == screenSizeTypeId &&
                                               double.TryParse(ps.Value, out var size) && size < 5.0));
                                break;
                            case "above6":
                                query = query.Where(p => p.ProductSpecifications
                                    .Any(ps => ps.SpecificationTypeId == screenSizeTypeId &&
                                               double.TryParse(ps.Value, out var size) && size > 6.0));
                                break;
                        }
                    }
                }

                // Lọc theo Bộ nhớ trong
                if (filterRequest.InternalMemory != null && filterRequest.InternalMemory.Any())
                {
                    var storageTypeId = await _unitOfWork.GenericRepository<SpecificationType>()
                                                         .GetQuery()
                                                         .Where(s => s.Name == "Storage")
                                                         .Select(s => s.SpecificationTypeId)
                                                         .FirstOrDefaultAsync();
                    if (storageTypeId == 0)
                    {
                        throw new Exception("Storage SpecificationTypeId not found.");
                    }

                    foreach (var internalMemory in filterRequest.InternalMemory)
                    {
                        switch (internalMemory)
                        {
                            case "under32":
                                query = query.Where(p => p.ProductSpecifications
                                    .Any(ps => ps.SpecificationTypeId == storageTypeId &&
                                               int.TryParse(ps.Value, out var size) && size <= 32));
                                break;
                            case "64and128":
                                query = query.Where(p => p.ProductSpecifications
                                    .Any(ps => ps.SpecificationTypeId == storageTypeId &&
                                               int.TryParse(ps.Value, out var size) && (size == 64 || size == 128)));
                                break;
                            case "256and512":
                                query = query.Where(p => p.ProductSpecifications
                                    .Any(ps => ps.SpecificationTypeId == storageTypeId &&
                                               int.TryParse(ps.Value, out var size) && (size == 256 || size == 512)));
                                break;
                            case "above512":
                                query = query.Where(p => p.ProductSpecifications
                                    .Any(ps => ps.SpecificationTypeId == storageTypeId &&
                                               int.TryParse(ps.Value, out var size) && size > 512));
                                break;
                        }
                    }
                }

                // Sắp xếp
                switch (filterRequest.Sort)
                {
                    case "banchay":
                        query = query.OrderByDescending(p => p.OrderItems
                            .Where(oi => oi.Order.OrderStatus == "done" && oi.ProductId == p.ProductId)
                            .Sum(oi => oi.Quantity));
                        break;
                    case "giathap":
                        query = query.OrderBy(p => p.Price);
                        break;
                    case "giacao":
                        query = query.OrderByDescending(p => p.Price);
                        break;
                }
                query = query.Where(p => p.Name.Contains(filterRequest.Search, StringComparison.OrdinalIgnoreCase));
                // Chuyển query về IQueryable để có thể sử dụng CountAsync
                var queryableQuery = query.AsQueryable();
                // Tính tổng số sản phẩm (dùng cho totalPages)
                int totalProducts = queryableQuery.Count();

                // Phân trang - Giới hạn tối đa 15 sản phẩm mỗi trang
                int pageNumber = filterRequest.PageNumber ?? 1;
                int pageSize = filterRequest.PageSize ?? 15;

                // Tính tổng số trang
                int totalPages = (int)Math.Ceiling((double)totalProducts / pageSize);

                var pagedQuery = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
                // Chuyển sang ViewModel và trả về
                var products = pagedQuery.Select(product => new ProductVm
                {
                    ProductId = product.ProductId,
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    OldPrice = product.OldPrice,
                    StockQuantity = product.StockQuantity,
                    BrandId = product.BrandId,
                    ImageId = product.ImageId,
                    Manufacturer = product.Manufacturer,
                    IsActive = product.IsActive,
                    Colors = product.Colors,
                    Discount = product.Discount,
                    CreatedAt = product.CreatedAt,
                    UpdatedAt = product.UpdatedAt,
                    Image = product.Image != null ? new ImageRequest
                    {
                        Name = product.Image.Name,
                        ImageBase64 = Convert.ToBase64String(product.Image.ImageData)
                    } : null,
                    Brand = product.Brand != null ? new BrandVm
                    {
                        BrandId = product.Brand.BrandId,
                        Name = product.Brand.Name,
                        IsActive = product.Brand.IsActive,
                        ImageId = product.Brand.ImageId,
                        ProductCount = product.Brand.Products.Count,
                        CreatedAt = product.CreatedAt,
                        UpdatedAt = product.Brand.UpdatedAt
                    } : null, // Bao gồm dữ liệu liên kết Brand
                    ProductSpecifications = product.ProductSpecifications.Select(ps => new ProductSpecificationVm
                    {
                        ProductId = ps.ProductId,
                        SpecificationTypeId = ps.SpecificationTypeId,
                        Value = ps.Value,
                        CreatedAt = ps.CreatedAt,
                        UpdatedAt = ps.UpdatedAt,
                        SpecificationType = new SpecificationTypeVm
                        {
                            SpecificationTypeId = ps.SpecificationType.SpecificationTypeId,
                            Name = ps.SpecificationType.Name,
                            CreatedAt = ps.SpecificationType.CreatedAt,
                            UpdatedAt = ps.SpecificationType.UpdatedAt,
                        }
                    }).ToList()
                }).ToList();
                return (products, totalPages);
            }
            catch (Exception ex)
            {
                // Ghi log lỗi hoặc xử lý tùy vào yêu cầu dự án
                Console.WriteLine($"An error occurred: {ex.Message}");
                return (Enumerable.Empty<ProductVm>(), 0);
            }
        }
        public async Task<IEnumerable<ProductVm>> GetNewestProductsAsync()
        {
            try
            {
                // Lấy danh sách sản phẩm bao gồm các thuộc tính cần thiết
                var query = await GetAllAsync(includesProperties: "Image,Brand,ProductSpecifications,ProductSpecifications.SpecificationType");

                // Sắp xếp theo ngày tạo (CreatedAt) giảm dần và lấy 4 sản phẩm mới nhất
                var newestProducts = query
                    .OrderByDescending(product => product.CreatedAt)
                    .Take(4)
                    .Select(product => new ProductVm
                    {
                        ProductId = product.ProductId,
                        Name = product.Name,
                        Description = product.Description,
                        Price = product.Price,
                        OldPrice = product.OldPrice,
                        StockQuantity = product.StockQuantity,
                        BrandId = product.BrandId,
                        ImageId = product.ImageId,
                        Manufacturer = product.Manufacturer,
                        IsActive = product.IsActive,
                        Colors = product.Colors,
                        Discount = product.Discount,
                        CreatedAt = product.CreatedAt,
                        UpdatedAt = product.UpdatedAt,
                        Image = product.Image != null ? new ImageRequest
                        {
                            Name = product.Image.Name,
                            ImageBase64 = Convert.ToBase64String(product.Image.ImageData)
                        } : null,
                        Brand = product.Brand != null ? new BrandVm
                        {
                            BrandId = product.Brand.BrandId,
                            Name = product.Brand.Name,
                            IsActive = product.Brand.IsActive,
                            ImageId = product.Brand.ImageId,
                            ProductCount = product.Brand.Products.Count,
                            CreatedAt = product.CreatedAt,
                            UpdatedAt = product.Brand.UpdatedAt
                        } : null, // Bao gồm dữ liệu liên kết Brand
                        ProductSpecifications = product.ProductSpecifications.Select(ps => new ProductSpecificationVm
                        {
                            ProductId = ps.ProductId,
                            SpecificationTypeId = ps.SpecificationTypeId,
                            Value = ps.Value,
                            CreatedAt = ps.CreatedAt,
                            UpdatedAt = ps.UpdatedAt,
                            SpecificationType = new SpecificationTypeVm
                            {
                                SpecificationTypeId = ps.SpecificationType.SpecificationTypeId,
                                Name = ps.SpecificationType.Name,
                                CreatedAt = ps.SpecificationType.CreatedAt,
                                UpdatedAt = ps.SpecificationType.UpdatedAt,
                            }
                        }).ToList()
                    })
                    .ToList();

                return newestProducts;
            }
            catch (Exception ex)
            {
                // Ghi log lỗi hoặc xử lý tùy theo yêu cầu dự án
                Console.WriteLine($"An error occurred: {ex.Message}");
                return Enumerable.Empty<ProductVm>();
            }
        }
        public Task<bool> AddProductToCartAsync(int productId, CartVm cartVm)
        {
            throw new NotImplementedException();
        }
        public async Task<IEnumerable<ProductVm>> SearchProductsByNameAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Search term cannot be null or empty.");
            }

            // Lấy danh sách sản phẩm chứa tên được tìm kiếm (không phân biệt chữ hoa/thường)
            var products = await GetAllAsync(p => p.Name.Contains(name));

            // Chuyển đổi sang ProductVm
            var result = products.Select(p => new ProductVm
            {
                ProductId = p.ProductId,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                OldPrice = p.OldPrice,
                StockQuantity = p.StockQuantity,
                BrandId = p.BrandId,
                ImageId = p.ImageId,
                Manufacturer = p.Manufacturer,
                IsActive = p.IsActive,
                Colors = p.Colors,
                Discount = p.Discount,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            });

            return result;
        }
        public async Task<IEnumerable<ProductVm>> GetTopDiscountedProductsAsync()
        {
            try
            {
                // Lấy danh sách sản phẩm bao gồm các thuộc tính cần thiết
                var query = await GetAllAsync(includesProperties: "Image,Brand,ProductSpecifications,ProductSpecifications.SpecificationType");

                // Sắp xếp theo mức giảm giá (Discount) giảm dần và lấy 4 sản phẩm giảm giá nhiều nhất
                var topDiscountedProducts = query
                    .Where(product => product.Discount > 0) // Lọc những sản phẩm có giảm giá
                    .OrderByDescending(product => product.Discount)
                    .Take(4)
                    .Select(product => new ProductVm
                    {
                        ProductId = product.ProductId,
                        Name = product.Name,
                        Description = product.Description,
                        Price = product.Price,
                        OldPrice = product.OldPrice,
                        StockQuantity = product.StockQuantity,
                        BrandId = product.BrandId,
                        ImageId = product.ImageId,
                        Manufacturer = product.Manufacturer,
                        IsActive = product.IsActive,
                        Colors = product.Colors,
                        Discount = product.Discount,
                        CreatedAt = product.CreatedAt,
                        UpdatedAt = product.UpdatedAt,
                        Image = product.Image != null ? new ImageRequest
                        {
                            Name = product.Image.Name,
                            ImageBase64 = Convert.ToBase64String(product.Image.ImageData)
                        } : null,
                        Brand = product.Brand != null ? new BrandVm
                        {
                            BrandId = product.Brand.BrandId,
                            Name = product.Brand.Name,
                            IsActive = product.Brand.IsActive,
                            ImageId = product.Brand.ImageId,
                            ProductCount = product.Brand.Products.Count,
                            CreatedAt = product.CreatedAt,
                            UpdatedAt = product.Brand.UpdatedAt
                        } : null, // Bao gồm dữ liệu liên kết Brand
                        ProductSpecifications = product.ProductSpecifications.Select(ps => new ProductSpecificationVm
                        {
                            ProductId = ps.ProductId,
                            SpecificationTypeId = ps.SpecificationTypeId,
                            Value = ps.Value,
                            CreatedAt = ps.CreatedAt,
                            UpdatedAt = ps.UpdatedAt,
                            SpecificationType = new SpecificationTypeVm
                            {
                                SpecificationTypeId = ps.SpecificationType.SpecificationTypeId,
                                Name = ps.SpecificationType.Name,
                                CreatedAt = ps.SpecificationType.CreatedAt,
                                UpdatedAt = ps.SpecificationType.UpdatedAt,
                            }
                        }).ToList()
                    })
                    .ToList();

                return topDiscountedProducts;
            }
            catch (Exception ex)
            {
                // Ghi log lỗi hoặc xử lý tùy theo yêu cầu dự án
                Console.WriteLine($"An error occurred: {ex.Message}");
                return Enumerable.Empty<ProductVm>();
            }
        }
    }


}