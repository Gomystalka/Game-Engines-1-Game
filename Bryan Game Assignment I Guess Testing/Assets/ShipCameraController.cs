using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ShipCameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    public Transform ship;
    public Vector3 cameraOffset;

    private void LateUpdate()
    {
        if (!ship) return;
        //Vector3 offsetPosition = ship.TransformPoint(cameraOffset);
        transform.position = ship.TransformPoint(cameraOffset);
        //transform.position = new Vector3(offsetPosition.x, transform.position.y, offsetPosition.z);
        transform.LookAt(new Vector3(ship.position.x, transform.position.y, ship.position.z));
    }

    private Vector3 RotatePointAroundPosition(Vector3 point, Vector3 position, Quaternion rotation) {
        Vector3 direction = point - position;
        direction = rotation * direction;
        return direction + position;
    }
}