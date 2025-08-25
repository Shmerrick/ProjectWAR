using AuthenticationServer.Server.Handler;
using Xunit;

namespace LauncherServer.Tests
{
    public class InputValidationTests
    {
        [Theory]
        [InlineData("validUser")]
        [InlineData("User123_")]
        public void ValidUsernames_Pass(string username)
        {
            Assert.True(InputValidator.IsValidUsername(username));
        }

        [Theory]
        [InlineData("tooooooooooooooooooolongusername")]
        [InlineData("bad!user")]
        [InlineData("")]
        public void InvalidUsernames_Fail(string username)
        {
            Assert.False(InputValidator.IsValidUsername(username));
        }

        [Theory]
        [InlineData("StrongPass1!")]
        [InlineData("Another$Pass123")]
        public void ValidPasswords_Pass(string password)
        {
            Assert.True(InputValidator.IsValidPassword(password));
        }

        [Theory]
        [InlineData("short")]
        [InlineData("contains space")]
        [InlineData("bad\npass")]
        public void InvalidPasswords_Fail(string password)
        {
            Assert.False(InputValidator.IsValidPassword(password));
        }

        [Theory]
        [InlineData("test@example.com")]
        [InlineData("user.name+tag@domain.co")]
        public void ValidEmails_Pass(string email)
        {
            Assert.True(InputValidator.IsValidEmail(email));
        }

        [Theory]
        [InlineData("invalid-email")]
        [InlineData("user@")]
        [InlineData("")]
        public void InvalidEmails_Fail(string email)
        {
            Assert.False(InputValidator.IsValidEmail(email));
        }
    }
}
