using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DemoAuth.Dto.Accounts
{
    public class RegisterAsPartnerRequestDto
    {
        [Required]
        public string CompanyName { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string ContactPerson { get; set; }
        [Required]
        public string Password { get; set; }
        public bool Subscribed { get; set; }
    }
}