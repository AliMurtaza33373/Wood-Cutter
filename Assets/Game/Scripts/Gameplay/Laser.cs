using UnityEngine;

public class Laser : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameplaySoundManager.Instance.PlayGetHit();
            GameplayEvents.PlayerGotHitEvent(1, transform.position);
        }
    }
}
