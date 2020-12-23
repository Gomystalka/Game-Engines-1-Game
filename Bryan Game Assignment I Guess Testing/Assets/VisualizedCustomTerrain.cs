using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public override void OnEnable()
    {
        base.OnEnable();
        _terrain = GetComponent<CustomTerrain>();
        _canRespawn = true;
        GenerateVisualizedHeight(true);
    }

    public override void Update()
    {
        base.Update();
        if (!Source.isPlaying) return;

        Vector3 destination = transform.position + respawnOffset + (transform.TransformDirection(respawnOffsetDirection) * (_terrain.width * _terrain.spacing.x));
        float dist = Vector3.Distance(new Vector3(player.position.x, destination.y, player.position.z), new Vector3(player.position.x, destination.y, destination.z));
        float dot = Vector3.Dot((destination - new Vector3(player.position.x, transform.position.y, player.position.z)).normalized, player.transform.forward);
        
        if (dist * (dot < 0 ? -1f : 1f) <= 0f && _canRespawn)
            OnRespawnThresholdReached();
        //_yIndex = _yIndex + 1 < length ? _yIndex + 1 : 0;

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