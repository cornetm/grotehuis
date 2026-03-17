using UnityEngine;

public class PrefabReferenceAuto : MonoBehaviour
{
    // ================= ITEM TYPES =================
    public enum ItemCategory { Weapons, Temporary, Limited }
    public enum WeaponType { ButcherKnife, KitchenKnife, ButterKnife }
    public enum TemporaryType { Flashlight, Lucifer, Candle }
    public enum LimitedType { Pills, Medkit } // <-- Medkit toegevoegd

    // ================= PREFAB REFERENCE =================
    [Header("Prefab Reference")]
    public GameObject prefab;
    public Texture icon;

    // ================= CATEGORY =================
    [Header("Item Category")]
    public ItemCategory category;

    public WeaponType weaponType;
    public TemporaryType temporaryType;
    public LimitedType limitedType;

    // ================= THROW SETTINGS =================
    [Header("Throw Settings")]
    public Vector3 throwRotationOffset = Vector3.zero;

    // ================= WEAPON STATS =================
    [Header("Weapon Stats")]
    public float damage;
    public float speed;
    public float range;

    // ================= TEMPORARY STATS =================
    [Header("Temporary Stats")]
    public float lifespan;

    // ================= LIMITED STATS =================
    [Header("Limited Stats")]
    public int number;
}