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
            ApplicationUser author = await _userManager.GetUserAsync(HttpContext.User);
            List<Post> userPosts = new List<Post>();


            if (blogId == null)
            {
                ViewBag.UserName = author.FirstName+"'s";
                var userBlogs = _context.Blogs.Where(b => b.Author == author);
                ViewBag.UserBlogs = userBlogs;

                foreach (var userBlog in userBlogs)
                {
                    List<Post> tempPost = _context.Posts.Where(p => p.BlogId == userBlog.Id).ToList();
                    userPosts.AddRange(tempPost);
                }
                return View(userPosts);
            }
            else
            {
                ViewBag.BlogId = blogId;
                var blog = _context.Blogs.Find(blogId);
                ViewBag.BlogName = blog.Title;
                var blogPosts = _context.Posts.Where(p => p.BlogId == blogId).ToList();

                return View(blogPosts);
            }
        }
        
        // GET: Post/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Post post = await _context.Posts.Include(p=>p.Comment).FirstOrDefaultAsync(p=>p.Id == id);
            post = await _context.Posts.Include(p => p.Tag).FirstOrDefaultAsync(p => p.Id == id);
            var comments =  _context.Comments.Include(c => c.Author).Where(c => c.PostId == id);

            ViewBag.Comments = comments;

            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            //string blogId = Request.Form["blog-id"];
            string commentUserName = Request.Form["post-comment-user"];

            var post = await _context.Posts.Include(p => p.Comment).FirstOrDefaultAsync(p => p.Id == id);


            if (post == null)
            {
                return NotFound();
            }
            else
            {
                post.Comment.Add(new Comment
                {
                    Created = DateTime.Now,
                    Body = Request.Form["post-comment-body"],
                    Author = await _userManager.GetUserAsync(HttpContext.User)
                });

                _context.Update(post);
                await _context.SaveChangesAsync();
            }

            ViewBag.Comments = post.Comment;

            return Redirect("/Post/Details/"+id);
        }

        // GET: Post/Create
        [Route("/Post/Create/{id}")]
        public IActionResult Create(int blogId)
        {

            string path = HttpContext.Request.Path.ToString();
            string[] pathComponent = path.Split('/');

            ViewBag.CreateBlogId = pathComponent[pathComponent.Length-1];
            // ViewData["BlogId"] = new SelectList(_context.Blogs, "Id", "Body"); // This creates a list of BlogIds which is not required
            return View();
        }

        // POST: Post/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost, ActionName("Create")]
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

                        if (tagList.Count > 0)
                        {
                            foreach (Tag tagItem in tagList)
                            {
                                if (String.Equals(tagItem.Name.ToUpper(), tagStr.ToUpper()))
                                {
                                    tagAlreadyExist = true;
                                    tag = tagItem;
                                    break;
                                }

                            }
                        }
                        else
                        {
                            tagAlreadyExist = false;
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
                post.Created = DateTime.Now; // Post creation date
                //tagAlreadyExist = false;

                _context.Posts.Add(post);

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            // ViewData["BlogId"] = new SelectList(_context.Blogs, "Id", "Body", post.BlogId);
            // return View(post);
            return RedirectToAction(nameof(Index));
        }

        // GET: Post/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var post = await _context.Posts.Include(p=> p.Tag).FirstOrDefaultAsync(p=> p.Id == id);

            if (post == null)
            {
                return NotFound();
            }
            //ViewData["BlogId"] = new SelectList(_context.Blogs, "Id", "Body", post.BlogId);
            return View(post);
        }

        // POST: Post/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Body,Created,Views,Tag,BlogId")] Post post)
        {
            Tag tag = new Tag();
            string postTitle = post.Title;
            string postBody = post.Body;
            string[] tags = new string[5]; // Max 5 tags
            string none = "none";
            bool tagAlreadyExist = false;

            if (id != post.Id)
            {
                return NotFound();
            }
            else
            {
                // remove all tags from the  post
                post = await _context.Posts.Include(p => p.Tag).FirstOrDefaultAsync(p => p.Id == id);
                post.Tag.Clear();
            }

            if (ModelState.IsValid)
            {
                tags[0] = Request.Form["first-tag"];
                tags[1] = Request.Form["second-tag"];
                tags[2] = Request.Form["third-tag"];
                tags[3] = Request.Form["fourth-tag"];
                tags[4] = Request.Form["fifth-tag"];

                // delete all the tags from the post in the database

                foreach (string tagStr in tags)
                {
                    if (tagStr != none)
                    {
                        // Check for duplicate Tags
                        List<Tag> tagList = _context.Tags.Where(t => t.Name == tagStr).ToList();

                        if (tagList.Count > 0)
                        {
                            foreach (Tag tagItem in tagList)
                            {
                                if (String.Equals(tagItem.Name.ToUpper(), tagStr.ToUpper()))
                                {
                                    tagAlreadyExist = true;
                                    tag = tagItem;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            tagAlreadyExist = false;
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

                post.Title = postTitle;
                post.Body = postBody;

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
