using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SessionDataManager))]
public class StepTrackerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SessionDataManager tracker = (SessionDataManager)target;

        GUILayout.Space(10);

        // Button: Open Save File Location
        if (GUILayout.Button("Open Save File Location"))
        {
            tracker.InitializeFilePath();

            if (!string.IsNullOrEmpty(tracker.filePath))
            {
                string folder = Path.GetDirectoryName(tracker.filePath);

                if (!string.IsNullOrEmpty(folder) && Directory.Exists(folder))
                {
                    Process.Start(folder);
                }
                else
                {
     
                }
            }
            else
            {
            
            }
        }

        // Button: Reset Participant ID
        if (GUILayout.Button("Reset Participant ID"))
        {
            PlayerPrefs.SetInt("LastParticipantID", 0);
            PlayerPrefs.Save();
  
        }
    }
}
