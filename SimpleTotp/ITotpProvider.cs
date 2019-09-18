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
        bool ValidateCode(String secretKey, String code, DateTimeOffset time);

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
        bool ValidateCode(String secretKey, String code, DateTimeOffset time, TimeSpan tolerance);

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
        bool ValidateCode(String secretKey,
                          String code,
                          DateTimeOffset time,
                          TimeSpan pastTolerance,
                          TimeSpan futureTolerance);
    }
}