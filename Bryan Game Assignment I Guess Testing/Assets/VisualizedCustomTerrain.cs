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

    [Header("Chunk Settings")]
    public int chunkCount = 3;

    private CustomTerrain _terrain;
    private bool _canRespawn = true;

    public override void OnEnable()
    {
        base.OnEnable();
        _terrain = GetComponent<CustomTerrain>();
        _canRespawn = true;
    }

    public override void Update()
    {
        base.Update();
        if (!Source.isPlaying) return;

        Vector3 destination = transform.position + respawnOffset + (transform.TransformDirection(respawnOffsetDirection) * (_terrain.width * _terrain.spacing.x));
        float dist = Vector3.Distance(new Vector3(player.position.x, destination.y, player.position.z), new Vector3(player.position.x, destination.y, destination.z));
        //if (dist <= 1f && _canRespawn)
            //OnRespawnThresholdReached();
        //_yIndex = _yIndex + 1 < length ? _yIndex + 1 : 0;

    }

    private void OnRespawnThresholdReached()
    {
        _canRespawn = false;
        //Invoke("ResetThresholdCheck", respawnThresholdCheckResetTime);
    }

    private void ResetThresholdCheck() => _canRespawn = true;

    private void GenerateVisualizedHeight()
    {
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

        Gizmos.color = Color.cyan;
        float one = w / chunkCount;
        for (int x = 1; x < chunkCount; x++) {
            Gizmos.DrawLine(transform.position + new Vector3(one * x, 0f, 0f), transform.position + new Vector3(one * x, 0f, h));
        }
        one = h / chunkCount;
        for (int y = 1; y < chunkCount; y++) {
            Gizmos.DrawLine(transform.position + new Vector3(0f, 0f, one * y), transform.position + new Vector3(w, 0f, one * y));

        }
    }
}
