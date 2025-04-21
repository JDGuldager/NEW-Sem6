using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class LilyPadBehavior : MonoBehaviour
{
    [Header("Step Logic")]
    [Tooltip("How long both feet must be on the pad before it's considered stepped on")]
    [SerializeField] private float bufferDuration = 1f;

    [Tooltip("Whether the pad can sink if player stands too long")]
    [SerializeField] private bool enableInstability = true;

    [Header("Visual Feedback")]
    [Tooltip("Material to apply when both feet are on the pad")]
    [SerializeField] private Material steppedOnMaterial;
    private Material originalMaterial;

    [Header("Float Animation")]
    [Tooltip("Speed at which the pad floats up/down")]
    [SerializeField] private float floatSpeed = 1.0f;

    [Tooltip("Y height of the water surface")]
    [SerializeField] private float surfaceHeight = 0f;

    [Header("Warning & Audio")]
    [Tooltip("Sound played when pad floats up")]
    [SerializeField] private AudioClip spawnSound;

    [Tooltip("Sound played when pad sinks")]
    [SerializeField] private AudioClip despawnSound;

    [Tooltip("Duration of shaking before sinking")]
    [SerializeField] private float warningDuration = 1f;

    [Tooltip("How intense the shake is before sinking")]
    [SerializeField] private float warningShakeIntensity = 0.1f;

    [Header("Danger Color Settings")]
    [Tooltip("Color when pad is safe")]
    [SerializeField] private Color safeColor = Color.green;

    [Tooltip("Color when pad is about to sink")]
    [SerializeField] private Color dangerColor = Color.red;

    [Tooltip("How fast the pad color lerps to danger color")]
    [SerializeField] private float colorLerpSpeed = 3f;

    private AudioSource audioSource;
    private Coroutine bufferRoutine;

    private Vector3 startPos;
    private Vector3 targetPos;
    private float elapsedTime = 0f;

    private bool isFloatingUp = false;
    private bool isSinking = false;
    private bool isSinkingDueToTimeout = false;

    private bool isLeftFootOn = false;
    private bool isRightFootOn = false;
    private bool playerOnPad = false;
    private float timeOnPad = 0f;

    public int stepIndex;

    public static event Action<LilyPadBehavior> OnBufferedStep;
    public static event Action<LilyPadBehavior> OnPadFailure;

    private LilyPadSpawner spawner;
    private MeshRenderer meshRenderer;
    private Material padMaterial;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        spawner = FindObjectOfType<LilyPadSpawner>();
    }

    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        padMaterial = meshRenderer.material; // Instance for runtime color change
        originalMaterial = padMaterial;
    }

    private void Update()
    {
        // Animate floating up
        if (isFloatingUp)
        {
            elapsedTime += Time.deltaTime * floatSpeed;
            transform.position = Vector3.Lerp(startPos, targetPos, elapsedTime);
            if (elapsedTime >= 1f) isFloatingUp = false;
        }
        // Animate sinking
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

        // Handle color transition & danger timer
        if (enableInstability && playerOnPad && !isSinkingDueToTimeout && !isSinking)
        {
            timeOnPad += Time.deltaTime;

            float maxTime = spawner != null ? spawner.GetPadMaxStandTime() : 3f;
            float dangerPercent = Mathf.Clamp01(timeOnPad / maxTime);

            Color currentColor = Color.Lerp(safeColor, dangerColor, dangerPercent);
            padMaterial.color = Color.Lerp(padMaterial.color, currentColor, Time.deltaTime * colorLerpSpeed);

            if (timeOnPad >= maxTime)
            {
                StartCoroutine(FailAndSink());
            }
        }
    }

    /// <summary>Triggers the lily pad to float up from below the surface</summary>
    public void FloatUp()
    {
        elapsedTime = 0f;
        startPos = new Vector3(transform.position.x, surfaceHeight - 1.5f, transform.position.z);
        targetPos = new Vector3(transform.position.x, surfaceHeight, transform.position.z);
        transform.position = startPos;
        isFloatingUp = true;
        PlaySound(spawnSound);
    }

    /// <summary>Triggers the lily pad to sink below the surface</summary>
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

    /// <summary>Handles step detection and visual feedback</summary>
    private void CheckFeetStatus()
    {
        playerOnPad = isLeftFootOn && isRightFootOn;

        if (playerOnPad && bufferRoutine == null)
        {
            bufferRoutine = StartCoroutine(BufferStep());

            if (steppedOnMaterial)
                meshRenderer.material = steppedOnMaterial;
        }
        else if (!playerOnPad)
        {
            if (bufferRoutine != null)
            {
                StopCoroutine(bufferRoutine);
                bufferRoutine = null;
            }

            timeOnPad = 0f;

            // Restore material and reset color
            if (originalMaterial)
            {
                meshRenderer.material = originalMaterial;
                padMaterial = originalMaterial;
            }

            padMaterial.color = safeColor;
        }
    }

    /// <summary>Buffer delay after both feet are detected</summary>
    private IEnumerator BufferStep()
    {
        yield return new WaitForSeconds(bufferDuration);
        OnBufferedStep?.Invoke(this);
    }

    /// <summary>Warning effect + sink due to overstaying on the pad</summary>
    private IEnumerator FailAndSink()
    {
        isSinkingDueToTimeout = true;
        Debug.Log("WARNING: Lily pad unstable — about to sink!");

        // Shake warning
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
