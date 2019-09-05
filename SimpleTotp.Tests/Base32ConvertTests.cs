using System;
using System.Text;
using Xunit;
// ReSharper disable ExpressionIsAlwaysNull
// ReSharper disable ConditionIsAlwaysTrueOrFalse

namespace SimpleTotp.Tests
{
    public class Base32ConvertTests
    {
        [Fact]
        public void Given_Base32Converter_When_NullArrayIsConvertedToBase32StringWithPadding_Then_ResultIsNull()
        {
            var input = default(byte[]);
            var pad = true;

            var result = Base32Convert.ToBase32String(input, pad);

            Assert.Null(result);
        }

        [Fact]
        public void Given_Base32Converter_When_NullArrayIsConvertedToBase32StringWithoutPadding_Then_ResultIsNull()
        {
            var input = default(byte[]);
            var pad = false;

            var result = Base32Convert.ToBase32String(input, pad);

            Assert.Null(result);
        }

        [Fact]
        public void Given_Base32Converter_When_EmptyArrayIsConvertedToBase32StringWithPadding_Then_ResultIsEmpty()
        {
            var input = new byte[0];
            var pad = true;

            var result = Base32Convert.ToBase32String(input, pad);

            Assert.Equal(String.Empty, result);
        }

        [Fact]
        public void Given_Base32Converter_When_EmptyArrayIsConvertedToBase32StringWithoutPadding_Then_ResultIsEmpty()
        {
            var input = new byte[0];
            var pad = false;

            var result = Base32Convert.ToBase32String(input, pad);

            Assert.Equal(String.Empty, result);
        }

        [Theory]
        [InlineData("12345", "GEZDGNBV")]
        [InlineData("123456", "GEZDGNBVGY======")]
        [InlineData("1234567", "GEZDGNBVGY3Q====")]
        [InlineData("12345678", "GEZDGNBVGY3TQ===")]
        [InlineData("123456789", "GEZDGNBVGY3TQOI=")]
        [InlineData("1234567890", "GEZDGNBVGY3TQOJQ")]
        public void Given_Base32Converter_When_AStringInUTF8IsConvertedToBase32StringWithPadding_Then_ResultIsCorrect(String input, String expected)
        {
            var pad = true;

            var result = Base32Convert.ToBase32String(Encoding.UTF8.GetBytes(input), pad);

            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("12345", "GEZDGNBV")]
        [InlineData("123456", "GEZDGNBVGY")]
        [InlineData("1234567", "GEZDGNBVGY3Q")]
        [InlineData("12345678", "GEZDGNBVGY3TQ")]
        [InlineData("123456789", "GEZDGNBVGY3TQOI")]
        [InlineData("1234567890", "GEZDGNBVGY3TQOJQ")]
        public void Given_Base32Converter_When_AStringInUTF8IsConvertedToBase32StringWithoutPadding_Then_ResultIsCorrect(String input, String expected)
        {
            var pad = false;

            var result = Base32Convert.ToBase32String(Encoding.UTF8.GetBytes(input), pad);

            Assert.Equal(expected, result);
        }
    }
}