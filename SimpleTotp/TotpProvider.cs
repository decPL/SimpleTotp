// ReSharper disable StringLiteralTypo

using System;
using System.Text;

namespace SimpleTotp
{
    /// <inheritdoc />
    public class TotpProvider : ITotpProvider
    {
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
    }
}