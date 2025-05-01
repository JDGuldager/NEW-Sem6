using UnityEngine;
using System;

[System.Serializable]
public class LilyPadRoute
{
    public GameObject[] pads;
}

public enum Difficulty
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
    [SerializeField] private GameObject difficultySelectorGroup;
    [SerializeField] private int tutorialRouteIndex = 0;
    [SerializeField] private int easyRouteIndex = 1;
    [SerializeField] private int mediumRouteIndex = 2;
    [SerializeField] private int hardRouteIndex = 3;

    [Header("Game Difficulty")]
    public Difficulty difficulty = Difficulty.Easy;

    public int currentRouteIndex = 0;
    public int currentStepIndex = 0;

    public GameObject ghostFeet;
    public FrogBehavior frogBehaviorscript;

    public float GetPadMaxStandTime()
    {
        return difficulty switch
        {
            Difficulty.Easy => 6f,
            Difficulty.Medium => 4f,
            Difficulty.Hard => 2.5f,
            _ => 4f,
        };
    }

    public LilyPadRoute CurrentRoute => allRoutes[currentRouteIndex];
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
        if (currentStepIndex >= CurrentRoute.pads.Length)
        {
            Debug.Log("Returned to start pad — ending route");

            var finalPad = CurrentRoute.pads[CurrentRoute.pads.Length - 1];
            finalPad.GetComponent<LilyPadBehavior>().SinkDown();

            currentRouteIndex++;
            currentStepIndex = 0;

            if (currentRouteIndex == 1 && difficultySelectorGroup != null)
            {
                difficultySelectorGroup.SetActive(true);
                Debug.Log("Tutorial complete — showing difficulty selector pads.");
                return;
            }

            Debug.Log("Waiting for player to choose next difficulty.");
            return;
        }

        // Auto-start only for tutorial
        if (currentRouteIndex == tutorialRouteIndex)
        {
            SessionDataManager.Instance.SetIsTutorial(true); //  Set tutorial flag

            Debug.Log($"Start Pad Stepped On — Starting Tutorial Route {currentRouteIndex + 1}");

            currentStepIndex = 0;
            var padObj = CurrentRoute.pads[0];
            padObj.SetActive(true);
            padObj.GetComponent<LilyPadBehavior>().FloatUp();

            ghostFeet.SetActive(true);
        }
    }

    public void SelectDifficulty(Difficulty selected)
    {
        difficulty = selected;
        SessionDataManager.Instance.selectedDifficulty = selected;

        switch (selected)
        {
            case Difficulty.Easy: currentRouteIndex = easyRouteIndex; break;
            case Difficulty.Medium: currentRouteIndex = mediumRouteIndex; break;
            case Difficulty.Hard: currentRouteIndex = hardRouteIndex; break;
        }

        currentStepIndex = 0;

        SessionDataManager.Instance.SetIsTutorial(false); //  Disable tutorial mode on difficulty selection

        var padObj = CurrentRoute.pads[0];
        padObj.SetActive(true);
        padObj.GetComponent<LilyPadBehavior>().FloatUp();

        Debug.Log($"Difficulty selected: {selected}. Starting route {currentRouteIndex}.");
    }

    private void OnLilyPadBufferedStep(LilyPadBehavior steppedPad)
    {
        if (steppedPad.stepIndex != currentStepIndex) return;

        Debug.Log($"Lily pad {steppedPad.stepIndex} activated");

        if (currentStepIndex == 0 && difficultySelectorGroup.activeSelf)
        {
            difficultySelectorGroup.SetActive(false);
            Debug.Log("Selector pads hidden after stepping on first route pad.");
        }

        if (currentStepIndex == 0)
        {
            SessionDataManager.Instance.BeginSession(); //  Start session when non-tutorial route begins
            startPad.Hide();
            if (SessionDataManager.Instance.selectedDifficultyPad != null)
            {
                SessionDataManager.Instance.selectedDifficultyPad.SetActive(false);
                SessionDataManager.Instance.selectedDifficultyPad = null;
            }
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

        if (currentStepIndex == CurrentRoute.pads.Length)
        {
            SessionDataManager.Instance.EndSession(); //  End session when reaching final pad
        }
    }

    private void OnPadFailure(LilyPadBehavior pad)
    {
        Debug.Log($"Pad {pad.stepIndex} failed — player stood too long!");
    }
}
