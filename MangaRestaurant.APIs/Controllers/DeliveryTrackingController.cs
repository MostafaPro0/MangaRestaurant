using MangaRestaurant.APIs.Dtos;
using MangaRestaurant.APIs.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace MangaRestaurant.APIs.Controllers
{
    /// <summary>
    /// Used by the delivery agent (Admin) to broadcast their GPS location
    /// to all customers watching the given order via SignalR.
    /// </summary>
    [Authorize(Roles = "Delivery,Admin")]
    public class DeliveryTrackingController : BaseApiController
    {
        private readonly IHubContext<DeliveryHub> _hubContext;

        public DeliveryTrackingController(IHubContext<DeliveryHub> hubContext)
        {
            _hubContext = hubContext;
        }

        // POST api/deliverytracking/update
        [HttpPost("update")]
        public async Task<IActionResult> UpdateLocation([FromBody] DeliveryLocationDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.OrderId))
                return BadRequest("OrderId is required.");

            await _hubContext.Clients
                .Group($"order_{dto.OrderId}")
                .SendAsync("ReceiveLocation", new
                {
                    dto.OrderId,
                    dto.Latitude,
                    dto.Longitude,
                    dto.Status,
                    Timestamp = DateTime.UtcNow
                });

            return Ok(new { message = "Location broadcast successfully." });
        }
    }
}
