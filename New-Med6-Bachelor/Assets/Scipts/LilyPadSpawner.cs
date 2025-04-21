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
        if (currentRouteIndex >= allRoutes.Length)
        {
            Debug.Log("No more routes left.");
            return;
        }

        Debug.Log($"Start Pad Stepped On — Starting Route {currentRouteIndex + 1}");

        currentStepIndex = 0;

        var padObj = CurrentRoute.pads[0];
        padObj.SetActive(true);
        padObj.GetComponent<LilyPadBehavior>().FloatUp();
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
            Debug.Log("Final pad reached — respawning Start Pad");

            steppedPad.SinkDown();
            startPad.Show();

            currentRouteIndex++;
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
