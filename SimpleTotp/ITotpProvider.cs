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
        RegistrationData GetAuthenticatorRegistrationData(String accountName,
                                       String issuer,
                                                           String secretKey = null,
                                       bool prefixAccountNameWithIssuer = true);
    }
}