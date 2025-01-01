using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace DemoAuth.Models.Accounts
{
    public class AppUser : IdentityUser
    {
        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; }
        [Required]
        [MaxLength(50)]
        public string LastName { get; set; }
        [Required]
        public string EmployeeType { get; set; }  // admin, staff, basic
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Guid OrganizationId { get; set; }
        public Organization Organization { get; set; }
    }
}