using BlogBravo.Data;
using BlogBravo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogBravo.Controllers
{
    public class SearchController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public SearchController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }


        public async Task<IActionResult> Index(string query)
        {
            var ApplicationDbContext = _context;
            SearchVM searchString = new SearchVM();
            searchString.BlogList = (from b in _context.Blogs
                                     select b).ToList();
            searchString.PostList = (from p in _context.Posts
                                     select p).ToList();
            searchString.TagList = (from t in _context.Tags
                                    select t).ToList();

            TempData["search"] = "search";
            if (!String.IsNullOrEmpty(query))
            {
                searchString.BlogList = searchString.BlogList.Where(b => b.Title.ToUpper().Contains(query.ToUpper())).ToList();
                searchString.PostList = searchString.PostList.Where(p => p.Title.ToUpper().Contains(query.ToUpper())).ToList();
                searchString.TagList = searchString.TagList.Where(t => t.Name.ToUpper().Contains(query.ToUpper())).ToList();
            }


            return View(searchString);
        }

        public ActionResult ViewBlog(int? blogId)
        {
            if (blogId != null)
            {
                Blog blog = _context.Blogs.Find(blogId);
                return View(blog);
            }
            else
            {
                return Problem("The blog is was null!");
            }
        }

        public ActionResult ViewPost(int? postId)
        {
            SearchVM search = new SearchVM();

            if (postId != null)
            {
                Post post = _context.Posts.Find(postId);
                Blog blog = _context.Blogs.Find(post.BlogId);
                ViewBag.Blog = blog;
                search.PostList = new List<Post>();
                search.PostList = _context.Posts.
                                     Where(p => p.BlogId == blog.Id)
                                    .ToList();

                search.PostList = search.PostList.Where(p => p.Created.ToString("MM").Equals(post.Created.ToString("MM"))).ToList();
                foreach(Post p in search.PostList)
                {
                    p.Created = Convert.ToDateTime(p.Created.ToString("yyyy-MM-dd"));
                }

                return View(search.PostList);
            }
            else
            {
                return Problem("Could not retrieve the posts with specific dates");
            }
        }
    }
}
