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
    public ItemSpawner itemSpawner; // Koppel hier je ItemSpawner

    // Lists
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
    public void AddItem(GameObject prefab, Texture icon = null)
    {
        if (slots.Count >= maxItems) return;

        RawImage newSlot = Instantiate(slotPrefab, inventoryParent);
        newSlot.gameObject.SetActive(true);

        if (icon != null)
            newSlot.texture = icon;

        // Voeg InventorySlotItem toe en initialiseer met prefab reference
        InventorySlotItem slotComp = newSlot.gameObject.AddComponent<InventorySlotItem>();
        slotComp.Initialize(prefab, this);

        slots.Add(newSlot);
        slotComponents.Add(slotComp);

        RepositionSlots();
    }

    // ================= TOGGLE SLOT =================
    private void ToggleSlot(int index)
    {
        if (index >= slots.Count) return;

        // Klik op hetzelfde slot = unequip
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
            // Spawn via ItemSpawner
            Vector3 spawnPos = playerTransform.position + playerTransform.forward * dropDistance;
            itemSpawner.SpawnDroppedItem(slotItem.prefab, spawnPos);
        }

        // Verwijder slot UI
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
