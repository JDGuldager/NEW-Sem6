using UnityEngine;
using UnityEngine.XR;

public class StartAreaRepositioner : MonoBehaviour
{
    [Header("Feet References")]
    public Transform leftFoot;
    public Transform rightFoot;

    [Header("Shared Parent to Move (StartPad + Routes)")]
    public Transform sharedParent;

    private bool alreadyMoved = false;

    void Update()
    {
        InputDevice rightDevice = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

        bool aPressed = false;
        rightDevice.TryGetFeatureValue(CommonUsages.primaryButton, out aPressed);

        if (aPressed && !alreadyMoved)
        {
            MoveToFeet();
            alreadyMoved = true;
        }
        else if (!aPressed)
        {
            alreadyMoved = false;
        }
    }

    private void MoveToFeet()
    {
        if (leftFoot == null || rightFoot == null || sharedParent == null)
        {
            Debug.LogWarning("Missing references in StartAreaRepositioner.");
            return;
        }

        Vector3 avgPos = (leftFoot.position + rightFoot.position) / 2f;
        Vector3 newPosition = new Vector3(avgPos.x, sharedParent.position.y, avgPos.z);

        sharedParent.position = newPosition;

        Debug.Log("Start area repositioned to feet.");
    }
}
