#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class EditorKeyListener
{
    private const KeyCode toggleKey = KeyCode.P;
    private const string targetSceneName = "Props";

    static EditorKeyListener()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private static void OnSceneGUI(SceneView sceneView)
    {
        Event e = Event.current;

        if (e.type == EventType.KeyDown && e.keyCode == toggleKey)
        {
            if (SceneManager.GetActiveScene().name != targetSceneName)
                return;

            EditorLightToggle[] lamps = Object.FindObjectsByType<EditorLightToggle>(FindObjectsSortMode.None);
            LightSwitch[] switches = Object.FindObjectsByType<LightSwitch>(FindObjectsSortMode.None);

            if (lamps.Length == 0 && switches.Length == 0)
                return;

            bool currentState = true;

            if (lamps.Length > 0)
            {
                SerializedObject so = new SerializedObject(lamps[0]);
                currentState = so.FindProperty("isOn").boolValue;
            }
            else
            {
                SerializedObject so = new SerializedObject(switches[0]);
                currentState = so.FindProperty("isOn").boolValue;
            }

            bool newState = !currentState;

            for (int i = 0; i < lamps.Length; i++)
            {
                SerializedObject so = new SerializedObject(lamps[i]);
                SerializedProperty prop = so.FindProperty("isOn");

                prop.boolValue = newState;
                so.ApplyModifiedProperties();
            }

            for (int i = 0; i < switches.Length; i++)
            {
                SerializedObject so = new SerializedObject(switches[i]);
                SerializedProperty prop = so.FindProperty("isOn");

                prop.boolValue = newState;
                so.ApplyModifiedProperties();
            }

            e.Use();
        }
    }
}
#endif