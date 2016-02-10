using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace DIP_Algorithm
{
    public partial class Form1 : Form
    {
        Encryption currentEncryption;
        EncryptionMeta output;
        Thread encryptionThread;
        public Form1()
        {
            InitializeComponent();
            algorithmList.SelectedIndex = 0;
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
            this.output = currentEncryption.Output;
            this.output.Output.Save(destinationFileList.Text+"\\"+output.Key+".bmp");
            keyTextBox.Text = output.Key;
            progressBar1.Value = 100;
            MessageBox.Show("Finished");
            
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
            if (algorithmList.SelectedIndex < 0)
            {
                MessageBox.Show("Please select an algorithm to be used.", "Error:");
                algorithmList.Focus();
            }
            else if (Directory.Exists(destinationFileList.Text))
            {
                
                if (algorithmList.Text == "Caesar's Cipher Encryption")
                {
                    Stream stream = openFileDialog1.OpenFile();
                    //Bitmap bitmap = new Bitmap(pictureBox1.Image);
                    this.currentEncryption = new CaesarsCipherEncryption(stream, new Bitmap(100, 100), keyTextBox.Text);
                    encrypt();
                    MessageBox.Show("Encryption 1");
                }
                else if (algorithmList.Text == "SHA-265 Key with Blowfish Encryption") {
                    Stream stream = openFileDialog1.OpenFile();
                    if(keyTextBox.Text.Length > 0)
                        currentEncryption = new SHA265_Blowfish(Encryption.GetBytes(keyTextBox.Text),stream);
                    else
                        currentEncryption = new SHA265_Blowfish(stream);
                    
                }

            }
            else
                MessageBox.Show("Please choose a destination path.","Error:" );
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
                    openFileList.Items.Add(openFileDialog1.FileName);
                    openFileList.Text = openFileDialog1.FileName;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);


            }
        }

        private void DecryptButton_Click(object sender, EventArgs e)
        {
            if(algorithmList.SelectedIndex == 0) { 
                try { 
                Stream stream = openFileDialog1.OpenFile();
                currentEncryption = new CaesarsCipherEncryption(stream, new Bitmap(100, 100));
                    ((CaesarsCipherEncryption)currentEncryption).DestinationFolder = destinationFileList.Text;
                EncryptionMeta output = new EncryptionMeta();
                if (keyTextBox.Text.Length > 0)
                {
                    output.Key = keyTextBox.Text;
                    output.Output = new Bitmap(stream);
                    stream.Close();
                    this.currentEncryption.Output = output;
                    this.currentEncryption.applyDecryption();
                }
                else
                { 
                    MessageBox.Show("Please input key.", "Error");
                    keyTextBox.Focus();
                }
                }catch(FileNotFoundException )
                {
                    MessageBox.Show("No file selected", "Error");
                }
            }
            else if (algorithmList.SelectedIndex == 0)
            {
                
            }
        }

        private void keyTextBox_TextChanged(object sender, EventArgs e)
        {
            keyLengthLabel.Text = keyTextBox.Text.Length + "";
        }
    }

    
}
