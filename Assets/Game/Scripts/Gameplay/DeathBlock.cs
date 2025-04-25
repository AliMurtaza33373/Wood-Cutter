using UnityEngine;

public class DeathBlock : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            GameplaySoundManager.Instance.PlayDie();
            GameplayEvents.GameResetEvent();
        }
    }
}
