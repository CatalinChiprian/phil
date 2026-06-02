using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PHIL_GUI.Services
{
    public class MediaService
    {
        public async Task RecordVideo(int actionId)
        {
            using var capture = new VideoCapture(0);

            if (!capture.IsOpened)
            {
                Console.WriteLine("No camera detected");
                return;
            }

            int width = 640;
            int height = 480;

            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string folder = Path.Combine(desktop, "Recordings");
            Directory.CreateDirectory(folder);
            string file = Path.Combine(folder, $"action_{actionId}_{DateTime.Now:HHmmss}.avi");

            using var writer = new VideoWriter(
                file,
                VideoWriter.Fourcc('M', 'J', 'P', 'G'),
                25,
                new System.Drawing.Size(width, height),
                true);

            DateTime start = DateTime.Now;

            while ((DateTime.Now - start).TotalSeconds < 10)
            {
                using var frame = capture.QueryFrame();

                if (frame != null)
                {
                    writer.Write(frame);
                }

                await Task.Delay(40);
            }
        }
    }
}
