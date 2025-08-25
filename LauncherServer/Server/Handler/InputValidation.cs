using System.Text.RegularExpressions;

namespace LauncherServer.Server.Handler
{
    public static class InputValidator
    {
        // Explicit type instantiation is used to remain compatible with older
        // language versions. The previous implementation relied on C# 9's
        // target-typed `new()` expressions which are unavailable when the
        // project is compiled with the current language version setting (7.3).
        private static readonly Regex UsernameRegex = new Regex("^[A-Za-z0-9_]{3,20}$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex PasswordRegex = new Regex("^[\x21-\x7E]{6,50}$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex EmailRegex = new Regex(@"^(([^<>()[\]\\.,;:\s@""']+(\.[^<>()[\]\\.,;:\s@""']+)*)|("".+""))@((\[[0-9]{1,3}(\.[0-9]{1,3}){3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

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
