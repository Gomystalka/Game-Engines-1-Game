using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    public Renderer Renderer { get; private set; }
    public bool IsVisible { get { return Renderer ? Renderer.isVisible : false; } }
    public Bounds Bounds { get; private set; }
    public Collider Collider { get; private set; }

    private void OnEnable()
    {
        Renderer = GetComponent<Renderer>();
        if (!Renderer)
            Renderer = GetComponentInChildren<Renderer>();
        if (!Renderer)
            Destroy(gameObject);

        Collider = GetComponent<Collider>();
        if (Collider)
            Bounds = Collider.bounds;
    }
}
