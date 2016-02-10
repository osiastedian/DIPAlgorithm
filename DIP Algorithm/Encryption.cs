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


    }
}
