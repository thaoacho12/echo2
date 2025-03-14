using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ServerApp.BLL.Services;
using ServerApp.BLL.Services.ViewModels;
using ServerApp.DAL.Infrastructure;
using ServerApp.DAL.Models;
using ServerApp.DAL.Repositories.Generic;
using System.Linq.Expressions;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace ServerApp.BLL.Test.ProductServiceTests
{
    public class ProductServiceTests
    {
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private Mock<ISpecificationTypeService> _mockSpecificationTypeService;
        private Mock<IImageService> _mockImageService;
        private Mock<IGenericRepository<Product>> _mockProductRepository;
        private ProductService _productService;

        [SetUp]
        public void Setup()
        {
            // Mock các dependencies
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockSpecificationTypeService = new Mock<ISpecificationTypeService>();
            _mockImageService = new Mock<IImageService>();
            _mockProductRepository = new Mock<IGenericRepository<Product>>();
            // Giả lập dữ liệu cho các phương thức của repository
            var mockProductRepository = new Mock<IGenericRepository<Product>>();
            mockProductRepository.Setup(repo => repo.GetAllAsync(It.IsAny<Expression<Func<Product, bool>>?>()))
                .ReturnsAsync(GetSampleProducts());
            _mockUnitOfWork.Setup(uow => uow.GenericRepository<Product>())
                .Returns(mockProductRepository.Object);

            var mockSpecTypeRepository = new Mock<IGenericRepository<SpecificationType>>();
            mockSpecTypeRepository.Setup(repo => repo.GetQuery())
                .Returns(GetSampleSpecificationTypes().AsQueryable());
            _mockUnitOfWork.Setup(uow => uow.GenericRepository<SpecificationType>())
                .Returns(mockSpecTypeRepository.Object);



            // Mock các method của IUnitOfWork và các repository
            _mockUnitOfWork.Setup(u => u.GenericRepository<Product>()).Returns(_mockProductRepository.Object);

            // Khởi tạo ProductService với các mock
            _productService = new ProductService(
                _mockUnitOfWork.Object,
                _mockSpecificationTypeService.Object,
                _mockImageService.Object
            );
        }

        [Test]
        public async Task AddProductAsync_ShouldAddProduct_WhenValidProductVm()
        {
            // Arrange: Tạo InputProductVm hợp lệ
            var inputProductVm = new InputProductVm
            {
                Name = "Product 1",
                Description = "Description",
                Price = 100,
                StockQuantity = 10,
                BrandId = 1,
                ImageId = 1,
                Image = new ImageRequest { Name = "image.jpg", ImageBase64 = "base64" },
                ProductSpecifications = new List<InputProductSpecificationVm>()
            };

            _mockImageService.Setup(m => m.AddImageAsync(It.IsAny<ImageRequest>()))
                .ReturnsAsync(1); // Giả sử ID hình ảnh trả về là 1
            _mockProductRepository.Setup(r => r.AddAsync(It.IsAny<Product>())).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act: Gọi AddProductAsync
            var result = await _productService.AddProductAsync(inputProductVm);

            // Assert: Kiểm tra kết quả trả về và các phương thức đã được gọi
            Assert.NotNull(result);
            Assert.AreEqual(inputProductVm.Name, result.Name);
            _mockImageService.Verify(m => m.AddImageAsync(It.IsAny<ImageRequest>()), Times.Once);
            _mockProductRepository.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        [Test]
        public async Task DeleteProductAsync_ShouldDeleteProduct_WhenProductIsInactive()
        {
            // Arrange: Tạo sản phẩm có IsActive = false và ImageId = 1
            var product = new Product
            {
                ProductId = 1,
                Name = "Product 1",
                Description = "This is a product description.",
                Price = 100.00m,
                OldPrice = 120.00m,
                StockQuantity = 50,
                BrandId = 1,
                Manufacturer = "Manufacturer A",
                IsActive = false,
                Colors = "Red, Blue",
                Discount = 10,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                ProductSpecifications = new List<ProductSpecification>(),
                ImageId = 1,
                Image = new Image() // Cung cấp thông tin hình ảnh nếu cần thiết
            };

            // Mock trả về sản phẩm với IsActive = false
            _mockProductRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);

            // Mock xóa sản phẩm và xóa ảnh liên quan
            _mockProductRepository.Setup(r => r.Delete(It.IsAny<int>())).Verifiable();
            _mockUnitOfWork.Setup(u => u.SaveChanges()).Returns(1); // Mock SaveChangesAsync trả về 1
            _mockUnitOfWork.Setup(u => u.GenericRepository<Image>().Delete(It.IsAny<int>())).Verifiable(); // Mock xóa ảnh

            // Act: Gọi DeleteProductAsync
            var result = await _productService.DeleteProductAsync(1);

            // Assert: Kiểm tra sản phẩm đã bị xóa không và ảnh đã bị xóa
            Assert.NotNull(result);
            _mockProductRepository.Verify(r => r.Delete(It.IsAny<int>()), Times.Once); // Kiểm tra Delete sản phẩm đã được gọi
            _mockUnitOfWork.Verify(u => u.SaveChanges(), Times.Once); // Kiểm tra SaveChangesAsync đã được gọi
            _mockUnitOfWork.Verify(u => u.GenericRepository<Image>().Delete(It.IsAny<int>()), Times.Once); // Kiểm tra Delete ảnh đã được gọi
        }


        [Test]
        public async Task DeleteProductAsync_ShouldThrowException_WhenProductNotFound()
        {
            // Arrange: Mock trường hợp không tìm thấy sản phẩm
            _mockProductRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Product)null);

            // Act & Assert: Kiểm tra ngoại lệ
            var ex = Assert.ThrowsAsync<ExceptionNotFound>(async () => await _productService.DeleteProductAsync(1));
            Assert.AreEqual("Product not found", ex.Message);
        }
        [Test]
        public async Task FilterProductsAsync_WithSortingByPriceAscending_ReturnsSortedResults()
        {
            // Arrange
            var filterRequest = new FilterRequest
            {
                Sort = "giathap"
            };

            // Act
            var (products, _) = await _productService.FilterProductsAsync(filterRequest);

            // Assert
            Assert.IsTrue(products.SequenceEqual(products.OrderBy(p => p.Price)));
        }

        [Test]
        public async Task FilterProductsAsync_WithSortingByPriceDescending_ReturnsSortedResults()
        {
            // Arrange
            var filterRequest = new FilterRequest
            {
                Sort = "giacao"
            };

            // Act
            var (products, _) = await _productService.FilterProductsAsync(filterRequest);

            // Assert
            Assert.IsTrue(products.SequenceEqual(products.OrderByDescending(p => p.Price)));
        }
        private List<Product> GetSampleProducts()
        {
            var specificationTypes = GetSampleSpecificationTypes();

            return new List<Product>
    {
        new Product
        {
            ProductId = 1,
            Name = "Product1",
            Price = 1500000,
            Brand = new Brand { Name = "BrandA" },
            ProductSpecifications = new List<ProductSpecification>
            {
                new ProductSpecification
                {
                    SpecificationType = specificationTypes.First(st => st.Name == "ScreenSize"),
                    Value = "6.5"
                },
                new ProductSpecification
                {
                    SpecificationType = specificationTypes.First(st => st.Name == "Storage"),
                    Value = "32"
                }
            }
        },
        new Product
        {
            ProductId = 2,
            Name = "Product2",
            Price = 5000000,
            Brand = new Brand { Name = "BrandB" },
            ProductSpecifications = new List<ProductSpecification>
            {
                new ProductSpecification
                {
                    SpecificationType = specificationTypes.First(st => st.Name == "ScreenSize"),
                    Value = "5.0"
                },
                new ProductSpecification
                {
                    SpecificationType = specificationTypes.First(st => st.Name == "Storage"),
                    Value = "64"
                },
                new ProductSpecification
                {
                    SpecificationType = specificationTypes.First(st => st.Name == "BatteryCapacity"),
                    Value = "5000mAh"
                }
            }
        }
    };
        }
        private List<SpecificationType> GetSampleSpecificationTypes()
        {
            return new List<SpecificationType>
    {
        new SpecificationType
        {
            SpecificationTypeId = 1,
            Name = "ScreenSize"
        },
        new SpecificationType
        {
            SpecificationTypeId = 2,
            Name = "Storage"
        },
        new SpecificationType
        {
            SpecificationTypeId = 3,
            Name = "BatteryCapacity"
        },
        new SpecificationType
        {
            SpecificationTypeId = 4,
            Name = "CameraResolution"
        },
        new SpecificationType
        {
            SpecificationTypeId = 5,
            Name = "Processor"
        }
    };
        }
        [Test]
        public async Task FilterProductsAsync_WithInternalMemoryFilter_ReturnsCorrectProducts()
        {
            // Arrange
            var filterRequest = new FilterRequest
            {
                InternalMemory = new List<string> { "under32" }
            };

            // Act
            var (products, _) = await _productService.FilterProductsAsync(filterRequest);

            // Assert
            Assert.IsTrue(products.All(p => p.ProductSpecifications.Any(ps => ps.SpecificationType.Name == "Storage" && int.Parse(ps.Value) <= 32)));
        }
        [Test]
        public async Task GetNewestProductsAsync_ShouldReturnEmptyList_WhenNoProductsExist()
        {
            // Arrange: Không có sản phẩm nào
            _mockProductRepository.Setup(r => r.GetAllAsync(It.IsAny<Expression<Func<Product, bool>>?>()))
                .ReturnsAsync(new List<Product>());

            // Act: Gọi phương thức GetNewestProductsAsync
            var result = await _productService.GetNewestProductsAsync();

            // Assert: Kiểm tra kết quả trả về là danh sách trống
            Assert.IsEmpty(result);
        }
        [Test]
        public async Task GetNewestProductsAsync_ShouldReturnEmptyList_WhenExceptionOccurs()
        {
            // Arrange: Giả lập trường hợp có lỗi xảy ra khi truy vấn cơ sở dữ liệu
            _mockProductRepository.Setup(r => r.GetAllAsync(It.IsAny<Expression<Func<Product, bool>>?>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act: Gọi phương thức GetNewestProductsAsync
            var result = await _productService.GetNewestProductsAsync();

            // Assert: Kiểm tra kết quả trả về là danh sách trống khi có lỗi
            Assert.IsEmpty(result);
        }
        [Test]
        public void SearchProductsByNameAsync_ShouldThrowArgumentException_WhenNameIsEmpty()
        {
            // Arrange: Trường hợp tìm kiếm với tên rỗng
            var searchTerm = string.Empty;

            // Act & Assert: Kiểm tra ngoại lệ
            var ex = Assert.ThrowsAsync<ArgumentException>(async () => await _productService.SearchProductsByNameAsync(searchTerm));
            Assert.AreEqual("Search term cannot be null or empty.", ex.Message);
        }
        [Test]
        public void SearchProductsByNameAsync_ShouldThrowArgumentException_WhenNameIsNull()
        {
            // Arrange: Trường hợp tìm kiếm với tên null
            string searchTerm = null;

            // Act & Assert: Kiểm tra ngoại lệ
            var ex = Assert.ThrowsAsync<ArgumentException>(async () => await _productService.SearchProductsByNameAsync(searchTerm));
            Assert.AreEqual("Search term cannot be null or empty.", ex.Message);
        }
        [Test]
        public async Task GetTopDiscountedProductsAsync_ShouldReturnEmpty_WhenNoProductsHaveDiscount()
        {
            // Arrange: Giả lập danh sách sản phẩm không có sản phẩm nào giảm giá
            var sampleProducts = new List<Product>
    {
        new Product { ProductId = 1, Name = "Product 1", Discount = 0, Price = 100 },
        new Product { ProductId = 2, Name = "Product 2", Discount = 0, Price = 200 }
    };

            _mockProductRepository.Setup(r => r.GetAllAsync(It.IsAny<Expression<Func<Product, bool>>?>()))
                .ReturnsAsync(sampleProducts);

            // Act: Gọi GetTopDiscountedProductsAsync
            var result = await _productService.GetTopDiscountedProductsAsync();

            // Assert: Kiểm tra kết quả trả về
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count()); // Không có sản phẩm giảm giá
        }
        [Test]
        public async Task GetTopDiscountedProductsAsync_ShouldReturnEmpty_WhenExceptionOccurs()
        {
            // Arrange: Giả lập lỗi khi gọi GetAllAsync
            _mockProductRepository.Setup(r => r.GetAllAsync(It.IsAny<Expression<Func<Product, bool>>?>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act: Gọi GetTopDiscountedProductsAsync
            var result = await _productService.GetTopDiscountedProductsAsync();

            // Assert: Kiểm tra kết quả trả về là danh sách rỗng
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count()); // Khi có lỗi, trả về danh sách rỗng
        }
        [Test]
        public async Task UpdateProductAsync_ShouldUpdateProduct_WhenValidProductIsProvided()
        {
            // Arrange: Giả lập sản phẩm và thiết lập dữ liệu cũ
            var existingProduct = new Product
            {
                ProductId = 1,
                Name = "Product 1",
                Description = "Description 1",
                Price = 100,
                OldPrice = 120,
                StockQuantity = 10,
                BrandId = 1,
                ImageId = 1,
                Manufacturer = "Manufacturer 1",
                IsActive = true,
                Colors = "Red",
                Discount = 10,
                UpdatedAt = DateTime.Now.AddMonths(-1)
            };

            var updatedProduct = new Product
            {
                ProductId = 1,
                Name = "Updated Product 1",
                Description = "Updated Description 1",
                Price = 90,
                OldPrice = 110,
                StockQuantity = 20,
                BrandId = 1,
                ImageId = 2,
                Manufacturer = "Updated Manufacturer",
                IsActive = true,
                Colors = "Blue",
                Discount = 20
            };

            _mockUnitOfWork.Setup(u => u.GenericRepository<Product>().ModifyAsync(It.IsAny<Product>()))
                .ReturnsAsync(1); // Giả lập thành công cập nhật (trả về số bản ghi đã được thay đổi)

            // Act: Gọi UpdateProductAsync
            var result = await _productService.UpdateProductAsync(updatedProduct);

            // Assert: Kiểm tra kết quả trả về là 1 (cập nhật thành công)
            Assert.AreEqual(1, result);
        }
        [Test]
        public async Task UpdateProductAsync_ShouldReturnZero_WhenProductDoesNotExist()
        {
            // Arrange: Giả lập một sản phẩm không tồn tại trong cơ sở dữ liệu
            var updatedProduct = new Product
            {
                ProductId = 999, // ID không tồn tại
                Name = "Non-existing Product",
                Description = "Non-existing Description",
                Price = 200,
                OldPrice = 250,
                StockQuantity = 5,
                BrandId = 1,
                ImageId = 1,
                Manufacturer = "Non-existing Manufacturer",
                IsActive = true,
                Colors = "Green",
                Discount = 15
            };

            _mockUnitOfWork.Setup(u => u.GenericRepository<Product>().ModifyAsync(It.IsAny<Product>()))
                .ReturnsAsync(0); // Giả lập không tìm thấy sản phẩm, không có bản ghi nào bị thay đổi

            // Act: Gọi UpdateProductAsync
            var result = await _productService.UpdateProductAsync(updatedProduct);

            // Assert: Kiểm tra kết quả trả về là 0 (không có bản ghi nào bị thay đổi)
            Assert.AreEqual(0, result);
        }
        [Test]
        public async Task UpdateProductAsync_ShouldReturnZero_WhenNoChangesInProduct()
        {
            // Arrange: Giả lập sản phẩm cũ và sản phẩm cập nhật không có thay đổi
            var existingProduct = new Product
            {
                ProductId = 1,
                Name = "Product 1",
                Description = "Description 1",
                Price = 100,
                OldPrice = 120,
                StockQuantity = 10,
                BrandId = 1,
                ImageId = 1,
                Manufacturer = "Manufacturer 1",
                IsActive = true,
                Colors = "Red",
                Discount = 10,
                UpdatedAt = DateTime.Now.AddMonths(-1)
            };

            var updatedProduct = new Product
            {
                ProductId = 1,
                Name = "Product 1",
                Description = "Description 1",
                Price = 100,
                OldPrice = 120,
                StockQuantity = 10,
                BrandId = 1,
                ImageId = 1,
                Manufacturer = "Manufacturer 1",
                IsActive = true,
                Colors = "Red",
                Discount = 10
            };

            _mockUnitOfWork.Setup(u => u.GenericRepository<Product>().ModifyAsync(It.IsAny<Product>()))
                .ReturnsAsync(0); // Không có thay đổi nào nên trả về 0 (số bản ghi bị thay đổi)

            // Act: Gọi UpdateProductAsync
            var result = await _productService.UpdateProductAsync(updatedProduct);

            // Assert: Kiểm tra kết quả trả về là 0 (không có thay đổi)
            Assert.AreEqual(0, result);
        }
        [Test]
        public async Task UpdateProductAsync_ShouldUpdateProductWithAllInformation_WhenValidDataIsProvided()
        {
            // Arrange: Giả lập một sản phẩm với đầy đủ thông tin cập nhật
            var updatedProduct = new Product
            {
                ProductId = 1,
                Name = "Updated Product 1",
                Description = "Updated Description 1",
                Price = 90,
                OldPrice = 110,
                StockQuantity = 20,
                BrandId = 1,
                ImageId = 2,
                Manufacturer = "Updated Manufacturer",
                IsActive = true,
                Colors = "Blue",
                Discount = 20
            };

            _mockUnitOfWork.Setup(u => u.GenericRepository<Product>().ModifyAsync(It.IsAny<Product>()))
                .ReturnsAsync(1); // Giả lập cập nhật thành công

            // Act: Gọi UpdateProductAsync
            var result = await _productService.UpdateProductAsync(updatedProduct);

            // Assert: Kiểm tra kết quả trả về là 1 (cập nhật thành công)
            Assert.AreEqual(1, result);
        }
    }
}