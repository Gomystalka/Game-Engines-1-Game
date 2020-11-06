using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserCaster : MonoBehaviour
{
    public int maxReflections = 10;
    public LayerMask surfaceMask;
    public float laserMaxLength = 100f;
    public Vector3 sourceOffset;
    public List<Line> lines;

    public Vector3[] _vs;

    [SerializeField] private LineRenderer _lineRenderer;

    private void OnEnable()
    {
        //_lineRenderer = GetComponent<LineRenderer>();
        lines = new List<Line>();
    }

    void Update()
    {
        CastLaser(transform.TransformPoint(sourceOffset), transform.forward, ref lines);

        _vs = lines.GetVectors();
        _lineRenderer.positionCount = _vs.Length;
        _lineRenderer.SetPositions(_vs);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.TransformPoint(sourceOffset), 0.05f);
    }

    private void CastLaser(Vector3 position, Vector3 direction, ref List<Line> lines, int reflectionIndex = 0)
    {
        if (reflectionIndex == 0)
            lines.Clear();
        if (reflectionIndex >= maxReflections) return;

        if (Physics.Raycast(position, direction, out RaycastHit hit, laserMaxLength, surfaceMask))
        {
            if (hit.transform == this) return;
            Debug.DrawRay(position, direction * hit.distance, Color.red);
            lines.Add(new Line()
            {
                start = position,
                end = hit.point
            });
            CastLaser(hit.point, Vector3.Reflect(direction, hit.normal), ref lines, reflectionIndex + 1);
        }
        else
        {
            lines.Add(new Line()
            {
                start = position,
                end = position + direction * laserMaxLength
            });
            Debug.DrawRay(position, direction * laserMaxLength, Color.blue);
        }
    }
}

[System.Serializable]
public struct Line {
    public Vector3 start;
    public Vector3 end;

    public Vector3 GetVector(int index) {
        if (index == 0)
            return start;
        else
            return end;
    }
}