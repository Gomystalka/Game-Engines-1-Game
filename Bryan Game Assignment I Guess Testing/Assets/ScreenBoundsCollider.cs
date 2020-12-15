using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenBoundsCollider : MonoBehaviour
{
    private Camera _camera;

    public Range verticalLimitOffset;
    public Range horizontalLimitOffset;

    private Vector3 _lastGoodPosition;
    private Rect _cameraViewRect;

    private void OnEnable()
    {
        _camera = Camera.main;
        Vector3 bl = _camera.ScreenToWorldPoint(Vector3.zero);
        Vector3 tr = _camera.ScreenToWorldPoint(new Vector3(_camera.pixelWidth, _camera.pixelHeight));
        _cameraViewRect = new Rect(bl.x, bl.y, tr.x - bl.x, tr.y - bl.y);
    }

    void Update()
    {
        if (!_camera) return;

        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, _cameraViewRect.xMin, _cameraViewRect.max.x * 2);
        transform.position = pos;
    }

    public enum CollisionLocation {
        Top,
        Bottom,
        Left,
        Right,
        None
    }
}
