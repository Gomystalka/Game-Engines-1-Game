using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utilities
{
    public static CollisionLocation RestrictToScreenBoundary(Camera camera, Transform transform, bool collisionDataOnly = false)
    {
        if (!camera) return CollisionLocation.None;
        CollisionLocation col = CollisionLocation.None;

        Vector3 viewportPos = camera.WorldToViewportPoint(transform.position);

        if (viewportPos.x <= 0f)
        {
            viewportPos.x = 0f;
            col ^= CollisionLocation.Left;
        }

        if (viewportPos.x >= 1f) {
            viewportPos.x = 1f;
            col ^= CollisionLocation.Right;
        }

        if (viewportPos.y <= 0f) {
            viewportPos.y = 0f;
            col ^= CollisionLocation.Bottom;
        }

        if (viewportPos.y >= 1f)
        {
            viewportPos.y = 1f;
            col ^= CollisionLocation.Top;
        }

        if(!collisionDataOnly)
            transform.position = camera.ViewportToWorldPoint(viewportPos);
        return col;
    }

    public static void RestrictToScreenBoundsNoData(Camera camera, Transform transform) {
        Vector3 viewportPos = camera.WorldToViewportPoint(transform.position);
        viewportPos.x = Mathf.Clamp01(viewportPos.x);
        viewportPos.y = Mathf.Clamp01(viewportPos.y);
        transform.position = camera.ViewportToWorldPoint(viewportPos);
    }

    private static Vector2 _goodAxis;
    private static Vector2 _goodAxisRaw;
    public static float inputInterpolationSpeed = 1f;

    public static float GetGoodAxis(string axis)
    {
        float value = 0f;
        if (axis == "Horizontal")
        {
            value = _goodAxis.x;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                value = Mathf.MoveTowards(_goodAxis.x, -1f, inputInterpolationSpeed * Time.deltaTime);
            else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                value = Mathf.MoveTowards(_goodAxis.x, 1f, inputInterpolationSpeed * Time.deltaTime);
            else
                value = Mathf.MoveTowards(_goodAxis.x, 0f, inputInterpolationSpeed * Time.deltaTime);
            _goodAxis = new Vector2(value, _goodAxis.y);
            return value;
        }
        else if (axis == "Vertical")
        {
            value = _goodAxis.y;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                value = Mathf.MoveTowards(_goodAxis.y, 1f, inputInterpolationSpeed * Time.deltaTime);
            else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
                value = Mathf.MoveTowards(_goodAxis.y, -1f, inputInterpolationSpeed * Time.deltaTime);
            else
                value = Mathf.MoveTowards(_goodAxis.y, 0f, inputInterpolationSpeed * Time.deltaTime);

            _goodAxis = new Vector2(_goodAxis.x, value);

            return value;
        }
        return value;
    }

    public static float GetGoodAxisRaw(string axis)
    {
        float value = 0f;
        if (axis == "Horizontal")
        {
            value = _goodAxisRaw.x;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                value = -1f;
            else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                value = 1f;
            else
                value = 0f;
            _goodAxisRaw = new Vector2(value, _goodAxisRaw.y);
            return value;
        }
        else if (axis == "Vertical")
        {
            value = _goodAxisRaw.y;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                value = 1f;
            else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
                value = -1f;
            else
                value = 0f;

            _goodAxisRaw = new Vector2(_goodAxisRaw.x, value);

            return value;
        }
        return value;
    }

    public static Vector3 CalculateDepthIndependentSize(Camera camera, Transform transform, float size) {
        Vector3 screenPoint = camera.WorldToScreenPoint(transform.position);
        Vector3 offset = screenPoint + Vector3.up * size;
        return Vector3.one * (camera.ScreenToWorldPoint(screenPoint) - camera.ScreenToWorldPoint(offset)).magnitude;
    }

    public static float MapRange(float v, float s, float st, float s2, float st2)
    {
        return s2 + (st2 - s2) * ((v - s) / (st - s));
    }

}
