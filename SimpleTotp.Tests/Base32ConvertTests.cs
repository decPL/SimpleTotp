﻿// ReSharper disable ExpressionIsAlwaysNull
// ReSharper disable ConditionIsAlwaysTrueOrFalse
// ReSharper disable ConvertToConstant.Local
// ReSharper disable StringLiteralTypo

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Xunit;

namespace SimpleTotp.Tests
{
    [ExcludeFromCodeCoverage]
    public class Base32ConvertTests
    {
        [Fact]
        public void Given_Base32Converter_When_NullArrayIsConvertedToBase32StringWithPadding_Then_ResultIsNull()
        {
            var input = default(byte[]);
            var applyPadding = true;

            var result = Base32Convert.ToBase32String(input, applyPadding);

            Assert.Null(result);
        }

        [Fact]
        public void Given_Base32Converter_When_NullArrayIsConvertedToBase32StringWithoutPadding_Then_ResultIsNull()
        {
            var input = default(byte[]);
            var applyPadding = false;

            var result = Base32Convert.ToBase32String(input, applyPadding);

            Assert.Null(result);
        }

        [Fact]
        public void Given_Base32Converter_When_EmptyArrayIsConvertedToBase32StringWithPadding_Then_ResultIsEmpty()
        {
            var input = new byte[0];
            var applyPadding = true;

            var result = Base32Convert.ToBase32String(input, applyPadding);

            Assert.Equal(String.Empty, result);
        }

        [Fact]
        public void Given_Base32Converter_When_EmptyArrayIsConvertedToBase32StringWithoutPadding_Then_ResultIsEmpty()
        {
            var input = new byte[0];
            var applyPadding = false;

            var result = Base32Convert.ToBase32String(input, applyPadding);

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
            var applyPadding = true;

            var result = Base32Convert.ToBase32String(Encoding.UTF8.GetBytes(input), applyPadding);

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
            var applyPadding = false;

            var result = Base32Convert.ToBase32String(Encoding.UTF8.GetBytes(input), applyPadding);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void Given_Base32Converter_When_NullBase32StringIsConvertedToByteArray_Then_ResultIsNull()
        {
            var base32String = default(String);
            
            var result = Base32Convert.FromBase32String(base32String);

            Assert.Null(result);
        }

        [Theory]
        [InlineData("")]
        [InlineData("    ")]
        [InlineData("\t")]
        public void Given_Base32Converter_When_WhitespaceBase32StringIsConvertedToByteArray_Then_ResultIsEmptyArray(String base32String)
        {
            var result = Base32Convert.FromBase32String(base32String);

            Assert.Empty(result);
        }

        [Theory]
        [InlineData("11")]
        [InlineData("BB=B==")]
        [InlineData("    GEZDGNBVGY======")]
        public void Given_Base32Converter_When_ANonBase32StringIsConvertedToByteArray_Then_AnExceptionIsThrown(String base32String)
        {
            Assert.Throws<ArgumentException>(() => Base32Convert.FromBase32String(base32String));
        }

        [Theory]
        [InlineData("12345", "GEZDGNBV")]
        [InlineData("123456", "GEZDGNBVGY")]
        [InlineData("123456", "GEZDGNBVGY======")]
        [InlineData("1234567", "GEZDGNBVGY3Q")]
        [InlineData("1234567", "GEZDGNBVGY3Q====")]
        [InlineData("12345678", "GEZDGNBVGY3TQ")]
        [InlineData("12345678", "GEZDGNBVGY3TQ===")]
        [InlineData("123456789", "GEZDGNBVGY3TQOI")]
        [InlineData("123456789", "GEZDGNBVGY3TQOI=")]
        [InlineData("1234567890", "GEZDGNBVGY3TQOJQ")]
        public void Given_Base32Converter_When_ABase32StringIsConvertedToByteArray_Then_CorrectDataIsReturned(
            String expected,
            String base32String)
        {
            var result = Base32Convert.FromBase32String(base32String);

            Assert.Equal(expected, Encoding.UTF8.GetString(result));
        }
        
        [Theory]
        [InlineData("12345")]
        [InlineData("Zażółć gęślą jaźń")]
        [InlineData("!@#$%^&*(){}:")]
        public void
            Given_Base32Converter_When_AStringInUTF8IsConvertedToBase32StringWithPaddingAndBack_Then_ItIsTheSameAsTheOriginalString(
                String input)
        {
            var applyPadding = true;

            var base32 = Base32Convert.ToBase32String(Encoding.UTF8.GetBytes(input), applyPadding);
            var result = Base32Convert.FromBase32String(base32);

            Assert.Equal(input, Encoding.UTF8.GetString(result));
        }

        [Theory]
        [InlineData("12345")]
        [InlineData("Zażółć gęślą jaźń")]
        [InlineData("!@#$%^&*(){}:")]
        public void
            Given_Base32Converter_When_AStringInUTF8IsConvertedToBase32StringWithoutPaddingAndBack_Then_ItIsTheSameAsTheOriginalString(
                String input)
        {
            var applyPadding = false;

            var base32 = Base32Convert.ToBase32String(Encoding.UTF8.GetBytes(input), applyPadding);
            var result = Base32Convert.FromBase32String(base32);

            Assert.Equal(input, Encoding.UTF8.GetString(result));
        }
    }
}