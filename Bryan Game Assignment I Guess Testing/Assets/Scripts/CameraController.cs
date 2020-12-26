using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;
    public Vector2 motionLimits;
    public float smoothing;

    private Vector3 _vel = Vector3.zero;

    private void LateUpdate()
    {
        if (!target) return;
        //transform.localPosition = Vector3.Lerp(transform.localPosition, target.localPosition + offset, smoothing * Time.deltaTime);
        //transform.localPosition = Vector3.SmoothDamp(transform.localPosition, target.localPosition + offset, ref _vel, smoothing * Time.deltaTime);
        //transform.localPosition = transform.localPosition.ClampXY(-motionLimits.x, motionLimits.x, -motionLimits.y, motionLimits.y);
    }
}