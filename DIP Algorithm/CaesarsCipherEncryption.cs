using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace DIP_Algorithm
{
    class CaesarsCipherEncryption : Encryption
    {
        public static int DEFAUL_KEY_LENGTH = 8;
        public double Percentage { get; set; }
        public static int BufferLength = 3;
        public long i = 0;
        public static List<byte> encryptAllBytes = new List<byte>();
        private Stream fileOutput;
        private string destinationFolder;
        public string DestinationFolder
        {
            get { return destinationFolder; }
            set {
                this.destinationFolder = value;
                if (!Directory.Exists(destinationFolder))
                    throw new Exception("Directory invalid");
            }
        }


        public CaesarsCipherEncryption(Stream stream, Bitmap map)
        {
            this.Source = stream;
        }
        public CaesarsCipherEncryption(Stream stream, Bitmap map,string key)
        {
            this.Source = stream;
            this.key = new byte[key.Length];
            for (int i = 0; i < this.key.Length; i++)
            {
                this.key[i] = (byte)key[i];
            }
        }

        private string byteToString(byte[] bytes)
        {
            string result = "";
            foreach (byte b in bytes)
            {
                result += (char)b;
            }
            return result;
        }

        public override void applyEncryption()
        {
            // Problem: Size is not accurate relative to the stream length causes extra data to be encoded during
            // decryption and will result to the data being corrupted.
            // Solution: 00010101 (Current format) change to 00 11 01 11
            //                                               127 64, 32 16, 8 4, 2 1   
            //                                                  R  G  B       R and B Contains data G does not
            //  
            Percentage = 0;
            EncryptionMeta meta = new EncryptionMeta();
            if(key == null ||  key.Length==0)
             key  = generateKey();
            int size = (int)Math.Sqrt(Source.Length/3)+1;
            meta.Key = byteToString(key);
            meta.Output = new Bitmap(size, size);
            int x = 0;
            int y = 0;
            int[] colorsARGB = new int[4];
            byte[] buffer = new byte[BufferLength];
            long streamLength = Source.Length;
            int recordLimit = 3;
            bool stopEncryption = false;
            while (y < meta.Output.Height && stopEncryption==false) {
                if (Source.Position > Source.Length)
                    break;
                while (x < meta.Output.Width && stopEncryption == false) {
                    colorsARGB[0]= colorsARGB[1]= colorsARGB[2]= colorsARGB[3] = 0;
                    Source.Read(buffer, 0, buffer.Length);
                    if (streamLength  < buffer.Length) { 
                        recordLimit = (int)streamLength;
                        stopEncryption = true;
                    }
                    else 
                        streamLength -= buffer.Length;
                    for (
                            int i = 0, maxIndicator = 16, dataFlag = 32;
                            i < recordLimit;
                            i++, maxIndicator /= 4, dataFlag /= 4
                       )
                    { 
                        colorsARGB[i] = ((buffer[i] + key[i]) > byte.MaxValue) ? buffer[i] + key[i] - byte.MaxValue : buffer[i] + key[i];
                        if (buffer[i] == byte.MaxValue)
                            colorsARGB[3] += maxIndicator;
                        colorsARGB[3] += dataFlag;
                    }
                    meta.Output.SetPixel(x, y, Color.FromArgb(colorsARGB[3], colorsARGB[0], colorsARGB[1], colorsARGB[2]));
                    Percentage = ((double)(meta.Output.Width*y)+x)* buffer.Length / Source.Length;
                    x++;
                }
                x = 0;
                y++;
            }
            Output =  meta;
        }
        
        


        public override double getPercentage()
        {
            return this.Percentage;
        }

        
        public override void applyDecryption()
        {
            Bitmap map = Output.Output;
            byte[] key = GetBytes(Output.Key);
            fileOutput = new FileStream(this.destinationFolder+"\\"+Encryption.GetString(key), FileMode.OpenOrCreate);
            byte[] buffer = new byte[BufferLength];
            bool stopDecrypt = false;
            for (int y = 0; y < map.Height && stopDecrypt == false ; y++) 
                for (int x = 0; x < map.Width && stopDecrypt == false; x++)
                {
                    Color currentpixel = map.GetPixel(x, y);
                    int currentByte = 0;
                    int alpha = currentpixel.A;
                    int recordLength = 0;
                    for (int i = 0, maxIndicator = (int)Math.Pow(4, buffer.Length - 1 - i),dataFlag = 32; 
                        i < buffer.Length;
                        i++, maxIndicator = (int)Math.Pow(4, buffer.Length-1-i),dataFlag/=4)
                        {
                            switch (i)
                                {
                                    case 0: currentByte = currentpixel.R; break;
                                    case 1: currentByte = currentpixel.G; break;
                                    case 2: currentByte = currentpixel.B; break;
                                }
                            int tempByte = currentByte - key[i];
                            buffer[i] = (tempByte < 0) ? (byte)(tempByte + (int)byte.MaxValue) : (byte)tempByte;
                            if (alpha >= dataFlag)
                            {
                                recordLength++;
                                alpha -= dataFlag;
                            }
                            if (buffer[i] == 0 && alpha >= maxIndicator)
                            { 
                                buffer[i] += byte.MaxValue;
                                alpha -= maxIndicator;
                            }
                        }
                    if (recordLength != BufferLength)
                        stopDecrypt = true;
                    fileOutput.Write(buffer, 0, recordLength);
                }
            fileOutput.Close();
            
        }

        public override byte[] generateKey()
        {
            byte[] result = new byte[DEFAUL_KEY_LENGTH];
            Random rn = new Random();
            for (int i = 0; i < BufferLength; i++)
            {
                int rand = -1;
                while (!char.IsLetterOrDigit((char)rand)) {
                    rand = rn.Next(0, byte.MaxValue);
                };
                result[i] = (byte) rand ;
            }
            return result;
        }
    }
}
