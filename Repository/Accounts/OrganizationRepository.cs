using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DemoAuth.Data;
using DemoAuth.Dto.Accounts;
using DemoAuth.Helpers;
using DemoAuth.Interface;
using DemoAuth.Models.Accounts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DemoAuth.Repository.Accounts
{
    public class OrganizationRepository : IOrganizationRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        public OrganizationRepository(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<Organization> CreateOrganizationAsync(RegisterAsPartnerRequestDto org)
        {
            var organization = new Organization
            {
                CompanyName = org.CompanyName,
                Email = org.Email,
                PhoneNumber = org.PhoneNumber,
                ContactPerson = $"{org.FirstName} {org.LastName}",
                Subscribed = org.Subscribed,
                SubscribedAt = org.Subscribed ? DateTime.UtcNow : null,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Organizations.AddAsync(organization);
            await _context.SaveChangesAsync();
            return organization;
        }

        public async Task<Organization?> DeleteOrganizationAsync(Guid id)
        {
            var organization = await _context.Organizations.Include(o => o.Users).FirstOrDefaultAsync(o => o.Id == id);

            if (organization == null) return null;

            _context.Remove(organization);
            await _context.SaveChangesAsync();
            return organization;
        }

        public async Task<Organization?> FindByCompanyNameAsync(string companyName)
        {
            return await _context.Organizations
                .Include(u => u.Users)
                .FirstOrDefaultAsync(o => o.CompanyName == companyName);
        }

        public async Task<Organization?> GetOrganizationByIdAsync(Guid Id)
        {
            return await _context.Organizations
                .Include(u => u.Users)
                .FirstOrDefaultAsync(i => i.Id == Id);
        }

        public async Task<List<Organization>> GetAllOrganizationAsync(OrganizationQueryObject query)
        {
            var organization = _context.Organizations.Include(u => u.Users).AsQueryable();

            // Query by Organization Company Name
            if (!string.IsNullOrWhiteSpace(query.CompanyName))
            {
                organization = organization.Where(s => s.CompanyName.Contains(query.CompanyName));
            }

            if (!string.IsNullOrWhiteSpace(query.SortBy))
            {
                // sortby can be dynamic
                if (query.SortBy.Equals("CompanyName", StringComparison.OrdinalIgnoreCase))
                {
                    organization = query.IsDescending
                    ? organization.OrderByDescending(o => o.CompanyName)
                    : organization.OrderBy(o => o.CompanyName);
                }
            }

            var skipNumber = (query.PageNumber - 1) * query.PageSize;

            return await organization.Skip(skipNumber).Take(query.PageSize).ToListAsync();
        }

        public async Task<(Organization?, bool)> UpdateOrganizationAsync(Guid id, UpdateOrganizationDto organizationDto)
        {
            var organization = await _context.Organizations
                .FirstOrDefaultAsync(o => o.Id == id);

            if (organization == null) return (null, false);

            // Store the old email to compare with the new one
            var oldEmail = organization.Email;
            bool emailDidUpdate = false;

            organization.CompanyName = organizationDto.CompanyName;
            organization.Email = organizationDto.Email;
            organization.PhoneNumber = organizationDto.PhoneNumber;
            organization.ContactPerson = organizationDto.ContactPerson;
            organization.Subscribed = organizationDto.Subscribed;
            organization.SubscribedAt = organizationDto.Subscribed ? DateTime.UtcNow : null;

            // If the email is updated, update the user's email as well
            if (oldEmail != organizationDto.Email)
            {
                // Fetch the user associated with this organization using oldemail
                // Use the old email because there's one or more users attached to an organization
                var user = await _userManager.FindByEmailAsync(oldEmail);
                if (user == null)
                {
                    Console.WriteLine($"Update failed. Cannot find user attached to this organization");
                    return (null, emailDidUpdate);
                }

                // Check the email to ensure it's not taken by another user using new email
                var userExists = await _userManager.FindByEmailAsync(organizationDto.Email);
                if (userExists != null)
                {
                    Console.WriteLine($"Update failed. User already exists with this email");
                    throw new InvalidOperationException("Email is already taken by another user.");
                }

                // Update user email
                user.Email = organizationDto.Email;
                user.EmailConfirmed = false;
                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    Console.WriteLine("Failed to update user email");
                    return (null, emailDidUpdate);
                }

                emailDidUpdate = true;

            }

            await _context.SaveChangesAsync();

            return (organization, emailDidUpdate);
        }
    }
}