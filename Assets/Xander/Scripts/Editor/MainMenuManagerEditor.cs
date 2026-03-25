#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MainMenuManager))]
public class MainMenuManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawDefaultInspectorExceptCarCrashBlock();

        SerializedProperty carCrashProp = serializedObject.FindProperty("carCrash");
        if (carCrashProp != null)
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Car Crash", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(carCrashProp, new GUIContent("CarCrash"));

            if (carCrashProp.boolValue)
            {
                EditorGUI.indentLevel++;

                DrawProp("startGameTag");
                DrawProp("startGamePromptObject");
                DrawProp("startGameLeftMouseObject");
                DrawProp("startGameRightMouseObject");
                DrawProp("roadTileManager");

                EditorGUI.indentLevel--;
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawDefaultInspectorExceptCarCrashBlock()
    {
        string[] excluded =
        {
            "m_Script",
            "carCrash",
            "startGameTag",
            "startGamePromptObject",
            "startGameLeftMouseObject",
            "startGameRightMouseObject",
            "roadTileManager"
        };

        DrawPropertiesExcluding(serializedObject, excluded);
    }

    private void DrawProp(string propertyName)
    {
        SerializedProperty prop = serializedObject.FindProperty(propertyName);
        if (prop != null)
            EditorGUILayout.PropertyField(prop, true);
    }
}
#endif