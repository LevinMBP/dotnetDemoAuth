using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DemoAuth.Dto.Accounts;
using DemoAuth.Helpers;
using DemoAuth.Interface;
using DemoAuth.Mappers;
using DemoAuth.Models.Accounts;
using DemoAuth.Repository.Accounts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;

namespace DemoAuth.Controllers.Accounts
{
    [Route("api/accounts")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountRepository _accountRepo;
        private readonly IOrganizationRepository _organizationRepo;
        public AccountController(
            IAccountRepository accountRepo,
            IOrganizationRepository organizationRepo)
        {
            _accountRepo = accountRepo;
            _organizationRepo = organizationRepo;
        }

        [HttpPost("register")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Register(RegisterUserRequestDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Gets user from token using nameidentifier
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();


            var getUser = await _accountRepo.FindByIdAsync(userId);
            if (getUser == null) return Unauthorized();


            // Gets User ORganization id
            var userOrganizationId = getUser.OrganizationId;


            // Checks if Organization is valid
            var orgExists = await _organizationRepo.GetOrganizationByIdAsync(userOrganizationId);
            if (orgExists == null) return BadRequest("Invalid user registration");

            // Check if email is registered
            var existingEmail = await _accountRepo.FindByEmailAsync(request.Email);
            if (existingEmail != null) return BadRequest("Email is already in used.");

            // Converts RegisterAsPartnerRequestDto to RegisterUserRequestDto
            var role = request.EmployeeType;
            var user = request.ToAppUserFromCreateUserRequest(userOrganizationId);

            // Create User
            var result = await _accountRepo.CreateAsync(user, request.Password);
            if (!result.Succeeded) return BadRequest(result.Errors);


            // Add user to roles with admin role
            var roleResult = await _accountRepo.AddToRoleAsync(user, role);
            if (!roleResult.Succeeded) return BadRequest($"Failed to assign {role} role to user: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");


            // Call the AccountRepository to send email confirmation
            var (success, message) = await _accountRepo.SendConfirmationEmailAsync(user, Url.Action("ConfirmEmail", "Account", null, protocol: Request.Scheme));


            return Ok(new { Message = message });

        }

        [HttpGet]
        [Route("{id:Guid}/users")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetAllUsersByOrganization([FromRoute] Guid id, [FromQuery] UserQueryObject query)
        {
            if (!ModelState.IsValid) // Model validation inherits from ControllerBase
                return BadRequest(ModelState);

            var users = await _accountRepo.GetAllUsersAsync(query, organizationId: id);
            var userDto = users.Select(u => u.ToUserDto());

            return Ok(userDto);
        }


        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
        {
            if (userId == null || token == null)
            {
                return BadRequest("Invalid request.");
            }

            var user = await _accountRepo.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Decode the token in case it was URL encoded
            // token = HttpUtility.UrlDecode(token);
            token = Uri.UnescapeDataString(token);

            // Log the userId and token to verify
            // Console.WriteLine($"Received userId: {userId}");
            // Console.WriteLine($"Received token: {token}");

            var result = await _accountRepo.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                return Ok(new { Message = "Email confirmed successfully." });
            }

            // Log or check result errors for more details
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return BadRequest($"Invalid token or email confirmation failed: {errors}");
        }

        [HttpPost("resend-confirmation-email")]
        public async Task<IActionResult> ResendConfirmationEmail([FromBody] ResendConfirmationDto request)
        {
            if (string.IsNullOrEmpty(request.Email))
                return BadRequest("Email is required.");

            var user = await _accountRepo.FindByEmailAsync(request.Email);
            if (user == null)
                return NotFound("User not found.");

            // Check if the user's email is already confirmed
            if (await _accountRepo.IsEmailConfirmedAsync(user))
                return Ok(new { Message = "Email is already confirmed." });

            // Call the AccountRepository to send email confirmation
            var (success, message) = await _accountRepo.SendConfirmationEmailAsync(user, Url.Action("ConfirmEmail", "Account", null, protocol: Request.Scheme));

            return Ok(new { Message = "A new confirmation email has been sent. Please check your inbox." });
        }

    }
}