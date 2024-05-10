using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace Price_Checker.Services
{
    internal class ImagesManagerService
    {
        private Queue<string> imageQueue = new Queue<string>();
        private readonly Dictionary<string, string> imagePaths = new Dictionary<string, string>();

        private System.Windows.Forms.Timer imageLoopTimer;
        private readonly System.Windows.Forms.PictureBox pictureBox1;

        public ImagesManagerService(System.Windows.Forms.PictureBox pictureBox)
        {
            this.pictureBox1 = pictureBox;
        }

        public void ImageSlideshow()
        {
            imageLoopTimer = new System.Windows.Forms.Timer();
            imageLoopTimer.Interval = 5000; // 5 seconds
            imageLoopTimer.Tick += DisplayNextImage;
            imageLoopTimer.Start();

            // Get the directory path of the currently executing assembly
            string appDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string imagesFolder = Path.Combine(appDirectory, "assets", "Images");

            // Fetch all image files in the specified directory
            List<string> imageFiles = Directory.EnumerateFiles(imagesFolder, "*.jpg").ToList();

            // Sort the image files based on the numeric prefix
            imageFiles.Sort((x, y) =>
            {
                int xPrefix = int.Parse(Path.GetFileNameWithoutExtension(x).Split('_')[0]);
                int yPrefix = int.Parse(Path.GetFileNameWithoutExtension(y).Split('_')[0]);
                return xPrefix.CompareTo(yPrefix);
            });

            // Populate the imageQueue with the sorted image file paths
            foreach (string imagePath in imageFiles)
            {
                imageQueue.Enqueue(imagePath);
            }

            // Display the first image
            DisplayNextImage(null, EventArgs.Empty);
        }
        public void DisplayNextImage(object sender, EventArgs e)
        {
            if (imageQueue.Count == 0)
            {
                // Repopulate the imageQueue with the image file paths
                imageQueue = new Queue<string>(imagePaths.Values);
            }

            if (imageQueue.Count > 0)
            {
                string imagePath = imageQueue.Dequeue();
                pictureBox1.Image = Image.FromFile(imagePath);
                imageQueue.Enqueue(imagePath); // Add the image back to the end of the queue
            }
        }
    }
}