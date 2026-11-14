using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class GameManager : MonoBehaviour
{
    [SerializeField] private AudioClip introMusic;
    [SerializeField, TextArea(5, 12)] private string story =
        "You were kidnapped and left inside a maze. Your head-mounted flashlight is your only light.\n\n" +
        "Find the exit door. It appears randomly along the edge of the maze every 15 minutes after " +
        "the previous door is closed. Use tactics and your weapon to stun the enemy and buy yourself " +
        "time.\n\n" +
        "Reach the core of the maze to claim a flag checkpoint. After that, you can automatically " +
        "revive at the checkpoint after 10 seconds up to three times. If you die again, you return " +
        "to the starting point.";

    private AudioSource audioSource;
    private bool showStory = true;
    private GUIStyle titleStyle;
    private GUIStyle storyStyle;
    private GUIStyle buttonStyle;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
    }

    private void Start()
    {
        if (introMusic != null)
        {
            audioSource.clip = introMusic;
            audioSource.loop = false;
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("Assign the Intro.mp3 clip to the GameManager's Intro Music field.", this);
        }
    }

    private void OnGUI()
    {
        if (!showStory)
        {
            return;
        }

        CreateStyles();

        float panelWidth = Mathf.Min(1400f, Screen.width * 0.92f);
        float panelHeight = Mathf.Min(900f, Screen.height * 0.92f);
        Rect panel = new Rect(
            (Screen.width - panelWidth) * 0.5f,
            (Screen.height - panelHeight) * 0.5f,
            panelWidth,
            panelHeight);

        Color previousColor = GUI.color;
        GUI.color = new Color(0.06f, 0.06f, 0.08f, 0.96f);
        GUI.Box(panel, GUIContent.none);
        GUI.color = Color.white;

        GUI.Label(
            new Rect(panel.x + 32f, panel.y + 28f, panel.width - 64f, 80f),
            "ESCAPE THE MAZE",
            titleStyle);
        GUI.Label(
            new Rect(panel.x + 56f, panel.y + 130f, panel.width - 112f, panel.height - 250f),
            story,
            storyStyle);

        float buttonWidth = 280f * Mathf.Clamp(Screen.height / 900f, 0.75f, 1.4f);
        float buttonHeight = 60f * Mathf.Clamp(Screen.height / 900f, 0.75f, 1.4f);
        Rect button = new Rect(
            panel.x + (panel.width - buttonWidth) * 0.5f,
            panel.yMax - buttonHeight - 24f,
            buttonWidth,
            buttonHeight);
        if (GUI.Button(button, "Continue", buttonStyle))
        {
            showStory = false;
            audioSource.Stop();
        }

        GUI.color = previousColor;
    }

    private void CreateStyles()
    {
        if (titleStyle == null)
        {
            titleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                wordWrap = true
            };
        }

        if (storyStyle == null)
        {
            storyStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                wordWrap = true
            };
        }

        if (buttonStyle == null)
        {
            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontStyle = FontStyle.Bold
            };
        }

        float scale = Mathf.Clamp(Screen.height / 900f, 0.75f, 1.4f);
        titleStyle.fontSize = Mathf.RoundToInt(54f * scale);
        storyStyle.fontSize = Mathf.RoundToInt(28f * scale);
        buttonStyle.fontSize = Mathf.RoundToInt(24f * scale);
    }
}
