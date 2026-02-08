using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviourPunCallbacks
{
    [Header("Screens")]
    [SerializeField] private GameObject titleScreen;
    [SerializeField] private GameObject lobbyScreen;
    [SerializeField] private GameObject roomsListScreen;

    [Header("PopUp Windows")]
    [SerializeField] private GameObject createGameWindow;
    [SerializeField] private GameObject joinGameWindow; // Change later
    [SerializeField] private GameObject loginWindow; // No purpose right now

    [Header("Title Screen")]
    [SerializeField] private Button createRoomButton;
    [SerializeField] private Button joinRoomButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button loginButton;
    [SerializeField] private Button createAccountButton;
    [SerializeField] private Button creditsButton;

    [Header("Lobby Screen")]
    [SerializeField] private TextMeshProUGUI playerListText;
    [SerializeField] private Button startGameButton;

    Button test;

    private void Start()
    {
        createRoomButton.interactable = false;
        joinRoomButton.interactable = false;
    }

    public override void OnConnectedToMaster()
    {
        createRoomButton.interactable = true;
        joinRoomButton.interactable = true;
    }

    private void SetScreen(GameObject Screen)
    {
        titleScreen.SetActive(false);
        lobbyScreen.SetActive(false);
        roomsListScreen.SetActive(false);

        Screen.SetActive(false);
    }

    public void ChangePopupWindowState(GameObject window, bool changeState)
    {
        // Open/Close popup windows and disable/enable main menu buttons respectively
        createRoomButton.interactable = changeState;
        joinRoomButton.interactable = changeState;
        settingsButton.interactable = changeState;
        quitButton.interactable = changeState;
        loginButton.interactable = changeState;
        createAccountButton.interactable = changeState;
        creditsButton.interactable = changeState;

        window.SetActive(!changeState);
    }

    public void OnCreateRoomButton (TMP_InputField roomNameInput)
    {
        NetworkManager.instance.CreateRoom(roomNameInput.text);
    }

    public void OnJoinRoomButton (TMP_InputField roomNameInput)
    {
        // Change this whole method when done implementing login and room list, it should be displaying room list later
        NetworkManager.instance.JoinRoom(roomNameInput.text);
    }

    public void OnPlayerNameUpdate(TMP_InputField playerNameInput) // IMPORTANT: replace this once username/password backend is working
    {
        PhotonNetwork.NickName = playerNameInput.text;
    }

    public override void OnJoinedRoom()
    {
        SetScreen(lobbyScreen);
        photonView.RPC("UpdateLobbyUI", RpcTarget.All);
    }

    [PunRPC]
    public void UpdateLobbyUI()
    {
        playerListText.text = "";

        foreach(Player player in PhotonNetwork.PlayerList)
        {
            playerListText.text += player.NickName + "\n";
        }

        // Host only
        if (PhotonNetwork.IsMasterClient)
        {
            startGameButton.interactable = true;
        }
        else
        {
            startGameButton.interactable = false;
        }
    }

    public void OnLeaveLobbyButton()
    {
        PhotonNetwork.LeaveRoom();
        SetScreen(titleScreen);
    }

    public void OnStartGameButton()
    {
        NetworkManager.instance.photonView.RPC("ChangeScrene", RpcTarget.All, "Game");
    }

}
