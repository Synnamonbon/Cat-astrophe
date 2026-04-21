using System;
using System.Collections.Generic;
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

    // Subscribe to Object Manager's "SomethingBroke" Action with argument in playerID and EnumObjectType objectType
    public event Action PointsUpdated;

    private void Awake()
    {
        chaosContribution = new Dictionary<int, int>() {};
        SingletonPattern();
        SubscribeToChaosEvents();
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
    }

    [PunRPC]
    public void AddPlayer(int playerID)
    {
        chaosContribution.Add(playerID, 0);
    }

    public void RemovePlayer(int playerID)
    {
        // Remove from chaos contrib when dc
    }

    [PunRPC]
    public void InitChaosTarget(int players)
    {
        numberOfPlayers = players;
        currentChaos = 0;
        targetChaos = 600 * numberOfPlayers;
    }

    private void BreakPoints(int playerID, ObjectType objectType)
    {
        // Update points according to object type
        int pts = ChaosDictionary.GetPointsForEvent(objectType);
        PlayerScorePoints(playerID, pts);
        // Check for tasks requiring BreakEvent
    }

    private void PlayerScorePoints(int playerID, int pts)
    {
        instance.photonView.RPC(nameof(instance.IncreaseCurrent), RpcTarget.AllBuffered, pts);
        if (!chaosContribution.ContainsKey(playerID))
        {
            AddPlayer(playerID);
        }
        chaosContribution[playerID] += pts;
        Debug.Log("New player contribuion: " + playerID + " - " + chaosContribution[playerID]);
        if (CheckWinCon())
        {
            Debug.Log("Points Reached!");
        }
    }

    private bool CheckWinCon()
    {
        return currentChaos >= targetChaos;
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
