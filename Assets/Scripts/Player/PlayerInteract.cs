using System;
using Photon.Pun;
using UnityEngine;

public interface IInteractable
{
    public void Interact(GameObject go);
}

public class PlayerInteract : MonoBehaviour
{
    [SerializeField] private float range = 30f;

    private QuickOutline currentOutline;
    private GameObject currentTarget;

    public event Action<int, string> InteractWith;     //playerID, tag

    private void OnEnable()
    {
        InputManager.instance.OnInteract += InteractAction;
    }

    private void OnDisable()
    {
        InputManager.instance.OnInteract -= InteractAction;
        ClearHighlight();
    }

    private void Update()
    {
        CheckHighlight();
    }

   private void CheckHighlight()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Ray r = cam.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));

        if (Physics.Raycast(r, out RaycastHit hitInfo, range))
        {
            GameObject go = hitInfo.collider.gameObject;
            if (go != currentTarget)
            {
                ClearHighlight();
                currentTarget = go;

                var interactable = go.GetComponentInParent<IInteractable>();
                if (interactable != null)
                {
                    GameObject outlineTarget = (interactable as MonoBehaviour).gameObject;
                    try
                    {
                        currentOutline = outlineTarget.AddComponent<QuickOutline>();
                        currentOutline.OutlineColor = Color.white;
                        currentOutline.OutlineWidth = 5f;
                    }
                    catch
                    {
                        if (currentOutline != null) Destroy(currentOutline);
                        currentOutline = null;
                    }
                }
            }
        }
        else
        {
            ClearHighlight();
            currentTarget = null;
        }
    }
private void ClearHighlight()
{
    if (currentOutline != null)
    {
        Destroy(currentOutline);
        currentOutline = null;
    }
}

    private void InteractAction(object sender, EventArgs e)
    {
        if (currentTarget != null && currentTarget.TryGetComponent(out IInteractable obj))
        {
            obj.Interact(gameObject);
            InteractWith?.Invoke(PhotonNetwork.LocalPlayer.ActorNumber, currentTarget.tag);
        }
    }
}