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

        // GET: AdminController
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ViewUsers()
        {
            blogData.Users = _userManager.Users.ToList();

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
