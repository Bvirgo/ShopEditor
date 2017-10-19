using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using LitJson;
using Jhqc.EditorCommon;
using System.IO;
using System.Collections.Specialized;
using ArtsWork;
using SimpleJSON;
using System.Text;
using MyFrameWork;
using ReadWriteCsv;
using System.Drawing;

/// <summary>
/// 走向图模块
/// </summary>
public class ShopModule : BaseModule
{

    #region BaseMember
    public const string PreFix_RouteID = "zp";
    public const string FileName_config = "config.txt";
    static string LocalDataPath = Utils.GetDataPath() + "ShopData/";
    public const string FileName_shopList = "shopList.txt";
    public const string FolderName_Sign = "sign/";
    public const string All_FileExtend_shopSignPic = ".jpg|.png";

    /// <summary>左侧</summary>
    public List<ShopModelMono> theShopModelList = new List<ShopModelMono>();
    /// <summary>底部</summary>
    public List<ShopSignVO> theShopSignList = new List<ShopSignVO>();
    /// <summary>右侧</summary>
    public List<ShopSignVO> theOriShopSignList = new List<ShopSignVO>();
    /// <summary>建筑-商铺列表</summary>
    Dictionary<string, List<ShopSignVO>> shopIdDic = new Dictionary<string, List<ShopSignVO>>();

    Dictionary<string, ShopRouteData> theAllShopRouteDic = new Dictionary<string, ShopRouteData>();
    Dictionary<string, List<ShopModelMono>> theAllShopModelDic = new Dictionary<string, List<ShopModelMono>>();

    /// <summary>铺面分类[code-显示名称]</summary>
    Dictionary<string, string> shopNameDic = new Dictionary<string, string>();
    /// <summary>铺面分类[code-贴图名称]</summary>
    Dictionary<string, string> shopTexDic = new Dictionary<string, string>();

    /// <summary>铺面按类型分类</summary>
    Dictionary<ShopType, List<ShopVO>> shopTypeDic;

    List<ShopSignEditorVo> shopSignEditorVoList = new List<ShopSignEditorVo>();

    Dictionary<string, GameObject> shopPrefabDic;
    public GameObject OnlyShopPrefab;
    public GameObject OnlySignPrefab;
    Dictionary<int, string> cellNameDic;
    public float CellWidth;

    /// <summary>
    /// 招牌图片Name_Crc
    /// </summary>
    Dictionary<string, string> picName_picCrc = new Dictionary<string, string>();

    ShopModelMono _curSelectShopModel;
    /// <summary>当前店铺面片</summary>
    public ShopModelMono CurSelectShopModel
    {
        get { return _curSelectShopModel; }
        set
        {
            ShopModelMono oldModel = _curSelectShopModel;
            if (oldModel != null)
                oldModel.IsSelect = false;
            _curSelectShopModel = value;

            if (_curSelectShopModel != null)
            {
                if (_curSelectShopModel.ShopSignList == null)
                    _curSelectShopModel.ShopSignList = new List<ShopSignVO>();
                _curSelectShopModel.IsSelect = true;

                Vector3 cameraPos = _curSelectShopModel.worldCenter + _curSelectShopModel.worldNormal.GetScaledVector(10f);
                cameraPos = new Vector3(cameraPos.x, 0, cameraPos.z);
                JHQCHelper.Instance.TheCameraCtrl.TheFocusCtrl.FocusPos = cameraPos;
                JHQCHelper.Instance.TheCameraCtrl.TheFocusCtrl.RotH = Utils.GetRotateY(_curSelectShopModel.worldNormal) + 90f;

                Message msg = new Message(MsgType.ShopView_RefreshBoardList, this);
                msg["data"] = CurSelectShopModel.ShopSignList;
                msg.Send();
            }
            else
            {
                //bottomListUI.DataSrc = null;
                //bottomListUI.RefreshUI();
                // TODO:刷新底部招牌UI
                Message msg = new Message(MsgType.ShopView_RefreshBoardList, this);
                msg["data"] = null;
                msg.Send();
            }
        }
    }


    string curRouteID;
    #endregion

    #region Init 
    public ShopModule()
    {
        this.AutoRegister = true;
    }

    protected override void OnLoad()
    {
        base.OnLoad();

        // 加载商铺铺面模型
        loadModelPrefab(() => { });

        RegisterMsg();

        // 注册Update
        MonoHelper.Instance.UpdateRegister(MoveShopModel);
    }

    protected override void OnRelease()
    {
        base.OnRelease();
        UnRegisterMsg();
    }

    private void RegisterMsg()
    {

        MessageCenter.Instance.AddListener(MsgType.ShopView_LoadRoute, LoadShopRoute);
        MessageCenter.Instance.AddListener(MsgType.ShopView_ShopItemClicked, ClickShopItem);
        MessageCenter.Instance.AddListener(MsgType.ShopView_SampleBoardClicked,ClickSampleBoardItem);
        MessageCenter.Instance.AddListener(MsgType.ShopView_NewPoint,NewPoint);
        MessageCenter.Instance.AddListener(MsgType.ShopView_DeleteShop,DeleteShopItem);
        MessageCenter.Instance.AddListener(MsgType.ShopView_DeleteBoard,DeleteBoardItem);
        MessageCenter.Instance.AddListener(MsgType.ShopView_LocalSave, SaveEditingShopDataConfig2Local);
        MessageCenter.Instance.AddListener(MsgType.ShopView_OnlyBoard,OnlyBoardModel);
        MessageCenter.Instance.AddListener(MsgType.ShopView_OnlyShop, OnlyShopModel);
        MessageCenter.Instance.AddListener(MsgType.ShopView_ShopAndBoard, ShopAndBoardModel);
        MessageCenter.Instance.AddListener(MsgType.ShopView_CancelPoint,CancelClickPoint);

    }

    private void UnRegisterMsg()
    {
        MessageCenter.Instance.RemoveListener(MsgType.ShopView_LoadRoute, LoadShopRoute);
        MessageCenter.Instance.RemoveListener(MsgType.ShopView_ShopItemClicked, ClickShopItem);
        MessageCenter.Instance.RemoveListener(MsgType.ShopView_SampleBoardClicked, ClickSampleBoardItem);
        MessageCenter.Instance.RemoveListener(MsgType.ShopView_NewPoint, NewPoint);
        MessageCenter.Instance.RemoveListener(MsgType.ShopView_DeleteShop, DeleteShopItem);
        MessageCenter.Instance.RemoveListener(MsgType.ShopView_DeleteBoard, DeleteBoardItem);
        MessageCenter.Instance.RemoveListener(MsgType.ShopView_LocalSave, SaveEditingShopDataConfig2Local);
        MessageCenter.Instance.RemoveListener(MsgType.ShopView_OnlyBoard, OnlyBoardModel);
        MessageCenter.Instance.RemoveListener(MsgType.ShopView_OnlyShop, OnlyShopModel);
        MessageCenter.Instance.RemoveListener(MsgType.ShopView_ShopAndBoard, ShopAndBoardModel);
        MessageCenter.Instance.RemoveListener(MsgType.ShopView_CancelPoint, CancelClickPoint);
    } 

    private void LoadShopRoute(Message _msg)
    {
        string strRoute = _msg["route"] as string;
        LoadShopConfig(() =>
        {
            LoadShopData(strRoute);
        }, (er) =>
         {
             LogicUtils.Instance.OnAlert(er);
         });
    }
    #endregion

    #region RefreshUI

    #endregion

