using System.Globalization;

namespace DIP_Algorithm
{
    public abstract class Encryption
    {
        public EncryptionMeta Output { get; set; }
        public byte[] key { get; set; }
        public System.IO.Stream Source { get; set; }


        public abstract void applyEncryption();
        public abstract void applyDecryption();
        public abstract double getPercentage();
        public abstract byte[] generateKey();
        public static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length ];
            char[] array = str.ToCharArray();
            for(int i = 0; i < array.Length; i++)
            {
                bytes[i] = (byte)array[i];
            }
            return bytes;
        }

        public static string GetString(byte[] bytes)
        {
            char[] array = new char[bytes.Length];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = (char)bytes[i];
            }
            return new string(array);
        }

        public static byte[] GetBytesFromHexString(string hexString)
        {
            if (hexString.Length % 2 != 0)
            {
                throw new System.ArgumentException(System.String.Format(CultureInfo.InvariantCulture, "The binary key cannot have an odd number of digits: {0}", hexString));
            }

            byte[] HexAsBytes = new byte[hexString.Length / 2];
            for (int index = 0; index < HexAsBytes.Length; index++)
            {
                string byteValue = hexString.Substring(index * 2, 2);
                HexAsBytes[index] = byte.Parse(byteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }

            return HexAsBytes;
        }


    }
}
