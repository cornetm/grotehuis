using UnityEngine;
using System.Collections.Generic;

public class ItemUse : MonoBehaviour
{
    [Header("Weapon Objects")]
    public GameObject butcherKnifeObject;
    public GameObject kitchenKnifeObject;
    public GameObject butterKnifeObject;

    [Header("Temporary Objects")]
    public GameObject flashlightObject;
    public Light flashlightLight;
    public GameObject luciferObject;
    public GameObject candleObject;

    [Header("Limited Objects")]
    public GameObject pillsObject;

    // Alleen nog activation state voor toggle items (zoals flashlight)
    private Dictionary<string, bool> activatedState = new Dictionary<string, bool>();

    void Start()
    {
        SetAllOff();
    }

    // ================= EQUIP =================
    public void EquipItem(PrefabReferenceAuto.ItemCategory category, object typeEnum)
    {
        SetState(category, typeEnum, true);

        string key = GetKey(category, typeEnum);

        if (!activatedState.ContainsKey(key))
            activatedState[key] = false;

        // Flashlight standaard uit bij equip
        if (category == PrefabReferenceAuto.ItemCategory.Temporary &&
            typeEnum.Equals(PrefabReferenceAuto.TemporaryType.Flashlight))
        {
            if (flashlightLight != null)
                flashlightLight.enabled = false;
        }
    }

    // ================= UNEQUIP =================
    public void UnequipItem(PrefabReferenceAuto.ItemCategory category, object typeEnum)
    {
        SetState(category, typeEnum, false);

        string key = GetKey(category, typeEnum);

        if (activatedState.ContainsKey(key))
            activatedState[key] = false;

        if (category == PrefabReferenceAuto.ItemCategory.Temporary &&
            typeEnum.Equals(PrefabReferenceAuto.TemporaryType.Flashlight))
        {
            if (flashlightLight != null)
                flashlightLight.enabled = false;
        }
    }

    // ================= TOGGLE (bijv flashlight) =================
    public void ToggleActivated(PrefabReferenceAuto.ItemCategory category, object typeEnum)
    {
        string key = GetKey(category, typeEnum);

        if (!activatedState.ContainsKey(key))
            activatedState[key] = false;

        activatedState[key] = !activatedState[key];

        if (category == PrefabReferenceAuto.ItemCategory.Temporary &&
            typeEnum.Equals(PrefabReferenceAuto.TemporaryType.Flashlight))
        {
            if (flashlightLight != null)
                flashlightLight.enabled = activatedState[key];
        }
    }

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

    // ================= RESET =================
    public void SetAllOff()
    {
        if (butcherKnifeObject != null) butcherKnifeObject.SetActive(false);
        if (kitchenKnifeObject != null) kitchenKnifeObject.SetActive(false);
        if (butterKnifeObject != null) butterKnifeObject.SetActive(false);

        if (flashlightObject != null) flashlightObject.SetActive(false);
        if (luciferObject != null) luciferObject.SetActive(false);
        if (candleObject != null) candleObject.SetActive(false);

        if (pillsObject != null) pillsObject.SetActive(false);

        activatedState.Clear();

        if (flashlightLight != null)
            flashlightLight.enabled = false;
    }
}
