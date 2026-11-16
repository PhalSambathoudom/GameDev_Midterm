
using System.Collections;
using UnityEngine;

public class DoorManager : MonoBehaviour
{
    [Header("Three Exit Doors")]
    [SerializeField] private ExitDoor[] doors;

    [Header("Door Settings")]
    [SerializeField] private float doorInterval = 900f;

    private ExitDoor currentOpenDoor;

    private void Start()
    {
        // Make sure exactly three doors are assigned.
        if (doors == null || doors.Length != 3)
        {
            Debug.LogError(
                "DoorManager needs exactly 3 doors!",
                this
            );
            enabled = false;
            return;
        }

        foreach (ExitDoor door in doors)
        {
            if (door == null)
            {
                Debug.LogError(
                    "A door reference is missing!",
                    this
                );
                enabled = false;
                return;
            }

            // Hide all doors first.
            door.CloseDoor();
        }

        // Start the door rotation.
        StartCoroutine(DoorCycle());
    }

    private IEnumerator DoorCycle()
    {
        // Open one random door immediately.
        OpenRandomDoor();

        while (true)
        {
            // Keep this door available for 15 minutes.
            yield return new WaitForSeconds(doorInterval);

            // Close the previous door.
            if (currentOpenDoor != null)
            {
                currentOpenDoor.CloseDoor();
            }

            // Select and open another random door.
            OpenRandomDoor();
        }
    }

    private void OpenRandomDoor()
    {
        int randomIndex;

        // Avoid selecting the same door twice in a row.
        do
        {
            randomIndex = Random.Range(0, doors.Length);
        }
        while (doors[randomIndex] == currentOpenDoor);

        currentOpenDoor = doors[randomIndex];

        currentOpenDoor.OpenDoor();

        Debug.Log(
            "New exit door appeared: " + currentOpenDoor.name
        );
    }
}
