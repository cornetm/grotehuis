using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ItemUse : MonoBehaviour
{
    [Header("Weapon Objects")]
    public GameObject butcherKnifeObject;
    public GameObject kitchenKnifeObject;
    public GameObject butterKnifeObject;

    [Header("Temporary Objects")]
    public GameObject flashlightObject;
    public Light flashlightLight;

    [Header("Flashlight Battery")]
    public Slider flashlightPowerSlider;
    public float maxFlashlightPower = 60f;

    public GameObject luciferObject;
    public GameObject candleObject;

    [Header("Limited Objects")]
    public GameObject pillsObject;
    public GameObject medkitObject;

    [Header("Medkit Settings")]
    public Slider medkitHoldSlider;
    public float medkitHoldDuration = 2f;
    public int medkitHealAmount = 50; // 🔹 Stel hier in hoeveel health de medkit geeft

    private Dictionary<string, bool> activatedState = new Dictionary<string, bool>();

    // ===== Medkit runtime variables =====
    public bool medkitEquipped = false;
    public float medkitTimer = 0f;
    public InventorySlotItem currentMedkitSlot = null;

    void Start()
    {
        SetAllOff();

        if (flashlightPowerSlider != null)
        {
            flashlightPowerSlider.maxValue = maxFlashlightPower;
            flashlightPowerSlider.value = maxFlashlightPower;
        }

        if (medkitHoldSlider != null)
        {
            medkitHoldSlider.gameObject.SetActive(false);
            medkitHoldSlider.maxValue = medkitHoldDuration;
            medkitHoldSlider.value = 0f;
        }
    }

    void Update()
    {
        // ===== Medkit logic =====
        if (medkitEquipped && currentMedkitSlot != null && medkitObject != null)
        {
            if (Input.GetMouseButton(0))
            {
                if (medkitHoldSlider != null)
                    medkitHoldSlider.gameObject.SetActive(true);

                medkitTimer += Time.deltaTime;

                if (medkitHoldSlider != null)
                    medkitHoldSlider.value = medkitTimer;

                if (medkitTimer >= medkitHoldDuration)
                {
                    // 🔹 Heal player met medkitHealAmount
                    PlayerHealth playerHealth = GameObject.FindObjectOfType<PlayerHealth>();
                    if (playerHealth != null)
                        playerHealth.Heal(medkitHealAmount);

                    // Remove medkit from inventory
                    if (currentMedkitSlot != null && currentMedkitSlot.inventorySystem != null)
                    {
                        int index = currentMedkitSlot.inventorySystem.slotComponents.IndexOf(currentMedkitSlot);
                        if (index >= 0)
                            currentMedkitSlot.inventorySystem.RemoveSlot(index);
                    }

                    // Disable medkit object
                    SetState(PrefabReferenceAuto.ItemCategory.Limited, PrefabReferenceAuto.LimitedType.Medkit, false);

                    // Reset medkit variables
                    medkitEquipped = false;
                    medkitTimer = 0f;
                    currentMedkitSlot = null;

                    if (medkitHoldSlider != null)
                        medkitHoldSlider.gameObject.SetActive(false);
                }
            }
            else
            {
                // Reset timer & slider if button released
                medkitTimer = 0f;
                if (medkitHoldSlider != null)
                {
                    medkitHoldSlider.value = 0f;
                    medkitHoldSlider.gameObject.SetActive(false);
                }
            }
        }
    }

    // ================= Equip / Unequip =================
    public void EquipItem(PrefabReferenceAuto.ItemCategory category, object typeEnum, InventorySlotItem slotItem = null)
    {
        SetState(category, typeEnum, true);

        string key = GetKey(category, typeEnum);
        if (!activatedState.ContainsKey(key))
            activatedState[key] = false;

        // Special logic voor medkit
        if (category == PrefabReferenceAuto.ItemCategory.Limited &&
            (PrefabReferenceAuto.LimitedType)typeEnum == PrefabReferenceAuto.LimitedType.Medkit)
        {
            medkitEquipped = true;
            currentMedkitSlot = slotItem;
            medkitTimer = 0f;

            if (medkitHoldSlider != null)
            {
                medkitHoldSlider.gameObject.SetActive(false);
                medkitHoldSlider.value = 0f;
            }
        }

        // Flashlight logic
        if (category == PrefabReferenceAuto.ItemCategory.Temporary &&
            typeEnum.Equals(PrefabReferenceAuto.TemporaryType.Flashlight))
        {
            if (flashlightLight != null)
                flashlightLight.enabled = false;
        }
    }

    public void UnequipItem(PrefabReferenceAuto.ItemCategory category, object typeEnum)
    {
        SetState(category, typeEnum, false);

        string key = GetKey(category, typeEnum);
        if (activatedState.ContainsKey(key))
            activatedState[key] = false;

        if (category == PrefabReferenceAuto.ItemCategory.Limited &&
            (PrefabReferenceAuto.LimitedType)typeEnum == PrefabReferenceAuto.LimitedType.Medkit)
        {
            medkitEquipped = false;
            medkitTimer = 0f;
            currentMedkitSlot = null;

            if (medkitHoldSlider != null)
            {
                medkitHoldSlider.gameObject.SetActive(false);
                medkitHoldSlider.value = 0f;
            }
        }

        if (category == PrefabReferenceAuto.ItemCategory.Temporary &&
            typeEnum.Equals(PrefabReferenceAuto.TemporaryType.Flashlight))
        {
            if (flashlightLight != null)
                flashlightLight.enabled = false;
        }
    }

    // ================= Helper Functions =================
    private string GetKey(PrefabReferenceAuto.ItemCategory category, object typeEnum)
    {
        return category.ToString() + "_" + typeEnum.ToString();
    }

    public void SetState(PrefabReferenceAuto.ItemCategory category, object typeEnum, bool state)
    {
        switch (category)
        {
            case PrefabReferenceAuto.ItemCategory.Weapons:
                PrefabReferenceAuto.WeaponType weapon = (PrefabReferenceAuto.WeaponType)typeEnum;
                if (weapon == PrefabReferenceAuto.WeaponType.ButcherKnife && butcherKnifeObject != null) butcherKnifeObject.SetActive(state);
                if (weapon == PrefabReferenceAuto.WeaponType.KitchenKnife && kitchenKnifeObject != null) kitchenKnifeObject.SetActive(state);
                if (weapon == PrefabReferenceAuto.WeaponType.ButterKnife && butterKnifeObject != null) butterKnifeObject.SetActive(state);
                break;

            case PrefabReferenceAuto.ItemCategory.Temporary:
                PrefabReferenceAuto.TemporaryType temp = (PrefabReferenceAuto.TemporaryType)typeEnum;
                if (temp == PrefabReferenceAuto.TemporaryType.Flashlight && flashlightObject != null) flashlightObject.SetActive(state);
                if (temp == PrefabReferenceAuto.TemporaryType.Lucifer && luciferObject != null) luciferObject.SetActive(state);
                if (temp == PrefabReferenceAuto.TemporaryType.Candle && candleObject != null) candleObject.SetActive(state);
                break;

            case PrefabReferenceAuto.ItemCategory.Limited:
                PrefabReferenceAuto.LimitedType limited = (PrefabReferenceAuto.LimitedType)typeEnum;
                if (limited == PrefabReferenceAuto.LimitedType.Pills && pillsObject != null) pillsObject.SetActive(state);
                if (limited == PrefabReferenceAuto.LimitedType.Medkit && medkitObject != null) medkitObject.SetActive(state);
                break;
        }
    }

    public void SetAllOff()
    {
        if (butcherKnifeObject != null) butcherKnifeObject.SetActive(false);
        if (kitchenKnifeObject != null) kitchenKnifeObject.SetActive(false);
        if (butterKnifeObject != null) butterKnifeObject.SetActive(false);

        if (flashlightObject != null) flashlightObject.SetActive(false);
        if (luciferObject != null) luciferObject.SetActive(false);
        if (candleObject != null) candleObject.SetActive(false);

        if (pillsObject != null) pillsObject.SetActive(false);
        if (medkitObject != null) medkitObject.SetActive(false);

        activatedState.Clear();

        if (flashlightLight != null)
            flashlightLight.enabled = false;

        if (medkitHoldSlider != null)
            medkitHoldSlider.gameObject.SetActive(false);
    }
}