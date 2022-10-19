using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CloudNativeWeb.Common
{
    public class EncryptionHelper
    {
        private byte[] Skey = ASCIIEncoding.ASCII.GetBytes("abc01def");

        public string Encrypt(string p_data)
        {
            //RC2 cryptoServiceProvider = new RC2CryptoServiceProvider();
            DESCryptoServiceProvider cryptoServiceProvider = new DESCryptoServiceProvider();

            cryptoServiceProvider.Padding = PaddingMode.PKCS7;
            cryptoServiceProvider.Mode = CipherMode.CBC;

            cryptoServiceProvider.Key = Skey;
            cryptoServiceProvider.IV = Skey;

            MemoryStream ms = new MemoryStream();
            CryptoStream cryStream = new CryptoStream(ms, cryptoServiceProvider.CreateEncryptor(), CryptoStreamMode.Write);

            byte[] data = Encoding.UTF8.GetBytes(p_data.ToCharArray());

            cryStream.Write(data, 0, data.Length);
            cryStream.FlushFinalBlock();

            return Convert.ToBase64String(ms.ToArray());
        }

        public string Decrypt(string p_data)
        {
            string result = null;

            //RC2 cryptoServiceProvider = new RC2CryptoServiceProvider();
            DESCryptoServiceProvider cryptoServiceProvider = new DESCryptoServiceProvider();

            cryptoServiceProvider.Padding = PaddingMode.PKCS7;
            cryptoServiceProvider.Mode = CipherMode.CBC;

            cryptoServiceProvider.Key = Skey;
            cryptoServiceProvider.IV = Skey;

            MemoryStream ms = new MemoryStream();
            CryptoStream cryStream = new CryptoStream(ms, cryptoServiceProvider.CreateDecryptor(), CryptoStreamMode.Write);

            byte[] data = Convert.FromBase64String(p_data);

            cryStream.Write(data, 0, data.Length);
            cryStream.FlushFinalBlock();

            result = Encoding.UTF8.GetString(ms.GetBuffer());
            if (string.IsNullOrEmpty(result) == false)
            {
                result = result.Replace('\0'.ToString(), string.Empty);
            }

            return result;
        }
    }
}