    #region Load Shop Common Data
    /// <summary>
    /// 加载商铺公共：SCD资源
    /// </summary>
    /// <param name="onDone"></param>
    /// <param name="onFail"></param>
    public void LoadShopConfig(Action onDone = null, Action<string> onFail = null)
    {
        WWWManager.Instance.Get("extra_libs/fetch_info", new NameValueCollection()
        {
            { "name", "scd" }
        }, resp =>
        {
            if (resp.Error == HttpResp.ErrorType.None)
            {
                try
                {
                    ShopNetData configData = new ShopNetData(resp.WwwText);

                    anaShopConfig(configData);

                    loadShopConfigAB(configData, () =>
                    {
                        if (onDone != null)
                            onDone();

                    }, onFail);
                }
                catch (Exception exc)
                {
                    string strTips = ("获取商铺配置出错: " + exc.Message);
                    if (onFail != null)
                        onFail(strTips);
                    return;
                }
            }
            else
            {
                string strTips = ("获取商铺配置出错: " + resp.Error++ + "  " + resp.WwwText);
                if (onFail != null)
                    onFail(strTips);
            }
        });
    }


    void anaShopConfig(ShopNetData snd)
    {
        Debug.Log("解析商铺配置表...");
        string shopConfigStr = snd.conf;
        JsonData shopConfigJD = JsonMapper.ToObject(shopConfigStr);

        shopTypeDic = new Dictionary<ShopType, List<ShopVO>>();
        for (int i = 0, length = shopConfigJD.Count; i < length; i++)
        {
            JsonData dataJD = shopConfigJD[i];
            if (i == 0)
            {
                List<ShopType> typeList = JsonUtils.ToItemVOList<ShopType>(dataJD["data"]);
                foreach (var shopType in typeList)
                {
                    shopTypeDic.Add(shopType, new List<ShopVO>());
                }
            }
            else
            {
                string shopTypeCode = dataJD["type"].ToString();

                List<ShopVO> shopList = dataJD["data"].ToItemVOList<ShopVO>();
                foreach (var shopVO in shopList)
                {
                    shopNameDic.AddRep(shopVO.code, shopVO.name);
                    shopTexDic.AddRep(shopVO.code, shopVO.tex);

                }
                ShopType type = shopTypeDic.Keys.FindItem<ShopType>(shopType => { return shopType.code == shopTypeCode; });
                shopTypeDic[type] = shopList;
            }
        }
        Debug.Log("商铺配置表解析完成");
    }

    #endregion

    #region Load Shop Config
    /// <summary>
    /// 加载走向图配置
    /// </summary>
    /// <param name="routeID"></param>
    public void LoadShopData(string routeID)
    {
        loadLocalShopData(routeID, onAnalysisError, onLocalNotExist, true);
    }

    /// <summary>加载本地商铺</summary>
    bool loadLocalShopData(string routeID, Action<string, string> onAnalysisError, Action<string, string> onLocalNotExist, bool needCreateShop)
    {
        CancelClickPoint();//清空打点

        string configStr = null;
        string shopListStr = null;
        string[] picPathArr = null;
        string checkExistMsg = checkLocalShopDataExist(routeID, ref configStr, ref shopListStr, ref picPathArr);

        if (checkExistMsg == "")
        {
            // 这里注释掉，从本地读取招牌数据
            // 单粒度招牌 拼接替换config
            //JHQCHelper.Instance.OnLoadSingleShopList(routeID, () =>
            //{
                // 重新读取
                string shopDataFolderPath = LocalDataPath + routeID + "/";
                string configPath = shopDataFolderPath + FileName_config;
                configStr = File.ReadAllText(configPath);

                if (tryLoadLocalShopData(routeID, configStr, shopListStr, picPathArr, needCreateShop) == "")
                {
                    curRouteID = routeID;
                    ShopRouteData srd = new ShopRouteData(routeID, configStr, shopListStr, picPathArr);
                    theAllShopRouteDic.AddRep(routeID, srd);

                    // TODO:刷新UI
                    // 同步招牌图片数据
                    for (int i = 0; i < theOriShopSignList.Count; i++)
                    {
                        ShopSignVO ssv = theOriShopSignList[i];
                        GetShopTexByCode(ssv.ShopCode, ssv.CellNum, (tx) =>
                         {
                             ssv.m_txtShop = tx;
                         });

                        ssv.m_strShopShowName = GetShopTypeByCode(ssv.ShopCode);
                    }

                    Message msg = new Message(MsgType.ShopView_RefreshSampleBoard, this);
                    msg["data"] = theOriShopSignList;
                    msg.Send();

                    RefreshShopList();
                }
                else
                {
                    if (onAnalysisError != null)
                        onAnalysisError(routeID, checkExistMsg);
                }

            //});

            return true;
        }
        else
        {
            if (onLocalNotExist != null)
                onLocalNotExist(routeID, checkExistMsg);
            return false;
        }
    }

    void onAnalysisError(string routeID, string checkExistMsg)
    {
        LogicUtils.Instance.OnAlert("走向图" + routeID + "本地数据格式不正确: " + checkExistMsg);
    }

    void onLocalNotExist(string routeID, string checkExistMsg)
    {
        if (checkExistMsg.Contains(errorNoConfigTxt) && checkExistMsg.Contains(errorNoShopListTxt))
        {
            //对都不存在做弹窗处理
            LogicUtils.Instance.OnAlert("走向图" + routeID + "本地数据不完整: " + checkExistMsg + "\r\n是否从服务器获取? 或者新建config.txt和从Excel生成",
                () => { loadFromServerWithFinishDialog(routeID); },
                () =>
                {
                    LogicUtils.Instance.OnAlert("是否从Excel生成shopList.txt?", () => { createShopListFromExcel(routeID); });
                    LogicUtils.Instance.OnAlert("是否新建config.txt?", () => { createNewConfigTxt(routeID); });
                },
                "从服务器下载", "新建");
        }
        else
        {
            //对两个配置分别不存在的情况分别弹窗处理
            if (checkExistMsg.Contains(errorNoConfigTxt))
            {
                LogicUtils.Instance.OnAlert("走向图" + routeID + "本地数据不完整: " + errorNoConfigTxt + "\r\n是否从服务器获取? 或者新建config.txt",
                    () => { loadFromServerWithFinishDialog(routeID); },
                    () => { createNewConfigTxt(routeID); },
                    "从服务器下载", "新建config.txt");
            }

            if (checkExistMsg.Contains(errorNoShopListTxt))
            {
                LogicUtils.Instance.OnAlert("走向图" + routeID + "本地数据不完整: " + errorNoShopListTxt + "\r\n是否从服务器获取? 或者从Excel生成shopList.txt?",
                    () => { loadFromServerWithFinishDialog(routeID); },
                    () => { createShopListFromExcel(routeID); },
                    "从服务器下载", "生成shopList.txt");
            }
        }

    }

    void createNewConfigTxt(string routeID)
    {
        string txtPath = LocalDataPath + routeID + "/config.txt";
        JsonData newConfigTxt = JsonUtils.EmptyJsonObject;
        newConfigTxt["data"] = JsonUtils.EmptyJsonArray;
        File.WriteAllText(txtPath, newConfigTxt.ToJson());
        LogicUtils.Instance.OnAlert("已创建新的config.txt, 请重新加载");
    }

    public static string Change(string excelPath)
    {
        string main = "";
        using (ReadWriteCsv.CsvFileReader reader = new ReadWriteCsv.CsvFileReader(excelPath, Encoding.Default))
        {
            ReadWriteCsv.CsvRow row = new ReadWriteCsv.CsvRow();

            while (reader.ReadRow(row))
            {
                string rowStr = "";
                foreach (string s in row)
                {
                    rowStr += s + "\t";
                }
                // 原来是11个Tab，但是替换的时候12个
                rowStr += "\t\t\t\t\t\t\t\t\t\t\t\t";
                main += rowStr;
            }

        }
        return main;
    }

