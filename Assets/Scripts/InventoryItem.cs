using UnityEngine;
using TMPro;

public class InventorySlotItem : MonoBehaviour
{
    [Header("Slot Info")]
    public GameObject prefab;

    [HideInInspector]
    public bool isEquipped = false;

    [HideInInspector]
    public PrefabReferenceAuto prefabRef;

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
                // Gebruik EquipItem ipv SetState!
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
                // Gebruik UnequipItem ipv SetState!
                itemUse.UnequipItem(prefabRef.category, GetEnumFromCategory(prefabRef));
            }
        }
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
    }
}
