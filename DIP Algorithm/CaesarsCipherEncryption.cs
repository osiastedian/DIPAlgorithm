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
            byte[] key = new byte[BufferLength];
            // KEY GENERATION


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
                    alpha   = buffer[0];
                    red     = buffer[1];
                    green   = buffer[2];
                    blue    = buffer[3];
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
    }
}
