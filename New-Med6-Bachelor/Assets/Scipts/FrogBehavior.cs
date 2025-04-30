using UnityEngine;

public class FrogBehavior : MonoBehaviour
{
    public LilyPadSpawner LilyPadSpawnerScript;

    Vector3 currentPos;
    Vector3 targetPosition;
    [SerializeField] private float speed = 1.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        currentPos = transform.position;
       targetPosition = LilyPadSpawnerScript.AllRoutes[LilyPadSpawnerScript.currentRouteIndex].pads[LilyPadSpawnerScript.currentStepIndex].transform.position;
        LerpFrog();
    }
    public void LerpFrog()
    {
        Vector3.Slerp(currentPos, targetPosition, Time.deltaTime * speed);
    }
}
