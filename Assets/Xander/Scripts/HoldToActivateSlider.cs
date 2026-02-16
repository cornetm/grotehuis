using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[RequireComponent(typeof(Slider))]
public class HoldToActivateSlider : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private KeyCode holdKey = KeyCode.E;
    [SerializeField] private float holdDuration = 2f;

    [Header("Behaviour")]
    [SerializeField] private bool resetOnRelease = true;
    [SerializeField] private bool disableAfterActivate = true;

    [Header("Event")]
    public UnityEvent onActivated;

    private Slider slider;
    private float holdTimer;
    private bool activated;

    void Awake()
    {
        slider = GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0f;
    }

    void OnEnable()
    {
        // reset wanneer UI opnieuw zichtbaar wordt
        ResetState();
    }

    void Update()
    {
        // Slider/UI moet actief en interactable zijn
        if (!slider.IsActive() || !slider.interactable || activated)
            return;

        if (Input.GetKey(holdKey))
        {
            holdTimer += Time.deltaTime;
            slider.value = Mathf.Clamp01(holdTimer / holdDuration);

            if (holdTimer >= holdDuration)
            {
                Activate();
            }
        }
        else if (resetOnRelease)
        {
            ResetState();
        }
    }

    private void Activate()
    {
        activated = true;
        slider.value = 1f;

        onActivated?.Invoke();

        if (disableAfterActivate)
        {
            slider.interactable = false;
        }
    }

    private void ResetState()
    {
        holdTimer = 0f;
        activated = false;
        slider.value = 0f;
    }
}
