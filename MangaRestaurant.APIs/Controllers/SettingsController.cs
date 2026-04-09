using AutoMapper;
using MangaRestaurant.APIs.Dtos;
using MangaRestaurant.APIs.Errors;
using MangaRestaurant.Core;
using MangaRestaurant.Core.Entities;
using MangaRestaurant.Core.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace MangaRestaurant.APIs.Controllers
{
    public class SettingsController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;

        public SettingsController(IUnitOfWork unitOfWork, IMapper mapper, INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<ActionResult<SiteSettingsDto>> GetSettings()
        {
            var settings = await _unitOfWork.Repository<SiteSettings>().GetAllAsync();
            var latestSettings = settings.OrderByDescending(s => s.Id).FirstOrDefault();
            
            if (latestSettings == null) return NotFound(new ApiResponse(404));

            return Ok(_mapper.Map<SiteSettings, SiteSettingsDto>(latestSettings));
        }

        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<SiteSettingsDto>> UpdateSettings(SiteSettingsDto settingsDto)
        {
            var settings = await _unitOfWork.Repository<SiteSettings>().GetAllAsync();
            var existingSettings = settings.OrderByDescending(s => s.Id).FirstOrDefault();

            if (existingSettings == null)
            {
                existingSettings = _mapper.Map<SiteSettingsDto, SiteSettings>(settingsDto);
                await _unitOfWork.Repository<SiteSettings>().AddAsync(existingSettings);
            }
            else
            {
                _mapper.Map(settingsDto, existingSettings);
                _unitOfWork.Repository<SiteSettings>().Update(existingSettings);
            }

            var result = await _unitOfWork.CompleteAsync();

            if (result <= 0) return BadRequest(new ApiResponse(400, "Problem updating settings"));

            // Notify ALL users about the settings update via SignalR
            await _notificationService.SendSettingsUpdatedNotification();

            return Ok(_mapper.Map<SiteSettings, SiteSettingsDto>(existingSettings));
        }
    }
}
