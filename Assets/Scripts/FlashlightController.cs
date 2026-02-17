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

    private bool isOn = false;

    void Start()
    {
        SetAllOff();
    }

    // Toggle voor elk type
    public void Toggle(PrefabReferenceAuto.ItemCategory category, object typeEnum)
    {
        isOn = !isOn;
        SetState(category, typeEnum, isOn);
    }

    // Zet specifiek object aan/uit
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

    // Alles uitzetten
    public void SetAllOff()
    {
        if (butcherKnifeObject != null) butcherKnifeObject.SetActive(false);
        if (kitchenKnifeObject != null) kitchenKnifeObject.SetActive(false);
        if (butterKnifeObject != null) butterKnifeObject.SetActive(false);

        if (flashlightObject != null) flashlightObject.SetActive(false);
        if (luciferObject != null) luciferObject.SetActive(false);
        if (candleObject != null) candleObject.SetActive(false);

        if (pillsObject != null) pillsObject.SetActive(false);

        isOn = false;
    }
}
