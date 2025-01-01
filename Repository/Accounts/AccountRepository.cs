using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DemoAuth.Dto.Accounts;
using DemoAuth.Helpers;
using DemoAuth.Interface;
using DemoAuth.Models.Accounts;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

namespace DemoAuth.Repository.Accounts
{
    public class AccountRepository : IAccountRepository
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailSender _emailSender;
        public AccountRepository(UserManager<AppUser> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }


        public async Task<IdentityResult> AddToRoleAsync(AppUser user, string role)
        {
            return await _userManager.AddToRoleAsync(user, role);
        }

        public async Task<IdentityResult> CreateAsync(AppUser appUser, string password)
        {
            return await _userManager.CreateAsync(appUser, password);
        }

        public async Task<IdentityResult> CreateUserAsync(RegisterUserRequestDto appUser, string password)
        {
            var user = new AppUser
            {
                UserName = appUser.Email, // Identity requires UserName, use Email
                FirstName = appUser.FirstName,
                LastName = appUser.LastName,
                Email = appUser.Email,
                PhoneNumber = appUser.PhoneNumber,
                EmployeeType = appUser.EmployeeType,
                CreatedAt = DateTime.UtcNow,
                // OrganizationId = appUser.OrganizationId
            };

            return await _userManager.CreateAsync(user, password);
        }

        public async Task<AppUser> FindByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<AppUser> FindByIdAsync(string Id)
        {
            return await _userManager.FindByIdAsync(Id);
        }

        public async Task<(bool, string)> SendConfirmationEmailAsync(AppUser user, string returnUrl)
        {
            // Generate Email Confirmation Token
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);


            // Encoded token
            // var encodedToken = WebUtility.UrlEncode(token);
            // var encodedToken = HttpUtility.UrlEncode(token);
            var encodedToken = Uri.EscapeDataString(token);


            // Create the email confirmation link
            var confirmationLink = $"{returnUrl}?userId={user.Id}&token={encodedToken}";

            // Send the email with the confirmation link
            await _emailSender.SendEmailAsync(user.Email,
                "Confirm your email",
                $"Please confirm your email by clicking <a href='{confirmationLink}'>here</a>.");

            return (true, "Registration successful. Please check your email to confirm your account.");
        }

        public async Task<IdentityResult> ConfirmEmailAsync(AppUser user, string token)
        {
            return await _userManager.ConfirmEmailAsync(user, token);
        }

        public async Task<bool> IsEmailConfirmedAsync(AppUser user)
        {
            return await _userManager.IsEmailConfirmedAsync(user);
        }

        public async Task<IdentityResult> AddToClaimsAsync(AppUser user, Guid oId, string role)
        {
            var claims = new List<Claim>
            {
                new Claim("OrganizationId", oId.ToString()),
                new Claim("UserRole", role)
            };

            return await _userManager.AddClaimsAsync(user, claims);
        }

        public async Task<List<AppUser>> GetAllUsersAsync(UserQueryObject query, Guid? organizationId = null)
        {
            // Base query for users
            var users = _userManager.Users.AsQueryable();

            // Filter by OrganizationId if provided
            if (organizationId.HasValue)
            {
                users = users.Where(u => u.OrganizationId == organizationId.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.Email))
            {
                users = users.Where(u => u.Email.Contains(query.Email));
            }

            if (!string.IsNullOrWhiteSpace(query.FirstName))
            {
                users = users.Where(u => u.FirstName.Contains(query.FirstName));
            }

            if (!string.IsNullOrWhiteSpace(query.LastName))
            {
                users = users.Where(u => u.LastName.Contains(query.LastName));
            }

            if (!string.IsNullOrWhiteSpace(query.SortBy))
            {
                users = query.SortBy.ToLower() switch
                {
                    "email" => query.IsDescending ? users.OrderByDescending(u => u.Email) : users.OrderBy(u => u.Email),
                    // You can add more sorting options here
                    _ => users
                };
            }

            var skipNumber = (query.PageNumber - 1) * query.PageSize;

            return await users.Skip(skipNumber).Take(query.PageSize).ToListAsync();
        }
    }
}