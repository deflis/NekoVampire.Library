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
        private SHA1 sha1;
        protected const int BlockSize = 64;

        public HMACSHA1(Byte[] key)
        {
            sha1 = new SHA1CryptoServiceProvider();
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
        }

        public Byte[] ComputeHash(Byte[] buffer)
        {
            Byte[] keyOpad = (Byte[])KeyValue.Clone();
            Byte[] keyIpad = (Byte[])KeyValue.Clone();
            for (int i = 0; i < BlockSize; i++)
            {
                keyIpad[i] ^= 0x36;
                keyOpad[i] ^= 0x5c;
            }

            Byte[] hash;
            {
                Byte[] inBuf = new Byte[keyIpad.Length + buffer.Length];
                keyIpad.CopyTo(inBuf, 0);
                buffer.CopyTo(inBuf, keyIpad.Length);
                hash = sha1.ComputeHash(inBuf);
            }

            {
                Byte[] outBuf = new Byte[keyOpad.Length + hash.Length];
                keyOpad.CopyTo(outBuf, 0);
                hash.CopyTo(outBuf, keyOpad.Length);

                return sha1.ComputeHash(outBuf);
            }
        }
    }
}
