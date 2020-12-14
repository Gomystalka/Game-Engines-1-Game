using UnityEngine;

public class ThirdPersonCameraController : MonoBehaviour
{
    //Made by Tomasz Galka <3
    [Header("References")]
    public Transform target;

    [Header("Camera Settings")]
    public bool reverseY;
    public bool reverseX;
    public float cameraHeight = 0f;
    public float xSensitivity = 0.5f;
    public float ySensitivity = 0.5f;
    public float fieldOfView = 20f;
    public float verticalLimitMin = -5f, verticalLimitMax = 45f;
    public float zoomInSpeed = 1f;
    public float zoomOutSpeed = 1f;
    public float minZoom;
    public float maxZoom;
    public bool lockCursor = true;

    [Header("Smoothing")]
    [Range(0, 1000f)] public float followSmoothing = 20f;
    [Range(0, 1000f)] public float rotationSmoothing = 0.01f;

    [Header("Collision")]
    public bool enableCollider;
    public bool drawCollider = true;
    public LayerMask collideableLayers;
    [Range(0, 1000f)] public float cameraSpeed = 10f;

    private float currX;
    private float currY;
    private Vector3 _axes;
    private Vector3 _oldPos;

    private RaycastHit _colliderHit;
    private bool _collided;
    private float _targetFieldOfView;

    public bool Moving { get; set; } = false;
    public bool BlockMovement { get; set; } = false;

    void Start()
    {
        if (lockCursor)
            SetCursorState(false);
    }

    public static void SetCursorState(bool state)
    {
        Cursor.lockState = state ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = state;
    }

    private void Update()
    {
        if (Time.timeScale > 0 && !BlockMovement)
        {
            _axes = new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse ScrollWheel"));

            currX = Mathf.Lerp(currX, currX + (!reverseX ? _axes.x : -_axes.x) * xSensitivity, rotationSmoothing * Time.deltaTime);
            currY = Mathf.Lerp(currY, currY + (!reverseY ? _axes.y : -_axes.y) * ySensitivity, rotationSmoothing * Time.deltaTime);
            currY = Mathf.Clamp(currY, verticalLimitMin, verticalLimitMax);

            if (_axes.z < 0f && fieldOfView <= maxZoom)
                _targetFieldOfView = fieldOfView + zoomOutSpeed;
            else if (_axes.z > 0f && fieldOfView >= minZoom)
                _targetFieldOfView = fieldOfView - zoomOutSpeed;

            Moving = _axes.x != 0f || _axes.y != 0f;
            _oldPos = transform.position;
        }
    }

    private void LateUpdate()
    {
        _collided = enableCollider ? Physics.Linecast(
            target.position,
            transform.position,
            out _colliderHit,
            collideableLayers) : false;

        if (Time.timeScale > 0 && !BlockMovement)
        {
            if (target)
            {
                Vector3 pPos = new Vector3(target.position.x, target.position.y + cameraHeight, target.position.z);
                if (_collided)
                    _targetFieldOfView = Mathf.Abs(Vector3.Distance(target.position, transform.position)) / 2f;

                if (fieldOfView != _targetFieldOfView)
                    fieldOfView = Mathf.MoveTowards(fieldOfView, _targetFieldOfView, cameraSpeed * Time.deltaTime);

                fieldOfView = Mathf.Clamp(fieldOfView, minZoom, maxZoom);
                transform.position = Vector3.Lerp(transform.position, pPos + Quaternion.Euler(currY, currX, 0f) * (Vector3.forward * -fieldOfView), followSmoothing * Time.deltaTime);
                transform.LookAt(pPos);
            }
            else
                transform.position = _oldPos;

            _oldPos = transform.position;
        }
    }

    private void DrawCollider()
    {
        if (!enableCollider || !drawCollider) return;
        Gizmos.color = _collided ? Color.green : Color.red;
        Gizmos.DrawLine(target.position, transform.position);
    }

    private void OnDrawGizmos() => DrawCollider();
}