using UnityEngine;
using Valve.VR;

public class TrackerManager : MonoBehaviour
{
    public SteamVR_Behaviour_Pose leftFootPose;
    public SteamVR_Behaviour_Pose rightFootPose;
    public SteamVR_Behaviour_Pose headPose;

    public Transform leftFootObject;
    public Transform rightFootObject;
    public Transform headObject;

    void Update()
    {
        if (headPose != null && headObject != null)
        {
            headObject.position = headPose.transform.position;
            headObject.rotation = headPose.transform.rotation;
        }

        if (leftFootPose != null && leftFootObject != null)
        {
            leftFootObject.position = leftFootPose.transform.position;
            leftFootObject.rotation = leftFootPose.transform.rotation;

            Vector3 leftFootRelative = headObject.InverseTransformPoint(leftFootObject.position);
            Debug.Log("Left Foot Relative Position: " + leftFootRelative);
        }

        if (rightFootPose != null && rightFootObject != null)
        {
            rightFootObject.position = rightFootPose.transform.position;
            rightFootObject.rotation = rightFootPose.transform.rotation;

            Vector3 rightFootRelative = headObject.InverseTransformPoint(rightFootObject.position);
           Debug.Log("Right Foot Relative Position: " + rightFootRelative);
        }
    }
}
