using System;
using System.Collections;
using System.Text;
using TMPro;
using UnityEditor.Compilation;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
// Serialisable classes
//using PlayerInfo;
//using DisplayName;          

public class AWSRegister : MonoBehaviour
{
    public TMP_InputField username_input;
    public TMP_InputField password_input;
    public TMP_InputField display_input;
    public Toggle prefFlag_input;
    public string session_token = "";
    public int uid = -1;


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
        } 
        else
        {
            Debug.Log("Error: " + request.error);
            Debug.Log(request.downloadHandler.text);
        }
    }

    // Generates a bearer token and adds the registered user into the active_users database
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
            uid = int.Parse(response.uid);
        }
        else
        {
            Debug.LogError("Error: " + request.error);
            Debug.Log(request.downloadHandler.text);
        }
    }

    // Logs a new guest user into the active_users and generates a bearer token for the session.
    private IEnumerator GuestLogin(string display_name)
    {
        // TODO: implement
        string url = AWSConfig.GetLoginURL();
        return null;
    }

    // Returns own preferred display name or active display name
    private IEnumerator GetUserDisplayName(bool prefFlag)
    {
        // TODO implement
        string json = $"{{\"uid\":\"{uid}\"}}";
        string url;
        if(prefFlag)
        {
            url = AWSConfig.GetPreferredNameURL(uid);
        } 
        else
        {
            url = AWSConfig.GetDisplayNameURL(uid);
        }

        UnityWebRequest request = new UnityWebRequest(url, "GET");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Authorization", $"Bearer {session_token}");
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Response: " + request.downloadHandler.text);
            DisplayName response = JsonUtility.FromJson<DisplayName>(request.downloadHandler.text);
            Debug.Log("Display name retreived: " + response.display_name);
        }
        else
        {
            Debug.Log("Error: " + request.error);
            Debug.Log(request.downloadHandler.text);
            Debug.Log(url);
        }
    }

    // Sets own preferred display name or active display name 
    private IEnumerator SetUserDisplayName(bool prefFlag, string disp_name)
    {
        string json = $"{{\"display_name\":\"{disp_name}\", \"uid\":\"{uid}\"}}";
        string url;
        if(prefFlag)
        {
            url = AWSConfig.SetPreferredNameURL(uid);
        } 
        else
        {
            url = AWSConfig.SetDisplayNameURL(uid);
        }
        
        UnityWebRequest request = new UnityWebRequest(url, "PUT");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Authorization", $"Bearer {session_token}");
        request.SetRequestHeader("Content-Type", "application/json");

        //string jsonBody = System.Text.Encoding.UTF8.GetString(request.uploadHandler.data);
        //Debug.Log(jsonBody);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Response: " + request.downloadHandler.text);
        }
        else
        {
            Debug.Log("Error: " + request.error);
            Debug.Log(request.downloadHandler.text);
            Debug.Log(url);
        }
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

    public void GetDisplayName()
    {
        bool prefFlag = prefFlag_input.isOn;
        StartCoroutine(GetUserDisplayName(prefFlag));
    }

    public void SetDisplayName()
    {
        string name = display_input.text;
        bool prefFlag = prefFlag_input.isOn;
        StartCoroutine(SetUserDisplayName(prefFlag, name));
    }
}
