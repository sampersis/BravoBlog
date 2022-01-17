using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogBravo.Models
{
    public class SearchVM
    {
        public List<Blog> BlogList { get; set; }
        public List<Post> PostList { get; set; }
        public List<Tag> TagList { get; set; }
    }
}
