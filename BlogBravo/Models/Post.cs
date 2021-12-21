using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BlogBravo.Models
{
    public class Post
    {
        public int Id { get; set; }
        [Required]
        [StringLength(50)]
        public string Title { get; set; }
        [Required]
        public string Body { get; set; }
        public DateTime Created { get; set; }
        public int Views { get; set; }
        [Required]
        public virtual Blog Blog { get; set; }
        [Required]
        public int BlogId { get; set; }
        public virtual ICollection<Comment> Comment { get; set; }

        [Required]
        public virtual ICollection<Tag> Tag { get; set; }
    }
}
