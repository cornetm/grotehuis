using UnityEngine;

public class PrefabReferenceAuto : MonoBehaviour
{
    [Header("Prefab Reference")]
    public GameObject prefab; // Zet hier het prefab vanuit Assets in
    public Texture icon;      // Optioneel: icon voor inventory

    [Header("Item Type")]
    public bool isWeapon = false;
    public bool isTemporary = false;
    public bool isLimited = false;

    [Header("Weapon Stats")]
    public float damage;
    public float speed;
    public float range;

    [Header("Temporary Stats")]
    public float lifespan;

    [Header("Limited Stats")]
    public int number;

    // ================== EDITOR VALIDATION ==================
    private void OnValidate()
    {
        // Zorg dat alleen 1 type tegelijk actief is
        if (isWeapon)
        {
            isTemporary = false;
            isLimited = false;
        }
        else if (isTemporary)
        {
            isWeapon = false;
            isLimited = false;
        }
        else if (isLimited)
        {
            isWeapon = false;
            isTemporary = false;
        }
    }
}
