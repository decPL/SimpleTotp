using System;

namespace SimpleTotp
{
    /// <summary>
    /// Information required for registering a secret Key
    /// </summary>
    public class RegistrationData
    {
        /// <summary>
        /// User's secret TOTP key
        /// <remarks>
        /// This is the unencoded secret key that will subsequently be used to validate user TOTP codes.
        /// Because the key is actually needed to verify the user's code, only reversible encryption can be applied when persisting it.
        /// </remarks> 
        /// </summary>
        public String SecretKey { get; set; }

        /// <summary>
        /// Base32-encoded format of the user's secret TOTP key
        /// <remarks>
        /// Most authenticators will require the user's TOTP key to be manually input in Base32.
        /// </remarks>
        /// </summary>
        public String ManualRegistrationKey { get; set; }

        /// <summary>
        /// Issuer of the TOTP secret key
        /// </summary>
        public String Issuer { get; set; }

        /// <summary>
        /// Name of the account for which the secret key was generated
        /// </summary>
        public String AccountName { get; set; }

        /// <summary>
        /// Uri to be encoded in a QR code for automatic authenticator registration
        /// </summary>
        public String QrCodeUri { get; set; }
    }
}