using UnityEngine;

public class FrogBehavior : MonoBehaviour
{
    public LilyPadSpawner LilyPadSpawnerScript;

    
    Vector3 targetPosition;
    [SerializeField] private float speed = 1.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        targetPosition = new Vector3
            (
            LilyPadSpawnerScript.CurrentRoute.pads[LilyPadSpawnerScript.currentStepIndex].transform.position.x,
            LilyPadSpawnerScript.CurrentRoute.pads[LilyPadSpawnerScript.currentStepIndex].transform.position.y +0.14f,
            LilyPadSpawnerScript.CurrentRoute.pads[LilyPadSpawnerScript.currentStepIndex].transform.position.z
            );

        if (LilyPadSpawnerScript.CurrentRoute.pads[LilyPadSpawnerScript.currentStepIndex].transform.position.y >= -0.5f)
        {
            LerpFrog();
        }
    }
    public void LerpFrog()
    {
        transform.position = Vector3.Slerp(transform.position, targetPosition, Time.deltaTime * speed);
    }
}
