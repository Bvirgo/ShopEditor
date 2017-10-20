using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZFrameWork;
using LitJson;
using System.IO;
using System;
using System.Text;

public class CompModule : BaseModule {

    #region Member
    private Queue<string> m_qResPaths;
    private JsonData m_jdFbx;
    private Dictionary<string, JsonData> resName_Crc;
    private List<IResourceNode> m_pMemeryRes;
    private Queue<CompConfigData> m_qUpdateComp;
    private List<CompConfigData> m_pUpdateErComp;
    private Dictionary<string,CompConfigData> code_CompConfig;

    private List<ComProperty> m_pComProperty;
    private GameObject m_objRoot;
    public CompModule()
    {
        this.AutoRegister = true;
    }

    protected override void OnLoad()
    {
        base.OnLoad();

        InitData();

        Register();

        LoadCompConfig();
    }

    private void Register()
    {
        MessageCenter.Instance.AddListener(MsgType.MainView_ReplaceAll, OnReplaceAll);
        MessageCenter.Instance.AddListener(MsgType.MainView_TagItemClick, TagItemClick);
        MessageCenter.Instance.AddListener(MsgType.MainView_ComItemClick, ComItemClick);
        MessageCenter.Instance.AddListener(MsgType.MainView_LoadRes, LoadRes);
        MessageCenter.Instance.AddListener(MsgType.MainView_NewComp, NewComp);
        MessageCenter.Instance.AddListener(MsgType.MainView_Save, SaveToServer);
        MessageCenter.Instance.AddListener(MsgType.MainView_Affirm, Affirm);
    }

    private void InitData()
    {
        m_objRoot = GameObject.Find("ComModelRoot");
        if (m_objRoot == null)
        {
            m_objRoot = new GameObject();
            m_objRoot.name = "ComModelRoot";
        }
        m_pComProperty = new List<ComProperty>();
        m_jdFbx = JsonUtils.EmptyJsonArray;
        resName_Crc = new Dictionary<string, JsonData>();

        m_qUpdateComp = new Queue<CompConfigData>();
        code_CompConfig = new Dictionary<string, CompConfigData>();
        m_pUpdateErComp = new List<CompConfigData>();
        m_pMemeryRes = new List<IResourceNode>();

        tag_comPropertys = new Dictionary<string, List<ComProperty>>();
        m_strCurTag = "";
    }
    protected override void OnRelease()
    {
        base.OnRelease();

        UnRegister();
    }

    private void UnRegister()
    {
        MessageCenter.Instance.RemoveListener(MsgType.MainView_ReplaceAll, OnReplaceAll);
        MessageCenter.Instance.RemoveListener(MsgType.MainView_TagItemClick, TagItemClick);
        MessageCenter.Instance.RemoveListener(MsgType.MainView_ComItemClick, ComItemClick);
        MessageCenter.Instance.RemoveListener(MsgType.MainView_LoadRes, LoadRes);
        MessageCenter.Instance.RemoveListener(MsgType.MainView_NewComp, NewComp);
        MessageCenter.Instance.RemoveListener(MsgType.MainView_Save, SaveToServer);
        MessageCenter.Instance.RemoveListener(MsgType.MainView_Affirm, Affirm);
    }

    #endregion

    #region 批量替换
    /// <summary>
    /// 组件批量替换
    /// </summary>
    /// <param name="_msg"></param>
    private void OnReplaceAll(Message _msg)
    {
        // 读取本地的组件模型
        LoadResources();
    }

    /// <summary>
    /// 加载本地组件模型
    /// </summary>
    private void LoadResources()
    {
        m_jdFbx = JsonUtils.EmptyJsonArray;
        resName_Crc = new Dictionary<string, JsonData>();

        m_qUpdateComp = new Queue<CompConfigData>();
        code_CompConfig = new Dictionary<string, CompConfigData>();
        m_pUpdateErComp = new List<CompConfigData>();
        m_pMemeryRes = new List<IResourceNode>();

        List<string> pResPaths = LogicUtils.Instance.OnImportFiles();
        if (pResPaths.Count > 0)
        {
            m_qResPaths = new Queue<string>(pResPaths);

            LogicUtils.Instance.OnShowWaiting(2, "Fbx加载中...", false, m_qResPaths.Count);

            LoadResToMemory();
        }
    }

