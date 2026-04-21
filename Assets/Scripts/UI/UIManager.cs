using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Slider hungerSlider;
    [SerializeField] private Slider chaosSlider;
    private PlayerHunger playerHunger;
    private float maxHunger;

    private void Start()
    {
        ChaosManager.instance.PointsUpdated += UpdateChaos;
    }

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

    private void UpdateChaos()
    {
        chaosSlider.value = ChaosManager.instance.GetCurrChaos() / ChaosManager.instance.GetTargetChaos();
    }

    public void AssignPlayerHungerUI(PlayerHunger player)
    {
        playerHunger = player;
        maxHunger = playerHunger.InitialiseHunger();
        Debug.Log("max hunger: " + maxHunger);
        Debug.Log("current hunger: " + playerHunger.currentHunger);
    }
}
