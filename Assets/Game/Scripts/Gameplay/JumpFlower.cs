using UnityEngine;

public class JumpFlower : MonoBehaviour
{
    [SerializeField]
    private GameObject collectVFXPrefab;

    private void Start()
    {
        GameManager.Instance.AddTree();
    }

    private void Update()
    {
        if ((PlayerInteractions.Instance.transform.position - transform.position).magnitude < 2.5f)
        {
            GameplayEvents.CutTreeEvent(gameObject);
            PlayerInteractions.Instance.LandedOnFlower();
            Lean.Pool.LeanPool.Spawn(collectVFXPrefab, transform.position, Quaternion.identity);
            gameObject.SetActive(false);
        }
    }
}