    private void PopWaiting()
    {
        LogicUtils.Instance.OnPopWaiting(2);
    }

    IEnumerator test(string _strPath, Action _cb)
    {
        yield return new WaitForSeconds(1);
        _cb();
    }

    private void LoadResToMemory()
    {
        if (m_qResPaths.Count > 0)
        {
            string strPath = m_qResPaths.Dequeue() as string;

            ResManager.Instance.OnLoadLocalRes(strPath, (res) =>
            {
                JsonData jd = new JsonData();
                jd["Path"] = strPath;
                jd["Name"] = res.GetName();
                jd["Crc"] = res.GetCrc();
                m_jdFbx.Add(jd);

                m_pMemeryRes.Add(res);

                PopWaiting();

                string strFileName = res.GetName();
                strFileName = Utils.GetFilePrefix(strFileName);
                if (resName_Crc.ContainsKey(strFileName))
                {
                    Debug.LogWarning("-------------模型文件重名：" + strFileName);
                }
                resName_Crc.AddOrReplace(strFileName.ToLower(), jd);
                LoadResToMemory();
            });
        }
        else
        {
            string strJd = m_jdFbx.ToJson();
            Utils.SaveInfo(strJd, "本地新组件AB信息");
            Debug.LogWarning("加载本地文件完成");

            // 上传资源到服务器
            UpdateRes(() => {
                Debug.Log("---------资源上传完成！-------");
            });

            // 加载服务器组件配置
            LoadCompConfig(ReadCompDataForUpdate);

        }
    }

    // 上传组件模型
    private void UpdateRes(Action _cb,Action<string> _ecb = null)
    {
        // 资源上传到服务器
        ResManager.Instance.OnUpdateToServer(m_pMemeryRes, () =>
        {
            _cb();

        }, (erInfo) =>
        {
            Debug.Log(erInfo);
            if (_ecb != null)
            {
                _ecb(erInfo);
            }
        });
    }


    /// <summary>
    /// 遍历文件路径
    /// </summary>
    /// <param name="source"></param>
    void PackOneDir(string source)
    {
        DirectoryInfo folder = new DirectoryInfo(source);
        FileSystemInfo[] files = folder.GetFileSystemInfos();
        int length = files.Length;
        for (int i = 0; i < length; i++)
        {
            // 如果这个文件是目录,递归到每个文件
            if (files[i] is DirectoryInfo)
            {
                PackOneDir(files[i].FullName);
            }
            else
            {
                if (files[i].Name.EndsWith(".assetbundle")
                    || files[i].Name.EndsWith(".FBX")
                    || files[i].Name.EndsWith(".fbx"))
                {
                    m_qResPaths.Enqueue(files[i].FullName);
                }
            }
        }
    }

    /// <summary>
    /// 解析配置
    /// </summary>
    /// <param name="_strJson"></param>
    private void ReadCompDataForUpdate(string _strJson)
    {
        JsonData ConfigJD = JsonMapper.ToObject(_strJson);

        JsonData jdNewCompConfig = JsonUtils.EmptyJsonArray;

        int nCount = 0;
        StringBuilder sb = new StringBuilder();

        // 所有组件分类
        for (int iClass = 0, lenClass = ConfigJD.Count; iClass < lenClass; iClass++)
        {
            JsonData classJD = ConfigJD[iClass];
            //string strKey = classJD.ReadString("name");
            string strInfo = classJD.ReadString("info");

            JsonData info = JsonMapper.ToObject(strInfo);
            //string strTag = info.ReadString("tag");
            //string strShowName = info.ReadString("name");
            //string strPicName = info.ReadString("picName");
            //string strPicCrc = info.ReadString("picCrc");
            string strCode = info.ReadString("code");

            string strData = info.ReadString("data");
            if (string.IsNullOrEmpty(strData))
            {
                Debug.LogError("Json Error:" + classJD.ToJson());
            }
            JsonData jdData = JsonMapper.ToObject(strData);
            JsonData dataJD = jdData;
            string strLowerCode = strCode.ToLower();

            if (resName_Crc.ContainsKey(strLowerCode))
            {
                JsonData jdNew = resName_Crc[strLowerCode];

                JsonData jdNewData = JsonUtils.EmptyJsonArray;

                CompConfigData ccd = new CompConfigData();
                ccd.m_strCode = strCode;

                if (dataJD.Count > 0)
                {
                    JsonData jdFbx = dataJD[0];
                    jdFbx["fileName"] = jdNew.ReadString("Name");
                    jdFbx["fileCrc"] = jdNew.ReadString("Crc");
                    jdFbx["type"] = "assetbundle";
                    jdFbx["children"] = JsonUtils.EmptyJsonArray;
                    jdNewData.Add(jdFbx);

                    info["data"] = jdNewData.ToJson();
                    classJD["info"] = info.ToJson();
                    jdNewCompConfig.Add(classJD);
                    nCount++;
                    resName_Crc.Remove(strLowerCode);
                    ccd.m_jsData = info;

                    code_CompConfig.Add(strCode, ccd);

                    m_qUpdateComp.Enqueue(ccd);
                }
            }
        }
        sb.Append("-------------------------新模型匹配失败---------------------------------\n\n");
        foreach (var item in resName_Crc)
        {
            sb.Append(item.Key);
            sb.Append("\n");
        }
        Utils.SaveInfo(jdNewCompConfig.ToJson(), "新组件AB配置");
        Debug.LogWarning(string.Format("替换成功个数：{0}", nCount));
        Utils.SaveInfo(sb.ToString(), "替换失败列表");

        // 测试
        Debug.Log("读取CompLibConfig完成!");

        m_pUpdateErComp.Clear();

        UpdateCompConfig(()=> {
            // 刷新组件列表
            RefreshCom();
        });
    }


