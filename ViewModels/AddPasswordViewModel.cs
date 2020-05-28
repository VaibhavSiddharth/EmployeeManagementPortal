using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeRegistrationApp.ViewModels
{
    public class AddPasswordViewModel
    {
        [Required]
        [Display(Name = "New Password")]
        [DataType(DataType.Password)]

        public string NewPassword { get; set; }

        [Display(Name ="Confirm Password")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage ="New Password and Confirm Password does not match")]
        public string ConfirmPassword { get; set; }
    }
}
