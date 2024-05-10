using AxWMPLib;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public class VideoManagerService
{

    private System.Timers.Timer playbackTimer;
    private Queue<string> videoQueue = new Queue<string>();
    private List<string> videoFilePaths = new List<string>();
    private AxWindowsMediaPlayer mediaPlayer;
    public VideoManagerService(AxWindowsMediaPlayer player)
    {
        mediaPlayer = player;
        mediaPlayer.PlayStateChange += new AxWMPLib._WMPOCXEvents_PlayStateChangeEventHandler(AxWindowsMediaPlayer1_PlayStateChange);
        LoadVideoFilePaths();

        playbackTimer = new System.Timers.Timer();
        playbackTimer.Elapsed += PlaybackTimer_Elapsed;

        PlayNextVideo();
    }

    private void LoadVideoFilePaths()
    {
        string appDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        string videosFolder = Path.Combine(appDirectory, "assets", "Videos");

        videoFilePaths = Directory.EnumerateFiles(videosFolder, "*.mp4").ToList();

        videoFilePaths.Sort((x, y) =>
        {
            int xPrefix = int.Parse(Path.GetFileNameWithoutExtension(x).Split('_')[0]);
            int yPrefix = int.Parse(Path.GetFileNameWithoutExtension(y).Split('_')[0]);
            return xPrefix.CompareTo(yPrefix);
        });

        videoQueue = new Queue<string>(videoFilePaths);
    }

    public void PlayNextVideo()
    {
        if (videoQueue.Count > 0)
        {
            string videoPath = videoQueue.Dequeue();
            mediaPlayer.URL = videoPath;
            mediaPlayer.Ctlcontrols.play();
            mediaPlayer.uiMode = "none";
            mediaPlayer.stretchToFit = true;

            // Check if the video duration is greater than 0
            if (mediaPlayer.currentMedia.duration > 0)
            {
                // Set the timer interval to the duration of the current video
                playbackTimer.Interval = (mediaPlayer.currentMedia.duration + 1) * 1000;
                playbackTimer.Start();
            }
            else
            {
                // Handle the case where the video duration is 0 or unavailable
                // You can set a default interval or handle it differently based on your requirements
                playbackTimer.Interval = 100000; // Set a default interval of 1minute
                playbackTimer.Start();
            }
        }
        else
        {
            // Repopulate the queue with the video file paths
            videoQueue = new Queue<string>(videoFilePaths);
            PlayNextVideo();
        }
    }

    private async void AxWindowsMediaPlayer1_PlayStateChange(object sender, _WMPOCXEvents_PlayStateChangeEvent e)
    {
        if (e.newState == 8) // 8 represents MediaEnded state
        {
            await Task.Delay(200); // Wait for 200 milliseconds
            PlayNextVideo();
        }
    }

    private void PlaybackTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
        playbackTimer.Stop(); // Stop the timer
        PlayNextVideo(); // Play the next video
    }
}
