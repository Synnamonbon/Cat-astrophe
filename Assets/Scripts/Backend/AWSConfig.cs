using UnityEngine;

public static class AWSConfig
{
    private const string BASE_URL = "https://njlqthr3ac.execute-api.eu-north-1.amazonaws.com";
    public static string GetRegisterURL() => $"{BASE_URL}/register";
    public static string GetLoginURL() => $"{BASE_URL}/login";
    public static string GetDisplayNameURL(int uid) => $"{BASE_URL}/get/display_name/{uid}";
    public static string GetPreferredNameURL(int uid) => $"{BASE_URL}/get/pref_name/{uid}";
    public static string SetDisplayNameURL(int uid) => $"{BASE_URL}/set/display_name/{uid}";
    public static string SetPreferredNameURL(int uid) => $"{BASE_URL}/set/pref_name/{uid}";
}
