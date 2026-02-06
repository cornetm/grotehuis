using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventorySystem : MonoBehaviour
{
    [Header("Inventory Slots")]
    public RawImage[] inventorySlots;

    [Header("Items")]
    public Item[] items;

    [Header("Inventory List")]
    public List<string> inventoryList = new List<string>(); // Prefab namen van opgepakte items

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
        // Toetsen 1 t/m 9 om slots te selecteren
        for (int i = 0; i < inventorySlots.Length && i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                if (items[i] != null && items[i].item)
                {
                    ToggleSlot(i);
                }
            }
        }
    }

    private void ToggleSlot(int index)
    {
        // Als dit slot al actief is → deselect
        if (activeSlot == index)
        {
            DeactivateAll();
            activeSlot = -1;
            return;
        }

        // Anders → selecteer dit slot
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

    /// <summary>
    /// Voeg item toe aan inventory list en zet eventueel icon in UI
    /// </summary>
    public void AddItem(string prefabName, Texture itemIcon = null)
    {
        // Voeg prefab naam toe
        inventoryList.Add(prefabName);
        Debug.Log("Added to inventory: " + prefabName);

        // Zet icon in eerste vrije slot
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (inventorySlots[i] != null && !inventorySlots[i].enabled)
            {
                inventorySlots[i].enabled = true;

                if (itemIcon != null)
                    inventorySlots[i].texture = itemIcon;

                break; // Alleen eerste vrije slot vullen
            }
        }
    }
}
