using System.Text.RegularExpressions;

namespace AuthenticationServer.Server.Handler
{
    public static class InputValidator
    {
        private static readonly Regex UsernameRegex = new("^[A-Za-z0-9_]{3,20}$", RegexOptions.Compiled);
        private static readonly Regex PasswordRegex = new("^[\x21-\x7E]{6,50}$", RegexOptions.Compiled);
        private static readonly Regex EmailRegex = new(@"^(([^<>()[\]\\.,;:\s@""']+(\.[^<>()[\]\\.,;:\s@""']+)*)|("".+""))@((\[[0-9]{1,3}(\.[0-9]{1,3}){3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$", RegexOptions.Compiled);

        public static bool IsValidUsername(string username)
        {
            return !string.IsNullOrWhiteSpace(username) && UsernameRegex.IsMatch(username);
        }

        public static bool IsValidPassword(string password)
        {
            return !string.IsNullOrWhiteSpace(password) && PasswordRegex.IsMatch(password);
        }

        public static bool IsValidEmail(string email)
        {
            return !string.IsNullOrWhiteSpace(email) && EmailRegex.IsMatch(email);
        }
    }
}
