
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Player))]
[RequireComponent(typeof(PlayerHealth))]
public class PlayerFlagCheckpoint : MonoBehaviour
{
    [Header("Respawn Settings")]
    [SerializeField] private int maximumCheckpointRespawns = 5;

    private Player player;
    private PlayerHealth health;

    private Flag ownedFlag;

    private Vector3 checkpointPosition;
    private Quaternion checkpointRotation;

    // Has the player collected a flag?
    private bool hasFlag;

    // Is the player currently carrying the flag?
    private bool isCarryingFlag;

    // Has the player saved a checkpoint?
    private bool hasCheckpoint;

    // Number of checkpoint respawns already used.
    private int checkpointRespawnsUsed;

    public bool HasFlag => hasFlag;
    public bool HasCheckpoint => hasCheckpoint;

    public int CheckpointRespawnsUsed => checkpointRespawnsUsed;

    private void Awake()
    {
        player = GetComponent<Player>();
        health = GetComponent<PlayerHealth>();
    }

    private void Update()
    {
        if (health == null || health.IsDead || !isCarryingFlag)
            return;

        // Press G to save the checkpoint.
        if (Keyboard.current != null &&
            Keyboard.current.gKey.wasPressedThisFrame)
        {
            SetCheckpointHere();
        }
    }

    // Called by Flag.cs when the player presses E.
    public void TryCollectFlag(Flag flag)
    {
        if (flag == null || isCarryingFlag || !flag.CanBeCollected)
            return;

        ownedFlag = flag;

        // Hide the flag after collection.
        ownedFlag.Collect();

        hasFlag = true;
        isCarryingFlag = true;

        Debug.Log("Flag collected!");
        Debug.Log("Press G to save your checkpoint.");
    }

    // Save the player's current position.
    public void SetCheckpointHere()
    {
        if (!isCarryingFlag)
        {
            Debug.Log("Collect the flag first!");
            return;
        }

        checkpointPosition = transform.position;
        checkpointRotation = transform.rotation;

        hasCheckpoint = true;

        // Reset respawn count for the new checkpoint.
        checkpointRespawnsUsed = 0;

        // Stop carrying the flag.
        isCarryingFlag = false;

        // Return the flag to its original position.
        if (ownedFlag != null)
        {
            ownedFlag.ReturnToOriginalPosition();
        }

        Debug.Log("Checkpoint saved at: " + checkpointPosition);
        Debug.Log("Flag returned! You can collect it again.");
    }

    // Called by PlayerHealth when the player dies.
    public void RespawnAfterDeath(Transform normalSpawnPoint)
    {
        // Respawn at the saved checkpoint if available.
        if (hasFlag &&
            hasCheckpoint &&
            checkpointRespawnsUsed < maximumCheckpointRespawns)
        {
            checkpointRespawnsUsed++;

            player.TeleportTo(
                checkpointPosition,
                checkpointRotation
            );

            Debug.Log(
                "Checkpoint respawn: " +
                checkpointRespawnsUsed + "/" +
                maximumCheckpointRespawns
            );

            return;
        }

        // If the checkpoint respawn limit is reached,
        // reset checkpoint progress.
        if (hasFlag)
        {
            Debug.Log(
                "Checkpoint limit reached! Returning to normal spawn."
            );

            hasFlag = false;
            isCarryingFlag = false;
            hasCheckpoint = false;

            checkpointRespawnsUsed = 0;

            if (ownedFlag != null)
            {
                ownedFlag.ReturnToOriginalPosition();
                ownedFlag = null;
            }
        }

        // Return to the normal starting position.
        if (normalSpawnPoint != null)
        {
            player.TeleportTo(
                normalSpawnPoint.position,
                normalSpawnPoint.rotation
            );
        }
        else
        {
            Debug.LogError("Normal spawn point is missing!");
        }
    }
}
