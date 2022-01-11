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
        public async Task<IActionResult> CreateUser()
        {
            // Do not waste time. If it is not possible to create an email address then the email is not OK and the user cannot be created

            string email = Request.Form["email"];
            //string emailReGExpression = @"^(?("")("".+?(?<!\\)""@)| (([0 - 9a - z]((\.(? !\.)) |[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$";
            string emailReGExpression = @"^[a-zA-Z0-9_!#$%&’*+/=?`{|}~^-]+(?:\\.[a-zA-Z0-9_!#$%&’*+/=?`{|}~^-]+)*@[a-zA-Z0-9-]+(?:\\.[a-zA-Z0-9-]+)*$";

            Regex emailCheck = new Regex(emailReGExpression);

            // MailAddress.TryCreate(email, out MailAddress emailAddress) did not work. It is not fully 5322  complaint

            if (!string.IsNullOrEmpty(email) && emailCheck.IsMatch(email))
            {
                if (ModelState.IsValid)
                {
                    ApplicationUser newUser = new ApplicationUser
                    {
                        UserName = email,
                        FirstName = Request.Form["first-name"],
                        LastName = Request.Form["last-name"],
                        Email = email
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
                        }
                    }
                    else
                    {
                        foreach (IdentityError error in UserCreatedOK.Errors)
                            ModelState.AddModelError("", error.Description);
                    }
                }
            }
            else
            {
                //failed
            }

            return View("Index");
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
