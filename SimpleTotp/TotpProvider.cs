using System;
using System.Text;

namespace SimpleTotp
{
    /// <inheritdoc />
    public class TotpProvider : ITotpProvider
    {
        /// <inheritdoc />
        public String GetBase32EncodedSecretKey(String secretKey)
            => Base32Convert.ToBase32String(Encoding.UTF8.GetBytes(secretKey), false);
    }
}