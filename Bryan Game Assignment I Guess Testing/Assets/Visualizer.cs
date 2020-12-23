using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Visualizer : AudioBehaviour
{
    [Header("Gizmos")]
    public float scale;
    public float spacing;
    public Vector3 frequencyRangeLabelOffset;

    [Header("Settings")]
    public float threshold;
    public float frequencyLow, frequencyHigh;
    public bool useScalar;

    public override void OnEnable()
    {
        base.OnEnable();
    }

    private void OnDrawGizmos()
    {
        if (!ManagerInstance || FrequencyBands == null) return;
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

    }

    public override void Update()
    {
        base.Update();
    }
    //44100 / 
}
