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
        public string playingconfdir = "/acv/program/img";
        public int contentFolderIndex = 1;
        public int contentPictureBox = 1;
        private bool isTimerStarted = false;
        private FileInfo[] imageFiles;

        private FileInfo currentImage;
        private int currentImageIndex;

        public Form1()
        {
            InitializeComponent();
            Cursor.Hide();

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

            currentImage = new FileInfo("/acv/program/logo/logo.png");
            pictureBox1.Image = Image.FromFile(currentImage.FullName);
            pictureBox1.BringToFront();

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

       

        private void animationAllDown()
        {
            this.showNextContentTimer.Stop();
            for (int i = 0; i < 1920; i++)
            {
                if (pictureBox1.Height <= 1920)
                {

                        pictureBox1.Height = pictureBox1.Height + 5;
                        pictureBox1.Refresh();

                }
            }
            this.showNextContentTimer.Start();

        }

        private void showNextContentTimer_Tick(object sender, EventArgs e)
        {

            DirectoryInfo Folder = new DirectoryInfo(playingconfdir);
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
                currentImage = new FileInfo("/acv/program/logo/logo.png");
            }
            
            pictureBox1.Size = new Size(pictureBox1.Width,0);
            pictureBox1.Image = Image.FromFile(currentImage.FullName);
            pictureBox1.Refresh();

            animationAllDown();

            currentImageIndex++;

        }

        private void startDownloadTimer_Tick(object sender, EventArgs e)
        {
            if (contentFolderIndex == 1)
            {
                playingconfdir = File.ReadAllText("/acv/program/playlistDirConfig2.txt", Encoding.UTF8);
                contentFolderIndex = 2;
            }
            else
            {
                playingconfdir = File.ReadAllText("/acv/program/playlistDirConfig.txt", Encoding.UTF8);
                contentFolderIndex = 1;
            }

            Acd acd = new Acd();
            
            try
            {
                acd.startDownload();
                this.showNextContentTimer.Stop();
                this.startDownloadTimer.Stop();
                acd.AddToPlaylistFiles(playingconfdir);

            }
            catch(Exception err)
            {
                // необходимо сделать запись в локальный log файл
               // MessageBox.Show(err.StackTrace);
            }
            
            this.showNextContentTimer.Start();
            this.startDownloadTimer.Start();
        }
    }
}
