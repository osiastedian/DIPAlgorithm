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
        public  const int KEY_DATA_ROW_HEIGHT = 1;
        private readonly short PIXEL_DATA_SIZE = 4;

        private Dictionary<int, MyPoint> keyMap;
        private Stream source;
        private Bitmap keyBitmap;
        private Bitmap dataBitmap;
        int key_X = 0;
        int key_Y = 0;
        int data_X = 0;
        int data_Y = 0;
        public float Percentage { get; set; }
        public new EncryptionMeta Output { get; set; }
        Random random;


        public OSIASEncryption(Stream source) 
        {
            this.source = source;
            keyMap = new Dictionary<int, MyPoint>(byte.MaxValue);

        }

        public override void applyDecryption()
        {
            
            
        }
        
        public override void applyEncryption()
        {
            keyBitmap = new Bitmap(KEY_WIDTH, KEY_HEIGHT + KEY_DATA_ROW_HEIGHT);
            putDataRow(source.Length, ref keyBitmap);
            int size = (int)Math.Sqrt(source.Length / 4) + 1;
            dataBitmap = new Bitmap(size, size);
            this.random = new Random();
            
            if (source!=null)
            {
                byte[] buffer = new byte[4];
                while(true)
                {
                    int limit = Math.Min((int)(source.Length - source.Position), 4);
                    source.Read(buffer, 0, buffer.Length);
                    for(int i=0;i< limit;i++)
                        buffer[i] = generatePosition(buffer[i]);
                    dataBitmap.SetPixel(data_X++, data_Y, Color.FromArgb(buffer[0], buffer[1], buffer[2], buffer[3]));
                    if (source.Position >= source.Length)
                        break;
                    if (source.Position >= source.Length -10)
                        continue;
                    if (data_X >= dataBitmap.Width) {
                        data_Y++;
                        data_X = 0;
                    }
                    Percentage = source.Position / source.Length;
                }
            }
            runChecker();
            this.Output = new EncryptionMeta(dataBitmap, keyBitmap);

        }
        
        private byte generatePosition(int data)
        {
            byte result = 0;
            MyPoint p = new MyPoint(-1,-1);
            if (keyMap.ContainsKey(data))
            {
                p = keyMap[data];
            }
            if(p.X == -1 || p.Y == -1)
            {
                bool operation = true;
                do
                {
                    p.X = random.Next(keyBitmap.Width);
                    p.Y = random.Next(KEY_DATA_ROW_HEIGHT,keyBitmap.Height);
                    operation = keyMap.ContainsValue(p);
                } while (operation);

                if (key_X >=keyBitmap.Width)
                {
                    key_X = 0;
                    key_Y++;
                }
                keyMap[data] = p;
                keyBitmap.SetPixel(p.X, p.Y, Color.FromArgb(data, random.Next()%byte.MaxValue, random.Next() % byte.MaxValue, random.Next() % byte.MaxValue));
            }
            result = (byte)(p.X * 16 + p.Y);
            return result;
        }

        private void runChecker() {
            MyPoint temp = new MyPoint(0, 0);
            for (int x = 0; x < 16; x++)
                for (int y = 0; y < 16; y++)
                {
                    temp.X = x;
                    temp.Y = y;
                    if (!keyMap.ContainsValue(temp)) {
                        break;
                    }
                }
        }

        private void putDataRow(long sourceLength,ref Bitmap keyMap)
        {
            byte[] sourceLengthBytes = BitConverter.GetBytes(sourceLength);
            for(int i=0;i<sourceLengthBytes.Length/4;i++)
            {
                byte alpha  = sourceLengthBytes[i * 4 + 0];
                byte red    = sourceLengthBytes[i * 4 + 1];
                byte green  = sourceLengthBytes[i * 4 + 2];
                byte blue   = sourceLengthBytes[i * 4 + 3];
                keyMap.SetPixel(key_X++, 0, Color.FromArgb(alpha, red, green, blue));
            }
        }

        private void retrieveDataRow(ref Bitmap keyMap)
        {
            byte[] sourceLengthBytes = BitConverter.GetBytes((long)0);
            for (int i = 0; i < sourceLengthBytes.Length / 4; i++)
            {
                Color c = keyMap.GetPixel(key_X++, 0);
                sourceLengthBytes[i * 4 + 0] = c.A;
                sourceLengthBytes[i * 4 + 1] = c.R;
                sourceLengthBytes[i * 4 + 2] = c.G;
                sourceLengthBytes[i * 4 + 3] = c.B;
            }
            
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


        

        private class MyPoint {
            public int X { get; set; }
            public int Y { get; set; }
            //public byte Occurence { get; set; }

            public MyPoint(int x, int y)
            {
                X = x;
                Y = y;
               // Occurence = 0;
            }
            public override bool Equals(object obj)
            {
                if (obj is MyPoint)
                {
                    MyPoint param = (MyPoint)obj;
                    if (param.X == this.X && param.Y == this.Y)
                        return true;
                    else return false;
                }
                else return false;
            }
            public override string ToString()
            {
                return "{ X:" + this.X + " Y:" + this.Y + "}";
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
