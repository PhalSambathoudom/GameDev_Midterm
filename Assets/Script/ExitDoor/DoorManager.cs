using System.Collections;
using UnityEngine;

public class DoorManager : MonoBehaviour
{
    [Header("Exit Doors (1 or more)")]
    [SerializeField] private ExitDoor[] doors;

    [Header("Door Settings")]
    [SerializeField] private float doorInterval = 900f;

    private ExitDoor currentOpenDoor;

    private void Start()
    {
        if (doors == null || doors.Length == 0)
        {
            Debug.LogError("DoorManager needs at least 1 door!", this);
            enabled = false;
            return;
        }

        foreach (ExitDoor door in doors)
        {
            if (door == null)
            {
                Debug.LogError("A door reference is missing!", this);
                enabled = false;
                return;
            }

            door.CloseDoor();
        }

        StartCoroutine(DoorCycle());
    }

    private IEnumerator DoorCycle()
    {
        OpenRandomDoor();

        // Only one door: leave it open, there is nothing to rotate to.
        if (doors.Length == 1) yield break;

        while (true)
        {
            yield return new WaitForSeconds(doorInterval);

            if (currentOpenDoor != null)
                currentOpenDoor.CloseDoor();

            OpenRandomDoor();
        }
    }

    private void OpenRandomDoor()
    {
        int randomIndex = 0;

        // Avoid picking the same door twice in a row (only possible with 2+ doors).
        if (doors.Length > 1)
        {
            do
            {
                randomIndex = Random.Range(0, doors.Length);
            }
            while (doors[randomIndex] == currentOpenDoor);
        }

        currentOpenDoor = doors[randomIndex];
        currentOpenDoor.OpenDoor();

        Debug.Log("New exit door appeared: " + currentOpenDoor.name);
    }
}