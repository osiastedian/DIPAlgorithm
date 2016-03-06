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
        // Byte array to write
        // Offset
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
                data = (byte)retrieveDataFromColor(keyBitmap.GetPixel(decryptionPointTemp.X, decryptionPointTemp.Y),decryptionPointTemp);//keyBitmap.GetPixel(decryptionPointTemp.X, decryptionPointTemp.Y).A;
                decryptionKeyMap[new MyPoint(decryptionPointTemp.X, decryptionPointTemp.Y)] = data;
            }
            data = decryptionKeyMap[decryptionPointTemp];
            return data;
        }

        public override void applyEncryption()
        {
            encryptionKeyMap = new Dictionary<int, MyPoint>(256);
            int size = (int)Math.Sqrt(source.Length / 4) + 1;
            dataBitmap = new Bitmap(size, size);
            this.random = new Random();
            this.keyBitmap = generateKey(source.Length);// predefine KeyBitmap Reason: Sometime smaller files will make key bitmap more easy to decrypt since it only occupies less pixels.
            if (source != null)
            {
                byte[] buffer = new byte[4];
                while (true)
                {
                    int limit = Math.Min((int)(source.Length - source.Position), 4);
                    source.Read(buffer, 0, buffer.Length);
                    for (int i = 0; i < limit; i++)
                        buffer[i] = getPositionByte(buffer[i], ref keyBitmap, ref encryptionKeyMap);
                    dataBitmap.SetPixel(data_X++, data_Y, Color.FromArgb(buffer[0], buffer[1], buffer[2], buffer[3]));
                    if (data_X >= dataBitmap.Width)
                    {
                        data_Y++;
                        data_X = 0;
                    }
                    if (source.Position >= source.Length)
                        break;
                    Percentage = (float)source.Position / (float)source.Length;
                }
            }
            this.Output = new EncryptionMeta(dataBitmap, keyBitmap);
        }

        /// <summary>
        /// Generates a KeyBitmap for OsiasEncryption 
        /// </summary>
        /// <param name="sourceLength"></param>
        /// <returns></returns>
        private Bitmap generateKey(long sourceLength)
        {
            Bitmap keyBitmap = new Bitmap(KEY_WIDTH, KEY_HEIGHT + KEY_DATA_ROW_HEIGHT);
            putDataRow(sourceLength, ref keyBitmap);
            byte i = 0;
            do
            {
                addDataToKeyBitmap(i++, ref keyBitmap, ref encryptionKeyMap);   // PRESET a KeyBitmap
            } while (i != 0);
            return keyBitmap;
        }

        /// <summary>
        /// Gets the position of the data and converts it to byte format.
        /// If data does not exist,the position is generated and the data and position is added to the
        /// encryptionKeyMap.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="keyBitmap"></param>
        /// <param name="encryptionKeyMap"></param>
        /// <returns></returns>
        private byte getPositionByte(int data,ref Bitmap keyBitmap,ref Dictionary<int,MyPoint> encryptionKeyMap)
        {
            byte result = 0;
            MyPoint p = new MyPoint(-1,-1);
            if (encryptionKeyMap.ContainsKey(data))
            {
                p = encryptionKeyMap[data];
            }
            else//if(p.X == -1 || p.Y == -1) Before
            {
                p = addDataToKeyBitmap(data, ref keyBitmap, ref encryptionKeyMap);
            }
            result = (byte)(p.X * 16 + p.Y);
            return result;
        }

        private MyPoint addDataToKeyBitmap(int data,ref Bitmap keyBitmap, ref Dictionary<int, MyPoint> encryptionKeyMap)
        {
            MyPoint p = generatePoint(ref encryptionKeyMap);
            encryptionKeyMap[data] = p;
            keyBitmap.SetPixel(p.X, p.Y, generateColorFromData(data,p));
            return p;
        }

        /// <summary>
        /// data  - is the data you wnat to decrypt
        /// point - is the point is the basis where you are goint to put the data in the pixel
        /// OddX   OddY    Store
        ///   0     0        A
        ///   0     1        R
        ///   1     0        G
        ///   1     1        B
        /// </summary>
        /// <param name="data"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        private Color generateColorFromData(int data,MyPoint point)
        {
            int[] pixel = new int[PIXEL_DATA_SIZE];
            bool evenX = (point.X % 2) == 0;
            bool evenY = (point.Y % 2) == 0;
            pixel[0] = (evenX && evenY) ? data : random.Next() % byte.MaxValue;
            pixel[1] = (evenX && !evenY) ? data : random.Next() % byte.MaxValue;
            pixel[2] = (!evenX && evenY) ? data : random.Next() % byte.MaxValue;
            pixel[3] = (!evenX && !evenY) ? data : random.Next() % byte.MaxValue;
            return Color.FromArgb(pixel[0], pixel[1], pixel[2], pixel[3]);
        }
        /// <summary>
        /// Retrieves the data fom the pixel selected based on
        ///  OddX   OddY    Store
        ///   0     0        A
        ///   0     1        R
        ///   1     0        G
        ///   1     1        B
        /// </summary>
        /// <param name="color"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        private int retrieveDataFromColor(Color color, MyPoint point)
        {
            int data = -1;
            bool evenX = (point.X % 2) == 0;
            bool evenY = (point.Y % 2) == 0;
            if (evenX && evenY)
                data = color.A;
            else if (evenX && !evenY)
                data = color.R;
            else if (!evenX && evenY)
                data = color.G;
            else if (!evenX && !evenY)
                data = color.B;
            return data;
        }

        /// <summary>
        /// Generates a MyPoint object that does not belong to the dictionary passed.
        /// </summary>
        /// <param name="dictionary"></param>
        /// <returns></returns>
        private MyPoint generatePoint(ref Dictionary<int, MyPoint> dictionary )
        {
            MyPoint p = new MyPoint(-1, -1);
            bool operation = true;
            do
            {
                p.X = random.Next(KEY_WIDTH);
                p.Y = random.Next(KEY_HEIGHT);
                operation = dictionary.ContainsValue(p);
            } while (operation);
            return p;
        }

        /// <summary>
        /// Puts data row for keyMap
        /// </summary>
        /// <param name="sourceLength"></param>
        /// <param name="keyMap"></param>
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

        /// <summary>
        /// Retrieves data from data row
        /// </summary>
        /// <param name="keyBitmap"></param>
        /// <returns></returns>
        private long retrieveDataRow(ref Bitmap keyBitmap)
        {
            byte[] sourceLengthBytes = BitConverter.GetBytes((long)0);
            for (int i = 0; i < sourceLengthBytes.Length / 4; i++)
            {
                Color c = keyBitmap.GetPixel(key_X++, 16);
                sourceLengthBytes[i * 4 + 0] = c.A;
                sourceLengthBytes[i * 4 + 1] = c.R;
                sourceLengthBytes[i * 4 + 2] = c.G;
                sourceLengthBytes[i * 4 + 3] = c.B;
            }
            return BitConverter.ToInt64(sourceLengthBytes, 0);
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
