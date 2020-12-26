using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;

[CustomEditor(typeof(MonoBehaviour), true)]
public class MonobehaviourEditorExtension : Editor
{
    public const float fixedDeltaTime = 1f / 60f;

    private List<Bonanza> _excludedProperties;
    private float _step = 0.2f;
    private float _h;
    private GUIStyle _style;

    private void OnEnable()
    {
        _excludedProperties = new List<Bonanza>();
        EditorApplication.update += delegate {
            _h = _h + _step * fixedDeltaTime < 1f ? _h + _step * fixedDeltaTime : 0f;
            Repaint();
        };
    }

    public override void OnInspectorGUI()
    {
        _style = new GUIStyle(GUI.skin.label)
        {
            richText = true,
            fontSize = 20
        };
        
        _style.normal.textColor = Color.HSVToRGB(_h, 1f, 1f);
        //base.OnInspectorGUI();
        DrawPropertiesExcluding(serializedObject, _excludedProperties.ToStringArray());
        DrawExcludedProperties();
        CheckForAttributes();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawExcludedProperties() {
        foreach (Bonanza prop in _excludedProperties) {
            if (serializedObject.FindProperty(prop.fieldName) is SerializedProperty p) {
                AudioBehaviourInspector.StyledPropertyField(p, new GUIContent(p.displayName), null, _style);
            }
        }
    }

    private void CheckForAttributes() {
        _excludedProperties.Clear();
       FieldInfo[] fields = target.GetType().GetFields();
        foreach (FieldInfo f in fields) {
            if(Attribute.GetCustomAttribute(f, typeof(RainbowAttribute)) is RainbowAttribute attr)
                _excludedProperties.Add(new Bonanza() {
                    fieldName = f.Name,
                    randomizeValue = attr.RandomValue
            });
        }
    }
}