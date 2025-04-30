using System.Collections;
using UnityEngine;
using UnityEngine.XR;

[RequireComponent(typeof(AudioSource))]
public class DifficultySelectorPad : MonoBehaviour
{
    [Header("StartPad")]
    [SerializeField] private StartPad startPadToHide;

    [Header("Difficulty")]
    public Difficulty difficulty;
    public DifficultySelectorPad[] otherPadsToDisable;
    public LilyPadSpawner spawner;

    [Header("Activation Settings")]
    [SerializeField] private float bufferDuration = 1f;
    [SerializeField] private float floatSpeed = 1.0f;
    [SerializeField] private float surfaceHeight = 0f;
    [SerializeField] private Material steppedOnMaterial;
    private Material originalMaterial;

    [Header("Audio")]
    [SerializeField] private AudioClip spawnSound;
    [SerializeField] private AudioClip selectSound;

    private AudioSource audioSource;
    private MeshRenderer meshRenderer;

    private Vector3 startPos;
    private Vector3 targetPos;
    private float elapsedTime;
    private bool isFloatingUp = false;
    private bool activated = false;

    private bool isLeftFootOn = false;
    private bool isRightFootOn = false;
    private bool playerOnPad = false;

    private Coroutine bufferRoutine;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        meshRenderer = GetComponent<MeshRenderer>();
        originalMaterial = meshRenderer.material;
    }

    private void OnEnable()
    {
        FloatUp();
    }

    private void Update()
    {
        if (isFloatingUp)
        {
            elapsedTime += Time.deltaTime * floatSpeed;
            transform.position = Vector3.Lerp(startPos, targetPos, elapsedTime);
            if (elapsedTime >= 1f) isFloatingUp = false;
        }
    }

    public void FloatUp()
    {
        elapsedTime = 0f;
        startPos = new Vector3(transform.position.x, surfaceHeight - 1.5f, transform.position.z);
        targetPos = new Vector3(transform.position.x, surfaceHeight, transform.position.z);
        transform.position = startPos;
        isFloatingUp = true;

        PlaySound(spawnSound);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("LeftFoot")) isLeftFootOn = true;
        if (other.CompareTag("RightFoot")) isRightFootOn = true;
        CheckFeetStatus();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("LeftFoot")) isLeftFootOn = false;
        if (other.CompareTag("RightFoot")) isRightFootOn = false;
        CheckFeetStatus();
    }

    private void CheckFeetStatus()
    {
        playerOnPad = isLeftFootOn && isRightFootOn;

        if (playerOnPad && bufferRoutine == null && !activated)
        {
            bufferRoutine = StartCoroutine(BufferStep());

            if (steppedOnMaterial)
                meshRenderer.material = steppedOnMaterial;

            //  Immediately despawn other pads visually
            foreach (var pad in otherPadsToDisable)
            {
                if (pad != null)
                    pad.gameObject.SetActive(false);
            }

            Debug.Log($"Player stepped on {difficulty} pad — hiding other selector pads.");
        }
        else if (!playerOnPad)
        {
            if (bufferRoutine != null)
            {
                StopCoroutine(bufferRoutine);
                bufferRoutine = null;
            }

            meshRenderer.material = originalMaterial;
        }
    }


    private IEnumerator BufferStep()
    {
        yield return new WaitForSeconds(bufferDuration);
        TriggerSelection();
    }

    private void TriggerSelection()
    {
        if (activated) return;
        activated = true;

        PlaySound(selectSound);
        SendHaptics();

        if (spawner != null)
            spawner.SelectDifficulty(difficulty);

        // Hide the StartPad after selection buffer completes
        if (startPadToHide != null)
        {
            startPadToHide.Hide();
            Debug.Log("StartPad hidden after difficulty selected.");
        }

        // Disable other pads
        foreach (var pad in otherPadsToDisable)
        {
            if (pad != null)
                pad.gameObject.SetActive(false);
        }

        gameObject.SetActive(false);
    }


    private void PlaySound(AudioClip clip)
    {
        if (audioSource && clip)
            audioSource.PlayOneShot(clip);
    }

    private void SendHaptics()
    {
        foreach (XRNode node in new[] { XRNode.LeftHand, XRNode.RightHand })
        {
            InputDevice device = InputDevices.GetDeviceAtXRNode(node);
            if (device.TryGetHapticCapabilities(out HapticCapabilities cap) && cap.supportsImpulse)
            {
                device.SendHapticImpulse(0, 0.5f, 0.2f);
            }
        }
    }
}
