using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EmployeeRegistrationApp.Models;
using EmployeeRegistrationApp.Security;
using EmployeeRegistrationApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EmployeeRegistrationApp.Controllers
{
    //[Route("[Controller]/[action]")]
    [Authorize]
    public class HomeController:Controller
    {
        private IEmployeeRepository _employeeRepository;
        private IHostingEnvironment hostingEnvironment;
        private readonly ILogger<HomeController> logger;
        private readonly IDataProtector protector;

        //public string Index()
        //{
        //    return "Hello World!";
        //}
        //public JsonResult Index() 
        //{
        //    return Json(new { id = 1, Name = "Vaibhav" });
        //}
        public HomeController(IEmployeeRepository employeeRepository, IHostingEnvironment hostingEnvironment,
                              ILogger<HomeController> logger,
                              IDataProtectionProvider dataProtectionProvider,
                              DataProtectionPurposeStrings dataProtectionPurposeStrings)
        {
            
            this._employeeRepository = employeeRepository;
            this.hostingEnvironment = hostingEnvironment;
            this.logger = logger;
            protector = dataProtectionProvider.CreateProtector(dataProtectionPurposeStrings.EmployeeIdRouteValue);
        }

        //[Route("")]  
        //[Route("~/Home")] 
        [AllowAnonymous]
        public ViewResult Index() 
        {
            //return _employeeRepository.GetEmployee(101).Name;
            IEnumerable<Employee> employeeList = _employeeRepository.GetEmployees().
                                                  Select( e=> {
                                                      e.EncryptedId = protector.Protect(e.Id.ToString());
                                                      return e;
                                                  });
            return View(employeeList);
        }

        //public string Details(int?id,string name) 
        //{
        //    return "id : "+ id.Value.ToString() + " name: " + name;
        //}
        //[Route("{id?}")]
        [AllowAnonymous]
        public ViewResult Details(string id)
        {
            
            int employeeId = Convert.ToInt32(protector.Unprotect(id));
            Employee employee = _employeeRepository.GetEmployee(employeeId);
            if(employee == null) 
            {
                Response.StatusCode = 404;
                return View("EmployeeNotFound", employeeId);
            }

            HomeDetailsViewModel homeDetailsViewModel = new HomeDetailsViewModel
            {
                PageTitle = "Employee-Details",
                Employee = employee
            };

            

            return View(homeDetailsViewModel);
        }

        [HttpGet]
        
        public ViewResult Create()
        {
            return View();
        }

        [HttpGet]
        
        public ViewResult Edit( int id)
        {
            Employee employee = _employeeRepository.GetEmployee(id);

            EmployeeEditViewModel model = new EmployeeEditViewModel 
                                          {
                                            Id = id,
                                            Name = employee.Name,
                                            Department = employee.Department,
                                            Email = employee.Email,
                                            ExistingPhotoPath = employee.PhotoPath
                                          };
            return View(model);
        }

        [HttpPost]
        
        public IActionResult Edit(EmployeeEditViewModel model)
        {
            
            if (ModelState.IsValid)
            {
                Employee employee = _employeeRepository.GetEmployee(model.Id);
                employee.Name = model.Name;
                employee.Department = model.Department;
                employee.Email = model.Email;

                if (model.Photo != null)
                {
                    if(model.ExistingPhotoPath!= null) 
                    {
                        string filepath = Path.Combine(hostingEnvironment.WebRootPath, "Images", model.ExistingPhotoPath);
                        System.IO.File.Delete(filepath);
                    }
                    employee.PhotoPath = ProcessUploadedFile(model);

                }
                

                 _employeeRepository.Update(employee);
                return RedirectToAction("Index");
            }

            return View();
        }

        private string ProcessUploadedFile(EmployeeViewModel model)
        {
            string uniqueFileName;
            string uploadFolder = Path.Combine(hostingEnvironment.WebRootPath, "Images");
            uniqueFileName = Guid.NewGuid().ToString() + "_" + model.Photo.FileName.Split('\\')[5];
            string uniqueFilePath = Path.Combine(uploadFolder, uniqueFileName);
            using(var filestream = new FileStream(uniqueFilePath, FileMode.Create))
            {
                model.Photo.CopyTo(filestream);
            }
            
            return uniqueFileName;
        }

        [HttpPost]
        
        public IActionResult Create(EmployeeViewModel model)
        {
            string uniqueFileName = null;
            


            if (ModelState.IsValid)
            {
                if (model.Photo != null)
                {
                    uniqueFileName = ProcessUploadedFile(model);

                }
                Employee employee = new Employee 
                                    {
                                        Name = model.Name,
                                        Email = model.Email,
                                        Department = model.Department,
                                        PhotoPath = uniqueFileName
                };

                Employee newEmployee = _employeeRepository.Add(employee);
                return RedirectToAction("Details", new { id = newEmployee.Id });
            }

            return View();
        }
    }
}