    #endregion

    #region Common
    private Dictionary<string, List<ComProperty>> tag_comPropertys;
    private void LoadCompConfig()
    {
        // 加载组件配置
        RefreshCom();
    }

    /// <summary>
    /// 刷新组件列表
    /// </summary>
    private void RefreshCom()
    {
        tag_comPropertys.Clear();
        m_pComProperty.Clear();
        Utils.RemoveChildren(m_objRoot.transform);

        // 加载组件配置
        LoadCompConfig((js) => {
            ReadCompDataForUI(js);
            for (int i = 0; i < m_pComProperty.Count; i++)
            {
                List<ComProperty> pComProperty;
                ComProperty cpp = m_pComProperty[i];
                string strTag = cpp.m_strTag;
                pComProperty = tag_comPropertys.ContainsKey(strTag) ? tag_comPropertys[strTag] : new List<ComProperty>();
                pComProperty.Add(cpp);
                tag_comPropertys.AddOrReplace(strTag, pComProperty);
            }
            List<string> pTags = new List<string>(tag_comPropertys.Keys);

            m_strCurTag = "";
            m_currentCom = null;

            // 通知UI，刷新组件列表
            Message msg = new Message(MsgType.MainView_RefreshTag, this);
            msg["tags"] = pTags;
            msg.Send();
        });
    }

