using System;
using System.Collections.Generic;
using System.Text;

namespace Signature.Utils.Interface
{
    public interface IEncrypter
    {
        string Encrypt(byte[] block);
    }
}
