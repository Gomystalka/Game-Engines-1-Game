using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 20f;
    public float initialForwardSpeed;
    public float intialAcceleration;
    public float targetForwardSpeed;
    public float shipForwardSpeed;
    public float axisDeadZone = 0.9f;
    public Transform target;
    public float crosshairDistance;
    public float crosshairSpeed;
    public float turnSpeed;
    public float rollAngleLimit;
    public float shipRange;
    public ScreenBoundsColliderOld bounds;

    private Transform _playerMesh;
    private float _currentForwardSpeed;
    private Vector2 _axes;
    private Camera _camera;
    private CannonController _cannonController;

    public bool IsCrosshairMoving { get { return _axes.magnitude >= axisDeadZone; } }

    private void OnEnable()
    {
        _camera = transform.parent.GetComponentInChildren<Camera>();
        if (!_camera)
            _camera = Camera.main;
        Utilities.inputInterpolationSpeed = crosshairSpeed;
        bounds.Constrain3DObject(transform);
        transform.localPosition = Vector3.zero;
        _playerMesh = GetComponentInChildren<MeshRenderer>().transform;
        _currentForwardSpeed = initialForwardSpeed;
        _targetCrosshairSprite = targetCrosshair.GetComponent<SpriteRenderer>();
        _targetCrosshairBillboard = targetCrosshair.GetComponent<Billboard>();
        _targetCrosshairParent = targetCrosshair.parent;
        _cannonController = GetComponent<CannonController>();
    }

    void Update()
    {
        _axes = new Vector2(Utilities.GetGoodAxis("Horizontal"), Utilities.GetGoodAxis("Vertical"));

        transform.localPosition += (Vector3)_axes * speed * Time.deltaTime;
        transform.parent.position += transform.forward * _currentForwardSpeed * Time.deltaTime;

        ApplyForwardAcceleration();
        ApplyYawAndPitch();
        ApplyRoll();
        bounds.Constrain3DObject(transform);
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, 0f);

        FireTargettingRay();
        UpdateCrosshair();

        if (_cannonController && Input.GetButton("Fire1"))
            _cannonController.FireProjectile();
    }

    private void ApplyRoll() {
        Vector3 eulerAngles = _playerMesh.eulerAngles;
        eulerAngles.z = Mathf.LerpAngle(eulerAngles.z, -_axes.x * rollAngleLimit, 0.1f);
        _playerMesh.eulerAngles = eulerAngles;
    }

    private void ApplyForwardAcceleration() {
        if (_currentForwardSpeed >= targetForwardSpeed)
            _currentForwardSpeed = targetForwardSpeed;
        else
            _currentForwardSpeed += intialAcceleration * Time.deltaTime;
    }

    private void ApplyYawAndPitch()
    {
        target.parent.position = Vector3.zero;
        Vector3 crosshairPos = target.localPosition;
        crosshairPos = new Vector3(_axes.x * shipRange, _axes.y * shipRange, crosshairDistance);
        target.localPosition = crosshairPos;

        transform.rotation = Quaternion.RotateTowards(transform.rotation,
    Quaternion.LookRotation(target.position),
    Mathf.Deg2Rad * turnSpeed * Time.deltaTime);
    }

    [Header("Targetting Settings")]
    public Transform targetCrosshair;
    public LayerMask targetMask;
    public float targettingRange = 100f;
    public float lockOnSpeed = 10f;
    
    private Target _currentTarget;
    private bool _lockedOn;
    private SpriteRenderer _targetCrosshairSprite;
    private Billboard _targetCrosshairBillboard;
    private Transform _targetCrosshairParent;
    public float lockedOnSizePx = 150f;

    private void FireTargettingRay() {
        if (_currentTarget && !_camera.CanSee(_currentTarget.Bounds))
        {
            OnTargetLost();
            _currentTarget = null;
        }

        if (_currentTarget) return;

        if (Physics.Raycast(_targetCrosshairParent.position, _targetCrosshairParent.forward, out RaycastHit hit, targettingRange, targetMask))
        { //Target found
            _currentTarget = hit.transform.GetComponent<Target>();
            targetCrosshair.SetParent(null, true);
            _targetCrosshairBillboard.enabled = true;
        }
    }

    private void UpdateCrosshair() {
        if (_currentTarget)
        {
            targetCrosshair.position = Vector3.MoveTowards(targetCrosshair.position, _currentTarget.transform.position, lockOnSpeed * Time.deltaTime);
            if (targetCrosshair.position == _currentTarget.transform.position && !_lockedOn)
            {
                _lockedOn = true;
                OnTargetLockedOn();
            }
            else
                targetCrosshair.localScale = Utilities.CalculateDepthIndependentSize(_camera, targetCrosshair, lockedOnSizePx);
        }
        else
        {
            if (_lockedOn)
            {
                _lockedOn = false;
                OnTargetLost();
            }
            targetCrosshair.localPosition = Vector3.MoveTowards(targetCrosshair.localPosition, Vector3.zero + (Vector3.forward * 3f), lockOnSpeed * Time.deltaTime);
        }
    }

    private void OnTargetLockedOn() {
        if (_targetCrosshairSprite)
            _targetCrosshairSprite.color = Color.red;
    }

    private void OnTargetLost() {
        if (_targetCrosshairSprite)
            _targetCrosshairSprite.color = Color.white;
        _targetCrosshairBillboard.enabled = false;
        targetCrosshair.SetParent(_targetCrosshairParent, true);
        targetCrosshair.localScale = Vector3.one;
        targetCrosshair.localEulerAngles = Vector3.zero;
    }

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 1.4f, $"MAG: {(Vector3)_axes * _currentForwardSpeed * Time.deltaTime}");
#endif
    }
}

[System.Flags]
public enum CollisionLocation
{
    None = 0,
    Top = 1 << 0,
    Bottom = 1 << 1,
    Left = 1 << 2,
    Right = 1 << 3
}