    /// <summary>
    /// 请求组件配置
    /// </summary>
    private void LoadCompConfig(Action<string> _cb)
    {
        HttpService.GetCompConfigUrl((strRes) =>
        {
            JsonData jd = JsonMapper.ToObject(strRes);

            //string strNetCrc = jd.ReadString("crc");
            string strUrl = jd.ReadString("url");

            //Debug.LogWarning("Crc:" + strNetCrc + "--Url:" + strUrl);
            LoadCompConfigFromNetByTxt(strUrl, _cb);

        }, (strRes) =>
        {
            Debug.LogError("加载组件配置Url失败");
        });
    }
    /// <summary>
    /// 下载配置
    /// </summary>
    /// <param name="url"></param>
    public void LoadCompConfigFromNetByTxt(string url, Action<string> _cb)
    {
        HttpService.GetRemoteRaw(url, (data) =>
        {
            string strCmpData = Encoding.UTF8.GetString(data);

            Utils.SimpleSaveInfo(strCmpData, "CompConfigFromNet");

            _cb(strCmpData);

            //string strTest = strCmpData.Substring(0, 1000);
            // 测试
            //LogicMgr.Instance.OnAlert(strTest, "测试", () => { Debug.LogWarning("ok"); }, () => { Debug.LogWarning("cancel"); });

            //LogicMgr.Instance.OnAlert("我又来啦~~~~~~~~~~~~~~~");

        }, failResp =>
        {
            Debug.LogError("加载组件配置失败");
        });
    }
    /// <summary>
    /// 上传组件配置
    /// </summary>
    private void UpdateCompConfig(Action _cb = null)
    {
        if (m_qUpdateComp.Count > 0)
        {
            CompConfigData ccd = m_qUpdateComp.Dequeue() as CompConfigData;

            HttpService.SaveCompConfig(ccd.m_strCode, ccd.m_jsData.ToJson(), (bS, resp) => {
                if (bS)
                {
                    Debug.LogWarning(string.Format("组件:{0},配置更新成功:{1}", ccd.m_strCode, resp.WwwText));
                }
                else
                {
                    Debug.LogError(string.Format("组件:{0},配置更新失败:{1}", ccd.m_strCode, resp.Error));
                    JsonData jd = new JsonData();
                    jd["Code"] = ccd.m_strCode;

                    // 上传失败的，先保存下来
                    if (code_CompConfig.ContainsKey(ccd.m_strCode) && !m_pUpdateErComp.Contains(ccd))
                    {
                        m_pUpdateErComp.Add(ccd);
                    }
                }

                UpdateCompConfig(_cb);
                Debug.Log(string.Format("剩余组件个数：{0}", m_qUpdateComp.Count));
            });
        }
        else
        {
            // 弹框提示失败组件，点击重新上传
            if (m_pUpdateErComp.Count > 0)
            {
                string strTips = string.Format("组件上传失败个数：{0} /n点击【确定】按钮，失败重传！", m_pUpdateErComp.Count);
                LogicUtils.Instance.OnAlert(strTips, () => {
                    m_qUpdateComp = new Queue<CompConfigData>(m_pUpdateErComp);
                    UpdateCompConfig(_cb);
                });
            }
            else
            {
                if (_cb != null)
                {
                    _cb();
                }
                LogicUtils.Instance.OnAlert("组件配置上传完成！");
            }
        }
    }

    #endregion

    #region 点操
    private string m_strCurTag;
    private ComProperty m_currentCom;
    private LocalResourceNode m_rNewImg;
    private LocalResourceNode m_rNewModel;
    private List<GameObject> m_pCompModel;
    /// <summary>
    /// 解析组件配置，提取组件属性
    /// </summary>
    /// <param name="_strJson"></param>
    private void ReadCompDataForUI(string _strJson)
    {
        m_pComProperty = new List<ComProperty>();

        JsonData ConfigJD = JsonMapper.ToObject(_strJson);
        // 所有组件分类
        for (int i = 0; i < ConfigJD.Count; ++i)
        {
            JsonData classJD = ConfigJD[i];
            ComProperty cpp = new ComProperty(classJD);
            m_pComProperty.Add(cpp);
        }
    }
    /// <summary>
    /// 选择组件分组
    /// </summary>
    /// <param name="_msg"></param>
    private void TagItemClick(Message _msg)
    {
        string strTag = _msg["tag"] as string;
        if (m_strCurTag.Equals(strTag))
        {
            return;
        }
        List<ComProperty> pComProperty = tag_comPropertys[strTag];
        Message msg = new Message(MsgType.MainView_RefreshCom,this);
        msg["coms"] = pComProperty;
        msg.Send();
        m_strCurTag = strTag;
        
    }

    /// <summary>
    /// 选择组件
    /// </summary>
    /// <param name="_msg"></param>
    private void ComItemClick(Message _msg)
    {
        ComProperty cpp = _msg["com"] as ComProperty;
        m_currentCom = cpp;

        m_rNewImg = null;
        m_rNewModel = null;

        // 载入组件
        LoadCompModelFromServer(cpp.m_strModelName,cpp.m_strModelCrc);
    }

