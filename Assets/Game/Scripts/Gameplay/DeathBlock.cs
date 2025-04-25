using UnityEngine;

public class DeathBlock : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            GameplayEvents.GameResetEvent();
        }
    }
}
