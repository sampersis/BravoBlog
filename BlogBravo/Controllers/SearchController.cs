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
            //searchString.BlogList = (from b in _context.Blogs
            //                         select b).ToList();
            //searchString.PostList = (from p in _context.Posts
            //                         select p).Include(p=> p.Blog).ToList();
            //searchString.TagList = (from t in _context.Tags
            //                        select t).Include(t=> t.Post).ToList();

            //foreach (Tag tag in search.TagList)
            //{
            //    foreach (Post post in tag.Post)
            //    {
            //        post.Blog = _context.Blogs.Find(post.Id);
            //        Post Post = tag.Post.FirstOrDefault(p=> p.Id == post.Id);
            //        if(Post != null)
            //        {
            //            Post = post;
            //        }
            //        else
            //        {
            //            return Problem("Controller: Search Action: Index: something went wrong when trying to find Blog of a Post " + post.ToString());
            //        }
            //    }
            //}

            ViewBag.Query = query;
            TempData["search"] = "search";
            if (!String.IsNullOrEmpty(query))
            {
                search.BlogList = search.BlogList.Where(b => b.Title.ToUpper().Contains(query.ToUpper())).ToList();
                search.PostList = search.PostList.Where(p => p.Title.ToUpper().Contains(query.ToUpper())).ToList();
                search.TagList = search.TagList.Where(t => t.Name.ToUpper().Contains(query.ToUpper())).ToList();
            }

            return View(search);
        }

        public ActionResult ViewBlog(int? blogId, string query)
        {
            SearchVM search = new SearchVM();

            if (blogId != null)
            {
                Blog blog = _context.Blogs.Find(blogId);
                ViewBag.Blog = blog;
                search.PostList = new List<Post>();
                search.PostList = _context.Posts.Where(p => p.BlogId == blog.Id).OrderByDescending(p => p.Created).ToList();
                search.PostList = search.PostList.Where(p => p.Title.ToUpper().Contains(query.ToUpper())).ToList();

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

        public ActionResult ViewPostById(int? postId)
        {
            return View();
        }


    }
}
