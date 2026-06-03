using Emgu.CV;
using Emgu.CV.Structure;
using PHIL_GUI.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PHIL_GUI.Services
{
    public class MediaService
    {
        private readonly IRecordContext recordContext;
        public MediaService(IRecordContext recordContext)
        {
            this.recordContext = recordContext;
        }
        public async Task RecordVideo(int actionId)
        {
            if (!recordContext.AreActionRecorded) return;

            using var capture = new VideoCapture(0);

            if (!capture.IsOpened)
            {
                Console.WriteLine("No camera detected");
                return;
            }

            using var firstFrame = capture.QueryFrame();

            if (firstFrame == null || firstFrame.IsEmpty)
            {
                Console.WriteLine("Failed to get first frame");
                return;
            }

            int width = firstFrame.Width;
            int height = firstFrame.Height;

            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string folder = Path.Combine(desktop, "Recordings");
            Directory.CreateDirectory(folder);
            string file = Path.Combine(folder, $"action_{actionId}_{DateTime.Now:HHmmss}.avi");

            using var writer = new VideoWriter(
                file,
                VideoWriter.Fourcc('X', 'V', 'I', 'D'),
                25,
                new System.Drawing.Size(width, height),
                true);

            DateTime start = DateTime.Now;


            using (var img = firstFrame.ToImage<Bgr, byte>())
            {
                writer.Write(img.Mat);
            }

            while ((DateTime.Now - start).TotalSeconds < 10)
            {
                using var frame = capture.QueryFrame();

                if (frame != null)
                {
                    using var img = frame.ToImage<Bgr, byte>();
                    writer.Write(img.Mat);
                }

                await Task.Delay(40);
            }
        }
    }
}
