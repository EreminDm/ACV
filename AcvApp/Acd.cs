using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Security.Cryptography;
using Newtonsoft.Json;
using System.Threading;

namespace AcvApp
{
    class Acd
    {
        public class Item
        {
            public string filename;
            public string hash;
            public int duration;
            public string clientType;
        }

        public int contentFolderIndex = 1;

        public void startDownload()
        {
            string configSaveFilePath = @"C:\acv\conf\config.json";
            string configSaveFileDir = @"C:\acv\conf\";
            Directory.CreateDirectory(configSaveFileDir);
            string package = File.ReadAllText("/acv/program/packageConfig.txt", Encoding.UTF8);
            string ftpConfigFullPath = "ftp://185.98.7.121/content/" + package + "/config.json";

            string chekedLocalFilePath = @"C:\acv\workcontent\";
            CleanDir(chekedLocalFilePath);

            //get config from ftp server
            getFile(ftpConfigFullPath, configSaveFilePath, true);
            readJSON(configSaveFilePath);
           
            //  Console.ReadLine();
        }

        public static void getFile(string ftpFilePath, string localSaveDir, bool isJSON)
        {
            using (WebClient request = new WebClient())
            {
                request.Credentials = new NetworkCredential("rtlsnet_kz", "p@ssw0rd");
                byte[] fileData = request.DownloadData(ftpFilePath);

                using (FileStream file = File.Create(localSaveDir))
                {
                    file.Write(fileData, 0, fileData.Length);
                    file.Close();
                }
                if (isJSON)
                {
                    SaveInUTF8(localSaveDir);
                }

                Console.WriteLine("Download Complete");
            }
        }

        public static void SaveInUTF8(string fullPathAndFileName)
        {
            using (FileStream fs = File.OpenRead(fullPathAndFileName))
            {
                byte[] b = new byte[1024];
                UTF8Encoding temp = new UTF8Encoding(true);
                while (fs.Read(b, 0, b.Length) > 0)
                {
                    Console.WriteLine(temp.GetString(b));
                }
            }

        }


        public static void readJSON(string localFilePath)
        {
            // read package from config file 
            string package = File.ReadAllText("/acv/program/packageConfig.txt", Encoding.UTF8);


            string ftpFilePath = "ftp://185.98.7.121/content/" + package + "/";
            string notChekedLocalFilePath = @"C:\acv\startcontent\";
            string chekedLocalFilePath = @"C:\acv\workcontent\";
            Directory.CreateDirectory(notChekedLocalFilePath);
            Directory.CreateDirectory(chekedLocalFilePath);
            // clean checked dir to new download
           
            using (StreamReader r = new StreamReader(localFilePath))
            {
                string json = r.ReadToEnd();
                List<Item> items = JsonConvert.DeserializeObject<List<Item>>(json);
                string[] fileNameArr = new string[items.Count];
                string[] hashArr = new string[items.Count];
                int[] durationArr = new int[items.Count];
                string[] clientTypeArr = new string[items.Count];
                for (int i = 0; i < items.Count; i++)
                {
                    //Console.WriteLine(items[i].clientType);
                    fileNameArr[i] = items[i].filename;
                    hashArr[i] = items[i].hash;
                    durationArr[i] = items[i].duration;
                    clientTypeArr[i] = items[i].clientType;
                }
                for (int i = 0; i < fileNameArr.Length; i++)
                {
                    string ftpFile = ftpFilePath + fileNameArr[i];
                    string localFile = notChekedLocalFilePath + fileNameArr[i];
                    getFile(ftpFile, localFile, false);

                    String md5Str = getMD5(localFile);
                    if (md5Str == hashArr[i])
                    {
                        string endContentPath = chekedLocalFilePath + fileNameArr[i];
                        File.Copy(localFile, endContentPath);
                        using (StreamWriter w = File.AppendText(chekedLocalFilePath + "playlist.json"))
                        {
                            var line = "{ \"filename\":" + fileNameArr[i] + ",\"hash\":" + hashArr[i] + ",\"duration\":" + durationArr[i] + ",\"clientType\":" + clientTypeArr[i] + "},";
                            w.WriteLine(line);
                        };
                    }
                    else
                    {
                        Console.Write("File is not usefull, hash summ is not correct" + localFile);
                        File.Delete(localFile);
                    }
                }
                SaveInUTF8(chekedLocalFilePath + "playlist.json");
            }
            // clean start dir
            CleanDir(notChekedLocalFilePath);

        }

        public static void CleanDir(string dirPath)
        {
            DirectoryInfo di = new DirectoryInfo(dirPath);

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }
        }

        // add to playlist dir
        public void AddToPlaylistFiles()
        {
            Form1 form = new Form1();

            string playlistdir = "";
            if (contentFolderIndex == 1)
            {
                playlistdir = File.ReadAllText("/acv/program/playlistDirConfig2.txt", Encoding.UTF8);
                contentFolderIndex = 1;
                form.playingCOntentFolderAdres = "/acv/program/img2";
                
            }
            else
            {
                playlistdir = File.ReadAllText("/acv/program/playlistDirConfig.txt", Encoding.UTF8);
                contentFolderIndex = 2;
            }
            
            string chekedLocalFilePath = @"C:\acv\workcontent\";
            CleanDir(playlistdir);
            // необходимо переделать только а удаление не нужных файлов

            foreach (var file in Directory.GetFiles(chekedLocalFilePath))
                File.Copy(file, Path.Combine(playlistdir, Path.GetFileName(file)), true);

        }
        private static String getMD5(String filepath)
        {

            using (var md5 = MD5.Create())
            {
                using (var stream = System.IO.File.OpenRead(filepath))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", string.Empty);
                }
            }
        }
    }
}
