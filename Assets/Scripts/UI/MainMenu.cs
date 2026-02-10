using System;
using Photon.Pun;
using TMPro;
using Unity.VisualScripting;
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

    public void Start()
    {
        playButton.interactable = false;
    }

    public override void OnConnectedToMaster()
    {
        playButton.interactable = true;
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

    public void OpenRoomMenu()
    {
        SetMenu(roomMenu);
    }

    private void SetMenu(GameObject menu)
    {
        loginMenu.SetActive(false);
        titleMenu.SetActive(false);
        lobbyMenu.SetActive(false);
        roomMenu.SetActive(false);

        menu.SetActive(true);
    }
}
