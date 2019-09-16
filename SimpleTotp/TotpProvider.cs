// ReSharper disable StringLiteralTypo

using System;
using System.Text;

namespace SimpleTotp
{
    /// <inheritdoc />
    public class TotpProvider : ITotpProvider
    {
        /// <inheritdoc />
        public String GetBase32EncodedSecretKey(String secretKey)
        {
            if (String.IsNullOrWhiteSpace(secretKey))
                throw new ArgumentException($"Provided {nameof(secretKey)} is empty", nameof(secretKey));

            return Base32Convert.ToBase32String(Encoding.UTF8.GetBytes(secretKey), false);
        }

        /// <inheritdoc />
        public string GetRegisterUriForQrCode(String secretKey,
                                              String issuer,
                                              String accountName,
                                              bool prefixAccountNameWithIssuer = true)
        {
            if (String.IsNullOrWhiteSpace(secretKey))
                throw new ArgumentException($"Provided {nameof(secretKey)} is empty", nameof(secretKey));
            if (String.IsNullOrWhiteSpace(issuer))
                throw new ArgumentException($"Provided {nameof(issuer)} is empty", nameof(issuer));
            if (String.IsNullOrWhiteSpace(accountName))
                throw new ArgumentException($"Provided {nameof(accountName)} is empty", nameof(accountName));

            if (issuer.Contains(":"))
                throw new ArgumentException($"{nameof(issuer)} contains a colon, which is not allowed", nameof(issuer));

            if (accountName.Contains(":"))
                throw new ArgumentException($"{nameof(accountName)} contains a colon, which is not allowed", nameof(accountName));

            var issuerEscaped = Uri.EscapeDataString(issuer);
            var accountNameEscaped = prefixAccountNameWithIssuer
                                         ? $"{issuerEscaped}:{Uri.EscapeDataString(accountName)}"
                                         : Uri.EscapeDataString(accountName);
            var encodedSecret = GetBase32EncodedSecretKey(secretKey);

            return $"otpauth://totp/{accountNameEscaped}?secret={encodedSecret}&issuer={issuerEscaped}";
        }
    }
}