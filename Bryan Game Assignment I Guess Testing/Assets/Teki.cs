using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Teki : AudioBehaviour
{
    private MeshFilter _meshFilter;
    private MeshRenderer _renderer;
    private Mesh _mesh;
    private Vector3[] _baseVertices;

    private int _vertexIndex = 0;
    [SerializeField] private float[] _vertexInfo;
    private float _currentAverage;
    private int _framesBetweenCaptures;
    public bool Capturing {
        get {
            return _vertexIndex < _mesh.vertexCount;
        }
    }

    //private Timer _dataCaptureTimer; //Too imprecise for lower delays

    [Header("Visualization")]
    public float independentSampleScalar = 100f;

    public override void OnEnable()
    {
        base.OnEnable();
        _meshFilter = GetComponent<MeshFilter>();
        _renderer = GetComponent<MeshRenderer>();
        if (_meshFilter)
            _mesh = _meshFilter.sharedMesh;
        if (_mesh)
        {
            _vertexInfo = new float[_mesh.vertexCount];
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
        EndDataCapture();
        StartCoroutine(CaptureData(VisualizationSettings.kEnemyDataCaptureIntervalSeconds / (float)_mesh.vertexCount));
    }

    public override void Update()
    {
        base.Update();
        //if (_dataCaptureTimer == null) return;
        //if (!_dataCaptureTimer.IsStopped)
        //{
        if (Capturing)
        {
            _framesBetweenCaptures++;
            _currentAverage += FFTSpectrumDataLeft.CalculateAverage() * independentSampleScalar;
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

    private void ManipulateVertices() {
        Mesh m = _meshFilter.mesh; //Get mesh instance
        Vector3[] _vertices = _baseVertices;
        for (int i = 0; i < _vertexInfo.Length; i++)
                _vertices[i] += _vertexInfo[i] * SampleScalar * _mesh.normals[i];
        m.vertices = _vertices;
        m.RecalculateNormals();
        m.RecalculateBounds();
        //m.RecalculateTangents();
    }

    private Vector3 RandomizeDirection() {
        return new Vector3(Random.Range(-1, 2), Random.Range(-1, 2), Random.Range(-1, 2));
    }
}