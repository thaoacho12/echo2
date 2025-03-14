
using Moq;
using NUnit.Framework;
using ServerApp.BLL.Services.Base;
using ServerApp.BLL.Services.InterfaceServices;
using ServerApp.DAL.Infrastructure;
using ServerApp.DAL.Models;
using ServerApp.DAL.Repositories.Generic;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServerApp.BLL.Test.BaseServiceTests.DeleteMultipleAsyncTests
{
    [TestFixture]
    public class DeleteMultipleAsyncTests
    {
        private Mock<IUnitOfWork> _unitOfWorkMock;
        private Mock<IGenericRepository<SomeEntity>> _repositoryMock;
        private SomeService _service;

        [SetUp]
        public void Setup()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _repositoryMock = new Mock<IGenericRepository<SomeEntity>>();
            _unitOfWorkMock.Setup(u => u.GenericRepository<SomeEntity>()).Returns(_repositoryMock.Object);
            _service = new SomeService(_unitOfWorkMock.Object);
        }



        [Test]
        public async Task DeleteMultipleAsync_ConditionFails_CallsOnConditionFailed()
        {
            // Arrange
            var ids = new List<int> { 1, 2, 3 };
            var onConditionFailedCalled = 0;

            _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((int id) => new SomeEntity { Id = id });

            // Act
            var result = await _service.DeleteMultipleAsync(
                ids,
                entity => false,
                async entity =>
                {
                    onConditionFailedCalled++;
                    await Task.CompletedTask;
                });

            // Assert
            Assert.AreEqual(0, result.countRemoved);
            Assert.AreEqual(3, result.countUpdated);
            Assert.AreEqual(3, onConditionFailedCalled);

            // Kiểm tra SaveChangesAsync đã được gọi đúng một lần
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

            // Kiểm tra CommitTransactionAsync cũng được gọi đúng một lần
            _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(), Times.Once);
        }





        [Test]
        public void DeleteMultipleAsync_DeleteThrowsException_RollsBackTransaction()
        {
            // Arrange
            var ids = new List<int> { 1 };
            _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(new SomeEntity { Id = 1 });
            _repositoryMock.Setup(r => r.DeleteAsync(It.IsAny<int>())).ThrowsAsync(new Exception("Repository error"));

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(() =>
                _service.DeleteMultipleAsync(ids, entity => true, async e => await Task.CompletedTask));
            Assert.That(ex.Message, Does.Contain("Failed to delete entities."));
            _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(), Times.Once);
        }


    }

    // Mô hình giả lập
    public class SomeEntity
    {
        public int Id { get; set; }
    }
    public interface ISomeEntityService : IBaseService<SomeEntity>
    { }
    public class SomeService : BaseService<SomeEntity>, ISomeEntityService
    {
        private readonly IUnitOfWork _unitOfWork;

        public SomeService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

    }

}
