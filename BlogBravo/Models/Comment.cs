using BlogBravo.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BlogBravo.Models
{
    public class Comment
    {
        public int Id { get; set; }
        [Required, StringLength(512)]
        public string Body { get; set; }
        public DateTime Created { get; set; }

        [Required]
        public virtual Post Post { get; set; }
        public int PostId { get; set; }

        [Required]
        public virtual ApplicationUser Author { get; set; }
    }
}
