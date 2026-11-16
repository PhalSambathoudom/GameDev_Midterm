
using UnityEngine;

public class Flag : MonoBehaviour, IInteractable
{
    private bool claimed;

    private Vector3 originalPosition;
    private Quaternion originalRotation;

    public bool CanBeCollected => !claimed;

    public string Prompt =>
        claimed ? "" : "Press E to collect the flag";

    private void Awake()
    {
        // Remember the original flag position.
        originalPosition = transform.position;
        originalRotation = transform.rotation;
    }


    // Called when the player presses E.
    public void Interact(PlayerInteract player)
    {
        if (claimed || player == null)
            return;

        PlayerHealth health = player.Health;

        if (health == null)
        {
            Debug.LogError("PlayerHealth is missing from Player!");
            return;
        }

        // Set the player's checkpoint at this flag.
        health.ClaimFlag(transform.position, transform.rotation);

        // Hide the flag after collection.
        Collect();
    }


    // Hide the flag after collection.
    public void Collect()
    {
        if (claimed)
            return;

        claimed = true;

        gameObject.SetActive(false);

        Debug.Log("Flag collected!");
    }

    // Return the flag to its original position.
    public void ReturnToOriginalPosition()
    {
        transform.SetPositionAndRotation(
            originalPosition,
            originalRotation
        );

        claimed = false;

        gameObject.SetActive(true);

        Debug.Log("Flag returned to original position!");
    }
}
