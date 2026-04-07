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
        private readonly IEmailService _emailService;

        public AccountsController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ITokenService tokenService, IMapper mapper, IConfiguration configuration, IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _mapper = mapper;
            _configuration = configuration;
            _emailService = emailService;
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

            return Ok(await CreateUserDtoAsync(user));
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

            return Ok(await CreateUserDtoAsync(user));
        }
        [Authorize]
        [HttpGet("GetCurrentUser")]
        public async Task<ActionResult<UserDTO>> GetCurrentUser()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(userEmail);
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

            return Ok(await CreateUserDtoAsync(user));
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

                return Ok(await CreateUserDtoAsync(user));
            }
            catch (Exception)
            {
                return Unauthorized(new ApiResponse(401, "Invalid Google Token"));
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

            return Ok(new { message = "Password changed successfully" });
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
                return BadRequest(new ApiResponse(400, "User already has a password. Use ChangePassword instead."));
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