    void createShopListFromExcel(string routeID)
    {
        string excelPath = LocalDataPath + routeID + "/" + routeID + ".csv";
        string shopListPath = LocalDataPath + routeID + "/shopList.txt";
        if (File.Exists(excelPath))
        {
            File.WriteAllText(shopListPath, Change(excelPath));//NOTE 检查生成是否是UTF8无BOM格式
            LogicUtils.Instance.OnAlert("shopList.txt创建完成, 请重新加载!");
        }
        else
            LogicUtils.Instance.OnAlert("未找到" + routeID + ".csv文件!");
    }

    /// <summary>从服务器下载, 完成后询问是否加载</summary>
    void loadFromServerWithFinishDialog(string routeID)
    {
        loadServerShopData(routeID, () =>
        {
            LogicUtils.Instance.OnAlert("走向图" + routeID + "下载完成, 是否加载?",
                () => { loadLocalShopData(routeID, onAnalysisError, onLocalNotExist, true); },
                () => { }
                );
        });
    }

    string errorNoConfigTxt = "缺少config.txt    ";
    string errorNoShopListTxt = "缺少shopList.txt    ";
    string errorNoSignFolder = "缺少sign文件夹    ";

    /// <summary>检查商铺数据完整性</summary>
    string checkLocalShopDataExist(string routeID, ref string configStr, ref string shopListStr, ref string[] picPathArr)
    {
        string shopDataFolderPath = LocalDataPath + routeID + "/";
        string configPath = shopDataFolderPath + FileName_config;
        string shopListPath = shopDataFolderPath + FileName_shopList;
        string signFolderPath = shopDataFolderPath + FolderName_Sign;
        string errorMsg = "";

        if (File.Exists(configPath))
            configStr = File.ReadAllText(configPath);
        else
            errorMsg += errorNoConfigTxt;
        if (File.Exists(shopListPath))
            shopListStr = File.ReadAllText(shopListPath);
        else
            errorMsg += errorNoShopListTxt;
        if (Directory.Exists(signFolderPath))
            picPathArr = Directory.GetFiles(signFolderPath);
        else
            errorMsg += errorNoSignFolder;
        return errorMsg;
    }

    /// <summary>尝试解析商铺数据</summary>
    string tryLoadLocalShopData(string routeID, string configStr, string shopListStr, string[] picPathArr, bool needCreateShop)
    {
        string errorMsg = "";
        errorMsg += anaShopInfo(shopListStr, routeID, out theOriShopSignList);
        errorMsg += anaShopSignConfig(configStr, routeID, out theShopModelList, needCreateShop);

        return errorMsg;
    }

    /// <summary>解析Config.txt</summary>
    string anaShopSignConfig(string fileStr, string routeID, out List<ShopModelMono> shopModelList, bool needCreateShop)
    {
        string errorMsg = "";
        shopModelList = new List<ShopModelMono>();

        // 清除原有的招牌模型 
        Utils.RemoveChildren(ShopModelFolder.transform);

        JsonData dataJD;
        try
        {
            dataJD = JsonMapper.ToObject(fileStr).ReadJsonData("data");
        }
        catch (Exception e)
        {
            errorMsg += "商铺配置的json格式不正确" + e.Data;
            return errorMsg;
        }
        if (!dataJD.IsArray)
        {
            errorMsg += "商铺配置的json格式不正确(not an array)";
            return errorMsg;
        }
        for (int i = 0, length = dataJD.Count; i < length; i++)
        {
            JsonData shopJD = dataJD[i];
            ShopModelMono smm = createShopContainerWorld(shopJD["pointList"].ToVec3List(), shopJD["buildName"].ToString());
            smm.ReadJsonData(shopJD);

            for (int shopIndex = 0, shopCount = smm.ShopSignList.Count; shopIndex < shopCount; shopIndex++)
            {
                smm.ShopSignList[shopIndex].RouteID = routeID;
                shopIdDic.AddToList(smm.BuildingName, smm.ShopSignList[shopIndex]);
            }

            shopModelList.Add(smm);
        }
        theAllShopModelDic.AddRep(routeID, shopModelList);
        if (needCreateShop)
        {
            MyFrameWork.MonoHelper.Instance.StartCoroutine(CreateShop());
        }
        return errorMsg;
    }

    private IEnumerator CreateShop()
    {
        for (int i = 0; i < theShopModelList.Count; i++)
        {
            theShopModelList[i].CreateShops();
            yield return new WaitForEndOfFrame();
        }

        yield return null;
    }

    /// <summary>解析ShopList.txt</summary>
    string anaShopInfo(string fileStr, string routeID, out List<ShopSignVO> oriShopSignList)
    {
        string errorMsg = "";
        oriShopSignList = new List<ShopSignVO>();

        string enterStr = "\t\t\t\t\t\t\t\t\t\t\t\t";
        fileStr = fileStr.Replace(enterStr, "\r\n");

        string[] splitLine = new string[] { "\r\n" };
        string[] split = new string[] { "\t" };
        string[] lineArr = fileStr.Split(splitLine, System.StringSplitOptions.None);
        if (lineArr.Length < 1)
        {
            errorMsg += "商铺数据格式不正确: " + fileStr;
            return errorMsg;
        }

        for (int lineIndex = 1, lineNum = lineArr.Length; lineIndex < lineNum; lineIndex++)
        {
            string lineStr = lineArr[lineIndex];
            string[] dataArr = lineStr.Split(split, System.StringSplitOptions.None);
            ShopSignVO ss = new ShopSignVO();
            if (dataArr.Length > 4)
            {
                ss.SignCode = dataArr[0].Trim();
                ss.NormalizeSignCode();
                ss.ShopName = dataArr[1].Trim();
                int.TryParse(dataArr[2].Trim(), out ss.CellNum);
                if (ss.CellNum <= 0)
                    ss.CellNum = 1;
                ss.ShopCode = getShopCodeByName(dataArr[3]);
                string prefabType = dataArr[4].Trim();
                if (prefabType == "只有铺面")
                    ss.PrefabType = ShopSignVO.PrefabTypeOnlyShop;
                else if (prefabType == "只有招牌")
                    ss.PrefabType = ShopSignVO.PrefabTypeOnlySign;
                else
                    ss.PrefabType = ShopSignVO.PrefabTypeShopSign;
                ss.RouteID = routeID;
            }
            if (ss.ShopName != "" && ss.ShopName != null)
                oriShopSignList.Add(ss);
        }
        return errorMsg;
    }

    /// <summary>按铺面名称获取铺面编码</summary>
    string getShopCodeByName(string shopTypeName)
    {
        if (shopTypeName == null)
            return null;
        foreach (var pair in shopNameDic)
        {
            if (pair.Value == shopTypeName)
                return pair.Key;
        }
        return null;
    }

    /// <summary>从服务器获取</summary>
    void loadServerShopData(string routeID, Action onDone)
    {
        Debug.Log("加载服务器数据...");
        HttpService.GetExtralRouteData(PreFix_RouteID + routeID, strJd =>
        {
            ShopNetData snd = new ShopNetData(strJd);

            downloadRouteFolder(snd, routeID, onDone);
        }, resp =>
        {
            LogicUtils.Instance.OnAlert("从服务器获取" + routeID + "商铺信息失败! 请检查后重试");
        });
    }

