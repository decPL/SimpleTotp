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
    }
}