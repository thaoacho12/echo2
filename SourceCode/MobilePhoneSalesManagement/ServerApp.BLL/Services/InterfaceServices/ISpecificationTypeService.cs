using ServerApp.BLL.Services.Base;
using ServerApp.BLL.Services.ViewModels;
using ServerApp.DAL.Models;

namespace ServerApp.BLL.Services.InterfaceServices
{
    public interface ISpecificationTypeService : IBaseService<SpecificationType>
    {
        Task<SpecificationTypeVm> AddSpecificationTypeAsync(InputSpecificationTypeVm specificationTypeVm);
        Task<SpecificationTypeVm> UpdateSpecificationTypeAsync(int id, InputSpecificationTypeVm specificationTypeVm);

        Task<SpecificationTypeVm> DeleteSpecificationTypeAsync(int id);

        Task<SpecificationTypeVm?> GetBySpecificationTypeIdAsync(int id);

        Task<IEnumerable<SpecificationTypeVm>> GetAllSpecificationTypeAsync();
    }
}
