using System;
using System.Drawing;
using System.IO;
using BlowFishCS;
using System.Security.Cryptography;

namespace DIP_Algorithm
{
    class SHA256_Blowfish : Encryption
    {
        
        public readonly int BlowFish_KEY_SIZE_DEFAULT = 56;
        private readonly byte[] BlowFish_IV_DEFAULT = {(byte)'I', (byte)'A', (byte)'N', (byte)'O', (byte)'S', (byte)'I', (byte)'A', (byte)'S' };
        public readonly int DATA_ROW_NUMBER = 1;
        public readonly int EXCESS_TOLERANCE = 1;
        public readonly int PIXEL_DATA_SIZE = 4;
        private float percentage;
        private BlowFish blowfish;
        private long outputLength = -1;
        private string destinationFolder;
        public string DestinationFolder
        {
            get { return destinationFolder; }
            set
            {
                this.destinationFolder = value;
                if (!Directory.Exists(destinationFolder))
                    throw new Exception("Directory invalid");
            }
        }


        public SHA256_Blowfish(Stream source)
        {
            key = new byte[BlowFish_KEY_SIZE_DEFAULT];
            System.Buffer.BlockCopy(generateKey(), 0, key, 0, BlowFish_KEY_SIZE_DEFAULT);
            this.Source = source;
            blowfish = new BlowFish(key);
            blowfish.IV = BlowFish_IV_DEFAULT;
            initializeOutput();
            this.Output.Key = BitConverter.ToString(key).Replace("-", "");
        }

        public SHA256_Blowfish(byte[] keySrc, Stream source) {
            keySrc = generateKey(keySrc);
            int length = Math.Min(keySrc.Length, BlowFish_KEY_SIZE_DEFAULT);
            key = new byte[length];
            keySrc.CopyTo(key, 0);
            this.Source = source;
            blowfish = new BlowFish(keySrc);
            blowfish.IV = BlowFish_IV_DEFAULT;
            initializeOutput();
        }
        private void initializeOutput() {
            if (Source != null) { 
                Output = new EncryptionMeta();
                int size = (int)Math.Sqrt(Source.Length / PIXEL_DATA_SIZE) + EXCESS_TOLERANCE;
                Output.Output = new Bitmap(size, size + DATA_ROW_NUMBER);
                Output.Key = GetString(key);
            }
        }

        public override void applyDecryption()
        {
            if (Output.Output == null)
                throw new Exception("No Bitmap to decrypt");
            retrieveDataFromDataRow();
            Stream destination = new FileStream(destinationFolder + "\\test", FileMode.OpenOrCreate);
            byte[] buffer = new byte[PIXEL_DATA_SIZE];
            byte[] encryptedData = new byte[outputLength];
            long count = 0;
            while (true)
            {
                if (count >= outputLength)
                    break;
                buffer = getDataFromImage(x, y);
                addBytes(ref encryptedData, count, buffer);
                count += buffer.Length;
                x++;
                if (x >= Output.Output.Width)
                {
                    y++;
                    x = 0;
                }
                percentage = count / outputLength;
            }
            encryptedData = blowfish.Decrypt_CBC(encryptedData);
            destination.Write(encryptedData, 0, encryptedData.Length);
            percentage = 1;
            destination.Close();
        }

        public override void applyEncryption()
        {
            byte[] buffer = new byte[PIXEL_DATA_SIZE];
            byte[] encryptedSource = new byte[Source.Length];
            Source.Read(encryptedSource, 0, encryptedSource.Length);
            encryptedSource = blowfish.Encrypt_CBC(encryptedSource);
            byte[] decrypted = blowfish.Decrypt_CBC(encryptedSource);
            outputLength = encryptedSource.Length;
            putDataToDataRow(encryptedSource.Length);
            long position = 0;
            while (true)
            {
                if (position >= encryptedSource.Length)
                    break;
                buffer = extractBytes(encryptedSource, position , position+4);
                position += 4;
                addDataToImage(buffer);
                x++;
                if (x >= Output.Output.Width)
                {
                    y++;
                    x = 0;
                }
                percentage = Source.Position / Source.Length;
            }
            percentage = 1;
        }
        private void addBytes(ref byte[] array,long start,byte []data)
        {
            for(long i = 0; i < data.Length && start<array.Length; i++)
            {
                array[start++] = data[i];
            }
        }
        private byte[] extractBytes(byte[] array,long start,long end)
        {
            byte[] result = new byte[end-start];
            for(long i=0;i< result.Length;i++)
            {
                result[i] = array[start++];
            }
            return result;

        }
        private void putDataToDataRow(long length)
        {
            byte[] lengthDataBytes = BitConverter.GetBytes(length);
            string hext = BitConverter.ToString(lengthDataBytes);
            long test = BitConverter.ToInt64(lengthDataBytes, 0);
            byte[] buffer = new byte[PIXEL_DATA_SIZE];
            for (int i = 0; i < lengthDataBytes.Length; )
            {
                buffer[0] = lengthDataBytes[i++];
                buffer[1] = lengthDataBytes[i++];
                buffer[2] = lengthDataBytes[i++];
                buffer[3] = lengthDataBytes[i++];
                addDataToImage(buffer);
                x++;
            }
            x = 0;
            y = 1;
            
        }
        private void retrieveDataFromDataRow() {
            x = y = 0;
            byte[] lengthDataBytes = new byte[8];
            getDataFromImage(x++, y).CopyTo(lengthDataBytes, 0);
            getDataFromImage(x, y).CopyTo(lengthDataBytes, 4);

            outputLength = BitConverter.ToInt64(lengthDataBytes, 0);
            x = 0;
            y = 1;
        }
        
        private int x = 0;
        private int y = 0;

        private byte[] getDataFromImage(int x, int y) {
            byte[] data = new byte[4];
            Color color = this.Output.Output.GetPixel(x, y);
            data[0] = color.A;
            data[1] = color.R;
            data[2] = color.G;
            data[3] = color.B;
            return data;
        }

        private void addDataToImage(byte []data) {
            if (data.Length != PIXEL_DATA_SIZE)
                throw new Exception("Too much data");
            this.Output.Output.SetPixel(x, y, System.Drawing.Color.FromArgb(data[0], data[1], data[2], data[3]));
            Color color = this.Output.Output.GetPixel(x, y);
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
