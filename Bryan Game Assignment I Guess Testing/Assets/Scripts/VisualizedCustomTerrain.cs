using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

[RequireComponent(typeof(CustomTerrain))]
[DefaultExecutionOrder(-10)]
public class VisualizedCustomTerrain : AudioBehaviour
{
    [Header("Terrain Respawning")]
    public Vector3 respawnOffset;
    public Vector3 respawnOffsetDirection;
    public Transform player;
    public float respawnThresholdCheckResetTime = 2f;
    public float heightScalar;
    public Vector2 perlinNoiseZoom;

    private CustomTerrain _terrain;
    private bool _canRespawn = true;

    [Header("Visualization Settings")]
    public float wireScalar = 1000f;
    public float wireThicknessRate = 10f;
    public int wireFrequencyBandIndex = 0;
    public float scaledBeatStrengthThreshold;
    public float wireColorIntensity;

    [Header("Enemy Spawns")]
    public Range spawnAreaRangeX;
    public Range spawnAreaRangeY;
    public float maxEnemySize;
    public float minTimeBetweenSpawns = 0.5f;
    public float spawnDistance;
    public GameObject enemyPrefab;

    public static float Hue { get; private set; }
    public static float InverseHue { get; private set; }

    public VideoPlayer videoPlayer;
    private float _spawnTimer;

    public override void OnEnable()
    {
        base.OnEnable();
        _terrain = GetComponent<CustomTerrain>();
        _canRespawn = true;
        GenerateVisualizedHeight(true);
        OnBeatDetected.AddListener(OnBeat);
        if (Source && !Menu.useVideoAudio)
        {
            Source.clip = Menu.selectedClip;
            Source.Play();
        }
        if (Source && Menu.useVideoAudio)
            Source.clip = null;

        if (videoPlayer)
        {
            string u = Menu.selectedVideoClipPath;
            videoPlayer.enabled = !string.IsNullOrEmpty(u) && u != "None";
            if (!videoPlayer.enabled) return;

            videoPlayer.url = u;
            videoPlayer.isLooping = Menu.loopVideo;
            videoPlayer.audioOutputMode = Menu.useVideoAudio ? VideoAudioOutputMode.AudioSource : VideoAudioOutputMode.None;
            videoPlayer.Stop();
            if (videoPlayer.audioOutputMode == VideoAudioOutputMode.AudioSource) {
                videoPlayer.EnableAudioTrack(0, true);
            }
            videoPlayer.Play();
        }
    }

    private void OnBeat(float instantEnergy, float averageLocalEnergy, float cxa) {
        /*
        float scaledCxa = cxa * 1000f;
        if (_terrain && _terrain.Renderer && scaledCxa >= scaledBeatStrengthThreshold)
        {
            Material mat = _terrain.Renderer.material;
            Debug.Log($"BEAT: I: {instantEnergy} | AVG: {averageLocalEnergy} | CxA: {scaledCxa}");
            mat.SetFloat("_WireThickness", mat.GetFloat("_MaxThickness") * wireScalar);
        }
        */

        float scaledCxa = cxa * 1000f;
        if (Time.time >= _spawnTimer)
        {
            _spawnTimer = Time.time + minTimeBetweenSpawns;
            if (scaledCxa >= scaledBeatStrengthThreshold || scaledBeatStrengthThreshold == -1f)
                SpawnEnemy();
        }
    }

    private void SpawnEnemy() {
        if (!player) return;
        float midY = transform.position.y + _terrain.mirrorPositionOffset.y / 2f;
        Vector3 spawnLocation = new Vector3(player.position.x + Random.Range(spawnAreaRangeX.low, spawnAreaRangeX.high), midY + Random.Range(spawnAreaRangeY.low, spawnAreaRangeY.high), player.position.z);
        spawnLocation.z = player.position.z + player.GetComponentInChildren<Player>().Camera.farClipPlane;
        GameObject g = Instantiate(enemyPrefab, spawnLocation, Quaternion.identity);
        Teki t = g.GetComponent<Teki>();
        if (t)
            t.ManipulateVertices(maxEnemySize);
        Destroy(g, 20f);
    }