    string loadConditionToString(int loadCondition)
    {
        if (loadCondition == 0)
            return "加载中";
        else if (loadCondition == 1)
            return "加载完成";
        else if (loadCondition == 2)
            return "加载失败";
        else
            return null;
    }

    void downloadRouteFolder(ShopNetData snd, string routeID, Action onDone, Action<string> onFail = null, Action<string> onProgress = null, bool needAlertWin = true)
    {
        string targetShopDataFolder = LocalDataPath + routeID + "/";
        if (!Directory.Exists(targetShopDataFolder))
        {
            Directory.CreateDirectory(targetShopDataFolder);
        }
        int infoLoadCondition = 0;
        int configLoadCondition = 0;
        int picArrLoadCondition = 0;
        Action onInfoConfigLoaded = () =>
        {
            if (infoLoadCondition != 0 && configLoadCondition != 0 && picArrLoadCondition != 0)
            {
                string infoLoadConditionStr = "shopList.txt" + (infoLoadCondition == 1 ? "加载成功" : "加载失败");
                string configLoadConditionStr = "config.txt" + (configLoadCondition == 1 ? "加载成功" : "加载失败");
                string picArrLoadConditionStr = "招牌图片" + (picArrLoadCondition == 1 ? "加载成功" : "加载失败");
                string downloadReport = infoLoadConditionStr + "\r\n" + configLoadConditionStr + "\r\n" + picArrLoadConditionStr;
                Debug.Log(downloadReport);
                if (needAlertWin)
                    LogicUtils.Instance.OnAlert(downloadReport);
                if (infoLoadCondition == 1 && configLoadCondition == 1 && picArrLoadCondition == 1)
                    onDone();
            }
        };

        //1.下载info
        if (snd.info != null)
        {
            //判断 info是否为url, 分别处理
            if (snd.info.IndexOf("http") != -1)
            {
                Debug.Log("snd.info:" + snd.info);
                HttpService.GetRemoteRaw(snd.info, byteArr =>
                {
                    File.WriteAllBytes(targetShopDataFolder + "shopList.txt", byteArr);
                    infoLoadCondition = 1;
                    onInfoConfigLoaded();
                    Debug.Log("shopList.txt");
                }, hr =>
                {
                    infoLoadCondition = 2;
                    Debug.LogError("加载shopList.txt失败: " + hr.Error + ", " + hr.WwwText);
                    onFail("加载shopList.txt失败: " + hr.Error + ", " + hr.WwwText);
                });
            }
            else if (snd.info.IndexOf(NameCrc.StrSplit) != -1)
            {
                string fileName;
                string fileCrc;
                NameCrc.GetNameAndCrcFromKey(snd.info, out fileName, out fileCrc);
                JHQCHelper.DownloadFile(fileName, fileCrc, targetShopDataFolder + "shopList.txt",
                    () =>
                    {
                        infoLoadCondition = 1;
                        onInfoConfigLoaded();
                    },
                    (errorType, errorMsg) =>
                    {
                        infoLoadCondition = 2;
                        Debug.LogError("加载shopList.txt失败: " + errorType + ", " + errorMsg);
                        onFail("加载shopList.txt失败: " + errorType + ", " + errorMsg);
                    });
            }
        }
        else
        {
            Debug.LogError("服务器未找对应的shopList.txt的地址");
            if (onFail != null)
            {
                onFail("服务器未找对应的shopList.txt的地址");
            }

            if (needAlertWin)
                LogicUtils.Instance.OnAlert("服务器未找对应的shopList.txt的地址!");
            infoLoadCondition = 2;
            onInfoConfigLoaded();
        }

        //2.下载config
        if (!string.IsNullOrEmpty(snd.conf))
        {
            string fileName;
            string fileCrc;
            NameCrc.GetNameAndCrcFromKey(snd.conf, out fileName, out fileCrc);
            string configPath = targetShopDataFolder + "config.txt";
            JHQCHelper.DownloadFile(fileName, fileCrc, configPath,
                () =>
                {
                    configLoadCondition = 1;
                    onInfoConfigLoaded();

                    //3.解析config 并下载图片
                    JsonData configJD = JsonMapper.ToObject(File.ReadAllText(configPath));
                    string picFolder = targetShopDataFolder + "sign/";
                    if (!Directory.Exists(picFolder))
                        Directory.CreateDirectory(picFolder);

                    //显示进度条
                    JsonData picJD = configJD.ReadJsonData("picNameCrc");
                    if (picJD != null)
                    {
                        int totalCount = configJD.ReadJsonData("picNameCrc").Keys.Count;

                        LogicUtils.Instance.OnShowWaiting(2, "正在下载" + routeID + "区块的招牌图片", false, totalCount);

                        LoadAllPic(configJD, picFolder, progressStr =>
                        {
                            Debug.Log("进度: " + progressStr);
                            if (onProgress != null)
                            {
                                onProgress(
                                    "shopList.txt" + loadConditionToString(infoLoadCondition) +
                                    ", config.txt" + loadConditionToString(configLoadCondition) +
                                    ", 图片文件夹 " + progressStr);
                            }

                            LogicUtils.Instance.OnPopWaiting(2);
                        }, () =>
                        {
                            picArrLoadCondition = 1;
                            onInfoConfigLoaded();
                            LogicUtils.Instance.OnHideWaiting();
                        });
                    }
                },
                (errorType, errorMsg) =>
                {
                    configLoadCondition = 2;
                    JHQCHelper.OnReqFail("加载shopList.txt失败: ", errorType, errorMsg);
                    onFail("加载shopList.txt失败: " + errorType + ", " + errorMsg);
                });
        }
        else
        {
            Debug.LogError("未找到config.txt的地址");
            if (onFail != null)
            {
                onFail("未找到config.txt的地址");
            }
            if (needAlertWin)
                LogicUtils.Instance.OnAlert("未找到config.txt的地址");
            configLoadCondition = 2;
            onInfoConfigLoaded();
        }
    }

    /// <summary>
    /// 下载商铺图片，写入本地商铺文件夹
    /// </summary>
    /// <param name="configJD"></param>
    /// <param name="targetFolder"></param>
    /// <param name="onProgress"></param>
    /// <param name="onDone"></param>
    void LoadAllPic(JsonData configJD, string targetFolder, Action<string> onProgress, Action onDone)
    {
        //TODO 170321 做本地文件夹检查
        JsonData picArrJD = configJD.ReadJsonData("picNameCrc");
        int totalCount = picArrJD.Keys.Count;
        int curCount = 0;

        if (totalCount == 0)
        {
            onDone();
            return;
        }

        foreach (var fileName in picArrJD.Keys)
        {
            string fileCrc = picArrJD.ReadString(fileName);
            picName_picCrc.AddOrReplace(fileName, fileCrc);

            QueueManager.Instance.Add(onTaskDone =>
            {
                JHQCHelper.DownloadFile(fileName, fileCrc, targetFolder + fileName,
                    () =>
                    {
                        curCount++;
                        onProgress(curCount + "/" + totalCount);
                        onTaskDone();
                        if (curCount == totalCount)
                            onDone();
                    },
                    (errorType, errorMsg) =>
                    {
                        curCount++;
                        onProgress(curCount + "/" + totalCount);
                        onTaskDone();
                        JHQCHelper.OnReqFail("图片加载失败, 请检查网络: ", errorType, errorMsg);
                        if (curCount == totalCount)
                            onDone();
                    });

            }, 1, fileName + "_" + fileCrc);

        }
    }

