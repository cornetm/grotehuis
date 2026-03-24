using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class CinemachineCrashShake : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CinemachineCamera cmCamera;

    [Header("Default Values")]
    [SerializeField] private float defaultAmplitude = 1f;
    [SerializeField] private float defaultFrequency = 1f;

    [Header("Crash Values")]
    [SerializeField] private float crashAmplitude = 6f;
    [SerializeField] private float crashFrequency = 6f;
    [SerializeField] private float crashDuration = 0.35f;

    [Header("Return")]
    [SerializeField] private float returnDuration = 0.6f;

    private CinemachineBasicMultiChannelPerlin noise;
    private Coroutine shakeRoutine;

    private void Awake()
    {
        if (cmCamera == null)
            cmCamera = GetComponent<CinemachineCamera>();

        if (cmCamera != null)
            noise = cmCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();

        if (noise != null)
        {
            noise.AmplitudeGain = defaultAmplitude;
            noise.FrequencyGain = defaultFrequency;
        }
    }

    public void TriggerCrashShake()
    {
        if (noise == null)
        {
            Debug.LogWarning("No CinemachineBasicMultiChannelPerlin found on this CinemachineCamera.");
            return;
        }

        if (shakeRoutine != null)
            StopCoroutine(shakeRoutine);

        shakeRoutine = StartCoroutine(CrashShakeRoutine());
    }

    public void SetShakeInstant(float amplitude, float frequency)
    {
        if (noise == null) return;

        noise.AmplitudeGain = amplitude;
        noise.FrequencyGain = frequency;
    }

    public void ResetShake()
    {
        if (noise == null) return;

        if (shakeRoutine != null)
            StopCoroutine(shakeRoutine);

        noise.AmplitudeGain = defaultAmplitude;
        noise.FrequencyGain = defaultFrequency;
    }

    private IEnumerator CrashShakeRoutine()
    {
        noise.AmplitudeGain = crashAmplitude;
        noise.FrequencyGain = crashFrequency;

        yield return new WaitForSeconds(crashDuration);

        float startAmp = noise.AmplitudeGain;
        float startFreq = noise.FrequencyGain;
        float timer = 0f;

        while (timer < returnDuration)
        {
            timer += Time.deltaTime;
            float t = timer / returnDuration;

            noise.AmplitudeGain = Mathf.Lerp(startAmp, defaultAmplitude, t);
            noise.FrequencyGain = Mathf.Lerp(startFreq, defaultFrequency, t);

            yield return null;
        }

        noise.AmplitudeGain = defaultAmplitude;
        noise.FrequencyGain = defaultFrequency;
    }
}