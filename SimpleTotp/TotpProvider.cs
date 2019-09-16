// ReSharper disable StringLiteralTypo

using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace SimpleTotp
{
    /// <inheritdoc />
    public class TotpProvider : ITotpProvider
    {
        /// <summary>
        /// Unix Epoch time - in ticks
        /// </summary>
        private const long UnixEpochInTicks = 621355968000000000L;

        /// <summary>
        /// Period for which the same code should be calculated - 30 seconds (in ticks)
        /// <remarks>
        /// TOTP specification allows this to be customized; most existing authenticators use a static value of 30 seconds - hence a const
        /// </remarks>
        /// </summary>
        private const long Period = 300000000L;

        /// <summary>
        /// Number of digits in generated TOTP code
        /// <remarks>
        /// TOTP specification allows this to be customized; most existing authenticators use a static value of 6 digits - hence a const
        /// </remarks>
        /// </summary>
        private const int CodeDigits = 6;

        /// <inheritdoc />
        public RegistrationData GetAuthenticatorRegistrationData(string accountName,
                                                                  string issuer,
                                                                  string secretKey = null,
                                                                  bool prefixAccountNameWithIssuer = true)
        {
            if (String.IsNullOrWhiteSpace(issuer))
                throw new ArgumentException($"Provided {nameof(issuer)} is empty", nameof(issuer));
            if (String.IsNullOrWhiteSpace(accountName))
                throw new ArgumentException($"Provided {nameof(accountName)} is empty", nameof(accountName));

            if (issuer.Contains(":"))
                throw new ArgumentException($"{nameof(issuer)} contains a colon, which is not allowed", nameof(issuer));

            if (accountName.Contains(":"))
                throw new ArgumentException($"{nameof(accountName)} contains a colon, which is not allowed", nameof(accountName));

            if (String.IsNullOrWhiteSpace(secretKey))
                secretKey = Guid.NewGuid().ToString();

            var issuerEscaped = Uri.EscapeDataString(issuer);
            var accountNameEscaped = prefixAccountNameWithIssuer
                                         ? $"{issuerEscaped}:{Uri.EscapeDataString(accountName)}"
                                         : Uri.EscapeDataString(accountName);
            var encodedSecret = Base32Convert.ToBase32String(Encoding.UTF8.GetBytes(secretKey), false);

            var qrCodeUri = $"otpauth://totp/{accountNameEscaped}?secret={encodedSecret}&issuer={issuerEscaped}";

            return new RegistrationData
                   {
                       AccountName = accountName,
                       Issuer = issuer,
                       ManualRegistrationKey = encodedSecret,
                       QrCodeUri = qrCodeUri,
                       SecretKey = secretKey
                   };
        }

        /// <inheritdoc />
        public string GetCodeAtSpecificTime(string secretKey, DateTimeOffset time)
        {
            if (String.IsNullOrWhiteSpace(secretKey))
                throw new ArgumentException($"Provided {nameof(secretKey)} is empty", nameof(secretKey));

            var utcTicks = time.UtcTicks;
            if (utcTicks <= UnixEpochInTicks)
                throw new ArgumentException($"{nameof(time)} must be after the Unix Epoch");

            var counter = (time.UtcTicks - UnixEpochInTicks) / Period;

            var bytes = BitConverter.GetBytes(counter);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            byte[] hash;
            using (var hashAlgorithm = new HMACSHA1(Encoding.UTF8.GetBytes(secretKey)))
            {
                hash = hashAlgorithm.ComputeHash(bytes);
            }

            var index = hash[hash.Length - 1] & 15;
            var code = ((hash[index] & sbyte.MaxValue) << 24
                        | hash[index + 1] << 16
                        | hash[index + 2] << 8
                        | hash[index + 3]).ToString(CultureInfo.InvariantCulture);

            if (code.Length > CodeDigits)
                code = code.Substring(code.Length - CodeDigits, CodeDigits);

            return code.PadLeft(CodeDigits, '0');
        }

        /// <inheritdoc />
        public string GetCodeAtSpecificTime(String secretKey, DateTimeOffset time, out TimeSpan remaining)
        {
            remaining = new TimeSpan(Period - time.UtcTicks % Period);
            return this.GetCodeAtSpecificTime(secretKey, time);
        }       
    }
}