using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApocalypseAPI.Models
{
    public class Blog
    {
        public int BlogId { get; set; }
        public DateTime BlogTimestamp { get; set; }
        public string BlogText { get; set; }
        public string BlogUrl { get; set; }
    }
}
