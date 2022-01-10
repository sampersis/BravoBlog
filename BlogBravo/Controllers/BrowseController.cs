using BlogBravo.Data;
using BlogBravo.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogBravo.Controllers
{
    public class BrowseController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public BrowseController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Browse
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ViewBlogs()
        {
            List<Blog> Blogs = _context.Blogs.ToList();

            return View(Blogs);
        }

        public ActionResult ViewPosts()
        {
            List<Post> Posts = _context.Posts.OrderBy(p => p.Title).OrderBy(p=>p.BlogId).ToList();
            return View(Posts);
        }

        public ActionResult ViewTags()
        {
            List<Tag> Tags = _context.Tags.Include(t => t.Post).ToList();
            Tags = Tags.OrderByDescending(t => t.Post.Count).ToList();

            return View(Tags);
        }

        public ActionResult ViewPostsWithTag()
        {
            string path = HttpContext.Request.Path;
            string[] pathComponents = path.Split('/');
            int tagId = Convert.ToInt32(pathComponents[pathComponents.Length - 1]);
            List<Post> postsThatUsesTheTag = new List<Post>();

            if (tagId > 0)
            {
                List<Post> Posts = _context.Posts.Include(p => p.Tag).ToList();

                if (Posts.Count != 0 )
                {
                    foreach(Post post in Posts)
                    {
                        if (post.Tag.Count > 0)
                        {
                            foreach (Tag tag in post.Tag)
                            {
                                if (tag.Id == tagId)
                                {
                                    postsThatUsesTheTag.Add(post);
                                }
                            }
                        }
                    }
                }

                return View(postsThatUsesTheTag);
            }

            return View();
        }

        public ActionResult ViewPost()
        {
            string path = HttpContext.Request.Path;
            string[] pathComponents = path.Split('/');
            int postId = Convert.ToInt32(pathComponents[pathComponents.Length - 1]);

            if (postId > 0)
            {
                var Post = _context.Posts.Include(p => p.Comment).FirstOrDefault(p => p.Id == postId);
                Post = _context.Posts.Include(p => p.Tag).FirstOrDefault(p => p.Id == postId);
                var comments = _context.Comments.Include(c => c.Author).Where(c => c.PostId == postId);
                ViewBag.Comments = comments;

                return View(Post);
            }

            return View();
        }

        public ActionResult Views()
        {
            string path = HttpContext.Request.Path;
            string[] pathComponents = path.Split('/');
            int BlogId = Convert.ToInt32(pathComponents[pathComponents.Length - 1]);

            if (BlogId > 0)
            {
                Blog blog = _context.Blogs.Find(BlogId);
                var Posts = _context.Posts.Include(p => p.Comment).Where(p => p.BlogId == BlogId).OrderByDescending(p => p.Created);
                foreach (var post in Posts)
                {
                    if (post.Comment.Count != 0)
                    {
                        var comments = _context.Comments.Include(c => c.Author).Where(c => c.PostId == post.Id).ToList();
                        post.Comment = comments;
                    }
                } 

                ViewBag.BlogPosts = Posts;

                string[] postDates = new string [Posts.Count()];

                int i = 0;
                foreach(var post in Posts)
                {
                    if (post != null)
                    {
                        postDates[i] = post.Created.ToString("yyyy-MMMM-dd");
                        i++;
                    }
                }

                Array.Sort(postDates);

                List<string> years = new List<string>();
                List<string> months = new List<string>();
                string[] componentsOfpostDates = new string[3];

                foreach (string postDate in postDates)
                {
                    componentsOfpostDates = postDate.Split('-');
                    years.Add(componentsOfpostDates[0]);
                }

                ViewBag.postYears = years = years.OrderBy(y => y.ElementAt(0)).Distinct().ToList();
                foreach(var year in years)
                {
                    for (i = 0; i < postDates.Length; i++)
                    {
                        componentsOfpostDates = postDates[i].Split('-');
                        if (String.Equals(year, componentsOfpostDates[0]))
                        {
                            months.Add(componentsOfpostDates[1]);
                        }
                    }

                    TempData[year] = months.OrderBy(m => m.ElementAt(0)).Distinct().ToList();
                    months.Clear();
                }
                
                return View(blog);
            }

            return View();
        }

            public ActionResult ViewBlogPost()
        {
             string path = HttpContext.Request.Path;
            string[] pathComponents = path.Split('/');
            int BlogId = Convert.ToInt32(pathComponents[pathComponents.Length - 1]);

            if (BlogId > 0)
            {
                var Post = _context.Posts.Where(p => p.BlogId == BlogId);
                var blog = _context.Blogs.Find(BlogId);
                ViewBag.BlogName = blog.Title;  
                return View(Post);
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

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

            return Redirect("/Browse/Index/");
        }
    }
}
