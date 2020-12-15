using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[DefaultExecutionOrder(-10)]
public sealed class AudioManager : MonoBehaviour {
    public const string kVisualizationVolume = "VisualizationVolume";

    [Header("Audio")]
    public int sampleCount;
    public FFTWindow fftType;
    public float frequencySmoothing;
    public float sampleScalar;
    public AudioSource source;
    public float lowestFrequency = 24f;
    [Range(0.001f, 3f)] public float volume = 2.718f; //2.718 = +20Db

    [Header("Microphone Input")]
    public bool useMicrophoneInput;
    public int microphoneAudioClipLength = 12;

    [Header("Audio Mixer Groups")]
    public AudioMixerGroup mainMixerGroup;
    public AudioMixerGroup audioDeviceInputGroup;
    public AudioMixerGroup volumeIndependentMixerGroup;

    public string CurrentAudioDevice { get; private set; }
    public int CurrentAudioDeviceIndex { get; private set; }

    private FrequencyBand[] _frequencyBands;
    public FrequencyBand[] FrequencyBands { get { return _frequencyBands ?? new FrequencyBand[0]; } }
    public AudioClip Clip { get { return source?.clip; } }
    public static int SampleRate { get { return AudioSettings.outputSampleRate; } }
    public float[] FFTSpectrumData { get; private set; }

    public static AudioManager Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Debug.LogWarning($"[{GetType().FullName}] There is already an AudioManager in the scene! The AudioManager component on the object [{name}] will be removed.");
            Destroy(this);
            return;
        }
        if (volumeIndependentMixerGroup)
            SetVolume(volume);
    }

    private void OnEnable() {
        if (!source)
            source = GetComponent<AudioSource>();

        if (!source)
            Debug.LogWarning($"[{GetType().FullName}] Audio Source component is missing. This AudioBehaviour will not function!");
        sampleCount = Mathf.ClosestPowerOfTwo(sampleCount);
        _frequencyBands = new FrequencyBand[(int)(Mathf.Log(sampleCount) / Mathf.Log(2f))];

        if (useMicrophoneInput)
            SetAudioDevice(0);
        else
            source.outputAudioMixerGroup = volumeIndependentMixerGroup;
    }

    public void SetAudioDevice(int deviceIndex) {
        if (!source) return;
        string[] devices = Microphone.devices;
        if (devices == null || devices.Length == 0)
        {
            Debug.LogWarning($"[{GetType().FullName}] Failed to initialize Microphone Input. There are no suitable devices connected to your device!");
            return;
        }
        else
            source.outputAudioMixerGroup = audioDeviceInputGroup;

        source.Stop();
        if(Microphone.IsRecording(CurrentAudioDevice))
            Microphone.End(CurrentAudioDevice);
        if (deviceIndex < 0) deviceIndex = 0;
        if (deviceIndex >= devices.Length) deviceIndex = devices.Length - 1;
        CurrentAudioDevice = devices[deviceIndex];
        Debug.Log($"Listening to Selected Audio Device: {CurrentAudioDevice}");
        source.clip = Microphone.Start(CurrentAudioDevice, true, microphoneAudioClipLength, AudioSettings.outputSampleRate);
        source.loop = true;
        source.Play();
    }

    private void Update()
    {
        if (!source) return;
        sampleCount = Mathf.ClosestPowerOfTwo(sampleCount);
        FFTSpectrumData = new float[sampleCount];
        source.GetSpectrumData(FFTSpectrumData, 0, fftType);
        FindFrequencyBands();
        SetVolume(volume);
    }

    public void ResetSpectrum()
    {
        if (!source) return;
        source.Stop();
        FFTSpectrumData = new float[sampleCount];
        _frequencyBands = new FrequencyBand[(int)(Mathf.Log(sampleCount) / Mathf.Log(2f))];
        source.Play();
    }

    private void FindFrequencyBands()
    {
        //int fff = 0;
        float hzPerBin = (SampleRate / 2f) / sampleCount;
        //Debug.Log($"Hz per Bin: {hzPerBin}");
        for (int i = 0; i < FrequencyBands.Length; i++)
        {
            /*
            int start = (int)Mathf.Pow(2, i) - 1; //-1 because 2^0 == 1 so Index 0 is not captured
            int end = (start + 1) + start;
            float average = 0f;
            for (int k = start; k < end; k++)
                average += spectrum[k] * (k + 1);
            average /= start + 1;

            /*
            float avg = 0f;
            int c = (int)Mathf.Pow(2, i) * 2;
            if (i == _frequencyBands.Length - 1)
                c += 2;
            for (int k = 0; k < c; k++)
            {
                avg += spectrum[fff] * (fff + 1);
                fff++;
            }

            avg /= fff;
            */


            int s = (int)Mathf.Pow(2, i) - 1;
            int e = s + s + 1;
            float avg = 0f;
            for (int k = s; k < e; k++)
                avg += FFTSpectrumData[s] * (k + 1);
            avg /= s + 1;

            FrequencyBand band = _frequencyBands[i];
            band.frequencyRange = new Range(s * hzPerBin, e * hzPerBin);
            band.frequency = avg * sampleScalar;
            band.smoothedFrequency = Mathf.MoveTowards(band.smoothedFrequency, band.frequency, frequencySmoothing);

            if (band.frequency > band.maxFrequency)
                band.maxFrequency = band.frequency;
            band.mappedFrequency = band.frequency / band.maxFrequency;
            band.smoothedMappedFrequency = band.smoothedFrequency / band.maxFrequency;
            _frequencyBands[i] = band;
        }
    }

    public float GetAverageFrequencyInRange(float freqLow, float freqHigh, bool useScalar = false) {
        float halfSampleRate = SampleRate / 2f;
        freqLow = Mathf.Clamp(freqLow, lowestFrequency, halfSampleRate);
        freqHigh = Mathf.Clamp(freqHigh, lowestFrequency, halfSampleRate);

        int startIndex = (int)(freqLow * sampleCount / halfSampleRate);
        int endIndex = (int)(freqHigh * sampleCount / halfSampleRate);

        float average = 0f;
        float scalar = useScalar ? sampleScalar : 1f;
        for (int i = startIndex; i <= endIndex; i++)
            average += FFTSpectrumData[i] * scalar;
        return average / (endIndex - startIndex + 1);
    }

    public void SetVolume(float volume) {
        volume = Mathf.Clamp(volume, 0.001f, 10f);
        volumeIndependentMixerGroup.audioMixer.SetFloat(kVisualizationVolume, Mathf.Log(volume) * 20f);
    }
}

