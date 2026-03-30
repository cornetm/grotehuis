using System.Collections.Generic;
using UnityEngine;

public class StartTrigger : MonoBehaviour
{
    [Header("Objecten die verdwijnen")]
    public List<GameObject> objectsToDisable = new List<GameObject>();

    [Header("Objecten die verschijnen")]
    public List<GameObject> objectsToEnable = new List<GameObject>();

    [Header("Room Generator")]
    public RoomGenerator roomGenerator;

    [Header("Start Door")]
    public StartDoor startDoor; // 👉 DEUR HIER SLEPEN

    // 💡 NIEUW: light toevoegen
    [Header("Start Light")]
    public Light startLight;

    private bool triggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (triggered) return;

        // check of het de player is
        if (other.CompareTag("Player"))
        {
            triggered = true;

            // 🔴 uitschakelen
            foreach (GameObject obj in objectsToDisable)
            {
                if (obj != null)
                    obj.SetActive(false);
            }

            // 🟢 inschakelen
            foreach (GameObject obj in objectsToEnable)
            {
                if (obj != null)
                    obj.SetActive(true);
            }

            // 🚀 ROOM GENERATOR STARTEN
            if (roomGenerator != null)
            {
                roomGenerator.BeginGeneration();
            }

            // 🚪 DEUR SLUITEN
            if (startDoor != null)
            {
                startDoor.SendMessage("OnTriggerEnter", other, SendMessageOptions.DontRequireReceiver);
            }

            // 💡 LIGHT AANZETTEN
            if (startLight != null)
            {
                startLight.intensity = 0.1f;
            }
        }
    }
}