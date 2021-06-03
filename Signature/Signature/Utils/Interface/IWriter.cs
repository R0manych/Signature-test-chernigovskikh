using Signature.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Signature.Utils.Interface
{
    public interface IWriter
    {
        void Write(ByteBlock byteBlock) => Console.WriteLine($"{byteBlock.ID} - {byteBlock.Hash}");
    }
}
