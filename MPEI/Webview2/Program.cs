using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Net;
using System.IO;

namespace OnlineVideos
{
    class Program
    {
        public static bool RegKeyExists(string basePath)
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey(basePath);
            if (key == null)
                key = Registry.CurrentUser.OpenSubKey(basePath);
            return key != null && key.GetValue("pv") != null;
        }

        public static int Main(string[] args)
        {
            //https://go.microsoft.com/fwlink/p/?LinkId=2124703
            bool installed = RegKeyExists(@"SOFTWARE\WOW6432Node\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}") ||
                RegKeyExists(@"Software\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}");
            try
            {
                if (!installed)
                {
                    ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072 | SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls;

                    var fileName = Path.Combine(Path.GetTempPath(), "MicrosoftEdgeWebview2Setup.exe");
                    using (WebClient client = new WebClient())
                    {
                        client.DownloadFile(@"https://go.microsoft.com/fwlink/p/?LinkId=2124703", fileName);
                        using (Process myProcess = new Process())
                        {
                            myProcess.StartInfo.UseShellExecute = false;
                            myProcess.StartInfo.FileName = fileName;
                            myProcess.Start();
                            myProcess.WaitForExit();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Something has gone wrong: " + e.Message);
                return 1;
            }
            return 0;
        }
    }
}
