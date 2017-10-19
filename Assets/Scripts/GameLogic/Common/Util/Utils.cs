using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using UnityEngine.EventSystems;
using System.Drawing;
using System.Windows.Forms;
using UnityEngine.UI;

public static class Utils
{
    #region 文件名相关
    /// <summary>
    /// 获取路径目录，除去文件名
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static string GetDirectoryName(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            return null;

        int index = fileName.LastIndexOf('/');
        if (index < 0)
            return "";

        return fileName.Substring(0, index);
    }
    /// <summary>
    /// 路径转为标准格式路径
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string GetStandardPath(string path)
    {
        int loopNum = 20;
        path = path.Replace(@"\", @"/");
        while (path.IndexOf(@"//") != -1)
        {
            path = path.Replace(@"//", @"/");
            loopNum--;
            if (loopNum < 0)
            {
                Debug.Log("路径清理失败: " + path);
                return path;
            }
        }
        return path;
    }

    public static string GetFolderPath(string path, bool fullPath = true)
    {
        path = GetStandardPath(path);
        if (fullPath)//获取全路径
        {
            if (path.LastIndexOf(@"/") == path.Length - 1)
                return GetFolderPath(path.Substring(0, path.Length - 1));
            else
                return path.Substring(0, path.LastIndexOf(@"/") + 1);
        }
        else//获取父级文件夹名
        {
            string[] strArr = path.Split('/');

            if (path.LastIndexOf(@"/") == path.Length - 1)
                return strArr[strArr.Length - 2];
            else
                return strArr[strArr.Length - 1];
        }
    }

    /// <summary>
    /// 通过全路径，获取文件名（去掉后缀）
    /// </summary>
    /// <param name="path"></param>
    /// <param name="needPostfix">是否需要带后缀</param>
    /// <returns></returns>
    public static string GetFileNameByPath(string path, bool needPostfix = false)
    {
        path = GetStandardPath(path);
        string fileFolderPath = path.Substring(0, path.LastIndexOf(@"/") + 1);

        string fileName = path.Substring(path.LastIndexOf("/") + 1, path.Length - fileFolderPath.Length);
        if (needPostfix)
            return fileName;
        else
            return fileName.Substring(0, fileName.LastIndexOf("."));
    }
    /// <summary>获取文件名后缀</summary>
    public static string GetFilePostfix(string fileName)
    {
        if (fileName == null)
            return null;
        string res;
        if (fileName.IndexOf(".") == -1)
            res = "";
        else
        {
            string[] ss = fileName.Split(new char[1] { '.' });
            res = ss[ss.Length - 1];
        }
        return res;
    }

    /// <summary>
    /// 去掉文件名后缀
    /// </summary>
    /// <param name="_strName"></param>
    /// <returns></returns>
    public static string GetFilePrefix(string _strName)
    {
        string strName = _strName;
        int nIndex = strName.LastIndexOf('.');
        if (nIndex > 0 && nIndex < strName.Length - 1)
        {
            strName = strName.Substring(0, strName.LastIndexOf('.'));
        }

        return strName;
    }

    public static string GetParentFolderPath(string path, bool fullPath = true)
    {
        path = GetStandardPath(path);
        if (fullPath)//获取全路径
        {
            if (path.LastIndexOf(@"/") == path.Length - 1)
                return GetFolderPath(path.Substring(0, path.Length - 1));
            else
                return path.Substring(0, path.LastIndexOf(@"/") + 1);
        }
        else//获取父级文件夹名
        {
            string[] strArr = path.Split('/');
            return strArr[strArr.Length - 2];
        }
    }
    #endregion

    #region 文件读写相关
    public static byte[] LoadFile(FileInfo fInfo)
    {
        return LoadFile(fInfo.FullName);
    }
    public static byte[] LoadFile(string path) //@path)
    {
        if (File.Exists(path))
        {
            using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                //创建文件长度缓冲区
                byte[] bytes = new byte[fileStream.Length];
                fileStream.Seek(0, SeekOrigin.Begin);
                //读取文件
                fileStream.Read(bytes, 0, (int)fileStream.Length);
                return bytes;
            }
        }
        else
        {
            return null;
        }
    }

    public static string LoadFile2String(string path)
    {
        if (File.Exists(path))
        {
            return File.ReadAllText(path);
        }
        else
        {
            return string.Empty;
        }
    }
    /// <summary>
    /// 读取文本：PC模式，移动模式
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string ReadTextFile(string path)
    {
        if (path.Contains("file://"))
        {
            WWW w3 = new WWW(path);

            while (!w3.isDone)
                System.Threading.Thread.Sleep(1);

            if (w3.error == null)
                return w3.text;
            else
            {
                return null;
            }
        }
        else
        {
            try
            {
                return File.ReadAllText(path);
            }
            catch (System.Exception)
            {
                return null;
            }
        }
    }

    public static void WriteTextFile(string path, string text)
    {
        File.WriteAllText(path, text);
    }

    /// <summary>
    /// 带时间戳保存
    /// </summary>
    /// <param name="_strMsg"></param>
    /// <param name="_strFileName"></param>
    /// <param name="_strType"></param>
    /// <param name="_bAppend"></param>
    public static void SaveInfo(string _strMsg, string _strFileName = "CityEditorInfo", string _strType = ".txt", bool _bAppend = false)
    {
        if (!Directory.Exists(GetDataPath() + "SaveData/"))
            Directory.CreateDirectory(GetDataPath() + "SaveData/");

        string strPath = GetDataPath() + "SaveData/" + _strFileName + "_" + DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss") + _strType;

        UTF8Encoding utf8BOM = new UTF8Encoding(true);
        StreamWriter sw = new StreamWriter(strPath, true, utf8BOM);
        sw.WriteLine(_strMsg);
        sw.Close();
    }

