using UnityEngine;

public class InteractBreak : MonoBehaviour, IInteractable
{
    private BreakableObject boRef;

    private void Start()
    {
        boRef = GetComponent<BreakableObject>();
    }

    public void Interact(GameObject go)
    {
        boRef.Explode();
    }
}
