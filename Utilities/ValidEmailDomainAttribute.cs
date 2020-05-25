using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeRegistrationApp.Utilities
{
    public class ValidEmailDomainAttribute:ValidationAttribute
    {
        private readonly string allowedDomain;

        public ValidEmailDomainAttribute(string allowedDomain)
        {
            this.allowedDomain = allowedDomain;
        }
        public override bool IsValid(object value)
        {
            string emailDomain = value.ToString().Split('@')[1];
            return emailDomain.ToUpper() == allowedDomain.ToUpper();
        }
    }
}
