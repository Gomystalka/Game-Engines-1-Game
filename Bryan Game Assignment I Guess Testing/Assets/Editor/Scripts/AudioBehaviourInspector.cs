using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;

[CustomEditor(typeof(AudioBehaviour), true)]
public class AudioBehaviourInspector : Editor
{
    public const float kElementSpace = 12f;

    private SerializedProperty _audioSourceProperty;
    private List<SerializedPropertyGroup> _properties;

    private List<string> _excludedProperties;
    private static GUIContent _errorContent;

    private static GUIStyle _richTextStyle, _richTextBoxStyle;

    private void OnEnable()
    {
        _excludedProperties = new List<string>();
        _errorContent = EditorGUIUtility.IconContent("CollabConflict");
        _properties = new List<SerializedPropertyGroup>();
        _audioSourceProperty = serializedObject.FindProperty("source");

        GetAllProperties(serializedObject.GetIterator(), ref _properties);
        _richTextStyle = null;
        _richTextBoxStyle = null;
    }

    public override void OnInspectorGUI()
    {
        if(_richTextStyle == null)
            _richTextStyle = new GUIStyle(GUI.skin.label)
            {
                richText = true
            };
        if(_richTextBoxStyle == null)
            _richTextBoxStyle = new GUIStyle(EditorStyles.helpBox)
            {
                richText = true,
                alignment = TextAnchor.MiddleCenter
            };
        AudioBehaviour a = (AudioBehaviour)target;
        EditorGUILayout.PropertyField(_audioSourceProperty);
        if (a && a.source)
        {
            foreach (SerializedPropertyGroup prop in _properties)
            {
                SerializedProperty p = prop.property ?? serializedObject.FindProperty(prop.name);
                if (p == null) continue;
                if (p.name == "source") continue;

                EditorGUILayout.PropertyField(p);
            }
        }
        serializedObject.ApplyModifiedProperties();
    }

    /*
    private void CheckForSynchedVariables(AudioBehaviour b)
    {
        _excludedProperties.Clear();
        FieldInfo[] info = b.GetType().GetFields();
        
        foreach (FieldInfo field in info)
        {
            if (System.Attribute.GetCustomAttribute(field, typeof(SynchronizedVarAttribute)) is SynchronizedVarAttribute attr) {
                _excludedProperties.Add(field.Name);
                SerializedProperty sv = serializedObject.FindProperty(field.Name);

                bool hasTarget = !string.IsNullOrEmpty(attr.Target);
                bool targetIsValid = false;
                if(hasTarget)
                    targetIsValid = IsValidField(info, attr.Target);
                GUIContent content = hasTarget && targetIsValid ? new GUIContent($"<b><color=green>{sv.displayName}</color></b>", $"Synchronized with {attr.Target}")
                    : new GUIContent($"<b><color=red>{sv.displayName}</color></b>", $"{(!targetIsValid ? $"Sync target [{attr.Target}] is not valid!" : "Sync target not set!")}");

                EditorGUI.BeginDisabledGroup(targetIsValid);
                Rect[] rects = StyledPropertyField(sv, content,
                    !targetIsValid ? _errorContent.image : null);
                EditorGUI.EndDisabledGroup();

                if (!targetIsValid)
                {
                    Debug.LogError($"SyncVar: [{field.Name}]'s Target is null or not valid! The variable will not be synchronized.");
                    continue;
                }
                if(attr.HideTargetInInspector)
                    _excludedProperties.Add(attr.Target);
                GUIContent syncContent = new GUIContent($"<size=14>Synched with <b><color=yellow>{attr.Target}</color></b></size>");
                Rect syncRect = rects[0].Resize(_richTextBoxStyle.CalcSize(syncContent));
                syncRect = syncRect.Move((rects[0].width) - (syncRect.width),
                    (rects[0].height / 2f) - (syncRect.height / 2f));
                GUI.Label(syncRect, syncContent, _richTextBoxStyle);
            }
        }
    }

    private bool IsValidField(FieldInfo[] info, string field) {

        foreach (FieldInfo f in info) {
                //FindFieldAtPath(f.GetType().GetProperties(), field);
        }


        //Debug.Log(serializedObject.FindProperty("source").FindPropertyRelative("volume"));



        //Debug.Log(FindPropertyAtPath(field, serializedObject, null, 0) == null);

        //foreach (FieldInfo f in info)
            //if (f.Name == field) return true;
        return false;
    }
    */

    public void FindFieldAtPath(PropertyInfo[] currentFieldInfo, string path) {
        Debug.Log(currentFieldInfo[0].GetType());
    }

    /*
    public SerializedProperty FindPropertyAtPath(string path, SerializedObject obj = null, SerializedProperty prop = null, int level = 0) {
        if (level == 2)
            Debug.LogWarning(path);

        if (string.IsNullOrEmpty(path)) return prop;

        string currentElement = path;
        int separatorIndex = path.IndexOf('.');
        if (separatorIndex != -1)
            currentElement = path.Substring(0, separatorIndex);
        SerializedProperty sp = level == 0 ? obj.FindProperty(currentElement) : prop.FindPropertyRelative(currentElement);
        if (sp == null)
            return null;
        else
        {
            return FindPropertyAtPath(path.Substring(currentElement.Length + 1), null, sp, level + 1);
        }
        //if ()
    }
    */

    public void GetAllProperties(SerializedProperty entryPoint, ref List<SerializedPropertyGroup> list) {
        list.Clear();
        SerializedProperty current = entryPoint.Copy();
        while (current.Next(true))
        {
            if(!current.name.StartsWith("m_"))
                list.Add(new SerializedPropertyGroup() {
                    property = null,
                    name = current.name
                });
        }
    }

    /// Made by Tomasz Galka UwU
    /// <summary>
    ///     A more customizable PropertyField from EditorGUILayout
    /// </summary>
    /// <param name="prop">The SerializedProperty to be drawn</param>
    /// <param name="content">The optional content</param>
    /// <param name="tex">The optional icon</param>
    /// <param name="options"></param>
    /// <returns>Array containing Rects of all used GUI elements. 0 = Prefix Label Rect 1 = Optional Icon Rect (Can be null) 2 = Property Field Rect</returns>
    public static Rect[] StyledPropertyField(SerializedProperty prop, GUIContent content, Texture tex = null, GUIStyle style = null, params GUILayoutOption[] options) {
        Rect[] rects = new Rect[3];
        GUILayout.BeginHorizontal();
        if (style == null) style = _richTextStyle;

        EditorGUILayout.PrefixLabel(content, style, style);
        rects[0] = GUILayoutUtility.GetLastRect();
        if (tex)
        {
            Rect r = GUILayoutUtility.GetLastRect();
            r.x += r.width - kElementSpace;
            GUI.Label(r, _errorContent);
            rects[1] = r;
        }
        EditorGUILayout.PropertyField(prop, GUIContent.none);
        rects[2] = GUILayoutUtility.GetLastRect();
        GUILayout.EndHorizontal();

        return rects;
    }
}

public class SerializedPropertyGroup {
    public SerializedProperty property;
    public string name;
}