using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventorySystem : MonoBehaviour
{
    [Header("UI")]
    public RectTransform inventoryParent;
    public RawImage slotPrefab;

    [Header("Layout")]
    public int maxItems = 5;
    public float spacing = 140f;
    public float yOffset = -200f;

    [Header("Scale")]
    public float normalScale = 1f;
    public float selectedScale = 1.4f;

    [Header("Player & Drop Settings")]
    public Transform playerTransform;
    public float dropDistance = 2f;

    [Header("Spawner")]
    public ItemSpawner itemSpawner;

    private List<RawImage> slots = new List<RawImage>();
    private List<InventorySlotItem> slotComponents = new List<InventorySlotItem>();

    private int activeSlot = -1;

    void Update()
    {
        // Slot select (1-9)
        for (int i = 0; i < slots.Count && i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                ToggleSlot(i);
        }

        // Drop item
        if (Input.GetKeyDown(KeyCode.F) && activeSlot >= 0)
        {
            slotComponents[activeSlot].Drop();
        }
    }

    // ================= ADD ITEM =================
    public void AddItem(GameObject prefab, Texture icon = null, int index = -1)
    {
        if (index < 0 && slots.Count >= maxItems) return;

        RawImage newSlot = Instantiate(slotPrefab, inventoryParent);
        newSlot.gameObject.SetActive(true);

        if (icon != null)
            newSlot.texture = icon;

        InventorySlotItem slotComp = newSlot.gameObject.AddComponent<InventorySlotItem>();
        slotComp.Initialize(prefab, this);

        if (index < 0 || index >= slots.Count)
        {
            slots.Add(newSlot);
            slotComponents.Add(slotComp);
        }
        else
        {
            slots.Insert(index, newSlot);
            slotComponents.Insert(index, slotComp);
        }

        RepositionSlots();
    }

    // ================= CHECK FULL =================
    public bool IsFull()
    {
        return slots.Count >= maxItems;
    }

    // ================= CHECK EQUIPPED =================
    public bool HasEquippedItem()
    {
        foreach (var slot in slotComponents)
        {
            if (slot.isEquipped)
                return true;
        }
        return false;
    }

    // ================= REPLACE EQUIPPED =================
    public void ReplaceEquippedItem(GameObject newPrefab, Texture newIcon)
    {
        for (int i = 0; i < slotComponents.Count; i++)
        {
            if (slotComponents[i].isEquipped)
            {
                InventorySlotItem oldSlot = slotComponents[i];
                int slotIndex = i;

                // Drop oude item in de wereld
                DropSlotItem(oldSlot);

                // Voeg nieuwe item toe op dezelfde plek
                AddItem(newPrefab, newIcon, slotIndex);

                // Equip nieuwe item
                ToggleSlot(slotIndex);

                break;
            }
        }
    }

    // ================= TOGGLE SLOT =================
    private void ToggleSlot(int index)
    {
        if (index >= slots.Count) return;

        if (activeSlot == index)
        {
            UnequipAll();
            return;
        }

        for (int i = 0; i < slotComponents.Count; i++)
        {
            bool selected = (i == index);
            slots[i].transform.localScale =
                selected ? Vector3.one * selectedScale : Vector3.one * normalScale;

            if (selected) slotComponents[i].Equip();
            else slotComponents[i].Unequip();
        }

        activeSlot = index;
    }

    // ================= UNEQUIP =================
    private void UnequipAll()
    {
        for (int i = 0; i < slotComponents.Count; i++)
        {
            slots[i].transform.localScale = Vector3.one * normalScale;
            slotComponents[i].Unequip();
        }

        activeSlot = -1;
    }

    // ================= DROP SLOT ITEM =================
    public void DropSlotItem(InventorySlotItem slotItem)
    {
        int index = slotComponents.IndexOf(slotItem);
        if (index < 0) return;

        if (slotItem.prefab != null && playerTransform != null && itemSpawner != null)
        {
            Vector3 spawnPos = playerTransform.position + playerTransform.forward * dropDistance;
            itemSpawner.SpawnDroppedItem(slotItem.prefab, spawnPos);
            Debug.Log($"Dropped {slotItem.prefab.name}.");
        }

        Destroy(slots[index].gameObject);
        slots.RemoveAt(index);
        slotComponents.RemoveAt(index);

        activeSlot = -1;
        RepositionSlots();
    }

    // ================= POSITION =================
    private void RepositionSlots()
    {
        int count = slots.Count;
        float startX = -(count - 1) * spacing * 0.5f;

        for (int i = 0; i < count; i++)
        {
            float x = startX + i * spacing;
            slots[i].rectTransform.anchoredPosition =
                new Vector2(x, yOffset);
            slots[i].transform.localScale = Vector3.one * normalScale;
        }
    }
}
