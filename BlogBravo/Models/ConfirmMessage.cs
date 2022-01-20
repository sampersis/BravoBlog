using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogBravo.Models
{
    public class ConfirmMessage
    {
        public string Email { get; set; }
        public string BlogTitle { get; set; }
        public string ConfirmText { get; set; }
    }
}
