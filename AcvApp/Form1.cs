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
            pictureBox2.Image = Image.FromFile(currentImage.FullName);

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

        private void animationPB2Down()
        {
            // animation of picture boxes 
            pictureBox2.BringToFront();
            pictureBox1.Visible = true;
            for (int i = 0; i < 1920; i++)
            {
                pictureBox2.Location = new Point(pictureBox2.Location.X, pictureBox2.Location.Y + 1);
                Thread.Sleep(3);
            }
            pictureBox2.Visible = false;
            pictureBox1.Location = new Point(0, 0);
        }

        private void animationPB1Down()
        {
            pictureBox1.BringToFront();
            pictureBox2.Visible = true;
            for (int i = 0; i < 1920; i++)
            {
                // pictureBox1.Top -= i;
                //pictureBox2.Top = 0;
                pictureBox1.Location = new Point(pictureBox1.Location.X, pictureBox1.Location.Y + 1);
                Thread.Sleep(3);
            }
            pictureBox1.Visible = false;
            pictureBox1.Location = new Point(0, 0);
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
            if (contentPictureBox == 1)
            {
                
                pictureBox2.Image = Image.FromFile(currentImage.FullName);
                contentPictureBox = 2;
            }
            else
            {

                
                pictureBox1.Image = Image.FromFile(currentImage.FullName);
                contentPictureBox = 1;
            }
            currentImageIndex++;
            if (contentPictureBox == 1)
            {
                animationPB1Down();

            }
            else
            {
                animationPB2Down();
            }
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
                //Thread.Sleep(2000);
                //currentImageIndex = 0;
                //currentImage = new FileInfo("/acv/program/logo/logo.png");
                //pictureBox1.Image = Image.FromFile(currentImage.FullName);
                //currentImageIndex++;
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
