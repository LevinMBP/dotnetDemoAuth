using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DemoAuth.Dto.Organization;
using DemoAuth.Models.Accounts;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DemoAuth.Mappers
{
    public static class OrganizationMappers
    {
        public static OrganizationDto ToOrganizationDto(this Organization organizationModel)
        {
            return new OrganizationDto
            {
                Id = organizationModel.Id,
                CompanyName = organizationModel.CompanyName,
                Email = organizationModel.Email,
                PhoneNumber = organizationModel.PhoneNumber,
                ContactPerson = organizationModel.ContactPerson,
                Subscribed = organizationModel.Subscribed,
                SubscribedAt = organizationModel.SubscribedAt,
                Users = organizationModel.Users.Select(o => o.ToUserDto()).ToList()
            };
        }
    }
}