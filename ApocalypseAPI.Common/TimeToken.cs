using System;

namespace ApocalypseAPI.Common
{


    public class TimeToken
    {
        public String Token { get; set; }
        public DateTime Timestamp { get; set; }
        public String UserName { get; set; }

        // Although we are using timestamp as part of the token, this is only in here for future purposes.
        // The intention would be to use the time to force relogin after N mins.
        public TimeToken(string userName, string token, DateTime timestamp)
        {
            UserName = userName;
            Token = token;
            Timestamp = timestamp;
        }
    }

}