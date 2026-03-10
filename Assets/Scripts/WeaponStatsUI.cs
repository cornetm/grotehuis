using UnityEngine;
using UnityEngine.UI;

public class WeaponStatsUI : MonoBehaviour
{
    [Header("Sliders (drag your UI sliders here)")]
    public Slider damageSlider;
    public Slider speedSlider;
    public Slider rangeSlider;

    [Header("Slider Max Values (set in Inspector)")]
    public float damageMax = 100f;
    public float speedMax = 20f;
    public float rangeMax = 20f;

    [Header("Inventory System Reference")]
    public InventorySystem inventorySystem; // Sleep hier je InventorySystem van de speler

    void Start()
    {
        // Zet de max waardes van de sliders
        if (damageSlider != null) damageSlider.maxValue = damageMax;
        if (speedSlider != null) speedSlider.maxValue = speedMax;
        if (rangeSlider != null) rangeSlider.maxValue = rangeMax;

        // Sliders uit bij start
        SetSlidersActive(false);
    }

    void Update()
    {
        UpdateWeaponStats();
    }

    private void UpdateWeaponStats()
    {
        InventorySlotItem equippedItem = inventorySystem.CurrentEquippedItem();

        if (equippedItem != null && equippedItem.prefabRef != null &&
            equippedItem.prefabRef.category == PrefabReferenceAuto.ItemCategory.Weapons)
        {
            // Sliders aanzetten
            SetSlidersActive(true);

            // Gebruik stats van InventorySlotItem en clamp binnen max
            if (damageSlider != null) damageSlider.value = Mathf.Clamp(equippedItem.weaponDamage, 0, damageMax);
            if (speedSlider != null) speedSlider.value = Mathf.Clamp(equippedItem.weaponSpeed, 0, speedMax);
            if (rangeSlider != null) rangeSlider.value = Mathf.Clamp(equippedItem.weaponRange, 0, rangeMax);
        }
        else
        {
            // Sliders uitzetten
            SetSlidersActive(false);
        }
    }

    private void SetSlidersActive(bool active)
    {
        if (damageSlider != null) damageSlider.gameObject.SetActive(active);
        if (speedSlider != null) speedSlider.gameObject.SetActive(active);
        if (rangeSlider != null) rangeSlider.gameObject.SetActive(active);
    }
}