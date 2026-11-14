using UnityEngine;

public class FirstPersonCam : MonoBehaviour
{
    public Transform player;
    public Vector3 offset;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        
    }
}

