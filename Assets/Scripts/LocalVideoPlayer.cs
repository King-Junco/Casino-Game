using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Collections.Generic;

public class LocalVideoPlayer : MonoBehaviour
{
    [Header("Video Player")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private RenderTexture renderTexture;

    [Header("UI")]
    [SerializeField] private RawImage videoDisplay;
    [SerializeField] private TMP_Dropdown videoDropdown; // Dropdown to select videos
    [SerializeField] private Button playButton;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button stopButton;
    [SerializeField] private TextMeshProUGUI statusText;

    private List<string> videoFiles = new List<string>();
    private string videoFolderPath;

    void Start()
    {
        // Set up the video folder path (next to Assets folder)
        videoFolderPath = Path.Combine(Application.dataPath, "Videos").Replace("\\", "/");

        // Create folder if it doesn't exist
        if (!Directory.Exists(videoFolderPath))
        {
            Directory.CreateDirectory(videoFolderPath);
            Debug.Log($"Created Videos folder at: {videoFolderPath}");
            UpdateStatus("Videos folder created! Add video files to: " + videoFolderPath);
        }

        // Load videos from folder
        LoadVideosFromFolder();

        // Setup buttons
        if (playButton != null)
        {
            playButton.onClick.AddListener(PlaySelectedVideo);
            playButton.interactable = videoFiles.Count > 0;
        }

        if (pauseButton != null)
        {
            pauseButton.onClick.AddListener(PauseVideo);
            pauseButton.interactable = false;
        }

        if (stopButton != null)
        {
            stopButton.onClick.AddListener(StopVideo);
            stopButton.interactable = false;
        }

        // Setup video player
        if (videoPlayer != null)
        {
            videoPlayer.playOnAwake = false;
            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            videoPlayer.targetTexture = renderTexture;
            videoPlayer.errorReceived += OnVideoError;
        }

        // Setup video display
        if (videoDisplay != null && renderTexture != null)
        {
            videoDisplay.texture = renderTexture;
        }
    }

    void LoadVideosFromFolder()
    {
        videoFiles.Clear();

        if (videoDropdown != null)
        {
            videoDropdown.ClearOptions();
        }

        // Search for video files
        string[] extensions = { "*.mp4", "*.mov", "*.avi", "*.mkv", "*.webm", "*.m4v", "*.wmv" };

        foreach (string extension in extensions)
        {
            string[] foundFiles = Directory.GetFiles(videoFolderPath, extension);
            videoFiles.AddRange(foundFiles);
        }

        if (videoFiles.Count == 0)
        {
            UpdateStatus($"No videos found in folder. Add videos to: {videoFolderPath}");

            if (videoDropdown != null)
            {
                videoDropdown.AddOptions(new List<string> { "No videos found..." });
                videoDropdown.interactable = false;
            }
            return;
        }

        // Populate dropdown with video names
        List<string> videoNames = new List<string>();
        foreach (string filePath in videoFiles)
        {
            videoNames.Add(Path.GetFileName(filePath));
        }

        if (videoDropdown != null)
        {
            videoDropdown.AddOptions(videoNames);
            videoDropdown.interactable = true;
        }

        UpdateStatus($"Found {videoFiles.Count} video(s). Select one and press Play!");
        Debug.Log($"Loaded {videoFiles.Count} videos from: {videoFolderPath}");
    }

    public void PlaySelectedVideo()
    {
        if (videoFiles.Count == 0)
        {
            UpdateStatus("No videos available!");
            return;
        }

        if (videoDropdown == null)
        {
            Debug.LogError("Video dropdown not assigned!");
            return;
        }

        int selectedIndex = videoDropdown.value;

        if (selectedIndex < 0 || selectedIndex >= videoFiles.Count)
        {
            UpdateStatus("Invalid video selection!");
            return;
        }

        string selectedVideo = videoFiles[selectedIndex];
        string videoName = Path.GetFileName(selectedVideo);

        if (!File.Exists(selectedVideo))
        {
            UpdateStatus($"Video file not found: {videoName}");
            return;
        }

        // Load and play the video
        string correctPath = "file:///" + selectedVideo.Replace("\\", "/");
        videoPlayer.url = correctPath;
        videoPlayer.Play();

        UpdateStatus($"Playing: {videoName}");

        if (pauseButton != null)
            pauseButton.interactable = true;
        if (stopButton != null)
            stopButton.interactable = true;
    }

    public void PauseVideo()
    {
        if (videoPlayer != null && videoPlayer.isPlaying)
        {
            videoPlayer.Pause();
            UpdateStatus("Paused");
        }
        else if (videoPlayer != null && videoPlayer.isPaused)
        {
            videoPlayer.Play();
            UpdateStatus("Playing...");
        }
    }

    public void StopVideo()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
            UpdateStatus("Stopped");

            if (pauseButton != null)
                pauseButton.interactable = false;
            if (stopButton != null)
                stopButton.interactable = false;
        }
    }

    void OnVideoError(VideoPlayer vp, string message)
    {
        UpdateStatus($"Error: {message}");
        Debug.LogError($"Video Error: {message}");
    }

    void UpdateStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;

        Debug.Log($"Video Player: {message}");
    }

    // Refresh button (optional - in case you add videos while game is running)
    public void RefreshVideoList()
    {
        LoadVideosFromFolder();
        UpdateStatus("Video list refreshed!");
    }

    void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.errorReceived -= OnVideoError;

            if (videoPlayer.isPlaying)
            {
                videoPlayer.Stop();
            }
        }
    }
}