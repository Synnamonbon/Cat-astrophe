using Photon.Pun;
using UnityEngine;

public class SessionManager : MonoBehaviour
{
    public static SessionManager instance;
    private string displayName;
    [System.NonSerialized] public string session_token = "";
    [System.NonSerialized] public int uid = 0;

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

    public void SetDisplayName(string name)
    {
        displayName = name;
        PhotonNetwork.NickName = displayName;
        Debug.Log("Display name set to " + PhotonNetwork.NickName);
    }

    public string GetDisplayName()
    {
        return displayName;
    }

    public void SetUID(int uid)
    {
        instance.uid = uid;
        Debug.Log("UID set to " + uid);
    }

    public int GetUID()
    {
        return instance.uid;
    }

    public void SetSessionToken(string session_token)
    {
        instance.session_token = session_token;
        Debug.Log("Session Token updated");
    }

    public string GetSessionToken()
    {
        return instance.session_token;
    }
}
