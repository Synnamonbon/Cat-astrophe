using System.Collections;
using Photon.Pun;
using UnityEngine;

public class PlayerHunger : MonoBehaviourPun
{
    [SerializeField] private float maxHunger = 60f;
    [SerializeField] private float hungerPerSecond = 1f;
    [SerializeField] private float hungerSprintingPerSecond = 2f;
    [SerializeField] private float eatCD = 0.1f;
    [Header("Low hunger settings")]
    [SerializeField] private float hungryMeowRate50 = 5f; // Second intervals where meow check is done at 50% hunger
    [SerializeField] private float hungryMeowRate25 = 3.33f; // Second intervals where meow check is done at 25% hunger
    [SerializeField] private float hungryMeowChance50 = 0.33f;
    [SerializeField] private float hungryMeowChance25 = 0.5f;
    [SerializeField] private float hungryToyDistractThreshold = 0.8f;

    private PlayerController playerController;

    [HideInInspector] public float currentHunger {get; private set;}
    private bool canEat = true;
    private Coroutine distractCoroutine;

    private void OnEnable()
    {
        playerController = GetComponent<PlayerController>();
        StartCoroutine(HungryMeowRoutine());
    }

    private void Update()
    {
        if (!photonView.IsMine) return;
        float drain = InputManager.instance.isSprinting ? hungerSprintingPerSecond : hungerPerSecond;

        currentHunger -= drain * Time.deltaTime;
        currentHunger = Mathf.Clamp(currentHunger, 0f, maxHunger);

        CheckDebuffs();
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
        if (!canEat) return;
        if (other.TryGetComponent<FoodData>(out FoodData food))
        {
            StartCoroutine(EatFood(food));
        }
    }

    private IEnumerator EatFood(FoodData food)
    {
        canEat = false;
        food.photonView.RPC(nameof(food.RequestConsume), RpcTarget.MasterClient, photonView.ViewID);
        yield return new WaitForSeconds(eatCD);
        canEat = true;
    }

    public float InitialiseHunger()
    {
        currentHunger = maxHunger;
        return maxHunger;
    }

    private void CheckDebuffs()
    {
        if (distractCoroutine == null && currentHunger < maxHunger * hungryToyDistractThreshold)
        {
            distractCoroutine = StartCoroutine(ToyDistractableCoroutine());
        }
    }

    private IEnumerator HungryMeowRoutine()
    {
        float meowInterval;
        float meowChance;
        while (true) {
            if (currentHunger > (maxHunger * 0.5)) yield return null;
            else
            {
                if (currentHunger <= (maxHunger * 0.25)) {
                    meowInterval = hungryMeowRate25;
                    meowChance = hungryMeowChance25;
                }
                else {
                    meowInterval = hungryMeowRate50;
                    meowChance = hungryMeowChance50;
                }
                //Debug.Log("Chance to meow: " + meowChance);
                HungryMeow(meowChance);
                yield return new WaitForSeconds(meowInterval);
            }
        }
    }

    private void HungryMeow(float meowChance)
    {
        if (Random.value < meowChance)
        {
            playerController.MakePlayerMeow();
        }
    }

    private IEnumerator ToyDistractableCoroutine()
    {
        playerController.isDistractable = true;
        yield return new WaitUntil(() => currentHunger >= (maxHunger*hungryToyDistractThreshold));
        playerController.isDistractable = false;
        distractCoroutine = null;
    }
}
