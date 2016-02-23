using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Collections;

namespace DIP_Algorithm
{
    class OSIASEncryption : Encryption
    {
        public  const int KEY_WIDTH = 100;
        public  const int KEY_HEIGHT = 100;
        private Hashtable dataMap;
        private Stream source;
        private Bitmap keyBitmap;
        private Bitmap data;
        

        public OSIASEncryption(Stream source) 
        {
            this.source = source;
            this.keyBitmap = new Bitmap(KEY_WIDTH, KEY_HEIGHT);
            int size = (int)Math.Sqrt(source.Length) + 1;
            this.data = new Bitmap(size, size);
            // HOW TO USE HASHMAP
        }

        public override void applyDecryption()
        {
            dataMap = new Hashtable();
            dataMap.Add(256, new LocationValue(0, 0, 2));
            
            throw new NotImplementedException();
        }

        private void addDataToMap(int data) 
        {
            dataMap.Add(data, new ArrayList());
            
        }

        public override void applyEncryption()
        {
            throw new NotImplementedException();
        }

        public override byte[] generateKey()
        {
            throw new NotImplementedException();
        }

        public override double getPercentage()
        {
            throw new NotImplementedException();
        }



        private class LocationValue {
            public int X { get; set; }
            public int Y { get; set; }
            public int Position { get; set; }

            public LocationValue(int x, int y, int position)
            {
                X = x;
                Y = y;
                this.Position = position;
            }

            
        }
    }
}