    /// <summary>
    /// 加载服务器资源模型
    /// </summary>
    /// <param name="_strName"></param>
    /// <param name="_strCrc"></param>
    private void LoadCompModelFromServer(string _strName,string _strCrc)
    {
        ResManager.Instance.OnLoadServerRes(_strName, _strCrc, (rNode) => {
            // 上个组件资源回调
            if (!rNode.GetName().Equals(m_currentCom.m_strModelName) 
            || m_rNewModel != null)
            {
                return;
            }

            switch (rNode.GetResType())
            {
                case ResType.Fbx:

                    ResManager.Instance.OnCreateFbx(rNode.GetResource() as byte[], (obj) =>
                    {
                        CreateNewModel(obj, Utils.GetFilePrefix(rNode.GetName()));
                    });
                    break;
                case ResType.AssetBundle:
  
                    AssetBundle ab = rNode.GetResource() as AssetBundle;
                    if (ab != null)
                    {
                        GameObject model = ab.LoadAsset<GameObject>(ab.GetAllAssetNames()[0]);
                        if (model != null)
                        {
                            model = GameObject.Instantiate(model);
                            CreateNewModel(model, Utils.GetFilePrefix(rNode.GetName()));
                        }
                        ab.Unload(false);

                    }
                    break;
                case ResType.Raw:
                    break;
                default:
                    break;
            }
        });
    }

    /// <summary>
    /// 导入资源
    /// </summary>
    /// <param name="_msg"></param>
    private void LoadRes(Message _msg)
    {
        if (m_currentCom == null)
        {
            LogicUtils.Instance.OnAlert("没有选中任何组件，不能导入资源！");
            return;
        }

        CompView mv = _msg.Sender as CompView;
        if (mv == null)
        {
            return;
        }
        string strPath = LogicUtils.Instance.OnImportOneFile();
        string strFileName = Utils.GetFileNameByPath(strPath).ToLower();
        //Debug.LogWarning("当前文件路径："+strPath+"--文件名:"+strFileName);
        string strFilePostName = Utils.GetFilePostfix(strPath).ToLower();

        // 检测模型文件名
        if (strFilePostName.Equals("fbx") ||
            strFilePostName.Equals("assetbundle"))
        {
            if (!m_currentCom.m_bIsNew && !m_currentCom.m_strCode.ToLower().Equals(strFileName))
            {
                LogicUtils.Instance.OnAlert("资源名和组件Code不一致，不能导入资源!");
                return;
            }

            // 新组件，Code唯一性检测
            if (m_currentCom.m_bIsNew && !IsLegalCode(strFileName))
            {
                LogicUtils.Instance.OnAlert("新组件Code和已有组件重复！");
                return;
            }
        }

        ResManager.Instance.OnLoadLocalRes(strPath, (rNode) => 
        {
            switch (rNode.GetResType())
            {
                case ResType.Texture:
                    Texture2D tx = new Texture2D(500, 500);
                    tx.LoadImage(rNode.GetResource() as byte[]);
                    m_rNewImg = rNode as LocalResourceNode;
                    if (mv != null)
                    {
                        mv.OnRefreshImg(tx,rNode.GetName());
                    }
                    break;
                case ResType.Fbx:
                    m_rNewModel = rNode as LocalResourceNode;
                    ResManager.Instance.OnCreateFbx(m_rNewModel.GetResource() as byte[], (obj) => 
                    {
                        CreateNewModel(obj, Utils.GetFilePrefix(m_rNewModel.GetName()));
                        if (mv != null)
                        {
                            mv.OnRefreshModel(rNode.GetName(),m_currentCom.m_bIsNew);
                        }

                    });
                    break;
                case ResType.AssetBundle:
                    m_rNewModel = rNode as LocalResourceNode;
                    ResManager.Instance.OnLoadLocalAb(m_rNewModel.GetResource() as byte[], (ab) =>
                    {
                        if (ab != null)
                        {
                            GameObject model = ab.LoadAsset<GameObject>(ab.GetAllAssetNames()[0]);
                            if (model != null)
                            {
                                model = GameObject.Instantiate(model);
                                CreateNewModel(model, Utils.GetFilePrefix(m_rNewModel.GetName()));
                                if (mv != null)
                                {
                                    mv.OnRefreshModel(rNode.GetName());
                                }
                            }
                            ab.Unload(false);

                        }
                    });
                    break;
                case ResType.Raw:
                    break;
                default:
                    break;
            }
            
        });
    }

    /// <summary>
    /// 场景中创建模型
    /// </summary>
    /// <param name="_go"></param>
    /// <param name="_strName"></param>
    private void CreateNewModel(GameObject _go,string _strName)
    {
        Utils.RemoveChildren(m_objRoot.transform);

        GameObject compObj = new GameObject();
        compObj.name = _strName;
        LogicUtils.ResetStandardShader(_go);

        _go.transform.SetParent(compObj.transform);
        _go.transform.position = Vector3.zero;
        _go.transform.localScale = Vector3.one;

        compObj.transform.position = Vector3.zero;
        compObj.transform.localScale = Vector3.one;
        compObj.transform.SetParent(m_objRoot.transform);
    }

