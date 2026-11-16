
using UnityEngine;

public class ExitDoor : MonoBehaviour, IInteractable
{
    [Header("Door References")]
    [SerializeField] private GameObject doorVisual;
    [SerializeField] private Collider interactionCollider;

    private bool isOpen;
    private bool hasWon;

    public string Prompt =>
        isOpen && !hasWon ? "Press E to Escape" : "";

    public void OpenDoor()
    {
        if (hasWon)
            return;

        isOpen = true;

        if (doorVisual != null)
            doorVisual.SetActive(true);

        if (interactionCollider != null)
            interactionCollider.enabled = true;

        Debug.Log(name + " appeared!");
    }

    public void CloseDoor()
    {
        isOpen = false;

        if (doorVisual != null)
            doorVisual.SetActive(false);

        if (interactionCollider != null)
            interactionCollider.enabled = false;

        Debug.Log(name + " disappeared!");
    }

    public void Interact(PlayerInteract player)
    {
        if (!isOpen || hasWon || player == null)
            return;

        WinGame();
    }

    private void WinGame()
    {
        hasWon = true;

        Debug.Log("YOU WIN!");

        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void OnGUI()
    {
        if (!hasWon)
            return;

        GUIStyle style = new GUIStyle();

        style.fontSize = 50;
        style.fontStyle = FontStyle.Bold;
        style.alignment = TextAnchor.MiddleCenter;
        style.normal.textColor = Color.green;

        GUI.Label(
            new Rect(0, 0, Screen.width, Screen.height),
            "YOU WIN!",
            style
        );
    }
}
