using UnityEngine;
using UnityEngine.UI;
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

    [Header("Flashlight Battery")]
    public Slider flashlightPowerSlider;   // NIEUW
    public float maxFlashlightPower = 60f; // NIEUW

    public GameObject luciferObject;
    public GameObject candleObject;

    [Header("Limited Objects")]
    public GameObject pillsObject;

    private Dictionary<string, bool> activatedState = new Dictionary<string, bool>();

    void Start()
    {
        SetAllOff();

        if (flashlightPowerSlider != null)
        {
            flashlightPowerSlider.maxValue = maxFlashlightPower;
            flashlightPowerSlider.value = maxFlashlightPower;
        }
    }

    public void EquipItem(PrefabReferenceAuto.ItemCategory category, object typeEnum)
    {
        SetState(category, typeEnum, true);

        string key = GetKey(category, typeEnum);

        if (!activatedState.ContainsKey(key))
            activatedState[key] = false;

        if (category == PrefabReferenceAuto.ItemCategory.Temporary &&
            typeEnum.Equals(PrefabReferenceAuto.TemporaryType.Flashlight))
        {
            if (flashlightLight != null)
                flashlightLight.enabled = false;
        }
    }

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