    /// <summary>
    /// 确认修改
    /// </summary>
    /// <param name="_msg"></param>
    private void Affirm(Message _msg)
    {
        string strNewTag = _msg["tag"] as string;
        string strNewShowName = _msg["name"] as string;
        m_currentCom.OnAffirm(m_rNewModel, m_rNewImg, strNewTag, strNewShowName);
    }

    /// <summary>
    /// 修改，新增到服务器
    /// </summary>
    /// <param name="_msg"></param>
    private void SaveToServer(Message _msg)
    {
        string strNewTag = _msg["tag"] as string;
        string strNewShowName = _msg["name"] as string;

        m_currentCom.OnAffirm(m_rNewModel, m_rNewImg, strNewTag, strNewShowName);

        // 遍历所有的需要更新的组件
        m_pMemeryRes.Clear();
        m_qUpdateComp = new Queue<CompConfigData>();
        code_CompConfig = new Dictionary<string, CompConfigData>();
        m_pUpdateErComp = new List<CompConfigData>();

        for (int i = 0; i < m_pComProperty.Count; i++)
        {
            ComProperty cpp = m_pComProperty[i];
            if (cpp.m_bNeedUpdate)
            {
                CompConfigData ccd = new CompConfigData();
                ccd.m_jsData = cpp.m_infoJson;
                ccd.m_strCode = cpp.m_strCode;
                m_qUpdateComp.Enqueue(ccd);
                code_CompConfig.AddOrReplace(ccd.m_strCode,ccd);

                if (cpp.m_rNewImg != null)
                {
                    m_pMemeryRes.Add(cpp.m_rNewImg);
                }
                if (cpp.m_rNewModel != null)
                {
                    m_pMemeryRes.Add(cpp.m_rNewModel);
                }
            }
        }

        // 先上传资源，再上传配置
        UpdateRes(() => {
            UpdateCompConfig(()=> {
                // 刷新列表
                RefreshCom();
            });
                
        });
    }

    /// <summary>
    /// 是否需要更新
    /// </summary>
    /// <param name="_strTag"></param>
    /// <param name="_strShowName"></param>
    /// <returns></returns>
    private bool IsNeedUpdate(string _strTag,string _strShowName)
    {
        bool bUpdate = false;

        if (m_currentCom == null )
        {
            return bUpdate;
        }

        if (m_currentCom.m_bIsNew && (
            m_rNewImg == null ||
            m_rNewModel == null))
        {
            LogicUtils.Instance.OnAlert("新组件，必须有模型和缩略图！");
            return bUpdate;
        }

        if (m_rNewImg != null)
        {
            bUpdate = true;
        }

        if (m_rNewModel != null)
        {
            bUpdate = true;
        }
        if (!_strShowName.Equals(m_currentCom.m_strShowName))
        {
            bUpdate = true;
        }
        if (!_strTag.Equals(m_currentCom.m_strTag))
        {
            bUpdate = true;
        }

        return bUpdate;
    }

    /// <summary>
    /// 新组件，组件名唯一性检测
    /// </summary>
    /// <param name="_strCode"></param>
    /// <returns></returns>
    private bool IsLegalCode(string _strCode)
    {
        bool bLegal = true;
        for (int i = 0; i < m_pComProperty.Count; i++)
        {
            string strCode = m_pComProperty[i].m_strCode;
            if (!string.IsNullOrEmpty(strCode) && strCode.ToLower().Equals(_strCode))
            {
                bLegal = false;
                break;
            }
        }

        return bLegal;
    }
    #endregion

    #region 新增组件

    /// <summary>
    /// 新组件
    /// </summary>
    /// <param name="_msg"></param>
    private void NewComp(Message _msg)
    {
        m_currentCom = null;
        m_rNewImg = null;
        m_rNewModel = null;

        ComProperty newCom = new ComProperty();
        m_currentCom = newCom;
        m_pComProperty.Add(newCom);
    }
    #endregion
}

public class CompConfigData
{
    public CompConfigData() { }
    public string m_strCode;
    public JsonData m_jsData;
}

