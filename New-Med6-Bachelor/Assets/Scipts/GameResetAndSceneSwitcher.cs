using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;

public class GameResetAndSceneSwitcher : MonoBehaviour
{
    [Tooltip("Name of the scene to reload. Leave blank to reload current scene.")]
    public string sceneToReload = "";

    private InputDevice rightController;
    private InputDevice leftController;
    private bool rightPressedLastFrame = false;
    private bool leftPressedLastFrame = false;

    void Start()
    {
        rightController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
    }

    void Update()
    {
        if (!rightController.isValid)
            rightController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        if (!leftController.isValid)
            leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);

        // RIGHT STICK → Reset Scene
        bool rightStickPressed;
        if (rightController.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out rightStickPressed))
        {
            if (rightStickPressed && !rightPressedLastFrame)
            {
                Debug.Log("Right joystick clicked — restarting game...");
                RestartGame();
            }
            rightPressedLastFrame = rightStickPressed;
        }

        // LEFT STICK → Switch Scenes
        bool leftStickPressed;
        if (leftController.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out leftStickPressed))
        {
            if (leftStickPressed && !leftPressedLastFrame)
            {
                Debug.Log("Left joystick clicked — switching scene...");
                SwitchScenes();
            }
            leftPressedLastFrame = leftStickPressed;
        }
    }

    private void RestartGame()
    {
        string sceneName = string.IsNullOrEmpty(sceneToReload)
            ? SceneManager.GetActiveScene().name
            : sceneToReload;

        SceneManager.LoadScene(sceneName);
    }

    private void SwitchScenes()
    {
        string current = SceneManager.GetActiveScene().name;
        string next = current == "VR" ? "AR" : "VR";
        SceneManager.LoadScene(next);
    }
}
