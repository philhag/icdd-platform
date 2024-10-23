using System;
using System.IO;
using System.Linq;
using System.Threading;
using IcddWebApp.WebApplication.Environment;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace IcddWebApp
{
    public class Program
    {
        public static bool TerminateThread = false;
        public static void Main(string[] args)
        {
            Logger.Log("System started", Logger.MsgType.Info, "Program.Main(string[] args)");
            Directory.CreateDirectory("uploads");
            Directory.CreateDirectory("wwwroot/downloads/container");
            Directory.CreateDirectory("wwwroot/downloads/logfiles");
            Directory.CreateDirectory("wwwroot/downloads/wexBIM");
            Directory.CreateDirectory("wwwroot/files");
            var thread = new Thread(new ThreadStart(CleanupDirectories));
            thread.Start();

            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();

        public static void CleanupDirectories()
        {
            int minutes = 5;

            DateTime dtLastRefresh = DateTime.Now.AddMinutes(-10);

            while (!TerminateThread)
            {
                if (DateTime.Now - dtLastRefresh > TimeSpan.FromMinutes(10))
                {
                    dtLastRefresh = DateTime.Now;

                    string[] files = Directory.GetFiles("uploads");
                    foreach (string file in files)
                    {
                        FileInfo fi = new FileInfo(file);
                        if (fi.LastAccessTime < DateTime.Now.AddMinutes(-minutes))
                            fi.Delete();
                    }

                    string[] files2 = Directory.GetFiles("wwwroot/downloads/container");
                    foreach (string file in files2)
                    {
                        FileInfo fi = new FileInfo(file);
                        if (fi.LastAccessTime < DateTime.Now.AddMinutes(-minutes))
                            fi.Delete();
                    }
                    //string[] files6 = Directory.GetFiles("wwwroot/downloads/wexBIM");
                    //foreach (string file in files6)
                    //{
                    //    FileInfo fi = new FileInfo(file);
                    //    if (fi.LastAccessTime < DateTime.Now.AddMinutes(-minutes))
                    //        fi.Delete();
                    //}
                    string[] directories = Directory.GetDirectories("wwwroot/downloads/wexBIM");
                    foreach (var dir in directories)
                    {
                        string[] files7 = Directory.GetFiles(dir);
                        foreach (string file in files7)
                        {
                            FileInfo fi = new FileInfo(file);
                            if (fi.LastAccessTime < DateTime.Now.AddDays(-minutes))
                                fi.Delete();
                        }
                        DirectoryInfo dirinf = new DirectoryInfo(dir);
                        if (!dirinf.EnumerateFiles().Any())
                            Directory.Delete(dir);
                    }
                    DirectoryInfo di = new DirectoryInfo(("wwwroot/files"));
                    foreach (FileInfo fi in di.EnumerateFiles())
                    {
                        if (fi.LastAccessTime < DateTime.Now.AddMinutes(-minutes))
                            fi.Delete();
                    }
                    foreach (DirectoryInfo dir in di.EnumerateDirectories())
                    {
                        if (dir.LastAccessTime < DateTime.Now.AddMinutes(-minutes))
                            dir.Delete(true);
                    }

                    string[] files4 = Directory.GetFiles("wwwroot/downloads/logfiles");
                    foreach (string file in files4)
                    {
                        FileInfo fi = new FileInfo(file);
                        if (fi.LastAccessTime < DateTime.Now.AddMinutes(-minutes))
                            fi.Delete();
                    }
#if DEBUG
                    TerminateThread = true;
#endif

                }
                Thread.Sleep(100);
            }

        }

    }
}

