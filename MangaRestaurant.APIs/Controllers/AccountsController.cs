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
        private readonly IConfiguration _configuration;

        public AccountsController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ITokenService tokenService, IMapper mapper, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _mapper = mapper;
            _configuration = configuration;
        }
        //Register User
        [HttpPost("Register")]
        public async Task<ActionResult<UserDTO>> Register(RegisterDTO registerModel)
        {
            if(CheclEmailExist(registerModel.Email).Result.Value)
            {
                return  BadRequest(new ApiResponse(400,"this Email Exist"));
            }
            var user = new AppUser()
            {
                DisplayName = registerModel.DisplayName,
                Email = registerModel.Email,
                UserName = registerModel.Email.Split('@')[0],
                PhoneNumber = registerModel.PhoneNumber,
            };
            var regRequestResult = await _userManager.CreateAsync(user, registerModel.Password);
            
            if (!regRequestResult.Succeeded) return BadRequest(new ApiResponse(400));

            // Assign Default Role to User
            await _userManager.AddToRoleAsync(user, "User");

            var mappedUser = _mapper.Map<AppUser, UserDTO>(user);
            mappedUser.Token = await _tokenService.CreateTokenAsync(user, _userManager);
            return Ok(mappedUser);
        }
        //Login User
        [HttpPost("Login")]
        public async Task<ActionResult<UserDTO>> Login(LoginDTO loginModel)
        {
            var user = await _userManager.FindByEmailAsync(loginModel.Email);
            if (user is null) return Unauthorized(new ApiResponse(401, "Invalid email or password"));

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginModel.Password, false);

            if (!result.Succeeded)
                return Unauthorized(new ApiResponse(401, "Invalid email or password"));

            var mappedUser = _mapper.Map<AppUser, UserDTO>(user);
            mappedUser.Token = await _tokenService.CreateTokenAsync(user, _userManager);
            return Ok(mappedUser);
        }
        [Authorize]
        [HttpGet("GetCurrentUser")]
        public async Task<ActionResult<UserDTO>> GetCurrentUser()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(userEmail);
            var returnedUser = _mapper.Map<AppUser, UserDTO>(user);
            returnedUser.Token = await _tokenService.CreateTokenAsync(user, _userManager);
            return Ok(returnedUser);
        }
        
        [Authorize]
        [HttpPut("UpdateProfile")]
        public async Task<ActionResult<UserDTO>> UpdateProfile(UpdateProfileDto updatedProfile)
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(userEmail);
            
            if (user is null) return Unauthorized(new ApiResponse(401));

            if (!string.IsNullOrEmpty(updatedProfile.PhoneNumber) && 
                !string.IsNullOrEmpty(updatedProfile.PhoneNumber2) && 
                updatedProfile.PhoneNumber == updatedProfile.PhoneNumber2)
            {
                return BadRequest(new ApiResponse(400, "Phone number and secondary phone number cannot be identical"));
            }

            user.DisplayName = updatedProfile.DisplayName;
            user.PhoneNumber = updatedProfile.PhoneNumber;
            user.PhoneNumber2 = updatedProfile.PhoneNumber2;
            
            if (!string.IsNullOrEmpty(updatedProfile.ProfilePictureUrl))
            {
                user.ProfilePictureUrl = updatedProfile.ProfilePictureUrl;
            }

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded) return BadRequest(new ApiResponse(400, "Failed to update profile"));

            var returnedUser = _mapper.Map<AppUser, UserDTO>(user);
            returnedUser.Token = await _tokenService.CreateTokenAsync(user, _userManager);
            return Ok(returnedUser);
        }

        [Authorize]
        [HttpPost("UploadImage")]
        public async Task<ActionResult<string>> UploadProfileImage(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("No file uploaded");

            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "profiles");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return the full URL using AppSettings BaseURL
            var baseUrl = _configuration["BaseURL"];
            var fileUrl = baseUrl + $"/images/profiles/{fileName}";
            return Ok(new { url = fileUrl });
        }

        [Authorize]
        [HttpGet("UserAddresses")]
        public async Task<ActionResult<IEnumerable<UserAddressDto>>> GetUserAddresses()
        {
            var user = await _userManager.FindUserWithAddressAsync(User);
            if (user == null) return Unauthorized(new ApiResponse(401));
            
            var mappedAddresses = _mapper.Map<ICollection<UserAddress>, IEnumerable<UserAddressDto>>(user.UserAddresses);
            return Ok(mappedAddresses);
        }

        [Authorize]
        [HttpPost("Address")]
        public async Task<ActionResult<UserAddressDto>> AddAddress(UserAddressDto addressDto)
        {
            var user = await _userManager.FindUserWithAddressAsync(User);
            if (user == null) return Unauthorized(new ApiResponse(401));

            var address = _mapper.Map<UserAddressDto, UserAddress>(addressDto);
            user.UserAddresses.Add(address);

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded) return BadRequest(new ApiResponse(400, "Could not add address"));

            return Ok(_mapper.Map<UserAddress, UserAddressDto>(address));
        }

        [Authorize]
        [HttpPut("Address")]
        public async Task<ActionResult<UserAddressDto>> UpdateAddress(UserAddressDto addressDto)
        {
            var user = await _userManager.FindUserWithAddressAsync(User);
            if (user == null) return Unauthorized(new ApiResponse(401));

            var address = user.UserAddresses.FirstOrDefault(a => a.Id == addressDto.Id);
            if (address == null) return NotFound(new ApiResponse(404, "Address not found"));

            _mapper.Map(addressDto, address);

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded) return BadRequest(new ApiResponse(400, "Could not update address"));

            return Ok(_mapper.Map<UserAddress, UserAddressDto>(address));
        }

        [Authorize]
        [HttpDelete("Address/{id}")]
        public async Task<ActionResult> DeleteAddress(int id)
        {
            var user = await _userManager.FindUserWithAddressAsync(User);
            if (user == null) return Unauthorized(new ApiResponse(401));

            var address = user.UserAddresses.FirstOrDefault(a => a.Id == id);
            if (address == null) return NotFound(new ApiResponse(404, "Address not found"));

            user.UserAddresses.Remove(address);

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded) return BadRequest(new ApiResponse(400, "Could not delete address"));

            return Ok();
        }

        [HttpGet("EmailExist")]
        public async Task<ActionResult<bool>> CheclEmailExist(string email)
        {
            return await _userManager.FindByEmailAsync(email) is not null;
        }

        [HttpPost("GoogleLogin")]
        public async Task<ActionResult<UserDTO>> GoogleLogin(GoogleLoginDTO googleAuth)
        {
            try
            {
                var settings = new Google.Apis.Auth.GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = new string[] { "331442573652-kqe2go0r9fvcsqgkikii5caukc5792u8.apps.googleusercontent.com" }
                };
                var payload = await Google.Apis.Auth.GoogleJsonWebSignature.ValidateAsync(googleAuth.IdToken, settings);
                
                var user = await _userManager.FindByEmailAsync(payload.Email);
                if (user == null)
                {
                    user = new AppUser
                    {
                        DisplayName = payload.Name ?? payload.GivenName ?? "Google User",
                        Email = payload.Email,
                        UserName = payload.Email.Split('@')[0] + "_" + Guid.NewGuid().ToString("N").Substring(0, 4)
                    };

                    var result = await _userManager.CreateAsync(user);
                    if (!result.Succeeded) return BadRequest(new ApiResponse(400, "Failed to create user account"));

                    await _userManager.AddToRoleAsync(user, "User");
                }

                var mappedUser = _mapper.Map<AppUser, UserDTO>(user);
                mappedUser.Token = await _tokenService.CreateTokenAsync(user, _userManager);
                return Ok(mappedUser);
            }
            catch (Exception)
            {
                return Unauthorized(new ApiResponse(401, "Invalid Google Token"));
            }
        }
    }
}
