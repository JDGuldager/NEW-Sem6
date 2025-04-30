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

    [Header("Materials")]
    [Tooltip("Material to apply when both feet are on the pad")]
    [SerializeField] private Material steppedOnMaterial;

    private Material originalMaterial;
    private MeshRenderer meshRenderer;

    [Header("Float Animation")]
    [Tooltip("Speed at which the pad floats up/down")]
    [SerializeField] private float floatSpeed = 1.0f;

    [Tooltip("Y height of the water surface")]
    [SerializeField] private float surfaceHeight = 0f;

    [Header("Warning & Audio")]
    [SerializeField] private AudioClip spawnSound;
    [SerializeField] private AudioClip despawnSound;
    [SerializeField] private float warningDuration = 1f;
    [SerializeField] private float warningShakeIntensity = 0.002f;

    [Header("Linked Obstacle")]
    [Tooltip("Obstacle to activate when this lily pad is active")]
    [SerializeField] private GameObject linkedObstacle;


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

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        spawner = FindObjectOfType<LilyPadSpawner>();
    }

    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        originalMaterial = meshRenderer.material;

        if (linkedObstacle == null)
        {
            // Auto find the first inactive child named "Obstacle"
            foreach (Transform child in transform)
            {
                if (!child.gameObject.activeSelf && child.name.ToLower().Contains("obstacle"))
                {
                    linkedObstacle = child.gameObject;
                    break;
                }
            }
        }
    }


    private void Update()
    {
        if (isFloatingUp)
        {
            elapsedTime += Time.deltaTime * floatSpeed;
            transform.position = Vector3.Lerp(startPos, targetPos, elapsedTime);
            if (elapsedTime >= 1f) isFloatingUp = false;
        }
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

        if (enableInstability && playerOnPad && !isSinkingDueToTimeout && !isSinking)
        {
            timeOnPad += Time.deltaTime;

            float maxTime = spawner != null ? spawner.GetPadMaxStandTime() : 3f;

            if (timeOnPad >= maxTime)
            {
                StartCoroutine(FailAndSink());
            }
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

        //meshRenderer.material = originalMaterial;

        //  Activate linked obstacle if one is set
        if (linkedObstacle != null)
        {
            linkedObstacle.SetActive(true);
        }
    }


    public void SinkDown()
    {
        elapsedTime = 0f;
        startPos = transform.position;
        targetPos = new Vector3(transform.position.x, surfaceHeight - 1.5f, transform.position.z);
        isSinking = true;
        PlaySound(despawnSound);

        if (linkedObstacle != null)
        {
            linkedObstacle.SetActive(false);
        }
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

    private void CheckFeetStatus()
    {
        playerOnPad = isLeftFootOn && isRightFootOn;

        if (playerOnPad && bufferRoutine == null)
        {
            bufferRoutine = StartCoroutine(BufferStep());

            //  Change material when player is on pad
            if (steppedOnMaterial)
            {
                meshRenderer.material = steppedOnMaterial;
            }
        }
        else if (!playerOnPad)
        {
            if (bufferRoutine != null)
            {
                StopCoroutine(bufferRoutine);
                bufferRoutine = null;
            }

            timeOnPad = 0f;

            //  Revert to original material
            meshRenderer.material = originalMaterial;
        }
    }

    private IEnumerator BufferStep()
    {
        yield return new WaitForSeconds(bufferDuration);
        OnBufferedStep?.Invoke(this);
    }

    private IEnumerator FailAndSink()
    {
        isSinkingDueToTimeout = true;

        Debug.Log("WARNING: Lily pad unstable — about to sink!");

        Vector3 originalPos = transform.position;
        float elapsed = 0f;

        float frequency = 10f; // how fast it oscillates
        float intensity = warningShakeIntensity; // small, controlled amplitude

        while (elapsed < warningDuration)
        {
            float offsetX = Mathf.Sin(elapsed * frequency) * intensity;
            float offsetZ = Mathf.Cos(elapsed * frequency) * intensity;
            transform.position = originalPos + new Vector3(offsetX, 0, offsetZ);

            elapsed += Time.deltaTime;
            yield return null;
        }

        SessionDataManager.Instance?.RegisterLilyPadTimeout();

        transform.position = originalPos;
        SinkDown();
        OnPadFailure?.Invoke(this);
    }

}
