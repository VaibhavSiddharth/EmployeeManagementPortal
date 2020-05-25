using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeRegistrationApp.ViewModels
{
    public class EmployeeEditViewModel:EmployeeViewModel
    {   
        public int Id { get; set; }

        public string ExistingPhotoPath { get; set; }
    }
}
