using UnityEngine;
using System.Collections;
using ZFrameWork;
using Jhqc.EditorCommon;
using System.Collections.Generic;
using ArtsWork;
using System;
using System.Text.RegularExpressions;
using LitJson;
using System.IO;
using System.Text;

public class JHQCHelper : Singleton<JHQCHelper>
{
    private GameObject m_objRoot;
    public override void Init()
    {
        base.Init();

    }

    #region SD材质模板
    public MatManager.MatCacheItem m_curMatItem;

    /// <summary>
    /// 记录模型 和 材质
    /// </summary>
    private Dictionary<GameObject, MatManager.MatCacheItem> m_dicObjAndMat = new Dictionary<GameObject, MatManager.MatCacheItem>();
    public MatManager.MatCacheItem GetMatByObj(GameObject _obj)
    {
        if (m_dicObjAndMat.ContainsKey(_obj))
        {
            return m_dicObjAndMat[_obj];
        }

        return null;
    }

    public void AddObjAndMat(GameObject _obj, MatManager.MatCacheItem _mat)
    {
        if (m_dicObjAndMat.ContainsKey(_obj))
        {
            m_dicObjAndMat.Remove(_obj);
        }

        m_dicObjAndMat.Add(_obj, _mat);
    }

    /// <summary>记录每个材质的每一个属性的ProceduralPropertyDescription</summary>
    public Dictionary<string, Dictionary<string, ProceduralPropertyDescription>> Mat2ParamName2DesDic = new Dictionary<string, Dictionary<string, ProceduralPropertyDescription>>();

    public void RecordMatPropName(MatManager.MatCacheItem _mat)
    {
        if (_mat == null)
            return;
        //记录每个subMat的每个属性的描述, 用于面板显示
        string matName = Utils.RemovePostfix_Instance(_mat.Material.name);
        foreach (var ppd in _mat.Material.GetProceduralPropertyDescriptions())
        {
            Mat2ParamName2DesDic.TryAddNoReplace(matName, new Dictionary<string, ProceduralPropertyDescription>());
            Mat2ParamName2DesDic[matName].AddRep(ppd.name, ppd);
        }
    }

    #endregion

    #region 材质实时加载
    private Dictionary<string, MatManager.MatCacheItem> m_dicMat = new Dictionary<string, MatManager.MatCacheItem>();

    public void GetMatByName(string _strName, Action<MatManager.MatCacheItem> _cb, Action _failCb = null)
    {
        if (m_dicMat.ContainsKey(_strName))
        {
            _cb(m_dicMat[_strName]);
        }
        else
        {
            MaterialInfo mif = GetMatInfo(_strName);
            if (null == mif)
            {
                _cb(null);
            }

            MatManager.Instance.LoadMat(_strName, mci =>
            {

                //AddMat(_strName, mci);
                RecordMatPropName(mci);

                _cb(mci);
            });
            MatManager.Instance.OnLoadError = (strError) =>
            {
                if (_failCb != null)
                {
                    _failCb();
                }
            };
        }
    }

    public void AddMat(string _strName, MatManager.MatCacheItem _mat)
    {
        if (!m_dicMat.ContainsKey(_strName))
        {
            m_dicMat.Add(_strName, _mat);
        }
    }

    public MaterialInfo GetMatInfo(string _strName)
    {
        List<MaterialInfo> pMInfo = MatManager.Instance.AllMatInfo;
        if (pMInfo != null)
        {
            for (int i = 0; i < pMInfo.Count; ++i)
            {
                if (pMInfo[i].name.Equals(_strName))
                {
                    return pMInfo[i];
                }
            }
        }

        return null;
    }

    /// <summary>
    /// 获取指定Tag下所有材质信息
    /// </summary>
    /// <param name="_strName"></param>
    /// <returns></returns>
    public List<MaterialInfo> GetAllMatInfoByTag(string _strName)
    {
        List<MaterialInfo> pRes = new List<MaterialInfo>();
        List<MaterialInfo> pMInfo = MatManager.Instance.AllMatInfo;
        for (int i = 0; i < pMInfo.Count; ++i)
        {
            string[] p = pMInfo[i].tags.Split(',');
            List<string> pList = new List<string>(p);
            if (pList.Contains(_strName))
            {
                pRes.Add(pMInfo[i]);
            }
        }
        return pRes;
    }
    #endregion