    /// <summary>
    /// 不加时间戳
    /// </summary>
    /// <param name="_strMsg"></param>
    /// <param name="_strFileName"></param>
    /// <param name="_strType"></param>
    /// <param name="_bAppend"></param>
    public static void SimpleSaveInfo(string _strMsg, string _strFileName = "CityEditorInfo", string _strType = ".txt", bool _bAppend = false)
    {
        if (!Directory.Exists(GetDataPath() + "SaveData/"))
            Directory.CreateDirectory(GetDataPath() + "SaveData/");

        string strPath = GetDataPath() + "SaveData/" + _strFileName + _strType;

        UTF8Encoding utf8BOM = new UTF8Encoding(true);
        StreamWriter sw = new StreamWriter(strPath, true, utf8BOM);
        sw.WriteLine(_strMsg);
        sw.Close();
    }

    #endregion

    #region 文件存在性
    public static bool IsDirExists(string path)
    {
        return Directory.Exists(path);
    }

    public static bool IsFileExists(string path)
    {
        return File.Exists(path);
    }
    #endregion

    #region 字符串相关操作
    public static bool HasChinese(this string str)
    {
        return Regex.IsMatch(str, @"[\u4e00-\u9fa5]");
    }

    public static bool IsLegelString(string tarStr)
    {
        return (tarStr != "" &&
            tarStr != "null" &&
            tarStr != null);
    }
    /// <summary>
    /// 纯数字
    /// </summary>
    /// <param name="_str"></param>
    /// <returns></returns>
    public static bool IsInt(string _str)
    {
        Regex regex = new Regex(@"^\d+$");

        // 区号纯数字
        return regex.IsMatch(_str);
    }

    public static byte[] String2ByteDefault(string str)
    {
        return Encoding.Default.GetBytes(str);
    }

    public static string Byte2StringDefault(byte[] byt)
    {
        return Encoding.Default.GetString(byt);
    }

    public static byte[] String2ByteUTF8(string str)
    {
        return Encoding.UTF8.GetBytes(str);
    }

    public static string Byte2StringUTF8(byte[] byt)
    {
        if (null == byt)
        {
            return string.Empty;
        }
        return Encoding.UTF8.GetString(byt);
    }
    #endregion

    #region 平台相关
    /// <summary>
    /// 获取平台dataPath
    /// </summary>
    /// <returns></returns>
    public static string GetDataPath()
    {
        string path = "";
        if (UnityEngine.Application.platform == RuntimePlatform.Android || UnityEngine.Application.platform == RuntimePlatform.IPhonePlayer)
        {
            path = UnityEngine.Application.persistentDataPath + "/";
        }
        else if (UnityEngine.Application.platform == RuntimePlatform.WindowsPlayer)
        {
            path = UnityEngine.Application.dataPath + "/";
        }
        else if (UnityEngine.Application.platform == RuntimePlatform.WindowsEditor)
        {
            path = UnityEngine.Application.dataPath + "/../";
        }
        else
        {
            path = UnityEngine.Application.persistentDataPath + "/";
        }

        return path;
    }
    #endregion

    #region 资源加载相关

    public static bool IsPic(string fileName)
    {
        string postFix = GetFilePostfix(fileName);
        return postFix == "png"
            || postFix == "PNG"
            || postFix == "jpg"
            || postFix == "JPG"
            || postFix == "jpeg"
            || postFix == "JPEG";
    }

    public static bool IsFbx(string fileName)
    {
        string postFix = GetFilePostfix(fileName);
        return postFix.ToLower().Equals("fbx");
    }

    public static bool IsAB(string fileName)
    {
        string postFix = GetFilePostfix(fileName);
        return postFix.ToLower().Equals("assetbundle");
    }
    /// <summary>
    /// 根据URL，加载Texture
    /// </summary>
    /// <param name="url"></param>
    /// <param name="cb"></param>
    /// <returns></returns>
    public static IEnumerator LoadTexture(string url, Action<Texture2D> cb)
    {
        //这里的url可以是web路径也可以是本地路径file://  
        WWW www = new WWW(url);
        //挂起程序段，等资源下载完成后，继续执行下去  
        yield return www;

        //判断是否有错误产生  
        if (string.IsNullOrEmpty(www.error))
        {
            //把下载好的图片回调给调用者  
            cb.Invoke(www.texture);
            //释放资源  
            www.Dispose();
        }
    }

    /// <summary>
    /// 根据URL ，记载AB
    /// </summary>
    /// <param name="url"></param>
    /// <param name="ab"></param>
    /// <returns></returns>
    public static IEnumerator LoadAssetBundle(string url, Action<AssetBundle> ab)
    {
        WWW www = new WWW(url);
        yield return www;

        if (string.IsNullOrEmpty(www.error))
        {
            ab.Invoke(www.assetBundle);
            www.Dispose();
        }
    }
    #endregion

    #region 资源本地保存
    /// <summary>
    /// 流，存为Png格式图片
    /// </summary>
    /// <param name="incomingTexture"></param>
    /// <param name="pathName">全路径</param>
    public static void SaveTextureFile(Texture2D incomingTexture, string pathName)
    {
        byte[] bytes = incomingTexture.EncodeToPNG();
        string dir = Path.GetDirectoryName(pathName);
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        File.WriteAllBytes(pathName, bytes);
    }

