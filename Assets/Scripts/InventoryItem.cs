using UnityEngine;
using TMPro;

public class InventoryItem : MonoBehaviour
{
    [Header("Slot Info")]
    public GameObject prefab; // Originele prefab uit Assets

    [HideInInspector]
    public bool isEquipped = false;

    private TextMeshProUGUI slotText;
    private InventorySystem inventorySystem;

    // ================= INIT =================
    public void Initialize(GameObject prefab, InventorySystem system)
    {
        this.prefab = prefab;
        this.inventorySystem = system;

        slotText = GetComponentInChildren<TextMeshProUGUI>();
        if (slotText != null)
        {
            slotText.gameObject.SetActive(false);
            slotText.text = prefab.name; // Toon prefab naam
        }
    }

    // ================= EQUIP =================
    public void Equip()
    {
        isEquipped = true;
        if (slotText != null)
            slotText.gameObject.SetActive(true);
    }

    // ================= UNEQUIP =================
    public void Unequip()
    {
        isEquipped = false;
        if (slotText != null)
            slotText.gameObject.SetActive(false);
    }

    // ================= DROP =================
    public void Drop()
    {
        if (!isEquipped || inventorySystem == null) return;
        inventorySystem.DropSlotItem(this);
    }
}
