using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventorySystem : MonoBehaviour
{
    [Header("UI")]
    public RectTransform inventoryParent;
    public RawImage slotPrefab;
    public Slider throwPowerSlider;

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

    [Header("Throw Settings")]
    public float chargeSpeed = 1.5f;
    public float maxThrowForce = 20f;
    public Camera playerCamera; // 📌 Voeg hier je Camera toe in de inspector

    [Header("Spawner")]
    public ItemSpawner itemSpawner;

    private List<RawImage> slots = new List<RawImage>();
    public List<InventorySlotItem> slotComponents = new List<InventorySlotItem>();

    private int activeSlot = -1;
    private bool chargingThrow = false;
    private float throwCharge = 0f;

    void Start()
    {
        if (throwPowerSlider != null)
        {
            throwPowerSlider.minValue = 0;
            throwPowerSlider.maxValue = 1;
            throwPowerSlider.value = 0;
            throwPowerSlider.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // ================= SLOT SELECT (1-9) =================
        for (int i = 0; i < slots.Count && i < 9; i++)
            if (Input.GetKeyDown(KeyCode.Alpha1 + i)) ToggleSlot(i);

        // ================= DROP ITEM (F) =================
        if (Input.GetKeyDown(KeyCode.F) && activeSlot >= 0)
            DropSlotItem(slotComponents[activeSlot]);

        // ================= THROW ITEM (Q) =================
        if (Input.GetKeyDown(KeyCode.Q) && activeSlot >= 0)
        {
            chargingThrow = true;
            throwCharge = 0f;
            if (throwPowerSlider != null)
                throwPowerSlider.gameObject.SetActive(true);
        }

        if (chargingThrow)
        {
            throwCharge += Time.deltaTime * chargeSpeed;
            throwCharge = Mathf.Clamp01(throwCharge);
            if (throwPowerSlider != null)
                throwPowerSlider.value = throwCharge;
        }

        if (Input.GetKeyUp(KeyCode.Q) && chargingThrow && activeSlot >= 0)
        {
            chargingThrow = false;
            float force = throwCharge * maxThrowForce;
            ThrowSlotItem(slotComponents[activeSlot], force);

            throwCharge = 0f;
            if (throwPowerSlider != null)
            {
                throwPowerSlider.value = 0;
                throwPowerSlider.gameObject.SetActive(false);
            }
        }
    }

    // ================= PUBLIC FUNCTIONS =================
    public bool IsFull() => slots.Count >= maxItems;

    public bool HasEquippedItem()
    {
        foreach (var slot in slotComponents)
            if (slot.isEquipped) return true;
        return false;
    }

    public void ReplaceEquippedItem(GameObject newPrefab, Texture newIcon)
    {
        for (int i = 0; i < slotComponents.Count; i++)
        {
            if (slotComponents[i].isEquipped)
            {
                int slotIndex = i;
                slotComponents[i].Unequip();
                DropSlotItem(slotComponents[i]);
                AddItem(newPrefab, newIcon, slotIndex);
                ToggleSlot(slotIndex);
                break;
            }
        }
    }

    public InventorySlotItem CurrentEquippedItem()
    {
        if (activeSlot < 0 || activeSlot >= slotComponents.Count)
            return null;
        return slotComponents[activeSlot];
    }

    // ================= ADD ITEM =================
    public void AddItem(GameObject prefab, Texture icon = null, int index = -1)
    {
        if (index < 0 && slots.Count >= maxItems) return;

        RawImage newSlot = Instantiate(slotPrefab, inventoryParent);
        newSlot.gameObject.SetActive(true);

        if (icon != null) newSlot.texture = icon;

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

        if (slots.Count == 1)
            ToggleSlot(0); // auto equip first item
    }

    // ================= DROP ITEM =================
    public void DropSlotItem(InventorySlotItem slotItem)
    {
        int index = slotComponents.IndexOf(slotItem);
        if (index < 0) return;

        if (slotItem.prefabRef != null)
        {
            ItemUse itemUse = GameObject.FindObjectOfType<ItemUse>();
            if (itemUse != null)
                itemUse.SetState(slotItem.prefabRef.category, slotItem.GetEnumFromCategory(slotItem.prefabRef), false);
        }

        Vector3 spawnPos = playerCamera.transform.position + playerCamera.transform.forward * dropDistance;
        itemSpawner.SpawnDroppedItem(slotItem.prefab, spawnPos, false);

        RemoveSlot(index);
    }

    // ================= THROW ITEM =================
    public void ThrowSlotItem(InventorySlotItem slotItem, float force)
    {
        int index = slotComponents.IndexOf(slotItem);
        if (index < 0) return;

        if (slotItem.prefabRef != null)
        {
            ItemUse itemUse = GameObject.FindObjectOfType<ItemUse>();
            if (itemUse != null)
                itemUse.SetState(slotItem.prefabRef.category, slotItem.GetEnumFromCategory(slotItem.prefabRef), false);
        }

        Vector3 spawnPos = playerCamera.transform.position + playerCamera.transform.forward * dropDistance;

        GameObject obj = itemSpawner.SpawnDroppedItem(slotItem.prefab, spawnPos, false);

        // ==== PER ITEM THROW ROTATION ====
        Vector3 offset = slotItem.prefabRef != null ? slotItem.prefabRef.throwRotationOffset : Vector3.zero;
        obj.transform.rotation = Quaternion.LookRotation(playerCamera.transform.forward) * Quaternion.Euler(offset);

        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 throwDir = playerCamera.transform.forward.normalized;
            rb.AddForce(throwDir * force, ForceMode.Impulse);
        }

        RemoveSlot(index);
    }

    public void RemoveSlot(int index)
    {
        Destroy(slots[index].gameObject);
        slots.RemoveAt(index);
        slotComponents.RemoveAt(index);
        activeSlot = -1;
        RepositionSlots();
    }

    // ================= SLOT SELECT / UNEQUIP =================
    void ToggleSlot(int index)
    {
        if (index >= slots.Count) return;

        if (activeSlot == index)
        {
            UnequipAll();
            return;
        }

        for (int i = 0; i < slotComponents.Count; i++)
        {
            bool selected = i == index;
            slots[i].transform.localScale = selected ? Vector3.one * selectedScale : Vector3.one * normalScale;

            if (selected) slotComponents[i].Equip();
            else slotComponents[i].Unequip();
        }

        activeSlot = index;
    }

    void UnequipAll()
    {
        for (int i = 0; i < slotComponents.Count; i++)
        {
            slots[i].transform.localScale = Vector3.one * normalScale;
            slotComponents[i].Unequip();
        }
        activeSlot = -1;
    }

    // ================= POSITION =================
    void RepositionSlots()
    {
        float startX = -(slots.Count - 1) * spacing * 0.5f;
        for (int i = 0; i < slots.Count; i++)
        {
            slots[i].rectTransform.anchoredPosition = new Vector2(startX + i * spacing, yOffset);
            slots[i].transform.localScale = slotComponents[i].isEquipped ? Vector3.one * selectedScale : Vector3.one * normalScale;
        }
    }
}