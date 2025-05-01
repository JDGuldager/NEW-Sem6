using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Audio;

public class FrogBehavior : MonoBehaviour
{
    public LilyPadSpawner LilyPadSpawnerScript;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip croakSound;

    public AnimationCurve curve;
    private Vector3 startPos;
    private Vector3 targetPos;

    private float current = 0f;
    private float target = 1f;

    private bool shouldJump = true;

    [SerializeField] private float journeyTime = 1.0f;

    [SerializeField] private float positionTolerance = 0.05f;
    private float yOffset;

    [SerializeField] private GameObject startPad;

    public GameObject nextPad;
    public Vector3 nextPadPos;
    public Vector3 spawnPos;

    public Animator frogAnimator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spawnPos = transform.position;

        if (LilyPadSpawnerScript == null)
        {
            Debug.LogError("LilyPadSpawnerScript is not assigned!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Determine the next pad or reset to the start pad
        if (LilyPadSpawnerScript.currentStepIndex < LilyPadSpawnerScript.CurrentRoute.pads.Length)
        {
            nextPad = LilyPadSpawnerScript.CurrentRoute.pads[LilyPadSpawnerScript.currentStepIndex];
            nextPadPos = nextPad.transform.position;
            yOffset = 0.06f;
        }
        else
        {
            nextPad = startPad;
            nextPadPos = spawnPos;
            yOffset = 0.05f;           
        }

        // Check if the target lily pad is valid
        if (nextPadPos.y >= -0.5f)
        {
            if (shouldJump)
            {
                // Set the starting position to the current position of the frog
                startPos = transform.position;
                targetPos = new Vector3(nextPadPos.x, yOffset, nextPadPos.z);

                if (!PositionsAreClose(startPos, targetPos))
                {
                    // Start the jump
                    shouldJump = false;
                    current = 0f; // Reset the interpolation progress
                    transform.position = targetPos;
                    transform.rotation = DirectionToLook(startPos, targetPos);
                }
            }

            // Smoothly interpolate the frog's position if the target lily pad is valid
            if (!shouldJump && current < target)
            {
                current += Time.deltaTime / journeyTime; // Increment progress over time
                LerpFrog();
            }
            else if (current >= target)
            {
                current = target; // Ensure it doesn't overshoot
                shouldJump = true; // Allow the next jump
                transform.position = targetPos; 
                frogAnimator.SetBool("InTheAir", false); 
            }
        }
        
        
    }

    public void LerpFrog()
    {
        frogAnimator.SetBool("InTheAir", true);
        // Smoothly interpolate the frog's position using the animation curve
        Vector3 interpolatedPosition = Vector3.Lerp(startPos, targetPos, current);

        // Update the Y position using the parabolic motion formula
        float parabolicY = curve.Evaluate(current); // Parabolic curve
        interpolatedPosition.y += parabolicY; // Add the parabolic Y motion

        // Apply the interpolated position to the frog
        transform.position = interpolatedPosition;

        
    }

    private bool PositionsAreClose(Vector3 pos1, Vector3 pos2)
    {
        // Check if the positions are close enough to be considered equal
        return Vector3.Distance(pos1, pos2) < positionTolerance;
    }
    private Quaternion DirectionToLook(Vector3 pos1, Vector3 pos2)
    {
        Vector3 direction = (pos2 - pos1).normalized;
        direction.y = 0;
        return Quaternion.LookRotation(direction, Vector3.up);

    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource && clip)
        {
            audioSource.PlayOneShot(clip);
        }
        Debug.Log("soundPLayed");
    }
}