// ReSharper disable ConvertToConstant.Local

using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace SimpleTotp.Tests
{
    [ExcludeFromCodeCoverage]
    public class ValidationHelperTests
    {
        [Fact]
        public void Given_NullExpression_When_CheckNotNullOrWhitespaceIsCalled_Then_ExceptionIsThrown()
        {
            Assert.Throws<Exception>(() => ValidationHelper.CheckNotNullOrWhitespace(null));
        }

        [Fact]
        public void Given_NonMemberExpression_When_CheckNotNullOrWhitespaceIsCalled_Then_ExceptionIsThrown()
        {
            Assert.Throws<Exception>(() => ValidationHelper.CheckNotNullOrWhitespace(() => null));
        }

        [Fact]
        public void Given_ExpressionPointingToNullMember_When_CheckNotNullOrWhitespaceIsCalled_Then_ArgumentExceptionIsThrown()
        {
            String test = null;
            Assert.Throws<ArgumentException>(() => ValidationHelper.CheckNotNullOrWhitespace(() => test));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t")]
        public void Given_ExpressionPointingToWhitespaceMember_When_CheckNotNullOrWhitespaceIsCalled_Then_ArgumentExceptionIsThrown(
            String value)
        {
            Assert.Throws<ArgumentException>(() => ValidationHelper.CheckNotNullOrWhitespace(() => value));
        }

        [Fact]
        public void Given_ExpressionPointingToNonNullMember_When_CheckNotNullOrWhitespaceIsCalled_Then_NothingHappens()
        {
            var test = "test";
            ValidationHelper.CheckNotNullOrWhitespace(() => test);
        }
    }
}