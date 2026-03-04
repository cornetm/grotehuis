using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InventorySlotItem : MonoBehaviour
{
    [Header("Slot Info")]
    public GameObject prefab;

    [Header("States")]
    public bool isEquipped = false;
    public bool activated = false;

    [HideInInspector]
    public PrefabReferenceAuto prefabRef;

    [Header("Flashlight Battery")]
    public float flashlightPower = 60f;
    public float batteryDrainPerSecond = 1f;

    [Header("Optional Components")]
    public Light flashlightLight;

    TextMeshProUGUI slotText;
    InventorySystem inventorySystem;
    ItemUse itemUse;

    public void Initialize(GameObject prefab, InventorySystem system)
    {
        this.prefab = prefab;
        inventorySystem = system;

        prefabRef = prefab.GetComponent<PrefabReferenceAuto>();
        itemUse = GameObject.FindObjectOfType<ItemUse>();

        slotText = GetComponentInChildren<TextMeshProUGUI>();
        if (slotText != null)
        {
            slotText.gameObject.SetActive(false);
            slotText.text = prefab.name;
        }

        activated = false;

        if (prefabRef != null &&
            prefabRef.category == PrefabReferenceAuto.ItemCategory.Temporary &&
            prefabRef.temporaryType == PrefabReferenceAuto.TemporaryType.Flashlight)
        {
            if (itemUse != null)
            {
                flashlightLight = itemUse.flashlightLight;

                if (itemUse.flashlightPowerSlider != null)
                {
                    itemUse.flashlightPowerSlider.maxValue = flashlightPower;
                    itemUse.flashlightPowerSlider.value = flashlightPower;
                }
            }
        }
    }

    void Update()
    {
        if (isEquipped)
        {
            if (Input.GetMouseButtonDown(0))
            {
                activated = !activated;

                if (flashlightLight != null && flashlightPower > 0)
                    flashlightLight.enabled = activated;
            }

            // ===== BATTERY DRAIN =====
            if (activated && flashlightPower > 0)
            {
                flashlightPower -= batteryDrainPerSecond * Time.deltaTime;

                if (itemUse != null && itemUse.flashlightPowerSlider != null)
                    itemUse.flashlightPowerSlider.value = flashlightPower;

                if (flashlightPower <= 0)
                {
                    flashlightPower = 0;
                    activated = false;

                    if (flashlightLight != null)
                        flashlightLight.enabled = false;
                }
            }
        }
    }

    public void Equip()
    {
        if (isEquipped) return;

        isEquipped = true;

        if (slotText != null)
            slotText.gameObject.SetActive(true);

        if (prefabRef != null && itemUse != null)
        {
            itemUse.EquipItem(prefabRef.category, GetEnumFromCategory(prefabRef));

            // 🔹 Update flashlight slider naar deze flashlight power
            if (prefabRef.category == PrefabReferenceAuto.ItemCategory.Temporary &&
                prefabRef.temporaryType == PrefabReferenceAuto.TemporaryType.Flashlight &&
                itemUse.flashlightPowerSlider != null)
            {
                itemUse.flashlightPowerSlider.value = flashlightPower;
            }
        }
    }

    // 🔥 FIXED UNEQUIP (Optie A)
    public void Unequip()
    {
        if (!isEquipped) return;

        isEquipped = false;

        if (slotText != null)
            slotText.gameObject.SetActive(false);

        // Check of er nog een ander slot met hetzelfde item equipped is
        bool otherSameItemEquipped = false;

        if (inventorySystem != null)
        {
            foreach (var slot in inventorySystem.slotComponents)
            {
                if (slot != this &&
                    slot.isEquipped &&
                    slot.prefabRef != null &&
                    prefabRef != null &&
                    slot.prefabRef.category == prefabRef.category &&
                    slot.GetEnumFromCategory(slot.prefabRef)
                        .Equals(GetEnumFromCategory(prefabRef)))
                {
                    otherSameItemEquipped = true;
                    break;
                }
            }
        }

        // Alleen uitzetten als er geen andere dezelfde actief is
        if (!otherSameItemEquipped)
        {
            if (prefabRef != null && itemUse != null)
                itemUse.UnequipItem(prefabRef.category, GetEnumFromCategory(prefabRef));

            activated = false;

            if (flashlightLight != null)
                flashlightLight.enabled = false;
        }
    }

    public object GetEnumFromCategory(PrefabReferenceAuto prefabRef)
    {
        switch (prefabRef.category)
        {
            case PrefabReferenceAuto.ItemCategory.Weapons:
                return prefabRef.weaponType;

            case PrefabReferenceAuto.ItemCategory.Temporary:
                return prefabRef.temporaryType;

            case PrefabReferenceAuto.ItemCategory.Limited:
                return prefabRef.limitedType;
        }

        return null;
    }

    public void Drop()
    {
        if (!isEquipped || inventorySystem == null) return;

        inventorySystem.DropSlotItem(this);

        activated = false;

        if (flashlightLight != null)
            flashlightLight.enabled = false;
    }
}
