using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.Networking;
using System.Text;

public class RegisterMenu : MonoBehaviour
{
    [SerializeField] private TMP_InputField username_input;
    [SerializeField] private TMP_InputField password_input;
    [SerializeField] private GameObject StartMenu;
    [SerializeField] private GameObject LoginMenu;
    [SerializeField] private GameObject Self;
    [SerializeField] private SessionManager SessionManager;             // Stores uid and session token.

    // Clear fields when the Menu is enabled.
    void OnEnable()
    {
        username_input.text = "";
        password_input.text = "";
    }

    // Registers a new user into the "registered_users" database
    private IEnumerator RegisterUser(string username, string password)
    {
        string json = $"{{\"username\":\"{username}\", \"password\":\"{password}\"}}";
        string url = AWSConfig.GetRegisterURL();

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Response: " + request.downloadHandler.text);
            NavigateLoginMenu();
        } 
        else
        {
            Debug.Log("Error: " + request.error);
            Debug.Log(request.downloadHandler.text);
        }
    }

    public void Register()
    {
        string username = username_input.text;
        string password = password_input.text;
        StartCoroutine(RegisterUser(username, password));
    }

    public void NavigateStartMenu()
    {
        StartMenu.SetActive(true);
        Self.SetActive(false);
    }

    public void NavigateLoginMenu()
    {
        LoginMenu.SetActive(true);
        Self.SetActive(false);
    }
}
