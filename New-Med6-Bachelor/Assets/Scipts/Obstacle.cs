using UnityEngine;

public class Obstacle : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("LeftFoot") || other.CompareTag("RightFoot"))
        {
            GameManager.Instance?.RegisterObstacleHit();

            // Optional: FX or sound here
            Destroy(gameObject);
        }
    }
}
