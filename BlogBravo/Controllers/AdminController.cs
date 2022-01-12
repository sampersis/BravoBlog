using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BlogBravo.Data;
using BlogBravo.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity.UI.Services;
using BlogBravo.Areas.Identity.Pages.Account;
using System.Net.Mail;
using System.Text.RegularExpressions;


namespace BlogBravo.Controllers 
{
    [Authorize(Roles = "sysadmin")]

    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        public BlogData blogData;

        public AdminController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _logger = logger;
            _emailSender = emailSender;
             blogData = new BlogData();
        }

        // First Page for Admin Pages
        public ActionResult Index()
        {
            return View();
        }

        // Partial Requests that results in partial views to be loaded in the first page
        public ActionResult ViewUsers()
        {
            blogData.Users = _userManager.Users.OrderBy(u=>u.LastName).ThenByDescending(u=>u.FirstName).ToList();
            ViewBag.Roles = _roleManager.Roles.ToList();

            return View(blogData.Users);
        }
        
        public ActionResult ViewRoles()
        {
            blogData.Roles = _roleManager.Roles.ToList();

            return View(blogData.Roles);
        }

        public ActionResult ViewBlogs()
        {
            blogData.Blogs = _context.Blogs.ToList();

            return View(blogData.Blogs);
        }

        public ActionResult ViewPosts()
        {
            blogData.Posts = _context.Posts.ToList();
            return View(blogData.Posts);
        }

        public ActionResult ViewComments()
        {
            blogData.Comments = _context.Comments.ToList();

            return View(blogData.Comments);
        }

        public ActionResult ViewTags()
        {
            blogData.Tags = _context.Tags.Include(t => t.Post).ToList();
            blogData.Tags = blogData.Tags.OrderByDescending(t => t.Post.Count).ToList();

            return View(blogData.Tags);
        }

        // ------------------------------------------- Role Methods --------------------------------------//

        // Create a new role
        [HttpPost]
        public async Task<IActionResult> CreateRole()
        {
            string role = Request.Form["role"];

            if (!String.IsNullOrEmpty(role))
            {
                var _role = await _roleManager.RoleExistsAsync(role);

                if (!_role)
                {
                    var RoleCreationStatus = await _roleManager.CreateAsync(new IdentityRole { Name = role });

                    if (RoleCreationStatus.Succeeded)
                    {
                        _logger.LogInformation("Role {0} created successfully: {1}", role, RoleCreationStatus.Succeeded);
                    }
                    else
                    {
                        _logger.LogInformation("Failed to create role {0}: {1}", role, RoleCreationStatus.Errors);
                    }
                }
                else
                {
                    ViewBag.Error = role + " already Exists";
                }
            }
            else
            {
                ViewBag.RoleStrEmpty = "Cannot create role based on an empty role string";
            }

            return RedirectToAction("Index");
        }

        // View a role
        public ActionResult ViewRole()
        {
            string path = HttpContext.Request.Path;
            string[] pathComponents = path.Split('/');
            string roleId = pathComponents[pathComponents.Length - 1];

            if(!String.IsNullOrEmpty(roleId))
            {
                var role = _roleManager.Roles.FirstOrDefault(r => r.Id == roleId);
                return View(role);
            }

            return NotFound();
        }

        // Delete a Role
        public ActionResult DeleteRole()
        {
            string path = HttpContext.Request.Path;
            string[] pathComponents = path.Split('/');
            string roleId = pathComponents[pathComponents.Length - 1];

            if (!String.IsNullOrEmpty(roleId))
            {
                var role = _roleManager.Roles.FirstOrDefault(r => r.Id == roleId);
                return View(role);
            }

            return NotFound();
        }

        [HttpPost, ActionName("DeleteRole")]
        public async Task<IActionResult> DeleteRoleConfirmed(string? roleId)
        {

            if (!String.IsNullOrEmpty(roleId))
            {
                var role = _roleManager.Roles.FirstOrDefault(r => r.Id == roleId);
                if (role != null)
                {
                    var roleDeletion = await _roleManager.DeleteAsync(role);

                    if (roleDeletion.Succeeded)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        return NotFound();
                    }
                }
                else
                {
                    return NotFound();
                }
            }

            return RedirectToAction("Index");
        }

        // Edit a Role
        public ActionResult EditRole()
        {
            string path = HttpContext.Request.Path;
            string[] pathComponents = path.Split('/');
            string roleId = pathComponents[pathComponents.Length - 1];

            if (!String.IsNullOrEmpty(roleId))
            {
                var role = _roleManager.Roles.FirstOrDefault(r => r.Id == roleId);
                return View(role);
            }

            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> EditRole(string? roleId)
        {
            if (!String.IsNullOrEmpty(roleId))
            {
                var role = _roleManager.Roles.FirstOrDefault(r => r.Id == roleId);
                role.Name = Request.Form["roleName"];

                var roleUpdate = await _roleManager.UpdateAsync(role);

                if (roleUpdate.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    return NotFound();
                }
            }
            else
            {
                return NotFound();

            }
        }

        //-------------------------------------------- User Methods -------------------------------------------------//

        [HttpPost]
        [Route("/error-development")]      
        public IActionResult HandleError() => Problem();
        public async Task<IActionResult> CreateUser()
        {
            if (ModelState.IsValid)
            {
                if (await _userManager.FindByEmailAsync(Request.Form["email"]) == null)
                {

                    ApplicationUser newUser = new ApplicationUser
                    {
                        UserName = Request.Form["email"],
                        FirstName = Request.Form["first-name"],
                        LastName = Request.Form["last-name"],
                        Email = Request.Form["email"]
                    };

                    IdentityResult UserCreatedOK = await _userManager.CreateAsync(newUser, Request.Form["password"]);
                    if (UserCreatedOK.Succeeded)
                    {
                        IdentityResult RoleAddedtoUserSuccess = await _userManager.AddToRoleAsync(newUser, Request.Form["role"]);
                        if (RoleAddedtoUserSuccess.Succeeded)
                        {
                            return View("Index");
                        }
                        else
                        {
                            foreach (IdentityError error in RoleAddedtoUserSuccess.Errors)
                                ModelState.AddModelError("", error.Description);
                            return Problem("Failed to creaye user!" + ModelState);
                        }
                    }
                    else
                    {
                        foreach (IdentityError error in UserCreatedOK.Errors)
                            ModelState.AddModelError("", error.Description);
                        return Problem("Failed to creaye user!" + ModelState);
                    }
                }
                else
                {
                    return Problem("A User with this email has already resistered: " + Request.Form["email"]);
                }

            }

            return View("Index");
        }

        public ActionResult DeleteUser()
        {
            string path = HttpContext.Request.Path;
            string[] pathComponents = path.Split('/');
            string userId = pathComponents[pathComponents.Length - 1];

            if (!String.IsNullOrEmpty(userId))
            {
                ApplicationUser user = _userManager.Users.FirstOrDefault(u => u.Id == userId);

                if (user != null)
                {
                    return View(user);
                }
                else
                { 
                    return Problem("Failed to retrieve information about the user with ID: " + userId);
                }
            }
            else
            {
                return Problem("User ID was not passed in the URL");
            }
        }

        [HttpPost, ActionName("DeleteUser")]
        public async Task<IActionResult> DeleteUserConfirmed(string? userId)
        {
            if (userId != null)
            {
                var user = _userManager.Users.FirstOrDefault(u => u.Id == userId);

                if (user != null)
                {
                    if (await _userManager.DeleteAsync(user) != null)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        return Problem(user.Email + " Could not be removed!");
                    }
                }
                else
                {
                    return Problem("Failed to retrieve information about the user with ID: " + userId);
                }
            }
            else
            {
                return Problem("User ID was null");
            }
        }

        public async Task<ActionResult> EditUser()
        {
            string path = HttpContext.Request.Path;
            string[] pathComponents = path.Split('/');
            string userId = pathComponents[pathComponents.Length - 1];

            if (!String.IsNullOrEmpty(userId))
            {
                var user = _userManager.Users.FirstOrDefault(u => u.Id == userId);

                if (user != null)
                {
                    IList<string> role = await _signInManager.UserManager.GetRolesAsync(user);
                    if (role.Count > 0)
                    {
                        ViewBag.Role = role.ElementAt(0);
                    }
                    else
                    {
                        ViewBag.Role = "undefined";
                    }
                    return View(user);
                }
                else
                {
                    return Problem("Failed to retrieve information about the user with ID: " + userId);
                }
            }
            else
            {
                return Problem("User ID was not passed in the URL");
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(string? userId)
        {
            if (userId != null)
            {
                var user = _userManager.Users.FirstOrDefault(u => u.Id == userId);

                if (user != null)
                {
                    user.FirstName = Request.Form["user-first-name"];
                    user.LastName = Request.Form["user-last-name"];
                    user.UserName = Request.Form["user-name"];
                    user.Email = Request.Form["user-email"];
                    
                    if(!String.IsNullOrWhiteSpace(Request.Form["user-role"]) && !String.IsNullOrEmpty(Request.Form["user-role"]))
                    {
                        var userCurrentRole = await _signInManager.UserManager.GetRolesAsync(user);
                        var updateUserRole = await _signInManager.UserManager.RemoveFromRolesAsync(user, userCurrentRole);
                        if (updateUserRole.Succeeded)
                        {
                            updateUserRole = await _signInManager.UserManager.AddToRoleAsync(user, Request.Form["user-role"]);
                            if (!updateUserRole.Succeeded)
                            {
                                return Problem($"Could not add role {Request.Form["user-role"]} from user {user.UserName}");
                            }
                        }
                        else
                        {
                            return Problem($"Could not remove role {userCurrentRole.ElementAt(0)} from user {user.UserName}");
                        }
                    }

                    if (!String.IsNullOrWhiteSpace(Request.Form["user-password"]) && !String.IsNullOrEmpty(Request.Form["user-password"]))
                    {
                        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                        var passwordChanged = await _userManager.ResetPasswordAsync(user, token, Request.Form["user-password"]);
                        if(!passwordChanged.Succeeded)
                        {
                            return Problem("Password update for user failed: " + userId + " " + user.Email);
                        }
                    }

                    var userUpdate = await _userManager.UpdateAsync(user);

                    if (userUpdate.Succeeded)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        return Problem("Failed to update user with ID: " + userId);
                    }
                }
                else
                {
                    return Problem("Failed to retrieve information about the user with ID: " + userId);
                }
            }
            else
            {
                return Problem("User ID was not passed in the URL");

            }
        }
        //------------------------------------------------Tag Methods ------------------------------------------------//

        // Remove a Tag from Tags table
        public ActionResult DeleteTag()
        {
            string path = HttpContext.Request.Path.ToString();
            string[] pathComponent = path.Split('/');
            int tagId = Convert.ToInt32(pathComponent[pathComponent.Length - 1]);
            Tag tag =  _context.Tags.Find(tagId);
            _context.Tags.Remove(tag);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

    }
}
