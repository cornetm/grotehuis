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

    [Header("Spawner")]
    public ItemSpawner itemSpawner;

    private List<RawImage> slots = new List<RawImage>();
    private List<InventorySlotItem> slotComponents = new List<InventorySlotItem>();

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
        // Slot select (1-9)
        for (int i = 0; i < slots.Count && i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i)) ToggleSlot(i);
        }

        // DROP F
        if (Input.GetKeyDown(KeyCode.F) && activeSlot >= 0)
            DropSlotItem(slotComponents[activeSlot]);

        // THROW Q START
        if (Input.GetKeyDown(KeyCode.Q) && activeSlot >= 0)
        {
            chargingThrow = true;
            throwCharge = 0f;
            if (throwPowerSlider != null)
                throwPowerSlider.gameObject.SetActive(true);
        }

        // CHARGING
        if (chargingThrow)
        {
            throwCharge += Time.deltaTime * chargeSpeed;
            throwCharge = Mathf.Clamp01(throwCharge);
            if (throwPowerSlider != null)
                throwPowerSlider.value = throwCharge;
        }

        // RELEASE THROW
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
    }

    // ================= CHECK INVENTORY =================
    public bool IsFull() => slots.Count >= maxItems;

    public bool HasEquippedItem()
    {
        foreach (var slot in slotComponents)
            if (slot.isEquipped) return true;
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

                Debug.Log("Equipped item replaced and old item dropped.");
                break;
            }
        }
    }

    // ================= DROP =================
    public void DropSlotItem(InventorySlotItem slotItem)
    {
        int index = slotComponents.IndexOf(slotItem);
        if (index < 0) return;

        Vector3 spawnPos = playerTransform.position + playerTransform.forward * dropDistance;
        itemSpawner.SpawnDroppedItem(slotItem.prefab, spawnPos);

        Debug.Log($"Dropped {slotItem.prefab.name}");
        RemoveSlot(index);
    }

    // ================= THROW =================
    public void ThrowSlotItem(InventorySlotItem slotItem, float force)
    {
        int index = slotComponents.IndexOf(slotItem);
        if (index < 0) return;

        Vector3 spawnPos = playerTransform.position + playerTransform.forward * dropDistance;
        GameObject obj = itemSpawner.SpawnDroppedItem(slotItem.prefab, spawnPos);

        // Richting van de camera
        Camera cam = Camera.main;
        if (cam != null)
        {
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 throwDir = cam.transform.forward.normalized;
                rb.AddForce(throwDir * force, ForceMode.Impulse);
            }
        }

        Debug.Log($"Threw {slotItem.prefab.name} with force {force} towards camera direction");
        RemoveSlot(index);
    }

    // ================= REMOVE SLOT =================
    void RemoveSlot(int index)
    {
        Destroy(slots[index].gameObject);
        slots.RemoveAt(index);
        slotComponents.RemoveAt(index);
        activeSlot = -1;
        RepositionSlots();
    }

    // ================= SLOT SELECT =================
    void ToggleSlot(int index)
    {
        if (index >= slots.Count) return;

        for (int i = 0; i < slotComponents.Count; i++)
        {
            bool selected = i == index;
            slots[i].transform.localScale = selected ? Vector3.one * selectedScale : Vector3.one * normalScale;
            if (selected) slotComponents[i].Equip();
            else slotComponents[i].Unequip();
        }

        activeSlot = index;
    }

    // ================= POSITION =================
    void RepositionSlots()
    {
        float startX = -(slots.Count - 1) * spacing * 0.5f;
        for (int i = 0; i < slots.Count; i++)
        {
            slots[i].rectTransform.anchoredPosition = new Vector2(startX + i * spacing, yOffset);
            slots[i].transform.localScale = Vector3.one * normalScale;
        }
    }
}
