using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public abstract class AudioBehaviour : MonoBehaviour {
    [Header("Audio")] //Doesn't work with this but I'll keep it anyway UwU
    public int sampleCount;
    public FFTWindow fftType;
    public float volume;
    public float frequencySmoothing;
    public float sampleScalar;
    public AudioSource source;
    public float lowestFrequency = 24f;

    [Header("Microphone Input")]
    public bool useMicrophoneInput;
    public int microphoneAudioClipLength = 12;

    [Header("Audio Mixer Groups")]
    public AudioMixerGroup mainMixerGroup;
    public AudioMixerGroup audioDeviceInputGroup;

    private string _currentAudioDevice;
    private int _audioDeviceIndex;

    private FrequencyBand[] _frequencyBands;

    protected FrequencyBand[] FrequencyBands { get { return _frequencyBands ?? new FrequencyBand[0]; } }
    protected AudioClip Clip { get { return source?.clip; } }
    protected int? SampleRate { get { return AudioSettings.outputSampleRate; } }

    protected float[] spectrum;

    private bool _behaviourReady;

    public virtual void OnEnable() {
        if (!source)
            source = GetComponent<AudioSource>();

        if (!source)
            Debug.LogError($"[{nameof(AudioBehaviour)}] Audio Source component is missing. This AudioBehaviour will not function!");
        sampleCount = Mathf.ClosestPowerOfTwo(sampleCount);
        _frequencyBands = new FrequencyBand[(int)(Mathf.Log(sampleCount) / Mathf.Log(2f))];
        //_frequencyBands = new FrequencyBand[3];
        _behaviourReady = true;

        if (useMicrophoneInput)
            SetAudioDevice(0);
        else
            source.outputAudioMixerGroup = mainMixerGroup;
    }

    private void SetAudioDevice(int deviceIndex) {
        if (!source) return;
        source.Stop();
        source.outputAudioMixerGroup = audioDeviceInputGroup;
        if(Microphone.IsRecording(_currentAudioDevice))
            Microphone.End(_currentAudioDevice);
        string[] devices = Microphone.devices;
        if (deviceIndex < 0) deviceIndex = 0;
        if (deviceIndex >= devices.Length) deviceIndex = devices.Length - 1;
        _currentAudioDevice = devices[deviceIndex];
        Debug.Log($"Selected Audio Device: {_currentAudioDevice}");
        source.clip = Microphone.Start(_currentAudioDevice, true, microphoneAudioClipLength, AudioSettings.outputSampleRate);
        source.loop = true;
        source.Play();
    }

    public virtual void Update()
    {
        if (!source) return;
        if (!_behaviourReady) {
            Debug.LogError($"[{GetType().Name}]: AudioBehaviour failed to initialize. Internal OnEnable call did not succeed. When overriding OnEnable, base.OnEnable() must be used!");
            return;
        }
        sampleCount = Mathf.ClosestPowerOfTwo(sampleCount);
        spectrum = new float[sampleCount];
        source.GetSpectrumData(spectrum, 0, fftType);
        FindFrequencyBands();
    }

    public void ResetSpectrum()
    {
        if (!source) return;
        source.Stop();
        spectrum = new float[sampleCount];
        _frequencyBands = new FrequencyBand[(int)(Mathf.Log(sampleCount) / Mathf.Log(2f))];
        source.Play();
    }

    private void FindFrequencyBands()
    {
        //int fff = 0;
        float hzPerBin = (float)(SampleRate / 2f) / sampleCount;
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
                avg += spectrum[s] * (k + 1);
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

    protected float GetAverageFrequencyInRange(float freqLow, float freqHigh, bool useScalar = false) {
        float halfSampleRate = (float)SampleRate / 2f;
        freqLow = Mathf.Clamp(freqLow, lowestFrequency, halfSampleRate);
        freqHigh = Mathf.Clamp(freqHigh, lowestFrequency, halfSampleRate);

        int startIndex = (int)(freqLow * sampleCount / halfSampleRate);
        int endIndex = (int)(freqHigh * sampleCount / halfSampleRate);

        float average = 0f;
        float scalar = useScalar ? sampleScalar : 1f;
        for (int i = startIndex; i <= endIndex; i++)
            average += spectrum[i] * scalar;
        return average / (endIndex - startIndex + 1);
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