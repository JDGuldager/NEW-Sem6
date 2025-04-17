using UnityEngine;

public class StepPlatform : MonoBehaviour
{
    public bool isCorrect = true;
    public bool isFirst = false;
    public bool isLast = false;

    private DataSaver dataSaver;

    private void Start()
    {
        dataSaver = GameObject.Find("Scripts").GetComponent<DataSaver>();
    }
    private void OnTriggerEnter(Collider other)
    {
        // You can check for player tag or VR rig component here
        if (other.CompareTag("Player"))
        {
            if (isCorrect)
            {
                dataSaver.OnCorrectStep(isFirst, isLast);
            }
            else
            {
               dataSaver.OnWrongStep();
            }
        }
    }
}