    public override void Update()
    {
        base.Update();
        if (!Source.isPlaying) return;
        if (AudioManager.PlayerDead)
        {
            if(videoPlayer && videoPlayer.isPlaying && videoPlayer.enabled && Menu.useVideoAudio)
                videoPlayer.playbackSpeed = Mathf.MoveTowards(videoPlayer.playbackSpeed, 0f, 0.15f * Time.deltaTime);
            else if(Clip && Source.isPlaying)
                Source.pitch = Mathf.Lerp(Source.pitch, 0f, Time.deltaTime);
        }

        if (!player) return;
        Vector3 destination = transform.position + respawnOffset + (transform.TransformDirection(respawnOffsetDirection) * (_terrain.width * _terrain.spacing.x));
        float dist = Vector3.Distance(new Vector3(player.position.x, destination.y, player.position.z), new Vector3(player.position.x, destination.y, destination.z));
        float dot = Vector3.Dot((destination - new Vector3(player.position.x, transform.position.y, player.position.z)).normalized, player.transform.forward);
        
        if (dist * (dot < 0 ? -1f : 1f) <= 0f && _canRespawn)
            OnRespawnThresholdReached();
        //_yIndex = _yIndex + 1 < length ? _yIndex + 1 : 0;
        if (_terrain && _terrain.Renderer)
        {
            Material mat = _terrain.Renderer.material;
            float a = 0;
            for (int i = 0; i < 3; i++)
            {
                FrequencyBand b = FrequencyBands[i];
                a += b.frequency;
            }
            float thickness = Mathf.Clamp(a / 3 * wireScalar, 0f, mat.GetFloat("_MaxThickness") - 200f);
            Hue = Utilities.MapRange(thickness, 0f, mat.GetFloat("_MaxThickness") - 200f, 0f, 1f);
            InverseHue = 1 - Hue;
            mat.SetFloat("_WireThickness", Mathf.Lerp(mat.GetFloat("_WireThickness"), thickness, Time.deltaTime * wireThicknessRate));
            mat.SetColor("_WireColor", Color.Lerp(mat.GetColor("_WireColor"), Color.HSVToRGB(Hue, 1f, 1f, true) * wireColorIntensity, Time.deltaTime * wireThicknessRate));

            float len = 0f;
            float time = 0f;
            if (videoPlayer && videoPlayer.isPlaying && videoPlayer.enabled && Menu.useVideoAudio)
            {
                len = (float)videoPlayer.length;
                time = (float)videoPlayer.time;

            }
            else if (Clip && Source.isPlaying)
            {
                len = Clip.length;
                time = Source.time;
                    
            }
            else
                return;

            float mappedSongLength = Utilities.MapRange(time, 0f, len, _terrain.maxMirrorHeight, heightScalar * 2f);
            _terrain.mirrorPositionOffset = new Vector3(_terrain.mirrorPositionOffset.x, mappedSongLength, _terrain.mirrorPositionOffset.z);
        }
    }

    private void OnRespawnThresholdReached()
    {
        //6 7 8 = 0 1 2

        
        if (_terrain)
            _terrain.transform.position = new Vector3(transform.position.x, transform.position.y, player.position.z);
        GenerateVisualizedHeight(false);
        _canRespawn = false;
        Invoke("ResetThresholdCheck", respawnThresholdCheckResetTime);
        
    }

    private void ResetThresholdCheck() => _canRespawn = true;

    private void GenerateVisualizedHeight(bool firstRun)
    {
        /*
        int bandFactor = Mathf.FloorToInt(_terrain.width / FrequencyBands.Length);
        int currentVertex = 0;
        for (int y = 0; y < _terrain.height; y++)
        {
            for (int i = 0; i < _terrain.width; i++)
            {
                _terrain.SetVertexHeight(currentVertex, Random.Range(0f, 30f));
                currentVertex++;
                //_heights[i, y] = FrequencyBands[Mathf.FloorToInt(i / bandFactor)].smoothedFrequency;
            }
        }
        */
        Chunk baseChunk = _terrain.chunks[0];
        int startIndex = firstRun ? 0 : 3;

        for (int u = 0; u < 3; u++)
            _terrain.ReplaceChunk(_terrain.chunks[u], _terrain.chunks[u + 6]);

        int bandFactor = Mathf.FloorToInt(_terrain.width / FrequencyBands.Length);
        Debug.Log(FrequencyBands[0].frequency);
        for (int c = startIndex; c < _terrain.chunks.Length; c++)
        {
            Chunk chunk = _terrain.chunks[c];
            for (int y = 0; y < baseChunk.height; y++)
            {
                for (int x = 0; x < baseChunk.width; x++)
                {
                    Vector2Int vertex = chunk.MapToChunkBounds(x, y);
                    Vector3 vert = _terrain.Vertices[_terrain.ToSingleIndex(vertex.x, vertex.y)];
                    vert.y = Mathf.PerlinNoise(x * perlinNoiseZoom.x, y * perlinNoiseZoom.y) * heightScalar;
                    _terrain.Vertices[_terrain.ToSingleIndex(vertex.x, vertex.y)] = vert;
                }
            }
        }
        _terrain.ApplyVertexChanges();
    }

    private void OnDrawGizmos()
    {
        if (!_terrain)
            _terrain = GetComponent<CustomTerrain>();
        float w = _terrain.width * _terrain.spacing.x;
        float h = _terrain.height * _terrain.spacing.z;
        Gizmos.color = Color.blue;
        Vector3 destination = transform.position + respawnOffset + (transform.TransformDirection(respawnOffsetDirection) * w);
        Gizmos.DrawLine(transform.position + respawnOffset, destination);
        Gizmos.color = Color.magenta;
        if (player)
            Gizmos.DrawLine(new Vector3(player.position.x, destination.y, player.position.z), new Vector3(player.position.x, destination.y, destination.z));

        /*
        float one = w / chunkCount;
        for (int x = 0; x < chunkCount + 1; x++) {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position + new Vector3(one * x, 0f, 0f), transform.position + new Vector3(one * x, 0f, h));

            int xIndex = ((_terrain.width / chunkCount) * x) % _terrain.width;

            Gizmos.color = Color.cyan;
            if (_terrain && _terrain.Vertices != null && _terrain.Vertices.Length > 0)
                Gizmos.DrawSphere(_terrain.transform.TransformPoint(_terrain.Vertices[xIndex]), 5f);
        }
        Gizmos.color = Color.green;
        */
        Gizmos.color = Color.blue;
        Vector3 mid = new Vector3(transform.position.x, transform.position.y + _terrain.mirrorPositionOffset.y / 2f, transform.position.z);
        Gizmos.DrawSphere(mid, 2f);
    }
}

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(VisualizedCustomTerrain))]
public class VisualizedCustomTerrainEditor : UnityEditor.Editor {
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }
}
#endif