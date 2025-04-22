using System.ComponentModel;
using Unity.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Gameplay Stats")]
    public int obstacleHits = 0;
    public int lilyPadTimeouts = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // prevent duplicates
        }
        else
        {
            Instance = this;
        }
    }

    public void RegisterObstacleHit()
    {
        obstacleHits++;
        Debug.Log($"Obstacle hit! Total hits: {obstacleHits}");
    }

    public void RegisterLilyPadTimeout()
    {
        lilyPadTimeouts++;
        Debug.Log($"Lily pad timeout! Total timeouts: {lilyPadTimeouts}");
    }
}
