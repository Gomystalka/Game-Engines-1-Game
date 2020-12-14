using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AudioBehaviour : MonoBehaviour
{
    protected AudioManager ManagerInstance { get; private set; }
    protected int Samples { get { return ManagerInstance.sampleCount; } }
    protected FFTWindow FFTWindow { get { return ManagerInstance.fftType; } set { ManagerInstance.fftType = value; } }
    protected float Smoothing { get { return ManagerInstance.frequencySmoothing; } set { ManagerInstance.frequencySmoothing = value; } }
    protected float SampleScalar { get { return ManagerInstance.sampleScalar; } set { ManagerInstance.sampleScalar = value; } }
    protected AudioSource Source { get { return ManagerInstance.source; }}
    protected float LowestFrequency { get { return ManagerInstance.lowestFrequency; } set { ManagerInstance.lowestFrequency = value; } }
    protected UnityEngine.Audio.AudioMixerGroup MainMixerGroup { get { return ManagerInstance.mainMixerGroup; }}
    protected UnityEngine.Audio.AudioMixerGroup VolumeIndependentGroup { get { return ManagerInstance.volumeIndependentMixerGroup; } }
    protected UnityEngine.Audio.AudioMixerGroup InputDeviceMixerGroup { get { return ManagerInstance.audioDeviceInputGroup; } }
    protected FrequencyBand[] FrequencyBands { get { return ManagerInstance.FrequencyBands; } }
    protected AudioClip Clip { get { return ManagerInstance.Clip; } }
    protected int SampleRate { get { return AudioManager.SampleRate; } }
    protected float HalfSampleRate { get { return AudioManager.SampleRate / 2f; } }
    protected float[] PreFFTSpectrumData { get { return ManagerInstance.PreFFTSpectrumData; } }

    private bool _behaviourReady;

    public virtual void OnEnable()
    {
        ManagerInstance = AudioManager.Instance;
        if (!ManagerInstance)
            ManagerInstance = FindObjectOfType<AudioManager>();
        if (!ManagerInstance)
            Debug.LogWarning($"[{GetType().FullName}] Failed to load AudioBehaviour. Audio Manager instance has not been set!");
        _behaviourReady = true;
    }

    public virtual void Update()
    {
        if (!_behaviourReady)
        {
            Debug.LogWarning($"[{GetType().FullName}]: AudioBehaviour failed to initialize. Internal OnEnable call did not succeed. When overriding OnEnable, base.OnEnable() must be used!");
            return;
        }
    }

    protected void SetInputDevice(int index) {
        if (!ManagerInstance) return;
        ManagerInstance.SetAudioDevice(index);
    }

    protected void ResetSpectrumData() {
        if (!ManagerInstance) return;
        ManagerInstance.ResetSpectrum();
    }
    //No null check for maximum performance
    protected void GetAverageFrequencyInRange(float lowestFrequency, float highestFrequency, bool useScalar = false) =>
        ManagerInstance.GetAverageFrequencyInRange(lowestFrequency, highestFrequency, useScalar);
}
