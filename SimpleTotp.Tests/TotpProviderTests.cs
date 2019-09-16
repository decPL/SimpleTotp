// ReSharper disable StringLiteralTypo

using System;
using Xunit;

namespace SimpleTotp.Tests
{
    public class TotpProviderTests
    {
        [Theory]
        [InlineData("12345", "GEZDGNBV")]
        [InlineData("123456", "GEZDGNBVGY")]
        [InlineData("1234567", "GEZDGNBVGY3Q")]
        [InlineData("12345678", "GEZDGNBVGY3TQ")]
        [InlineData("123456789", "GEZDGNBVGY3TQOI")]
        [InlineData("1234567890", "GEZDGNBVGY3TQOJQ")]
        public void Given_ATotpProvider_When_GetBase32EncodedSecretKeyIsCalled_Then_EncodedSecretKeyIsReturned(
            String secretKey,
            String expected)
        {
            var provider = new TotpProvider();

            var result = provider.GetBase32EncodedSecretKey(secretKey);

            Assert.Equal(expected, result);
        }
    }
}