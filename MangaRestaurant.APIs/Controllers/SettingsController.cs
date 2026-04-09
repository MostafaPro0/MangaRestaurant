using AutoMapper;
using MangaRestaurant.APIs.Dtos;
using MangaRestaurant.APIs.Errors;
using MangaRestaurant.Core;
using MangaRestaurant.Core.Entities;
using MangaRestaurant.Core.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using MangaRestaurant.APIs.Resources;

namespace MangaRestaurant.APIs.Controllers
{
    public class SettingsController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public SettingsController(IUnitOfWork unitOfWork, IMapper mapper, INotificationService notificationService, IStringLocalizer<SharedResource> localizer)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _notificationService = notificationService;
            _localizer = localizer;
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

            if (result <= 0) return BadRequest(new ApiResponse(400, _localizer["SETTINGS_UPDATE_FAILED"]));

            // Notify ALL users about the settings update via SignalR
            await _notificationService.SendSettingsUpdatedNotification();

            return Ok(_mapper.Map<SiteSettings, SiteSettingsDto>(existingSettings));
        }
    }
}
