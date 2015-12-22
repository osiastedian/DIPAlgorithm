using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIP_Algorithm
{
    public abstract class Encryption
    {
        public EncryptionMeta Output { get; set; }
        public abstract void applyEncryption();
        public abstract double getPercentage();
        
        

        
    }
}
