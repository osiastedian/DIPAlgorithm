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
        private long encryptedLength = -1;
        private long orignalLength = -1;
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
            byte[] encryptedData = new byte[encryptedLength];
            long count = 0;
            while (true)
            {
                if (count >= encryptedLength)
                    break;
                buffer = getDataFromImage(x, y);
                
                addBytes(ref encryptedData, count, buffer, Math.Min(PIXEL_DATA_SIZE, encryptedLength - count));
                count += buffer.Length;
                x++;
                if (x >= Output.Output.Width)
                {
                    y++;
                    x = 0;
                }
                percentage = count / encryptedLength;
            }
            encryptedData = blowfish.Decrypt_CBC(encryptedData);
            destination.Write(encryptedData, 0, Convert.ToInt32(orignalLength));
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
            encryptedLength = encryptedSource.Length;
            putDataToDataRow(encryptedSource.Length,Source.Length);
            long count = 0;
            while (true)
            {
                if (count >= encryptedSource.Length)
                    break;
                buffer = extractBytes(encryptedSource, count , count+PIXEL_DATA_SIZE);
                count += PIXEL_DATA_SIZE;
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
        private void addBytes(ref byte[] array,long start,byte []data,long lengthRecord)
        {
            if (lengthRecord != PIXEL_DATA_SIZE) {
                Console.WriteLine(lengthRecord);
            }
            for(long i = 0; i < lengthRecord && start<array.Length; i++)
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
        private void putDataToDataRow(long encryptedLength,long originalLength)
        {
            byte[] encryptedLengthDataBytes = BitConverter.GetBytes(encryptedLength);
            byte[] originalLengthDataBytes = BitConverter.GetBytes(originalLength);
            string hext = BitConverter.ToString(encryptedLengthDataBytes);
            long test = BitConverter.ToInt64(encryptedLengthDataBytes, 0);
            byte[] buffer = new byte[PIXEL_DATA_SIZE];
            for (int i = 0; i < encryptedLengthDataBytes.Length; )
            {
                buffer[0] = encryptedLengthDataBytes[i++];
                buffer[1] = encryptedLengthDataBytes[i++];
                buffer[2] = encryptedLengthDataBytes[i++];
                buffer[3] = encryptedLengthDataBytes[i++];
                addDataToImage(buffer);
                x++;
            }
            for (int i = 0; i < encryptedLengthDataBytes.Length;)
            {
                buffer[0] = originalLengthDataBytes[i++];
                buffer[1] = originalLengthDataBytes[i++];
                buffer[2] = originalLengthDataBytes[i++];
                buffer[3] = originalLengthDataBytes[i++];
                addDataToImage(buffer);
                x++;
            }
            x = 0;
            y = 1;
            
        }
        private void retrieveDataFromDataRow() {
            x = y = 0;
            byte[] encryptedLengthDataBytes = new byte[8];
            byte[] originalLengthDataBytes = new byte[8];
            getDataFromImage(x++, y).CopyTo(encryptedLengthDataBytes, 0);
            getDataFromImage(x++, y).CopyTo(encryptedLengthDataBytes, 4);
            getDataFromImage(x++, y).CopyTo(originalLengthDataBytes, 0);
            getDataFromImage(x++, y).CopyTo(originalLengthDataBytes, 4);

            encryptedLength = BitConverter.ToInt64(encryptedLengthDataBytes, 0);
            orignalLength = BitConverter.ToInt64(originalLengthDataBytes, 0); 
            x = 0;
            y = 1;
        }
        
        private int x = 0;
        private int y = 0;

        private byte[] getDataFromImage(int x, int y) {
            byte[] data = new byte[PIXEL_DATA_SIZE];
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
