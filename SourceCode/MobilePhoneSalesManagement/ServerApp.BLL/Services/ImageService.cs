using ServerApp.BLL.Services.Base;
using ServerApp.BLL.Services.InterfaceServices;
using ServerApp.BLL.Services.ViewModels;
using ServerApp.DAL.Data;
using ServerApp.DAL.Infrastructure;
using ServerApp.DAL.Models;
using ServerApp.DAL.Seed;
namespace ServerApp.BLL.Services
{
    public class ImageService : BaseService<Image>, IImageService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ShopDbContext _context;

        public ImageService(IUnitOfWork unitOfWork, ShopDbContext context) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        public async Task<PagedResult<Image>> GetAllImageId(int? pageNumber, int? pageSize)
        {
            int currentPage = pageNumber ?? 1;
            int currentPageSize = pageSize ?? 10;

            var query = await GetAllAsync();

            var totalCount = query.Count();
            var paginatedImages = query
                .OrderBy(u => u.ImageId)
                .Skip((currentPage - 1) * currentPageSize)
                .Take(currentPageSize)
                .ToList();
            var imageRequests = paginatedImages.Select(image => new Image
            {
                ImageId = image.ImageId,
                Name = image.Name,
            });


            return new PagedResult<Image>
            {
                CurrentPage = currentPage,
                PageSize = currentPageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / currentPageSize),
                Items = imageRequests
            };
        }


        public async Task<ImageRequest?> GetByImageIdAsync(int id)
        {
            var image = await _unitOfWork.GenericRepository<Image>().GetByIdAsync(id);
            if (image == null)
            {
                throw new ExceptionNotFound("Image not found");
            }

            // Chuyển mảng byte thành Base64 string
            string base64Image = Convert.ToBase64String(image.ImageData);
            var imageRequest = new ImageRequest
            {
                Name = image.Name,
                ImageBase64 = base64Image
            };
            return imageRequest;

        }
        public async Task<int> AddImageAsync(ImageRequest imageRequest)
        {
            if (string.IsNullOrEmpty(imageRequest.ImageBase64) || !imageRequest.ImageBase64.Contains(","))
            {
                imageRequest.ImageBase64 = $"," + SeedData.image_default;
            }
            //ValidateModelPropertiesWithAttribute(imageRequest);
            var image_data = imageRequest.ImageBase64.Split(",")[1];
            var image = new Image { Name = imageRequest.Name, ImageData = Convert.FromBase64String(image_data) };
            await _unitOfWork.GenericRepository<Image>().AddAsync(image);
            await _context.SaveChangesAsync();
            return image.ImageId;
        }
        public async Task<int> UpdateImageAsync(int id, ImageRequest imageRequest)
        {

            //ValidateModelPropertiesWithAttribute(imageRequest);
            var image = await _unitOfWork.GenericRepository<Image>().GetByIdAsync(id);
            if (image == null)
            {
                throw new ExceptionNotFound("Image not found");
            }

            // Chuyển mảng byte thành Base64 string
            string base64Image = Convert.ToBase64String(image.ImageData);

            var image_data = imageRequest.ImageBase64.Split(",")[1];
            image.Name = imageRequest.Name;
            image.ImageData = Convert.FromBase64String(image_data);

            return await _unitOfWork.GenericRepository<Image>().ModifyAsync(image);
        }


    }

}
