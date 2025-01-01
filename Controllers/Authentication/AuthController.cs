using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DemoAuth.Dto.Auth;
using DemoAuth.Interface;
using DemoAuth.Models.Accounts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DemoAuth.Controllers.Authentication
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ITokenService _tokenService;
        public AuthController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            ITokenService tokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {

            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(loginRequest.Email);
            if (user == null) return Unauthorized("Invalid credentials. 1");


            // check if email is verified
            if (!await _userManager.IsEmailConfirmedAsync(user))
                return Unauthorized("Please confirm your email before logging in.");


            // Check for Organization????

            // Generate Access Token
            var accessToken = _tokenService.GenerateAccessToken(user);
            // Generate Refresh Token
            var refreshToken = await _tokenService.GenerateAndStoreRefreshTokenAsync(user);

            Response.Cookies.Append("RefreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,  // Makes the cookie inaccessible to JavaScript
                Secure = true,    // Ensure it's only sent over HTTPS
                SameSite = SameSiteMode.None,  // Required for cross-origin requests
                Expires = DateTime.UtcNow.AddDays(7)  // Set cookie expiration
            });

            return Ok(new
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            });
        }

        [HttpPost("logout")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Logout()
        {
            // Remove the refresh token from the database (if applicable)
            var refreshToken = Request.Cookies["RefreshToken"];  // If stored in a cookie, for example
            Console.WriteLine($"Request RefreshToken: {refreshToken}");


            if (refreshToken != null)
            {
                await _tokenService.RevokeRefreshTokenAsync(refreshToken);

                // Change token expiration
                Response.Cookies.Append("RefreshToken", "", new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddDays(-1), // Expire the cookie immediately
                    HttpOnly = true,  // Make it inaccessible to JavaScript
                    Secure = true,    // Ensure it's only sent over HTTPS
                    SameSite = SameSiteMode.Strict
                });
            }

            // Logout from Identity
            await _signInManager.SignOutAsync();

            return Ok(new { message = "Logged out successfully" });
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Verify incoming refresh token
            var isValid = await _tokenService.VerifyRefreshTokenAsync(request.RefreshToken);
            if (!isValid) return Unauthorized("Invalid refresh token");


            // retrieve user using incoming refresh token
            var refreshTokenEntity = await _tokenService.GetRefreshTokenEntityAsync(request.RefreshToken);
            if (refreshTokenEntity == null) return Unauthorized("Refresh token is expired or revoked");


            var user = await _userManager.FindByIdAsync(refreshTokenEntity.UserId.ToString());
            if (user == null) return Unauthorized("Invalid user");


            // Rotate the refresh token (invalidate the old one and issue a new one)
            var newRefreshToken = await _tokenService.RotateRefreshTokenAsync(request.RefreshToken);
            if (newRefreshToken == null) return Unauthorized("Unable to rotate refresh token");

            // Generate a new access token using the same user
            var accessToken = _tokenService.GenerateAccessToken(user);

            Response.Cookies.Append("RefreshToken", newRefreshToken, new CookieOptions
            {
                HttpOnly = true,  // Makes the cookie inaccessible to JavaScript
                Secure = true,    // Ensure it's only sent over HTTPS
                SameSite = SameSiteMode.None,  // Required for cross-origin requests
                Expires = DateTime.UtcNow.AddDays(7)  // Set cookie expiration
            });

            return Ok(new
            {
                access = accessToken,
                refresh = newRefreshToken
            });
        }
    }
}