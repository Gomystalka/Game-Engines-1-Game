using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Terrain))]
[System.Obsolete("Deprecated due to Unity's terrible Terrain system not supporting non-square Height Maps")]
public class VisualizedTerrain : AudioBehaviour
{
    public int width;
    public int length;
    public int height;

    private Terrain _terrain;

    private float[,] _heights;
    private int _yIndex;

    [Header("Terrain Respawning")]
    public Vector3 respawnOffset;
    public Vector3 respawnOffsetDirection;
    public Transform player;
    public float respawnThresholdCheckResetTime = 2f;

    [Header("Gizmos")]
    public float gizmoHeight = 2f;

    private float _dist;
    private bool _canRespawn = true;

    public override void OnEnable()
    {
        base.OnEnable();
        _terrain = GetComponent<Terrain>();
        _canRespawn = true;
        //256 x 256
        //256 % 256

        _heights = new float[width, length];
        CreateTerrainHeights();
    }

    private void CreateTerrainHeights() {
        TerrainData data = _terrain.terrainData;
        data.heightmapResolution = width + 1;
        data.size = new Vector3(width, height, length);
        //data.SetHeights(0, 0, _heights);
    }

    private void GenerateVisualizedHeight()
    {
        int bandFactor = Mathf.FloorToInt(FFTSpectrumData.Length / FrequencyBands.Length);
        for (int y = 0; y < length; y++)
        {
            for (int i = 0; i < width; i++)
            {
                _heights[i, y] = FrequencyBands[Mathf.FloorToInt(i / bandFactor)].smoothedFrequency;
            }
        }
        if (_terrain)
            _terrain.terrainData.SetHeights(0, 0, _heights);
    }

    public override void Update()
    {
        base.Update();
        if (!Source.isPlaying) return;

        if (_terrain)
        {
            //GenerateVisualizedHeight();
            //_terrain.terrainData.SetHeights(0, 0, _heights);
        }

        Vector3 destination = transform.position + respawnOffset + (transform.TransformDirection(respawnOffsetDirection) * width);
        _dist = Vector3.Distance(new Vector3(player.position.x, destination.y, player.position.z), new Vector3(player.position.x, destination.y, destination.z));
        if (_dist <= 1f && _canRespawn)
            OnRespawnThresholdReached();
        _yIndex = _yIndex + 1 < length ? _yIndex + 1 : 0;
    }

    private void OnRespawnThresholdReached() {
        _canRespawn = false;
        if(player)
            transform.position = new Vector3(transform.position.x, transform.position.y, player.position.z);
        GenerateVisualizedHeight();
        Invoke("ResetThresholdCheck", respawnThresholdCheckResetTime);
    }

    private void ResetThresholdCheck() => _canRespawn = true;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Vector3 offsetPos = transform.position + new Vector3(width / 2f, gizmoHeight, (length / 2f));
        Gizmos.DrawWireCube(offsetPos, new Vector3(width, 1f, length));
        Gizmos.color = Color.blue;

        Vector3 destination = transform.position + respawnOffset + (transform.TransformDirection(respawnOffsetDirection) * width);
        Gizmos.DrawLine(transform.position + respawnOffset, destination);
        Gizmos.color = Color.magenta;
        if (player)
            Gizmos.DrawLine(new Vector3(player.position.x, destination.y, player.position.z), new Vector3(player.position.x, destination.y, destination.z));
    }
}