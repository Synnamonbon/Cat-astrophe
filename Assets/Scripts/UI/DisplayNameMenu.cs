using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.Networking;
using System.Text;
using Unity.Collections;

public class DisplayNameMenu : MonoBehaviour
{
    [SerializeField] private TMP_InputField display_input;
    [SerializeField] private GameObject StartMenu;
    [SerializeField] private GameObject TitleMenu;
    [SerializeField] private GameObject Self;
    [SerializeField] private SessionManager SessionManager;             // Stores uid and session token.

    // Try to get pref name for logged user.
    void OnEnable()
    {
        display_input.text = "";
        StartCoroutine(GetUserPrefName());
    }

    private IEnumerator GetUserPrefName()
    {
        string json = $"{{\"uid\":\"{SessionManager.GetUID()}\"}}";
        string url = AWSConfig.GetPreferredNameURL(SessionManager.GetUID());

        UnityWebRequest request = new UnityWebRequest(url, "GET");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Authorization", $"Bearer {SessionManager.GetSessionToken()}");
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Response: " + request.downloadHandler.text);
            DisplayName response = JsonUtility.FromJson<DisplayName>(request.downloadHandler.text);
            display_input.text = response.display_name;
        }
        else
        {
            Debug.Log("Error: " + request.error);
        }
    }

    // Sets own preferred display name or active display name 
    private IEnumerator SetUserDisplayName(bool prefFlag, string disp_name)
    {
        string json = $"{{\"display_name\":\"{disp_name}\", \"uid\":\"{SessionManager.GetUID()}\"}}";
        string url;
        if(prefFlag)
        {
            url = AWSConfig.SetPreferredNameURL(SessionManager.GetUID());
        } 
        else
        {
            url = AWSConfig.SetDisplayNameURL(SessionManager.GetUID());
        }
        
        UnityWebRequest request = new UnityWebRequest(url, "PUT");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Authorization", $"Bearer {SessionManager.GetSessionToken()}");
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Response: " + request.downloadHandler.text);
        }
        else
        {
            Debug.Log("Error: " + request.error);
        }
    }

    public void SetDisplayName()
    {
        string name = display_input.text;
        StartCoroutine(SetUserDisplayName(false, name));
        if (SessionManager.GetUID() > 0)        // uid 0 -> not logged in; uid < 0 -> guest login
        {                                       // As such set the preferred name to the current selected name
            Debug.Log("Logged In");
            StartCoroutine(SetUserDisplayName(true, name));
        }
        SessionManager.SetDisplayName(name);
        NavigateTitleMenu();
    }

    public void NavigateTitleMenu()
    {
        TitleMenu.SetActive(true);
        Self.SetActive(false);
    }

    public void NavigateStartMenu()
    {
        StartMenu.SetActive(true);
        // Counts as a Log out
        SessionManager.SetSessionToken("");
        SessionManager.SetUID(0);
        Self.SetActive(false);
    }
}
