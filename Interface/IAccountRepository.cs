using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DemoAuth.Dto.Accounts;
using DemoAuth.Helpers;
using DemoAuth.Models.Accounts;
using Microsoft.AspNetCore.Identity;

namespace DemoAuth.Interface
{
    public interface IAccountRepository
    {
        Task<IdentityResult> CreateUserAsync(RegisterUserRequestDto appUser, string password);
        Task<IdentityResult> CreateAsync(AppUser appUser, string password);
        Task<AppUser> FindByEmailAsync(string email);
        Task<AppUser> FindByIdAsync(string Id);
        Task<IdentityResult> AddToRoleAsync(AppUser user, string role);
        Task<IdentityResult> AddToClaimsAsync(AppUser user, Guid oId, string role);
        Task<(bool, string)> SendConfirmationEmailAsync(AppUser user, string returnUrl);
        Task<IdentityResult> ConfirmEmailAsync(AppUser user, string token);
        Task<bool> IsEmailConfirmedAsync(AppUser user);
        Task<List<AppUser>> GetAllUsersAsync(UserQueryObject query, Guid? organizationId = null);
    }
}