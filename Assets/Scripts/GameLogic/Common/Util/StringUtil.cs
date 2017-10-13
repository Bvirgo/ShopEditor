using UnityEngine;
using System.Collections;

public class StringUtil
{
    static public Vector2 StringToVector2(string str)
    {
        //InfoTips.LogInfo (str);
        str = str.Substring(1, str.Length - 2);
        string[] nums = str.Split(",".ToCharArray(), 2);
        return new Vector2(float.Parse(nums[0]), float.Parse(nums[1]));
    }
    static public Vector3 StringToVector3(string str)
    {
        //InfoTips.LogInfo (str);
        str = str.Substring(1, str.Length - 2);
        string[] nums = str.Split(",".ToCharArray(), 3);
        return new Vector3(float.Parse(nums[0]), float.Parse(nums[1]), float.Parse(nums[2]));
    }

    static public Vector4 StringToVector4(string str)
    {
        //InfoTips.LogInfo (str);
        str = str.Substring(1, str.Length - 2);
        string[] nums = str.Split(",".ToCharArray(), 4);
        return new Vector4(float.Parse(nums[0]), float.Parse(nums[1]), float.Parse(nums[2]), float.Parse(nums[3]));
    }

    static public Quaternion StringToQuaternion(string str)
    {
        //InfoTips.LogInfo (str);
        str = str.Substring(1, str.Length - 2);
        string[] nums = str.Split(",".ToCharArray(), 4);
        return new Quaternion(float.Parse(nums[0]), float.Parse(nums[1]), float.Parse(nums[2]), float.Parse(nums[3]));
    }

    static public Color StringToColor4(string str)
    {
        //InfoTips.LogInfo (str);
        str = str.Substring(5, str.Length - 6);
        string[] nums = str.Split(",".ToCharArray(), 4);
        return new Color(float.Parse(nums[0]), float.Parse(nums[1]), float.Parse(nums[2]), float.Parse(nums[3]));
    }
}
