using BlogBravo.Data;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogBravo.Models
{
    public class BlogData
    {
        public BlogData() {
            this.Admin = new ApplicationUser();
            this.Users = new List<ApplicationUser>();
            this.Blogs = new List<Blog>();
            this.Posts = new List<Post>();
            this.Comments = new List<Comment>();
            this.Tags = new List<Tag>();
        }

        public ApplicationUser Admin { get; set; }
        
        public List <ApplicationUser> Users { get; set; }
        public List <IdentityRole> Roles { get; set; }
        public List<Blog> Blogs { get; set; }
        public List<Post> Posts { get; set; }
        public List<Comment> Comments { get; set; }
        public List<Tag> Tags { get; set; }
    }
}
