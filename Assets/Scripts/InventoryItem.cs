using UnityEngine;
using TMPro;

public class InventorySlotItem : MonoBehaviour
{
    [Header("Slot Info")]
    public GameObject prefab;

    [Header("States")]
    public bool isEquipped = false;       // zichtbaar in Inspector
    public bool activated = false;        // zichtbaar in Inspector

    [HideInInspector]
    public PrefabReferenceAuto prefabRef;

    [Header("Optional Components")]
    public Light flashlightLight;         // Wordt automatisch gekoppeld als dit een flashlight is

    TextMeshProUGUI slotText;
    InventorySystem inventorySystem;

    public void Initialize(GameObject prefab, InventorySystem system)
    {
        this.prefab = prefab;
        inventorySystem = system;

        prefabRef = prefab.GetComponent<PrefabReferenceAuto>();

        slotText = GetComponentInChildren<TextMeshProUGUI>();
        if (slotText != null)
        {
            slotText.gameObject.SetActive(false);
            slotText.text = prefab.name;
        }

        activated = false;

        // ===== AUTOMATISCH FLASHLIGHT LIGHT COMPONENT OPHALEN =====
        if (prefabRef != null && prefabRef.category == PrefabReferenceAuto.ItemCategory.Temporary)
        {
            if (prefabRef.temporaryType == PrefabReferenceAuto.TemporaryType.Flashlight)
            {
                // Zoek ItemUse in scene
                ItemUse itemUse = GameObject.FindObjectOfType<ItemUse>();
                if (itemUse != null)
                {
                    flashlightLight = itemUse.flashlightLight;
                    if (flashlightLight != null)
                        flashlightLight.enabled = false; // standaard uit
                }
            }
        }
    }

    void Update()
    {
        // Alleen reageren als dit item equipped is
        if (isEquipped)
        {
            // Linkermuisknop toggled activated
            if (Input.GetMouseButtonDown(0))
            {
                activated = !activated;

                // Als het een zaklamp is, zet dan het Light component aan/uit
                if (prefabRef != null && prefabRef.category == PrefabReferenceAuto.ItemCategory.Temporary)
                {
                    if (prefabRef.temporaryType == PrefabReferenceAuto.TemporaryType.Flashlight && flashlightLight != null)
                    {
                        flashlightLight.enabled = activated;
                    }
                }
            }
        }
    }

    public void Equip()
    {
        if (isEquipped) return;
        isEquipped = true;
        if (slotText != null) slotText.gameObject.SetActive(true);

        if (prefabRef != null)
        {
            ItemUse itemUse = GameObject.FindObjectOfType<ItemUse>();
            if (itemUse != null)
            {
                itemUse.EquipItem(prefabRef.category, GetEnumFromCategory(prefabRef));
            }
        }
    }

    public void Unequip()
    {
        if (!isEquipped) return;
        isEquipped = false;
        if (slotText != null) slotText.gameObject.SetActive(false);

        if (prefabRef != null)
        {
            ItemUse itemUse = GameObject.FindObjectOfType<ItemUse>();
            if (itemUse != null)
            {
                itemUse.UnequipItem(prefabRef.category, GetEnumFromCategory(prefabRef));
            }
        }

        activated = false;

        if (flashlightLight != null)
            flashlightLight.enabled = false;
    }

    public object GetEnumFromCategory(PrefabReferenceAuto prefabRef)
    {
        switch (prefabRef.category)
        {
            case PrefabReferenceAuto.ItemCategory.Weapons: return prefabRef.weaponType;
            case PrefabReferenceAuto.ItemCategory.Temporary: return prefabRef.temporaryType;
            case PrefabReferenceAuto.ItemCategory.Limited: return prefabRef.limitedType;
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
