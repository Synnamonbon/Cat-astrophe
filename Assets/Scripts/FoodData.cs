using UnityEngine;

public class FoodData : MonoBehaviour
{
    [SerializeField] private Food_SO foodData;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerHunger>(out PlayerHunger playerHunger))
        {
            playerHunger.RestoreFood(foodData.HungerRestoreValue);
            Destroy(gameObject);
        }
    }
}
