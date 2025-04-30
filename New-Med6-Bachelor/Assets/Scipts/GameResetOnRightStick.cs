using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;

public class GameResetOnRightStick : MonoBehaviour
{
    [Tooltip("Name of the scene to reload. Leave blank to reload current scene.")]
    public string sceneToReload = "";

    private InputDevice rightController;
    private bool pressedLastFrame = false;

    void Start()
    {
        rightController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
    }

    void Update()
    {
        if (!rightController.isValid)
        {
            // Refresh device if disconnected or null
            rightController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
            return;
        }

        bool isPressed;
        if (rightController.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out isPressed))
        {
            if (isPressed && !pressedLastFrame)
            {
                Debug.Log("Right joystick clicked — restarting game...");
                RestartGame();
            }

            pressedLastFrame = isPressed;
        }
    }

    private void RestartGame()
    {
        string sceneName = string.IsNullOrEmpty(sceneToReload)
            ? SceneManager.GetActiveScene().name
            : sceneToReload;

        SceneManager.LoadScene(sceneName);
    }
}
