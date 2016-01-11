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
            int size = (int)Math.Sqrt(stream.Length/4);
            meta.Output = new Bitmap(size, size);
            int x = 0;
            int y = 0;
            int alpha,red,green,blue = 0;
            byte[] buffer = new byte[BufferLength];
            while (y < meta.Output.Height) {
                while (x < meta.Output.Width) {
                    alpha= red = green = blue = 0;
                    stream.Read(buffer, 0, buffer.Length);
                    red     = ((buffer[0] + key[0]) > byte.MaxValue) ? buffer[0] + key[0] - byte.MaxValue : buffer[0] + key[0];
                    green   = ((buffer[1] + key[1]) > byte.MaxValue) ? buffer[1] + key[1] - byte.MaxValue : buffer[1] + key[1];
                    blue    = ((buffer[2] + key[2]) > byte.MaxValue) ? buffer[2] + key[2] - byte.MaxValue : buffer[2] + key[2];
                    for (int i = 0,startIndicator=16; i < buffer.Length; i++,startIndicator/=4) {
                        if (buffer[i] == byte.MaxValue)
                            alpha += startIndicator;
                    }
                    encryptAllBytes.AddRange(buffer);
                    meta.Output.SetPixel(x, y, Color.FromArgb(alpha,red, green, blue));
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

            for (int y = 0; y < map.Height; y++) 
                for (int x = 0; x < map.Width; x++) {
                    Color currentpixel = map.GetPixel(x, y);
                    int currentByte = 0;
                    byte alpha = currentpixel.A;
                    for (int i = 0, maxIndicator = (int)Math.Pow(4, buffer.Length - 1 - i); 
                        i < buffer.Length;
                        i++, maxIndicator = (int)Math.Pow(4, buffer.Length-1-i))
                    {
                        switch (i)
                        {
                            case 0: currentByte = currentpixel.R; break;
                            case 1: currentByte = currentpixel.G; break;
                            case 2: currentByte = currentpixel.B; break;
                        }
                        int tempByte = currentByte - key[i];
                        buffer[i] = (tempByte < 0) ? (byte)(tempByte + (int)byte.MaxValue) : (byte)tempByte;
                        if (buffer[i] == 0 && alpha >= maxIndicator) { 
                            buffer[i] += byte.MaxValue;
                            alpha -= (byte)maxIndicator;
                        }
                    }
                    fileOutput.Write(buffer, 0, BufferLength);
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
