using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class CameraController : NetworkBehaviour
{
    public GameObject cameraHolder; // The camera game object or a holder containing the camera
    public Vector3 offset; // Set this in the inspector to the desired offset from the player

    void Start()
    {
        // Enable the camera only if this prefab is controlled by the local player
        if (IsLocalPlayer)
        {
            // Ensure the camera is active only for the local player
            cameraHolder.SetActive(true);
        }
        else
        {
            // Disable the camera for non-local players
            cameraHolder.SetActive(false);
        }
    }

    void Update()
    {
        // Update the camera position only if this instance is the local player and the client is connected
        if (IsLocalPlayer && NetworkManager.Singleton.IsConnectedClient && SceneManager.GetActiveScene().name == "SampleScene")
        {
            Vector3 newPosition = transform.position + offset; // Apply the offset to follow the player
            cameraHolder.transform.position = newPosition;

            // If needed, make the camera look at the player or another target
            // cameraHolder.transform.LookAt(transform.position);
        }
    }
}
