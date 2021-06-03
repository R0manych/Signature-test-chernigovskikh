using System;
using System.Collections.Generic;
using System.Text;

namespace Signature.Data.Models
{
    public class ByteBlock
    {
        #region Properties

        public int ID { get; }
        public byte[] Buffer { get; }
        public string Hash { get; set; }

        #endregion

        #region Constructors

        public ByteBlock(int id, byte[] buffer) : this(id, buffer, "")
        {

        }

        public ByteBlock(int id, byte[] buffer, string hash)
        {
            ID = id;
            Buffer = buffer;
            Hash = hash;
        }

        #endregion
    }
}
