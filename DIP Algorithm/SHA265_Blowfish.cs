using System;
using BlowFishCS;
using System.Security.Cryptography;

namespace DIP_Algorithm
{
    class SHA265_Blowfish : Encryption
    {
        
        /**
            Data Row Format
            Data length = 102

            
            
            
        */

        public readonly int BlowFish_KEY_SIZE_DEFAULT = 56;
        public readonly int DATA_ROW_NUMBER = 1;
        public readonly int EXCESS_TOLERANCE = 1;
        public readonly int PIXEL_DATA_SIZE = 4;
        private float percentage;
        private BlowFish blowfish;
        private EncryptionMeta output;
        private long outputLength = -1;
        

        public SHA265_Blowfish(System.IO.Stream source)
        {
            this.key = new byte[BlowFish_KEY_SIZE_DEFAULT];
            System.Buffer.BlockCopy(generateKey(), 0, key, 0, BlowFish_KEY_SIZE_DEFAULT);
            this.Source = source;
            blowfish = new BlowFish(key);
            initializeOutput();
        }

        public SHA265_Blowfish(byte[] keySrc, System.IO.Stream source) {
            keySrc = generateKey(keySrc);
            int length = Math.Min(keySrc.Length, BlowFish_KEY_SIZE_DEFAULT);
            key = new byte[length];
            for (int i=0;i<length;i++)
                key[i] = keySrc[i];
            this.Source = source;
            blowfish = new BlowFish(keySrc);
            initializeOutput();
        }
        private void initializeOutput() {
            output = new EncryptionMeta();
            int size = (int)Math.Sqrt(Source.Length / PIXEL_DATA_SIZE) + EXCESS_TOLERANCE;
            output.Output = new System.Drawing.Bitmap(size, size + DATA_ROW_NUMBER);
            output.Key = GetString(key);
        }

        public override void applyDecryption()
        {

        }

        public override void applyEncryption()
        {
            byte[] buffer = new byte[PIXEL_DATA_SIZE];
            while (true)
            { 
                if (Source.Position >= Source.Length)
                    break;
                Source.Read(buffer, 0, PIXEL_DATA_SIZE);
                addDataToImage(buffer);
                x++;
                if (x >= output.Output.Width)
                {
                    y++;
                    x = 0;
                }
                percentage = Source.Position / Source.Length;
            }
            percentage = 100f;
        }
        private void putDataToDataRow()
        {
            byte[] lengthDataBytes = BitConverter.GetBytes(Source.Length);
            byte[] buffer = new byte[PIXEL_DATA_SIZE];
            for (x = 0; x < lengthDataBytes.Length; x++)
            {
                for(int i=0;i< lengthDataBytes.Length; i++)
                    lengthDataBytes.CopyTo(buffer, i);
                addDataToImage(buffer);
            }
            y++;
            
        }
        private void retrieveDataFromDataRow() {

        }
        
        private int x = 0;
        private int y = 0;

        private void addDataToImage(byte []data) {
            if (data.Length != PIXEL_DATA_SIZE)
                throw new Exception("Too much data");
            this.output.Output.SetPixel(x, y, System.Drawing.Color.FromArgb(data[1], data[2], data[3], data[4]));
        }


        public override byte[] generateKey()
        {
            return (new HMACSHA256()).Key;
        }

        public byte[] generateKey(byte[] predefined) {
            return (new HMACSHA256(predefined)).Key;
        }

        public override double getPercentage()
        {
            return this.percentage;
        }
    }
}
