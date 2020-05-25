using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace EmployeeRegistrationApp.Models
{
    public static class ModelBuilderExtension
    {
        public static void Seed(this ModelBuilder modelBuilder) 
        {
            modelBuilder.Entity<Employee>().HasData(
                new Employee
                {
                    Id = 1,
                    Name = "Mary",
                    Department = Dept.Development,
                    Email = "mary@zenith.com"
                },
                new Employee
                {
                    Id = 2,
                    Name = "John",
                    Department = Dept.Operation,
                    Email = "john@zenith.com"
                }
            );
        }
    }
}
