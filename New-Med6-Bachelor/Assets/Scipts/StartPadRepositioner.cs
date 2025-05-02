using UnityEngine;
using UnityEngine.XR;

public class StartAreaRepositioner : MonoBehaviour
{
    [Header("Feet References")]
    public Transform leftFoot;
    public Transform rightFoot;

    [Header("Shared Parent to Move (StartPad + Routes)")]
    public Transform sharedParent;
    public LilyPadSpawner spawner;

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

        // Rotate to face the first lily pad
        if (spawner != null && spawner.CurrentRoute.pads.Length > 0)
        {
            Vector3 targetPadPosition = spawner.CurrentRoute.pads[0].transform.position;
            Vector3 directionToFirstPad = targetPadPosition - newPosition;
            directionToFirstPad.y = 0f; // ignore vertical difference

            if (directionToFirstPad.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToFirstPad);
                sharedParent.rotation = targetRotation;
            }
        }

        Debug.Log("Start area repositioned and rotated to face first lily pad.");
    }
}