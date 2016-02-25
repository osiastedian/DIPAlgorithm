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

        public const int CAESARS_CIPHER = 0;
        public const int BLOWFISH = 1;
        public const int OSIAS_ENCRYPTION = 2;

        Encryption currentEncryption;
        EncryptionMeta output;
        Thread encryptionThread;
        Thread progressThread;
        System.Timers.Timer timer;
        double timerTicks = 0;
        public static string OPERATION_ENCRYPTION = "Encryption";
        public static string OPERATION_DECRYPTION = "Decryption";
        
        private string currentOperation = null;
        public Form1()
        {
            InitializeComponent();
            algorithmList.SelectedIndex = 0;

            timer = new System.Timers.Timer(10);
            timer.Elapsed += delegate {
                timerTicks += timer.Interval;
                statusLabel.Text = ""+ timeToString(timerTicks);
            };


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
        private void EncryptAndDecryptFunction(bool operation) {
            if (encryptionThread!=null && encryptionThread.IsAlive)
            {
                MessageBox.Show("There's another encryption process ongoing.");
            }
            else
            {
                Bitmap map = new Bitmap(100, 100);
                if(operation)
                    encryptionThread = new Thread(new ThreadStart(currentEncryption.applyEncryption));
                else
                    encryptionThread = new Thread(new ThreadStart(currentEncryption.applyDecryption));
                encryptionThread.Start();
                progressThread = new Thread(new ThreadStart(runProgressListener));
                progressThread.Start();
            }
        }

        private void normalPostEncryption() {
            pictureBox1.Image = currentEncryption.Output.Output;
            output = currentEncryption.Output;
            if (currentOperation == OPERATION_ENCRYPTION)
                output.Output.Save(destinationFileList.Text + "\\" + output.Key.Substring(0, 8) + ".bmp");
            keyTextBox.Text = output.Key;
            progressBar1.Value = 100;
            timer.Stop();
            MessageBox.Show("Finished " + currentOperation + "Time:" + timeToString(timerTicks));
            timerTicks = 0;
            currentOperation = null;
        }
        private void normalDuringEncryption()
        {
            progressBar1.Value = (int)(currentEncryption.getPercentage() * 100);
        }
        private void normalPreEncryption()
        {
            timer.Start();
        }
        public delegate void PreEncryption();
        public delegate void PostEncryption();
        public delegate void DuringEncryption();
        public PostEncryption postEncrtypion;
        public DuringEncryption duringEncryption;
        public PreEncryption preEncryption;
        public void runProgressListener() {
            this.Invoke(preEncryption);
            while (encryptionThread.IsAlive)
            {
                this.Invoke(duringEncryption);
            }
            
            this.Invoke(postEncrtypion);
        }

        private string timeToString(double ticks)
        {
            DateTime dt = new DateTime(0);
            dt = dt.AddMilliseconds(ticks);
            return dt.Hour+":"+dt.Minute+":"+dt.Second+":"+dt.Millisecond;
        }

        private void encryptButton_Click(object sender, EventArgs e)
        {
            currentOperation = OPERATION_ENCRYPTION;
            if (algorithmList.SelectedIndex < 0)
            {
                MessageBox.Show("Please select an algorithm to be used.", "Error:");
                algorithmList.Focus();
            }
            else if (Directory.Exists(destinationFileList.Text))
            {
                
                if (algorithmList.Text == "Caesar's Cipher Encryption")
                {
                    if(caesarscipher())
                        EncryptAndDecryptFunction(true);

                }
                else if (algorithmList.Text == "SHA-265 Key with Blowfish Encryption")
                {
                    if(blowFish())
                        EncryptAndDecryptFunction(true);
                }
                else if (algorithmList.Text == "OSIAS Encryption")
                {
                    osiasEncryption();
                    EncryptAndDecryptFunction(true);
                }
                
            }
            else
                MessageBox.Show("Please choose a destination path.","Error:" );
        }

        private void OsiasPostEncryption()
        {
            OSIASEncryption.EncryptionMeta output = ((OSIASEncryption)currentEncryption).Output;
            pictureBox1.Image = output.Output;
            progressBar1.Value = 100;                   
            timer.Stop();
            string destFolder = this.destinationFileList.Text;
            output.Key.Save(destFolder + "\\Key.bmp");
            output.Output.Save(destFolder + "\\Output.bmp");
            MessageBox.Show("Finished " + currentOperation + "Time:" + timeToString(timerTicks));
            timerTicks = 0;
            currentOperation = null;
        }

        private bool osiasEncryption()
        {
            bool ok = false;
            Stream stream = openFileDialog1.OpenFile();
            this.currentEncryption = new OSIASEncryption(stream);
            this.preEncryption = normalPreEncryption;
            this.duringEncryption = normalDuringEncryption;
            this.postEncrtypion = OsiasPostEncryption;
            ok = true;
            return ok;
            

        }

        private bool caesarscipher()
        {
            bool ok = false;
            Stream stream = openFileDialog1.OpenFile();
            this.currentEncryption = new CaesarsCipherEncryption(stream, new Bitmap(100, 100), keyTextBox.Text);
            this.preEncryption = normalPreEncryption;
            this.duringEncryption = normalDuringEncryption;
            this.postEncrtypion = normalPostEncryption;
            ok = true;
            return ok;
        }
        private bool blowFish()
        {
            bool ok = false;
            Stream stream = openFileDialog1.OpenFile();
            if (keyTextBox.Text.Length > 0)
            {
                if (keyTextBox.Text.Length >= 8)
                {
                    byte[] key;
                    if (hexStringFlag.Checked)
                        key = Encryption.GetBytesFromHexString(keyTextBox.Text);
                    else
                        key = Encryption.GetBytes(keyTextBox.Text);
                    currentEncryption = new SHA256_Blowfish(key, stream);
                    this.preEncryption = normalPreEncryption;
                    this.duringEncryption = normalDuringEncryption;
                    this.postEncrtypion = normalPostEncryption;
                    ok = true;
                }
                else
                    MessageBox.Show("Blowfish needs atleast 8 bytes/characters as key");
            }
            else
            {
                hexStringFlag.Checked = true;
                currentEncryption = new SHA256_Blowfish(stream);
                EncryptAndDecryptFunction(true);
            }
            return ok;
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
            currentOperation = OPERATION_DECRYPTION;
            switch (algorithmList.SelectedIndex)
            {
                case CAESARS_CIPHER:
                                    { 
                                        this.caesarscipher();
                                        try
                                        {
                                            currentEncryption = new CaesarsCipherEncryption(stream, new Bitmap(100, 100));
                                            ((CaesarsCipherEncryption)currentEncryption).DestinationFolder = destinationFileList.Text;
                                            EncryptionMeta output = new EncryptionMeta();
                                            if (keyTextBox.Text.Length > 0)
                                            {
                                                output.Key = keyTextBox.Text;
                                                output.Output = new Bitmap(stream);
                                                stream.Close();
                                                this.currentEncryption.Output = output;
                                                EncryptAndDecryptFunction(false);
                                            }
                                            else
                                            {
                                                MessageBox.Show("Please input key.", "Error");
                                                keyTextBox.Focus();
                                            }
                                        }
                                        catch (FileNotFoundException)
                                        {
                                            MessageBox.Show("No file selected", "Error");
                                        }
                                    }
                                    break;
                case BLOWFISH:
                                {
                                    byte[] key;
                                    if (hexStringFlag.Checked)
                                        key = Encryption.GetBytesFromHexString(keyTextBox.Text);
                                    else
                                        key = Encryption.GetBytes(keyTextBox.Text);
                                    currentEncryption = new SHA256_Blowfish(key, null);
                                    ((SHA256_Blowfish)currentEncryption).DestinationFolder = destinationFileList.Text;
                                    EncryptionMeta output = new EncryptionMeta();
                                    if (keyTextBox.Text.Length > 0)
                                    {
                                        output.Key = keyTextBox.Text;
                                        output.Output = new Bitmap(stream);
                                        Color color = output.Output.GetPixel(0, 0);
                                        stream.Close();
                                        currentEncryption.Output = output;
                                        EncryptAndDecryptFunction(false);
                                    }

                                }
                                break;
                case OSIAS_ENCRYPTION:
                                {
                                   
                                    


                                }
                                break;

            }
        }
        

        private void keyTextBox_TextChanged(object sender, EventArgs e)
        {
            if (keyTextBox.Text.Length > 56 && algorithmList.SelectedIndex == 1 && hexStringFlag.Checked == false) {
                MessageBox.Show("Blowfish Requirement: Key Length to big. (Maximum is 56)");
                keyTextBox.Text = keyTextBox.Text.Substring(0, 56);
                keyTextBox.Focus();
            }
            keyLengthLabel.Text = keyTextBox.Text.Length + "";
        }

        private void algorithmList_SelectedIndexChanged(object sender, EventArgs e)
        {
            keyTextBox_TextChanged(sender, e);
            if (algorithmList.SelectedIndex == OSIAS_ENCRYPTION)
            {
                keyTextBox.Width = openFileList.Location.X + openFileList.Width - keyTextBox.Location.X;
                keyBitmapOpenButton.Visible = true;
                label5.Visible = false;
                keyLengthLabel.Visible = false;
                hexStringFlag.Visible = false;
            }
            else {
                keyTextBox.Width = algorithmList.Width;
                keyBitmapOpenButton.Visible = false;
                label5.Visible = true;
                keyLengthLabel.Visible = true;
                hexStringFlag.Visible = true;
            }
                
        }

        private void keyBitmapOpenButton_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            if (File.Exists(openFileDialog1.FileName))
            {
                keyTextBox.Text = openFileDialog1.FileName;
                openFileDialog1.FileName = "";
            }
        }
    }

    
}
