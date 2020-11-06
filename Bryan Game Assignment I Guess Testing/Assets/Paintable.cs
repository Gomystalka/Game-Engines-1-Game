using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Paintable : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Paintable Surface Settings")]
    public float maximumPaintDistance;
    public float brushSize;
    public Color brushColor;
    public bool useCustomLayerMask;
    public LayerMask customLayerMask;
    public bool hitTriggers;

    private Renderer _renderer;
    private Texture _defaultTexture;
    private Texture2D _canvasTexture;
    private MeshFilter _filter;

    [SerializeField] private Camera _camera;

    private void OnEnable()
    {
        if(!_camera)
            _camera = Camera.main;
        _renderer = GetComponent<Renderer>();
        _defaultTexture = _renderer.material.mainTexture;
        _filter = GetComponent<MeshFilter>();
        if (_defaultTexture)
            _canvasTexture = CreateCanvasTexture(_defaultTexture.width, _defaultTexture.height);
        else
            _canvasTexture = CreateCanvasTexture(800, 800);
        
        //_renderer.material.SetTe
    }

    private Texture2D CreateCanvasTexture(int width, int height) {
        return new Texture2D(width, height);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        LayerMask mask = useCustomLayerMask ? customLayerMask : (LayerMask)gameObject.layer;
        Ray r = _camera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(r, out RaycastHit hit, maximumPaintDistance, mask, hitTriggers ? QueryTriggerInteraction.Collide : QueryTriggerInteraction.Ignore)) {

        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
    }

    public void OnPointerUp(PointerEventData eventData)
    {
    }

    private void OnDrawGizmos()
    {

    }
}
