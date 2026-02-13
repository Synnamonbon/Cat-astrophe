using Photon.Pun;
using UnityEngine;

public class SessionManager : MonoBehaviour
{
    public static SessionManager instance;
    private string displayName;

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

}
