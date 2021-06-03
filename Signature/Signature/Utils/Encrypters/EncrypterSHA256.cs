using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using Signature.Utils.Interface;

namespace Signature
{
    public class EncrypterSHA256 : IEncrypter
    {
        #region Members

        private readonly SHA256 _sha256 = SHA256.Create();

        #endregion

        #region Methods

        public string Encrypt(byte[] block)
        {           
            return ConvertHashToString(_sha256.ComputeHash(block));
        }

        private string ConvertHashToString(byte[] hashValue)
        {
            var resultString = new StringBuilder();
            for (int i = 0; i < hashValue.Length; i++)
            {
                resultString.Append(hashValue[i].ToString("x2"));
            }
            return resultString.ToString();
        }

        #endregion
    }
}
