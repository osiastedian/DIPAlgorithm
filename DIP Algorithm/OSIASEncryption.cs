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

        private Dictionary<int, MyPoint> encryptionKeyMap;
        private Dictionary<MyPoint,byte > decryptionKeyMap;
        public string destinationFolder = null;
        private Stream source;
        private Bitmap keyBitmap; 
        private Bitmap dataBitmap;

        private MyPoint decryptionPointTemp;
        public new Stream Source { get { return this.source; } set { this.source = value; } }

        public Bitmap KeyBitMap {
            get
            {
                return this.keyBitmap;
            }
            set
            {
                this.keyBitmap = value;
            }
        }
        public Bitmap DataBitmap
        {
            get
            {
                return this.dataBitmap;
            }
            set
            {
                this.dataBitmap = value;
            }
        }

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
        }

        public override void applyDecryption()
        {
            if(keyBitmap!=null && dataBitmap!=null && source!=null)
            {
                decryptionKeyMap = new Dictionary<MyPoint, byte>(256);
                long sourceLength = retrieveDataRow(ref keyBitmap);
                byte[] buffer = new byte[PIXEL_DATA_SIZE];
                while (true)
                {
                    if (source.Length >= sourceLength)
                        break;
                    Color c = dataBitmap.GetPixel(data_X++, data_Y);
                    if (data_X >= dataBitmap.Width)
                    {
                        data_X = 0;
                        data_Y++;
                    }
                    buffer[0] = getDataFromKey(c.A);
                    buffer[1] = getDataFromKey(c.R);
                    buffer[2] = getDataFromKey(c.G);
                    buffer[3] = getDataFromKey(c.B);
                    source.Write(buffer, 0, (int)Math.Min(PIXEL_DATA_SIZE, sourceLength - source.Length));
                }
                source.Close();

            }            
        }

        private byte getDataFromKey(byte bytePosition)
        {
            int number = (int)bytePosition;
            //if(decryptionPointTemp==null)
                decryptionPointTemp = new MyPoint(0,0);
            decryptionPointTemp.X = number / KEY_WIDTH;
            decryptionPointTemp.Y = number % KEY_HEIGHT;
            byte data = 0;
            if (!decryptionKeyMap.ContainsKey(decryptionPointTemp))
            {
                data = keyBitmap.GetPixel(decryptionPointTemp.X, decryptionPointTemp.Y).A;
                decryptionKeyMap[new MyPoint(decryptionPointTemp.X, decryptionPointTemp.Y)] = data;
            }
            
            else
                Console.Write("Exist");
            if (decryptionKeyMap.Count == 256)
            {
                if (decryptionKeyMap.Count == 256)
                {
                    byte[] test = new byte[256];

                    foreach (int obj in decryptionKeyMap.Values)
                    {
                        test[obj]++;
                    }
                    Console.Write("");

                }
                Console.Write("");

            }
            data = decryptionKeyMap[decryptionPointTemp];
            return data;
        }

        public override void applyEncryption()
        {
            encryptionKeyMap = new Dictionary<int, MyPoint>(256);
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
                    for(int i=0;i< limit; i++) { 
                        buffer[i] = generatePosition(buffer[i]);
                        Console.WriteLine(buffer[i]);
                    }
                    dataBitmap.SetPixel(data_X++, data_Y, Color.FromArgb(buffer[0], buffer[1], buffer[2], buffer[3]));
                    if (data_X >= dataBitmap.Width)
                    {
                        data_Y++;
                        data_X = 0;
                    }
                    if (source.Position >= source.Length)
                        break;
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
            if (encryptionKeyMap.ContainsKey(data))
            {
                p = encryptionKeyMap[data];
            }
            if(p.X == -1 || p.Y == -1)
            {
                bool operation = true;
                do
                {
                    p.X = random.Next(KEY_WIDTH);
                    p.Y = random.Next(KEY_HEIGHT);
                    operation = encryptionKeyMap.ContainsValue(p);
                } while (operation);

                if (key_X >=keyBitmap.Width)
                {
                    key_X = 0;
                    key_Y++;
                }
                encryptionKeyMap[data] = p;
                keyBitmap.SetPixel(p.X, p.Y, Color.FromArgb(data, random.Next()%byte.MaxValue, random.Next() % byte.MaxValue, random.Next() % byte.MaxValue));
            }
            if (encryptionKeyMap.Count == 256)
            {
                if (encryptionKeyMap.Count == 256)
                {
                    int[,] test = new int[16, 17];

                    foreach (MyPoint obj in encryptionKeyMap.Values)
                    {
                        test[obj.X,obj.Y]++;
                    }
                    Console.Write("");

                }
                Console.Write("");

            }
            result = (byte)(p.X * 16 + p.Y);
            decryptionPointTemp = new MyPoint(0, 0);
            decryptionPointTemp.X = result / KEY_WIDTH;
            decryptionPointTemp.Y = result % KEY_HEIGHT;
            if (!decryptionPointTemp.Equals(p))
                Console.WriteLine("EQ ERROR");
            return result;
        }

        private void runChecker() {
            MyPoint temp = new MyPoint(0, 0);
            for (int x = 0; x < 16; x++)
                for (int y = 0; y < 16; y++)
                {
                    temp.X = x;
                    temp.Y = y;
                    if (!encryptionKeyMap.ContainsValue(temp)) {
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
                keyMap.SetPixel(key_X++, 16, Color.FromArgb(alpha, red, green, blue));
            }
        }

        private long retrieveDataRow(ref Bitmap keyMap)
        {
            byte[] sourceLengthBytes = BitConverter.GetBytes((long)0);
            for (int i = 0; i < sourceLengthBytes.Length / 4; i++)
            {
                Color c = keyMap.GetPixel(key_X++, 16);
                sourceLengthBytes[i * 4 + 0] = c.A;
                sourceLengthBytes[i * 4 + 1] = c.R;
                sourceLengthBytes[i * 4 + 2] = c.G;
                sourceLengthBytes[i * 4 + 3] = c.B;
            }
            return BitConverter.ToInt64(sourceLengthBytes, 0);
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
                if (obj is MyPoint && obj!=null)
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
            public override int GetHashCode()
            {
                return X * 16 + Y;
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
