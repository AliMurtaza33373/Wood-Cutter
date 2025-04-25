using UnityEngine;

public class Laser : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameplayEvents.PlayerGotHitEvent(1, transform.position);
        }
    }
}
