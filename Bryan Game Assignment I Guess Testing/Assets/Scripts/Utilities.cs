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

public static class Extensions
{
    public static Vector2Int ToIntegerVector(this Vector2 v) {
        return new Vector2Int((int)v.x, (int)v.y);
    }

    public static float SumOf(this float[] arr)
    {
        float c = 0;
        foreach (float t in arr)
            c += t;
        return c;
    }

    public static float CalculateAverage(this float[] arr)
    {
        return arr.SumOf() / arr.Length;
    }

    public static bool IsWhole(this float f)
    {
        return f % 1 == 0;
    }

    public static string ElementsAsString<T>(this T[] arr)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        foreach (T s in arr)
            sb.AppendLine(s.ToString());
        return sb.ToString();
    }

    public static Rect Move(this Rect rect, float x, float y)
    {
        rect.x += x;
        rect.y += y;
        return rect;
    }

    public static Rect ResizeBy(this Rect rect, float width, float height)
    {
        rect.width += width;
        rect.height += height;
        return rect;
    }

    public static Rect Resize(this Rect rect, float width, float height)
    {
        rect.width = width;
        rect.height = height;
        return rect;
    }

    public static Rect Resize(this Rect rect, Vector2 size)
    {
        rect.width = size.x;
        rect.height = size.y;
        return rect;
    }

    public static System.Reflection.FieldInfo GetField<T>(this T t, string field)
    {
        return t.GetType().GetField(field);
    }

    public static string[] ToStringArray(this List<Bonanza> list)
    {
        string[] arr = new string[list.Count];
        for (int i = 0; i < arr.Length; i++)
            arr[i] = list[i].fieldName;
        return arr;
    }

    /*
    public static Vector3[] GetVectors(this List<Line> lineList)
    {
        List<Vector3> vectors = new List<Vector3>();
        for (int i = 0; i < lineList.Count; i++)
        {
            for (int k = 0; k < 2; k++)
                vectors.Add(lineList[i].GetVector(k));
        }
        if (vectors.Count % 2 != 0)
            vectors.Add(Vector3.one);
        return vectors.ToArray();
    }
    */

    public static Vector3[] CalculateFrustumCorners(this Camera camera, float depth)
    {
        if (!camera) return null;
        depth = Mathf.Clamp(depth, camera.nearClipPlane, camera.farClipPlane);
        Vector3[] corners = new Vector3[4];

        for (int x = 0; x < 2; x++)
        {
            for (int y = 0; y < 2; y++)
            {
                corners[x * 2 + y] = camera.ViewportToWorldPoint(new Vector3(x, y, depth));
            }
        }

        return corners;
    }

    public static bool IsMostlyNegative(this Vector3 vector)
    {
        return vector.x <= 0f && vector.z <= 0f;
    }

    public static Vector3 Clamp(this Vector3 v, float xMin, float xMax, float yMin, float yMax, float zMin, float zMax)
    {
        return new Vector3(Mathf.Clamp(v.x, xMin, xMax), Mathf.Clamp(v.y, yMin, yMax), Mathf.Clamp(v.z, zMin, zMax));
    }

    public static Vector3 ClampXY(this Vector3 v, float xMin, float xMax, float yMin, float yMax)
    {
        return new Vector3(Mathf.Clamp(v.x, xMin, xMax), Mathf.Clamp(v.y, yMin, yMax), v.z);
    }

    public static bool CanSee(this Camera camera, Bounds bounds)
    {
        return GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(camera), bounds);
    }
}
