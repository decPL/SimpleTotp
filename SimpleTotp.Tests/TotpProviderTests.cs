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
        public void Given_AnEmptyAccountName_When_GetAuthenticatorRegistrationDataIsCalled_Then_AnArgumentExceptionIsThrown(
            String accountName)
        {
            var provider = new TotpProvider();

            Assert.Throws<ArgumentException>(() => provider.GetAuthenticatorRegistrationData(accountName, "ISSUER"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t")]
        public void Given_AnEmptyIssuer_When_GetAuthenticatorRegistrationDataIsCalled_Then_AnArgumentExceptionIsThrown(
            String issuer)
        {
            var provider = new TotpProvider();

            Assert.Throws<ArgumentException>(() => provider.GetAuthenticatorRegistrationData("ACCOUNTNAME", issuer));
        }

        [Fact]
        public void Given_AnAccountNameWithColon_When_GetAuthenticatorRegistrationDataIsCalled_Then_AnArgumentExceptionIsThrown()
        {
            var provider = new TotpProvider();

            Assert.Throws<ArgumentException>(() => provider.GetAuthenticatorRegistrationData("ACCOUNT:NAME", "ISSUER"));
        }

        [Fact]
        public void Given_AnIssuerWithColon_When_GetAuthenticatorRegistrationDataIsCalled_Then_AnArgumentExceptionIsThrown()
        {
            var provider = new TotpProvider();

            Assert.Throws<ArgumentException>(() => provider.GetAuthenticatorRegistrationData("ACCOUNTNAME", "ISS:UER"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t")]
        public void Given_AnEmptySecretKey_When_GetAuthenticatorRegistrationDataIsCalled_Then_ANewSecretKeyIsGenerated(
            String secretKey)
        {
            var provider = new TotpProvider();

            var result = provider.GetAuthenticatorRegistrationData("ACCOUNT_NAME", "ISSUER", secretKey);

            Assert.NotNull(result);
            Assert.False(String.IsNullOrWhiteSpace(result.SecretKey));
        }

        [Theory]
        [InlineData("12345", "GEZDGNBV")]
        [InlineData("123456", "GEZDGNBVGY")]
        [InlineData("1234567", "GEZDGNBVGY3Q")]
        [InlineData("12345678", "GEZDGNBVGY3TQ")]
        [InlineData("123456789", "GEZDGNBVGY3TQOI")]
        [InlineData("1234567890", "GEZDGNBVGY3TQOJQ")]
        public void Given_AValidSecretKey_When_GetAuthenticatorRegistrationDataIsCalled_Then_ResultContainsTheEncodedSecretKey(
            String secretKey,
            String expected)
        {
            var provider = new TotpProvider();

            var result = provider.GetAuthenticatorRegistrationData("ACCOUNT_NAME", "ISSUER", secretKey);

            Assert.NotNull(result);
            Assert.Equal(expected, result.ManualRegistrationKey);
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
        public void Given_CorrectRegistrationData_When_GetAuthenticatorRegistrationDataIsCalled_Then_TheCorrectUriIsReturned(
            String secretKey,
            String issuer,
            String accountName,
            String expectedQrCode)
        {
            var provider = new TotpProvider();

            var result = provider.GetAuthenticatorRegistrationData(accountName, issuer, secretKey);

            Assert.NotNull(result);
            Assert.Equal(secretKey, result.SecretKey);
            Assert.Equal(accountName, result.AccountName);
            Assert.Equal(issuer, result.Issuer);
            Assert.Equal(expectedQrCode, result.QrCodeUri);
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
        public void Given_CorrectRegistrationData_When_GetAuthenticatorRegistrationDataIsCalledWithoutAccountNamePrefixing_Then_TheCorrectUriIsReturned(
            String secretKey,
            String issuer,
            String accountName,
            String expectedQrCode)
        {
            var provider = new TotpProvider();

            var result = provider.GetAuthenticatorRegistrationData(accountName, issuer, secretKey, false);

            Assert.NotNull(result);
            Assert.Equal(secretKey, result.SecretKey);
            Assert.Equal(accountName, result.AccountName);
            Assert.Equal(issuer, result.Issuer);
            Assert.Equal(expectedQrCode, result.QrCodeUri);
        }
    }
}