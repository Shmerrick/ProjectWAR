using ApocalypseAPI.Shared;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace ApocalypseAPI.Test
{
    public class LoginManagerTests
    {
        [Fact]
        public void NonNumberNonValidAuthKey()
        {
            var manager = new LoginManager(new DbConnectionService(""));
            Assert.False(manager.ValidAuthKey("sd"));
        }
        [Fact]
        public void NonDivisibleNumberFails()
        {
            var manager = new LoginManager(new DbConnectionService(""));
            Assert.False(manager.ValidAuthKey("3"));
            Assert.False(manager.ValidAuthKey("-3"));
            Assert.False(manager.ValidAuthKey("11"));
            Assert.False(manager.ValidAuthKey("23"));
            Assert.False(manager.ValidAuthKey("34342211"));
        }
        [Fact]
        public void DivisibleNumberPasses()
        {
            var manager = new LoginManager(new DbConnectionService(""));
            Assert.True(manager.ValidAuthKey("7"));
            Assert.True(manager.ValidAuthKey("70"));
            Assert.True(manager.ValidAuthKey("217"));
            Assert.True(manager.ValidAuthKey("5764801"));
        }
    }
}
