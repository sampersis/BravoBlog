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
                return NotFound();
            }

            var blog = await _context.Blogs
                .Include(b => b.Author)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (blog == null)
            {
                return NotFound();
            }

            return View(blog);
        }

        // GET: Blog/Create
        public IActionResult Create()
        {
            string userName = _userManager.GetUserId(HttpContext.User);

            if (!String.IsNullOrEmpty(userName))
            {
                // Get the Current User Id and send it to page for display
                ViewBag.AuthorId = _userManager.GetUserId(HttpContext.User);
                //ViewData["AuthorId"] = new SelectList(_context.Set<ApplicationUser>(), "Id", "Id");
                return View();
            }
            else
            {
                return NotFound();
            }
        }

        // POST: Blog/Create
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Create([Bind("Id,Title,Body,Created,AuthorId")] Blog blog)
        {
            ApplicationUser author = await _userManager.GetUserAsync(HttpContext.User);
            // author.Email = _userManager.GetUserName(HttpContext.User);
            if (ModelState.IsValid)
            {
                blog.Created = DateTime.Now;
                blog.AuthorId = author.Id;
                blog.Author = author;
                _context.Add(blog);
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
                        statusMsg = "Your blog has been created. Aconfirmation has been sent by email.";
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
                return NotFound();
            }

            var blog = await _context.Blogs.FindAsync(id);
            if (blog == null)
            {
                return NotFound();
            }

            //ViewBag.AuthorId = _userManager.GetUserId(HttpContext.User);
            //ViewData["AuthorId"] = new SelectList(_context.Set<ApplicationUser>(), "Id", "Id", blog.AuthorId);
            return View(blog);
        }

        // POST: Blog/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Body,Created,AuthorId")] Blog blog)
        {
            ApplicationUser author = await _userManager.GetUserAsync(HttpContext.User);

            if (id != blog.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    blog.AuthorId = author.Id;
                    blog.Author = author;
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
            ViewData["AuthorId"] = new SelectList(_context.Set<ApplicationUser>(), "Id", "Id", blog.AuthorId);
            return View(blog);
        }

        // GET: Blog/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blog = await _context.Blogs
                .Include(b => b.Author)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (blog == null)
            {
                return NotFound();
            }

            return View(blog);
        }

        // POST: Blog/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var blog = await _context.Blogs.FindAsync(id);
            _context.Blogs.Remove(blog);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BlogExists(int id)
        {
            return _context.Blogs.Any(e => e.Id == id);
        }
    }
}
