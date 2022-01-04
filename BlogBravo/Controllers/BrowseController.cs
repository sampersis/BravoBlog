using BlogBravo.Data;
using BlogBravo.Models;
using Microsoft.AspNetCore.Http;
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

        public BrowseController(ApplicationDbContext context)
        {
            _context = context;
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
            List<Post> Posts = _context.Posts.ToList();
            return View(Posts);
        }

        public ActionResult ViewTags()
        {
            List<Tag> Tags = _context.Tags.ToList();

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

        // ------------------------------------ MIcrosoft Code --------------------------- //
        // GET: Browse/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Browse/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Browse/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Browse/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Browse/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Browse/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Browse/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
