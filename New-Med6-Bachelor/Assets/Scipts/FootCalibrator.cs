using UnityEngine;
using UnityEngine.XR;

public class FootCalibrator : MonoBehaviour
{
    [Header("Foot Tracking")]
    public FootTracking footTracking;

    [Header("Calibration References")]
    public Transform leftFoot;
    public Transform rightFoot;

    [Header("Calibration Settings")]
    public XRNode leftFootSource = XRNode.LeftHand;
    public XRNode rightFootSource = XRNode.RightHand;

    private InputDevice leftDevice;
    private InputDevice rightDevice;

    private bool alreadyCalibratedThisPress = false;

    void Start()
    {
        leftDevice = InputDevices.GetDeviceAtXRNode(leftFootSource);
        rightDevice = InputDevices.GetDeviceAtXRNode(rightFootSource);
    }

    void Update()
    {
        bool aButtonPressed = false;

        if (rightDevice.isValid)
        {
            rightDevice.TryGetFeatureValue(CommonUsages.primaryButton, out aButtonPressed);
        }

        if (aButtonPressed && !alreadyCalibratedThisPress)
        {
            CalibrateFeet();
            alreadyCalibratedThisPress = true;
        }
        else if (!aButtonPressed)
        {
            alreadyCalibratedThisPress = false; // Reset when button released
        }
    }

    private void CalibrateFeet()
    {
        Vector3 leftPos, rightPos;
        Quaternion leftRot, rightRot;

        if (leftDevice.TryGetFeatureValue(CommonUsages.devicePosition, out leftPos) &&
            leftDevice.TryGetFeatureValue(CommonUsages.deviceRotation, out leftRot))
        {
            Vector3 worldLeftOffset = leftFoot.position - leftPos;
            Quaternion worldLeftRotOffset = Quaternion.Inverse(leftRot) * leftFoot.rotation;

            footTracking.leftFootOffset = Quaternion.Inverse(leftRot) * worldLeftOffset;
            footTracking.leftFootRotationOffset = worldLeftRotOffset;
        }

        if (rightDevice.TryGetFeatureValue(CommonUsages.devicePosition, out rightPos) &&
            rightDevice.TryGetFeatureValue(CommonUsages.deviceRotation, out rightRot))
        {
            Vector3 worldRightOffset = rightFoot.position - rightPos;
            Quaternion worldRightRotOffset = Quaternion.Inverse(rightRot) * rightFoot.rotation;

            footTracking.rightFootOffset = Quaternion.Inverse(rightRot) * worldRightOffset;
            footTracking.rightFootRotationOffset = worldRightRotOffset;
        }

        Debug.Log("Foot Calibration Complete via A Button!");
    }
}
