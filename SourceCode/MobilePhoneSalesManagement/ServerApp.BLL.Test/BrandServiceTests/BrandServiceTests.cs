using Microsoft.EntityFrameworkCore;
using ServerApp.BLL.Services;
using ServerApp.BLL.Services.InterfaceServices;
using ServerApp.DAL.Data;
using ServerApp.DAL.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ServerApp.DAL.Data;
using System.Linq.Expressions;
using ServerApp.BLL.Services.InterfaceServices;
using ServerApp.DAL.Models;

namespace ServerApp.Tests
{
    [TestFixture]
    public class BrandServiceTests
    {
        private DbContextOptions<ShopDbContext> _dbContextOptions;
        private ShopDbContext _dbContext;
        private UnitOfWork _unitOfWork;
        private BrandService _brandService;
        private readonly IImageService _imageService;

        [SetUp]
        public void SetUp()
        {
            // Cấu hình DbContext với in-memory database
            _dbContextOptions = new DbContextOptionsBuilder<ShopDbContext>()
                .UseInMemoryDatabase("InMemoryDb") // Sử dụng in-memory database
                .Options;

            _dbContext = new ShopDbContext(_dbContextOptions);

            // Tạo UnitOfWork và BrandService
            _unitOfWork = new UnitOfWork(_dbContext);
            _brandService = new BrandService(_unitOfWork, _imageService);

            // Thêm dữ liệu mẫu vào cơ sở dữ liệu
            SeedDatabase();
        }

        private void SeedDatabase()
        {
            // Thêm dữ liệu mẫu vào database in-memory
            var brands = new List<Brand>
            {
                new Brand
                {
                    BrandId = 1,
                    Name = "Brand 1",
                    ImageId = 1,
                    IsActive = true,
                    Products = new List<Product>(),
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                },
                new Brand
                {
                    BrandId = 2,
                    Name = "Brand 2",
                    ImageId = 2,
                    IsActive = true,
                    Products = new List<Product>(),
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                }
            };

            _dbContext.Brands.AddRange(brands);
            _dbContext.SaveChanges();
        }

        [Test]
        public async Task GetAllBrandAsync_ShouldReturnListOfBrandVm()
        {
            // Act
            var result = await _brandService.GetAllBrandAsync();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(2));
            Assert.That(result.First().Name, Is.EqualTo("Brand 1"));
            Assert.That(result.Last().Name, Is.EqualTo("Brand 2"));
        }


        //[Test]
        //public async Task GetAllBrandAsync_ShouldReturnPagedResult()
        //{
        //    // Act
        //    var result = await _brandService.GetAllBrandAsync(pageNumber: 1, pageSize: 2, filter: null, sortField: "UpdatedAt", orderBy: true);

        //    Assert.AreEqual(2,result.Items.ToList().Count);
        //    Assert.AreEqual(3, result.TotalCount); 
        //    Assert.AreEqual("Brand 2", result.Items.First().Name); // Kiểm tra tên của phần tử đầu tiên
        //    Assert.AreEqual("Brand 3", result.Items.Last().Name);// Kiểm tra item cuối cùng
        //}

        //[Test]
        //public async Task GetAllBrandAsync_WithFilter_ShouldReturnFilteredResult()
        //{
        //    // Define filter to only return active brands
        //    Expression<Func<Brand, bool>> filter = b => b.IsActive == true;

        //    // Act
        //    var result = await _brandService.GetAllBrandAsync(pageNumber: 1, pageSize: 2, filter: filter, sortField : "updatedDate",orderBy:true);

        //    // Assert
        //    Assert.IsNotNull(result);
        //    Assert.AreEqual(2, result.Items.ToList().Count);
        //    Assert.AreEqual(3, result.TotalCount); // Tổng số bản ghi không thay đổi
        //    Assert.AreEqual("Brand 1", result.Items.First().Name); // Kiểm tra tên Brand đầu tiên
        //}

        [TearDown]
        public void TearDown()
        {
            // Giải phóng tài nguyên khi test kết thúc
            _dbContext.Dispose();
            _unitOfWork.Dispose();
        }
    }
}
