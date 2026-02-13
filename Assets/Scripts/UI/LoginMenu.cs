using Photon.Pun;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LoginMenu : MonoBehaviour
{
    [SerializeField] private MainMenu mainMenu;
    [SerializeField] private GameObject displayNamePopup;
    [SerializeField] private GameObject buttons;


    public void EnableDisplayNamePopup()
    {
        displayNamePopup.SetActive(true);
        buttons.SetActive(false);
    }

    public void SetDisplayName(TMP_InputField displayNameInput)
    {
        PhotonNetwork.NickName = displayNameInput.text;
        Debug.Log("Display name set to " + PhotonNetwork.NickName);
        mainMenu.OpenTitleMenu();
    }
}
