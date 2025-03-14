using ServerApp.BLL.Services.Base;
using ServerApp.BLL.Services.InterfaceServices;
using ServerApp.BLL.Services.ViewModels;
using ServerApp.DAL.Infrastructure;
using ServerApp.DAL.Models;

namespace ServerApp.BLL.Services
{

    public class SpecificationTypeService : BaseService<SpecificationType>, ISpecificationTypeService
    {
        private readonly IUnitOfWork _unitOfWork;

        public SpecificationTypeService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<SpecificationTypeVm>> GetAllSpecificationTypeAsync()
        {
            var specificationTypes = await GetAllAsync();

            var SpecificationTypeViewModels = specificationTypes.Select(specificationType => new SpecificationTypeVm
            {
                SpecificationTypeId = specificationType.SpecificationTypeId,
                Name = specificationType.Name,
                CreatedAt = specificationType.CreatedAt,
                UpdatedAt = specificationType.UpdatedAt
            });

            return SpecificationTypeViewModels;
        }

        public async Task<SpecificationTypeVm?> GetBySpecificationTypeIdAsync(int id)
        {
            var specificationType = await GetByIdAsync(id);
            if (specificationType == null) return null;
            var specificationTypeVm = new SpecificationTypeVm
            {
                SpecificationTypeId = specificationType.SpecificationTypeId,
                Name = specificationType.Name,
                CreatedAt = specificationType.CreatedAt,
                UpdatedAt = specificationType.UpdatedAt
            };

            return specificationTypeVm;
        }
        public async Task<SpecificationTypeVm> AddSpecificationTypeAsync(InputSpecificationTypeVm specificationTypeVm)
        {
            ValidateModelPropertiesWithAttribute(specificationTypeVm);

            var findSpecificationType = await _unitOfWork.GenericRepository<SpecificationType>().GetAsync(b =>
                b.Name == specificationTypeVm.Name
            );

            if (findSpecificationType != null)
            {
                throw new ExceptionBusinessLogic("SpecificationType name is already in use.");
            }

            var specificationType = new SpecificationType
            {
                Name = specificationTypeVm.Name
            };

            var result = await AddAsync(specificationType);
            if (result <= 0)
            {
                throw new ArgumentException("Failed to add SpecificationType.");
            }

            return new SpecificationTypeVm
            {
                SpecificationTypeId = specificationType.SpecificationTypeId,
                Name = specificationType.Name
            };
        }

        public async Task<SpecificationTypeVm> UpdateSpecificationTypeAsync(int id, InputSpecificationTypeVm specificationTypeVm)
        {
            ValidateModelPropertiesWithAttribute(specificationTypeVm);

            var specificationType = await _unitOfWork.GenericRepository<SpecificationType>().GetByIdAsync(id);
            if (specificationType == null)
            {
                throw new ArgumentException("SpecificationType not found.");
            }

            var findSpecificationType = await _unitOfWork.GenericRepository<SpecificationType>().GetAsync(b =>
                b.SpecificationTypeId != id &&
                b.Name == specificationTypeVm.Name
            );

            if (findSpecificationType != null)
            {
                throw new ExceptionBusinessLogic("SpecificationType name is already in use.");
            }

            specificationType.Name = specificationTypeVm.Name;
            specificationType.UpdatedAt = DateTime.Now;

            var result = await _unitOfWork.GenericRepository<SpecificationType>().ModifyAsync(specificationType);
            if (result <= 0)
            {
                throw new ArgumentException("Failed to update SpecificationType.");
            }

            return new SpecificationTypeVm
            {
                SpecificationTypeId = specificationType.SpecificationTypeId,
                Name = specificationType.Name,
                CreatedAt = specificationType.CreatedAt
            };
        }

        public async Task<SpecificationTypeVm> DeleteSpecificationTypeAsync(int id)
        {
            var specificationType = await _unitOfWork.GenericRepository<SpecificationType>().GetByIdAsync(id);

            if (specificationType == null) return null;

            // Lưu thay đổi vào cơ sở dữ liệu
            _unitOfWork.GenericRepository<SpecificationType>().Delete(id);

            if (_unitOfWork.SaveChanges() > 0)
            {
                return new SpecificationTypeVm
                {
                    SpecificationTypeId = specificationType.SpecificationTypeId,
                    Name = specificationType.Name
                };
            }

            // Nếu lưu thất bại
            throw new ArgumentException("Failed to delete specificationType");

        }

    }

}
