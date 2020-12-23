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

    [Header("Gizmos")]
    public float chunkCornerSize = 10f;

    private MeshFilter _filter;
    private MeshRenderer _renderer;
    public Vector3[] Vertices { get; private set; }
    private int[] _tris;

    [Header("Chunks")]
    [Range(1, 32)] public int chunkSize;
    public bool drawChunks;
    public Chunk[] chunks;

    public Vector2Int v;

    private void OnEnable()
    {
        _filter = GetComponent<MeshFilter>();
        _renderer = GetComponent<MeshRenderer>();
        _filter.mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        _filter.mesh = GeneratePlaneMesh();
        RecalculateChunks();
    }

    private Mesh GeneratePlaneMesh() {
        Mesh m = new Mesh();
        Vertices = new Vector3[(width + 1) * (height + 1)];
        _tris = new int[width * height * 6];
        int vertIndex = 0;
        for (int y = 0; y <= height; y++)
        {
            for (int x = 0; x <= width; x++)
            {
                Vertices[vertIndex] = new Vector3(x * spacing.x, 0f, y * spacing.z);
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
        m.vertices = Vertices;
        m.triangles = _tris;
        m.RecalculateTangents();
        m.RecalculateNormals();
        m.RecalculateBounds();
        return m;
    }

    public void SetVertexHeight(int index, float newHeight) {
        Vector3 v = Vertices[index];
        Vertices[index] = new Vector3(v.x, newHeight, v.z);
    }

    public void ApplyVertexChanges() {
        Mesh m = _filter.sharedMesh;
        m.vertices = Vertices;
        //m.triangles = _tris;
        m.RecalculateTangents();
        m.RecalculateNormals();
        m.RecalculateBounds();
    }

    public void RecalculateChunks() {
        float oneW = width / chunkSize;
        float oneH = height / chunkSize;
        chunks = new Chunk[chunkSize * chunkSize];
        int c = 0;
        for (int y = 0; y < chunkSize; y++)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                int index = (int)((x * oneW) + (width + 1) * (y * oneH));
                Chunk chunk = new Chunk()
                {
                    bottomLeftIndex = index,
                    bottomRightIndex = (int)((x * oneW) + (width + 1) * ((y + 1) * oneH)),
                    topLeftIndex = (int)(((x + 1) * oneW) + (width + 1) * (y * oneH)),
                    topRightIndex = (int)(((x + 1) * oneW) + (width + 1) * ((y + 1) * oneH))
                };
                chunk.bottomLeftPoint = new Vector2(x * oneW, y * oneH).ToIntegerVector();
                chunk.topRightPoint = new Vector2((x + 1) * oneW, (y + 1) * oneH).ToIntegerVector();
                chunk.topLeftPoint = new Vector2(chunk.topRightPoint.x - oneW, chunk.topRightPoint.y).ToIntegerVector();
                chunk.bottomRightPoint = new Vector2(chunk.bottomLeftPoint.x + oneW, chunk.bottomLeftPoint.y).ToIntegerVector();
                chunk.width = (int)oneW;
                chunk.height = (int)oneH;

                chunks[c] = chunk;
                c++;
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        float w = width * spacing.x;
        float h = height * spacing.z;
        Vector3 offsetPos = transform.position + new Vector3(w / 2f, 0f, (h / 2f));
        Gizmos.DrawWireCube(offsetPos, new Vector3(w, 1f, h));

        if (!drawChunks) return;
        Gizmos.color = Color.cyan;
        for (int y = 0; y < chunkSize + 1; y++)
        {
            for (int x = 0; x < chunkSize + 1; x++)
            {
                float oneWSpaced = w / chunkSize;
                float oneHSpaced = h / chunkSize;
                float oneW = width / chunkSize;
                float oneH = height / chunkSize;

                if (y == 0)
                    Gizmos.DrawLine(transform.position + new Vector3(0f, 0f, x * oneHSpaced), transform.position + new Vector3(w, 0f, x * oneHSpaced));
                if (x == 0)
                    Gizmos.DrawLine(transform.position + new Vector3(y * oneWSpaced, 0f, 0f), transform.position + new Vector3(y * oneWSpaced, 0f, h));

                int index = (int)((x * oneW) * (width + 1) + (y * oneH));
                //if (Vertices != null && Vertices.Length > 0)
                    //Gizmos.DrawSphere(transform.TransformPoint(Vertices[index]), 5f);
            }
        }

        Gizmos.color = Color.red;
        if (chunks != null && Vertices != null && Vertices.Length > 0)
        {
            
            for (int ch = 0; ch < chunks.Length; ch++)
            {
                //Gizmos.DrawSphere(LocalVertexToWorldSpace(c.bottomLeft), chunkCornerSize);
                //Gizmos.DrawSphere(LocalVertexToWorldSpace(c.bottomRight), chunkCornerSize);
                //Gizmos.DrawSphere(LocalVertexToWorldSpace(c.topLeft), chunkCornerSize);
                //Gizmos.DrawSphere(LocalVertexToWorldSpace(c.topRight), chunkCornerSize);
#if UNITY_EDITOR
                UnityEditor.Handles.Label(LocalVertexToWorldSpace(chunks[ch].Center.x, chunks[ch].Center.y), $"CHUNK INDEX: {ch}");
#endif
                //Gizmos.color = Color.red;
                //Gizmos.DrawSphere(LocalVertexToWorldSpace((int)chunks[ch].bottomLeftPoint.x, (int)chunks[ch].bottomLeftPoint.y), chunkCornerSize);
                Gizmos.color = Color.magenta;
            }
            //Gizmos.DrawSphere(LocalVertexToWorldSpace(chunks[1].Center.x, chunks[1].Center.y), chunkCornerSize);
            //Gizmos.DrawSphere(LocalVertexToWorldSpace(PopulateChunk(0, v.x, v.y, 100f)), chunkCornerSize);
        }
    }

    public Vector2Int PopulateChunk(int chunkIndex, int x, int y, float height) {
        if (chunkIndex < chunks.Length) {
            Chunk c = chunks[chunkIndex];
            Vector2Int mappedPoint = c.MapToChunkBounds(x, y);

            return mappedPoint;
        }
        return Vector2Int.zero;
    }

    public int ToSingleIndex(int x, int y)
    {
        return y * (height + 1) + x;
    }

    public Vector3 LocalVertexToWorldSpace(int index) {
        return transform.TransformPoint(Vertices[index]);
    }

    public Vector3 LocalVertexToWorldSpace(int x, int y) {
        return LocalVertexToWorldSpace(y * (height + 1) + x);
    }

    public Vector3 LocalVertexToWorldSpace(Vector2Int point)
    {
        return LocalVertexToWorldSpace(point.x, point.y);
    }
}

[System.Serializable]
public class Chunk {
    public int bottomLeftIndex;
    public int bottomRightIndex;
    public int topLeftIndex;
    public int topRightIndex;
    public Vector2Int bottomLeftPoint;
    public Vector2Int bottomRightPoint;
    public Vector2Int topLeftPoint;
    public Vector2Int topRightPoint;
    public int width;
    public int height;

    public int MinX
    {
        get
        {
            return bottomLeftPoint.x;
        }
    }

    public int MaxX {
        get {
            return bottomRightPoint.x;
        }
    }

    public int MinY
    {
        get
        {
            return bottomLeftPoint.y;
        }
    }

    public int MaxY
    {
        get
        {
            return topLeftPoint.y;
        }
    }

    public Vector2Int Center {
        get {
            return new Vector2Int(MaxX - (width / 2), MaxY - (height / 2));
        }
    }

    public Vector2Int ClampToChunkRange(int x, int y) {
        x = Mathf.Clamp(x, MinX, MaxX);
        y = Mathf.Clamp(y, MinY, MaxY);
        return new Vector2Int(x, y);
    }

    public Vector2Int ClampToChunkRange(Vector2Int point) {
        point.x = Mathf.Clamp(point.x, MinX, MaxX);
        point.y = Mathf.Clamp(point.y, MinY, MaxY);
        return point;
    }

    public Vector2Int MapToChunkBounds(int x, int y) {
        return ClampToChunkRange(MinX + x, MinY + y);
    }
}
