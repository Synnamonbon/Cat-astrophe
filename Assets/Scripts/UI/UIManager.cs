using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Slider hungerSlider;
    [SerializeField] private Slider chaosSlider;
    [SerializeField] private TMP_Text timer;
    private PlayerHunger playerHunger;
    private float maxHunger;
    private float timerRemaining = 0f;

    private void OnLevelWasLoaded()
    {
        ChaosManager.instance.PointsUpdated += UpdateChaos;
        GameManager.instance.SetTimer += SetTimer;
    }

    private void Update()
    {
        UpdateHunger();
    }

    private void FixedUpdate()
    {
        UpdateTimer();
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

    private void UpdateTimer()
    {
        if (timerRemaining != 0f)
        {
            timerRemaining -= Time.fixedDeltaTime;
            timer.text = ((int)(timerRemaining / 60)).ToString() + ":" + ((int)(timerRemaining % 60)).ToString();
            //Debug.Log("Update timer!");
        }
        else
        {
            //Debug.Log("NotSet");
        }
    }

    public void AssignPlayerHungerUI(PlayerHunger player)
    {
        playerHunger = player;
        maxHunger = playerHunger.InitialiseHunger();
        //Debug.Log("max hunger: " + maxHunger);
        //Debug.Log("current hunger: " + playerHunger.currentHunger);
    }

    private void SetTimer(float remaining)
    {
        timerRemaining = remaining;
    }
}
