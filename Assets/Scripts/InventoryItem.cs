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
        isEquipped = true;
        if (slotText != null)
            slotText.gameObject.SetActive(true);

        // 🔦 Auto-enable flashlight controller als item Temporary → Flashlight
        if (prefabRef != null &&
            prefabRef.category == PrefabReferenceAuto.ItemCategory.Temporary &&
            prefabRef.temporaryType == PrefabReferenceAuto.TemporaryType.Flashlight)
        {
            FlashlightController fl = GameObject.FindObjectOfType<FlashlightController>();
            if (fl != null)
                fl.gameObject.SetActive(true);
        }
    }

    public void Unequip()
    {
        isEquipped = false;
        if (slotText != null)
            slotText.gameObject.SetActive(false);

        // 🔦 Auto-disable flashlight als dit uit-equip wordt
        if (prefabRef != null &&
            prefabRef.category == PrefabReferenceAuto.ItemCategory.Temporary &&
            prefabRef.temporaryType == PrefabReferenceAuto.TemporaryType.Flashlight)
        {
            FlashlightController fl = GameObject.FindObjectOfType<FlashlightController>();
            if (fl != null)
                fl.SetState(false);
        }
    }

    public void Drop()
    {
        if (!isEquipped || inventorySystem == null) return;
        inventorySystem.DropSlotItem(this);
    }
}
