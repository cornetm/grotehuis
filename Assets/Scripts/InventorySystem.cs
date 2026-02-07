using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventorySystem : MonoBehaviour
{
    [Header("Inventory Slots")]
    public RawImage[] inventorySlots;

    [Header("Slot Scale Settings")]
    public float normalScale = 1f;
    public float selectedScale = 1.4f;

    [Header("Inventory List")]
    public List<string> inventoryList = new List<string>();

    private int activeSlot = -1;

    void Start()
    {
        // Alles uit + normaal formaat
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (inventorySlots[i] != null)
            {
                inventorySlots[i].enabled = false;
                inventorySlots[i].transform.localScale = Vector3.one * normalScale;
            }
        }
    }

    void Update()
    {
        // 1 t/m 9 selecteren slots
        for (int i = 0; i < inventorySlots.Length && i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SelectSlot(i);
            }
        }
    }

    private void SelectSlot(int index)
    {
        if (index >= inventorySlots.Length) return;
        if (!inventorySlots[index].enabled) return;

        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (inventorySlots[i] == null) continue;

            // Geselecteerde slot groot, rest klein
            inventorySlots[i].transform.localScale =
                (i == index) ? Vector3.one * selectedScale : Vector3.one * normalScale;
        }

        activeSlot = index;
    }

    // Wordt aangeroepen vanuit FirstPersonCamera
    public void AddItem(string prefabName, Texture itemIcon = null)
    {
        inventoryList.Add(prefabName);
        Debug.Log("Added to inventory: " + prefabName);

        // Eerste vrije slot vinden
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (!inventorySlots[i].enabled)
            {
                inventorySlots[i].enabled = true;

                if (itemIcon != null)
                    inventorySlots[i].texture = itemIcon;

                // Automatisch selecteren als eerste item
                if (activeSlot == -1)
                    SelectSlot(i);

                break;
            }
        }
    }
}
