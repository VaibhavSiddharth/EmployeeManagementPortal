using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using EmployeeRegistrationApp.Models;
using EmployeeRegistrationApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EmployeeRegistrationApp.Controllers
{
    //[Authorize(Policy = "AdminRolePolicy")]
    public class AdministrationController : Controller
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ILogger<AdministrationController> logger;

        public AdministrationController(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager,
                                        ILogger<AdministrationController> logger)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
            this.logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpPost]
        
        public async Task<IActionResult> ManageUserClaims(UserClaimsViewModel model)
        {
            var user = await userManager.FindByIdAsync(model.UserId);
            if(user == null)
            {
                ViewBag.ErrorMessage = $"User with id:{model.UserId} cannot be found";
                return View("NotFound");
            }

            var claims = await userManager.GetClaimsAsync(user);
            var result = await userManager.RemoveClaimsAsync(user, claims);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot remove existing user claims");
                return View(model);
            }

            result = await userManager.AddClaimsAsync(user, model.Claims.Select(
                                                      c => new Claim(c.ClaimType, c.IsSelected? "true":"false")
                                                      )
                                                     );

            if (!result.Succeeded)
            {
                ModelState.AddModelError(String.Empty, "Cannot add selected claims to the user");
                return View(model);
            }

            return RedirectToAction("EditUsers", new { id = model.UserId });
        }

        [HttpGet]
        
        public async Task<IActionResult> ManageUserClaims(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);

            if(user == null)
            {
                ViewBag.ErrorMessage = $"User with id:{userId} could not be found";
                return View("NotFound");
            }

            var existingUserClaims = await userManager.GetClaimsAsync(user);
            var model = new UserClaimsViewModel { UserId = userId };

            foreach(Claim claim in ClaimsStore.AllClaims)
            {
                UserClaim userClaim = new UserClaim 
                                      {
                                        ClaimType = claim.Type
                                      };
                if(existingUserClaims.Any(c=>c.Type == claim.Type && c.Value == "true"))
                {
                    userClaim.IsSelected = true;
                }
                model.Claims.Add(userClaim);

            }
            return View(model);

        }
        [HttpGet]
        [Authorize(Policy = "EditRolePolicy")]
        public async Task<IActionResult> ManageUserRoles(string userId)
        {
            ViewBag.UserId = userId;
            var user = await userManager.FindByIdAsync(userId);
            if(user == null)
            {
                ViewBag.ErrorMessage = $" User with id:{userId} could not be found";
                return View("NotFound");
            }

            var model = new List<UserRolesViewModel>();

            foreach(var role in roleManager.Roles)
            {
                var userRolesViewModel = new UserRolesViewModel
                {
                    RoleId = role.Id,
                    RoleName = role.Name
                };

                if(await userManager.IsInRoleAsync(user, role.Name))
                {
                    userRolesViewModel.isSelected = true;
                }

                else
                {
                    userRolesViewModel.isSelected = false;
                }

                model.Add(userRolesViewModel);
            }
            return View(model);
        }

        [HttpPost]
        [Authorize(Policy = "EditRolePolicy")]
        public  async Task<IActionResult> ManageUserRoles(List<UserRolesViewModel> model, string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if(user == null)
            {
                ViewBag.ErrorMessage = $"User with id:{userId} could not be found";
                return View("NotFound");
            }

            var roles = await userManager.GetRolesAsync(user);
            var result = await userManager.RemoveFromRolesAsync(user, roles);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot remove existing roles for the user");
                return View(model);
            }

            result = await userManager.AddToRolesAsync(user, model.Where(x => x.isSelected).Select(y => y.RoleName));

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot add selected roles to user");
                return View(model);
            }

            return RedirectToAction("EditUsers", new { id = userId });
        }

        [HttpPost]
        [Authorize(Policy = "DeleteRolePolicy")]
        public async Task<IActionResult> DeleteRole(string id)
        {
            var role = await roleManager.FindByIdAsync(id);
            if(role == null)
            {
                ViewBag.ErrorMessage = $"Role with id:{id} cannot be found";
                return RedirectToAction("NotFound");
            }
            else
            {
                try
                {
                    //throw new Exception("Custom Exception");
                    var result = await roleManager.DeleteAsync(role);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("ListRoles");
                    }
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View("ListRoles");
                }
                catch(DbUpdateException ex)
                {
                    logger.LogError($"Error deleting role.Error : {ex.Message}");
                    ViewBag.ErrorTitle = $"{role.Name} role is already in use";
                    ViewBag.ErrorMessage = $"{role.Name} cannot be deleted as there are users associated with it." +
                                           $"Please delete the associated users and then try again...";
                    return View("Error");
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await userManager.FindByIdAsync(id);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with id :{id} cannot be found";
                return RedirectToAction("NotFound");
            }

            var result = await userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction("ListUsers");
            }


            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return View("ListUsers");

        }
        [HttpGet]
        public async Task<IActionResult> EditUsers(string id)
        {
            var user = await userManager.FindByIdAsync(id);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {id} cannot be found";
                return View("NotFound");
            }

            var userClaims = await userManager.GetClaimsAsync(user);
            var roles = await userManager.GetRolesAsync(user);

            var model = new EditUserViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                City = user.City,
                Claims = userClaims.Select(c =>c.Type+":"+ c.Value).ToList(),
                Roles = roles
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditUsers(EditUserViewModel editUserViewModel)
        {
            var user = await userManager.FindByIdAsync(editUserViewModel.Id);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {editUserViewModel.Id} cannot be found";
                return View("NotFound");
            }

            else
            {
                user.Email = editUserViewModel.Email;
                user.UserName = editUserViewModel.UserName;
                user.City = editUserViewModel.City;

                var result = await userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    return RedirectToAction("ListUsers");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(editUserViewModel);
        }


        [HttpGet]
        public IActionResult ListUsers()
        {
            var users = userManager.Users;
            return View(users);
        }

        [HttpGet]
        public IActionResult CreateRole()
        {
            return View();
        }



        [HttpPost]
        public async Task<IActionResult> CreateRole(CreateRoleViewModel model)
        {
            if (ModelState.IsValid)
            {
                IdentityRole identityRole = new IdentityRole { Name = model.RoleName };
                IdentityResult result = await roleManager.CreateAsync(identityRole);
                if (result.Succeeded)
                {
                    return RedirectToAction("ListRoles", "Administration");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult ListRoles()
        {
            var roles = roleManager.Roles;
            return View(roles);
        }

        [HttpGet]
        //[Authorize(Policy = "EditRolePolicy")]
        public async Task<IActionResult> EditRole(string Id)
        {
            var role = await roleManager.FindByIdAsync(Id);
            if (role == null)
            {
                ViewBag.ErrorMesage = $"Role with id {Id} cannot be found";
                return View("NotFound");
            }

            var model = new EditRoleViewModel
            {
                Id = role.Id,
                RoleName = role.Name
            };

            foreach (var user in userManager.Users)
            {

                if (await userManager.IsInRoleAsync(user, role.Name))
                {
                    model.Users.Add(user.UserName);
                }


            }

            return View(model);
        }

        [HttpPost]
        //[Authorize(Policy = "EditRolePolicy")]
        public async Task<IActionResult> EditRole(EditRoleViewModel rolemodel)
        {
            var role = await roleManager.FindByIdAsync(rolemodel.Id);
            if (role == null)
            {
                ViewBag.ErrorMesage = $"Role with id {rolemodel.Id} cannot be found";
                return View("NotFound");
            }

            else
            {
                role.Name = rolemodel.RoleName;
                var result = await roleManager.UpdateAsync(role);
                if (result.Succeeded)
                {
                    return RedirectToAction("ListRoles", "Administration");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(rolemodel);
            }


        }

        [HttpGet]
        public async Task<IActionResult> EditUsersInRole(string roleId)
        {
            ViewBag.RoleId = roleId;
            var role = await roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                ViewBag.ErrorMesage = $"Role with id {roleId} cannot be found";
                return View("NotFound");
            }

            var model = new List<UserRoleViewModel>();

            foreach (var user in userManager.Users)
            {
                var userRoleViewModel = new UserRoleViewModel
                {
                    UserId = user.Id,
                    UserName = user.UserName
                };

                if (await userManager.IsInRoleAsync(user, role.Name))
                {
                    userRoleViewModel.IsSelected = true;
                }
                else
                {
                    userRoleViewModel.IsSelected = false;
                }
                model.Add(userRoleViewModel);
            }
            return View(model);

        }

        [HttpPost]
        public async Task<IActionResult> EditUsersInRole(List<UserRoleViewModel> model, string roleId)
        {
            var role = await roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                ViewBag.ErrorMesage = $"Role with id {roleId} cannot be found";
                return View("NotFound");
            }

            for (int i = 0; i < model.Count; i++)
            {
                var user = await userManager.FindByIdAsync(model[i].UserId);

                IdentityResult result = null;
                if (model[i].IsSelected && !(await userManager.IsInRoleAsync(user, role.Name)))
                {
                    result = await userManager.AddToRoleAsync(user, role.Name);
                }

                else if (!model[i].IsSelected && (await userManager.IsInRoleAsync(user, role.Name)))
                {
                    result = await userManager.RemoveFromRoleAsync(user, role.Name);
                }

                else
                {
                    continue;
                }

                if (result.Succeeded)
                {
                    if (i < model.Count - 1)
                    {
                        continue;
                    }

                    else
                    {
                        return RedirectToAction("EditRole", new { Id = roleId });
                    }
                }
            }

            return RedirectToAction("EditRole", new { Id = roleId });

        }


    }
}
