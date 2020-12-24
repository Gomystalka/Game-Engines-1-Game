using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Video;
using UnityEngine.UI;

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
    public TextMeshProUGUI previewButtonLabel;

    private void OnEnable()
    {
        RefreshFiles();
    }

    public void RefreshFiles() {
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
                        elements.Add(fName);
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
                        elements.Add(fName);
                }
            }
        }
        if (elements.Count > 0)
            videoDropdown.AddOptions(elements);

        List<string> empty = new List<string>(new string[] { "None" });
        if (musicDropdown.options.Count == 0)
            musicDropdown.AddOptions(empty);
        if (videoDropdown.options.Count == 0)
            videoDropdown.AddOptions(empty);
    }

    public void OnVideoAudioToggle() {
        if (useVideoAudioToggle && musicDropdown)
            musicDropdown.gameObject.SetActive(!useVideoAudioToggle.isOn);
    }

    public void OnPreviewButtonClicked() {
        if (previewButtonLabel && videoPlayer)
        {
            if (videoPlayer.isPlaying)
            {
                videoPlayer.Pause();
                previewButtonLabel.text = "PLAY";
            }
            else
            {
                videoPlayer.Play();
                previewButtonLabel.text = "PAUSE";
            }
            //previewButtonLabel.text = $"{(videoPlayer.isPlaying ? "PAUSE" : "PREVIEW")}"; Didn't work idk
        }
    }

}
