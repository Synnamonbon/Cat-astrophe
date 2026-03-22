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
        Debug.Log(currentHunger);
    }

    void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("Food"))
        {
            currentHunger += 20f;
            Mathf.Clamp(currentHunger, 0f, maxHunger);
            Destroy(collision.gameObject);
        }
    }

}
