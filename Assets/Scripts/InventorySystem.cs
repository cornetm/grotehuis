using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    // Lists
    private List<RawImage> slots = new List<RawImage>();
    private List<Item> slotItems = new List<Item>();
    private List<string> slotPrefabNames = new List<string>();
    private List<TextMeshProUGUI> slotTexts = new List<TextMeshProUGUI>();

    private int activeSlot = -1;

    void Update()
    {
        for (int i = 0; i < slots.Count && i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                ToggleSlot(i);
            }
        }
    }

    // ================= ADD ITEM =================
    public void AddItem(string prefabName, Texture icon = null)
    {
        if (slots.Count >= maxItems)
            return;

        // Instantiate slot
        RawImage newSlot = Instantiate(slotPrefab, inventoryParent);
        newSlot.gameObject.SetActive(true);

        if (icon != null)
            newSlot.texture = icon;

        // Item component
        Item item = newSlot.gameObject.AddComponent<Item>();
        item.item = false;

        // TMP text in prefab zoeken
        TextMeshProUGUI tmpText = newSlot.GetComponentInChildren<TextMeshProUGUI>();
        if (tmpText != null)
            tmpText.gameObject.SetActive(false);

        // Add to lists
        slots.Add(newSlot);
        slotItems.Add(item);
        slotPrefabNames.Add(prefabName);
        slotTexts.Add(tmpText);

        RepositionSlots();
    }

    // ================= TOGGLE SLOT =================
    private void ToggleSlot(int index)
    {
        if (index >= slots.Count) return;

        // Klik op hetzelfde slot = UNEQUIP
        if (activeSlot == index)
        {
            UnequipAll();
            return;
        }

        // Equip nieuwe slot
        for (int i = 0; i < slots.Count; i++)
        {
            bool selected = (i == index);

            slots[i].transform.localScale =
                selected ? Vector3.one * selectedScale : Vector3.one * normalScale;

            slotItems[i].item = selected;

            // TMP text aan/uit
            if (slotTexts[i] != null)
                slotTexts[i].gameObject.SetActive(selected);

            if (selected && slotTexts[i] != null)
                slotTexts[i].text = slotPrefabNames[i];
        }

        activeSlot = index;
    }

    // ================= UNEQUIP =================
    private void UnequipAll()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            slots[i].transform.localScale = Vector3.one * normalScale;
            slotItems[i].item = false;

            if (slotTexts[i] != null)
                slotTexts[i].gameObject.SetActive(false);
        }

        activeSlot = -1;
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
