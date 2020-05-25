using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using EmployeeRegistrationApp.Models;
using Microsoft.AspNetCore.Http;

namespace EmployeeRegistrationApp.ViewModels
{
    public class EmployeeViewModel
    {

        [Required]
        [MaxLength(50, ErrorMessage = "Name cannot exceed more than 50 characters ")]
        public string Name { get; set; }

        [Required]
        [RegularExpression(@"^([a-zA-Z0-9_\-\.]+)@([a-zA-Z0-9_\-\.]+)\.([a-zA-Z]{2,5})$", ErrorMessage = "Invalid Email Address")]
        [Display(Name = "Office Email")]
        public string Email { get; set; }

        [Required]
        public Dept? Department { get; set; }

        public IFormFile Photo { get; set; }
    }
}
