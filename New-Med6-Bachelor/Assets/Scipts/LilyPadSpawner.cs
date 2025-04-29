using UnityEngine;

[System.Serializable]
public class LilyPadRoute
{
    public GameObject[] pads;
}

public enum DifficultyLevel
{
    Easy,
    Medium,
    Hard
}

public class LilyPadSpawner : MonoBehaviour
{
    [Header("Routes")]
    [SerializeField] private LilyPadRoute[] allRoutes;
    [SerializeField] private StartPad startPad;

    [Header("Game Difficulty")]
    public DifficultyLevel difficulty = DifficultyLevel.Easy;

    private int currentRouteIndex = 0;
    private int currentStepIndex = 0;

    public float GetPadMaxStandTime()
    {
        return difficulty switch
        {
            DifficultyLevel.Easy => 6f,
            DifficultyLevel.Medium => 4f,
            DifficultyLevel.Hard => 2.5f,
            _ => 4f,
        };
    }

    private LilyPadRoute CurrentRoute => allRoutes[currentRouteIndex];
    public LilyPadRoute[] AllRoutes => allRoutes;

    public GameObject ghostFootL;
    public GameObject ghostFootR;

    private void OnEnable()
    {
        StartPad.SteppedOnStart += OnStartPadSteppedOn;
        LilyPadBehavior.OnBufferedStep += OnLilyPadBufferedStep;
        LilyPadBehavior.OnPadFailure += OnPadFailure;
    }

    private void OnDisable()
    {
        StartPad.SteppedOnStart -= OnStartPadSteppedOn;
        LilyPadBehavior.OnBufferedStep -= OnLilyPadBufferedStep;
        LilyPadBehavior.OnPadFailure -= OnPadFailure;
    }

    private void OnStartPadSteppedOn(StartPad pad)
    {
        // If player is returning from previous route
        if (currentStepIndex >= CurrentRoute.pads.Length)
        {
            Debug.Log("Returned to start pad — ending route");

            // Sink the final pad now
            var finalPad = CurrentRoute.pads[CurrentRoute.pads.Length - 1];
            finalPad.GetComponent<LilyPadBehavior>().SinkDown();

            currentRouteIndex++;
            currentStepIndex = 0;

            // Spawn next route if available
            if (currentRouteIndex < allRoutes.Length)
            {
                var padObj = CurrentRoute.pads[0];
                padObj.SetActive(true);
                padObj.GetComponent<LilyPadBehavior>().FloatUp();
            }
            else
            {
                Debug.Log("All routes completed!");
            }

            return;
        }

        // First time stepping on start pad — start a route
        if (currentRouteIndex < allRoutes.Length)
        {
            Debug.Log($"Start Pad Stepped On — Starting Route {currentRouteIndex + 1}");

            currentStepIndex = 0;

            var padObj = CurrentRoute.pads[0];
            padObj.SetActive(true);
            padObj.GetComponent<LilyPadBehavior>().FloatUp();

            // Show ghost feet
            ghostFootL.SetActive(true);
            ghostFootR.SetActive(true);
        }
    }


    private void OnLilyPadBufferedStep(LilyPadBehavior steppedPad)
    {
        if (steppedPad.stepIndex != currentStepIndex) return;

        Debug.Log($"Lily pad {steppedPad.stepIndex} activated");

        if (currentStepIndex == 0)
        {
            startPad.Hide();
        }
        else
        {
            var previous = CurrentRoute.pads[currentStepIndex - 1];
            previous.GetComponent<LilyPadBehavior>().SinkDown();
        }

        currentStepIndex++;

        if (currentStepIndex < CurrentRoute.pads.Length)
        {
            var next = CurrentRoute.pads[currentStepIndex];
            next.SetActive(true);
            next.GetComponent<LilyPadBehavior>().FloatUp();
        }
        else
        {
            Debug.Log("Final pad reached — return to start pad");
            
            startPad.Show();

        }

    }

    private void OnPadFailure(LilyPadBehavior pad)
    {
        Debug.Log($"Pad {pad.stepIndex} failed — player stood too long!");

        // Optional: restart route, show fail UI, etc.
        // Example:
        // SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
