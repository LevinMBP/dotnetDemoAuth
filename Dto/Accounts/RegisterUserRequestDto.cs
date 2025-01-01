using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DemoAuth.Dto.Accounts
{
    public class RegisterUserRequestDto
    {
        [Required]
        [MaxLength(50, ErrorMessage = "First name can only have 50 characters")]
        public string FirstName { get; set; }
        [Required]
        [MaxLength(50, ErrorMessage = "Last name can only have 50 characters")]
        public string LastName { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        [Required]
        public string EmployeeType { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        // [Required]
        // public Guid OrganizationId { get; set; } // Foreign key to Organization
    }
}