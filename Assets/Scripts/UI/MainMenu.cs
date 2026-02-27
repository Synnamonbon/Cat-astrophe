using System;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviourPunCallbacks
{
    [Header("Menus")]
    [SerializeField] private GameObject loginMenu;
    [SerializeField] private GameObject titleMenu;
    [SerializeField] private GameObject lobbyMenu;
    [SerializeField] private GameObject roomMenu;

    [Header("Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button displayNameConfirmButton;
    [SerializeField] private Button startGameButton;

    [Header("Textboxes")]
    [SerializeField] private TextMeshProUGUI roomNameText;
    [SerializeField] private TextMeshProUGUI playerListText;

    public void Start()
    {
        playButton.interactable = false;
        displayNameConfirmButton.interactable = false;
    }

    public override void OnConnectedToMaster()
    {
        playButton.interactable = true;
        displayNameConfirmButton.interactable = true;
    }


    public void OnDisplayNameConfirmButton(TMP_InputField displayNameInput)
    {
        SessionManager.instance.SetDisplayName(displayNameInput.text);

        SetMenu(titleMenu);
    }

    public void OnTitleMenuPlayButton()
    {
        SetMenu(lobbyMenu);

        quitButton.gameObject.SetActive(false);
    }

    public void OpenTitleMenu()
    {
        SetMenu(titleMenu);

        quitButton.gameObject.SetActive(true);
    }

    private void SetMenu(GameObject menu)
    {
        loginMenu.SetActive(false);
        titleMenu.SetActive(false);
        lobbyMenu.SetActive(false);
        roomMenu.SetActive(false);

        menu.SetActive(true);
    }

    // Room Menu Scripts

    public override void OnJoinedRoom()
    {
        SetMenu(roomMenu);
        roomNameText.text = PhotonNetwork.CurrentRoom.Name;
        photonView.RPC("UpdateLobbyUI", RpcTarget.All);
        Debug.Log("Joined room " + PhotonNetwork.CurrentRoom.Name);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdateLobbyUI();
    }

    public void OnLeaveRoomButton()
    {
        PhotonNetwork.LeaveRoom();
        SetMenu(lobbyMenu);
    }

    public void OnStartGameButton()
    {
        if(NetworkManager.instance == null){Debug.Log("No network manager");}
        else {Debug.Log("Not null");}
        NetworkManager.instance.photonView.RPC("ChangeScene", RpcTarget.All, "Game");
    }
    
    [PunRPC]
    public void UpdateLobbyUI()
    {
        Debug.Log("UpdateLobbyUI called. Players: " + PhotonNetwork.PlayerList.Length);
        Debug.Log("playerListText null? " + (playerListText == null));

        playerListText.text = "";

        foreach(Player player in PhotonNetwork.PlayerList)
        {
            playerListText.text += player.NickName + "\n";
        }

        // Host Only
        if (PhotonNetwork.IsMasterClient)
        {
            startGameButton.interactable = true;
        }
        else
        {
            startGameButton.interactable = false;
        }
    }
}
