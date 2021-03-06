﻿using System;
using System.Linq;
using System.Text;

namespace SimpleTotp
{
    /// <summary>
    /// Converts to/from Base32
    /// </summary>
    /// <remarks>
    /// <see href="https://en.wikipedia.org/wiki/Base32"/>
    /// </remarks>
    public static class Base32Convert
    {
        /// <summary>
        /// Length of the block in the Base32 conversion algorithm
        /// </summary>
        private const int BlockLength = 5;

        /// <summary>
        /// Number of bits in byte (duh!) - used for readability only
        /// </summary>
        private const int BitsInByte = 8;

        /// <summary>
        /// Base32 padding character
        /// </summary>
        private const char PaddingChar = '=';

        /// <summary>
        /// Longest possible padding length in the Base32 algorithm
        /// </summary>
        private const int MaxPaddingLengthInBase32 = (BlockLength - 1) * BitsInByte / BlockLength;

        /// <summary>
        /// Allowed characters for Base32
        /// </summary>
        public static char[] Base32CharacterSet { get; } = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567".ToCharArray();

        /// <summary>
        /// Converts input byte array to a Base32 string.
        /// </summary>
        /// <param name="input">Input data</param>
        /// <param name="applyPadding">
        /// True if '=' padding should be added, false otherwise (default: true)
        /// <see href="https://tools.ietf.org/html/rfc3548"/>
        /// </param>
        /// <returns>Base32 string representation of the input data</returns>
        public static String ToBase32String(byte[] input, bool applyPadding = true)
        {
            if (input == null)
            {
                return null;
            }

            if (input.Length == 0)
            {
                return String.Empty;
            }

            var inputBitsBuilder = new StringBuilder(input.Length * BitsInByte + BlockLength - 1);
            foreach (var @byte in input)
            {
                var value = Convert.ToString(@byte, 2);
                inputBitsBuilder.Append(new String('0', BitsInByte - value.Length));
                inputBitsBuilder.Append(value);
            }

            inputBitsBuilder.Append(new String('0', BlockLength - 1));

            var inputBits = inputBitsBuilder.ToString();
            var result = new StringBuilder(input.Length * BitsInByte / BlockLength + MaxPaddingLengthInBase32);
            for (var i = 0; i < input.Length * BitsInByte; i += BlockLength)
            {
                var block = inputBits.Substring(i, BlockLength);

                result.Append(Base32CharacterSet[Convert.ToInt32(block, 2)]);
            }

            if (!applyPadding)
            {
                return result.ToString();
            }

            const int bitsInBlock = BlockLength * BitsInByte;
            result.Append(new String(PaddingChar,
                                     (bitsInBlock - input.Length * BitsInByte % bitsInBlock) % bitsInBlock / BlockLength));

            return result.ToString();
        }

        /// <summary>
        /// Converts input from a Base32 string to a byte array
        /// </summary>
        /// <param name="base32String">Base32 string</param>
        /// <returns>Decoded data</returns>
        public static byte[] FromBase32String(String base32String)
        {
            if (base32String == null)
                return null;
            if (String.IsNullOrWhiteSpace(base32String))
                return new byte[0];

            var sanitized = base32String.TrimEnd(PaddingChar).ToCharArray();

            if (sanitized.Any(c => !Base32CharacterSet.Contains(c)))
            {
                throw new ArgumentException("Provided input string is not Base32", nameof(base32String));
            }

            var inputBitsBuilder = new StringBuilder(sanitized.Length * BlockLength + BitsInByte - 1);
            foreach (var index in sanitized.Select(c => Array.IndexOf(Base32CharacterSet, c)))
            {
                var value = Convert.ToString(index, 2);
                inputBitsBuilder.Append(new String('0', BlockLength - value.Length));
                inputBitsBuilder.Append(value);
            }

            inputBitsBuilder.Append(new String('0', BitsInByte - 1));
            var inputBits = inputBitsBuilder.ToString();

            return Enumerable.Range(0, sanitized.Length * BlockLength / BitsInByte)
                             .Select(i => Convert.ToByte(inputBits.Substring(i * BitsInByte, BitsInByte), 2))
                             .ToArray();
        }
    }
}