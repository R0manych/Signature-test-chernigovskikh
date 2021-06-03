using Signature.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Signature.Data
{
    public class BlockManager
    {
        #region Members

        private int _blockId;

        private object _locker = new object();

        #endregion

        public ByteBlock CreateByteBlock(byte[] buffer)
        {
            lock (_locker)
            {
                var block = new ByteBlock(_blockId, buffer);
                _blockId++;
                return block;
            }
        }
    }
}
