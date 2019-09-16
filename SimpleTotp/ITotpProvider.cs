using System;
using JetBrains.Annotations;

namespace SimpleTotp
{
    /// <summary>
    /// Simple TOTP algorithm provider
    /// </summary>
    [PublicAPI]
    public interface ITotpProvider
    {
        /// <summary>
        /// Returns the secret key encoded using the Base32 encoding
        /// </summary>
        /// <param name="secretKey">Secret key</param>
        /// <returns>Secret key encoded using the Base32 encoding</returns>
        String GetBase32EncodedSecretKey(String secretKey);

        /// <summary>
        /// Returns the secret key Uri to be displayed as a QR code for authenticator registration
        /// </summary>
        /// <param name="secretKey">Secret key</param>
        /// <param name="issuer">Secret key issuer</param>
        /// <param name="accountName">Account name, for which the secret was generated</param>
        /// <param name="prefixAccountNameWithIssuer">
        /// True if account name should be prefixed with issuer, false otherwise.
        /// Default - true (recommended).
        /// </param>
        /// <returns>Secret key Uri to be displayed as a QR code for authenticator registration</returns>
        String GetRegisterUriForQrCode(String secretKey,
                                       String issuer,
                                       String accountName,
                                       bool prefixAccountNameWithIssuer = true);
    }
}