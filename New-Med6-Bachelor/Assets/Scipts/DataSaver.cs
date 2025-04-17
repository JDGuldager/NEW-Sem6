using System.IO;
using UnityEngine;

public class DataSaver : MonoBehaviour
{
    [HideInInspector]
    public string filePath = "";

    public int participantID;

    private float startTime;
    private float endTime;
    private int errorCount = 0;
    private bool timerStarted = false;
    private bool timerEnded = false;

    void Awake()
    {
        InitializeFilePath();
        filePath = Application.persistentDataPath + "/StepData.txt";
        participantID = GetNextParticipantID();

        PlayerPrefs.SetInt("LastParticipantID", participantID);
        PlayerPrefs.Save();
    }

    int GetNextParticipantID()
    {
        if (PlayerPrefs.HasKey("LastParticipantID"))
        {
            return PlayerPrefs.GetInt("LastParticipantID") + 1;
        }

        if (File.Exists(filePath))
        {
            var lines = File.ReadAllLines(filePath);
            return lines.Length + 1;
        }

        return 1;
    }

    public void OnCorrectStep(bool isFirst, bool isLast)
    {
        if (isFirst && !timerStarted)
        {
            startTime = Time.time;
            timerStarted = true;
        }

        if (isLast && timerStarted && !timerEnded)
        {
            endTime = Time.time;
            timerEnded = true;
            WriteDataToFile();
        }
    }

    public void OnWrongStep()
    {
        if (timerStarted && !timerEnded)
        {
            errorCount++;
        }
    }

    void WriteDataToFile()
    {
        float totalTime = endTime - startTime;
        string line = $"Participant: {participantID}, Time: {totalTime:F2}, Errors: {errorCount}";
        File.AppendAllText(filePath, line + "\n");

        Debug.Log($"Data saved: {line}");
        Debug.Log("Saved to: " + filePath);
    }
    public void InitializeFilePath()
    {
        filePath = Application.persistentDataPath + "/StepData.txt";
    }

}