    /// <summary>
    /// 商铺信息保存到本地，这里没有拆分为单粒度数据
    /// </summary>
    void SaveEditingShopDataConfig2Local(Message _msg)
    {
        string routeID = curRouteID;
        JsonData saveJD = JsonUtils.EmptyJsonObject;
        saveJD["routeID"] = routeID;
        saveJD["shopNum"] = theShopModelList.Count.ToString();
        saveJD["version"] = Utils.Time2String(); ;
        saveJD["data"] = theShopModelList.ToJsonDataList();
        JsonData fileArrJD = saveJD["picNameCrc"] = JsonUtils.EmptyJsonObject;
        string[] fileArr = Directory.GetFiles(LocalDataPath + routeID + "/sign");
        for (int iFile = 0, nFile = fileArr.Length; iFile < nFile; iFile++)
        {
            string filePath = fileArr[iFile];
            string fileName = Utils.GetFileNameByPath(filePath, true).ToLower();
            if (!Utils.IsPic(filePath))
                continue;
            string fileCrc = Crc32.CountCrcRetString(File.ReadAllBytes(filePath));

            picName_picCrc.AddOrReplace(fileName, fileCrc);

            fileArrJD[fileName] = fileCrc;
        }

        string saveStr = saveJD.ToJson();
        File.WriteAllText(LocalDataPath + routeID + "/config.txt", saveStr);

        // 预防单粒度招牌计算错误，本地预留永久备份
        //Utils.SimpleSaveInfo("config_" + routeID, saveStr);

        LogicUtils.Instance.OnAlert("本地保存成功!");
    }

    /// <summary>获取保存到云端的商铺信息的格式</summary>
    string getExtraLibData(string routeID, string configCrc, string shopListCrc)
    {
        string xx = "{\"name\":\"zp" + routeID + "\", \"conf\":\"Route_zp" + routeID + "_ShopConfig.txt#Name____Crc#" + configCrc + "\", \"info\":\"Route_zp" + routeID + "_ShopList.txt#Name____Crc#" + shopListCrc + "\"}";
        return xx;
    }
    #endregion

    #region Point
    List<Vector3> pointList;
    GameObject ball1;
    GameObject ball2;
    GameObject ball3;
    void initBall()
    {
        if (ball1 == null)
        {
            Vector3 ballScale = new Vector3(0.3f, 0.3f, 0.3f);
            ball1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ball1.transform.localScale = ballScale;
            ball1.name = "ball1";
            ball1.SetActive(false);
            ball2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ball2.transform.localScale = ballScale;
            ball2.SetActive(false);
            ball2.name = "ball2";
            ball3 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ball3.transform.localScale = ballScale;
            ball3.SetActive(false);
            ball3.name = "ball3";
        }
    }
    string strOldGUID;
    private void onClickPoint(string buildGUID, Vector3 worldVec)
    {

        initBall();
        if (pointList == null)
        {
            pointList = new List<Vector3>();
        }

        if (pointList.Count == 0)
        {
            strOldGUID = buildGUID;
        }
        else if (!strOldGUID.Equals(buildGUID))
        {
            return;
        }

        pointList.Add(worldVec);

        switch (pointList.Count)
        {
            case 1:
                ball1.SetActive(true);
                ball1.transform.position = worldVec;
                break;
            case 2:
                ball2.SetActive(true);
                ball2.transform.position = worldVec;
                break;
            case 3:
                ball3.SetActive(true);
                ball3.transform.position = worldVec;
                break;
            default:
                break;
        }

        if (pointList.Count == 4)
        {
            Debug.Log("创建框体");
            ShopModelMono smm = createShopContainerWorld(pointList, buildGUID);
            AddShopSignEditorVo(smm, "signFile");
            theShopModelList.Insert(0,smm);

            ball1.SetActive(false);
            ball2.SetActive(false);
            ball3.SetActive(false);
            CurSelectShopModel = smm;
            pointList = null;
            smm.CreateShops(true);
            strOldGUID = string.Empty;
            RefreshShopList();
        }
    }

    void AddShopSignEditorVo(ShopModelMono _mono, string _type = null)
    {
        ShopSignEditorVo vo = new ShopSignEditorVo();
        if (_mono != null)
        {
            vo.undo = _mono.ToJsonData();
            vo.type = _type;
            vo.shopModel = _mono;
        }
        shopSignEditorVoList.Add(vo);
    }

    private void CancelClickPoint(Message _msg)
    {
        CancelClickPoint();
    }

    /// <summary>
    /// Cancel Point
    /// </summary>
    private void CancelClickPoint()
    {
        pointList = null;
        //UILabel stepLabel = GameObject.Find("Step2").GetComponent<UILabel>();
        //stepLabel.text = step2Text + "[选择左下角点]";
        if (ball1 != null && ball2 != null && ball3 != null)
        {
            ball1.SetActive(false);
            ball2.SetActive(false);
            ball3.SetActive(false);
        }
        //CurStep = 2;
    }

    /// <summary>
    /// Undo Point
    /// </summary>
    private void UndoClickPoint()
    {
        initBall();
        if (pointList == null)
        {
            pointList = new List<Vector3>();
        }
        // UILabel stepLabel = GameObject.Find("Step2").GetComponent<UILabel>();
        switch (pointList.Count)
        {
            case 1:
                ball1.SetActive(false);
                //stepLabel.text = step2Text + "[选择左上角点]";
                pointList.RemoveAt(0);
                break;
            case 2:
                ball2.SetActive(false);
                //stepLabel.text = step2Text + "[选择左上角点]";
                pointList.RemoveAt(1);
                break;
            case 3:
                ball3.SetActive(false);
                // stepLabel.text = step2Text + "[选择右上角点]";
                pointList.RemoveAt(2);
                break;
            default:
                break;
        }
    }
    #endregion

    #region Shop Resource
    AssetBundle shopAB;
    /// <summary>
    /// 获取招牌公共资源(右侧招牌模版的铺面部分)
    /// </summary>
    /// <param name="snd"></param>
    /// <param name="onDone"></param>
    /// <param name="onFail"></param>
    void loadShopConfigAB(ShopNetData snd, Action onDone, Action<string> onFail)
    {
        string strShopABPath = UnityEngine.Application.streamingAssetsPath + "\\ShopAB.assetbundle";
        if (File.Exists(strShopABPath))
        {
            try
            {
                shopAB = AssetBundle.LoadFromFile(strShopABPath);
                if (shopAB != null)
                {
                    onDone();
                }
            }
            catch (Exception)
            {
                LoadShopConfigABFromNet(snd, onDone, onFail);
            }

        }
        else
        {
            LoadShopConfigABFromNet(snd, onDone, onFail);
        }
    }

    void LoadShopConfigABFromNet(ShopNetData snd, Action onDone, Action<string> onFail)
    {
        HttpService.GetRemoteAB(snd.url, ab =>
        {
            shopAB = ab;
            if (shopAB != null)
                onDone();
            else
                onFail("获取到的铺面AB文件为null!");
        }, null);
    }

    public string GetShopTypeByCode(string shopCode)
    {
        if (shopCode == null)
            return "null";
        string shopType;
        shopNameDic.TryGetValue(shopCode, out shopType);
        if (shopType == null)
            return "null";
        return shopType;
    }

    /// <summary>
    /// 获取图片Crc
    /// </summary>
    /// <param name="_strPicName"></param>
    /// <returns></returns>
    public string OnGetPicCrcByName(string _strPicName)
    {
        string strCrc = "Error";
        if (picName_picCrc.ContainsKey(_strPicName))
        {
            strCrc = picName_picCrc[_strPicName];
        }
        return strCrc;
    }

