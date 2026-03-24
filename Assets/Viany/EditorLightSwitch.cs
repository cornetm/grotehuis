using UnityEngine;

[ExecuteAlways]
public class LightSwitch : MonoBehaviour
{
    [SerializeField] private bool isOn = true;

    [SerializeField] private EditorLightToggle[] targets;

    [SerializeField] private Transform switchHandle;

    private readonly Vector3 onRotation = Vector3.zero;
    private readonly Vector3 offRotation = new Vector3(0f, 0f, 180f);

    private void OnValidate()
    {
        ApplyState();
    }

    private void Awake()
    {
        ApplyState();
    }

    private void ApplyState()
    {
        for (int i = 0; i < targets.Length; i++)
        {
            targets[i].SetState(isOn);
        }

        switchHandle.localEulerAngles = isOn ? onRotation : offRotation;
    }
}