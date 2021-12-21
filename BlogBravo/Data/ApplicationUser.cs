using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using BlogBravo.Models;
using Microsoft.AspNetCore.Identity;

namespace BlogBravo.Data
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(32)]
        [DisplayName("First Name")]
        public string FirstName { get; set; }
        [Required]
        [StringLength(32)]
        [DisplayName("Last Name")]
        public string LastName { get; set; }

        public virtual ICollection<Comment> Comments { get; set; }

        public ICollection<Blog> Blogs { get; set; }
    }
}
