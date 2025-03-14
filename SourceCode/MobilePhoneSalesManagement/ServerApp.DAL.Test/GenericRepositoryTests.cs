using Microsoft.EntityFrameworkCore;
using ServerApp.DAL.Data;
using ServerApp.DAL.Models;
using ServerApp.DAL.Repositories.Generic;
using System.Linq.Expressions;

[TestFixture]
public class GenericRepositoryTests
{
    private DbContextOptions<ShopDbContext> _options;
    private ShopDbContext _context;
    private GenericRepository<Brand> _repository;

    [SetUp]
    public void Setup()
    {
        // Cấu hình lại cơ sở dữ liệu InMemory cho mỗi test
        _options = new DbContextOptionsBuilder<ShopDbContext>()
                      .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())  // Sử dụng tên ngẫu nhiên cho mỗi test
                      .Options;

        _context = new ShopDbContext(_options);
        _repository = new GenericRepository<Brand>(_context);
    }

    [TearDown]
    public void TearDown()
    {
        // Dọn dẹp sau mỗi test case (nghĩa là xóa dữ liệu trong bộ nhớ)
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    // Test GetQuery
    [Test]
    public void GetQuery_ReturnsQueryable()
    {
        // Act
        var result = _repository.GetQuery();

        // Assert
        Assert.IsInstanceOf<IQueryable<Brand>>(result);
    }

    // Test FirstOrDefaultAsync
    [Test]
    public async Task FirstOrDefaultAsync_ReturnsFirstOrDefault()
    {
        // Arrange
        var brand = new Brand { BrandId = 1, Name = "Test Brand" };
        await _context.Brands.AddAsync(brand);
        await _context.SaveChangesAsync();

        Expression<Func<Brand, bool>> predicate = p => p.BrandId == 1;

        // Act
        var result = await _repository.FirstOrDefaultAsync(predicate);

        // Assert
        Assert.NotNull(result);
        Assert.AreEqual(brand.BrandId, result?.BrandId);
        Assert.AreEqual(brand.Name, result?.Name);
    }

    [Test]
    public async Task GetAsync_ReturnsFilteredResultWithOrderAndInclude()
    {
        // Arrange
        var brand1 = new Brand { BrandId = 1, Name = "Brand 1" };
        var brand2 = new Brand { BrandId = 2, Name = "Brand 2" };

        await _context.Brands.AddRangeAsync(brand1, brand2);
        await _context.SaveChangesAsync();

        Expression<Func<Brand, bool>> filter = p => p.BrandId == 1;
        Func<IQueryable<Brand>, IOrderedQueryable<Brand>> orderBy = query => query.OrderBy(b => b.Name);

        // Act
        var result = await _repository.GetAsync(filter, orderBy);

        // Assert
        Assert.NotNull(result);
        Assert.AreEqual(1, result?.BrandId);
        Assert.AreEqual("Brand 1", result?.Name);
    }

    // Test GetAllAsync
    [Test]
    public async Task GetAllAsync_ReturnsFilteredResult()
    {
        // Arrange
        var brand1 = new Brand { BrandId = 1, Name = "Brand 1" };
        var brand2 = new Brand { BrandId = 2, Name = "Brand 2" };
        await _context.Brands.AddRangeAsync(brand1, brand2);
        await _context.SaveChangesAsync();

        Expression<Func<Brand, bool>> filter = p => p.BrandId == 1;

        // Act
        var result = await _repository.GetAllAsync(filter);

        // Assert
        Assert.NotNull(result);
        var resultList = result.ToList();  // Chuyển đổi kết quả sang danh sách
        Assert.That(resultList, Has.Count.EqualTo(1)); // Kiểm tra số lượng
        Assert.AreEqual(1, resultList.First().BrandId);  // Kiểm tra giá trị
    }

    [Test]
    public async Task ModifyAsync_UpdatesEntity()
    {
        // Arrange
        var brand = new Brand { BrandId = 1, Name = "Test Brand" };
        await _context.Brands.AddAsync(brand);
        await _context.SaveChangesAsync();

        // Cập nhật thực thể
        brand.Name = "Updated Brand";

        // Act
        var result = await _repository.ModifyAsync(brand);

        // Assert
        Assert.AreEqual(1, result); // Mong đợi một dòng bị ảnh hưởng
        var updatedBrand = await _context.Brands.FindAsync(brand.BrandId);

        // Kiểm tra thực thể đã được cập nhật
        Assert.NotNull(updatedBrand);
        Assert.AreEqual("Updated Brand", updatedBrand?.Name);
    }
}
