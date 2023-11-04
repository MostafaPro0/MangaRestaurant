using AutoMapper;
using MangaRestaurant.APIs.Dtos;
using MangaRestaurant.APIs.Errors;
using MangaRestaurant.APIs.Extensions;
using MangaRestaurant.Core.Entities.Identity;
using MangaRestaurant.Core.Service;
using MangaRestaurant.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MangaRestaurant.APIs.Controllers
{
    public class AccountsController : BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;

        public AccountsController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ITokenService tokenService, IMapper mapper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _mapper = mapper;
        }
        //Register User
        [HttpPost("Register")]
        public async Task<ActionResult<UserDTO>> Register(RegisterDTO registerModel)
        {
            var user = new AppUser()
            {
                DisplayName = registerModel.DisplayName,
                Email = registerModel.Email,
                UserName = registerModel.Email.Split('@')[0],
                PhoneNumber = registerModel.PhoneNumber,
            };
            var regRequestResult = await _userManager.CreateAsync(user, registerModel.Password);
            var mappedUser = new UserDTO()
            {
                DisplayName = user.DisplayName,
                Email = user.Email,
                Token = await _tokenService.CreateTokenAsync(user, _userManager)
            };
            return !regRequestResult.Succeeded ? BadRequest(new ApiResponse(400)) : Ok(mappedUser);
        }
        //Login User
        [HttpPost("Login")]
        public async Task<ActionResult<UserDTO>> Login(LoginDTO loginModel)
        {
            var user = await _userManager.FindByEmailAsync(loginModel.Email);
            if (user is null) return Unauthorized(new ApiResponse(401));

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginModel.Password, false);

            return !result.Succeeded ? Unauthorized(new ApiResponse(401)) : Ok(new UserDTO { DisplayName = user.DisplayName, Email = user.Email, Token = await _tokenService.CreateTokenAsync(user, _userManager) });
        }
        [Authorize]
        [HttpGet("GetCurrentUser")]
        public async Task<ActionResult<UserDTO>> GetCurrentUser()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(userEmail);
            var returnedUser = new UserDTO()
            {
                DisplayName = user.DisplayName,
                Email = user.Email,
                Token = await _tokenService.CreateTokenAsync(user, _userManager)
            };
            return Ok(returnedUser);
        }
        [Authorize]
        [HttpGet("UserAddress")]//Get User Address
        public async Task<ActionResult<UserAddressDto>> GetCurrentUserAddress()
        {
            var user = await _userManager.FindUserWithAddressAsync(User);
            var mappedAddress = _mapper.Map<UserAddress, UserAddressDto>(user.UserAddress);
            return Ok(mappedAddress);
        }
        //Update User Address
        [Authorize]
        [HttpPut("UserAddress")]
        public async Task<ActionResult<UserAddressDto>> UpdateUserAddress(UserAddressDto updatedAddress)
        {
            var user = await _userManager.FindUserWithAddressAsync(User);
            if (user is null) return Unauthorized(new ApiResponse(401));
            var address = _mapper.Map<UserAddressDto, UserAddress>(updatedAddress);
            user.UserAddress = address;
            var updateAddres = await _userManager.UpdateAsync(user);
            return updateAddres.Succeeded ? Ok(updateAddres) : BadRequest(new ApiResponse(400));
        }

        [HttpGet("EmailExist")]
        public async Task<ActionResult<bool>> CheclEmailExist(string email)
        {
            return await _userManager.FindByEmailAsync(email) is not null;
        }
    }
}
