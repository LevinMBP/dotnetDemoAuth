using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DemoAuth.Dto.Accounts;
using DemoAuth.Helpers;
using DemoAuth.Models.Accounts;

namespace DemoAuth.Interface
{
    public interface IOrganizationRepository
    {
        Task<Organization> CreateOrganizationAsync(RegisterAsPartnerRequestDto org);
        Task<Organization?> FindByCompanyNameAsync(string companyName);
        Task<Organization?> GetOrganizationByIdAsync(Guid Id);
        Task<List<Organization>> GetAllOrganizationAsync(OrganizationQueryObject query);
        Task<(Organization?, bool)> UpdateOrganizationAsync(Guid id, UpdateOrganizationDto organizationDto);
        Task<Organization?> DeleteOrganizationAsync(Guid id);
    }
}