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
        Encryption currentEncryption;
        EncryptionMeta output;
        Thread encryptionThread;
        Thread decryptionThread;
        Thread updateThread;
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
                    file.Close();
                    openFileList.Items.Add(openFileDialog1.FileName);
                    openFileList.Text = openFileDialog1.FileName;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);

                
            }
        }

        private void savDialogutton_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();
            destinationFileList.Text = folderBrowserDialog1.SelectedPath;
            destinationFileList.Items.Add(folderBrowserDialog1.SelectedPath);
            
        }
        private void encrypt() {
            if (encryptionThread!=null && encryptionThread.IsAlive)
            {
                MessageBox.Show("There's another encryption process ongoing.");
            }
            else
            {
                Bitmap map = new Bitmap(100, 100);
                encryptionThread = new Thread(new ThreadStart(currentEncryption.applyEncryption));
                encryptionThread.Start();
                BackgroundWorker worker = new BackgroundWorker();
                worker.DoWork += Worker_DoWork;
                worker.ProgressChanged += Worker_ProgressChanged;
                worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
                worker.WorkerReportsProgress = true;
                worker.RunWorkerAsync();
            }
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            pictureBox1.Image = currentEncryption.Output.Output;
            MessageBox.Show("Finished");
            this.output = currentEncryption.Output;
            this.output.Output.Save("C:\\Users\\osias\\Desktop\\test2.bmp");
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            while (encryptionThread.IsAlive) { 
                worker.ReportProgress((int)(currentEncryption.getPercentage() * 100));
            }
        }

        private void encryptButton_Click(object sender, EventArgs e)
        {
            Stream stream = openFileDialog1.OpenFile();
            //Bitmap bitmap = new Bitmap(pictureBox1.Image);
            this.currentEncryption  = new CaesarsCipherEncryption(stream, new Bitmap(100, 100)); 
            encrypt();
        }

        private void openButtonDecryption_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();

            try
            {
                Stream file = openFileDialog1.OpenFile();
                if (file != null)
                {
                    file.Close();
                    srcListDecryption.Items.Add(openFileDialog1.FileName);
                    srcListDecryption.Text = openFileDialog1.FileName;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);


            }
        }

        private void DecryptButton_Click(object sender, EventArgs e)
        {
            Stream stream = openFileDialog1.OpenFile();
            //Bitmap bitmap = new Bitmap(pictureBox1.Image);
            this.currentEncryption = new CaesarsCipherEncryption(stream, new Bitmap(100, 100));
            EncryptionMeta output = new EncryptionMeta();
            output.Key = Microsoft.VisualBasic.Interaction.InputBox("Insert Key:","KEY");
            output.Output = new Bitmap(stream);
            this.currentEncryption.Output = output;
            this.currentEncryption.applyDecryption();

        }
    }

    
}
