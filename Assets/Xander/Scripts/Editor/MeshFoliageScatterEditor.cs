using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MeshFoliageScatter))]
public class MeshFoliageScatterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MeshFoliageScatter scatter = (MeshFoliageScatter)target;

        GUILayout.Space(10);

        if (GUILayout.Button("Generate Foliage", GUILayout.Height(35)))
        {
            scatter.GenerateFoliage();

            EditorUtility.SetDirty(scatter);
            if (scatter.generatedParent != null)
                EditorUtility.SetDirty(scatter.generatedParent.gameObject);
        }

        if (GUILayout.Button("Clear Generated Foliage", GUILayout.Height(30)))
        {
            scatter.ClearGeneratedFoliage();

            EditorUtility.SetDirty(scatter);
            if (scatter.generatedParent != null)
                EditorUtility.SetDirty(scatter.generatedParent.gameObject);
        }
    }
}