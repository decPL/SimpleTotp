// ReSharper disable StringLiteralTypo

using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace SimpleTotp.Tests
{
    [ExcludeFromCodeCoverage]
    public class TotpProviderTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t")]
        public void Given_AnEmptySecretKey_When_GetBase32EncodedSecretKeyIsCalled_Then_AnArgumentExceptionIsThrown(
            String secretKey)
        {
            var provider = new TotpProvider();

            Assert.Throws<ArgumentException>(() => provider.GetBase32EncodedSecretKey(secretKey));
        }

        [Theory]
        [InlineData("12345", "GEZDGNBV")]
        [InlineData("123456", "GEZDGNBVGY")]
        [InlineData("1234567", "GEZDGNBVGY3Q")]
        [InlineData("12345678", "GEZDGNBVGY3TQ")]
        [InlineData("123456789", "GEZDGNBVGY3TQOI")]
        [InlineData("1234567890", "GEZDGNBVGY3TQOJQ")]
        public void Given_AValidSecretKey_When_GetBase32EncodedSecretKeyIsCalled_Then_EncodedSecretKeyIsReturned(
            String secretKey,
            String expected)
        {
            var provider = new TotpProvider();

            var result = provider.GetBase32EncodedSecretKey(secretKey);

            Assert.Equal(expected, result);
        }
    }
}