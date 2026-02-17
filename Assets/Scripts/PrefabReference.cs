using UnityEngine;

public class PrefabReferenceAuto : MonoBehaviour
{
    public enum ItemCategory { Weapons, Temporary, Limited }
    public enum WeaponType { ButcherKnife, KitchenKnife, ButterKnife }
    public enum TemporaryType { Flashlight, Lucifer, Candle }
    public enum LimitedType { Pills }

    [Header("Prefab Reference")]
    public GameObject prefab;
    public Texture icon;

    [Header("Item Category")]
    public ItemCategory category;

    public WeaponType weaponType;
    public TemporaryType temporaryType;
    public LimitedType limitedType;

    [Header("Weapon Stats")]
    public float damage;
    public float speed;
    public float range;

    [Header("Temporary Stats")]
    public float lifespan;

    [Header("Limited Stats")]
    public int number;
}
