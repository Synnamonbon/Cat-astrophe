using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Slider hungerSlider;
    [SerializeField] private Slider chaosSlider;
    [SerializeField] private TMP_Text timer;
    [SerializeField] private GameObject taskList;
    private PlayerHunger playerHunger;
    private float maxHunger;
    private float timerRemaining = 0f;
    private Dictionary<string, int> tasksProgress;

    private void OnLevelWasLoaded()
    {
        ChaosManager.instance.PointsUpdated += UpdateChaos;
        ChaosManager.instance.TaskAssigned += AssignTaskToPlayerUI;
        ChaosManager.instance.TaskProgUpdated += UpdateTaskUI;
        ChaosManager.instance.TaskComplete += CompleteTask;
        GameManager.instance.SetTimer += SetTimer;
        tasksProgress = new Dictionary<string, int>();
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

    public void AssignTaskToPlayerUI(string title, int target)
    {
        // check length of Assigned Tasks isnt above 5
        if (tasksProgress.Count < 5 && !tasksProgress.ContainsKey(title))
        {
            // Set text at curr length to be "title 0 / target"
            int idx = tasksProgress.Count;
            TMP_Text text = GetTaskTextAtIDX(idx);
            // append 0 to end of taskProgress list
            tasksProgress.Add(title, 0);
            if (text != null)
            {
                TextFormatter(text, title, target.ToString());
            }
        }
    }

    private TMP_Text GetTaskTextAtIDX(int idx)
    {
        GameObject textContainer = taskList.transform.GetChild(idx).gameObject;
        if (textContainer.TryGetComponent<TMP_Text>(out TMP_Text text))
        {
            return text;
        }
        return null;
    }

    private void TextFormatter(TMP_Text text, string title, string target)
    {
        text.text = $"{title} {tasksProgress[title]}/{target}";
    }

    private void UpdateTaskUI(string title, int newProg)
    {
        // Update UI to reflect new progress. Keep last element for target
        foreach(Transform child in taskList.transform)
        {
            if (child.TryGetComponent<TMP_Text>(out TMP_Text text))     // Get the TMP element from child
            {
                if(text.text.Contains(title))                           // Ensure its got the right title
                {
                    char target = text.text.Substring(text.text. Length - 1, 1)[0];
                    tasksProgress[title] = newProg;
                    TextFormatter(text, title, target.ToString());
                }
            }
        }
    }

    private void CompleteTask(string title)
    {
        foreach(Transform child in taskList.transform)
        {
            if (child.TryGetComponent<TMP_Text>(out TMP_Text text))     // Get the TMP element from child
            {
                if(text.text.Contains(title))
                {
                    text.text = "<s>" + text.text + "<s>";
                }
            }
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
