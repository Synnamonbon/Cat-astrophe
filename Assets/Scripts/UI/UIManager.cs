using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Slider hungerSlider;
    private PlayerHunger playerHunger;
    private float maxHunger;

    private void Update()
    {
        UpdateHunger();
    }

    private void UpdateHunger()
    {
        if (playerHunger!= null)
        {
            hungerSlider.value = playerHunger.currentHunger/maxHunger;
        }
    }

    public void AssignPlayerHungerUI(PlayerHunger player)
    {
        playerHunger = player;
        maxHunger = playerHunger.InitialiseHunger();
        //Debug.Log("max hunger: " + maxHunger);
        //Debug.Log("current hunger: " + playerHunger.currentHunger);
    }
}
