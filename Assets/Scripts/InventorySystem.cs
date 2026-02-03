using UnityEngine;
using UnityEngine.UI;

public class InventorySystem : MonoBehaviour
{
    [Header("Inventory Slots")]
    public RawImage[] inventorySlots;

    [Header("Items")]
    public Item[] items;

    private int activeSlot = -1; // -1 = niets geselecteerd

    void Start()
    {
        // Alles uit bij start
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (inventorySlots[i] != null)
                inventorySlots[i].enabled = false;
        }
    }

    void Update()
    {
        // Toetsen 1 t/m 9
        for (int i = 0; i < inventorySlots.Length && i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                // Alleen als item bestaat
                if (items[i] != null && items[i].item)
                {
                    ToggleSlot(i);
                }
            }
        }
    }

    private void ToggleSlot(int index)
    {
        // Als dit slot al actief is → alles uit (deselect)
        if (activeSlot == index)
        {
            DeactivateAll();
            activeSlot = -1;
            return;
        }

        // Anders → dit slot selecteren
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (inventorySlots[i] != null)
                inventorySlots[i].enabled = (i == index);
        }

        activeSlot = index;
    }

    private void DeactivateAll()
    {
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (inventorySlots[i] != null)
                inventorySlots[i].enabled = false;
        }
    }
}
