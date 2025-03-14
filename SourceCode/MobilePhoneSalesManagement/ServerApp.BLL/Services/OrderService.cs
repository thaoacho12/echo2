using Microsoft.EntityFrameworkCore;
using ServerApp.BLL.Services.Base;
using ServerApp.BLL.Services.InterfaceServices;
using ServerApp.BLL.Services.ViewModels;
using ServerApp.DAL.Infrastructure;
using ServerApp.DAL.Models;

namespace ServerApp.BLL.Services
{
    public class OrderService : BaseService<Order>, IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOrderItemService _orderItemService;

        public OrderService(IUnitOfWork unitOfWork, IOrderItemService orderItemService) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _orderItemService = orderItemService;
        }
        public async Task<PagedResult<OrderAdminVm>> GetAllOrdersAsync(int? pageNumber, int? pageSize, string? keySearch)
        {
            // Xác định các giá trị mặc định cho pageNumber và pageSize nếu chúng null
            int currentPage = pageNumber ?? 1; // Mặc định trang đầu tiên
            int currentPageSize = pageSize ?? 10; // Mặc định 10 bản ghi mỗi trang

            keySearch = keySearch?.Trim()?.ToLower();

            // Lọc dữ liệu
            var query = await _unitOfWork.OrderRepository.GetAllAsync(
            filter: order =>
            (string.IsNullOrEmpty(keySearch) || // Nếu searchTerm trống thì bỏ qua tìm kiếm
            order.FirstName.ToLower().Contains(keySearch) || order.LastName.ToLower().Contains(keySearch) || // Tìm kiếm theo email
            order.OrderId.ToString().Contains(keySearch)),
                include: x => x.Include(x => x.OrderItems).ThenInclude(oi => oi.Product).ThenInclude(p => p.Image)
            );

            var totalCount = query.Count();
            var paginatedOrders = query
                .OrderBy(u => u.UserId)
                .Skip((currentPage - 1) * currentPageSize)
                .Take(currentPageSize)
                .ToList();

            var orderVms = paginatedOrders.Select(order => new OrderAdminVm
            {
                OrderId = order.OrderId,
                CreatedDate = order.CreatedAt,
                CustomerId = order.UserId,
                OrderInfo = new OrderInfoVm
                {
                    Address = order.Address,
                    Country = order.Country,
                    Email = order.Email,
                    FirstName = order.FirstName,
                    LastName = order.LastName,
                    Note = order.Note,
                    Phone = order.Phone,
                    Postcode = order.Postcode
                },
                OrderStatus = order.OrderStatus,
                TotalAmount = order.TotalPrice,
                CartItems = order.OrderItems.Select(item => new OrderItemVm
                {
                    OrderId = item.OrderId,
                    Quantity = item.Quantity,
                    Product = new ProductOrderVm
                    {
                        ProductId = item.Product.ProductId,
                        ImageUrl = item.Product.Image != null
                                ? Convert.ToBase64String(item.Product.Image.ImageData)
                                : null,
                        ProductName = item.Product.Name,
                        Price = item.Product.Price
                    }
                }).ToList()
            });

            return new PagedResult<OrderAdminVm>
            {
                CurrentPage = currentPage,
                PageSize = currentPageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / currentPageSize),
                Items = orderVms
            };
        }

        public async Task<Order> CreateOrderAsync(int userId, OrderVm orderVm)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var model = orderVm.OrderInfo;
                var order = new Order
                {
                    UserId = userId,
                    TotalPrice = orderVm.TotalAmount,
                    OrderStatus = "Pending",
                    Address = model.Address,
                    Country = model.Country,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Phone = model.Phone,
                    Postcode = model.Postcode,
                    Note = model.Note,
                    PaymentStatus = ""
                };

                await _unitOfWork.OrderRepository.AddAsync(order);
                await _unitOfWork.SaveChangesAsync();

                foreach (var item in orderVm.CartItems)
                {
                    await _orderItemService.AddOrderItemAsync(new OrderItem
                    {
                        OrderId = order.OrderId,
                        Quantity = item.Quantity,
                        ProductId = item.ProductId,
                        Price = item.Price
                    });
                }
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
                return order;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<Order> GetOrderByIdAsync(int orderId)
        {
            var order = await _unitOfWork.OrderRepository.GetByIdAsync(orderId);
            if (order == null) throw new KeyNotFoundException("Order not found");
            return order;
        }

        public async Task<ServiceResult> ConfirmOrderAsync(int orderId)
        {
            var order = await _unitOfWork.OrderRepository.GetByIdAsync(orderId);
            if (order == null)
            {
                return new ServiceResult
                {
                    Success = false,
                    Message = "Không tìm thấy đơn hàng"
                };
            }

            if (order.OrderStatus.ToLower() != "Pending".ToLower())
            {
                return new ServiceResult
                {
                    Success = false,
                    Message = "Đơn hàng phải ở trạng thái chờ"
                };
            }

            order.OrderStatus = "Confirmed";
            await _unitOfWork.SaveChangesAsync();
            return new ServiceResult
            {
                Success = true,
                Message = "Đã xác nhận đơn hàng"
            };
        }

        public async Task<ServiceResult> ConfirmDeliveryAsync(int orderId)
        {
            var order = await _unitOfWork.OrderRepository.GetByIdAsync(orderId);
            if (order == null)
            {
                return new ServiceResult
                {
                    Success = false,
                    Message = "Không tìm thấy đơn hàng"
                };
            }

            if (order.OrderStatus.ToLower() != "Confirmed".ToLower())
            {
                return new ServiceResult
                {
                    Success = false,
                    Message = "Đơn hàng phải ở trạng thái xác nhận"
                };
            }

            order.OrderStatus = "Delivered";
            await _unitOfWork.SaveChangesAsync();
            return new ServiceResult
            {
                Success = true,
                Message = "Đã xác nhận vận chuyển"
            };
        }

        public async Task<ServiceResult> CancelOrderAsync(int orderId)
        {
            var order = await _unitOfWork.OrderRepository.GetByIdAsync(orderId);
            if (order == null)
            {
                return new ServiceResult
                {
                    Success = false,
                    Message = "Không tìm thấy đơn hàng"
                };
            }

            if (order.OrderStatus == "Delivered")
            {
                return new ServiceResult
                {
                    Success = false,
                    Message = "Đơn hàng đang vận chuyển"
                };
            }

            try
            {
                // Lấy danh sách các OrderItem liên quan đến đơn hàng
                var orderItems = await _unitOfWork.OrderItemRepository.GetAllAsync(oi => oi.OrderId == orderId);

                // Xóa tất cả các OrderItem
                foreach (var item in orderItems)
                {
                    _unitOfWork.OrderItemRepository.Delete(item);
                }

                // Xóa chính đơn hàng
                _unitOfWork.OrderRepository.Delete(order);

                // Lưu thay đổi
                _unitOfWork.SaveChanges();

                return new ServiceResult
                {
                    Success = true,
                    Message = "Đã hủy đơn hàng"
                };
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu có
                return new ServiceResult
                {
                    Success = false,
                    Message = $"Đã xảy ra lỗi khi hủy đơn hàng: {ex.Message}"
                };
            }
        }
        public async Task<bool> ProcessPaymentAsync(int orderId, PaymentVm paymentVm)
        {
            // Kiểm tra thông tin thanh toán và xử lý
            var order = await _unitOfWork.OrderRepository.GetByIdAsync(orderId);
            if (order != null)
            {
                // Thực hiện xử lý thanh toán (có thể kết nối với một hệ thống thanh toán bên ngoài)
                if (paymentVm.PaymentMethod == "Credit Card")
                {
                    // Xử lý thanh toán qua thẻ tín dụng
                }
                else if (paymentVm.PaymentMethod == "PayPal")
                {
                    // Xử lý thanh toán qua PayPal
                }

                // Cập nhật trạng thái thanh toán của đơn hàng trong hệ thống
                order.PaymentStatus = "Paid";  // Giả sử có thuộc tính PaymentStatus
                await _unitOfWork.OrderRepository.UpdateAsync(order);
                await _unitOfWork.SaveChangesAsync();

                return true;
            }

            return false;
        }
        public async Task<bool> CompleteOrderAsync(int orderId)
        {
            var order = await _unitOfWork.OrderRepository.GetByIdAsync(orderId);
            if (order != null)
            {
                // Đảm bảo rằng đơn hàng có trạng thái "Đã thanh toán" trước khi hoàn tất
                if (order.PaymentStatus == "Paid")
                {
                    // Thay đổi trạng thái đơn hàng sang "Completed" (hoàn tất)
                    order.OrderStatus = "Completed";  // Giả sử có thuộc tính OrderStatus
                    await _unitOfWork.OrderRepository.UpdateAsync(order);
                    await _unitOfWork.SaveChangesAsync();
                    return true;
                }
            }

            return false;
        }

        public async Task<List<OrderClientVm>> GetAllOrdersByUserIdAsync(int userId)
        {

            // Lọc dữ liệu
            var query = await _unitOfWork.OrderRepository.GetAllAsync(
            filter: order =>
            order.UserId == userId,
                include: x => x.Include(x => x.OrderItems).ThenInclude(oi => oi.Product).ThenInclude(p => p.Image)
            );

            var ordersQuery = query
                .OrderBy(u => u.UserId)
                .ToList();

            var orderVms = ordersQuery.Select(order => new OrderClientVm
            {
                OrderId = order.OrderId,
                CustomerName = order.FirstName + " " + order.LastName,
                OrderDate = order.CreatedAt,
                OrderStatus = order.OrderStatus,
                OrderClientDetailVm = new OrderClientDetailVm
                {
                    OrderId = order.OrderId,
                    CartItems = order.OrderItems.Select(item => new OrderItemVm
                    {
                        OrderId = item.OrderId,
                        Quantity = item.Quantity,
                        Product = new ProductOrderVm
                        {
                            ProductId = item.Product.ProductId,
                            ImageUrl = item.Product.Image != null
                                ? Convert.ToBase64String(item.Product.Image.ImageData)
                                : null,
                            ProductName = item.Product.Name,
                            Price = item.Product.Price,
                            Color = item.Product.Colors
                        }
                    }).ToList(),
                    CreatedDate = order.CreatedAt,
                    OrderInfoVm = new OrderInfoVm
                    {
                        Address = order.Address,
                        Country = order.Country,
                        Email = order.Email,
                        FirstName = order.FirstName,
                        LastName = order.LastName,
                        Note = order.Note,
                        Phone = order.Phone
                    },
                    TotalAmount = order.TotalPrice,
                    OrderStatus = order.OrderStatus
                }
            });

            return orderVms.ToList();
        }
    }
}
