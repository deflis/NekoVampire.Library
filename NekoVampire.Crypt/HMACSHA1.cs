using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace NekoVampire.Crypt
{
    public class HMACSHA1
    {
        protected Byte[] KeyValue;
        private SHA1 sha1 = new SHA1CryptoServiceProvider();
        protected const int BlockSize = 64;

        public HMACSHA1(Byte[] key)
        {
            byte [] kv;
            if (key.Length > BlockSize)
            {
                kv = sha1.ComputeHash(key);
            }
            else
            {
                kv = (Byte[])key.Clone();
            }
            KeyValue = new Byte[BlockSize];

            kv.CopyTo(KeyValue, 0);

            if (kv.Length < BlockSize)
            {
                for (int i = kv.Length; i < BlockSize; i++)
                {
                    KeyValue[i] = 0;
                }
            }
        }

        public Byte[] ComputeHash(Byte[] buffer)
        {
            Byte[] keyOpad = new Byte[BlockSize];
            Byte[] keyIpad = new Byte[BlockSize];
            for( int i = 0 ; i < BlockSize ; i++ ) {
                    keyOpad[i]^= 0x36 ;
                    keyIpad[i]^= 0x5c ;
            }
            List<Byte> buf = new List<byte>(keyOpad);
            buf.AddRange(buffer);

            var hash = sha1.ComputeHash(buf.ToArray());
            buf = new List<byte>(hash);
            buf.AddRange(keyIpad);

            return sha1.ComputeHash(buf.ToArray());
        }
    }
}
