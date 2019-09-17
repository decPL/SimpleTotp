// ReSharper disable StringLiteralTypo

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

[assembly:InternalsVisibleTo("SimpleTotp.Tests")]

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
            ValidationHelper.CheckNotNullOrWhitespace(() => issuer);
            ValidationHelper.CheckNotNullOrWhitespace(() => accountName);

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
            ValidationHelper.CheckNotNullOrWhitespace(() => secretKey);
            
            var counter = this.CalculateCounter(time);

            return this.CalculateCodeInternal(Encoding.UTF8.GetBytes(secretKey), counter);
        }

        /// <inheritdoc />
        public string GetCodeAtSpecificTime(String secretKey, DateTimeOffset time, out TimeSpan remaining)
        {
            remaining = new TimeSpan(Period - time.UtcTicks % Period);
            return this.GetCodeAtSpecificTime(secretKey, time);
        }

        /// <summary>
        /// Returns a collection of valid codes between (<paramref name="time"/> - <paramref name="pastTolerance"/>)
        /// and (<paramref name="time"/> + <paramref name="futureTolerance"/>).
        /// </summary>
        /// <param name="secretKey">User's secret TOTP key</param>
        /// <param name="time">Reference point in time for specifying the valid time period (usually: current time)</param>
        /// <param name="pastTolerance">Specified how far to the past will the valid period reach</param>
        /// <param name="futureTolerance">Specified how far to the future will the valid period reach</param>
        /// <returns>A collection of valid TOTP codes</returns>
        protected internal IEnumerable<String> GetValidCodesForPeriod(String secretKey,
                                                                      DateTimeOffset time,
                                                                      TimeSpan pastTolerance,
                                                                      TimeSpan futureTolerance)
        {
            ValidationHelper.CheckNotNullOrWhitespace(() => secretKey);

            var minCounter = this.CalculateCounter(time - pastTolerance);
            var maxCounter = this.CalculateCounter(time + futureTolerance);
            var secretKeyBytes = Encoding.UTF8.GetBytes(secretKey);

            for (var step = minCounter; step <= maxCounter; step++)
            {
                yield return this.CalculateCodeInternal(secretKeyBytes, step);
            }
        }

        /// <summary>
        /// Gets a TOTP code based on a secret and a counter
        /// </summary>
        /// <param name="secretKeyBytes">User's secret TOTP key as byte array</param>
        /// <param name="counter">TOTP counter</param>
        /// <returns>TOTP code</returns>
        protected string CalculateCodeInternal(byte[] secretKeyBytes, long counter)
        {
            var bytes = BitConverter.GetBytes(counter);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            byte[] hash;
            using (var hashAlgorithm = new HMACSHA1(secretKeyBytes))
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

        /// <summary>
        /// Calculates the counter used to determine the time-based factor in the TOTP code
        /// </summary>
        /// <param name="time">Time for which the counter is calculated</param>
        /// <returns>TOTP counter</returns>
        protected long CalculateCounter(DateTimeOffset time)
        {
            var utcTicks = time.UtcTicks;
            if (utcTicks <= UnixEpochInTicks)
                throw new ArgumentException($"{nameof(time)} must be after the Unix Epoch");

            var counter = (time.UtcTicks - UnixEpochInTicks) / Period;
            return counter;
        }
    }
}