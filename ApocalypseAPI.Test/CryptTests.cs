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
            var enc = Shared.Cryptography.EncryptString("XYZ", "1234123123dfsasdfasdf");
            var result = Shared.Cryptography.DecryptString(enc, "1234123123dfsasdfasdf");

            Assert.True(result == "XYZ");

        }
        [Fact]
        public void FullTripCryptoEmptyString()
        {
            var enc = Shared.Cryptography.EncryptString("", "1234123123dfsasdfasdf");
            var result = Shared.Cryptography.DecryptString(enc, "1234123123dfsasdfasdf");

            Assert.True(result == "");

        }
        [Fact]
        public void FullTripCryptoNcalls()
        {
            var enc = Shared.Cryptography.EncryptString("", "1234123123dfsasdfasdf");
            var enc1 = Shared.Cryptography.EncryptString("asdf", "1234123123dfsasdfasdf");
            var enc2 = Shared.Cryptography.EncryptString("asfd", "1234123123dfsasdfasdf");

            var result = Shared.Cryptography.DecryptString(enc2, "1234123123dfsasdfasdf");

            Assert.True(result == "asfd");

        }
    }
    }