public class Timer {
    public bool repeating;
    public float interval;
    public System.Action action;

    public float Time { get; private set; }
    public bool IsStopped { get; private set; }

    private Timer() { }

    public static Timer ScheduleTimer(System.Action action, float interval, bool repeating) {
        return new Timer() {
            action = action,
            interval = interval,
            repeating = repeating
        };
    }

    public void WaitForTimer(float timePassed) {
        if (IsStopped) return;
        if (Time >= interval)
        {
            Time = 0;
            IsStopped = !repeating;
            action?.Invoke();
        }
        else
            Time += timePassed;
    }

    public void Resume() => IsStopped = false;
    public void Pause() => IsStopped = true;
    public void Reset() {
        IsStopped = false;
        Time = 0;
    }
}

public static class Extensions {
    public static float SumOf(this float[] arr) {
        float c = 0;
        foreach (float t in arr)
            c += t;
        return c;
    }

    public static float CalculateAverage(this float[] arr) {
        return arr.SumOf() / arr.Length;
    }

    public static bool IsWhole(this float f) {
        return f % 1 == 0;
    }

    public static string ElementsAsString<T>(this T[] arr) {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        foreach (T s in arr)
            sb.AppendLine(s.ToString());
        return sb.ToString();
    }

    public static Rect Move(this Rect rect, float x, float y) {
        rect.x += x;
        rect.y += y;
        return rect;
    }

    public static Rect ResizeBy(this Rect rect, float width, float height)
    {
        rect.width += width;
        rect.height += height;
        return rect;
    }

    public static Rect Resize(this Rect rect, float width, float height)
    {
        rect.width = width;
        rect.height = height;
        return rect;
    }

    public static Rect Resize(this Rect rect, Vector2 size)
    {
        rect.width = size.x;
        rect.height = size.y;
        return rect;
    }

    public static System.Reflection.FieldInfo GetField<T>(this T t, string field) {
        return t.GetType().GetField(field);
    }

    public static string[] ToStringArray(this List<Bonanza> list) {
        string[] arr = new string[list.Count];
        for (int i = 0; i < arr.Length; i++)
            arr[i] = list[i].fieldName;
        return arr;
    }

    public static Vector3[] GetVectors(this List<Line> lineList)
    {
        List<Vector3> vectors = new List<Vector3>();
        for (int i = 0; i < lineList.Count; i++)
        {
            for (int k = 0; k < 2; k++)
                vectors.Add(lineList[i].GetVector(k));
        }
        if (vectors.Count % 2 != 0)
            vectors.Add(Vector3.one);
        return vectors.ToArray();
    }
}

public struct Bonanza
{
    public string fieldName;
    public bool randomizeValue;
}

[System.Serializable]
public struct FrequencyBand {
    public float frequency;
    public float maxFrequency;
    public float mappedFrequency;
    public float smoothedFrequency;
    public float smoothedMappedFrequency;
    public Range frequencyRange;
}

[System.Serializable]
public struct Range {
    public float low;
    public float high;
    public Int Integer { get { return new Int((int)low, (int)high); } }

    public Range(float low, float high) {
        this.low = low;
        this.high = high;
    }

    public override string ToString()
    {
        return $"Low: {low} | High: {high}";
    }

    public class Int {
        public int low;
        public int high;

        public Int(int low, int high) {
            this.low = low;
            this.high = high;
        }

        public override string ToString()
        {
            return $"Low: {low} | High: {high}";
        }
    }
}