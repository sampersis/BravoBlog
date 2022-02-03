using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BlogBravo.Data;
using BlogBravo.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using BlogBravo.Areas.Identity.Pages.Account;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;
using Microsoft.Extensions.Options;

namespace BlogBravo.Controllers
{
    [Authorize(Roles = "author")]
    public class BlogController : Controller
    {
        private readonly HttpClient httpClient = new HttpClient();
        private readonly ApplicationDbContext _context;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private  RequestSettings _requestSettings;

        public BlogController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender, IOptions<RequestSettings> requestSettings)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _requestSettings = requestSettings.Value;
        }

        // GET: Blog
        public async Task<IActionResult> Index()
        {
            ApplicationUser author = await _userManager.GetUserAsync(HttpContext.User);
            var applicationDbContext =  _context.Blogs.Where(b => b.Author == author);            
            return View(await applicationDbContext.ToListAsync());

        }

        // GET: Blog/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return BadRequest(StatusCodes.Status406NotAcceptable);
            }

            var blog = await _context.Blogs.Include(b => b.Author)
            .FirstOrDefaultAsync(m => m.Id == id);
            if (await UserAccessAuthorization(blog.AuthorId))
            {

                if (blog == null)
                {
                    return NotFound();
                }

                return View(blog);
            }
            else
            {
                string ticketId = Guid.NewGuid().ToString();
                return Unauthorized($"Access Denied! A security Incident Ticket {ticketId} created!");
            }

        }

        // GET: Blog/Create
        public IActionResult Create()
        {
            string userName = _userManager.GetUserId(HttpContext.User);

            if (!String.IsNullOrEmpty(userName))
            {
                // Get the Current User Id and send it to page for display
                ViewBag.AuthorId = _userManager.GetUserId(HttpContext.User);
                return View();
            }
            else
            {
                return NotFound($"Cannot find user {userName.ToString()}");
            }
        }

        // POST: Blog/Create
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Create([Bind("Id,Title,Body,Created,AuthorId")] Blog blog)
        {
            ApplicationUser author = await _userManager.FindByIdAsync(blog.AuthorId);
            if (ModelState.IsValid)
            {
                blog.Created = DateTime.Now;
                await _context.AddAsync(blog);
                int saveBlog = await _context.SaveChangesAsync();
                if (saveBlog > 0)
                {

                    ConfirmMessage sendMsg = new ConfirmMessage()
                    {
                        Email = author.Email,
                        BlogTitle = blog.Title,
                        ConfirmText = $"<html><body><h3> Dear {author.FirstName} {author.LastName},"
                        + $"</h2><h3> Your Blog <b><u>{blog.Title}</u></b> was created on {blog.Created}.</h3>"
                        + $"</h2><h3>Kind Regards,</h3>"
                        + $"<h3>Bravo Blog Team</h3></body></html>"
                    };

                    TempData["EmailStatus"] = await SendConfirmation(sendMsg);
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["AuthorId"] = new SelectList(_context.Set<ApplicationUser>(), "Id", "Id", blog.AuthorId);
            return View(blog);
        }

        private async Task<string> SendConfirmation(ConfirmMessage  sendMsg)
        {
            string funcUrl = _requestSettings.MyAzureFunctionUrl;
            //string funcUrl = _requestSettings.MyLocalFunctionUrl;
            string statusMsg = "";

            using (var myrequest = new HttpRequestMessage(HttpMethod.Post, funcUrl))
            {
                var json = JsonConvert.SerializeObject(sendMsg);
                var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
                myrequest.Content = httpContent;

                using (var newresponse = await httpClient.SendAsync(myrequest).ConfigureAwait(false))
                {
                    if (newresponse.IsSuccessStatusCode)
                    {
                        statusMsg = "Your blog has been created. A confirmation has been sent by email.";
                    }
                    else
                        statusMsg = "Failed to create your blog!";
                }
            }

            return statusMsg;
        }

        // GET: Blog/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return Conflict("The blog id was null! Cannot proceed.");
            }

            var blog = await _context.Blogs.FindAsync(id);
            if (await UserAccessAuthorization(blog.AuthorId))
            {

                if (blog == null)
                {
                    return NotFound();
                }

                return View(blog);
            }
            else
            { 
                string ticketId = Guid.NewGuid().ToString();
                return Unauthorized($"Access Denied! A security Incident Ticket {ticketId} created!");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Blog blog)
        {
            if (id != blog.Id)
            {
                return BadRequest("Blog id is null!");
            }
            else
            {
                blog.Author = _context.Blogs.Find(id).Author;
                blog.AuthorId = _context.Blogs.Find(id).AuthorId;
            }

            if (await UserAccessAuthorization(blog.AuthorId))
            {
                if (ModelState.IsValid)
                { 
                    try
                    {
                        _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
                        _context.Update(blog);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!BlogExists(blog.Id))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
                    return RedirectToAction(nameof(Index));
                }
            }
            else
            {
                string ticketId = Guid.NewGuid().ToString();
                return Unauthorized($"Access Denied! A security Incident Ticket {ticketId} created!");
            }

            ViewData["AuthorId"] = new SelectList(_context.Set<ApplicationUser>(), "Id", "Id", blog.AuthorId);
            return View(blog);
        }

        // GET: Blog/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return BadRequest("Blog id is null!");
            }
            var blog = await _context.Blogs.Include(b => b.Author)
                        .FirstOrDefaultAsync(m => m.Id == id);

            if (await UserAccessAuthorization(blog.AuthorId))
            {
                if (blog == null)
                {
                    return NotFound();
                }

                return View(blog);
            }
            else
            {
                string ticketId = Guid.NewGuid().ToString();
                return Unauthorized($"Access Denied! A security Incident Ticket {ticketId} created!");
            }
        }

        // POST: Blog/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            var blog = await _context.Blogs.FindAsync(id);
            if (await UserAccessAuthorization(blog.AuthorId))
            {
                _context.Blogs.Remove(blog);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                string ticketId = Guid.NewGuid().ToString();
                return Unauthorized($"Access Denied! A security Incident Ticket {ticketId} created!");
            }
        }

        private bool BlogExists(int id)
        {
            return _context.Blogs.Any(e => e.Id == id);
        }

        private async Task<bool> UserAccessAuthorization(string authorId)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            if (user.Id == authorId)
            { return true; }
            else
            { return false; }  
        }
    }
}
