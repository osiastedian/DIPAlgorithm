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
        public EncryptionMeta Output { get; set; }
        Stream stream;
        Bitmap map;
        public long i = 0;
        public CaesarsCipherEncryption(Stream stream, Bitmap map) {
            this.stream = stream;
               this.map = map;
        }
        public void applyEncryption()
        {
            Percentage = 0;
            EncryptionMeta meta = new EncryptionMeta();
            StreamReader reader = new StreamReader(stream);
            int test = (int)Math.Sqrt(stream.Length)+1;
            long length = stream.Length;
            meta.Output = new Bitmap(test, test);
            int currentByte = 0;
            int x = 0;
            int y = 0;
            int red,green,blue = 0;
            
            for (; i < stream.Length; i++) {
                currentByte = reader.Read();
                x = (int)(i % meta.Output.Width);
                y = (int)(i / meta.Output.Width);
                red = green = blue = 0; 
                switch (i % 3) {
                    case 0: red = currentByte; break;
                    case 1: green = currentByte; break;
                    case 2: blue = currentByte; break;
                }
                byte[] bytes = BitConverter.GetBytes(currentByte);
                meta.Output.SetPixel(x, y, Color.FromArgb(bytes[0], bytes[1], bytes[2]));
                Percentage = i / stream.Length;
            }

            Output =  meta;
        }
    }
}
