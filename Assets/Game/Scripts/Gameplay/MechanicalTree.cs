using UnityEngine;

public class MechanicalTree : MonoBehaviour
{
    [SerializeField]
    private GameObject normalTree, destroyedTree;

    [SerializeField]
    private ParticleSystem cutParticleSystem;

    private bool cutDown;

    private void Start()
    {
        cutDown = false;
        GameManager.Instance.AddTree();
        normalTree.SetActive(true);
        destroyedTree.SetActive(false);
    }

    public void CutDownTree()
    {
        if (cutDown)
            return;

        GameplaySoundManager.Instance.PlaySlashTree();
        GetComponent<BoxCollider>().enabled = false;
        cutDown = true;
        GameplayEvents.CutTreeEvent(gameObject);
        cutParticleSystem.Play();
        normalTree.SetActive(false);
        destroyedTree.SetActive(true);
    }
}
