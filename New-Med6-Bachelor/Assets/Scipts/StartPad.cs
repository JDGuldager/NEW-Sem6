// StartPad.cs
using UnityEngine;
using System;
using System.Collections;

public class StartPad : MonoBehaviour
{
    public static event Action<StartPad> SteppedOnStart;

    private bool isLeftFootOn = false;
    private bool isRightFootOn = false;
    private bool hasSteppedOn = false;

    [SerializeField] private float bufferDuration = 1f;
    private Coroutine bufferRoutine;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("LeftFoot")) isLeftFootOn = true;
        else if (other.CompareTag("RightFoot")) isRightFootOn = true;

        CheckFeetStatus();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("LeftFoot")) isLeftFootOn = false;
        else if (other.CompareTag("RightFoot")) isRightFootOn = false;

        CheckFeetStatus();
    }

    private void CheckFeetStatus()
    {
        if (isLeftFootOn && isRightFootOn && !hasSteppedOn)
        {
            hasSteppedOn = true;
            bufferRoutine = StartCoroutine(BufferStep());
        }
        else if (!isLeftFootOn || !isRightFootOn)
        {
            hasSteppedOn = false;
            if (bufferRoutine != null)
            {
                StopCoroutine(bufferRoutine);
                bufferRoutine = null;
            }
        }
    }

    private IEnumerator BufferStep()
    {
        yield return new WaitForSeconds(bufferDuration);
        SteppedOnStart?.Invoke(this);
    }
    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

}