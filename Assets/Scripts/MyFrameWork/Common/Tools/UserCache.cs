using UnityEngine;

/// <summary>
/// 用户本地缓存
/// </summary>
public static class UserCache
{
    private static readonly string USER_NAME_KEY = "username";
    private static readonly string PASSWORD_KEY = "password";
    // 走向图ID
    private static readonly string ROUTEID_KEY = "routeid";

    // 上次选中材质
    private static readonly string MAT_KEY = "matIndex";

    internal static void SetUserName(string name)
    {
        PlayerPrefs.SetString(USER_NAME_KEY, name);
    }

    internal static void SetRouteID(string _routeid)
    {
        PlayerPrefs.SetString(ROUTEID_KEY, _routeid);
    }

    public static string GetRouteID()
    {
        return PlayerPrefs.GetString(ROUTEID_KEY);
    }

    internal static void SetMatKey(int _nKey)
    {
        PlayerPrefs.SetInt(MAT_KEY, _nKey);
    }

    public static int GetMatKey()
    {
        return PlayerPrefs.GetInt(MAT_KEY);
    }

    public static string GetUserName()
    {
        return PlayerPrefs.GetString(USER_NAME_KEY);
    }

    internal static void SetPassword(string pwd)
    {
        PlayerPrefs.SetString(PASSWORD_KEY, pwd);
    }

    public static string GetPassword()
    {
        return PlayerPrefs.GetString(PASSWORD_KEY);
    }
}
