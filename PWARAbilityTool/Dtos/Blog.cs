using System;

namespace PWARAbilityTool
{
    public class Blog
    {
        public int BlogId { get; set; }
        public DateTime BlogTimestamp { get; set; }
        public string BlogText { get; set; }
        public string BlogTitle { get; set; }
        public string BlogUrl { get; set; }

        public override string ToString()
        {
            return $"Id:{BlogId} Title:{BlogTitle}";
        }
    }
}