    public void GetShopTexByCode(string shopCode, int cellNum, Action<Texture2D> callback)
    {
        if (shopCode == null)
        {
            callback(Texture2D.whiteTexture);
            return;
        }
        string cellPostfix = "_" + (cellNum <= 3 ? cellNum.ToString() : "3");

        string picName;
        if (shopTexDic.TryGetValue(shopCode, out picName))
        {
            Texture2D tex = shopAB.LoadAsset<Texture2D>(shopTexDic[shopCode] + cellPostfix);
            callback(tex);
        }
        else
        {
            Debug.Log("未能获取到商铺图片, 错误的ShopCode: " + shopCode + ", cell:" + cellNum);
            callback(Texture2D.whiteTexture);
        }

    }

    // 加载招牌图片，TMD，竟然是下载到本地，从本地读取的，没有用到CRC
    public void GetSignTexByCode(string signCode, string routeID, Action<Texture2D> callback)
    {
        string filePath = LocalDataPath + routeID + "/sign/" + signCode;
        if (!string.IsNullOrEmpty(signCode) && File.Exists(filePath))
            ResManager.Instance.OnLoadLocalTexture(filePath, tex2D => { callback(tex2D); });
        else
            callback(Texture2D.whiteTexture);
    }

    #endregion

    #region Shop Model & Editor
    GameObject shopModelFolder;
    /// <summary>
    /// 商铺模型挂载点
    /// </summary>
    public GameObject ShopModelFolder
    {
        get
        {
            if (shopModelFolder == null)
            {
                shopModelFolder = new GameObject();
                shopModelFolder.name = "shopModelFolder";
            }
            return shopModelFolder;
        }
    }

    void loadModelPrefab(Action onDone)
    {
        string prefabPath = UnityEngine.Application.streamingAssetsPath + "/ShopRes/ShopCellModel.assetbundle";

        ResManager.Instance.OnLoadLocalAbByPath(prefabPath, (ab) =>
        {
            shopPrefabDic = new Dictionary<string, GameObject>();
            GameObject all = ab.mainAsset as GameObject;
            for (int i = 0, length = all.transform.childCount; i < length; i++)
            {
                GameObject child = all.transform.GetChild(i).gameObject;
                shopPrefabDic.AddRep(child.name, child);
            }
            cellNameDic = new Dictionary<int, string>();
            cellNameDic.Add(1, "sp_aa");
            cellNameDic.Add(2, "sp_ab");
            cellNameDic.Add(3, "sp_ac");
            cellNameDic.Add(4, "sp_ad");
            cellNameDic.Add(5, "sp_ae");
            cellNameDic.Add(6, "sp_af");
            cellNameDic.Add(7, "sp_ag");
            for (int i = 8; i < 50; i++)
            {
                cellNameDic.AddRep(i, "sp_ag");
            }
            OnlyShopPrefab = OnlyShopPrefab == null ? Resources.Load("ShopRes/OnlyShopPrefab") as GameObject : OnlyShopPrefab;
            OnlySignPrefab = OnlySignPrefab == null ? Resources.Load("ShopRes/OnlySignPrefab") as GameObject : OnlySignPrefab;

            //Debug.Log("商铺模型加载完成" + OnlyShopPrefab + "" + OnlySignPrefab);
            onDone();
        });
    }


    public GameObject CreateShopModel(int cellNum, string prefabType = "ShopSign")
    {
        switch (prefabType)
        {
            case "ShopSign":
                string index = cellNameDic[cellNum];

                GameObject shopSignGO = GameObject.Instantiate(shopPrefabDic[index]);

                Utils.ForAllChildren(shopSignGO, go =>
                {
                    Renderer r = go.GetComponent<Renderer>();
                    if (r != null)
                    {
                        for (int i = 0, len = r.materials.Length; i < len; i++)
                        {
                            r.materials[i].shader = Shader.Find("Legacy Shaders/Diffuse");
                        }
                    }
                });
                return shopSignGO;
            case "OnlyShop":
                GameObject shop = GameObject.Instantiate(OnlyShopPrefab);
                Vector3 oldShopScale = shop.transform.localScale;
                shop.transform.GetChild(0).localScale = new Vector3(3.8f * cellNum, 3.5f, 1);
                shop.transform.GetChild(0).localPosition = new Vector3(-1.9f * cellNum, 0, 1.75f);
                return shop;
            case "OnlySign":
                GameObject sign = GameObject.Instantiate(OnlySignPrefab);
                Vector3 oldsignScale = sign.transform.localScale;
                sign.transform.GetChild(0).localScale = new Vector3(3.8f * cellNum, 3.5f, 1);
                sign.transform.GetChild(0).localPosition = new Vector3(-1.9f * cellNum, 0, 1.75f);
                return sign;
            default:
                return shopPrefabDic[cellNameDic[cellNum]];
        }
    }

    public static ShopModelMono createShopContainerWorld(List<Vector3> pointList, string buildGUID)
    {
        //MeshFilter mf = Model.GetComponentInChildren<MeshFilter>();
        GameObject shopContainer = new GameObject();
        shopContainer.name = "shopModel";
        ShopModelMono shopModel = shopContainer.AddComponent<ShopModelMono>();
        shopModel.CellWidth = 3.8f;//TODO 从配置读取
        shopModel.BuildingName = buildGUID;
        shopModel.CreatePlane(pointList);
        return shopModel;
    }

    /// <summary>
    /// 招牌编辑：
    /// 移动：上下左右方向键
    /// 旋转：Q/E
    /// 缩放：按住Alt + 方向键
    /// </summary>
    void MoveShopModel()
    {
        if (!Utils.IsOnUI && CurSelectShopModel != null)
        {
            Vector3 dir = Vector3.zero;
            Vector3 rot = Vector3.zero;
            Vector3 sca = Vector3.zero;
            if (!Input.GetKey(KeyCode.LeftAlt))
            {
                if (Input.GetKeyDown(KeyCode.UpArrow))
                    dir += Vector3.forward;
                if (Input.GetKeyDown(KeyCode.DownArrow))
                    dir += Vector3.back;
                if (Input.GetKeyDown(KeyCode.LeftArrow))
                    dir += Vector3.right;
                if (Input.GetKeyDown(KeyCode.RightArrow))
                    dir += Vector3.left;
                if (Input.GetKeyDown(KeyCode.R))
                    dir += Vector3.up;
                if (Input.GetKeyDown(KeyCode.F))
                    dir += Vector3.down;
                if (CurSelectShopModel != null && dir != Vector3.zero)
                    CurSelectShopModel.Move(dir);

                if (Input.GetKeyDown(KeyCode.Q))
                    rot += Vector3.down;
                if (Input.GetKeyDown(KeyCode.E))
                    rot += Vector3.up;
                if (CurSelectShopModel != null && rot != Vector3.zero)
                    CurSelectShopModel.Rotate(rot);
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.UpArrow))
                    sca += Vector3.down;
                if (Input.GetKeyDown(KeyCode.DownArrow))
                    sca += Vector3.up;
                if (Input.GetKeyDown(KeyCode.LeftArrow))
                    sca += Vector3.right;
                if (Input.GetKeyDown(KeyCode.RightArrow))
                    sca += Vector3.left;
                if (CurSelectShopModel != null && sca != Vector3.zero)
                    CurSelectShopModel.Scale(sca);
            }

