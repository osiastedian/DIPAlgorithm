using System;
using System.Collections.Generic;

using System.IO;
using System.Drawing;
using System.Collections;

namespace DIP_Algorithm
{
    class OSIASEncryption : Encryption
    {
        public  const int KEY_WIDTH = 16;
        public  const int KEY_HEIGHT = 16;
        private Dictionary<int, Point> keyMap;
        private Stream source;
        private Bitmap keyBitmap;
        private Bitmap dataBitmap;
        private long x;
        private long y;
        private readonly short PIXEL_DATA_SIZE = 4;
        public float Percentage { get; set; }
        public new EncryptionMeta Output { get; set; }


        public OSIASEncryption(Stream source) 
        {
            this.source = source;
            keyMap = new Dictionary<int, Point>(byte.MaxValue);

        }

        public override void applyDecryption()
        {
            
            
        }

       

        private static Bitmap generateKeyBitmap(Dictionary<int,Point> map)
        {
            Bitmap b = null; 

            return b;
        }
        int key_X = 0;
        int key_Y = 0;
        int data_X = 0;
        int data_Y = 0;
        public override void applyEncryption()
        {
            keyBitmap = new Bitmap(KEY_WIDTH, KEY_HEIGHT);
            int size = (int)Math.Sqrt(source.Length / 2) + 1;
            dataBitmap = new Bitmap(size, size);
            if (source!=null)
            {
                byte[] buffer = new byte[4];
                while(true)
                {
                    int limit = Math.Min((int)(source.Length - source.Position), 4);
                    source.Read(buffer, 0, buffer.Length);
                    for(int i=0;i< limit;i++)
                        buffer[i] = generatePosition(buffer[i]);
                    dataBitmap.SetPixel(data_X, data_Y, Color.FromArgb(buffer[0], buffer[1], buffer[2], buffer[3]));
                    if (source.Position >= source.Length)
                        break;
                    Percentage = source.Position / source.Length;
                }
            }

        }
        private byte generatePosition(int data)
        {
            byte result = 0;
            Point p = new Point(-1,-1);
            if (keyMap.ContainsKey(data))
            {
                p = keyMap[data];
            }
            if(p.X == -1 || p.Y == -1)
            {
                p = new Point(key_X++, key_Y);
                if(key_X >=keyBitmap.Width)
                {
                    key_X = 0;
                    key_Y++;
                }
                keyMap[data] = p;
            }
            keyBitmap.SetPixel(p.X, p.Y, Color.FromArgb(data, byte.MaxValue, byte.MaxValue, byte.MaxValue));
            result = (byte)(p.X * 16 + p.Y);
            return result;
        }

        private void putData(long sourceLength)
        {
            byte[] sourceBytes = BitConverter.GetBytes(sourceLength);
            
            
        }
        private void decryptPixelData(long sourceLength)
        {

        }
        public override byte[] generateKey()
        {
            throw new NotImplementedException();
        }

        public override double getPercentage()
        {
            return Percentage;
        }


        

        private class LocationValue {
            public int X { get; set; }
            public int Y { get; set; }
            public byte Occurence { get; set; }

            public LocationValue(int x, int y)
            {
                X = x;
                Y = y;
                Occurence = 0;
            }

        }

        public class EncryptionMeta {
            public Bitmap Output{ get; set; }
            public Bitmap Key { get; set; }
            public EncryptionMeta(Bitmap output,Bitmap key)
            {
                Output = output;
                Key = key;
            }
        }
    }
}
