using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace AcvApp
{
    public partial class Form1 : Form
    {
        
        private bool isTimerStarted = false;
        private FileInfo[] imageFiles;

        private FileInfo currentImage;
        private int currentImageIndex;
        
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.BringToFront();
            this.Focus();
            this.KeyPreview = true;

            this.showNextContentTimer.Start();
            this.startDownloadTimer.Start();
            isTimerStarted = true;
            currentImageIndex = 0;

            currentImage = new FileInfo("/acv/program/logo/jcdecaux_logo.png");
            pictureBox1.Image = Image.FromFile(currentImage.FullName);

            this.KeyDown += new KeyEventHandler(Form1_KeyDown);

        }

        void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            //Pressing Esc
            if (e.KeyData == Keys.Escape) Application.Exit();

            if (e.KeyData == Keys.Space)
            {
                if (isTimerStarted) showNextContentTimer.Stop();
                else showNextContentTimer.Start();
            }
        }

        private void showNextContentTimer_Tick(object sender, EventArgs e)
        {
            DirectoryInfo Folder = new DirectoryInfo("/acv/program/img"); 
            imageFiles = Folder.GetFiles("*.jpg")
                      .Concat(Folder.GetFiles("*.gif"))
                      .Concat(Folder.GetFiles("*.png"))
                      .Concat(Folder.GetFiles("*.jpeg"))
                      .Concat(Folder.GetFiles("*.bmp")).ToArray();

            try
            {
                currentImage = imageFiles.ElementAt(currentImageIndex);

            }
            catch (ArgumentOutOfRangeException expn)
            {
                currentImageIndex = 0;
                currentImage = imageFiles.ElementAt(currentImageIndex);
            }
            catch (ArgumentNullException expn)
            {
                currentImageIndex = 0;
                currentImage = new FileInfo("/acv/program/logo/jcdecaux_logo.png");
            }

            pictureBox1.Image = Image.FromFile(currentImage.FullName);
            currentImageIndex++;

        }

        private void startDownloadTimer_Tick(object sender, EventArgs e)
        {
            
            Acd acd = new Acd();
            try
            {
                acd.startDownload();
                Thread.Sleep(2000);
                currentImageIndex = 0;
                currentImage = new FileInfo("/acv/program/logo/jcdecaux_logo.png");
                pictureBox1.Image = Image.FromFile(currentImage.FullName);
                currentImageIndex++;
                this.showNextContentTimer.Stop();
                this.startDownloadTimer.Stop();
                acd.AddToPlaylistFiles();

            }
            catch(Exception err)
            {
                MessageBox.Show(err.StackTrace);
            }
            this.showNextContentTimer.Start();
            this.startDownloadTimer.Start();
        }
    }
}
