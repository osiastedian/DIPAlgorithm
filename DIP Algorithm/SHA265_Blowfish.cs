using System;
using BlowFishCS;
using System.Security.Cryptography;

namespace DIP_Algorithm
{
    class SHA265_Blowfish : Encryption
    {
        BlowFish blowfish;
        public override void applyDecryption()
        {
            throw new NotImplementedException();
        }

        public override void applyEncryption()
        {
            throw new NotImplementedException();
        }

        public override byte[] generateKey()
        {
            return (new HMACSHA256()).Key;
        }

        public override double getPercentage()
        {
            throw new NotImplementedException();
        }
    }
}
