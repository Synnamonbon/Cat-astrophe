using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LoginMenu : MonoBehaviour
{
    [SerializeField] private GameObject displayNamePopup;
    [SerializeField] private GameObject buttons;


    public void EnableDisplayNamePopup()
    {
        displayNamePopup.SetActive(true);
        buttons.SetActive(false);
    }
}
