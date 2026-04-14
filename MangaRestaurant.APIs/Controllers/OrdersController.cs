using AutoMapper;
using MangaRestaurant.APIs.Dtos;
using MangaRestaurant.APIs.Errors;
using MangaRestaurant.Core;
using MangaRestaurant.Core.Entities;
using MangaRestaurant.Core.Entities.Order;
using MangaRestaurant.Core.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MangaRestaurant.Core.Entities.Identity;
using System.Security.Claims;
using Microsoft.Extensions.Localization;
using MangaRestaurant.APIs;
using Microsoft.AspNetCore.SignalR;

namespace MangaRestaurant.APIs.Controllers
{
    public class OrdersController : BaseApiController
    {
        private readonly IOrderService _orderService;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly UserManager<AppUser> _userManager;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public OrdersController(IOrderService orderService, IMapper mapper, IUnitOfWork unitOfWork, IConfiguration configuration, UserManager<AppUser> userManager, IStringLocalizer<SharedResource> localizer)
        {
            _orderService = orderService;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _userManager = userManager;
            _localizer = localizer;
        }

        private async Task EnrichOrderImages(OrderToReturnDTO mappedOrder)
        {
            if (mappedOrder == null) return;
            var allProducts = await _unitOfWork.Repository<Product>().GetAllAsync();
            var baseUrl = _configuration["BaseURL"];

            foreach (var item in mappedOrder.Items)
            {
                var product = allProducts.FirstOrDefault(p => p.Id == item.ProductId);
                if (product != null && !string.IsNullOrEmpty(product.PictureUrl))
                {
                    item.CurrentPictureUrl = $"{baseUrl}/{product.PictureUrl}";
                }
            }
        }

