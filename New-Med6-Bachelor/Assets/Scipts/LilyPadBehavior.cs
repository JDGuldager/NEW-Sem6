using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class LilyPadBehavior : MonoBehaviour
{
    [Header("Step Logic")]
    [Tooltip("How long both feet must be on the pad before it's considered 'stepped on'")]
    [SerializeField] private float bufferDuration = 1f;

    [Tooltip("Whether the pad can sink if player stands too long")]
    [SerializeField] private bool enableInstability = true;

    [Header("Visual Feedback")]
    [Tooltip("Material to use when both feet are on the pad")]
    [SerializeField] private Material steppedOnMaterial;
    private Material originalMaterial;

    [Header("Float Animation")]
    [Tooltip("Speed at which pad floats up/down")]
    [SerializeField] private float floatSpeed = 1.0f;

    [Tooltip("Y height of the water surface")]
    [SerializeField] private float surfaceHeight = 0f;

    [Header("Warning + Audio")]
    [Tooltip("Sound played when pad floats up")]
    [SerializeField] private AudioClip spawnSound;

    [Tooltip("Sound played when pad sinks")]
    [SerializeField] private AudioClip despawnSound;

    [Tooltip("Duration of shake before failure sink")]
    [SerializeField] private float warningDuration = 1f;

    [Tooltip("Shake intensity before failure")]
    [SerializeField] private float warningShakeIntensity = 0.1f;

    private AudioSource audioSource;
    private Coroutine bufferRoutine;

    // Float/sink control
    private Vector3 startPos;
    private Vector3 targetPos;
    private float elapsedTime = 0f;
    private bool isFloatingUp = false;
    private bool isSinking = false;
    private bool isSinkingDueToTimeout = false;

    // Foot detection
    private bool isLeftFootOn = false;
    private bool isRightFootOn = false;
    private bool playerOnPad = false;
    private float timeOnPad = 0f;

    // Pad identity
    public int stepIndex;

    // Events
    public static event Action<LilyPadBehavior> OnBufferedStep;
    public static event Action<LilyPadBehavior> OnPadFailure;

    // Spawner ref to get difficulty
    private LilyPadSpawner spawner;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        spawner = FindObjectOfType<LilyPadSpawner>();
    }

    private void Start()
    {
        // Save the original material on startup
        originalMaterial = GetComponent<MeshRenderer>().material;
    }

    private void Update()
    {
        // Floating up animation
        if (isFloatingUp)
        {
            elapsedTime += Time.deltaTime * floatSpeed;
            transform.position = Vector3.Lerp(startPos, targetPos, elapsedTime);
            if (elapsedTime >= 1f) isFloatingUp = false;
        }

        // Sinking animation
        else if (isSinking)
        {
            elapsedTime += Time.deltaTime * floatSpeed;
            transform.position = Vector3.Lerp(startPos, targetPos, elapsedTime);
            if (elapsedTime >= 1f)
            {
                isSinking = false;
                gameObject.SetActive(false);
            }
        }

        // Time tracking for instability failure
        if (enableInstability && playerOnPad && !isSinkingDueToTimeout && !isSinking)
        {
            timeOnPad += Time.deltaTime;

            if (spawner != null && timeOnPad >= spawner.GetPadMaxStandTime())
            {
                StartCoroutine(FailAndSink());
            }
        }
    }

    //Triggers the lily pad to float up from below the surface
    public void FloatUp()
    {
        elapsedTime = 0f;
        startPos = new Vector3(transform.position.x, surfaceHeight - 1.5f, transform.position.z);
        targetPos = new Vector3(transform.position.x, surfaceHeight, transform.position.z);
        transform.position = startPos;
        isFloatingUp = true;
        PlaySound(spawnSound);
    }

    // Triggers the lily pad to sink below the surface
    public void SinkDown()
    {
        elapsedTime = 0f;
        startPos = transform.position;
        targetPos = new Vector3(transform.position.x, surfaceHeight - 1.5f, transform.position.z);
        isSinking = true;
        PlaySound(despawnSound);
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource && clip)
        {
            audioSource.PlayOneShot(clip);
        }
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

    // Checks if both feet are on, starts timer and changes material
    private void CheckFeetStatus()
    {
        playerOnPad = isLeftFootOn && isRightFootOn;

        if (playerOnPad && bufferRoutine == null)
        {
            bufferRoutine = StartCoroutine(BufferStep());

            // Change material to stepped-on
            if (steppedOnMaterial)
                GetComponent<MeshRenderer>().material = steppedOnMaterial;
        }
        else if (!playerOnPad)
        {
            // Reset buffer
            if (bufferRoutine != null)
            {
                StopCoroutine(bufferRoutine);
                bufferRoutine = null;
            }

            timeOnPad = 0f;

            // Restore material
            if (originalMaterial)
                GetComponent<MeshRenderer>().material = originalMaterial;
        }
    }

    // Triggers after player stands on pad with both feet for 1 second
    private IEnumerator BufferStep()
    {
        yield return new WaitForSeconds(bufferDuration);
        OnBufferedStep?.Invoke(this);
    }

    // Triggers when player overstays on pad (based on difficulty)
    private IEnumerator FailAndSink()
    {
        isSinkingDueToTimeout = true;

        Debug.Log("WARNING: Lily pad unstable — about to sink!");

        // Shake before sinking
        Vector3 originalPos = transform.position;
        float elapsed = 0f;

        while (elapsed < warningDuration)
        {
            float shakeX = UnityEngine.Random.Range(-warningShakeIntensity, warningShakeIntensity);
            float shakeZ = UnityEngine.Random.Range(-warningShakeIntensity, warningShakeIntensity);
            transform.position = originalPos + new Vector3(shakeX, 0, shakeZ);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = originalPos;
        SinkDown();

        OnPadFailure?.Invoke(this);
    }
}
