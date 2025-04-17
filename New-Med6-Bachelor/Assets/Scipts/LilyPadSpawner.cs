using System;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class LilyPadSpawner : MonoBehaviour
{

    public GameObject[] Route11;
    public GameObject[] Route12;
    public GameObject[] Route13;
    private GameObject[] routeToSpawn;
    public bool readyForRouteSpawn;
    public bool isReturningToStart;
    private bool difficultyCompleted = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        isReturningToStart = false;
        readyForRouteSpawn = true;
        routeToSpawn = Route11;
    }

    // Update is called once per frame
    void Update()
    {
        if (readyForRouteSpawn == true)
        {
            SpawnNextRoute();
        }
        
    }

    private void OnEnable()
    {
        LilyPadBehavior.OnSteppedOn += OnLilyPadSteppedOn;
        StartPad.SteppedOnStart += OnStartPadSteppedOn;
    }

    public void SpawnNextRoute()
    {
        if (routeToSpawn != null)
        { 
        routeToSpawn[0].SetActive(true);
        readyForRouteSpawn = false;
        }
        else if (!difficultyCompleted)
        {
            Debug.Log("Difficulty Completed");
            difficultyCompleted = true;
        }

    }

    private void OnStartPadSteppedOn(StartPad startPad)
    {
        if (isReturningToStart)
        {
            Debug.Log("Route Completed");
            routeToSpawn[0].SetActive(false);
            isReturningToStart = false;

            // Assign the next route or set to null if Route13 was the last one
            routeToSpawn = routeToSpawn == Route11 ? Route12
                          : routeToSpawn == Route12 ? Route13
                          : null;

            readyForRouteSpawn = true;
        }
        else
        {
            Debug.Log("Heading Out");
        }
    }



    private void OnLilyPadSteppedOn(LilyPadBehavior lilyPadBehavior)
    {              
            Debug.Log("LilyPad " + lilyPadBehavior.tag + " stepped on");

            if (lilyPadBehavior.gameObject.CompareTag("1"))
            {                        
              routeToSpawn[1].SetActive(!isReturningToStart);            
            }

            else if (lilyPadBehavior.gameObject.CompareTag("2"))
            {
            routeToSpawn[2].SetActive(!isReturningToStart);
            }
            
            else if (lilyPadBehavior.gameObject.CompareTag("3"))
            {
                Debug.Log("Returning to start");
                isReturningToStart = true;
            }
    }
}



 
    

