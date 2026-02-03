using UnityEngine;
using UnityEngine.UI;

public class InventorySystem : MonoBehaviour
{
    [Header("Inventory Slots")]
    public RawImage[] inventorySlots; // alle RawImages van de inventory

    void Start()
    {
        // Zet alle slots uit bij start
        foreach (RawImage slot in inventorySlots)
        {
            if (slot != null)
            {
                slot.enabled = false;
                slot.texture = null;
            }
        }
    }

    // ================= INVENTORY FUNCTIES =================

    // Activeer een slot en zet alle andere uit
    public void ActivateSlot(int slotNumber)
    {
        int index = slotNumber - 1; // slotNumber 1 => index 0

        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (inventorySlots[i] != null)
            {
                inventorySlots[i].enabled = (i == index);
            }
        }
    }

    // Optioneel: Deactiveer een slot
    public void DeactivateSlot(int slotNumber)
    {
        int index = slotNumber - 1;
        if (index >= 0 && index < inventorySlots.Length && inventorySlots[index] != null)
        {
            inventorySlots[index].enabled = false;
        }
    }

    void Update()
    {
        // Check toetsen 1 t/m 9 voor snel activeren
        for (int i = 0; i < inventorySlots.Length && i < 9; i++)
        {
            if (Input.GetKeyDown((i + 1).ToString()))
            {
                ActivateSlot(i + 1); // activeer slot en zet de rest uit
            }
        }
    }
}
