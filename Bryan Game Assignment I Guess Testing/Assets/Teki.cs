using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Teki : AudioBehaviour
{
    private MeshFilter _meshFilter;
    private MeshCollider _collider;
    private MeshRenderer _renderer;
    private Mesh _mesh;
    private Vector3[] _baseVertices;

    public const int kHitPoints = 4;

    //private int _vertexIndex = 0;
    //[SerializeField] private float[] _vertexInfo;
    //private float _currentAverage;
    //private int _framesBetweenCaptures;
    /*
    public bool Capturing {
        get {
            return _vertexIndex < _mesh.vertexCount;
        }
    }
    */

    //private Timer _dataCaptureTimer; //Too imprecise for lower delays

    [Header("Visualization")]
    public float independentSampleScalar = 100f;

    private float _spinSpeed;
    private Vector3 _direction;
    private int _hitPoints;

    public override void OnEnable()
    {
        base.OnEnable();
        _meshFilter = GetComponent<MeshFilter>();
        _renderer = GetComponent<MeshRenderer>();
        _collider = GetComponent<MeshCollider>();
        if (_meshFilter)
            _mesh = _meshFilter.sharedMesh;
        if (_mesh)
        {
            //_vertexInfo = new float[_mesh.vertexCount];
            _baseVertices = _mesh.vertices;
        }
        else
        {
            Debug.LogWarning($"[{GetType().FullName}:{name}] The attached MeshFilter contains no mesh!");
            return;
        }
        //if (_dataCaptureTimer != null) _dataCaptureTimer.Reset();

        /*
        _dataCaptureTimer = Timer.ScheduleTimer(delegate {
            _vertexInfo[_vertexIndex] = _currentAverage / _framesBetweenCaptures;
            if (_vertexInfo[_vertexIndex] < VisualizationSettings.kMinimumPeak)
                _vertexInfo[_vertexIndex] = 0f;
            _currentAverage = 0f;
            _framesBetweenCaptures = 0;
            _vertexIndex++;
            Debug.Log("Huh");
            if (_vertexIndex >= _mesh.vertexCount)
                EndDataCapture();
        }, VisualizationSettings.kEnemyDataCaptureIntervalSeconds / _mesh.vertexCount, true);
        */
        /*
        EndDataCapture();
        StartCoroutine(CaptureData(VisualizationSettings.kEnemyDataCaptureIntervalSeconds / (float)_mesh.vertexCount));
    */
        _spinSpeed = Random.Range(5f, 30f);
        _direction = RandomizeDirection();
        _hitPoints = kHitPoints;
        //ManipulateVertices();
    }

    /*
    public override void Update()
    {
        base.Update();
        //if (_dataCaptureTimer == null) return;
        //if (!_dataCaptureTimer.IsStopped)
        //{
        if (Capturing)
        {
            _framesBetweenCaptures++;
            _currentAverage += FFTSpectrumDataChannel0.CalculateAverage() * independentSampleScalar;
        }
            //_dataCaptureTimer.WaitForTimer(Time.deltaTime);
        //}
    }

    private IEnumerator CaptureData(float updateFrequency) {
        Debug.Log($"Capture Loop: {updateFrequency}s per update for {_mesh.vertexCount} updates.");
        while (_vertexIndex < _mesh.vertexCount)
        {
            yield return new WaitForSeconds(updateFrequency);
            _vertexInfo[_vertexIndex] = _currentAverage / _framesBetweenCaptures;
            if (_vertexInfo[_vertexIndex] < VisualizationSettings.kMinimumPeak)
                _vertexInfo[_vertexIndex] = 0f;
            _currentAverage = 0f;
            _framesBetweenCaptures = 0;
            _vertexIndex++;
        }
        EndDataCapture();
    }

    private void EndDataCapture() {
        StopAllCoroutines();
        _currentAverage = 0f;
        _framesBetweenCaptures = 0;
        //_dataCaptureTimer.Reset();
        //_dataCaptureTimer = null;
        Debug.Log($"Captured {_vertexInfo.Length} samples.");
        ManipulateVertices();
    }
    */
    public override void Update()
    {
        base.Update();
        if (_renderer)
        {
            Material mat = _renderer.material;
            mat.color = Color.Lerp(mat.color, Color.HSVToRGB(VisualizedCustomTerrain.InverseHue, 1f, 1f, true) * 1.05f, Time.deltaTime * 4f);
            mat.SetColor("_EmissionColor", mat.color);
        }

        transform.Rotate(transform.TransformDirection(_direction), _spinSpeed * Time.deltaTime);
    }

    public void ManipulateVertices(float maxHeight) {
        Mesh m = _meshFilter.mesh; //Get mesh instance
        Vector3[] _vertices = _baseVertices;
        int spectrumFactor = Mathf.FloorToInt(_baseVertices.Length / FrequencyBands.Length - 1);
        Debug.Log(FrequencyBands.Length);
        for (int i = 0; i < _baseVertices.Length; i++)
        {
            _vertices[i] += m.normals[i] * FrequencyBands[Mathf.Clamp(Mathf.FloorToInt(i / spectrumFactor), 0, FrequencyBands.Length - 1)].frequency * 0.5f;
            Vector3 v = _vertices[i];
            v.x = Mathf.Clamp(v.x, -maxHeight, maxHeight);
            v.y = Mathf.Clamp(v.y, -maxHeight, maxHeight);
            v.z = Mathf.Clamp(v.z, -maxHeight, maxHeight);
            _vertices[i] = v;
        }
                //_vertices[i] += _vertexInfo[i] * SampleScalar * _mesh.normals[i];
        m.vertices = _vertices;
        m.RecalculateNormals();
        m.RecalculateBounds();
        _collider.sharedMesh = m;
        //m.RecalculateTangents();
    }

    private Vector3 RandomizeDirection() {
        return new Vector3(Random.Range(-1, 2), Random.Range(-1, 2), Random.Range(-1, 2));
    }

    private void Kill() {
        AudioManager.Instance.SpawnParticle(0, transform.position);
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Rocket"))
        {
            Kill();
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("Projectile")) {
            _hitPoints--;
            if (_hitPoints <= 0)
                Kill();
            Destroy(other.gameObject);
        }
    }
}