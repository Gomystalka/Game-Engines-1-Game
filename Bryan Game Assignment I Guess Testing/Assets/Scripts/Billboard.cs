using UnityEngine;

public class Billboard : MonoBehaviour
{
    public Camera cameraReference;
    public RotationMode rotationMode;
    public bool flip;

    private void Update()
    {
        if (!cameraReference) return;

        Vector3 position = cameraReference.transform.position;
        switch (rotationMode)
        {
            case RotationMode.XAxis:
                position.x = transform.position.x;
                break;
            case RotationMode.YAxis:
                position.y = transform.position.y;
                break;
            case RotationMode.ZAxis:
                position.z = transform.position.z;
                break;
        }
        transform.LookAt(flip ? 2f * transform.position - position : position);
    }

    public enum RotationMode
    {
        AllAxis,
        XAxis,
        YAxis,
        ZAxis
    }
}
