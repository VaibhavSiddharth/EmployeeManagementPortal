using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using EmployeeRegistrationApp.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeRegistrationApp.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        [Remote(action: "IsEmailInUse", controller:"Account")]
        [ValidEmailDomain(allowedDomain: "zenith.com", ErrorMessage ="Domain must be zenith.com")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        
        [Display(Name ="Confirm Password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage ="Password and Confirm Password does not match")]
        public string ConfirmPassword { get; set; }

        public string City { get; set; }
    }
}
