using UnityEngine;

public class FlashlightController : MonoBehaviour
{
    public Light flashlightLight;

    bool isOn = false;

    void Start()
    {
        if (flashlightLight == null)
            flashlightLight = GetComponentInChildren<Light>();

        SetState(false);
    }

    public void Toggle()
    {
        isOn = !isOn;
        SetState(isOn);
    }

    // 🔹 Public zodat InventorySlotItem hem kan uitzetten
    public void SetState(bool state)
    {
        isOn = state;
        if (flashlightLight != null)
            flashlightLight.enabled = state;
    }
}
