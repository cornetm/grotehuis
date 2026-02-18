using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PrefabReferenceAuto))]
public class PrefabReferenceAutoEditor : Editor
{
    public override void OnInspectorGUI()
    {
        PrefabReferenceAuto item = (PrefabReferenceAuto)target;

        // ================= PREFAB REFERENCE =================
        EditorGUILayout.LabelField("Prefab Reference", EditorStyles.boldLabel);
        item.prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", item.prefab, typeof(GameObject), false);
        item.icon = (Texture)EditorGUILayout.ObjectField("Icon", item.icon, typeof(Texture), false);

        EditorGUILayout.Space();

        // ================= CATEGORY =================
        EditorGUILayout.LabelField("Item Category", EditorStyles.boldLabel);
        item.category = (PrefabReferenceAuto.ItemCategory)EditorGUILayout.EnumPopup("Category", item.category);

        EditorGUILayout.Space();

        // ================= CATEGORY SPECIFIC FIELDS =================
        switch (item.category)
        {
            case PrefabReferenceAuto.ItemCategory.Weapons:
                item.weaponType = (PrefabReferenceAuto.WeaponType)
                    EditorGUILayout.EnumPopup("Weapon Type", item.weaponType);

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Weapon Stats", EditorStyles.boldLabel);
                item.damage = EditorGUILayout.FloatField("Damage", item.damage);
                item.speed = EditorGUILayout.FloatField("Speed", item.speed);
                item.range = EditorGUILayout.FloatField("Range", item.range);
                break;

            case PrefabReferenceAuto.ItemCategory.Temporary:
                item.temporaryType = (PrefabReferenceAuto.TemporaryType)
                    EditorGUILayout.EnumPopup("Temporary Type", item.temporaryType);

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Temporary Stats", EditorStyles.boldLabel);
                item.lifespan = EditorGUILayout.FloatField("Lifespan", item.lifespan);
                break;

            case PrefabReferenceAuto.ItemCategory.Limited:
                item.limitedType = PrefabReferenceAuto.LimitedType.Pills;

                EditorGUILayout.LabelField("Limited Item: Pills");

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Limited Stats", EditorStyles.boldLabel);
                item.number = EditorGUILayout.IntField("Number", item.number);
                break;
        }

        EditorGUILayout.Space();

        // ================= THROW SETTINGS =================
        EditorGUILayout.LabelField("Throw Settings", EditorStyles.boldLabel);
        item.throwRotationOffset = EditorGUILayout.Vector3Field("Throw Rotation Offset", item.throwRotationOffset);

        // ================= APPLY CHANGES =================
        if (GUI.changed)
            EditorUtility.SetDirty(item);
    }
}