/// <summary>
/// 组件基本属性
/// </summary>
public class ComProperty
{
    public string m_strImg;
    public string m_strImgCrc;
    public string m_strModelName;
    public string m_strModelCrc;
    public string m_strCode;
    public string m_strShowName;
    public string m_strTag;
    public string m_strModelType;
    public JsonData m_infoJson;
    public bool m_bNeedUpdate;

    public IResourceNode m_rNewImg;
    public IResourceNode m_rNewModel;

    public bool m_bIsNew;
    public ComProperty(JsonData _jd)
    {
        string strInfo = _jd.ReadString("info");
        JsonData info = JsonMapper.ToObject(strInfo);
        m_infoJson = info;
        m_strTag = info.ReadString("tag","TestTag");
        m_strShowName = info.ReadString("name");
        m_strImg = info.ReadString("picName");
        m_strImgCrc = info.ReadString("picCrc");
        m_strCode = info.ReadString("code","BJL341");

        string strData = info.ReadString("data");
        JsonData dataJD = JsonMapper.ToObject(strData);
        if (dataJD.Count > 0)
        {
            JsonData jdModel = dataJD[0];
            m_strModelName = jdModel.ReadString("fileName");
            m_strModelCrc = jdModel.ReadString("fileCrc");
            m_strModelType = jdModel.ReadString("type");
        }

        m_bNeedUpdate = false;
        m_bIsNew = false;
    }

    /// <summary>
    ///  新组件
    /// </summary>
    public ComProperty()
    {
        m_bNeedUpdate = false;
        m_bIsNew = true;
        m_infoJson = JsonUtils.EmptyJsonObject;
        m_strTag = "NewTest";
        m_strShowName = "新组件";
        m_infoJson["tag"] = m_strTag;
        m_infoJson["name"] = m_strShowName;
    }

    /// <summary>
    /// 确认修改
    /// </summary>
    /// <param name="_rModelNode"></param>
    /// <param name="_rImgNode"></param>
    /// <param name="_strTag"></param>
    /// <param name="_strShowName"></param>
    public void OnAffirm(IResourceNode _rModelNode, IResourceNode _rImgNode,
    string _strTag,
    string _strShowName)
    {
        m_bNeedUpdate = true;

        string strData = m_infoJson.ReadString("data", JsonUtils.EmptyJsonArray.ToJson());
        JsonData jdData = JsonMapper.ToObject(strData);
        JsonData dataJD = jdData;
        JsonData jdNewData = JsonUtils.EmptyJsonArray;

        if (dataJD.Count < 1)
        {
            JsonData jd = JsonUtils.EmptyJsonObject;
            dataJD.Add(jd);
        }

        JsonData jdFbx = dataJD[0];
        if (_rImgNode != null)
        {
            m_infoJson["picName"] = _rImgNode.GetName();
            m_infoJson["picCrc"] = _rImgNode.GetCrc();

            m_strImg = _rImgNode.GetName();
            m_strImgCrc = _rImgNode.GetCrc();

            m_rNewImg = _rImgNode;
        }
        if (_rModelNode != null)
        {
            jdFbx["fileName"] = _rModelNode.GetName();
            jdFbx["fileCrc"] = _rModelNode.GetCrc();
            jdFbx["type"] = _rModelNode.GetResType().ToString();

            m_strModelCrc = _rModelNode.GetCrc();
            m_strModelName = _rModelNode.GetName();
            m_strModelType = _rModelNode.GetResType().ToString();

            m_rNewModel = _rModelNode;

            if (m_bIsNew)
            {
                m_strCode = Utils.GetFilePrefix(m_strModelName);
                m_infoJson["code"] = m_strCode;
                m_infoJson["wallLength"] = "0";
            }
        }
        if (!_strShowName.Equals(m_strShowName))
        {
            m_infoJson["name"] = _strShowName;
            m_strShowName = _strShowName;
        }
        if (!_strTag.Equals(m_strTag))
        {
            m_infoJson["tag"] = _strTag;
            m_strTag = _strTag;
        }

        jdFbx["children"] = JsonUtils.EmptyJsonArray;
        jdNewData.Add(jdFbx);
        m_infoJson["data"] = jdNewData.ToJson();
        Debug.LogWarning("NewComp:" + m_infoJson.ToJson());
    }
}
