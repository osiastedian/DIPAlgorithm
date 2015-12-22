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
            int size = (int)Math.Sqrt(stream.Length/3);
            meta.Output = new Bitmap(size, size);
            int x = 0;
            int y = 0;
            int red,green,blue = 0;
            byte[] buffer = new byte[3];

            while (y < meta.Output.Height) {
                while (x < meta.Output.Width) {
                    red = green = blue = 0;
                    stream.Read(buffer, 0, buffer.Length);
                    stream.Position += buffer.Length;
                    red = buffer[0];
                    green = buffer[1];
                    blue = buffer[2];
                    meta.Output.SetPixel(x, y, Color.FromArgb(red, green, blue));
                    /// UPDATES
                    Percentage = ((double)(meta.Output.Width*y)+x)*3 / stream.Length;
                    x += 3;
                }
                x = 0;
                y++;

            }
            Output =  meta;
        }
        // TODO: Edit Encryption Method.
        public override double getPercentage()
        {
            return this.Percentage;
        }
    }
}
