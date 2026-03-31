using Photon.Pun;
using UnityEngine;

public class PlayerHunger : MonoBehaviourPun
{
    [SerializeField] private float maxHunger = 60f;
    [SerializeField] private float hungerPerSecond = 1f;
    [SerializeField] private float hungerSprintingPerSecond = 2f;

    [HideInInspector] public float currentHunger {get; private set;}

    private void Start()
    {
        currentHunger = maxHunger;
    }

    private void Update()
    {
        if (!photonView.IsMine) return;
        float drain = InputManager.instance.isSprinting ? hungerSprintingPerSecond : hungerPerSecond;

        currentHunger -= drain * Time.deltaTime;
        currentHunger = Mathf.Clamp(currentHunger, 0f, maxHunger);
        //Debug.Log(currentHunger);
    }

    [PunRPC]
    public void RestoreHunger(float value)
    {
        currentHunger += value;
        Mathf.Clamp(currentHunger, 0f, maxHunger);
        Debug.Log("Player hunger restored to "+ currentHunger);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine) return;

        if (other.TryGetComponent<FoodData>(out FoodData food))
        {
            food.photonView.RPC(nameof(food.RequestConsume), RpcTarget.MasterClient, photonView.ViewID);
        }
    }
}
