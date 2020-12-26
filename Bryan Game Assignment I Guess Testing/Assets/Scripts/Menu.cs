using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Video;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.Networking;
using NAudio.Wave;
using UnityEngine.SceneManagement;

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
    public TextMeshProUGUI audioPreviewButtonLabel;

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

    public AudioSource previewSource;
    public GameObject previewObject;

    public void SetVolume(float volume)
    {
        volume = Mathf.Clamp(volume, 0.001f, 10f);
        independentMixerGroup.audioMixer.SetFloat(AudioManager.kVisualizationVolume, Mathf.Log(volume) * 20f);
    }

    private void OnEnable()
    {
        RefreshFiles();
        volumeSlider.value = 1f;
        //OnVolumeSliderChanged(1f);
        loopVideo = false;
        useVideoAudio = false;
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
        _videoClips.Add("None");
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
        {
            musicDropdown.AddOptions(elements);
            OnAudioClipSelected(0);
        }

        elements.Clear();
        elements.Add("None");
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
        if (elements.Count > 1)
        {
            videoDropdown.AddOptions(elements);
            selectedVideoClipPath = _videoClips[0];
        }

        List<string> empty = new List<string>(new string[] { "None" });
        if (musicDropdown.options.Count == 0)
            musicDropdown.AddOptions(empty);
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
            if (selectedVideoClipPath == "None") return;
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

    public void OnAudioPreviewButtonClicked()
    {
        if (previewSource) {
            if (previewSource.isPlaying)
                previewSource.Pause();
            else
                previewSource.Play();
        }
    }

    public void OnVideoSelected(int index)
    {
        if (index == 0) return;
        if (videoDropdown && index < _videoClips.Count)
        {
            selectedVideoClipPath = _videoClips[index];
        }
    }

    public void OnAudioClipSelected(int index) {
        if (musicDropdown && index < _audioClips.Count)
        {
            string f = _audioClips[index];
            StartCoroutine(GetAudioClip(f, System.IO.Path.GetFileName(f), index));
        }
    }

    IEnumerator GetAudioClip(string path, string name, int replaceIndex)
    {
        AudioType t = GetTypeFromFile(name);
        if (t != AudioType.MPEG) {

            using (UnityWebRequest r = UnityWebRequestMultimedia.GetAudioClip(path, t))
            {
                yield return r.SendWebRequest();

                if (r.isNetworkError || r.isHttpError)
                    Debug.Log(r.error);
                else
                {
                    selectedClip = DownloadHandlerAudioClip.GetContent(r);
                    if (selectedClip)
                        previewSource.clip = selectedClip;
                }
            }

            if (selectedClip)
                previewSource.clip = selectedClip;
        } else {
            string p = ConvertMPEG(path, replaceIndex);
            if (replaceIndex < _audioClips.Count)
            {
                _audioClips[replaceIndex] = p;
                RefreshFiles();
            }
        }
    }

    private string ConvertMPEG(string path, int index) {
        using (Mp3FileReader m = new Mp3FileReader(path)) {
            using (WaveStream s = WaveFormatConversionStream.CreatePcmStream(m)) {
                WaveFileWriter.CreateWaveFile(path + " WAV VER.wav", s);
            }
        }
        string newPath = path + " WAV VER.wav";

        if (!new System.IO.DirectoryInfo(path).Exists)
            System.IO.File.Delete(path);
        StartCoroutine(GetAudioClip(newPath, System.IO.Path.GetFileName(path + " WAV VER.wav"), index));
        return newPath;
    }

    private AudioType GetTypeFromFile(string file) {
        if (file.EndsWith(".aif"))
            return AudioType.AIFF;
        else if (file.EndsWith(".wav"))
            return AudioType.WAV;
        else if (file.EndsWith(".mp3"))
            return AudioType.MPEG;
        else
            return AudioType.OGGVORBIS;
    }

    private void Update()
    {
        if (videoPlayer && previewButtonLabel)
        {
            previewButtonLabel.text = $"{(videoPlayer.isPlaying ? "PAUSE" : "PREVIEW")}";
            audioPreviewButtonLabel.text = $"{(previewSource.isPlaying ? "PAUSE" : "PREVIEW")}";
            videoDropdown.enabled = !videoPlayer.isPlaying && !string.IsNullOrEmpty(selectedVideoClipPath);
            useVideoAudioToggle.interactable = !videoPlayer.isPlaying && !string.IsNullOrEmpty(selectedVideoClipPath) && selectedVideoClipPath != "None";
            previewObject.SetActive(selectedVideoClipPath != "None");
        }

        if (startButton) {
            startButton.interactable = _audioClips.Count > 0 && selectedClip;
        }
    }

    public void OnVolumeSliderChanged(float vol) {
        SetVolume(vol);
        if (volumeText)
            volumeText.text = $"Volume: {Mathf.RoundToInt((vol / 2.718f) * 100f)}";
    }

    public void LoadScene(int sceneIndex) {
        SceneManager.LoadScene(sceneIndex);
    }
}
