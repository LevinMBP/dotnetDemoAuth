using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DemoAuth.Dto.Accounts;
using DemoAuth.Models.Accounts;
using Microsoft.AspNetCore.StaticAssets;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DemoAuth.Mappers
{
    public static class AppUserMappers
    {
        public static UserDto ToUserDto(this AppUser userModel)
        {
            return new UserDto
            {
                Id = userModel.Id,
                FirstName = userModel.FirstName,
                LastName = userModel.LastName,
                Email = userModel.Email,
                PhoneNumber = userModel.PhoneNumber,
                EmployeeType = userModel.EmployeeType,
                OrganizationId = userModel.OrganizationId,
                EmailConfirmed = userModel.EmailConfirmed,
                PhoneNumberConfirmed = userModel.PhoneNumberConfirmed
            };
        }

        public static AppUser ToAppUserFromCreatePartner(
            this RegisterAsPartnerRequestDto registerRequest,
            Guid oId,
            string role)
        {
            return new AppUser
            {
                UserName = registerRequest.Email,
                FirstName = registerRequest.FirstName,
                LastName = registerRequest.LastName,
                Email = registerRequest.Email,
                PhoneNumber = registerRequest.PhoneNumber,
                EmployeeType = role,
                OrganizationId = oId,
                CreatedAt = DateTime.UtcNow
            };
        }

        public static AppUser ToAppUserFromCreateUserRequest(
            this RegisterUserRequestDto registerRequest,
            Guid organizationId)
        {
            return new AppUser
            {
                UserName = registerRequest.Email,
                FirstName = registerRequest.FirstName,
                LastName = registerRequest.LastName,
                Email = registerRequest.Email,
                PhoneNumber = registerRequest.PhoneNumber,
                EmployeeType = registerRequest.EmployeeType,
                OrganizationId = organizationId,
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}