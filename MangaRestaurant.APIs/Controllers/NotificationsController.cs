using AutoMapper;
using MangaRestaurant.APIs.Dtos;
using MangaRestaurant.APIs.Errors;
using MangaRestaurant.Core;
using MangaRestaurant.Core.Entities;
using MangaRestaurant.Core.RepositoriesContract;
using MangaRestaurant.Core.Specifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MangaRestaurant.APIs.Controllers
{
    [Authorize]
    public class NotificationsController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public NotificationsController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<NotificationToReturnDto>>> GetNotifications()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var isAdmin = User.IsInRole("Admin");

            var spec = new NotificationsForUserSpecification(email, isAdmin);
            var notifications = await _unitOfWork.Repository<Notification>().GetAllAsyncWithSpecAsync(spec);

            return Ok(_mapper.Map<IReadOnlyList<Notification>, IReadOnlyList<NotificationToReturnDto>>(notifications));
        }

        [HttpPost("{id}/read")]
        public async Task<ActionResult> MarkAsRead(int id)
        {
            var notification = await _unitOfWork.Repository<Notification>().GetAsync(id);

            if (notification == null) return NotFound(new ApiResponse(404));

            var email = User.FindFirstValue(ClaimTypes.Email);
            var isAdmin = User.IsInRole("Admin");

            if (notification.TargetUser != "All" && notification.TargetUser != "Admins" && notification.TargetUser != email)
            {
                return Unauthorized(new ApiResponse(401));
            }

            if (notification.TargetUser == "Admins" && !isAdmin)
            {
                return Unauthorized(new ApiResponse(401));
            }

            notification.IsRead = true;
            _unitOfWork.Repository<Notification>().Update(notification);
            
            await _unitOfWork.CompleteAsync();

            return Ok();
        }

        [HttpPost("read-all")]
        public async Task<ActionResult> MarkAllAsRead()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var isAdmin = User.IsInRole("Admin");

            var spec = new NotificationsForUserSpecification(email, isAdmin);
            var notifications = await _unitOfWork.Repository<Notification>().GetAllAsyncWithSpecAsync(spec);

            foreach (var notif in notifications)
            {
                if (!notif.IsRead)
                {
                    notif.IsRead = true;
                    _unitOfWork.Repository<Notification>().Update(notif);
                }
            }

            await _unitOfWork.CompleteAsync();
            return Ok();
        }
    }
}
