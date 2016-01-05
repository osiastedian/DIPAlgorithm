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
        public static int BufferLength = 4;
        Stream stream;
        Bitmap map;
        public long i = 0;
        public CaesarsCipherEncryption(Stream stream, Bitmap map) {
            this.stream = stream;
               this.map = map;
        }
        public override void applyEncryption()
        {
            Percentage = 0;
            EncryptionMeta meta = new EncryptionMeta();
            byte[] key = generateKey();
            int size = (int)Math.Sqrt(stream.Length/4);
            meta.Output = new Bitmap(size, size);
            int x = 0;
            int y = 0;
            int alpha,red,green,blue = 0;
            byte[] buffer = new byte[BufferLength];
            String s = "";
            while (y < meta.Output.Height) {
                while (x < meta.Output.Width) {
                    red = green = blue = 0;
                    stream.Read(buffer, 0, buffer.Length);
                    alpha   = (buffer[0] + key[0]) %byte.MaxValue;
                    red     = (buffer[1] + key[1]) % byte.MaxValue;
                    green   = (buffer[2] + key[2]) % byte.MaxValue;
                    blue    = (buffer[3] + key[3]) % byte.MaxValue;
                    meta.Output.SetPixel(x, y, Color.FromArgb(alpha,red, green, blue));
                    // FOR TESTING PURPOSES {
                    s += (char)alpha;
                    s += (char)red;
                    s += (char)green;
                    s += (char)blue;
                    // }
                    /// UPDATES
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
            throw new NotImplementedException();
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
