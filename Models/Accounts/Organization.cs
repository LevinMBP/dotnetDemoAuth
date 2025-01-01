using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DemoAuth.Models.Accounts
{
    public class Organization
    {
        public Guid Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string CompanyName { get; set; } = string.Empty;
        [Required]
        [EmailAddress]
        [MaxLength(50)]
        public string Email { get; set; } = string.Empty;
        [Required]
        [MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;
        [Required]
        [MaxLength(50)]
        public string ContactPerson { get; set; } = string.Empty;
        public bool Subscribed { get; set; } = false;
        public DateTime? SubscribedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<AppUser> Users { get; set; } = new List<AppUser>();
    }
}