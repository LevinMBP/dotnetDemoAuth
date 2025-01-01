using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using DemoAuth.Dto.Accounts;

namespace DemoAuth.Dto.Organization
{
    public class OrganizationDto
    {
        public Guid Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
        public bool Subscribed { get; set; } = false;
        public DateTime? SubscribedAt { get; set; }
        public List<UserDto> Users { get; set; } = new List<UserDto>();
    }
}