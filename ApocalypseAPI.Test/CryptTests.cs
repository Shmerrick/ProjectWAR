using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace ApocalypseAPI.Test
{
    public class CryptTests
    {
        [Fact]
        public void FullTripCrypto()
        {
            var enc = Common.Cryptography.EncryptString("XYZ", "1234123123dfsasdfasdf");
            var result = Common.Cryptography.DecryptString(enc, "1234123123dfsasdfasdf");

            Assert.True(result == "XYZ");

        }
        [Fact]
        public void FullTripCryptoEmptyString()
        {
            var enc = Common.Cryptography.EncryptString("", "1234123123dfsasdfasdf");
            var result = Common.Cryptography.DecryptString(enc, "1234123123dfsasdfasdf");

            Assert.True(result == "");

        }
        [Fact]
        public void FullTripCryptoNcalls()
        {
            var enc = Common.Cryptography.EncryptString("", "1234123123dfsasdfasdf");
            var enc1 = Common.Cryptography.EncryptString("asdf", "1234123123dfsasdfasdf");
            var enc2 = Common.Cryptography.EncryptString("asfd", "1234123123dfsasdfasdf");

            var result = Common.Cryptography.DecryptString(enc2, "1234123123dfsasdfasdf");

            Assert.True(result == "asfd");

        }
    }
    }
