using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
[DefaultExecutionOrder(-10)]
public class CustomTerrain : MonoBehaviour
{
    [Header("Terrain Generation Settings")]
    public int width;
    public int height;
    public Vector3 spacing;

    [Header("Terrain Respawning")]
    public Vector3 respawnOffset;
    public Vector3 respawnOffsetDirection;
    public Transform player;
    public float respawnThresholdCheckResetTime = 2f;

    [Header("Gizmos")]
    public float gizmoHeightOffset;

    private MeshFilter _filter;
    private MeshRenderer _renderer;
    private Vector3[] _vertices;
    private int[] _tris;
    private float _dist;
    private bool _canRespawn = true;

    private void OnEnable()
    {
        _filter = GetComponent<MeshFilter>();
        _renderer = GetComponent<MeshRenderer>();
        _filter.mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        _filter.mesh = GeneratePlaneMesh();
    }

    private Mesh GeneratePlaneMesh() {
        Mesh m = new Mesh();
        _vertices = new Vector3[(width + 1) * (height + 1)];
        _tris = new int[width * height * 6];
        int vertIndex = 0;
        for (int y = 0; y <= height; y++)
        {
            for (int x = 0; x <= width; x++)
            {
                _vertices[vertIndex] = new Vector3(x * spacing.x, 0f, y * spacing.z);
                vertIndex++;
            }
        }

        int triIndex = 0;
        int index = 0;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                _tris[triIndex] = index;
                _tris[triIndex + 1] = index + width + 1;
                _tris[triIndex + 2] = index + 1;
                _tris[triIndex + 3] = index + 1;
                _tris[triIndex + 4] = index + width + 1;
                _tris[triIndex + 5] = index + width + 2;
                index++;
                triIndex += 6;
            }
            index++;
        }
        m.Clear();
        m.vertices = _vertices;
        m.triangles = _tris;
        m.RecalculateTangents();
        m.RecalculateNormals();
        m.RecalculateBounds();
        return m;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        float w = width * spacing.x;
        float h = height * spacing.z;
        Vector3 offsetPos = transform.position + new Vector3(w / 2f, 0f, (h / 2f));
        Gizmos.DrawWireCube(offsetPos, new Vector3(w, 1f, h));
        Gizmos.color = Color.blue;
        Vector3 destination = transform.position + respawnOffset + (transform.TransformDirection(respawnOffsetDirection) * w);
        Gizmos.DrawLine(transform.position + respawnOffset, destination);
        Gizmos.color = Color.magenta;
        if (player)
            Gizmos.DrawLine(new Vector3(player.position.x, destination.y, player.position.z), new Vector3(player.position.x, destination.y, destination.z));
    }
}
