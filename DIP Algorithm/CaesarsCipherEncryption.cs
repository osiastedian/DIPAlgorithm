using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIP_Algorithm
{
    class CaesarsCipherEncryption : Encryption
    {
        public double Percentage { get; set; }
        public static int BufferLength = 3;
        Stream stream;
        Bitmap map;
        public long i = 0;
        public static List<byte> encryptAllBytes = new List<byte>();
        public CaesarsCipherEncryption(Stream stream, Bitmap map)
        {
            this.stream = stream;
            this.map = map;
        }
        
        public override void applyEncryption()
        {
            Percentage = 0;
            EncryptionMeta meta = new EncryptionMeta();
            byte[] key = generateKey();
            string keyStr = "" + (char)key[0] + (char)key[1] + (char)key[2] 
                //+ (char)key[3]
                ;
            int size = (int)Math.Sqrt(stream.Length/3)+1;
            // Problem: Size is not accurate relative to the stream length causes extra data to be encoded during
            // decryption and will result to the data being corrupted.
            // Solution: 00010101 (Current format) change to 00 11 01 11
            //                                               127 64, 32 16, 8 4, 2 1   
            //                                                  R  G  B       R and B Contains data G does not
            //                                                  FM FM FM      F = Flag if is data M = maximum 255 value

            meta.Output = new Bitmap(size, size);
            int x = 0;
            int y = 0;
            int[] colorsARGB = new int[4];
            byte[] buffer = new byte[BufferLength];
            long streamLength = stream.Length;
            int recordLimit = 3;
            bool stopEncryption = false;
            while (y < meta.Output.Height && stopEncryption==false) {
                if (stream.Position > stream.Length)
                    break;
                while (x < meta.Output.Width && stopEncryption == false) {
                    colorsARGB[0]= colorsARGB[1]= colorsARGB[2]= colorsARGB[3] = 0;
                    stream.Read(buffer, 0, buffer.Length);
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
                    Percentage = ((double)(meta.Output.Width*y)+x)* buffer.Length / stream.Length;
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
            Bitmap map = this.Output.Output;
            string keyStr = this.Output.Key;
            byte[] key = new byte[keyStr.Length];
            for (int i = 0; i < key.Length; i++) {
                key[i] = (byte)keyStr.ElementAt(i);
            }
            Stream fileOutput = new FileStream("C:\\Users\\osias\\Desktop\\testfileoutput",FileMode.OpenOrCreate);
            byte[] buffer = new byte[BufferLength];
            //List<byte> allBytes = new List<byte>(); /*Used during testing 
            bool stopDecrypt = false;
            for (int y = 0; y < map.Height && stopDecrypt == false ; y++) 
                for (int x = 0; x < map.Width && stopDecrypt == false; x++) {
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
                        if (alpha >= dataFlag) {
                            recordLength++;
                            alpha -= dataFlag;
                        }

                        if (buffer[i] == 0 && alpha >= maxIndicator) { 
                            buffer[i] += byte.MaxValue;
                            alpha -= maxIndicator;
                        }
                    }
                    if (recordLength != BufferLength)
                        stopDecrypt = true;
                    fileOutput.Write(buffer, 0, recordLength);
                    /*Used during testing 
                    allBytes.AddRange(buffer);
                    if (allBytes.Count > 916) {
                        int test = 00;
                    }*/
                }
            /* 
                    Used during testing
            for (int i=0;i<encryptAllBytes.Count;i++) {
                if (encryptAllBytes[i] != allBytes[i])
                {
                    byte e = encryptAllBytes[i];
                    byte d = allBytes[i];
                    throw new Exception("Not equal");
                }
            }
            */
            fileOutput.Close();
            
        }

        public override byte[] generateKey()
        {
            byte[] result = new byte[BufferLength];
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
