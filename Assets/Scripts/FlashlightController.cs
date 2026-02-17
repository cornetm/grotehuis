using System.Collections.Generic;
using UnityEngine;

public class ItemUse : MonoBehaviour
{
    [Header("Weapon Objects")]
    public GameObject butcherKnifeObject;
    public GameObject kitchenKnifeObject;
    public GameObject butterKnifeObject;

    [Header("Temporary Objects")]
    public GameObject flashlightObject;
    public GameObject luciferObject;
    public GameObject candleObject;

    [Header("Limited Objects")]
    public GameObject pillsObject;

    // Track hoe veel van elk type equipped zijn
    private Dictionary<string, int> equippedCount = new Dictionary<string, int>();

    void Start()
    {
        SetAllOff();
    }

    // ================= EQUIP ITEM =================
    public void EquipItem(PrefabReferenceAuto.ItemCategory category, object typeEnum)
    {
        string key = GetKey(category, typeEnum);
        if (!equippedCount.ContainsKey(key)) equippedCount[key] = 0;
        equippedCount[key]++;

        SetState(category, typeEnum, true);
    }

    // ================= UNEQUIP ITEM =================
    public void UnequipItem(PrefabReferenceAuto.ItemCategory category, object typeEnum)
    {
        string key = GetKey(category, typeEnum);
        if (!equippedCount.ContainsKey(key)) return;

        equippedCount[key]--;
        if (equippedCount[key] <= 0)
        {
            equippedCount[key] = 0;
            SetState(category, typeEnum, false);
        }
    }

    // ================= HULPFUNCTIE VOOR KEY =================
    private string GetKey(PrefabReferenceAuto.ItemCategory category, object typeEnum)
    {
        return category.ToString() + "_" + typeEnum.ToString();
    }

    // ================= SET STATE =================
    public void SetState(PrefabReferenceAuto.ItemCategory category, object typeEnum, bool state)
    {
        switch (category)
        {
            case PrefabReferenceAuto.ItemCategory.Weapons:
                PrefabReferenceAuto.WeaponType weapon = (PrefabReferenceAuto.WeaponType)typeEnum;
                switch (weapon)
                {
                    case PrefabReferenceAuto.WeaponType.ButcherKnife:
                        if (butcherKnifeObject != null) butcherKnifeObject.SetActive(state);
                        break;
                    case PrefabReferenceAuto.WeaponType.KitchenKnife:
                        if (kitchenKnifeObject != null) kitchenKnifeObject.SetActive(state);
                        break;
                    case PrefabReferenceAuto.WeaponType.ButterKnife:
                        if (butterKnifeObject != null) butterKnifeObject.SetActive(state);
                        break;
                }
                break;

            case PrefabReferenceAuto.ItemCategory.Temporary:
                PrefabReferenceAuto.TemporaryType temp = (PrefabReferenceAuto.TemporaryType)typeEnum;
                switch (temp)
                {
                    case PrefabReferenceAuto.TemporaryType.Flashlight:
                        if (flashlightObject != null) flashlightObject.SetActive(state);
                        break;
                    case PrefabReferenceAuto.TemporaryType.Lucifer:
                        if (luciferObject != null) luciferObject.SetActive(state);
                        break;
                    case PrefabReferenceAuto.TemporaryType.Candle:
                        if (candleObject != null) candleObject.SetActive(state);
                        break;
                }
                break;

            case PrefabReferenceAuto.ItemCategory.Limited:
                PrefabReferenceAuto.LimitedType limited = (PrefabReferenceAuto.LimitedType)typeEnum;
                switch (limited)
                {
                    case PrefabReferenceAuto.LimitedType.Pills:
                        if (pillsObject != null) pillsObject.SetActive(state);
                        break;
                }
                break;
        }
    }

    // ================= ZET ALLES UIT =================
    public void SetAllOff()
    {
        if (butcherKnifeObject != null) butcherKnifeObject.SetActive(false);
        if (kitchenKnifeObject != null) kitchenKnifeObject.SetActive(false);
        if (butterKnifeObject != null) butterKnifeObject.SetActive(false);

        if (flashlightObject != null) flashlightObject.SetActive(false);
        if (luciferObject != null) luciferObject.SetActive(false);
        if (candleObject != null) candleObject.SetActive(false);

        if (pillsObject != null) pillsObject.SetActive(false);

        equippedCount.Clear();
    }
}
