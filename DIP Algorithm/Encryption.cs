namespace DIP_Algorithm
{
    public abstract class Encryption
    {
        public EncryptionMeta Output { get; set; }
        public abstract void applyEncryption();
        public abstract void applyDecryption();
        public abstract double getPercentage();
        public abstract byte[] generateKey();
        

        
    }
}
