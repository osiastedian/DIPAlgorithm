using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DIP_Algorithm
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
        }

        private void openDialogButton_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();

            try
            {
                Stream file = openFileDialog1.OpenFile();
                if (file != null)
                {
                    openFileList.Items.Add(openFileDialog1.FileName);
                    openFileList.Text = openFileDialog1.FileName;


                }
            }
            catch (Exception)
            {

                
            }
        }

        private void savDialogutton_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();
            destinationFileList.Text = folderBrowserDialog1.SelectedPath;
            destinationFileList.Items.Add(folderBrowserDialog1.SelectedPath);
            
        }

        private void encrypt(Stream stream, Encryption algorithm) {
            
            Bitmap map = new Bitmap(100, 100);
            CaesarsCipherEncryption encryption = (CaesarsCipherEncryption)algorithm;
            Thread th = new Thread(new ThreadStart(encryption.applyEncryption));
            th.Start();
            int x = 0;
            while (true) {
                if (th.IsAlive)
                {
                     long  up = encryption.i;
                     long  down = stream.Length;
                    
                     decimal d = Decimal.Divide(Convert.ToDecimal(up), Convert.ToDecimal(down));
                    x = (int)(d*100); 
                    progressBar1.Value = x;
                    
                    Thread.Sleep(10);
                }
                else
                    break;

            }
            pictureBox1.Image = encryption.Output.Output;
            MessageBox.Show("Finished");
            
        }

        private void encryptButton_Click(object sender, EventArgs e)
        {
            Stream stream = openFileDialog1.OpenFile();
            //Bitmap bitmap = new Bitmap(pictureBox1.Image);
            CaesarsCipherEncryption algo = new CaesarsCipherEncryption(stream,new Bitmap(100,100));
            encrypt(stream, algo);
        }
    }

    
}
