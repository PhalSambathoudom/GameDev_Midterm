using UnityEngine;

// Put on the flag prefab at the core of the maze (needs a Collider).
public class Flag : MonoBehaviour, IInteractable
{
    private bool claimed;
    public string Prompt => claimed ? "" : "Press E to claim the flag";

    public void Interact(PlayerInteract player)
    {
        if (claimed) return;
        claimed = true;
        player.Health.ClaimFlag(transform.position, player.transform.rotation);
        // the flag stays in the maze as the checkpoint marker
    }
}