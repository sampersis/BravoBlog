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


        public ActionResult Index(string query)
        {
            var ApplicationDbContext = _context;
            SearchVM search = new SearchVM();

            search.BlogList = _context.Blogs.ToList();
            search.PostList = _context.Posts.Include(p=>p.Blog).ToList();
            search.TagList = _context.Tags.Include(t => t.Post).ToList();

            ViewBag.Query = query;
            TempData["searchpath"] = "search";
            TempData["query"] = query;
            if (!String.IsNullOrEmpty(query))
            {
                search.BlogList = search.BlogList.Where(b => b.Title.ToUpper().Contains(query.ToUpper())).ToList();
                search.PostList = search.PostList.Where(p => p.Title.ToUpper().Contains(query.ToUpper())).ToList();
                search.TagList = search.TagList.Where(t => t.Name.ToUpper().Contains(query.ToUpper())).ToList();
                return View(search);
            }
            else
            { 
                return Problem("The query string was empty!"); 
            }


        }

        public ActionResult ViewBlog(int? blogId, string query)
        {
            SearchVM search = new SearchVM();

            if (blogId != null)
            {
                Blog blog = _context.Blogs.Find(blogId);
                ViewBag.Blog = blog;
                search.PostList = new List<Post>();

               Post postWithTheLatestDate = _context.Posts.Where(p => p.BlogId == blog.Id).OrderByDescending(p => p.Created).FirstOrDefault();
               var PostList = _context.Posts.Where(p => p.BlogId == blog.Id).OrderByDescending(p => p.Created).AsEnumerable().GroupBy(p =>p.Created).Select(g=>g.First());

                foreach (var post in PostList)
                {
                    if(post.Created.ToString("MM").Equals(postWithTheLatestDate.Created.ToString("MM")))
                    { 
                        search.PostList.Add(post); 
                    }
                    
                }

                return View(search.PostList);
            }
            else
            {
                return Problem("Could not retrieve the posts with specific dates");
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

                return View(search.PostList);
            }
            else
            {
                return Problem("Could not retrieve the posts with specific dates");
            }
        }

        public ActionResult ViewPostById(int? postId)
        {
            return View();
        }


    }
}
