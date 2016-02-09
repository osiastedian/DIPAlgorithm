using System;
using System.Drawing;

namespace DIP_Algorithm
{
    public class EncryptionMeta
    {
        public static int DEFAULT_HEIGHT = 100;
        public static int DEFAULT_WIDTH = 100;
        public Bitmap Output { get; set; }
        public String Key { get; set; }
        public EncryptionMeta() {
            Output = new Bitmap(DEFAULT_HEIGHT, DEFAULT_WIDTH);
        }
    }
}
