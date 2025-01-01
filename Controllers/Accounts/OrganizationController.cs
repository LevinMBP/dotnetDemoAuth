using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DemoAuth.Data;
using DemoAuth.Dto.Accounts;
using DemoAuth.Helpers;
using DemoAuth.Interface;
using DemoAuth.Mappers;
using DemoAuth.Models.Accounts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto.Modes;

namespace DemoAuth.Controllers.Accounts
{
    [Route("api/admin-accounts")]
    [ApiController]
    public class OrganizationController : ControllerBase
    {
        private readonly IAccountRepository _accountRepo;
        private readonly IOrganizationRepository _organizationRepo;
        public OrganizationController(
            IAccountRepository accountRepo,
            IOrganizationRepository organizationRepo)
        {
            _accountRepo = accountRepo;
            _organizationRepo = organizationRepo;
        }

        // Public route, no authorization needed
        [HttpPost("register-partner")]
        public async Task<IActionResult> RegisterAsPartner([FromBody] RegisterAsPartnerRequestDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Check if company with the same name already exists
            var existingOrg = await _organizationRepo.FindByCompanyNameAsync(request.CompanyName);
            if (existingOrg != null) return BadRequest("Same company name already exists.");

            // Check if email is registered
            var existingEmail = await _accountRepo.FindByEmailAsync(request.Email);
            if (existingEmail != null) return BadRequest("Email is already in used.");



            // Create Organization
            var organization = await _organizationRepo.CreateOrganizationAsync(request);


            // Converts RegisterAsPartnerRequestDto to RegisterUserRequestDto
            var role = "Admin";
            var user = request.ToAppUserFromCreatePartner(organization.Id, role);

            // Create Admin User
            var result = await _accountRepo.CreateAsync(user, request.Password);
            if (!result.Succeeded) return BadRequest(result.Errors);


            // Add user to roles with admin role
            var roleResult = await _accountRepo.AddToRoleAsync(user, role);
            if (!roleResult.Succeeded) return BadRequest($"Failed to assign {role} role to user: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");


            // Call the AccountRepository to send email confirmation
            var (success, message) = await _accountRepo.SendConfirmationEmailAsync(user, Url.Action("ConfirmEmail", "Account", null, protocol: Request.Scheme));


            // Successful creating of Organization, Admin User, User to Role
            return Ok(new { message = "Organization and Admin user successfully registered.\nPlease check your email to confirm your account." });
        }

        [HttpGet("partners")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "SuperUser-001")]
        public async Task<IActionResult> GetAllPartners([FromQuery] OrganizationQueryObject query)
        {
            if (!ModelState.IsValid) // Model validation inherits from ControllerBase
                return BadRequest(ModelState);


            var organizations = await _organizationRepo.GetAllOrganizationAsync(query);
            if (organizations == null) return NotFound();
            var organizationDto = organizations.Select(o => o.ToOrganizationDto());

            return Ok(organizationDto);
        }

        [HttpGet("users")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "SuperUser-001")]
        public async Task<IActionResult> GetAllUsers([FromQuery] UserQueryObject query)
        {
            if (!ModelState.IsValid) // Model validation inherits from ControllerBase
                return BadRequest(ModelState);

            var users = await _accountRepo.GetAllUsersAsync(query);
            var userDto = users.Select(u => u.ToUserDto());

            return Ok(userDto);
        }

        [HttpGet("{id:Guid}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin, SuperUser-001")]
        public async Task<IActionResult> GetOrganizationById([FromRoute] Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var organization = await _organizationRepo.GetOrganizationByIdAsync(id);
            if (organization == null) return NotFound();

            return Ok(organization.ToOrganizationDto());
        }

        [HttpPut]
        [Route("{id:Guid}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin, SuperUser-001")]
        public async Task<IActionResult> UpdateOrganization(
            [FromRoute] Guid id, [FromBody] UpdateOrganizationDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updateMessage = "Update successful.";

            var (organization, emailDidUpdate) = await _organizationRepo.UpdateOrganizationAsync(id, updateDto);
            if (organization == null) return NotFound();

            // If the email was updated, send confirmation email
            if (emailDidUpdate)
            {
                var user = await _accountRepo.FindByEmailAsync(updateDto.Email);
                if (user == null) return NotFound(new { message = "User associated with the email not found." });

                // Call the AccountRepository to send email confirmation
                var (success, message) = await _accountRepo
                    .SendConfirmationEmailAsync(user, Url.Action("ConfirmEmail", "Account", null, protocol: Request.Scheme));

                updateMessage = "Update successful. Please check your email to activate your account";
            }

            return Ok(new
            {
                message = updateMessage,
                data = organization.ToOrganizationDto()
            });
        }

        [HttpDelete]
        [Route("{id:Guid}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "SuperUser-001, Admin")]
        public async Task<IActionResult> DeleteOrganization([FromRoute] Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Gets user from token using nameidentifier
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            // Get user
            var user = await _accountRepo.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var role = user.EmployeeType.ToLower();

            // Check if the user is SuperUser-001
            if (role == "superuser-001")
            {
                // Proceed with delete
                var org = await _organizationRepo.DeleteOrganizationAsync(id);
                if (org == null) return NotFound();
                return NoContent();
            }

            // Check if user is admin
            if (role != "admin") return Forbid();
            // Check if user's organizationId == request id
            if (user.OrganizationId != id) return Forbid();

            var organization = await _organizationRepo.DeleteOrganizationAsync(id);
            if (organization == null) return NotFound();

            return NoContent();
        }

        [HttpGet("test-claims")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin, SuperUser-001")]
        public IActionResult TestClaims()
        {
            var userClaims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
            Console.WriteLine("User Claims: " + string.Join(", ", userClaims.Select(c => $"{c.Type}: {c.Value}")));

            return Ok(userClaims);  // This will return the claims in the response
        }

    }
}