using ServerApp.BLL.Services.Base;
using ServerApp.DAL.Infrastructure;
using ServerApp.DAL.Models;
using ServerApp.DAL.Repositories.Generic;

namespace ServerApp.BLL.Services
{
    public class ReviewService : BaseService<Review>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGenericRepository<Review> _reviewRepository;

        public ReviewService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _reviewRepository = unitOfWork.GenericRepository<Review>();
        }

        public async Task<int> AddAsync(Review entity)
        {
            await _reviewRepository.AddAsync(entity);
            return await _unitOfWork.SaveChangesAsync();
        }

        public async Task<bool> UpdateAsync(Review entity)
        {
            await _reviewRepository.UpdateAsync(entity);
            return await _unitOfWork.SaveChangesAsync() > 0;
        }

        public bool Delete(int id)
        {
            var entity = _reviewRepository.GetByIdAsync(id);
            if (entity != null)
            {
                _reviewRepository.DeleteAsync(id);
                _unitOfWork.SaveChanges();
                return true;
            }
            return false;
        }

        public async Task<Review?> GetByIdAsync(int id)
        {
            return await _reviewRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Review>> GetAllAsync()
        {
            return await _reviewRepository.GetAllAsync();
        }
    }

}
