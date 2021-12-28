using BlogBravo.Areas.Identity.Pages.Account;
using BlogBravo.Data;
using BlogBravo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogBravo.Controllers
{
    [Authorize(Roles = "author")]
    public class PostController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        public PostController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        // GET: Post
        public async Task<IActionResult> Index(int? blogId)
        {
            if (blogId == 0)
            {
                var applicationDbContext = _context.Posts;
                return View(await applicationDbContext.ToListAsync());
            }
            else
            {
                ViewBag.BlogId = blogId;
                var applicationDbContext = _context.Posts.Include(p => p.Blog);
                return View(await applicationDbContext.ToListAsync());
            }
        }


        // GET: Post/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Blog)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // GET: Post/Create
        [Route("/Post/Create/{id}")]
        public IActionResult Create(int blogId)
        {
            ViewBag.CreateBlogId = blogId;
            //ViewData["BlogId"] = new SelectList(_context.Blogs, "Id", "Body");
            return View();
        }

        // POST: Post/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePost(Post post)
        {
            Tag tag = new Tag();
            string[] tags = new string[5]; // Max 5 tags
            string none = "none";
            bool tagAlreadyExist = false;

            if (ModelState.IsValid)
            {
                tags[0] = Request.Form["first-tag"];
                tags[1] = Request.Form["second-tag"];
                tags[2] = Request.Form["third-tag"];
                tags[3] = Request.Form["fourth-tag"];
                tags[4] = Request.Form["fifth-tag"];

                foreach (string tagStr in tags)
                {
                    if (tagStr != none)
                    {
                        // Check for duplicate Tags
                        List<Tag> tagList = _context.Tags.Where(t => t.Name == tagStr).ToList();

                        foreach (Tag tagItem in tagList)
                        {
                            if (String.Equals(tagItem.Name.ToUpper(),tagStr.ToUpper()))
                            {
                                tagAlreadyExist = true;
                                tag = tagItem;
                                break;
                            }
                            else
                            {
                                tagAlreadyExist = false;
                            }
                        }


                        if (!tagAlreadyExist)
                        {
                            post.Tag.Add(new Tag() { Name = tagStr });
                        }
                        else
                        {
                            post.Tag.Add(tag);
                        }
                    }
                }

                _context.Add(post);

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["BlogId"] = new SelectList(_context.Blogs, "Id", "Body", post.BlogId);
            return View(post);
        }

        // GET: Post/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }
            ViewData["BlogId"] = new SelectList(_context.Blogs, "Id", "Body", post.BlogId);
            return View(post);
        }

        // POST: Post/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Body,Created,Views,BlogId")] Post post)
        {
            if (id != post.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(post);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(post.Id))
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
            ViewData["BlogId"] = new SelectList(_context.Blogs, "Id", "Body", post.BlogId);
            return View(post);
        }

        // GET: Post/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Blog)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // POST: Post/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PostExists(int id)
        {
            return _context.Posts.Any(e => e.Id == id);
        }
    }
}
