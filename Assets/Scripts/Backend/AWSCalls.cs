using System;
using System.Collections;
using System.Text;
using TMPro;
using UnityEditor.Compilation;
using UnityEngine;
using UnityEngine.Networking;
// Serialisable classes
//using PlayerInfo;
//using DisplayName;          

public class AWSRegister : MonoBehaviour
{
    public TMP_InputField username_input;
    public TMP_InputField password_input;
    private string session_token = "";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

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
        } 
        else
        {
            Debug.Log("Error: " + request.error);
            Debug.Log(request.downloadHandler.text);
        }
    }

    private IEnumerator LoginUser(string username, string password)
    {
        string json = $"{{\"username\":\"{username}\", \"password\":\"{password}\"}}";
        string url = AWSConfig.GetLoginURL();

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Response: " + request.downloadHandler.text);
            PlayerInfo response = JsonUtility.FromJson<PlayerInfo>(request.downloadHandler.text);
            session_token = response.session_token;
        }
        else
        {
            Debug.LogError("Error: " + request.error);
            Debug.Log(request.downloadHandler.text);
        }
    }

    public IEnumerator GetPlayer()
    {
        // TODO implement
        return null;
    }

    public void Register()
    {
        string username = username_input.text;
        string password = password_input.text;
        StartCoroutine(RegisterUser(username, password));
    }

    public void Login()
    {
        string username = username_input.text;
        string password = password_input.text;
        StartCoroutine(LoginUser(username, password));
    }
}
