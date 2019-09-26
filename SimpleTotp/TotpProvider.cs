// ReSharper disable StringLiteralTypo

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

[assembly:InternalsVisibleTo("SimpleTotp.Tests")]

namespace SimpleTotp
{
    /// <summary>
    /// Simple TOTP algorithm provider
    /// </summary>
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

        /// <summary>
        /// Default tolerance for TOTP code validation
        /// </summary>
        private static readonly TimeSpan DefaultTolerance = TimeSpan.FromMinutes(1);

        /// <summary>
        /// Calculates the data required for the user to add an account to TOTP authenticators
        /// </summary>
        /// <param name="accountName">
        /// Name of the account for which registration will be made
        /// <remarks>Has no functional impact in TOTP, but helps the user identify the account in their TOTP authenticator.</remarks>
        /// </param>
        /// <param name="issuer">
        /// Issuer of the TOTP secret key
        /// <remarks>Has no functional impact in TOTP, but helps the user identify the account in their TOTP authenticator.</remarks>
        /// </param>
        /// <param name="secretKey">User's secret TOTP key. Will be generated automatically if none provided.</param>
        /// <param name="prefixAccountNameWithIssuer">
        /// Allows controlling how the QR code uri is generated.
        /// True if account name should be prefixed with the issuer, false otherwise.
        /// Default - true (recommended).
        /// </param>
        /// <returns>Data required to add the specified account to TOTP authenticators</returns>
        public RegistrationData GetAuthenticatorRegistrationData(String accountName,
                                                                 String issuer,
                                                                 String secretKey = null,
                                                                 bool prefixAccountNameWithIssuer = true)
        {
            ValidationHelper.CheckNotNullOrWhitespace(() => issuer);
            ValidationHelper.CheckNotNullOrWhitespace(() => accountName);

            if (issuer.Contains(":"))
                throw new ArgumentException($"{nameof(issuer)} contains a colon, which is not allowed", nameof(issuer));

            if (accountName.Contains(":"))
                throw new ArgumentException($"{nameof(accountName)} contains a colon, which is not allowed",
                                            nameof(accountName));

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

        /// <summary>
        /// Gets a TOTP code for a specific key at a given time point
        /// <remarks>This can be utilized to write your own TOTP authenticator or for testing your TOTP authentication</remarks>
        /// </summary>
        /// <param name="secretKey">User's secret TOTP key</param>
        /// <param name="time">
        /// Time point at which to generate the time-based code
        /// <remarks>Must be greater that the Unix Epoch time</remarks>
        /// </param>
        /// <returns>Generated TOTP code</returns>
        public string GetCodeAtSpecificTime(string secretKey, DateTimeOffset time)
        {
            ValidationHelper.CheckNotNullOrWhitespace(() => secretKey);
            
            var counter = this.CalculateCounter(time);

            return this.CalculateCode(Encoding.UTF8.GetBytes(secretKey), counter);
        }

        /// <summary>
        /// Gets a TOTP code for a specific key at a given time point
        /// <remarks>This can be utilized to write your own TOTP authenticator or for testing your TOTP authentication</remarks>
        /// </summary>
        /// <param name="secretKey">User's secret TOTP key</param>
        /// <param name="time">
        /// Time point at which to generate the time-based code
        /// <remarks>Must be greater that the Unix Epoch time</remarks>
        /// </param>
        /// <param name="remaining">Remaining time before the current code changes</param>
        /// <returns>Generated TOTP code</returns>
        public string GetCodeAtSpecificTime(String secretKey, DateTimeOffset time, out TimeSpan remaining)
        {
            remaining = new TimeSpan(Period - time.UtcTicks % Period);
            return this.GetCodeAtSpecificTime(secretKey, time);
        }

        /// <summary>
        /// Checks if the provided TOTP code is valid, using default time tolerance
        /// </summary>
        /// <param name="secretKey">User's secret TOTP key</param>
        /// <param name="code">
        /// TOTP code
        /// <remarks>The code is a numeric string of length 6 (left-padded with 0s)</remarks>
        /// </param>
        /// <param name="time">Time when the code should be valid (plus/minus default tolerance)</param>
        /// <returns>True if the code is valid, false otherwise</returns>
        public bool ValidateCode(String secretKey, String code, DateTimeOffset time)
            => this.ValidateCode(secretKey, code, time, DefaultTolerance);

        /// <summary>
        /// Checks if the provided TOTP code is valid, using provided time tolerance
        /// </summary>
        /// <param name="secretKey">User's secret TOTP key</param>
        /// <param name="code">
        /// TOTP code
        /// <remarks>The code is a numeric string of length 6 (left-padded with 0s)</remarks>
        /// </param>
        /// <param name="time">
        /// Time when the code should be valid (plus/minus <paramref name="tolerance"/>)
        /// <remarks>Effective "window" of valid codes is <paramref name="time"/>-<paramref name="tolerance"/> to
        /// <paramref name="time"/>+<paramref name="tolerance"/></remarks>
        /// </param>
        /// <param name="tolerance">
        /// Specifies how far in the future/past will we look for valid codes to check the provided <paramref name="code"/> against.
        /// <remarks>
        /// This is used to negate the impact of time passing between user seeing the code and this code validating it.
        /// It also offsets the potential minor differences in clock time between the user's device and the server.
        /// </remarks>
        /// </param>
        /// <returns>True if the code is valid, false otherwise</returns>
        public bool ValidateCode(String secretKey, String code, DateTimeOffset time, TimeSpan tolerance)
            => this.ValidateCode(secretKey, code, time, tolerance, tolerance);

        /// <summary>
        /// Checks if the provided TOTP code is valid, using provided time tolerance (past/future)
        /// </summary>
        /// <param name="secretKey">User's secret TOTP key</param>
        /// <param name="code">
        /// TOTP code
        /// <remarks>The code is a numeric string of length 6 (left-padded with 0s)</remarks>
        /// </param>
        /// <param name="time">
        /// Time when the code should be valid (plus/minus <paramref name="pastTolerance"/>/<paramref name="futureTolerance"/>)
        /// <remarks>Effective "window" of valid codes is <paramref name="time"/>-<paramref name="pastTolerance"/> to
        /// <paramref name="time"/>+<paramref name="futureTolerance"/></remarks>
        /// </param>
        /// <param name="pastTolerance">
        /// Specifies how far in the past will we look for valid codes to check the provided <paramref name="code"/> against.
        /// <remarks>
        /// This is used to negate the impact of time passing between user seeing the code and this code validating it.
        /// It also offsets the potential minor differences in clock time between the user's device and the server.
        /// </remarks>
        /// </param>
        /// <param name="futureTolerance">
        /// Specifies how far in the future will we look for valid codes to check the provided <paramref name="code"/> against.
        /// <remarks>
        /// This is used to negate the potential minor differences in clock time between the user's device and the server.
        /// </remarks>
        /// </param>
        /// <returns>True if the code is valid, false otherwise</returns>
        public bool ValidateCode(String secretKey, String code, DateTimeOffset time, TimeSpan pastTolerance, TimeSpan futureTolerance)
        {
            ValidationHelper.CheckNotNullOrWhitespace(() => secretKey);
            ValidationHelper.CheckNotNullOrWhitespace(() => code);

            var validCodes = this.GetValidCodesForPeriod(secretKey, time, pastTolerance, futureTolerance);
            return validCodes.Contains(code, StringComparer.OrdinalIgnoreCase);
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
                yield return this.CalculateCode(secretKeyBytes, step);
            }
        }

        /// <summary>
        /// Gets a TOTP code based on a secret and a counter
        /// </summary>
        /// <param name="secretKeyBytes">User's secret TOTP key as byte array</param>
        /// <param name="counter">TOTP counter</param>
        /// <returns>TOTP code</returns>
        protected string CalculateCode(byte[] secretKeyBytes, long counter)
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