            if (dir != Vector3.zero || rot != Vector3.zero || sca != Vector3.zero)
                onDataChange();
        }
    }

    void onDataChange()
    {
        if (CurSelectShopModel != null)
        {
            CurSelectShopModel.CreateShops();
        }
    }

    #endregion

    #region UI Message
    /// <summary>
    /// Shop Selected
    /// </summary>
    /// <param name="_msg"></param>
    private void ClickShopItem(Message _msg)
    {
        string strGUID = _msg["guid"] as string;

        ShopModelMono smm = theShopModelList.Find((item) => { return item.guid.Equals(strGUID); });
        if (smm != null && smm != CurSelectShopModel)
        {
            CurSelectShopModel = smm;
        }
    }

    /// <summary>
    /// New Board
    /// </summary>
    /// <param name="_msg"></param>
    private void ClickSampleBoardItem(Message _msg)
    {
        ShopSignVO ssv = _msg["data"] as ShopSignVO;
        if (CurSelectShopModel != null)
        {
            ShopSignVO newSsv = ssv.Clone();
            GetShopTexByCode(ssv.ShopCode, ssv.CellNum, (tx) =>
            {
                newSsv.m_txtShop = tx;
                CurSelectShopModel.ShopSignList.Add(newSsv);
                CurSelectShopModel.CreateShops();
                Message msg = new Message(MsgType.ShopView_RefreshBoardList, this);
                msg["data"] = CurSelectShopModel.ShopSignList;
                msg.Send();
            });

        }
    }

    /// <summary>
    /// New Point
    /// </summary>
    /// <param name="_msg"></param>
    private void NewPoint(Message _msg)
    {
        string strGUID = _msg["guid"] as string;
        Vector3 vPos = (Vector3)_msg["pos"];

        onClickPoint(strGUID,vPos);
    }

    /// <summary>
    /// Delete Shop
    /// </summary>
    /// <param name="_msg"></param>
    private void DeleteShopItem(Message _msg)
    {
        string strGUID = _msg["guid"] as string;

        ShopModelMono smm = theShopModelList.Find((item) => { return item.guid.Equals(strGUID); });
        if (smm != null)
        {
            theShopModelList.Remove(smm);
            GameObject.Destroy(smm.gameObject);
            RefreshShopList();
        }
    }

    /// <summary>
    /// Delete Board 
    /// </summary>
    /// <param name="_msg"></param>
    private void DeleteBoardItem(Message _msg)
    {
        ShopSignVO ssv = _msg["data"] as ShopSignVO;
        if (CurSelectShopModel != null && CurSelectShopModel.ShopSignList.Contains(ssv))
        {
            CurSelectShopModel.ShopSignList.Remove(ssv);
            CurSelectShopModel.CreateShops();

            Message msg = new Message(MsgType.ShopView_RefreshBoardList, this);
            msg["data"] = CurSelectShopModel.ShopSignList;
            msg.Send();
            Debug.LogWarning(string.Format("当前商铺，还有招牌数：{0}",CurSelectShopModel.ShopSignList.Count));
        }
    }

    /// <summary>
    /// Refresh Left Shop List
    /// </summary>
    private void RefreshShopList()
    {
        List<Message> pList = new List<Message>();
        for (int i = 0; i < theShopModelList.Count; i++)
        {
            Message m = new Message("", this);
            m["guid"] = theShopModelList[i].guid;
            m["info"] = theShopModelList[i].GetText("");
            pList.Add(m);
        }

        CurSelectShopModel = null;

        Message msg = new Message(MsgType.ShopView_RefreshShopList, this);
        msg["data"] = pList;
        msg.Send();
    }

    /// <summary>
    /// Only Shop 
    /// </summary>
    /// <param name="_msg"></param>
    private void OnlyShopModel(Message _msg)
    {
        if (CurSelectShopModel != null && CurSelectShopModel.ShopSignList != null)
        {
            for (int iSS = 0, nSS = CurSelectShopModel.ShopSignList.Count; iSS < nSS; iSS++)
            {
                ShopSignVO ss = CurSelectShopModel.ShopSignList[iSS];
                ss.PrefabType = ShopSignVO.PrefabTypeOnlyShop;
            }
            CurSelectShopModel.CreateShops();
        }
    }

    /// <summary>
    /// Only Board
    /// </summary>
    /// <param name="_msg"></param>
    private void OnlyBoardModel(Message _msg)
    {
        if (CurSelectShopModel != null && CurSelectShopModel.ShopSignList != null)
        {
            for (int iSS = 0, nSS = CurSelectShopModel.ShopSignList.Count; iSS < nSS; iSS++)
            {
                ShopSignVO ss = CurSelectShopModel.ShopSignList[iSS];
                ss.PrefabType = ShopSignVO.PrefabTypeOnlySign;
            }
            CurSelectShopModel.CreateShops();
        }
    }

    /// <summary>
    /// Board And Shop
    /// </summary>
    /// <param name="_msg"></param>
    private void ShopAndBoardModel(Message _msg)
    {
        if (CurSelectShopModel != null && CurSelectShopModel.ShopSignList != null)
        {
            for (int iSS = 0, nSS = CurSelectShopModel.ShopSignList.Count; iSS < nSS; iSS++)
            {
                ShopSignVO ss = CurSelectShopModel.ShopSignList[iSS];
                ss.PrefabType = ShopSignVO.PrefabTypeShopSign;
            }
            CurSelectShopModel.CreateShops();
        }
    }
    #endregion
}

#region 辅助数据

public class ShopType
{
    public string name;
    public string code;
    public ShopType()
    {

    }
    public ShopType(string name, string code)
    {
        this.name = name;
        this.code = code;
    }

    public string GetLabelText()
    {
        return name;
    }
}
public class ShopVO
{
    public string code;

    public string name;
    /// <summary>全路径</summary>
    public string tex;

    public ShopVO()
    {

    }

    public string GetLabelText()
    {
        return name;
    }

    public override string ToString()
    {
        return "[ShopVO]name: " + name + ", code: " + code;
    }
}

public class ShopNetData
{

    public int id;

    public string name;

    public string url;

    public string info; //附加信息url

    public string conf; // 配置信息url, 存在特殊情况

    public ShopNetData(string _strJd)
    {
        JsonData jd = JsonMapper.ToObject(_strJd);
        jd = jd.ReadJsonData("extra_info");

        name = jd.ReadString("name");
        url = jd.ReadString("url");
        conf = jd.ReadString("conf");
        info = jd.ReadString("info");
    }
}

[Serializable]
public class LoginDataCollection : JsonBase
{
    public string access_token;
    public int user_id;
    public string role;
    public double server_time;
    public List<ShopNetData> extra_info;
}

[Serializable]
public class SingleShopNetDataCollection : JsonBase
{
    public ShopNetData extra_info;
}

/// <summary>
/// 招牌样板
/// </summary>
public class ShopSignVO : IJsonData, IListData
{
    public const string PrefabTypeShopSign = "ShopSign";
    public const string PrefabTypeOnlyShop = "OnlyShop";
    public const string PrefabTypeOnlySign = "OnlySign";

    public static string SignType_Normal = "SighType_Nomal";
    public static string SignType_Text = "SighType_Text";

    /// <summary>走向图ID TODO 在创建时赋予</summary>
    public string RouteID = "";

    public int CellNum = 1;//间数
    public string ShopName;//商铺名称
    public string ShopCode;//铺面贴图代码
    public string SignCode;//招牌贴图代码

    public string SignCrc;
    public string PrefabType = "ShopSign";//商铺模型类型
    public string ShopID = "1";//ID
    public string SignType = /*SignType_Text*/SignType_Normal;
    public string SignText = "我是招牌";
    const string DefaultPicExt = ".jpg";

    // 店铺图片
    public Texture2D m_txtShop;

    // 店铺显示名
    public string m_strShopShowName;

    public string m_strGuid;

    // 编辑器对新组建生成的guid
    public string m_strTempGuid;

    public string m_strBuildGuid;

    private JsonData m_pointList;

