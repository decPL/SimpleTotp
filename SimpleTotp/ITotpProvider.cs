using System;

namespace SimpleTotp
{
    /// <summary>
    /// Simple TOTP algorithm provider
    /// </summary>
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
        String GetCodeAtSpecificTime(String secretKey, DateTimeOffset time);

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
        String GetCodeAtSpecificTime(String secretKey, DateTimeOffset time, out TimeSpan remaining);
    }
}