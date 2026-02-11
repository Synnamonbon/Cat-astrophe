using UnityEngine;

public static class AWSConfig
{
    private const string BASE_URL = "https://njlqthr3ac.execute-api.eu-north-1.amazonaws.com";
    public static string GetRegisterURL() => $"{BASE_URL}/register";
    public static string GetLoginURL() => $"{BASE_URL}/login";
    public static string GetPlayerURL(int uid) => $"{BASE_URL}/get/{uid}";
}
