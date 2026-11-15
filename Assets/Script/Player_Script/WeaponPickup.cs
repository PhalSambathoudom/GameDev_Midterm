using UnityEngine;

public class WeaponPickup : MonoBehaviour, IInteractable
{
    [Header("Stats")]
    public string weaponName = "Weapon";
    public float stunDuration = 3f;
    public float range = 2.5f;
    public float cooldown = 0.8f;

    [Header("How it looks in the player's hand (tweak in Play mode)")]
    public Vector3 heldLocalPosition;
    public Vector3 heldLocalEuler;

    public string Prompt => $"Press E to pick up {weaponName}";

    public void Interact(PlayerInteract player) => player.PickUp(this);

    public void OnEquip(Transform hand)
    {
        SetPhysics(false);
        transform.SetParent(hand);
        transform.localPosition = heldLocalPosition;
        transform.localRotation = Quaternion.Euler(heldLocalEuler);
    }

    public void OnDrop(Vector3 position)
    {
        transform.SetParent(null);
        transform.position = position;
        SetPhysics(true);
    }

    private void SetPhysics(bool on)
    {
        foreach (var c in GetComponentsInChildren<Collider>()) c.enabled = on;
        var rb = GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = !on;
    }
}