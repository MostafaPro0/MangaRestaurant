using AutoMapper;
using MangaRestaurant.APIs.Dtos;
using MangaRestaurant.APIs.Errors;
using MangaRestaurant.Core;
using MangaRestaurant.Core.Entities.Order;
using MangaRestaurant.Core.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MangaRestaurant.APIs.Controllers
{
    public class OrdersController : BaseApiController
    {
        private readonly IOrderService _orderService;
        private readonly IMapper _mapper;

        public OrdersController(IOrderService orderService, IMapper mapper)
        {
            _orderService = orderService;
            _mapper = mapper;
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
            var order = await _orderService.CreateOrderAsync(buyerEmail, orderDTO.BasketId, orderDTO.DeliveryMethodId, mappedAddress);
            if (order is null) return BadRequest(new ApiResponse(400, "There is a Problem With Your Order"));
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
            if (orders is null) return NotFound(new ApiResponse(404, "There is no Order for this User"));
            var mappedOrders = _mapper.Map<IReadOnlyList<Order>, IReadOnlyList<OrderToReturnDTO>>(orders);
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
            if (orders is null) return NotFound(new ApiResponse(404, $"There is no Order with id={orderId}"));
            var mappedOrder = _mapper.Map<Order, OrderToReturnDTO>(orders);
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
            return Ok(_mapper.Map<Order, OrderToReturnDTO>(order));
        }

        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [HttpPut("Admin/{orderId}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse>> UpdateOrderStatusAdmin(int orderId, [FromBody] OrderStatus status)
        {
            var ok = await _orderService.UpdateOrderStatusAsync(orderId, status);
            if (!ok) return NotFound(new ApiResponse(404, $"Order {orderId} not found or not updated"));
            return Ok(new ApiResponse(200, "Order status updated successfully"));
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
                return BadRequest(new ApiResponse(400, "Status is required"));

            if (!Enum.TryParse<OrderStatus>(request.Status, true, out var status))
                return BadRequest(new ApiResponse(400, "Invalid order status"));

            var existing = await _orderService.GetOrderByIdAsync(orderId);
            if (existing is null) return NotFound(new ApiResponse(404, $"Order {orderId} not found"));

            var buyerEmail = User.FindFirstValue(ClaimTypes.Email);
            if (!User.IsInRole("Admin") && !string.Equals(existing.BuyerEmail, buyerEmail, StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }

            var ok = await _orderService.UpdateOrderStatusAsync(orderId, status);
            if (!ok) return NotFound(new ApiResponse(404, $"Order {orderId} not updated"));
            return Ok(new ApiResponse(200, "Order status updated successfully"));
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
            var topCategories = orders
                .SelectMany(o => o.Items)
                .GroupBy(i => i.ProductItemOrder.ProductName.Split(' ').FirstOrDefault() ?? "Other")
                .Select(g => new TopCategoryDTO
                {
                    Name = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToList();

            // Calculate Delivery Methods
            var topDelivery = orders
                .GroupBy(o => o.DeliveryMethod.ShortName)
                .Select(g => new TopDeliveryDTO
                {
                    Name = g.Key,
                    Count = g.Count()
                })
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
                TopDeliveryMethods = topDelivery,
                PeakHours = peakHours,
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

        [ProducesResponseType(typeof(IReadOnlyList<DeliveryMethod>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [HttpGet("DeliveryMethods")]
        public async Task<ActionResult<IReadOnlyList<DeliveryMethod>>> GetDeliveryMethods()
        {
            var deliveryMethod = await _orderService.GetDeliveryMethodsAsync();
            return Ok(deliveryMethod);
        }
    }
}
