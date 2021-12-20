using BlogBravo.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BlogBravo.Models
{
    public class Blog
    {
        public int Id { get; set; }
        [Required]
        [StringLength(50)]
        public string Title { get; set; }
        [Required]
        public string Body { get; set; }
        [Required]
        public DateTime Created { get; set; }
        public virtual ICollection<Post> Post { get; set; }

        public virtual ApplicationUser Author { get; set; }
        public string AuthorId { get; set; }
    }
}
