using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;

namespace IISLogParser
{
    internal class Program
    {
        //Declarations
        private static DirectoryInfo directoryInfo = null;

        private static FileInfo[] files = null;
        private static FileStream outputpath;
        private static string outputfolder = ConfigurationManager.AppSettings["OutputFolder"];
        private static string[] machineNames = ConfigurationManager.AppSettings["MchineNames"].Split(',');

        private static void Main(string[] args)
        {
            //string driveLetter = Directory.GetDirectoryRoot(AppDomain.CurrentDomain.BaseDirectory);
            //string directory= Directory.CreateDirectory(driveLetter + "SCMIIS").ToString();
            try
            {
                string yesterdaysdate = DateTime.Now.AddDays(-1).ToString("dd-MM-yyyy");
                string todaysdate = DateTime.Now.ToString("dd-MM-yyyy");

                if (File.Exists(outputfolder + yesterdaysdate + ".txt"))
                {
                    File.Delete(outputfolder + yesterdaysdate + ".txt");
                    outputpath = File.Create(outputfolder + todaysdate + ".txt");
                    outputpath.Close();
                }
                else
                {
                    outputpath = File.Create(outputfolder + todaysdate + ".txt");
                    outputpath.Close();
                }

                //actual method call
                ReadLogFiles();
            }
            catch (Exception ex)
            {
                File.AppendAllText(outputpath.Name, ex.Message);
            }
        }

        private static object ReadLogFiles()
        {
            string folder = "";
            string server = "";

            try
            {
                if (!string.IsNullOrEmpty(machineNames.ToString()) && !string.IsNullOrEmpty(machineNames.ToString()))
                {
                    for (int k = 0; k < machineNames.Length; k++)
                    {
                        server = machineNames[k];
                        folder = ConfigurationManager.AppSettings["LogFolder"];
                        folder = @"\\" + machineNames[k] + folder;
                        directoryInfo = new DirectoryInfo(folder);
                        DateTime to_date = DateTime.Now.AddDays(-1);
                        files = directoryInfo.GetFiles().Where(x => x.LastWriteTime.Date == to_date.Date).ToArray();

                        for (int i = 0; i < files.Length; i++)
                        {
                            string path = files[i].FullName;

                            if (IsFileBlocked(files[i].FullName))
                                continue;

                            RunLogParser(path);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return null;
        }

        private static bool IsFileBlocked(String fileFullName)
        {
            var request = WebRequest.Create(fileFullName);
            try
            {
                request.GetResponse();
            }
            catch (WebException ex)
            {
                if (ex.Message.ToLower().Contains("the process cannot access the file"))
                {
                    return true;
                }
            }
            return false;
        }

        private static void RunLogParser(string fileName)
        {
            var logParserPath = @"C:\Program Files (x86)\Log Parser 2.2\LogParser.exe";

            var query = $"\"SELECT *,TO_TIMESTAMP(date, time) AS utc-timestamp,TO_LOCALTIME(utc-timestamp) AS local-timestamp INTO SCMIIS FROM {fileName}\"";

            var startInfo = new ProcessStartInfo
            {
                FileName = logParserPath,
                Arguments = query + @" -i:w3c -o:SQL -server:co1ebsscmpsql03 -database:SQLInfo -driver:""SQL Server"" -createTable:ON",
                CreateNoWindow = false,
                UseShellExecute = false,
                RedirectStandardOutput = true
            };

            try
            {
                using (var process = Process.Start(startInfo))
                {
                    process.WaitForExit();
                    var output = process.StandardOutput.ReadToEnd();
                    File.AppendAllText(outputpath.Name, output);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}