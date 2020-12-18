using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipPlayer : MonoBehaviour
{
    public ScreenBoundsColliderOld screenBounds;
    public Vector3 yMidRight, xMidRight;
    public float controlSpeed = 6f;
    public float speed = 14f;

    [Header("Crosshair Settings")]
    public GameObject crosshair;
    public float crosshairDistance;

    private float _h, _v;

    void Update()
    {
        _h = Input.GetAxis("Horizontal");
        _v = Input.GetAxis("Vertical");



        transform.parent.position += transform.forward * speed * Time.deltaTime;
        CollisionLocation col = screenBounds.Constrain3DObject(transform);
    }

    private void LateUpdate()
    {
        if (_v == 0 && _h == 0)
            crosshair.transform.position = Vector3.Lerp(crosshair.transform.position, transform.position + (transform.forward * crosshairDistance), controlSpeed * Time.deltaTime);
        else
        {
            Vector3 horizontalDelta = crosshair.transform.right * _h;
            Vector3 verticalDelta = crosshair.transform.up * _v;
            crosshair.transform.position += horizontalDelta * controlSpeed * Time.deltaTime;
            crosshair.transform.position += verticalDelta * controlSpeed * Time.deltaTime;
            transform.LookAt(crosshair.transform);
        }

            //transform.parent.LookAt(crosshair.transform);
            //screenBounds.Constrain3DObject(crosshair.transform);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        if (!Application.isPlaying)
        {
            yMidRight = new Vector3(screenBounds.FrustumCorners[3].x, transform.position.y, screenBounds.FrustumCorners[3].z);
            xMidRight = screenBounds.FrustumCorners[3] - (screenBounds.FrustumCorners[3] - screenBounds.FrustumCorners[1]).normalized * Vector3.Distance(transform.position, yMidRight);
        }

        Gizmos.DrawLine(transform.position, yMidRight);
        Gizmos.DrawSphere(yMidRight, 0.2f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, xMidRight);
        Gizmos.DrawSphere(xMidRight, 0.2f);

#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 1.4f, $"(X [MAX: {screenBounds.HorizontalDistance}]: {Vector3.Distance(transform.position, yMidRight)} Y [MAX: {screenBounds.VerticalDistance}]: {Vector3.Distance(transform.position, xMidRight)}) DOT: {Vector3.Dot((yMidRight -transform.position).normalized, transform.right)}");
#endif
    }
}
