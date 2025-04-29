using UnityEngine;
using UnityEngine.XR;

public class FootTracking : MonoBehaviour
{
    [Header("Foot References")]
    public Transform leftFoot;
    public Transform rightFoot;

    [Header("Offset Settings")]
    public Vector3 leftFootOffset = Vector3.zero;
    public Quaternion leftFootRotationOffset = Quaternion.identity;
    public Vector3 rightFootOffset = Vector3.zero;
    public Quaternion rightFootRotationOffset = Quaternion.identity;

    void Update()
    {
        InputDevice leftDevice = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        InputDevice rightDevice = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

        Vector3 pos;
        Quaternion rot;

        if (leftDevice.TryGetFeatureValue(CommonUsages.devicePosition, out pos) &&
            leftDevice.TryGetFeatureValue(CommonUsages.deviceRotation, out rot))
        {
            leftFoot.position = pos + rot * leftFootOffset;
            leftFoot.rotation = rot * leftFootRotationOffset;
        }

        if (rightDevice.TryGetFeatureValue(CommonUsages.devicePosition, out pos) &&
            rightDevice.TryGetFeatureValue(CommonUsages.deviceRotation, out rot))
        {
            rightFoot.position = pos + rot * rightFootOffset;
            rightFoot.rotation = rot * rightFootRotationOffset;
        }
    }
}
