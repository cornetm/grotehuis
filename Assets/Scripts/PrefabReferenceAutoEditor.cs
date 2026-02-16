using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PrefabReferenceAuto))]
public class PrefabReferenceAutoEditor : Editor
{
    public override void OnInspectorGUI()
    {
        PrefabReferenceAuto item = (PrefabReferenceAuto)target;

        // Toon standaard prefab & icon
        EditorGUILayout.LabelField("Prefab Reference", EditorStyles.boldLabel);
        item.prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", item.prefab, typeof(GameObject), false);
        item.icon = (Texture)EditorGUILayout.ObjectField("Icon", item.icon, typeof(Texture), false);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Item Type", EditorStyles.boldLabel);

        // Type toggles
        item.isWeapon = EditorGUILayout.Toggle("Is Weapon?", item.isWeapon);
        item.isTemporary = EditorGUILayout.Toggle("Is Temporary?", item.isTemporary);
        item.isLimited = EditorGUILayout.Toggle("Is Limited?", item.isLimited);

        // Zorg dat maar 1 type tegelijk kan
        if (item.isWeapon)
        {
            item.isTemporary = false;
            item.isLimited = false;
        }
        else if (item.isTemporary)
        {
            item.isWeapon = false;
            item.isLimited = false;
        }
        else if (item.isLimited)
        {
            item.isWeapon = false;
            item.isTemporary = false;
        }

        EditorGUILayout.Space();

        // Toon relevante stats
        if (item.isWeapon)
        {
            EditorGUILayout.LabelField("Weapon Stats", EditorStyles.boldLabel);
            item.damage = EditorGUILayout.FloatField("Damage", item.damage);
            item.speed = EditorGUILayout.FloatField("Speed", item.speed);
            item.range = EditorGUILayout.FloatField("Range", item.range);
        }
        else if (item.isTemporary)
        {
            EditorGUILayout.LabelField("Temporary Stats", EditorStyles.boldLabel);
            item.lifespan = EditorGUILayout.FloatField("Lifespan", item.lifespan);
        }
        else if (item.isLimited)
        {
            EditorGUILayout.LabelField("Limited Stats", EditorStyles.boldLabel);
            item.number = EditorGUILayout.IntField("Number", item.number);
        }

        // Zorg dat wijzigingen opgeslagen worden
        if (GUI.changed)
            EditorUtility.SetDirty(item);
    }
}
