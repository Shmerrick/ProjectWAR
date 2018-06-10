using System;
using System.Linq;
using System.Threading.Tasks;

namespace ApocalypseAPI.Shared
{


    public class TimeToken
    {
        public String Token { get; set; }
        public DateTime Timestamp { get; set; }
        public String UserName { get; set; }

        public TimeToken(string userName, string token, DateTime timestamp)
        {
            UserName = userName;
            Token = token;
            Timestamp = timestamp;
        }
    }

}