    /// <summary>
    /// 保存RenderTexture，到本地
    /// </summary>
    /// <param name="rt"></param>
    /// <param name="filename"></param>
    /// <returns></returns>
    public static bool SaveRenderTextureToPNG(RenderTexture rt, string filename)
    {
        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = rt;
        Texture2D png = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, false);
        png.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);

        SaveTextureFile(png, filename);

        Texture2D.DestroyImmediate(png);
        png = null;
        RenderTexture.active = prev;
        return true;

    }
    #endregion

    #region 唯一性相关
    /// <summary>
    /// 获取字符串MD5码
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static string StringMD5(string data)
    {
        byte[] result = Encoding.Default.GetBytes(data.Trim());
        MD5 md5 = new MD5CryptoServiceProvider();
        byte[] output = md5.ComputeHash(result);
        return BitConverter.ToString(output).Replace("-", "");
    }
    /// <summary>
    /// 流，计算Crc 
    /// 这个和Crc32.cs中的方法结果一致！
    /// </summary>
    /// <param name="pBuf"></param>
    /// <returns></returns>
    public static uint CaclCRC(byte[] pBuf)
    {
        // Table of CRC-32's of all single byte values
        uint[] crctab = new uint[]
        {
          0x00000000, 0x77073096, 0xee0e612c, 0x990951ba, 0x076dc419,
          0x706af48f, 0xe963a535, 0x9e6495a3, 0x0edb8832, 0x79dcb8a4,
          0xe0d5e91e, 0x97d2d988, 0x09b64c2b, 0x7eb17cbd, 0xe7b82d07,
          0x90bf1d91, 0x1db71064, 0x6ab020f2, 0xf3b97148, 0x84be41de,
          0x1adad47d, 0x6ddde4eb, 0xf4d4b551, 0x83d385c7, 0x136c9856,
          0x646ba8c0, 0xfd62f97a, 0x8a65c9ec, 0x14015c4f, 0x63066cd9,
          0xfa0f3d63, 0x8d080df5, 0x3b6e20c8, 0x4c69105e, 0xd56041e4,
          0xa2677172, 0x3c03e4d1, 0x4b04d447, 0xd20d85fd, 0xa50ab56b,
          0x35b5a8fa, 0x42b2986c, 0xdbbbc9d6, 0xacbcf940, 0x32d86ce3,
          0x45df5c75, 0xdcd60dcf, 0xabd13d59, 0x26d930ac, 0x51de003a,
          0xc8d75180, 0xbfd06116, 0x21b4f4b5, 0x56b3c423, 0xcfba9599,
          0xb8bda50f, 0x2802b89e, 0x5f058808, 0xc60cd9b2, 0xb10be924,
          0x2f6f7c87, 0x58684c11, 0xc1611dab, 0xb6662d3d, 0x76dc4190,
          0x01db7106, 0x98d220bc, 0xefd5102a, 0x71b18589, 0x06b6b51f,
          0x9fbfe4a5, 0xe8b8d433, 0x7807c9a2, 0x0f00f934, 0x9609a88e,
          0xe10e9818, 0x7f6a0dbb, 0x086d3d2d, 0x91646c97, 0xe6635c01,
          0x6b6b51f4, 0x1c6c6162, 0x856530d8, 0xf262004e, 0x6c0695ed,
          0x1b01a57b, 0x8208f4c1, 0xf50fc457, 0x65b0d9c6, 0x12b7e950,
          0x8bbeb8ea, 0xfcb9887c, 0x62dd1ddf, 0x15da2d49, 0x8cd37cf3,
          0xfbd44c65, 0x4db26158, 0x3ab551ce, 0xa3bc0074, 0xd4bb30e2,
          0x4adfa541, 0x3dd895d7, 0xa4d1c46d, 0xd3d6f4fb, 0x4369e96a,
          0x346ed9fc, 0xad678846, 0xda60b8d0, 0x44042d73, 0x33031de5,
          0xaa0a4c5f, 0xdd0d7cc9, 0x5005713c, 0x270241aa, 0xbe0b1010,
          0xc90c2086, 0x5768b525, 0x206f85b3, 0xb966d409, 0xce61e49f,
          0x5edef90e, 0x29d9c998, 0xb0d09822, 0xc7d7a8b4, 0x59b33d17,
          0x2eb40d81, 0xb7bd5c3b, 0xc0ba6cad, 0xedb88320, 0x9abfb3b6,
          0x03b6e20c, 0x74b1d29a, 0xead54739, 0x9dd277af, 0x04db2615,
          0x73dc1683, 0xe3630b12, 0x94643b84, 0x0d6d6a3e, 0x7a6a5aa8,
          0xe40ecf0b, 0x9309ff9d, 0x0a00ae27, 0x7d079eb1, 0xf00f9344,
          0x8708a3d2, 0x1e01f268, 0x6906c2fe, 0xf762575d, 0x806567cb,
          0x196c3671, 0x6e6b06e7, 0xfed41b76, 0x89d32be0, 0x10da7a5a,
          0x67dd4acc, 0xf9b9df6f, 0x8ebeeff9, 0x17b7be43, 0x60b08ed5,
          0xd6d6a3e8, 0xa1d1937e, 0x38d8c2c4, 0x4fdff252, 0xd1bb67f1,
          0xa6bc5767, 0x3fb506dd, 0x48b2364b, 0xd80d2bda, 0xaf0a1b4c,
          0x36034af6, 0x41047a60, 0xdf60efc3, 0xa867df55, 0x316e8eef,
          0x4669be79, 0xcb61b38c, 0xbc66831a, 0x256fd2a0, 0x5268e236,
          0xcc0c7795, 0xbb0b4703, 0x220216b9, 0x5505262f, 0xc5ba3bbe,
          0xb2bd0b28, 0x2bb45a92, 0x5cb36a04, 0xc2d7ffa7, 0xb5d0cf31,
          0x2cd99e8b, 0x5bdeae1d, 0x9b64c2b0, 0xec63f226, 0x756aa39c,
          0x026d930a, 0x9c0906a9, 0xeb0e363f, 0x72076785, 0x05005713,
          0x95bf4a82, 0xe2b87a14, 0x7bb12bae, 0x0cb61b38, 0x92d28e9b,
          0xe5d5be0d, 0x7cdcefb7, 0x0bdbdf21, 0x86d3d2d4, 0xf1d4e242,
          0x68ddb3f8, 0x1fda836e, 0x81be16cd, 0xf6b9265b, 0x6fb077e1,
          0x18b74777, 0x88085ae6, 0xff0f6a70, 0x66063bca, 0x11010b5c,
          0x8f659eff, 0xf862ae69, 0x616bffd3, 0x166ccf45, 0xa00ae278,
          0xd70dd2ee, 0x4e048354, 0x3903b3c2, 0xa7672661, 0xd06016f7,
          0x4969474d, 0x3e6e77db, 0xaed16a4a, 0xd9d65adc, 0x40df0b66,
          0x37d83bf0, 0xa9bcae53, 0xdebb9ec5, 0x47b2cf7f, 0x30b5ffe9,
          0xbdbdf21c, 0xcabac28a, 0x53b39330, 0x24b4a3a6, 0xbad03605,
          0xcdd70693, 0x54de5729, 0x23d967bf, 0xb3667a2e, 0xc4614ab8,
          0x5d681b02, 0x2a6f2b94, 0xb40bbe37, 0xc30c8ea1, 0x5a05df1b,
          0x2d02ef8d
        };

        uint c = 0xffffffff;  // begin at shift register contents 
        int i, n = pBuf.Length;
        for (i = 0; i < n; i++)
        {
            c = crctab[((int)c ^ pBuf[i]) & 0xff] ^ (c >> 8);
        }
        return c ^ 0xffffffff;
    }
    #endregion

    #region Transform相关

    /// <summary>
    /// 整体缩放
    /// </summary>
    /// <param name="tar"></param>
    /// <param name="multi"></param>
    /// <returns></returns>
    public static Vector3 GetScaledVector(this Vector3 tar, float multi)
    {
        return new Vector3(tar.x * multi, tar.y * multi, tar.z * multi);
    }


    /// <summary>
    /// 遍历Transform
    /// </summary>
    /// <param name="tf"></param>
    /// <param name="Act">(Transform tf, Transform tfParent)</param>
    public static void RecurTraverseTransform(Transform tf, Action<Transform, Transform> Act)
    {
        int count = tf.childCount;
        if (0 == count)
        {
            Act(tf, tf);
            return;
        }
        for (int i = 0; i < count; i++)
        {
            var child = tf.GetChild(i);
            if (Act != null)
            {
                Act(child, tf);
            }
            RecurTraverseTransform(child, Act);
        }
    }
    public static Vector3 GetMoveInput(bool allowArrow = true)
    {
        float kz = 0;
        if (allowArrow)
        {
            kz += Input.GetKey(KeyCode.UpArrow) ? 1f : 0f * 1f;
            kz += Input.GetKey(KeyCode.DownArrow) ? -1f : 0f * 1f;
        }
        kz += Input.GetKey(KeyCode.W) ? 1f : 0f * 1f;
        kz += Input.GetKey(KeyCode.S) ? -1f : 0f * 1f;
        float kx = 0;
        if (allowArrow)
        {
            kx -= Input.GetKey(KeyCode.LeftArrow) ? -1f : 0f * 1f;
            kx -= Input.GetKey(KeyCode.RightArrow) ? 1f : 0f * 1f;
        }
        kx -= Input.GetKey(KeyCode.A) ? -1f : 0f * 1f;
        kx -= Input.GetKey(KeyCode.D) ? 1f : 0f * 1f;
        return new Vector3(kx, 0, kz);
    }

    /// <summary>获取朝目标方向前进后退左右平移后, 坐标的改变</summary>
    /// <param name="rotH">水平面旋转方向</param>
    /// <param name="dirX">左右移动距离</param>
    public static Vector3 MoveTowards(float rotH, float dirX, float dirZ)
    {
        float dx = dirX * Mathf.Sin(rotH) + dirZ * Mathf.Cos(rotH);
        float dz = dirX * Mathf.Cos(rotH) - dirZ * Mathf.Sin(rotH);
        return new Vector3(dx, 0, dz);
    }
    public static Vector3 MoveTowards(float rotH, Vector3 dir)
    {
        float dx = dir.x * Mathf.Sin(rotH) + dir.z * Mathf.Cos(rotH);
        float dz = dir.x * Mathf.Cos(rotH) - dir.z * Mathf.Sin(rotH);
        return new Vector3(dx, 0, dz);
    }

    /// 遍历Transform子集并执行operate
    /// 用例 1: SongeUtil.forAllChildren(gameObject,tar => {tar.transform.position = Vector3.zero;});
    /// </summary>
    public static void ForAllChildren(GameObject target, Action<GameObject> operate, bool includeTarget = true)
    {
        if (target == null)
            return;

        if (includeTarget)
            operate(target);
        for (int i = 0, length = target.transform.childCount; i < length; i++)
        {
            Transform childTran = target.transform.GetChild(i);
            operate(childTran.gameObject);
            ForAllChildren(childTran.gameObject, operate, false);
        }
    }
    #endregion

    #region 系统时间相关
    /// <summary>
    /// 获取当前时间
    /// </summary>
    /// <param name="formart"></param>
    /// <returns></returns>
    public static string Time2String(string formart = "yyyy-MM-dd_HH.mm.ss")
    {
        return DateTime.Now.ToString(formart);
    }
    #endregion

    #region double & float
    public static double TryParam(this double _dValue, string _strValue)
    {
        double d = 0;
        if (string.IsNullOrEmpty(_strValue))
        {
            return d;
        }
        double.TryParse(_strValue, out d);
        return d;
    }
    #endregion

    #region 射线检测相关
    /// <summary>
    /// 获取鼠标点中的Mono对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T GetMonoByMouse<T>() where T : MonoBehaviour
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit rh;
        if (Physics.Raycast(ray, out rh, 99999f))
        {
            return rh.collider.GetComponentInParent<T>();
        }
        return null;
    }

    /// <summary>
    /// 按层进行射线检测
    /// </summary>
    /// <param name="hit">导出碰撞信息</param>
    /// <param name="LayerNameList">层名称列表</param>
    /// <param name="targetCamera">目标相机</param>
    /// <returns>是否碰撞</returns>
    public static bool RayCastByLayer(ref RaycastHit hit, string[] LayerNameList = null, Camera targetCamera = null)
    {
        if (targetCamera == null)
            targetCamera = Camera.main;
        Ray ray = targetCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] raycastHitArr;
        if (LayerNameList == null)
            raycastHitArr = Physics.RaycastAll(ray);
        else
            raycastHitArr = Physics.RaycastAll(ray, 999999, LayerMask.GetMask(LayerNameList));
        foreach (RaycastHit rayCastHit in raycastHitArr)
        {
            hit = rayCastHit;
            return true;
        }
        return false;
    }

    /// <summary>
    /// 获取鼠标点击位置
    /// </summary>
    /// <param name="_strGroundLay">检测层</param>
    /// <returns></returns>
    public static Vector3 GetWorldPosByMouse(string _strGroundLay)
    {
        Vector3 vPos = Vector3.zero;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray,out hit,int.MaxValue,LayerMask.NameToLayer(_strGroundLay)))
        {
            vPos = hit.point;
        }
        return vPos;
    }
    #endregion

    #region Dictionary扩展
    /// <summary>
    /// 尝试直接通过key直接获取TValue的值 : 如果不存在, 返回defaultValue
    /// </summary>
    public static TValue TryGetReturnValue<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key, TValue defaultValue = default(TValue))
    {
        TValue ret;
        if (!dic.TryGetValue(key, out ret))
        {
            return defaultValue;
        }
        return ret;
    }

    /// <summary>
    /// 尝试获取给定key的value值,如果没有key,则建立默认value的指定key pair
    /// </summary>
    /// <returns>获取到的value值或默认value值</returns>
    public static TValue ForceGetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key, TValue defaultValue = default(TValue))
        where TValue : new()
    {
        var value = dic.TryGetReturnValue(key, defaultValue);
        if (value == null)
        {
            value = new TValue();
            dic.AddOrReplace(key, value);
        }
        return value;

    }

    /// <summary>
    /// 尝试将键和值添加到字典中：如果不存在，才添加；存在，不添加也不抛导常
    /// </summary>
    public static Dictionary<TKey, TValue> TryAddNoReplace<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue value)
    {
        if (!dict.ContainsKey(key))
        {
            dict.Add(key, value);
        }
        return dict;
    }
    /// <summary>
    /// 将键和值添加或替换到字典中：如果不存在，则添加；存在，则替换
    /// </summary>
    public static Dictionary<TKey, TValue> AddOrReplace<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue value)
    {
        dict[key] = value;
        return dict;
    }

    /// <summary>
    /// 向字典中批量添加键值对
    /// </summary>
    /// <param name="isReplaceExisted">如果已存在，是否替换</param>
    public static Dictionary<TKey, TValue> AddRange<TKey, TValue>(this Dictionary<TKey, TValue> dict, IEnumerable<KeyValuePair<TKey, TValue>> dictValues,
        bool isReplaceExisted)
    {
        if (null == dictValues || null == dict)
        {
            return null;
        }
        var it = dictValues.GetEnumerator();
        while (it.MoveNext())
        {
            var item = it.Current;
            if (isReplaceExisted)
            {
                dict.AddOrReplace(item.Key, item.Value);
            }
            else
            {
                dict.TryAddNoReplace(item.Key, item.Value);
            }
        }
        return dict;
    }

    /// <summary>
    /// 向字典中批量删除键值对
    /// </summary>
    public static Dictionary<TKey, TValue> RemoveRange<TKey, TValue>(this Dictionary<TKey, TValue> dict, IEnumerable<KeyValuePair<TKey, TValue>> dictValues)
    {
        if (null == dictValues || null == dict)
        {
            return null;
        }
        var it = dictValues.GetEnumerator();
        while (it.MoveNext())
        {
            var item = it.Current;
            dict.Remove(item.Key);
        }
        return dict;
    }
    /// <summary>添加进key-value(list)型字典, 并确保列表非空与不重复添加</summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <param name="dic"></param>
    /// <param name="key"></param>
    /// <param name="tar"></param>
    /// <returns></returns>
    public static Dictionary<T1, List<T2>> AddToList<T1, T2>(this Dictionary<T1, List<T2>> dic, T1 key, T2 tar)
    {
        if (!dic.ContainsKey(key))
            dic.Add(key, new List<T2>());
        List<T2> list = dic[key];
        if (!list.Contains(tar))
            list.Add(tar);
        return dic;
    }

    public static Dictionary<T1, T2> RemoveFromDic<T1, T2>(this Dictionary<T1, T2> dic, Predicate<T2> pred, Action<T2> operation = null)
    {
        Dictionary<T1, T2> deleteDic = new Dictionary<T1, T2>();
        foreach (var key in dic.Keys)
        {
            deleteDic.AddRep(key, dic[key]);
        }
        foreach (var key in deleteDic.Keys)
        {
            dic.Remove(key);
            operation(deleteDic[key]);
        }

        return dic;
    }

    /// <summary>替换/添加, 如果字典中已有则替换值</summary>
    public static Dictionary<T1, T2> AddRep<T1, T2>(this Dictionary<T1, T2> dic, T1 key, T2 value)
    {
        if (dic.ContainsKey(key))
            dic[key] = value;
        else
            dic.Add(key, value);
        return dic;
    }
    #endregion

    #region List 扩展

    public static List<T> Clone<T>(this List<T> list)
    {
        List<T> newList = new List<T>();
        for (int iList = 0, nList = list.Count; iList < nList; iList++)
        {
            newList.Add(list[iList]);
        }
        return newList;
    }

    public static List<T> RemoveFromList<T>(this List<T> list, Predicate<T> pred, Action<T> operation = null)
    {
        List<T> deleteList = new List<T>();
        for (int i = 0, length = list.Count; i < length; i++)
        {
            if (pred(list[i]))
                deleteList.Add(list[i]);
        }
        for (int i = 0, length = deleteList.Count; i < length; i++)
        {
            list.Remove(deleteList[i]);
            if (operation != null)
                operation(deleteList[i]);
        }
        return list;
    }

    public static T FindItem<T>(this IEnumerable<T> enu, Predicate<T> judgeFunc)
    {
        foreach (var item in enu)
        {
            if (judgeFunc(item))
                return item;
        }
        return default(T);
    }
    public static List<T> FindAllItem<T>(this IEnumerable<T> enu, Predicate<T> judgeFunc)
    {
        List<T> list = new List<T>();
        foreach (var item in enu)
        {
            if (judgeFunc(item))
                list.Add(item);
        }
        return list;
    }
    #endregion

    #region Math

    /// <summary>获取法向量</summary>
    public static Vector3 GetNormalVector(Vector3 va, Vector3 vb, Vector3 vc)
    {
        //平面方程Ax+BY+CZ+d=0 行列式计算
        float A = va.y * vb.z + vb.y * vc.z + vc.y * va.z - va.y * vc.z - vb.y * va.z - vc.y * vb.z;
        float B = -(va.x * vb.z + vb.x * vc.z + vc.x * va.z - vc.x * vb.z - vb.x * va.z - va.x * vc.z);
        float C = va.x * vb.y + vb.x * vc.y + vc.x * va.y - va.x * vc.y - vb.x * va.y - vc.x * vb.y;
        //float D = -(va.x * vb.y * vc.z + vb.x * vc.y * va.z + vc.x * va.y * vb.z - va.x * vc.y * vb.z - vb.x * va.y * vc.z - vc.x * vb.y * va.z);
        float E = Mathf.Sqrt(A * A + B * B + C * C);
        Vector3 res = new Vector3(A / E, B / E, C / E);
        return (res);
    }

    #endregion

    #region Unity相关
    /// <summary>去除Unity实例化物体后添加的" (Instance)"字段</summary>
    public static string RemovePostfix_Instance(string str)
    {
        string backstr = " (Instance)";
        while (str.EndsWith(backstr))
            str = str.Substring(0, str.Length - backstr.Length);
        return str;
    }

    public static void RemoveChildren(Transform _tf)
    {
        if (_tf != null)
        {
            for (int i = _tf.childCount - 1; i >= 0; i--)
            {
                GameObject obj = _tf.GetChild(i).gameObject;

                GameObject.Destroy(obj);
            }
        }
    }

    /// <summary>
    /// 添加MeshCollider
    /// </summary>
    /// <param name="_obj"></param>
    public static void AddMeshCollider(GameObject _obj)
    {
        ForAllChildren(_obj, tar =>
        {
            if (tar.GetComponent<MeshFilter>() != null)
            {
                if (tar.GetComponent<Collider>() != null)
                    UnityEngine.Object.Destroy(tar.GetComponent<Collider>());
                tar.AddComponent<MeshCollider>();
            }
        });
    }

    #endregion

    #region UGUI相关
    /// <summary>
    /// 判断鼠标是否在UI上
    /// </summary>
    /// <returns></returns>
    public static bool IsUI()
    {
        Vector2 screenPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

        //实例化点击事件
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        //将点击位置的屏幕坐标赋值给点击事件
        eventDataCurrentPosition.position = new Vector2(screenPosition.x, screenPosition.y);

        List<RaycastResult> results = new List<RaycastResult>();
        //向点击处发射射线
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

        return results.Count > 0;
    }

    /// <summary>
    /// Mouse Is On InputField
    /// </summary>
    /// <returns></returns>
    public static bool IsOnInputField()
    {
        Vector2 screenPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

        //实例化点击事件
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        //将点击位置的屏幕坐标赋值给点击事件
        eventDataCurrentPosition.position = new Vector2(screenPosition.x, screenPosition.y);

        List<RaycastResult> results = new List<RaycastResult>();
        //向点击处发射射线
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

        for (int i = 0; i < results.Count; i++)
        {
            GameObject obj = results[i].gameObject;
            if (obj.GetComponent<InputField>() != null)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 鼠标是否在UGUI上
    /// </summary>
    public static bool IsOnUI
    {
        get { return IsUI(); }
    }

    /// <summary>
    /// 设置RectTransform
    /// </summary>
    /// <param name="_tf"></param>
    /// <param name="_fWidth">宽度</param>
    /// <param name="_fHeight">高度</param>
    public static void ResetRectTransform(Transform _tf,float _fWidth = -1,float _fHeight = -1)
    {
        if (_tf != null)
        {
            RectTransform rtf = _tf.GetComponent<RectTransform>();

            if (-1 == _fWidth)
            {
                _fWidth = rtf.sizeDelta.x;
            }

            if (-1 == _fHeight)
            {
                _fHeight = rtf.sizeDelta.y;
            }

            if (rtf != null)
            {
                rtf.sizeDelta = new Vector2(_fWidth,_fHeight);
            }
        }
    }

    /// <summary>
    /// 生成Sprite
    /// </summary>
    /// <param name="_tx"></param>
    /// <returns></returns>
    public static Sprite GetSprite(Texture2D _tx)
    {
        Sprite spr = null;
        if (_tx != null)
        {
            spr = Sprite.Create(_tx, new Rect(0, 0, _tx.width, _tx.height), Vector2.zero);
        }
        return spr;
    }

    /// <summary>
    /// 设置滑动手感
    /// </summary>
    /// <param name="_tf"></param>
    /// <param name="_nLenth"></param>
    public static void ResetScrollSensitivity(Transform _tf,int _nLenth)
    {
        if (_tf != null && _nLenth > 0)
        {
            ScrollRect sr = _tf.GetComponent<ScrollRect>();
            if (sr != null)
            {
                int nSss = _nLenth % 500;
                nSss = nSss > 1 ? nSss : 1;
                sr.scrollSensitivity = nSss;
            }
        }
    }

    #endregion

    #region 加密 & 解密
    // 定义秘钥
    private const string ENCRYPT_KEY = "cyan";
    /// <summary> /// 加密字符串   
    /// </summary>  
    /// <param name="str">要加密的字符串</param>  
    /// <returns>加密后的字符串</returns>  
    public static string Encrypt(string str)
    {
        DESCryptoServiceProvider descsp = new DESCryptoServiceProvider();   //实例化加/解密类对象   

        byte[] key = Encoding.Unicode.GetBytes(ENCRYPT_KEY); //定义字节数组，用来存储密钥    

        byte[] data = Encoding.Unicode.GetBytes(str);//定义字节数组，用来存储要加密的字符串  

        MemoryStream MStream = new MemoryStream(); //实例化内存流对象      

        //使用内存流实例化加密流对象   
        CryptoStream CStream = new CryptoStream(MStream, descsp.CreateEncryptor(key, key), CryptoStreamMode.Write);

        CStream.Write(data, 0, data.Length);  //向加密流中写入数据      

        CStream.FlushFinalBlock();              //释放加密流      

        return Convert.ToBase64String(MStream.ToArray());//返回加密后的字符串  
    } 
    /// <summary>  
    /// 解密字符串   
    /// </summary>  
    /// <param name="str">要解密的字符串</param>  
    /// <returns>解密后的字符串</returns>  
    public static string Decrypt(string str)
    {
        DESCryptoServiceProvider descsp = new DESCryptoServiceProvider();   //实例化加/解密类对象    

        byte[] key = Encoding.Unicode.GetBytes(ENCRYPT_KEY); //定义字节数组，用来存储密钥    

        byte[] data = Convert.FromBase64String(str);//定义字节数组，用来存储要解密的字符串  

        MemoryStream MStream = new MemoryStream(); //实例化内存流对象      

        //使用内存流实例化解密流对象       
        CryptoStream CStream = new CryptoStream(MStream, descsp.CreateDecryptor(key, key), CryptoStreamMode.Write);

        CStream.Write(data, 0, data.Length);      //向解密流中写入数据     

        CStream.FlushFinalBlock();               //释放解密流      

        return Encoding.Unicode.GetString(MStream.ToArray());       //返回解密后的字符串  
    }
    #endregion

    #region 鼠标操作
    public static float GetRotateY(Vector3 dir)
    {
        return Mathf.Atan2(dir.x, dir.z) / Mathf.PI * 180;
    }
    //示例 eularAngle = new Vector3( -GetRotatV, GetRotateY, 0)
    /// <summary>获取仰角</summary>
    public static float GetRotatV(Vector3 dir)
    {
        float len2 = dir.x * dir.x + dir.z * dir.z;
        float len = Mathf.Sqrt(len2);
        float atan = Mathf.Atan2(dir.y, len);
        return atan / Mathf.PI * 180;
    }

    /// <summary>获取鼠标在水平面的投影点</summary>
    public static Vector3 GetMousePointOnHorizontal(Camera tarCamera = null)
    {
        if (tarCamera == null)
            tarCamera = Camera.main;
        Ray ray = tarCamera.ScreenPointToRay(Input.mousePosition);
        float ratio = -ray.origin.y / ray.direction.y;
        var ret = ray.origin + ray.direction * ratio;
        return ret;
    }


    public static Vector3 GetMousePointOnGround<T>(ref RaycastHit rh, Camera tarCamera = null) where T : IMouseCtrl
    {
        if (tarCamera == null)
            tarCamera = Camera.main;
        Ray ray = tarCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] rhArr = Physics.RaycastAll(ray, 9999f);

        for (int i = 0, length = rhArr.Length; i < length; i++)
        {
            RaycastHit tarRh = rhArr[i];
            T tarMC = tarRh.collider.GetComponentInParent<T>();
            if (tarMC != null)
            {
                rh = tarRh;
                return rh.point;
            }
        }
        rh = default(RaycastHit);
        Vector3 defaultVec = GetMousePointOnHorizontal(tarCamera);
        rh.point = defaultVec;
        return defaultVec;
    }

    public static Vector3 GetPointOnGround<T>(Vector3 vec, ref RaycastHit rh) where T : IMouseCtrl
    {
        Ray ray = new Ray(new Vector3(vec.x, 999f, vec.z), Vector3.down);
        RaycastHit[] rhArr = Physics.RaycastAll(ray, 9999f);

        for (int i = 0, length = rhArr.Length; i < length; i++)
        {
            RaycastHit tarRh = rhArr[i];
            T tarMC = tarRh.collider.GetComponentInParent<T>();
            if (tarMC != null)
            {
                rh = tarRh;
                return rh.point;
            }
        }
        rh = default(RaycastHit);
        Vector3 defaultVec = new Vector3(vec.x, 0, vec.z);
        rh.point = defaultVec;
        return defaultVec;
    }

    /// <summary>
    /// 获取鼠标在指定层碰撞点
    /// </summary>
    /// <param name="_vPos"></param>
    /// <param name="_nLayer"></param>
    /// <returns></returns>
    public static Vector3 GetPointOnLayer(string _strLayer)
    {
        int nLayer = LayerMask.NameToLayer(_strLayer);
        Vector3 vPos = Vector3.zero;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] pHit = Physics.RaycastAll(ray, int.MaxValue);
        for (int i = 0; i < pHit.Length; i++)
        {
            RaycastHit hit = pHit[i];
            if (hit.collider.gameObject.layer == nLayer)
            {
                vPos = hit.point;
                break;
            }
        }
        return vPos;
    }

    #endregion

    #region 图片操作
    public static Bitmap TextToBitmapAllDefault(string text)
    {
        System.Drawing.Font  FontDefault = new System.Drawing.Font("宋体", 20);
        return TextToBitmap(text, FontDefault, Rectangle.Empty, System.Drawing.Color.Black, System.Drawing.Color.White);
    }
    /// <summary>
    /// 把文字转换才Bitmap
    /// new System.Drawing.Font("宋体", 12)
    /// Rectangle.Empty
    /// Brushes.Black
    /// System.Drawing.Color.White
    /// </summary>
    /// <param name="text"></param>
    /// <param name="font"></param>
    /// <param name="rect">用于输出的矩形，文字在这个矩形内显示，为空时自动计算</param>
    /// <param name="fontcolor">字体颜色</param>
    /// <param name="backColor">背景颜色</param>
    /// <returns></returns>
    public static Bitmap TextToBitmap(string text, System.Drawing.Font font, Rectangle rect, System.Drawing.Color fontColor,
        System.Drawing.Color backColor)
    {
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }
        System.Drawing.Graphics g;
        Bitmap bmp;

        //new PointF() 
        if (rect == Rectangle.Empty)
        {
            bmp = new Bitmap(1, 1);
            g = System.Drawing.Graphics.FromImage(bmp);
            var Sz = TextRenderer.MeasureText(g, text, font);
            bmp.Dispose();
            bmp = new Bitmap(Sz.Width, Sz.Height);
            rect = new Rectangle(0, 0, Sz.Width, Sz.Height);

            //bmp = new Bitmap(1, 1);
            //g = System.Drawing.Graphics.FromImage(bmp);
            ////计算绘制文字所需的区域大小（根据宽度计算长度），重新创建矩形区域绘图
            //SizeF sizef = g.MeasureString(text, font, PointF.Empty, format);

            //int width = (int)(sizef.Width + 1);
            //int height = (int)(sizef.Height + 1);
            //rect = new Rectangle(0, 0, width, height);
            //bmp.Dispose();
            //bmp = new Bitmap(width, height);
        }
        else
        {
            bmp = new Bitmap(rect.Width, rect.Height);
        }

        g = System.Drawing.Graphics.FromImage(bmp);

        //使用ClearType字体功能
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
        g.FillRectangle(new SolidBrush(backColor), rect);
        //g.DrawString(text, font, Brushes.Black, rect, format);
        TextRenderer.DrawText(g, text, font, rect, fontColor);
        //bmp.Save("fontphoto");
        return bmp;
    }

    public static Texture2D ImageToTexture2D(System.Drawing.Image imgOrg, bool isDeleteOrgin = false)
    {
        if (null == imgOrg)
        {
            return null;
        }

        Texture2D ret = new Texture2D(imgOrg.Width, imgOrg.Height);
        if (ret.LoadImage(ImageToByteArrayDefault(imgOrg, isDeleteOrgin)))
        {
            return ret;
        }
        return null;
    }

    static void DestroyDisposableObj(IDisposable obj)
    {
        if (obj != null)
        {
            obj.Dispose();
        }
        obj = null;
    }

    public static byte[] ImageToByteArrayDefault(System.Drawing.Image imageOrg, bool isDeleteOrgin = false)
    {
        return ImageToByteArray(imageOrg, System.Drawing.Imaging.ImageFormat.Png, isDeleteOrgin);
    }

    public static byte[] ImageToByteArray(System.Drawing.Image imageOrg, System.Drawing.Imaging.ImageFormat saveFormat,
        bool isDeleteOrgin = false)
    {
        if (null == imageOrg)
        {
            return null;
        }
        using (var ms = new MemoryStream())
        {
            imageOrg.Save(ms, saveFormat);
            if (isDeleteOrgin)
            {
                DestroyDisposableObj(imageOrg);
            }
            return ms.ToArray();
        }
    }
    #endregion
}