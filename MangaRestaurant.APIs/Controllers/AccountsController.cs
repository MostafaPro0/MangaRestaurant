﻿using MangaRestaurant.APIs.Dtos;
using MangaRestaurant.APIs.Errors;
using MangaRestaurant.Core.Entities.Identity;
using MangaRestaurant.Core.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MangaRestaurant.APIs.Controllers
{
    public class AccountsController : BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ITokenService _tokenService;

        public AccountsController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ITokenService tokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
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
                Token = await _tokenService.CreateTokenAsync(user)
            };
            return !regRequestResult.Succeeded ? BadRequest(new ApiResponse(400)) : Ok(mappedUser);
        }  
        //Login User
        [HttpPost("Login")]
        public async Task<ActionResult<UserDTO>> Login(LoginDTO loginModel)
        {
            var user = await _userManager.FindByEmailAsync(loginModel.Email);
            if (user is null) return Unauthorized(new ApiResponse(401));

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginModel.Password,false);

            return !result.Succeeded ? Unauthorized(new ApiResponse(401)) : Ok(new UserDTO { DisplayName = user.DisplayName, Email = user.Email, Token = await _tokenService.CreateTokenAsync(user) });
        }
    }
}