    #region 商铺单粒度
    /// <summary>
    /// 招牌配置是否从本地加载：也就是不读取单粒度招牌数据
    /// </summary>
    private bool m_bIsLoadShopConfigFromLocal = false;
    public bool IsLoadShopConfigFromLocal
    {
        get
        {
            return m_bIsLoadShopConfigFromLocal;
        }
    }

    /// <summary>
    /// 服务器获取的招牌gui_JD
    /// </summary>
    private Dictionary<string, JsonData> guid_shopVoJd = new Dictionary<string, JsonData>();
    
    /// <summary>
    /// 走向图单粒度招牌数据列表
    /// </summary>
    /// <param name="_strRouteID"></param>
    /// <param name="_cbDone"></param>
    public void OnLoadSingleShopList(string _strRouteID, Action _cbDone)
    {
        HttpService.GetBlockSingleTargetList(_strRouteID, (js) =>
        {
            AnalysisShopList(_strRouteID, js);

            _cbDone();

        }, (strError) =>
        {
            Debug.LogWarning(string.Format("获取走向图：{0}，单粒度招牌列表失败：{1},加载原始config数据!", _strRouteID, strError));

            _cbDone();
        }, HttpService.SingleTargetType.Shop);
    }

    /// <summary>
    /// 单粒度招牌拼接为config配置
    /// </summary>
    /// <param name="_strRouteID"></param>
    /// <param name="_strJD"></param>
    private void AnalysisShopList(string _strRouteID, string _strJD)
    {
        JsonData shopJd = JsonMapper.ToObject(_strJD);
        guid_shopVoJd.Clear();

        shopJd = shopJd.ReadJsonData("shopSignList");

        // 勾选了从本地加载商铺数据
        if (IsLoadShopConfigFromLocal)
        {
            if (shopJd.Count > 1)
            {
                LogicUtils.Instance.OnAlert(string.Format("该区块存在单粒度商铺数据，不能用本地商铺配置覆盖！"));
                AnalysisSingleShopConfig(shopJd, _strRouteID);
            }
            else
            {
                LogicUtils.Instance.OnAlert(string.Format("勾选了从本地加载商铺旧配置，是否确定加载该配置？"),() =>
                {
                    return;
                }, () =>
                {
                    AnalysisSingleShopConfig(shopJd, _strRouteID);

                });
            }
        }
        else
        {
            AnalysisSingleShopConfig(shopJd, _strRouteID);
        }
    }

    /// <summary>
    /// 解析商铺单粒度数据
    /// </summary>
    /// <param name="_jd"></param>
    private void AnalysisSingleShopConfig(JsonData _jd, string _strRouteID)
    {
        JsonData shopJd = _jd;
        if (shopJd != null)
        {
            Dictionary<string, string> picName_picCrc = new Dictionary<string, string>();
            JsonData shopsJD = JsonUtils.EmptyJsonArray;
            JsonData picJD = JsonUtils.EmptyJsonArray;

            for (int i = 0; i < shopJd.Count; i++)
            {
                JsonData jd = shopJd[i];
                JsonData shopJD = JsonUtils.EmptyJsonObject;
                JsonData shopSignListData = JsonUtils.EmptyJsonArray;
                string strGUID = jd.ReadString("guid");

                guid_shopVoJd.AddOrReplace(strGUID, jd);
                // 一个招牌
                shopSignListData.Add(jd);

                // 组装商铺
                shopJD["buildName"] = jd.ReadString("BuildGuid");
                shopJD["pointList"] = jd.ReadJsonData("PointList");
                shopJD["shopSignList"] = shopSignListData;
                shopsJD.Add(shopJD);

                // 招牌图片
                string strPicName = jd.ReadString("SignCode", "null");
                string strPicCrc = jd.ReadString("SignCrc", "null");
                picName_picCrc.AddOrReplace(strPicName, strPicCrc);

                JsonData newPic = new JsonData();
                newPic[strPicName] = strPicCrc;
                picJD.Add(newPic);
            }


            // 覆盖Config.txt
            string FileName_config = "config.txt";
            string LocalDataPath = Utils.GetDataPath() + "ShopData/";

            string shopDataFolderPath = LocalDataPath + _strRouteID + "/";
            string configPath = shopDataFolderPath + FileName_config;

            if (File.Exists(configPath))
            {
                JsonData newConfigJD = JsonUtils.EmptyJsonObject;
                newConfigJD["data"] = shopsJD;
                
                File.WriteAllText(configPath, newConfigJD.ToJson());
            }
        }
    }

