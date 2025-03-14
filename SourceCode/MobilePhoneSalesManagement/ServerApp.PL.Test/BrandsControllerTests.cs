using Microsoft.AspNetCore.Mvc;
using Moq;
using ServerApp.BLL.Services;
using ServerApp.BLL.Services.InterfaceServices;
using ServerApp.BLL.Services.ViewModels;
using ServerApp.DAL.Models;
using ServerApp.PL.Controllers;
using System.Linq.Expressions;

namespace ServerApp.PL.Test
{
    [TestFixture]
    public class BrandsControllerTests
    {
        private Mock<IBrandService> _mockBrandService;
        private BrandsController _controller;

        [SetUp]
        public void SetUp()
        {
            // Tạo mock cho IBrandService
            _mockBrandService = new Mock<IBrandService>();

            // Khởi tạo controller với mock service
            _controller = new BrandsController(_mockBrandService.Object);
        }

        [Test]
        public async Task GetBrands_ReturnsOkResult_WithBrands_ByPage()
        {
            // Arrange: Thiết lập dữ liệu giả mock trả về từ service
            var mockBrands = new List<BrandVm> { new BrandVm { BrandId = 1, Name = "Brand1" } };
            var pagedResult = new PagedResult<BrandVm>
            {
                Items = mockBrands,
                TotalCount = mockBrands.Count
            };

            // Đảm bảo gọi đúng phiên bản GetAllBrandAsync và trả về PagedResult
            _mockBrandService.Setup(service => service.GetAllBrandAsync(
                    It.IsAny<int?>(),
                    It.IsAny<int?>(),
                    It.IsAny<Expression<Func<Brand, bool>>>(),
                    "updatedDate",
                    true))
                .Returns(Task.FromResult(pagedResult));

            // Act: Gọi phương thức GetBrands trên controller
            var result = await _controller.GetBrands(1, 10, "updatedDate", true);

            // Assert: Kiểm tra kết quả trả về
            var actionResult = result.Result as OkObjectResult;
            Assert.IsNotNull(actionResult);
            Assert.AreEqual(200, actionResult.StatusCode);

            // Kiểm tra giá trị trả về là PagedResult, không phải IEnumerable trực tiếp
            var resultValue = actionResult.Value as PagedResult<BrandVm>;
            Assert.IsNotNull(resultValue);
            Assert.AreEqual(mockBrands.Count, resultValue.TotalCount);
            Assert.AreEqual(mockBrands, resultValue.Items);
        }

    }
}
