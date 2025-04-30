using System.IO;
using UnityEngine;

public class SessionDataManager : MonoBehaviour
{
    public static SessionDataManager Instance { get; private set; }

    [Header("Stats")]
    public int obstacleHits = 0;
    public int lilyPadTimeouts = 0;

    [Header("Timing")]
    private float startTime;
    private float endTime;
    private bool timerStarted = false;
    private bool timerEnded = false;

    [Header("Participant Info")]
    public int participantID;
    public string filePath;
    public Difficulty selectedDifficulty;

    [Header("Session Control")]
    [SerializeField] private bool isTutorial = false;
    public GameObject selectedDifficultyPad;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // prevent duplicates
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // persist between scenes
        }

        InitializeFilePath();
        participantID = GetNextParticipantID();
        PlayerPrefs.SetInt("LastParticipantID", participantID);
        PlayerPrefs.Save();
    }

    public void InitializeFilePath()
    {
        filePath = Application.persistentDataPath + "/StepData.txt";
    }

    int GetNextParticipantID()
    {
        if (PlayerPrefs.HasKey("LastParticipantID"))
            return PlayerPrefs.GetInt("LastParticipantID") + 1;

        if (File.Exists(filePath))
        {
            var lines = File.ReadAllLines(filePath);
            return lines.Length + 1;
        }

        return 1;
    }

    public void SetIsTutorial(bool tutorial)
    {
        isTutorial = tutorial;
        Debug.Log($"[Session] Tutorial mode: {isTutorial}");
    }

    public void BeginSession()
    {
        if (isTutorial) return;

        if (!timerStarted)
        {
            startTime = Time.time;
            timerStarted = true;
            Debug.Log($"[Session {participantID}] Timer started.");
        }
    }

    public void EndSession()
    {
        if (isTutorial) return;

        if (timerStarted && !timerEnded)
        {
            endTime = Time.time;
            timerEnded = true;
            WriteDataToFile();
        }
    }

    public void RegisterObstacleHit()
    {
        if (!isTutorial && timerStarted && !timerEnded)
        {
            obstacleHits++;
            Debug.Log($"Obstacle hit! Total: {obstacleHits}");
        }
    }

    public void RegisterLilyPadTimeout()
    {
        if (!isTutorial && timerStarted && !timerEnded)
        {
            lilyPadTimeouts++;
            Debug.Log($"Lily pad timeout! Total: {lilyPadTimeouts}");
        }
    }

    [ContextMenu("Write Data To File")]
    public void WriteDataToFile()
    {
        if (isTutorial)
        {
            Debug.Log("[Session] Skipped saving (tutorial mode)");
            return;
        }

        float totalTime = endTime - startTime;
        string line = $"Participant: {participantID}, Difficulty: {selectedDifficulty}, Time: {totalTime:F2}, Obstacles Hit: {obstacleHits}, Lily Pad Timeouts: {lilyPadTimeouts}";
        File.AppendAllText(filePath, line + "\n");

        Debug.Log($"Session data saved: {line}");
        Debug.Log("Saved to: " + filePath);
    }
}
