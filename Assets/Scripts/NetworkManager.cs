using System;
using Photon.Pun;
using UnityEngine;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager instance;

    private void Awake()
    {
        SingletonPattern();
    }

    private void SingletonPattern()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnected()
    {
        Debug.Log("Connected");
    }
}
