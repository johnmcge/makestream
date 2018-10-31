using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace makestream
{
    class Program
    {
        public static string StreamFileSuffix = "-stream";
        public static DateTime dtMarker;

        static void Main(string[] args)
        {
            string usageString = GetUsageString();
            if (args.Length < 1)
            {
                Console.WriteLine("too few arguments, need at least one video file.");
                Console.WriteLine(Environment.NewLine + usageString + Environment.NewLine);
                return;
            }

            int i = 0;
            foreach (string fileToProcess in args)
            {
                i++;
                if (!File.Exists(fileToProcess))
                    Console.WriteLine(Environment.NewLine + "File not found: " + fileToProcess);
                else
                {
                    Console.WriteLine("  Processing " + i.ToString() + " of " + args.Length.ToString() + " videos");
                    ProcessVideoFile(fileToProcess);
                }
            }
        }

        public static void ProcessVideoFile(string fileName)
        {
            int idx = fileName.LastIndexOf('.');
            string ext = fileName.Substring((idx), (fileName.Length - idx));

            if (ext.ToLower() != ".mp4")
            {
                Console.WriteLine(Environment.NewLine + "only mp4 supported at this time. File skipped: " + fileName);
                return;
            }

            string outputFileName = fileName.Substring(0, idx) + StreamFileSuffix + ext;
            
            /**
             *  ffmpeg parameters are specific to my use case, converting 1080p video output from my camera 
             *  down to 720p. Easily changed below to meet your needs, or could be parameterized. As an example,
             *  a 26 minute video weighing in at 4.5GB slims down to 424MB, and doesn't look too bad. You can 
             *  get better quality (and longer conversion time) by changing "-preset slower" to " -preset slowest".
             *  on my machine with "slower", conversions take about 2.5 x the length of the video being converted.
            **/
            
            // ffmpeg -i input.mp4 -vf scale=-1:720 -c:v libx264 -crf 25 -preset slower -c:a copy input-stream.mp4
            StringBuilder cmdText = new StringBuilder();

            cmdText.Append(" -y");
            cmdText.Append(" -i " + fileName);
            cmdText.Append(" -vf scale=-1:720");
            cmdText.Append(" -c:v libx264");
            cmdText.Append(" -crf 25");
            cmdText.Append(" -preset slower");
            cmdText.Append(" -c:a copy");
            cmdText.Append(" " + outputFileName);

            dtMarker = DateTime.Now;
            Console.WriteLine("  " + dtMarker.ToString("hh':'mm':'ss") + "  Making: " + outputFileName);

            using (Process process = new Process())
            {
                process.StartInfo.FileName = "ffmpeg.exe";
                process.StartInfo.Arguments = cmdText.ToString();
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.Start();
                process.WaitForExit();
                process.Dispose();
            }

            string dur = (DateTime.Now - dtMarker).ToString("hh':'mm':'ss");
            Console.WriteLine("  " + DateTime.Now.ToString("hh':'mm':'ss") + "  done");
            Console.WriteLine("  " + dur + "  elapsed time" + Environment.NewLine);
        }

        public static string GetUsageString()
        {
            StringBuilder sbUsage = new StringBuilder();
            sbUsage.Append(Environment.NewLine);
            sbUsage.Append(" makestream file1 file2 file3");
            sbUsage.Append(Environment.NewLine);
            sbUsage.Append(" you can list as many files as you like, it will do them one at a time.");
            sbUsage.Append(Environment.NewLine);
            return sbUsage.ToString();
        }

    }
}
