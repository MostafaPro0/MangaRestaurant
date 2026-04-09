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
using Microsoft.Extensions.Localization;
using MangaRestaurant.APIs.Resources;

namespace MangaRestaurant.APIs.Controllers
{
    public class AccountsController : BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public AccountsController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ITokenService tokenService, IMapper mapper, IConfiguration configuration, IEmailService emailService, IStringLocalizer<SharedResource> localizer)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _mapper = mapper;
            _configuration = configuration;
            _emailService = emailService;
            _localizer = localizer;
        }
        //Register User
        [HttpPost("Register")]
        public async Task<ActionResult<UserDTO>> Register(RegisterDTO registerModel)
        {
            if(CheclEmailExist(registerModel.Email).Result.Value)
            {
                return  BadRequest(new ApiResponse(400,_localizer["DUPLICATE_EMAIL"]));
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

            return Ok(await CreateUserDtoAsync(user));
        }
        //Login User
        [HttpPost("Login")]
        public async Task<ActionResult<UserDTO>> Login(LoginDTO loginModel)
        {
            var user = await _userManager.FindByEmailAsync(loginModel.Email);
            if (user is null) return Unauthorized(new ApiResponse(401, _localizer["INVALID_LOGIN"]));

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginModel.Password, false);

            if (!result.Succeeded)
                return Unauthorized(new ApiResponse(401, _localizer["INVALID_LOGIN"]));

            if (user.IsBanned)
                return Unauthorized(new ApiResponse(401, _localizer["ACCOUNT_BANNED"]));

            return Ok(await CreateUserDtoAsync(user));
        }
        [Authorize]
        [HttpGet("GetCurrentUser")]
        public async Task<ActionResult<UserDTO>> GetCurrentUser()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(userEmail);

            if (user != null && user.IsBanned)
            {
                return Unauthorized(new ApiResponse(401, _localizer["ACCOUNT_BANNED"]));
            }

            return Ok(await CreateUserDtoAsync(user));
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
                return BadRequest(new ApiResponse(400, _localizer["IDENTICAL_PHONE_ERROR"]));
            }

            user.DisplayName = updatedProfile.DisplayName;
            user.PhoneNumber = updatedProfile.PhoneNumber;
            user.PhoneNumber2 = updatedProfile.PhoneNumber2;
            
            if (!string.IsNullOrEmpty(updatedProfile.ProfilePictureUrl))
            {
                user.ProfilePictureUrl = updatedProfile.ProfilePictureUrl;
            }

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded) return BadRequest(new ApiResponse(400, _localizer["PROFILE_UPDATE_FAILED"]));

            return Ok(await CreateUserDtoAsync(user));
        }

        [Authorize]
        [HttpPost("UploadImage")]
        public async Task<ActionResult<string>> UploadProfileImage(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest(_localizer["NO_FILE_UPLOADED"].Value);

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
            if (!result.Succeeded) return BadRequest(new ApiResponse(400, _localizer["ADDRESS_ADD_FAILED"]));

            return Ok(_mapper.Map<UserAddress, UserAddressDto>(address));
        }

        [Authorize]
        [HttpPut("Address")]
        public async Task<ActionResult<UserAddressDto>> UpdateAddress(UserAddressDto addressDto)
        {
            var user = await _userManager.FindUserWithAddressAsync(User);
            if (user == null) return Unauthorized(new ApiResponse(401));

            var address = user.UserAddresses.FirstOrDefault(a => a.Id == addressDto.Id);
            if (address == null) return NotFound(new ApiResponse(404, _localizer["ADDRESS_NOT_FOUND"]));

            _mapper.Map(addressDto, address);

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded) return BadRequest(new ApiResponse(400, _localizer["ADDRESS_UPDATE_FAILED"]));

            return Ok(_mapper.Map<UserAddress, UserAddressDto>(address));
        }

        [Authorize]
        [HttpDelete("Address/{id}")]
        public async Task<ActionResult> DeleteAddress(int id)
        {
            var user = await _userManager.FindUserWithAddressAsync(User);
            if (user == null) return Unauthorized(new ApiResponse(401));

            var address = user.UserAddresses.FirstOrDefault(a => a.Id == id);
            if (address == null) return NotFound(new ApiResponse(404, _localizer["ADDRESS_NOT_FOUND"]));

            user.UserAddresses.Remove(address);

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded) return BadRequest(new ApiResponse(400, _localizer["ADDRESS_DELETE_FAILED"]));

            return Ok();
        }

        [HttpGet("EmailExist")]
        public async Task<ActionResult<bool>> CheclEmailExist(string email)
        {
            return await _userManager.FindByEmailAsync(email) is not null;
        }

        [HttpGet("ListByRole")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<UserDTO>>> ListByRole(string role)
        {
            var users = await _userManager.GetUsersInRoleAsync(role);
            var dtos = new List<UserDTO>();
            foreach (var user in users)
            {
                var dto = await CreateUserDtoAsync(user);
                dto.Role = role;
                dtos.Add(dto);
            }
            return Ok(dtos);
        }

        [HttpGet("All")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetAllUsers()
        {
            var users = _userManager.Users.ToList();
            var dtos = new List<UserDTO>();
            foreach (var user in users)
            {
                var dto = await CreateUserDtoAsync(user);
                var roles = await _userManager.GetRolesAsync(user);
                dto.Role = roles.FirstOrDefault();
                dtos.Add(dto);
            }
            return Ok(dtos);
        }

        [HttpPost("Admin/Create")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserDTO>> CreateUserByAdmin(RegisterDTO model, [FromQuery] string role = "User")
        {
            if (await CheclEmailExist(model.Email).ContinueWith(t => t.Result.Value))
                return BadRequest(new ApiResponse(400, _localizer["DUPLICATE_EMAIL"]));

            var user = new AppUser
            {
                DisplayName = model.DisplayName,
                Email = model.Email,
                UserName = model.Email.Split('@')[0],
                PhoneNumber = model.PhoneNumber
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded) return BadRequest(new ApiResponse(400, _localizer["ACCOUNT_CREATE_FAILED"]));

            await _userManager.AddToRoleAsync(user, role);

            var dto = await CreateUserDtoAsync(user);
            dto.Role = role;
            return Ok(dto);
        }

        [HttpPut("Admin/UpdateRole")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> UpdateUserRole([FromQuery] string userId, [FromQuery] string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound(new ApiResponse(404, _localizer["USER_NOT_FOUND"]));

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, role);

            return Ok(new ApiResponse(200, _localizer["USER_ROLE_UPDATED"]));
        }

        [HttpPut("Admin/ToggleBan/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> ToggleUserBan(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound(new ApiResponse(404, _localizer["USER_NOT_FOUND"]));

            // Don't allow banning self
            var currentUserEmail = User.FindFirstValue(ClaimTypes.Email);
            if (user.Email == currentUserEmail)
            {
                return BadRequest(new ApiResponse(400, "You cannot ban yourself"));
            }

            user.IsBanned = !user.IsBanned;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded) return BadRequest(new ApiResponse(400, "Failed to update user status"));

            return Ok(new { isBanned = user.IsBanned });
        }

        [HttpDelete("Admin/Delete/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound(new ApiResponse(404, _localizer["USER_NOT_FOUND"]));

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded) return BadRequest(new ApiResponse(400, _localizer["PROFILE_UPDATE_FAILED"]));

            return Ok(new ApiResponse(200, _localizer["USER_DELETED_SUCCESS"]));
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
                    if (!result.Succeeded) return BadRequest(new ApiResponse(400, _localizer["ACCOUNT_CREATE_FAILED"]));

                    await _userManager.AddToRoleAsync(user, "User");
                }

                if (user.IsBanned)
                    return Unauthorized(new ApiResponse(401, _localizer["ACCOUNT_BANNED"]));

                return Ok(await CreateUserDtoAsync(user));
            }
            catch (Exception)
            {
                return Unauthorized(new ApiResponse(401, _localizer["INVALID_GOOGLE_TOKEN"]));
            }
        }

        [HttpPost("ForgotPassword")]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordDto forgotPasswordDto)
        {
            var user = await _userManager.FindByEmailAsync(forgotPasswordDto.Email);
            if (user == null)
            {
                // To prevent email enumeration, return Ok even if user doesn't exist
                return Ok(new { message = "If your email exists in our system, you will receive a reset link." });
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            
            // Encode token for URL
            var encodedToken = System.Web.HttpUtility.UrlEncode(token);
            
            // Link to Frontend Reset Password Page
            var resetLink = $"{_configuration["FrontBaseURL"]}/account/reset-password?token={encodedToken}&email={user.Email}";

            var emailBody = $@"
                <div style='font-family: Arial, sans-serif; padding: 20px; color: #333;'>
                    <h2 style='color: #ff3e3e;'>Manga Restaurant - Password Reset</h2>
                    <p>Hello {user.DisplayName},</p>
                    <p>You requested a password reset. Click the button below to set a new password:</p>
                    <div style='margin: 30px 0;'>
                        <a href='{resetLink}' style='background: #ff3e3e; color: white; padding: 12px 25px; text-decoration: none; border-radius: 5px; font-weight: bold;'>Reset Password</a>
                    </div>
                    <p>If the button doesn't work, copy and paste this link into your browser:</p>
                    <p style='color: #666;'>{resetLink}</p>
                    <p>This link will expire soon. If you didn't request this, please ignore this email.</p>
                    <hr style='border: 0; border-top: 1px solid #eee; margin-top: 40px;'>
                    <p style='font-size: 0.8em; color: #999;'>&copy; {DateTime.Now.Year} Manga Restaurant. All rights reserved.</p>
                </div>";

            await _emailService.SendEmailAsync(user.Email, "Reset Your Password - Manga Restaurant", emailBody);

            return Ok(new { message = "If your email exists in our system, you will receive a reset link." });
        }

        [HttpPost("ResetPassword")]
        public async Task<ActionResult> ResetPassword(ResetPasswordDto resetPasswordDto)
        {
            var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);
            if (user == null) return BadRequest(new ApiResponse(400, "Invalid request"));

            var result = await _userManager.ResetPasswordAsync(user, resetPasswordDto.Token, resetPasswordDto.NewPassword);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            return Ok(new { message = "Password has been reset successfully." });
        }

        [Authorize]
        [HttpPost("ChangePassword")]
        public async Task<ActionResult> ChangePassword(ChangePasswordDto changePasswordDto)
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null) return Unauthorized(new ApiResponse(401));

            var result = await _userManager.ChangePasswordAsync(user, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            return Ok(new { message = _localizer["PASSWORD_CHANGED"].Value });
        }

        [Authorize]
        [HttpPost("AddPassword")]
        public async Task<ActionResult<UserDTO>> AddPassword(AddPasswordDto addPasswordDto)
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null) return Unauthorized(new ApiResponse(401));

            if (await _userManager.HasPasswordAsync(user))
            {
                return BadRequest(new ApiResponse(400, _localizer["HAS_PASSWORD_ERROR"]));
            }

            var result = await _userManager.AddPasswordAsync(user, addPasswordDto.NewPassword);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            return Ok(await CreateUserDtoAsync(user));
        }

        private async Task<UserDTO> CreateUserDtoAsync(AppUser user)
        {
            var userDto = _mapper.Map<AppUser, UserDTO>(user);
            userDto.Token = await _tokenService.CreateTokenAsync(user, _userManager);
            userDto.HasPassword = await _userManager.HasPasswordAsync(user);
            return userDto;
        }
    }
}
