using System;
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
        /// Length of the block in the Base32 conversion algorithm - in bits
        /// </summary>
        private const int BlockLengthBits = BlockLength * 8;

        /// <summary>
        /// Base32 padding character
        /// </summary>
        private const char PaddingChar = '=';

        /// <summary>
        /// Allowed characters for Base32
        /// </summary>
        public static char[] Base32CharacterSet { get; } = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567".ToCharArray();

        /// <summary>
        /// Converts input byte array to a Base32 string.
        /// </summary>
        /// <param name="input">Input data</param>
        /// <param name="pad">
        /// True if '=' padding should be added, false otherwise (default: true)
        /// <see href="https://tools.ietf.org/html/rfc3548"/>
        /// </param>
        /// <returns>Base32 string representation of the input data</returns>
        public static String ToBase32String(byte[] input, bool pad = true)
        {
            if (input == null)
            {
                return null;
            }

            if (input.Length == 0)
            {
                return String.Empty;
            }

            var inputBitsBuilder = new StringBuilder(input.Length * 8 + BlockLength - 1);
            foreach (var b in input)
            {
                var value = Convert.ToString(b, 2);
                inputBitsBuilder.Append(new String('0', 8 - value.Length));
                inputBitsBuilder.Append(value);
            }

            inputBitsBuilder.Append(new String('0', 7));

            var inputBits = inputBitsBuilder.ToString();
            var result = new StringBuilder(input.Length * 8 / 5 + 6);
            for (int i = 0; i < input.Length * 8; i += BlockLength)
            {
                var block = inputBits.Substring(i, BlockLength);

                result.Append(Base32CharacterSet[Convert.ToInt32(block, 2)]);
            }

            if (pad)
            {
                result.Append(new String(PaddingChar,
                                         (BlockLengthBits - input.Length * 8 % BlockLengthBits) % BlockLengthBits / 5));
            }

            return result.ToString();
        }
    }
}