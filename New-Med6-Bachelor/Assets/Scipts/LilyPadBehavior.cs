using System;
using UnityEngine;

public class LilyPadBehavior : MonoBehaviour
{
    [SerializeField] private Material steppedOn;
    private Material originalMaterial;

    public static event Action<LilyPadBehavior> OnSteppedOn;

    private Vector3 startPos;
    private Vector3 endPos;
    private float lerpTime = 1f;
    private float elapsedTime = 0f;

    private bool isLeftFootOn = false;
    private bool isRightFootOn = false;
    private bool hasSteppedOn = false;

    void Start()
    {
        startPos = transform.position;
        endPos = new Vector3(startPos.x, 0, startPos.z);
        originalMaterial = GetComponent<MeshRenderer>().material;
    }

    void Update()
    {
        if (elapsedTime < lerpTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / lerpTime;
            transform.position = Vector3.Lerp(startPos, endPos, t);
        }
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
            OnSteppedOn?.Invoke(this);
            GetComponent<MeshRenderer>().material = steppedOn;
        }
        else if (!isLeftFootOn || !isRightFootOn)
        {
            hasSteppedOn = false;
            GetComponent<MeshRenderer>().material = originalMaterial;
        }
    }
}
