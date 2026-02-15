using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class StartMenu : MonoBehaviour
{
    [SerializeField] private GameObject displayNamePopup;
    [SerializeField] private GameObject buttons;
    [SerializeField] private GameObject LoginMenu;
    [SerializeField] private GameObject RegisterMenu;
    [SerializeField] private GameObject Self;


    public void EnableDisplayNamePopup()            // Currently only used for Guest Logins. Rework into Guest Login l8r
    {
        displayNamePopup.SetActive(true);
        buttons.SetActive(false);
    }

    public void NavigateLoginMenu()
    {
        LoginMenu.SetActive(true);
        Self.SetActive(false);
    }

    public void NavigateRegisterMenu()
    {
        RegisterMenu.SetActive(true);
        Self.SetActive(false);
    }
}
