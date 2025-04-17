using UnityEngine;
using System;

public class StartPad : MonoBehaviour
{
    public LilyPadSpawner lilyPadSpawnerScript;
    public static event Action<StartPad> SteppedOnStart;

    private bool isLeftFootOn = false;
    private bool isRightFootOn = false;
    private bool hasSteppedOn = false;

    void Start()
    {
        // Optional initialization if needed
    }

    void Update()
    {
        // No need for Update right now
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("LeftFoot"))
        {
            isLeftFootOn = true;
        }
        else if (other.CompareTag("RightFoot"))
        {
            isRightFootOn = true;
        }

        CheckFeetStatus();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("LeftFoot"))
        {
            isLeftFootOn = false;
        }
        else if (other.CompareTag("RightFoot"))
        {
            isRightFootOn = false;
        }

        CheckFeetStatus();
    }

    private void CheckFeetStatus()
    {
        if (isLeftFootOn && isRightFootOn && !hasSteppedOn)
        {
            hasSteppedOn = true;
            SteppedOnStart?.Invoke(this);
        }
        else if (!isLeftFootOn || !isRightFootOn)
        {
            hasSteppedOn = false;
        }
    }
}
