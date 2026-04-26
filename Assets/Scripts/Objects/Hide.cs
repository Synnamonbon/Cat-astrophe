using UnityEngine;

public class Hide : MonoBehaviour, IInteractable
{
    public void Interact(int playerID)
    {
        Debug.Log("Hiding");
    }
}
