using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeRegistrationApp.Models
{
    public class MockEmployeeRepository : IEmployeeRepository
    {
        private List<Employee> _employeeList;

        public MockEmployeeRepository()
        {
            _employeeList = new List<Employee>
                            {
                            new Employee{Id=101,Name="Mark",Department=Dept.Operation,Email="abc@yahoo.com"},
                            new Employee{Id=102,Name="John",Department=Dept.Development,Email="xyz@yahoo.com"},
                            new Employee{Id=103,Name="Peter",Department=Dept.ProductionSupport,Email="pqr@yahoo.com"}
                            };
        }

        public Employee Add(Employee employee)
        {
            employee.Id = _employeeList.Max(e => e.Id) + 1;
            _employeeList.Add(employee);
            return employee;
        }

        public Employee Delete(int Id)
        {
            Employee employee = _employeeList.FirstOrDefault(e => e.Id == Id);
            if(employee!=null)
            {
                _employeeList.Remove(employee);
            }
            return employee;
        }

        public Employee Update(Employee employeeChanges)
        {
            Employee employee = _employeeList.FirstOrDefault(e => e.Id == employeeChanges.Id);
            if(employee != null)
            {
                employee.Name = employeeChanges.Name;
                employee.Email = employeeChanges.Email;
                employee.Department = employeeChanges.Department;
            }
            return employee;
        }

        Employee IEmployeeRepository.GetEmployee(int Id)
        {
            return _employeeList.FirstOrDefault(e => e.Id == Id);
        }

        IEnumerable<Employee> IEmployeeRepository.GetEmployees()
        {
            return _employeeList;
        }
    }
}
