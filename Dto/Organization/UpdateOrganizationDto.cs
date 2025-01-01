using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DemoAuth.Dto.Accounts
{
    public class UpdateOrganizationDto
    {
        [Required]
        [MaxLength(50, ErrorMessage = "Company name cannot be over 50 characters")]
        public string CompanyName { get; set; } = string.Empty;
        [Required]
        [EmailAddress]
        [MaxLength(50, ErrorMessage = "Email cannot be over 50 characters")]
        public string Email { get; set; } = string.Empty;
        [Required]
        [MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;
        [Required]
        [MaxLength(50, ErrorMessage = "Contact Person cannot be over 50 characters")]
        public string ContactPerson { get; set; } = string.Empty;
        public bool Subscribed { get; set; } = false;
    }
}