    /// <summary>
    /// 比较招牌数据差异性
    /// </summary>
    /// <param name="_strGUID"></param>
    /// <param name="_jd"></param>
    /// <returns></returns>
    public bool IsShopNeedUpdate(string _strGUID, JsonData _jd)
    {
        if (guid_shopVoJd.ContainsKey(_strGUID))
        {
            string strOldJd = guid_shopVoJd[_strGUID].ToJson();
            string strNewJd = _jd.ToJson();
            strOldJd = strOldJd.Replace("{", "");
            strOldJd = strOldJd.Replace("}", "");
            //if (strOldJd.Equals(strNewJd))
            if (strNewJd.Contains(strOldJd))
            {
                return false;
            }
            Debug.LogWarning(string.Format("两次保存招牌数据不一致：\n,第原始招牌:{0}\n,第修改过的招牌数据：{1}", strOldJd, strNewJd));
        }
        return true;
    }
    #endregion

    #region Camera
    private CameraCtrl m_cc;
    /// <summary>
    /// Camera Ctroller
    /// </summary>
    public CameraCtrl TheCameraCtrl
    {
        get {
            if (m_cc == null)
            {
                m_cc = Camera.main.GetComponent<CameraCtrl>();
            }
            return m_cc;
        }
        
    }
    #endregion

    #region 下载文件
    /// <summary>
    /// 下载文件，写到指定位置
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="fileCrc"></param>
    /// <param name="targetPath"></param>
    /// <param name="onDone"></param>
    /// <param name="onFail"></param>
    /// <param name="retry"></param>
    public static void DownloadFile(string fileName, string fileCrc, 
        string targetPath, Action onDone, Action<HttpResp.ErrorType, string> onFail, int retry = 3)
    {
        //检查目标位置是否已有同Name, Crc的文件存在
        if (File.Exists(targetPath))
        {
            byte[] byteArr = File.ReadAllBytes(targetPath);
            if (Crc32.CountCrcRetString(byteArr) == fileCrc)
            {
                onDone();
                return;
            }
        }

        if (retry <= 0)
            return;

        ResManager.Instance.OnLoadServerRes(fileName, fileCrc,
                      lr =>
                      {
                          File.WriteAllBytes(targetPath, lr.GetResource() as byte[] );
                          onDone();
                      });
    }

    /// <summary>请求失败通用回调</summary>
    /// <param name="prefixMsg">描述文字前缀</param>
    public static void OnReqFail(string prefixMsg, HttpResp.ErrorType errorType, string errorMsg, bool needAlertWindow = true)
    {
        Debug.LogError(errorType + ", " + errorMsg);
        if (needAlertWindow)
           LogicUtils.Instance.OnAlert(prefixMsg + errorType + ", " + errorMsg);
    }
    #endregion

    #region 场景初始化
    /// <summary>
    /// 场景初始化：加载地面，角色
    /// </summary>
    public void OnInitScene()
    {
        // 加载地面模型
        GameObject objGround = Resources.Load(Defines.MainGroundPath) as GameObject;
        objGround = GameObject.Instantiate(objGround);
        objGround.transform.position = Vector3.zero;
        objGround.layer = LayerMask.NameToLayer(Defines.MapsLayerName);

        // 挂载Player
        Camera c = Camera.main;
        if (c.gameObject.GetComponent<CameraCtrl>() == null)
        {
            c.gameObject.AddComponent<CameraCtrl>();
        }
    }
    #endregion
}
