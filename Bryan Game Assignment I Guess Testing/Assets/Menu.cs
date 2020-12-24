using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Video;
using UnityEngine.UI;
using UnityEngine.Audio;

public class Menu : MonoBehaviour
{
    public string[] allowedVideoExtensions;
    public string[] allowedAudioExtensions;

    public static AudioClip selectedClip;
    public static string selectedVideoClipPath;
    public TMP_Dropdown musicDropdown;
    public TMP_Dropdown videoDropdown;

    public VideoPlayer videoPlayer;
    public Toggle useVideoAudioToggle;
    public Toggle loopVideoToggle;
    public TextMeshProUGUI previewButtonLabel;

    private List<string> _videoClips;
    private List<string> _audioClips;

    public static bool useVideoAudio;
    public static bool loopVideo;

    private string _previouslyLoadedVideo;

    public AudioMixerGroup independentMixerGroup;
    public Slider volumeSlider;
    public TextMeshProUGUI volumeText;

    public Button startButton;
    public Button playPreviewButton;

    public void SetVolume(float volume)
    {
        volume = Mathf.Clamp(volume, 0.001f, 10f);
        independentMixerGroup.audioMixer.SetFloat(AudioManager.kVisualizationVolume, Mathf.Log(volume) * 20f);
    }

    private void OnEnable()
    {
        RefreshFiles();
        volumeSlider.value = 1f;
        OnVolumeSliderChanged(1f);
    }

    public void RefreshFiles() {
        selectedVideoClipPath = "";
        selectedClip = null;
        _audioClips = new List<string>();
        _videoClips = new List<string>();

        musicDropdown.ClearOptions();
        videoDropdown.ClearOptions();
        string path = System.IO.Path.Combine(Application.streamingAssetsPath, "Music");
        List<string> elements = new List<string>();
        if (System.IO.Directory.Exists(path))
        {
            foreach (string f in System.IO.Directory.GetFiles(path)) {
                string fName = System.IO.Path.GetFileName(f);
                string fLow = fName.ToLower();
                foreach (string ext in allowedAudioExtensions) {
                    if (fLow.EndsWith(ext) && !fLow.Contains(".meta"))
                    {
                        elements.Add(fName);
                        _audioClips.Add(f);
                    }
                }
            }
        }
        if (elements.Count > 0)
            musicDropdown.AddOptions(elements);

        elements.Clear();

        path = System.IO.Path.Combine(Application.streamingAssetsPath, "Videos");
        if (System.IO.Directory.Exists(path))
        {
            foreach (string f in System.IO.Directory.GetFiles(path))
            {
                string fName = System.IO.Path.GetFileName(f);
                string fLow = fName.ToLower();
                foreach (string ext in allowedVideoExtensions)
                {

                    if (fLow.EndsWith(ext) && !fLow.Contains(".meta"))
                    {
                        elements.Add(fName);
                        _videoClips.Add(f);
                    }
                }
            }
        }
        if (elements.Count > 0)
        {
            videoDropdown.AddOptions(elements);
            selectedVideoClipPath = _videoClips[0];
        }

        List<string> empty = new List<string>(new string[] { "None" });
        if (musicDropdown.options.Count == 0)
            musicDropdown.AddOptions(empty);
        if (videoDropdown.options.Count == 0)
            videoDropdown.AddOptions(empty);
        elements.Clear();
    }

    public void OnVideoAudioToggle() {
        if (useVideoAudioToggle && musicDropdown)
        {
            musicDropdown.gameObject.SetActive(!useVideoAudioToggle.isOn);
            useVideoAudio = useVideoAudioToggle.isOn;
            videoPlayer.audioOutputMode = useVideoAudio ? VideoAudioOutputMode.AudioSource : VideoAudioOutputMode.None;
            videoPlayer.Stop();
        }
    }

    public void OnVideoLoopToggle()
    {
        if (loopVideoToggle && musicDropdown)
        {
            loopVideo = loopVideoToggle.isOn;
            videoPlayer.isLooping = loopVideo;
        }
    }

    public void OnPreviewButtonClicked() {
        if (previewButtonLabel && videoPlayer)
        {
            if (selectedVideoClipPath != _previouslyLoadedVideo)
            {
                _previouslyLoadedVideo = selectedVideoClipPath;
                videoPlayer.url = $"file://{selectedVideoClipPath}";
            }
            if (videoPlayer.isPlaying)
                videoPlayer.Pause();
            else
                videoPlayer.Play();
            //previewButtonLabel.text = $"{(videoPlayer.isPlaying ? "PAUSE" : "PREVIEW")}"; Didn't work idk
        }
    }

    public void OnVideoSelected(int index) {
        if (videoDropdown && index < _videoClips.Count) {
            selectedVideoClipPath = _videoClips[index];
        }
    }

    private void Update()
    {
        if (videoPlayer && previewButtonLabel)
        {
            previewButtonLabel.text = $"{(videoPlayer.isPlaying ? "PAUSE" : "PREVIEW")}";
            videoDropdown.enabled = !videoPlayer.isPlaying && !string.IsNullOrEmpty(selectedVideoClipPath);
            useVideoAudioToggle.interactable = !videoPlayer.isPlaying && !string.IsNullOrEmpty(selectedVideoClipPath);
        }

        if (startButton) {
            startButton.interactable = _audioClips.Count > 0;
        }
    }

    public void OnVolumeSliderChanged(float vol) {
        SetVolume(vol);
        if (volumeText)
            volumeText.text = $"Volume: {Mathf.RoundToInt((vol / 2.718f) * 100f)}";
    }

}
