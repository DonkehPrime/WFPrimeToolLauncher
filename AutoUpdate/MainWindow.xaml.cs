using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.ComponentModel;
using System.IO;
using Ionic.Zip;

namespace AutoUpdate
{
   
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static WebClient client = new WebClient();
        public static string checkv;
        public MainWindow()
        {
            InitializeComponent();
            CheckVersion();
        }


        void CheckVersion()
        {
            string filename = "version.txt";
                
            var version = client.DownloadString(new Uri($"https://github.com/DonkehPrime/WFPrimeTool/releases/download/custom/" + filename));
            if (File.Exists(filename))
            {
                using(FileStream fs = File.OpenRead(filename))
                {
                    byte[] b = new byte[fs.Length];
                    UTF8Encoding temp = new UTF8Encoding(true);

                    while (fs.Read(b, 0, b.Length) > 0)
                    {
                        checkv = temp.GetString(b);
                    }
                }
                if(checkv != version)
                {
                    File.Delete(filename);
                    using (FileStream fs = File.Create(filename))
                    {
                        Byte[] info = new UTF8Encoding(true).GetBytes(version);
                        fs.Write(info, 0, info.Length);
                    }
                    Title = "WFPrimeTool Auto Updater v"+version;
                    startDownload();
                }
                if(checkv == version)
                {
                    Title = "WFPrimeTool Auto Updater v" + version;
                    Directory.SetCurrentDirectory("WFPrimeTool");
                    System.Diagnostics.Process.Start(@"WF Prime Tool.exe");
                    this.Close();
                }
            }
            else
            {
                using (FileStream fs = File.Create(filename))
                {
                    Byte[] info = new UTF8Encoding(true).GetBytes(version);
                    fs.Write(info, 0, info.Length);
                }
                Title = "WFPrimeTool Auto Updater v" + version;
                startDownload();
            }
        }

        void startDownload()
        {
            Thread thread = new Thread(() => {
                
                //client.Headers.Add("alt", "media");
           
                client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
                client.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);
                client.DownloadFileAsync(new Uri($"https://github.com/DonkehPrime/WFPrimeTool/releases/download/custom/WFPrimeTool.zip"), @"WFPrimeTool.zip");
            });
            thread.Start();
        }
        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke((MethodInvoker)delegate {
                double bytesIn = double.Parse(e.BytesReceived.ToString());
                double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
                double percentage = bytesIn / totalBytes * 100;
                label.Content = "Downloaded " + e.BytesReceived + " of " + e.TotalBytesToReceive;
                progressBar.Value = int.Parse(Math.Truncate(percentage).ToString());
            });
        }
        void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke((MethodInvoker)delegate {
                label.Content = "                       Completed";
                string zipToUnpack = "WFPrimeTool.zip";
                string unpackDirectory = "";
                using (ZipFile zip1 = ZipFile.Read(zipToUnpack))
                {
                    // here, we extract every entry, but we could extract conditionally
                    // based on entry name, size, date, checkbox status, etc.  
                    foreach (ZipEntry g in zip1)
                    {
                        g.Extract(unpackDirectory, ExtractExistingFileAction.OverwriteSilently);
                    }
                }
                File.Delete("WFPrimeTool.zip");
                Directory.SetCurrentDirectory("WFPrimeTool");
                System.Diagnostics.Process.Start(@"WF Prime Tool.exe");
                this.Close();
            });
        }
    }
}
