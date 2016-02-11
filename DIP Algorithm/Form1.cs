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
        Thread progressThread;
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
                progressThread = new Thread(new ThreadStart(this.runProgressListener));
                progressThread.Start();
            }
        }

        public void runProgressListener() {

            while (encryptionThread.IsAlive)
            {
                this.Invoke((MethodInvoker)delegate {
                    progressBar1.Value = (int)(currentEncryption.getPercentage() * 100);
                });
            }


            this.Invoke((MethodInvoker)delegate {
                // FINISHED
                pictureBox1.Image = currentEncryption.Output.Output;
                output = currentEncryption.Output;
                output.Output.Save(destinationFileList.Text + "\\" + output.Key.Substring(0,8) + ".bmp");
                keyTextBox.Text = output.Key;
                progressBar1.Value = 100;
                MessageBox.Show("Finished");
            });
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
                }
                else if (algorithmList.Text == "SHA-265 Key with Blowfish Encryption") {
                    Stream stream = openFileDialog1.OpenFile();
                    if(keyTextBox.Text.Length > 0)
                    {
                        if (keyTextBox.Text.Length > 8)
                        {
                            currentEncryption = new SHA265_Blowfish(Encryption.GetBytes(keyTextBox.Text), stream);
                            encrypt();
                        }
                        else
                            MessageBox.Show("Blowfish needs atleast 8 bytes/characters as key");
                    }
                    else
                    { 
                        currentEncryption = new SHA265_Blowfish(stream);
                        encrypt();
                    }

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
            Stream stream = openFileDialog1.OpenFile();
            if (algorithmList.SelectedIndex == 0) { 
                try { 
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
            else if (algorithmList.SelectedIndex == 1)
            {
                currentEncryption = new SHA265_Blowfish(null);
                ((SHA265_Blowfish)currentEncryption).DestinationFolder = destinationFileList.Text;
                EncryptionMeta output = new EncryptionMeta();
                if (keyTextBox.Text.Length > 0)
                {
                    output.Key = keyTextBox.Text;
                    output.Output = new Bitmap(stream);
                    Color color = output.Output.GetPixel(0, 0);
                    stream.Close();
                    currentEncryption.Output = output;
                    currentEncryption.applyDecryption();
                }

            }
        }

        private void keyTextBox_TextChanged(object sender, EventArgs e)
        {
            if (keyTextBox.Text.Length > 56 && algorithmList.SelectedIndex == 1) {
                MessageBox.Show("Blowfish Requirement: Key Length to big. (Maximum is 56)");
                keyTextBox.Text = keyTextBox.Text.Substring(0, 56);
                keyTextBox.Focus();
            }
            keyLengthLabel.Text = keyTextBox.Text.Length + "";
        }

        private void algorithmList_SelectedIndexChanged(object sender, EventArgs e)
        {
            keyTextBox_TextChanged(sender, e);
        }
    }

    
}
