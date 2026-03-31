using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class TimelineToGameplaySwitcher : MonoBehaviour
{
    [Header("Enable These")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private PauseMenuToggle pauseMenuToggle;

    [Header("Disable These")]
    [SerializeField] private Animator animatorToDisable;
    [SerializeField] private PlayableDirector playableDirectorToDisable;
    [SerializeField] private SignalReceiver signalReceiverToDisable;

    public void SwitchToGameplay()
    {
        // Eerst deze uitzetten
        if (animatorToDisable != null)
            animatorToDisable.enabled = false;

        if (playableDirectorToDisable != null)
            playableDirectorToDisable.enabled = false;

        if (signalReceiverToDisable != null)
            signalReceiverToDisable.enabled = false;

        // Daarna deze aanzetten
        if (playerMovement != null)
            playerMovement.enabled = true;

        if (characterController != null)
            characterController.enabled = true;

        if (pauseMenuToggle != null)
            pauseMenuToggle.enabled = true;

        Debug.Log("Switched from timeline/cutscene to gameplay.");
    }
}