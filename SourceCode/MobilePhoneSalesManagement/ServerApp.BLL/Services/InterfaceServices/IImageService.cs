using ServerApp.BLL.Services.Base;
using ServerApp.BLL.Services.ViewModels;
using ServerApp.DAL.Models;

namespace ServerApp.BLL.Services.InterfaceServices
{
    public interface IImageService : IBaseService<Image>
    {
        Task<int> AddImageAsync(ImageRequest imageRequest);

        Task<int> UpdateImageAsync(int id, ImageRequest imageRequest);
        Task<ImageRequest?> GetByImageIdAsync(int id);
        Task<PagedResult<Image>> GetAllImageId(int? pageNumber, int? pageSize);

    }
}
