using ApocalypseAPI.Shared;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace ApocalypseAPI.Test
{
    public class TokenManagerTests
    {
        public readonly TimeToken token1 = new TimeToken("rez", "&*&^*&^121212", DateTime.Now);
        public readonly TimeToken token2 = new TimeToken("joe", "&*&12121121212", DateTime.Now);
        public readonly TimeToken token3 = new TimeToken("sam", "&*&^3232121212", DateTime.Now);
        public readonly TimeToken token4 = new TimeToken("jim", "&123555212", DateTime.Now);

        public TokenManagerTests()
        {
           
        }

        [Fact]
        public void CreateTokenReturnsSomething()
        {
            var manager = new TimeTokenManager();
            var token = manager.CreateToken("soemthing", "mypassword");
            Assert.NotEmpty(token);
        }

        [Fact]
        public void CreateTokenReturns3elements()
        {
            var manager = new TimeTokenManager();
            var token = manager.CreateToken("soemthing", "mypassword");
            Assert.True(token.Split('|').Length == 3);
        }

        [Fact]
        public void RemoveTokenByNameFromFront()
        {
            var manager = new TimeTokenManager();
            manager.TokenList = new List<TimeToken>();
            

            manager.TokenList.Add(token1);
            manager.TokenList.Add(token2);
            manager.TokenList.Add(token3);

            Assert.True(manager.TokenList.Count == 3);

            manager.RemoveToken("rez");

            Assert.True(manager.TokenList.Count == 2);

            Assert.True(manager.TokenList[0].UserName == "joe");
            Assert.True(manager.TokenList[1].UserName == "sam");

            manager.RemoveToken("joe");
            Assert.True(manager.TokenList.Count == 1);
            Assert.True(manager.TokenList[0].UserName == "sam");

        }

        [Fact]
        public void RemoveTokenByNameFromMid()
        {
            var manager = new TimeTokenManager();
            manager.TokenList = new List<TimeToken>();
           
            manager.TokenList.Add(token1);
            manager.TokenList.Add(token2);
            manager.TokenList.Add(token3);

            Assert.True(manager.TokenList.Count == 3);

            manager.RemoveToken("joe");

            Assert.True(manager.TokenList.Count == 2);

            Assert.True(manager.TokenList[0].UserName == "rez");
            Assert.True(manager.TokenList[1].UserName == "sam");


        }

        [Fact]
        public void AddTokenToListWorks()
        {
            var manager = new TimeTokenManager();
            manager.TokenList = new List<TimeToken>();
            
            Assert.True(manager.TokenList.Count == 0);

            manager.TokenList.Add(token1);
            manager.TokenList.Add(token2);
            manager.TokenList.Add(token3);

            Assert.True(manager.TokenList.Count == 3);

            manager.TokenList.Add(token4);

            Assert.True(manager.TokenList.Count == 4);

            Assert.True(manager.TokenList[0].UserName == "rez");
            Assert.True(manager.TokenList[3].UserName == "jim");


        }

        [Fact]
        public void DecodeDecryptStringReturnsValues()
        {
            var manager = new TimeTokenManager();
            var eeToken = manager.EncodeEncryptToken("X");
            var result = manager.DecodeDecryptToken(eeToken);
            Assert.NotEmpty(result);
            Assert.True(result == "X");
        }

        [Fact]
        public void EncodeEncryptStringReturnsValues()
        {
            var manager = new TimeTokenManager();
            var result = manager.EncodeEncryptToken("X");
            Assert.NotEmpty(result);
            var result1 = manager.EncodeEncryptToken("");
            Assert.NotEmpty(result1);
            var result2 = manager.EncodeEncryptToken("983279123skldjflskdj");
            Assert.NotEmpty(result2);
        }

        [Fact]
        public void EncodeEncryptStringChange()
        {
            var manager = new TimeTokenManager();
            var result = manager.EncodeEncryptToken("X");
            var result1 = manager.EncodeEncryptToken("");
            var result2 = manager.EncodeEncryptToken("983279123skldjflskdj");
            Assert.True(result != result1);
            Assert.True(result1 != result2);
            Assert.True(result != result2);
        }

        [Fact]
        public void BadlyFormedTokenFails()
        {
            var manager = new TimeTokenManager();
            var badlyFormedToken = manager.EncodeEncryptToken("983279123skldjflskdj");
            Assert.False(manager.WellFormedToken(badlyFormedToken));
        }

        [Fact]
        public void WellFormedTokenPasses()
        {
            var manager = new TimeTokenManager();
            var wellFormedToken = manager.EncodeEncryptToken("983279123skldjflskdj|X|Y");
            Assert.True(manager.WellFormedToken(wellFormedToken));
        }

        [Fact]
        public void ValidTokenNonExistingFails()
        {
            var manager = new TimeTokenManager();

            var token1 = manager.CreateToken("sam", "pwd1");
            
            var wellFormedToken = manager.EncodeEncryptToken(token1);

            Assert.False(manager.IsValidToken(wellFormedToken));
        }
        [Fact]
        public void ValidTokenExistingPasses()
        {
            var manager = new TimeTokenManager();

            var token1 = manager.CreateToken("sam", "pwd1");
            var wellFormedToken = manager.EncodeEncryptToken(token1);
            manager.AddToken(new TimeToken("sam", wellFormedToken, DateTime.Now));

            Assert.True(manager.IsValidToken(wellFormedToken));

            var token2 = manager.CreateToken("", "pwd1");
            var wellFormedToken2 = manager.EncodeEncryptToken(token1);
            manager.AddToken(new TimeToken("", wellFormedToken2, DateTime.Now));

            Assert.True(manager.IsValidToken(wellFormedToken2));

            var token3 = manager.CreateToken("Û", "pwd1");
            var wellFormedToken3 = manager.EncodeEncryptToken(token1);
            manager.AddToken(new TimeToken("Û", wellFormedToken3, DateTime.Now));

            Assert.True(manager.IsValidToken(wellFormedToken3));
        }
    }
}