        private async Task EnrichOrderImages(IEnumerable<OrderToReturnDTO> mappedOrders)
        {
            if (mappedOrders == null || !mappedOrders.Any()) return;
            var allProducts = await _unitOfWork.Repository<Product>().GetAllAsync();
            var baseUrl = _configuration["BaseURL"];

            foreach (var order in mappedOrders)
            {
                foreach (var item in order.Items)
                {
                    var product = allProducts.FirstOrDefault(p => p.Id == item.ProductId);
                    if (product != null && !string.IsNullOrEmpty(product.PictureUrl))
                    {
                        item.CurrentPictureUrl = $"{baseUrl}/{product.PictureUrl}";
                    }
                }
            }
        }
        //Create Order
        [ProducesResponseType(typeof(Order), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Order>> CreateOrder(OrderDTO orderDTO)
        {
            var buyerEmail = User.FindFirstValue(ClaimTypes.Email);
            var mappedAddress = _mapper.Map<UserAddressDto, OrderAddress>(orderDTO.ShippingAddress);
            var order = await _orderService.CreateOrderAsync(buyerEmail, orderDTO.BasketId, mappedAddress, orderDTO.OrderType);
            if (order is null) return BadRequest(new ApiResponse(400, _localizer["ORDER_ERROR"]));
            return Ok(order);
        }

        [ProducesResponseType(typeof(IReadOnlyList<OrderToReturnDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IReadOnlyList<OrderToReturnDTO>>> GetUserOrders()
        {
            var buyerEmail = User.FindFirstValue(ClaimTypes.Email);
            var orders = await _orderService.GetOrdersForSpecificUserAsync(buyerEmail);
            if (orders is null || !orders.Any()) return NotFound(new ApiResponse(404, _localizer["USER_ORDERS_NOT_FOUND"]));
            var mappedOrders = _mapper.Map<IReadOnlyList<Order>, IReadOnlyList<OrderToReturnDTO>>(orders);
            await EnrichOrderImages(mappedOrders);
            return Ok(mappedOrders);
        }

        [ProducesResponseType(typeof(IReadOnlyList<OrderToReturnDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [HttpGet("{orderId}")]
        [Authorize]
        public async Task<ActionResult<OrderToReturnDTO>> GetUserOrderById(int orderId)
        {
            var buyerEmail = User.FindFirstValue(ClaimTypes.Email);
            var orders = await _orderService.GetOrderByIdForSpecificUserAsync(buyerEmail, orderId);
            if (orders is null) return NotFound(new ApiResponse(404, _localizer["ORDER_NOT_FOUND"]));
            var mappedOrder = _mapper.Map<Order, OrderToReturnDTO>(orders);
            await EnrichOrderImages(mappedOrder);
            return Ok(mappedOrder);
        }

        [ProducesResponseType(typeof(IReadOnlyList<OrderToReturnDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [HttpGet("Admin/All")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IReadOnlyList<OrderToReturnDTO>>> GetAllOrders()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            var mapped = _mapper.Map<IReadOnlyList<Order>, IReadOnlyList<OrderToReturnDTO>>(orders);
            await EnrichOrderImages(mapped);
            return Ok(mapped);
        }

        [ProducesResponseType(typeof(OrderToReturnDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [HttpGet("Admin/{orderId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<OrderToReturnDTO>> GetOrderByIdAdmin(int orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order is null) return NotFound(new ApiResponse(404, $"Order {orderId} not found"));
            var mappedOrder = _mapper.Map<Order, OrderToReturnDTO>(order);
            await EnrichOrderImages(mappedOrder);
            return Ok(mappedOrder);
        }

        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [HttpPut("Admin/{orderId}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse>> UpdateOrderStatusAdmin(int orderId, [FromBody] OrderStatus status)
        {
            var ok = await _orderService.UpdateOrderStatusAsync(orderId, status);
            if (!ok) return NotFound(new ApiResponse(404, _localizer["ORDER_STATUS_ERROR"]));
            return Ok(new ApiResponse(200, _localizer["ORDER_STATUS_SUCCESS"]));
        }

        public class UpdateOrderStatusRequest
        {
            public string Status { get; set; }
        }

        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [HttpPut("{orderId}/status")]
        [Authorize]
        public async Task<ActionResult<ApiResponse>> UpdateOrderStatus(int orderId, [FromBody] UpdateOrderStatusRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Status))
                return BadRequest(new ApiResponse(400, _localizer["REQUIRED_FIELD"]));

            if (!Enum.TryParse<OrderStatus>(request.Status, true, out var status))
                return BadRequest(new ApiResponse(400, _localizer["INVALID_STATUS"]));

            var existing = await _orderService.GetOrderByIdAsync(orderId);
            if (existing is null) return NotFound(new ApiResponse(404, _localizer["ORDER_NOT_FOUND"]));

            var buyerEmail = User.FindFirstValue(ClaimTypes.Email);
            if (!User.IsInRole("Admin") && !string.Equals(existing.BuyerEmail, buyerEmail, StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }

            var ok = await _orderService.UpdateOrderStatusAsync(orderId, status);
            if (!ok) return NotFound(new ApiResponse(404, _localizer["ORDER_STATUS_ERROR"]));
            return Ok(new ApiResponse(200, _localizer["ORDER_STATUS_SUCCESS"]));
        }

        [HttpPut("{orderId}/assign-delivery")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse>> AssignDeliveryPerson(int orderId, [FromBody] AssignDeliveryRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.EmployeeId))
                return BadRequest(new ApiResponse(400, _localizer["EMPLOYEE_ID_REQUIRED"]));

            var user = await _userManager.FindByIdAsync(request.EmployeeId);
            if (user == null) return NotFound(new ApiResponse(404, _localizer["DELIVERY_PERSON_NOT_FOUND"]));

            var ok = await _orderService.AssignDeliveryPersonAsync(orderId, request.EmployeeId, user.DisplayName);
            if (!ok) return NotFound(new ApiResponse(404, _localizer["ORDER_STATUS_ERROR"]));

            return Ok(new ApiResponse(200, _localizer["DELIVERY_ASSIGN_SUCCESS"]));
        }

        [HttpPut("{orderId}/assign-waiter")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse>> AssignWaiter(int orderId, [FromBody] AssignDeliveryRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.EmployeeId))
                return BadRequest(new ApiResponse(400, _localizer["EMPLOYEE_ID_REQUIRED"]));

            var user = await _userManager.FindByIdAsync(request.EmployeeId);
            if (user == null) return NotFound(new ApiResponse(404, _localizer["WAITER_NOT_FOUND"]));

            var ok = await _orderService.AssignWaiterAsync(orderId, request.EmployeeId, user.DisplayName);
            if (!ok) return NotFound(new ApiResponse(404, _localizer["ORDER_STATUS_ERROR"]));

            return Ok(new ApiResponse(200, _localizer["WAITER_ASSIGN_SUCCESS"]));
        }

        [ProducesResponseType(typeof(AdminReportDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [HttpGet("Admin/Report")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<AdminReportDTO>> GetAdminReport()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            var totalOrders = orders.Count;
            
            // Calculate Sales Trend (Last 7 Days)
            var last7Days = Enumerable.Range(0, 7)
                .Select(i => DateTimeOffset.Now.Date.AddDays(-i))
                .Select(date => new DailySalesDTO
                {
                    Date = date.ToString("MMM dd"),
                    Revenue = orders
                        .Where(o => o.OrderDate.Date == date && o.OrderStatus != OrderStatus.PaymentFailed)
                        .Sum(o => o.GetTotal())
                })
                .Reverse()
                .ToList();

            // Calculate Top Products
            var topProducts = orders
                .SelectMany(o => o.Items)
                .GroupBy(i => new { i.ProductItemOrder.ProductName, i.ProductItemOrder.ProductNameAr })
                .Select(g => new TopProductDTO
                {
                    Name = g.Key.ProductName,
                    NameAr = g.Key.ProductNameAr,
                    Quantity = (int)g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.Quantity)
                .Take(5)
                .ToList();

            // Calculate Top Categories (Using Products associated with orders)
            // Note: Since OrderItem doesn't store Category directly in DB, we'd ideally join but 
            // for now we group by common naming or rely on product service. 
            // Better: Group by the current product list to get counts.
            // Correctly Calculate Top Categories by joining with real Categories from DB
            var allProducts = await _unitOfWork.Repository<Product>().GetAllAsync();
            var allCategories = await _unitOfWork.Repository<ProductCategory>().GetAllAsync();

            var topCategories = orders
                .SelectMany(o => o.Items)
                .Select(item => allProducts.FirstOrDefault(p => p.Id == item.ProductItemOrder.ProductId))
                .Where(p => p != null)
                .GroupBy(p => p.CategoryId)
                .Select(g => {
                    var cat = allCategories.FirstOrDefault(c => c.Id == g.Key);
                    return new TopCategoryDTO
                    {
                        Name = cat?.Name ?? "Other",
                        NameAr = cat?.NameAr ?? "أخرى",
                        Count = g.Count()
                    };
                })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToList();

            // Calculate Top Drivers (Most orders delivered)
            var topDrivers = orders
                .Where(o => !string.IsNullOrEmpty(o.DeliveryPersonId))
                .GroupBy(o => o.DeliveryPersonName ?? o.DeliveryPersonId)
                .Select(g => new TopDriverDTO
                {
                    Name = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToList();

            // Calculate Peak Hours (from OrderDate)
            var peakHours = orders
                .GroupBy(o => o.OrderDate.Hour)
                .Select(g => new PeakHourDTO
                {
                    Hour = g.Key,
                    Count = g.Count()
                })
                .OrderBy(x => x.Hour)
                .ToList();

            // Calculate Top Viewed Products
            var topViewedProducts = allProducts
                .OrderByDescending(p => p.Views)
                .Take(5)
                .Select(p => new TopViewedProductDTO
                {
                    Name = p.Name,
                    NameAr = p.NameAr,
                    Views = p.Views,
                    PictureUrl = string.IsNullOrEmpty(p.PictureUrl) ? "" : $"{_configuration["BaseURL"]}/{p.PictureUrl}"
                })
                .ToList();

            // Calculate Top Wishlisted Products
            var allWishlists = await _unitOfWork.Repository<WishlistItem>().GetAllAsync();
            var topWishlistedProducts = allWishlists
                .GroupBy(w => w.ProductId)
                .Select(g => {
                    var product = allProducts.FirstOrDefault(p => p.Id == g.Key);
                    return new TopProductDTO
                    {
                        Name = product?.Name ?? "Unknown",
                        NameAr = product?.NameAr ?? "غير معروف",
                        Quantity = g.Count() // Reusing Quantity field for total wishlist count
                    };
                })
                .OrderByDescending(x => x.Quantity)
                .Take(5)
                .ToList();

            var report = new AdminReportDTO
            {
                TotalOrders = totalOrders,
                PendingOrders = orders.Count(o => o.OrderStatus == OrderStatus.Pending),
                PaymentReceivedOrders = orders.Count(o => o.OrderStatus == OrderStatus.PaymentReceived),
                PaymentFailedOrders = orders.Count(o => o.OrderStatus == OrderStatus.PaymentFailed),
                Revenue = orders.Sum(o => o.GetTotal()),
                AverageOrderValue = totalOrders > 0 ? orders.Average(o => o.GetTotal()) : 0,
                SalesLast7Days = last7Days,
                TopProducts = topProducts,
                TopCategories = topCategories,
                TopDrivers = topDrivers,
                PeakHours = peakHours,
                TopViewedProducts = topViewedProducts,
                TopWishlistedProducts = topWishlistedProducts,
                TopEmployees = orders
                    .GroupBy(o => o.BuyerEmail)
                    .Select(g => new TopEmployeeDTO
                    {
                        Name = g.Key,
                        OrderCount = g.Count(),
                        TotalRevenue = g.Sum(o => o.GetTotal())
                    })
                    .OrderByDescending(x => x.TotalRevenue)
                    .Take(5)
                    .ToList()
            };
            return Ok(report);
        }

        // ── Assign Driver ─────────────────────────────────────────────────────
        /// <summary>
        /// Admin assigns a delivery person to a specific order.
        /// Sends a real-time SignalR notification to the driver + persists it in DB.
        /// </summary>
        [HttpPost("{id}/assign-driver")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> AssignDriver(
            int id,
            [FromBody] AssignDriverDto dto,
            [FromServices] Microsoft.AspNetCore.SignalR.IHubContext<MangaRestaurant.APIs.Hubs.NotificationHub> hubContext)
        {
            var order = await _unitOfWork.Repository<Order>().GetAsync(id);
            if (order == null) return NotFound(new ApiResponse(404));

            // Update the order
            order.DeliveryPersonId   = dto.DeliveryPersonId;
            order.DeliveryPersonName = dto.DeliveryPersonName;
            _unitOfWork.Repository<Order>().Update(order);

            // Find the driver's email to target them in NotificationHub
            var driver = await _userManager.FindByIdAsync(dto.DeliveryPersonId);
            if (driver == null) return BadRequest(new ApiResponse(400, "Driver not found"));

            // Persist notification
            var notification = new MangaRestaurant.Core.Entities.Notification
            {
                Title      = "New Delivery Assignment",
                TitleAr    = "مهمة توصيل جديدة",
                Message    = $"You have been assigned to order #{id}. Please start delivery.",
                MessageAr  = $"تم تعيينك للطلب رقم #{id}. يرجى بدء التوصيل.",
                Type       = MangaRestaurant.Core.Entities.NotificationType.DeliveryAssignment,
                RelatedId  = id.ToString(),
                TargetUser = driver.Email!,
            };
            await _unitOfWork.Repository<MangaRestaurant.Core.Entities.Notification>().AddAsync(notification);
            await _unitOfWork.CompleteAsync();

            // Real-time push to the driver's SignalR group
            await hubContext.Clients
                .Group(driver.Email!)
                .SendAsync("ReceiveNotification", new
                {
                    notification.Title,
                    notification.TitleAr,
                    notification.Message,
                    notification.MessageAr,
                    notification.Type,
                    notification.RelatedId,
                    notification.CreatedAt,
                });

            return Ok(new { message = $"Driver {dto.DeliveryPersonName} assigned to order #{id}" });
        }

        // ── Get Driver's Assigned Orders ───────────────────────────────────────
        [HttpGet("my-deliveries")]
        [Authorize(Roles = "Delivery")]
        public async Task<ActionResult<IReadOnlyList<OrderToReturnDTO>>> GetMyDeliveries()
        {
            var driverId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var allOrders = await _unitOfWork.Repository<Order>().GetAllAsync();
            var myOrders  = allOrders
                .Where(o => o.DeliveryPersonId == driverId
                         && o.OrderStatus != OrderStatus.Delivered
                         && o.OrderStatus != OrderStatus.Cancelled)
                .ToList();

            var result = _mapper.Map<IReadOnlyList<Order>, IReadOnlyList<OrderToReturnDTO>>(myOrders);
            await EnrichOrderImages(result);
            return Ok(result);
        }
    }
}
