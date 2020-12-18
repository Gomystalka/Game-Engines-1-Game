using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-20)]
[RequireComponent(typeof(Camera))]
public class ScreenBoundsColliderOld : MonoBehaviour
{
    [Header("Offsets")]
    public Range verticalLimitOffset;
    public Range horizontalLimitOffset;

    public float distance = 0;
    private Camera _camera;

    public Vector3[] FrustumCorners { get; private set; }
    public Vector3 CenterPoint { get; private set; }

    public float HorizontalDistance {
        get {
            return Vector3.Distance(FrustumCorners[0], FrustumCorners[2]);
        }
    }

    public float VerticalDistance {
        get {
            return Vector3.Distance(FrustumCorners[0], FrustumCorners[1]);
        }
    }

    #region Ortographic Only
    public float MinX {
        get {
            return FrustumCorners[0].x;
        }
    }

    public float MaxX
    {
        get
        {
            return FrustumCorners[3].x;
        }
    }

    public float MinY
    {
        get
        {
            return FrustumCorners[0].y;
        }
    }

    public float MaxY
    {
        get
        {
            return FrustumCorners[1].y;
        }
    }

    public float OrthoClampToWidth(float x)
    {
        return Mathf.Clamp(x, MinX, MaxX);
    }

    public float OrthoClampToHeight(float y)
    {
        return Mathf.Clamp(y, MinY, MaxY);
    }
    #endregion

    private void OnEnable()
    {
        _camera = GetComponent<Camera>();
        FrustumCorners = new Vector3[4];
    }

    private void Update()
    {
        FrustumCorners = _camera.CalculateFrustumCorners(distance);
        CenterPoint = new Vector3((MinX + MaxX) / 2f, (MinY + MaxY) / 2f, (FrustumCorners[0].z + FrustumCorners[3].z) / 2f);
    }

    public Vector3 yMidPoint;
    public Vector3 xMidPoint;

    public CollisionLocation Constrain3DObject(Transform objectTransform, bool collisionInfoOnly = false) {
        yMidPoint = new Vector3(FrustumCorners[3].x, objectTransform.position.y, FrustumCorners[3].z);
        xMidPoint = FrustumCorners[3] - (FrustumCorners[3] - FrustumCorners[1]).normalized * Vector3.Distance(objectTransform.position, yMidPoint);

        float hDist = Vector3.Distance(yMidPoint, objectTransform.position);
        float vDist = Vector3.Distance(xMidPoint, objectTransform.position);
        float hDirDot = Vector3.Dot((yMidPoint - objectTransform.position).normalized, objectTransform.right);
        float vDirDot = Vector3.Dot((xMidPoint - objectTransform.position).normalized, objectTransform.up);
        hDist *= hDirDot < 0f ? -1f : 1f;
        vDist *= vDirDot < 0f ? -1f : 1f;
        CollisionLocation location = CollisionLocation.None;

        if (hDist <= 0f)
        {
            if(!collisionInfoOnly)
                objectTransform.position = new Vector3(yMidPoint.x, objectTransform.position.y, yMidPoint.z);
            location ^= CollisionLocation.Right;
        }

        if (hDist >= HorizontalDistance)
        {
            if(!collisionInfoOnly)
                objectTransform.position = new Vector3(FrustumCorners[0].x, objectTransform.position.y, FrustumCorners[0].z);
            location ^= CollisionLocation.Left;
        }

        if (vDist <= 0f)
        {
            if (!collisionInfoOnly)
                objectTransform.position = new Vector3(objectTransform.position.x, xMidPoint.y, objectTransform.position.z);
            location ^= CollisionLocation.Top;
        }

        if (vDist >= VerticalDistance)
        {
            if (!collisionInfoOnly)
                objectTransform.position = new Vector3(objectTransform.position.x, FrustumCorners[0].y, objectTransform.position.z);
            location ^= CollisionLocation.Bottom;
        }

        return location;
    }

    private void OnDrawGizmos()
    {
        if (!_camera)
            _camera = GetComponent<Camera>();
        if (!Application.isPlaying) { //Do not calculate Frustum Corners in play mode
            FrustumCorners = _camera.CalculateFrustumCorners(distance);
            CenterPoint = new Vector3((MinX + MaxX) / 2f, (MinY + MaxY) / 2f, (FrustumCorners[0].z + FrustumCorners[3].z) / 2f);
        }

        Gizmos.color = Color.yellow;

        for (int i = 0; i < FrustumCorners.Length; i++) {
            UnityEditor.Handles.Label(FrustumCorners[i] + Vector3.up * 2f, $"I: {i}");
            Gizmos.DrawSphere(FrustumCorners[i], 0.2f);
        }
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(FrustumCorners[0], FrustumCorners[3]);
        Gizmos.DrawLine(FrustumCorners[1], FrustumCorners[2]);
        Gizmos.DrawSphere(CenterPoint, 0.1f);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(FrustumCorners[0], FrustumCorners[1]);
        Gizmos.DrawLine(FrustumCorners[1], FrustumCorners[3]);
        Gizmos.DrawLine(FrustumCorners[3], FrustumCorners[2]);
        Gizmos.DrawLine(FrustumCorners[2], FrustumCorners[0]);
    }
}