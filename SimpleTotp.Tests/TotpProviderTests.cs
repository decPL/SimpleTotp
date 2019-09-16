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

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t")]
        public void Given_AnEmptySecretKey_When_GetRegisterUriForQrCodeIsCalled_Then_AnArgumentExceptionIsThrown(
            String secretKey)
        {
            var provider = new TotpProvider();

            Assert.Throws<ArgumentException>(() => provider.GetRegisterUriForQrCode(secretKey,
                                                                                    "ISSUER",
                                                                                    "ACCOUNTNAME"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t")]
        public void Given_AnEmptyIssuer_When_GetRegisterUriForQrCodeIsCalled_Then_AnArgumentExceptionIsThrown(
            String issuer)
        {
            var provider = new TotpProvider();

            Assert.Throws<ArgumentException>(() => provider.GetRegisterUriForQrCode("SECRETKEY",
                                                                                    issuer,
                                                                                    "ACCOUNTNAME"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t")]
        public void Given_AnEmptyAccountName_When_GetRegisterUriForQrCodeIsCalled_Then_AnArgumentExceptionIsThrown(
            String accountName)
        {
            var provider = new TotpProvider();

            Assert.Throws<ArgumentException>(() => provider.GetRegisterUriForQrCode("SECRETKEY",
                                                                                    "ISSUER",
                                                                                    accountName));
        }

        [Fact]
        public void Given_AnIssuerWithColon_When_GetRegisterUriForQrCodeIsCalled_Then_AnArgumentExceptionIsThrown()
        {
            var provider = new TotpProvider();

            Assert.Throws<ArgumentException>(() => provider.GetRegisterUriForQrCode("SECRETKEY",
                                                                                    "ISS:UER",
                                                                                    "ACCOUNTNAME"));
        }

        [Fact]
        public void Given_AnAccountNameWithColon_When_GetRegisterUriForQrCodeIsCalled_Then_AnArgumentExceptionIsThrown()
        {
            var provider = new TotpProvider();

            Assert.Throws<ArgumentException>(() => provider.GetRegisterUriForQrCode("SECRETKEY",
                                                                                    "ISSUER",
                                                                                    "ACCOUNT:NAME"));
        }

        [Theory]
        [InlineData(
            "123456",
            "ISSUER",
            "ACCOUNT_NAME",
            "otpauth://totp/ISSUER:ACCOUNT_NAME?secret=GEZDGNBVGY&issuer=ISSUER")]
        [InlineData(
            "123456",
            "ISSUER(TEST#ME%)",
            "ACCOUNT&NAME(%3A)",
            "otpauth://totp/ISSUER%28TEST%23ME%25%29:ACCOUNT%26NAME%28%253A%29?secret=GEZDGNBVGY&issuer=ISSUER%28TEST%23ME%25%29")]
        public void Given_CorrectRegistrationData_When_GetRegisterUriForQrCodeIsCalled_Then_TheCorrectUriIsReturned(
            String secretKey,
            String issuer,
            String accountName,
            String expected)
        {
            var provider = new TotpProvider();

            var result = provider.GetRegisterUriForQrCode(secretKey, issuer, accountName);

            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(
            "123456",
            "ISSUER",
            "ACCOUNT_NAME",
            "otpauth://totp/ACCOUNT_NAME?secret=GEZDGNBVGY&issuer=ISSUER")]
        [InlineData(
            "123456",
            "ISSUER(TEST#ME%)",
            "ACCOUNT&NAME(%3A)",
            "otpauth://totp/ACCOUNT%26NAME%28%253A%29?secret=GEZDGNBVGY&issuer=ISSUER%28TEST%23ME%25%29")]
        public void Given_CorrectRegistrationData_When_GetRegisterUriForQrCodeIsCalledWithoutAccountNamePrefixing_Then_TheCorrectUriIsReturned(
            String secretKey,
            String issuer,
            String accountName,
            String expected)
        {
            var provider = new TotpProvider();

            var result = provider.GetRegisterUriForQrCode(secretKey, issuer, accountName, false);

            Assert.Equal(expected, result);
        }
    }
}