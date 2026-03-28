using UnityEngine;

public class PlayerHunger : MonoBehaviour
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
        float drain = InputManager.instance.isSprinting ? hungerSprintingPerSecond : hungerPerSecond;

        currentHunger -= drain * Time.deltaTime;
        currentHunger = Mathf.Clamp(currentHunger, 0f, maxHunger);
        //Debug.Log(currentHunger);
    }

    private void RestoreHunger(float value)
    {
        currentHunger += value;
        Mathf.Clamp(currentHunger, 0f, maxHunger);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<FoodData>(out FoodData food))
        {
            if (food.isUsed) return;
            RestoreHunger(food.Consume());
            food.DestroyFood();
        }
    }

}