    //basicDataInputter 新增店铺大类
    public string shopClass = "";

    public bool IsSignText
    {
        private set { }
        get { return SignType == SignType_Text; }
    }

    public ShopSignVO()
    {
        // 新商铺，默认guid为-1
        m_strGuid = "-1";

        m_strTempGuid = Guid.NewGuid().ToString();
    }

    public ShopSignVO(int cellNum)
    {
        this.CellNum = cellNum;
        // 新商铺，默认guid为-1
        m_strGuid = "-1";

        m_strTempGuid = Guid.NewGuid().ToString();
    }

    /// <summary>
    /// 原始结构config保存，不需要guid，buildname，pointList
    /// </summary>
    /// <returns></returns>
    public JsonData ToJsonData()
    {
        JsonData jd = new JsonData();
        jd["CellNum"] = CellNum;
        jd["ShopName"] = ShopName;
        jd["ShopCode"] = ShopCode;
        jd["SignCode"] = SignCode;
        jd["SignCrc"] = SignCrc;
        jd["PrefabType"] = PrefabType.ToString();
        jd["ShopID"] = ShopID;
        jd["RouteID"] = RouteID;
        jd["SighType"] = SignType;
        jd["SighText"] = SignText;
        return jd;
    }

    /// <summary>
    /// 单粒度招牌数据
    /// </summary>
    /// <param name="_strBuildingGUID"></param>
    /// <param name="_pPointList"></param>
    /// <returns></returns>
    public JsonData GetSingleJsonData(string _strBuildingGUID, List<Vector3> _pPointList)
    {
        /*
         * "{\"CellNum\":\"1\",
         * \"guid\":\"1442217\",
         * \"BuildGuid\":\"e5298404-7ee0-43d9-a5aa-8d48b8445d63\",
         * \"ShopName\":\"\\u6B27\\u97E9\\u5916\\u8D38\",\
         * "ShopCode\":\"152\",
         * \"SignCode\":\"zp039403011080.jpg\",
         * \"SignCrc\":\"2544662481\",
         * \"PrefabType\":\"ShopSign\",
         * \"ShopID\":\"352179\",
         * \"RouteID\":\"057204001_songyiTest2\",
         * \"SighType\":\"SighType_Nomal\",
         * \"SighText\":\"\\u6211\\u662F\\u62DB\\u724C\",
         * \"PointList\":[\"1387.301,1.490,-2328.806\",\"1387.301,6.864,-2328.806\",\"1392.220,6.864,-2328.901\",\"1392.220,1.490,-2328.901\"]}"
         * \"PointList\":[\"1387.301,1.490,-2328.806\",\"1387.301,6.864,-2328.806\",\"1392.220,6.864,-2328.901\",\"1392.220,1.490,-2328.901\"],
         * \"BuildGuid\":\"e5298404-7ee0-43d9-a5aa-8d48b8445d63\"}"
         */
        JsonData jd = new JsonData();
        //jd["CellNum"] = "1";
        jd["CellNum"] = CellNum.ToString();
        jd["guid"] = m_strGuid;
        jd["BuildGuid"] = _strBuildingGUID;
        jd["ShopName"] = ShopName;
        jd["ShopCode"] = ShopCode;
        jd["SignCode"] = SignCode;
        jd["SignCrc"] = SignCrc;
        jd["PrefabType"] = PrefabType.ToString();
        jd["ShopID"] = ShopID;
        jd["RouteID"] = RouteID;
        jd["SighType"] = SignType;
        jd["SighText"] = SignText;

        List<string> dataList = _pPointList.ConvertAll<string>((vec) => { return JsonUtils.vecToStr(vec); });
        m_pointList = JsonUtils.ToJsonData(dataList);

        jd["PointList"] = m_pointList;
        jd["tmp_guid"] = m_strTempGuid;
        return jd;
    }

    public IJsonData ReadJsonData(JsonData jd)
    {
        CellNum = jd.ReadInt("CellNum");

        ShopName = jd.ReadString("ShopName");
        ShopCode = jd.ReadString("ShopCode");
        SignCode = jd.ReadString("SignCode");
        SignCrc = jd.ReadString("SignCrc");
        PrefabType = jd.ReadString("PrefabType", "");
        ShopID = jd.ReadString("ShopID", Guid.NewGuid().ToString());
        RouteID = jd.ReadString("RouteID", "");
        SignType = jd.ReadString("SighType", SignType_Normal);
        SignText = jd.ReadString("SighText", ShopName);

        m_strGuid = jd.ReadString("guid", "-1");
        m_strBuildGuid = jd.ReadString("BuildGuid", "null");

        m_pointList = jd.ReadJsonData("PointList");
        return this;
    }

    public void NormalizeSignCode()
    {
        SignCode = SignCode.ToLower();
        if (!SignCode.EndsWith(".jpg") && !SignCode.EndsWith(".png"))
        {
            SignCode = SignCode + ".jpg";
        }
    }

    Texture GetSignText()
    {
        if (IsSignText)
        {
            Bitmap bmp = Utils.TextToBitmapAllDefault(SignText);
            return Utils.ImageToTexture2D(bmp, true);
        }
        else
        {
            return null;
        }
    }

    public ShopSignVO Clone()
    {
        return new ShopSignVO().ReadJsonData(ToJsonData()) as ShopSignVO;
    }

    #region IListData
    public bool IsSelect { get; set; }

    public Action OnDataChange { get; set; }

    public string GetText(string paramName)
    {
        string val = null;
        switch (paramName)
        {
            case "ShopName":
                val = ShopName;
                break;
            case "ShopCode":
                val = m_strShopShowName;
                break;
            case "CellNum":
                val = CellNum.ToString() + "间";
                break;
            case "ShopID":
                val = ShopID;
                break;
            case "PrefabType":
                switch (PrefabType)
                {
                    case "ShopSign":
                        val = "店铺+招牌";
                        break;
                    case "OnlyShop":
                        val = "只有店铺";
                        break;
                    case "OnlySign":
                        val = "只有招牌";
                        break;
                }
                break;
        }
        return val;
    }

    public void GetTexture(string paramName, Action<Texture2D> onGet)
    {
        switch (paramName)
        {
            case "Sign":
                GetSignTexByCode(SignCode, RouteID, onGet);
                break;
            case "Shop":
                onGet(m_txtShop);
                break;
        }
    }

    public void GetSignTexByCode(string signCode, string routeID, Action<Texture2D> callback)
    {
        string LocalDataPath = Utils.GetDataPath() + "ShopData/";
        string filePath = LocalDataPath + routeID + "/sign/" + signCode;
        if (!string.IsNullOrEmpty(signCode) && File.Exists(filePath))
            ResManager.Instance.OnLoadLocalTexture(filePath, tex2D => { callback(tex2D); });
        else
            callback(Texture2D.whiteTexture);
    }


    public void ChangeData(string paramName, object value)
    {
        switch (paramName)
        {
            case "ShopName":
                ShopName = value.ToString();
                break;
        }
    }
    #endregion
}

class ShopSignEditorVo
{
    public JsonData undo;
    public string type;
    public ShopModelMono shopModel;
}

/// <summary>商铺数据集(ShopList.txt + Config.txt)</summary>
public class ShopRouteData
{
    public string RouteID;
    public string ConfigStr;
    public string ShopListStr;
    public string[] PicPathArr;

    public ShopRouteData(string routeID, string configStr, string shopListStr, string[] picPathArr)
    {
        RouteID = routeID;
        ConfigStr = configStr;
        ShopListStr = shopListStr;
        PicPathArr = picPathArr;
    }
}
#endregion


