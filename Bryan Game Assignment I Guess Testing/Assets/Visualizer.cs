using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Visualizer : AudioBehaviour
{
    //[SynchronizedVar(Target = "source")]
    [Header("Gizmos")]
    public float scale;
    public float spacing;
    public Vector3 frequencyRangeLabelOffset;

    [Header("Settings")]
    public float threshold;
    public float frequencyLow, frequencyHigh;
    public bool useScalar;

    [Header("Player")]
    public Transform friend;
    public float speed;
    public float gravity;
    public float groundLevel;
    public Vector2 shakeRangeX;
    public Vector2 shakeRangeZ;
    public float timeoutSeconds = 1.5f;

    private float _timer;
    private Vector3 _oldPosition;

    [Gay] public int yes;

    public override void OnEnable()
    {
        base.OnEnable();
        _timer = 0f;
        if (friend)
            _oldPosition = friend.position;
    }

    private void Start()
    {

    }

    private void OnDrawGizmos()
    {
        /*
        for (int s = 0; s < FrequencyBands.Length; s++) {
            float freq = FrequencyBands[s].smoothedFrequency;
            Gizmos.color = Color.HSVToRGB(1f / FrequencyBands.Length * s, 1f, 1f);
            float yScale = freq;
            Vector3 pos = transform.position + new Vector3(s * scale + spacing, yScale / 2f, 0f);
            Gizmos.DrawCube(pos, new Vector3(scale, yScale, scale));

#if UNITY_EDITOR
            GUIStyle st = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };
            st.normal.textColor = Color.yellow;
        UnityEditor.Handles.Label(new Vector3(pos.x, transform.position.y, pos.z) + frequencyRangeLabelOffset,
                $"{FrequencyBands[s].frequencyRange.Integer.low}Hz - {FrequencyBands[s].frequencyRange.Integer.high}Hz",
                st);
#endif
            //Gizmos.color = Color.red;
            //Gizmos.DrawCube(transform.position + new Vector3(pos.x, FrequencyBands[s].maxFrequency - scale), new Vector3(scale, scale, scale));
        }
        */
    }

    public override void Update()
    {
        base.Update();
        if (!friend) return;
        Vector3 position = friend.position;

        if (position.y > groundLevel)
            position.y -= gravity * Time.deltaTime;
        else
            position.y = groundLevel;

        float avg = GetAverageFrequencyInRange(frequencyLow, frequencyHigh, useScalar);
        Debug.Log(avg);
        if (avg >= threshold)
        {
            _timer = 0f;
            position.y += speed * Time.deltaTime;
            ShakePlayer(ref position);
        }

        if (_timer >= timeoutSeconds)
            friend.position = _oldPosition;
        else
            _timer += Time.deltaTime;

        friend.transform.position = position;
    }

    private void ShakePlayer(ref Vector3 position) =>
        position = new Vector3(_oldPosition.x + Random.Range(shakeRangeX.x, shakeRangeX.y),
            _oldPosition.y,
            _oldPosition.z + Random.Range(shakeRangeZ.x, shakeRangeZ.y));

    //44100 / 
}
