using System;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class ChaosManager : MonoBehaviourPun
{
    public static ChaosManager instance;
    // Tracks numeric value of Chaos Meter, will need to connect to UI eventually.
    // On scene start we must set the target value for chaos meter according to the number of players we have
    // Then we have to add an increase chaos meter function
    private int currentChaos = 0;
    private int targetChaos = 0;
    private int levelNumber = 0;
    private int numberOfPlayers = 0;
    private Dictionary<int, int> chaosContribution;
    // Task abstract class with event "Condition", int counter and int targetValue
    private Dictionary<int, List<Task>> playerTasks;
    private List<TaskDetails_SO> allTasks;

    // Subscribe to Object Manager's "SomethingBroke" Action with argument in playerID and EnumObjectType objectType
    public event Action PointsUpdated;
    public event Action<string, int> TaskAssigned;
    public event Action<string, int> TaskProgUpdated;
    public event Action<string> TaskComplete;
    public event Action<bool> GameWonEvent;

    private void Awake()
    {
        SingletonPattern();
    }

    private void OnLevelWasLoaded()
    {
        chaosContribution = new Dictionary<int, int>() {};
        playerTasks = new Dictionary<int, List<Task>>() {};
        SubscribeToChaosEvents();
        LoadAllTasks();
    }

    private void SingletonPattern()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    // Subscribe to any event that should be adding chaos points here
    private void SubscribeToChaosEvents()
    {
        InteractableManager.instance.OnBreakEvent += BreakPoints;
        NPCManager.instance.OnSaveCatEvent += SaveCatPoints;
        GameManager.instance.EndGame += EndGameEvent;
        GameManager.instance.InteractForTask += InteractChaos;
        GameManager.instance.PawForTask += PawChaos;
        GameManager.instance.MeowForTask += MeowChaos;
        InteractableManager.instance.OnDraggedFarEvent += DragChaos;
    }

    private void LoadAllTasks()
    {
        allTasks = new List<TaskDetails_SO>(Resources.LoadAll<TaskDetails_SO>("Tasks"));
    }

    private void GenerateTaskIndexesForPlayer(int playerID, int number)
    {
        // When task is randomly generated here, RPC to all users to generate the same task
        List<int> ints = new List<int>();
        for (int i = 0; i < number; i++)
        {
            int j = UnityEngine.Random.Range(0, allTasks.Count);
            if (!ints.Contains(j))
            {
                ints.Add(j);
                photonView.RPC(nameof(AssignTaskAtIndexToPlayer), RpcTarget.AllBuffered, j, playerID);
            }
            else
            {
                i--;
            }
        }
    }

    [PunRPC]
    public void AddPlayer(int playerID)
    {
        chaosContribution.Add(playerID, 0);
    }

    public void RemovePlayer(int playerID)
    {
        // Remove from chaos contrib when dc
        // Lower target
    }

    [PunRPC]
    private void AssignTaskAtIndexToPlayer(int idx, int playerID)
    {
        if (!playerTasks.ContainsKey(playerID))
        {
            playerTasks.Add(playerID, new List<Task>());
        }
        Task t = new Task();
        t.InitTask(playerID, allTasks[idx]);
        playerTasks[playerID].Add(t);
        t.CompleteTaskEvent += ScoreTask;
        t.UpdateProgressEvent += UpdateTaskDelegate;
        //Debug.Log("Added task " + t.taskName + " to " + playerID);
        if (PhotonNetwork.LocalPlayer.ActorNumber == playerID)
        {
            // Invoke assign task event
            TaskAssigned?.Invoke(t.taskName, t.target);
        }
    }

    [PunRPC]
    public void InitChaos(int players)
    {
        numberOfPlayers = players;
        currentChaos = 0;
        targetChaos = 600 * numberOfPlayers;
        // Master client assigns tasks to everyone, RPC call to them specifically to receive their tasks?
        if (PhotonNetwork.IsMasterClient)
        {
            playerTasks = new Dictionary<int, List<Task>>() {};
            // Get all players in room
            Player[] allPlayers = PhotonNetwork.PlayerList;
            // Generate random tasks for each player based on number of players
            foreach (Player player in allPlayers)
            {
                int playerID = player.ActorNumber;
                GenerateTaskIndexesForPlayer(playerID, 5);
            }
        }
    }

    private void BreakPoints(int playerID, ObjectType objectType, Vector3 objectPosition, string tag)
    {
        // Update points according to object type
        int pts = ChaosDictionary.GetPointsForEvent(objectType);
        instance.photonView.RPC(nameof(instance.PlayerScorePoints), RpcTarget.AllBuffered, playerID, pts);
        // Check for tasks requiring BreakEvent
        // for loop through tasks, check their conditions, if the condition is break and the tag matches, call increment
        foreach (Task t in playerTasks[playerID])
        {
            if (t.conditionTrack == ConditionTrack.BreakObject)
            {
                if (t.conditionTag == tag || t.conditionTag == "Any")
                {
                    t.IncrementCondition();
                }
            }
        }
    }

    private void InteractChaos(int playerID, string tag)
    {
        foreach (Task t in playerTasks[playerID])
        {
            if (t.conditionTrack == ConditionTrack.InteractObject)
            {
                if (t.conditionTag == tag || t.conditionTag == "Any")
                {
                    t.IncrementCondition();
                }
            }
        }
    }

    private void PawChaos(int playerID, string tag)
    {
        foreach (Task t in playerTasks[playerID])
        {
            if (t.conditionTrack == ConditionTrack.PawObject)
            {
                if (t.conditionTag == tag || t.conditionTag == "Any")
                {
                    t.IncrementCondition();
                }
            }
        }
    }

    private void DragChaos(int playerID, string tag)
    {
        foreach (Task t in playerTasks[playerID])
        {
            if (t.conditionTrack == ConditionTrack.Distance)
            {
                if (t.conditionTag == tag || t.conditionTag == "Any")
                {
                    t.IncrementCondition();
                }
            }
        }
    }

    private void MeowChaos(int playerID, string tag)
    {
        foreach (Task t in playerTasks[playerID])
        {
            if (t.conditionTrack == ConditionTrack.MeowDistance)
            {
                if (t.conditionTag == tag || t.conditionTag == "Any")
                {
                    t.IncrementCondition();
                }
            }
        }
    }

    private void SaveCatPoints(int playerID)
    {
        instance.photonView.RPC(nameof(instance.PlayerScorePoints), RpcTarget.AllBuffered, playerID, ChaosDictionary.GetPointsForEvent("CatSave"));
    }

    [PunRPC]
    private void PlayerScorePoints(int playerID, int pts)
    {
        IncreaseCurrent(pts);
        if (!chaosContribution.ContainsKey(playerID))
        {
            AddPlayer(playerID);
        }
        chaosContribution[playerID] += pts;
        //Debug.Log("New player contribuion: " + playerID + " - " + chaosContribution[playerID]);
    }

    private void UpdateTaskDelegate(string title, int newProg)
    {
        TaskProgUpdated?.Invoke(title, newProg);
    }

    private void ScoreTask(Task task)
    {
        // get points from the task for the player.
        if (task.playerID == PhotonNetwork.LocalPlayer.ActorNumber)
        {
            //Debug.Log("Score Task " + task.taskName);
            instance.photonView.RPC(nameof(instance.PlayerScorePoints), RpcTarget.AllBuffered, task.playerID, ChaosDictionary.GetPointsForEvent(task.taskType));
            TaskComplete?.Invoke(task.taskName);
        }
    }

    private bool CheckWinCon()
    {
        return currentChaos >= targetChaos;
    }

    private void EndGameEvent()
    {
        GameWonEvent?.Invoke(CheckWinCon());
    }

    [PunRPC]
    private void IncreaseCurrent(int pts)
    {
        currentChaos += pts;
        // Invoke Updated points event
        PointsUpdated?.Invoke();
    }

    public float GetCurrChaos()
    {
        return currentChaos;
    }

    public float GetTargetChaos()
    {
        return targetChaos;
    }
}
